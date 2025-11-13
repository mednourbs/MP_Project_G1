using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Moteur physique principal qui gère la simulation de tous les corps rigides et contraintes.
/// Coordonne l'intégration des forces, collisions et ruptures de contraintes.
/// </summary>
public class PhysicsEngine : MonoBehaviour
{
    [SerializeField] private float timeStep = 0.016f;  // ~60 FPS
    [SerializeField] private int substeps = 1;
    [SerializeField] private float collisionSphereRadius = 0.5f;
    [SerializeField] private float collisionRestitution = 0.95f;  // Augmenté pour plus d'interactions
    [SerializeField] private float groundLevel = -3f;  // Position du sol (Y)
    [SerializeField] private float groundFriction = 0.95f;  // Friction au sol

    private List<RigidBody> bodies = new List<RigidBody>();
    private List<Constraint> constraints = new List<Constraint>();
    private CollisionDetector.CollisionInfo[] lastFrameCollisions;
    
    private int nextBodyID = 0;
    private int nextConstraintID = 0;

    public static PhysicsEngine Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>Crée un nouveau corps rigide dans la simulation</summary>
    public RigidBody CreateRigidBody(float mass = 1f)
    {
        RigidBody body = new RigidBody(nextBodyID++, mass);
        bodies.Add(body);
        return body;
    }

    /// <summary>Crée une contrainte entre deux corps</summary>
    public Constraint CreateConstraint(RigidBody bodyA, RigidBody bodyB, 
                                      Vector3 anchorA, Vector3 anchorB,
                                      float stiffness = 100f, float breakingThreshold = 0.5f)
    {
        Constraint constraint = new Constraint(nextConstraintID++, bodyA, bodyB, anchorA, anchorB, stiffness, breakingThreshold);
        constraints.Add(constraint);
        return constraint;
    }

    /// <summary>Ajoute un corps existant à la simulation</summary>
    public void AddRigidBody(RigidBody body)
    {
        if (!bodies.Contains(body))
            bodies.Add(body);
    }

    /// <summary>Retire un corps de la simulation</summary>
    public void RemoveRigidBody(RigidBody body)
    {
        bodies.Remove(body);
    }

    private void FixedUpdate()
    {
        float dt = timeStep / substeps;

        for (int step = 0; step < substeps; step++)
        {
            // 1. Intègre les forces
            foreach (var body in bodies)
                body.IntegrateForces(dt);

            // 2. Applique les forces de contrainte
            foreach (var constraint in constraints)
                constraint.ApplyConstraintForce();

            // 3. Détecte et résout les collisions
            DetectAndResolveCollisions();

            // 4. Vérifie les ruptures de contraintes
            CheckConstraintBreaking();

            // 5. Détecte collision avec le sol
            DetectAndResolveGroundCollisions();

            // 6. Intègre la vélocité et met à jour la position/rotation
            foreach (var body in bodies)
                body.IntegrateVelocity(dt);
        }
    }

    /// <summary>Détecte et résout les collisions entre corps</summary>
    private void DetectAndResolveCollisions()
    {
        var collisions = CollisionDetector.DetectCollisions(bodies, collisionSphereRadius);
        lastFrameCollisions = collisions.ToArray();

        foreach (var collision in collisions)
        {
            CollisionDetector.ResolveCollision(collision, collisionRestitution);
            CollisionDetector.SeparateOverlapping(collision);
        }
    }

    /// <summary>Détecte et résout les collisions avec le sol</summary>
    private void DetectAndResolveGroundCollisions()
    {
        foreach (var body in bodies)
        {
            if (body.IsKinematic) continue;

            // Vérifie si le corps est sous le niveau du sol (avec rayon de sphère)
            float lowestPoint = body.Position.y - collisionSphereRadius;

            if (lowestPoint < groundLevel)
            {
                // Collision détectée avec le sol
                // Sépare l'objet du sol
                body.Position = new Vector3(body.Position.x, groundLevel + collisionSphereRadius, body.Position.z);

                // Résout la collision élastique
                if (body.LinearVelocity.y < 0)
                {
                    // Inverse la vélocité Y et applique la restitution
                    body.LinearVelocity = new Vector3(
                        body.LinearVelocity.x * groundFriction,  // Friction horizontale au sol
                        -body.LinearVelocity.y * collisionRestitution,  // Rebond
                        body.LinearVelocity.z * groundFriction   // Friction horizontale au sol
                    );
                }
            }
        }
    }

    /// <summary>Vérifie les ruptures de contraintes et applique les impulsions</summary>
    private void CheckConstraintBreaking()
    {
        List<Constraint> constraintsToRemove = new List<Constraint>();

        foreach (var constraint in constraints)
        {
            float energy = constraint.UpdateAndCheckBreaking();

            if (constraint.IsBroken)
            {
                // Applique l'impulsion basée sur l'énergie stockée
                Vector3 direction = (constraint.GetWorldAnchorB() - constraint.GetWorldAnchorA()).normalized;
                
                Constraint.ApplyImpulseFromEnergy(constraint.BodyA, -direction, energy);
                Constraint.ApplyImpulseFromEnergy(constraint.BodyB, direction, energy);

                constraintsToRemove.Add(constraint);

                // Debug log
                Debug.Log($"Constraint {constraint.ID} broken with energy {energy:F2}J");
            }
        }

        // Retire les contraintes cassées
        foreach (var constraint in constraintsToRemove)
            constraints.Remove(constraint);
    }

    /// <summary>Retourne tous les corps rigides</summary>
    public List<RigidBody> GetAllBodies() => new List<RigidBody>(bodies);

    /// <summary>Retourne toutes les contraintes</summary>
    public List<Constraint> GetAllConstraints() => new List<Constraint>(constraints);

    /// <summary>Retourne les collisions du dernier frame</summary>
    public CollisionDetector.CollisionInfo[] GetLastFrameCollisions() => lastFrameCollisions ?? System.Array.Empty<CollisionDetector.CollisionInfo>();

    /// <summary>Retourne le niveau du sol</summary>
    public float GetGroundLevel() => groundLevel;

    /// <summary>Pause la simulation</summary>
    public void Pause()
    {
        Time.timeScale = 0f;
    }

    /// <summary>Reprend la simulation</summary>
    public void Resume()
    {
        Time.timeScale = 1f;
    }
}
