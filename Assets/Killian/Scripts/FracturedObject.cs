using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Représente un objet fracturé dans la scène avec ses shards et contraintes.
/// Synchronise la physique personnalisée avec les GameObjects Unity pour le rendu.
/// </summary>
public class FracturedObject : MonoBehaviour
{
    [SerializeField] private Vector3 objectSize = new Vector3(2f, 2f, 2f);
    [SerializeField] private int fractureSubdivisions = 2;
    [SerializeField] private float shardMass = 1f;
    [SerializeField] private float constraintStiffness = 100f;
    [SerializeField] private float constraintBreakingThreshold = 0.5f;
    [SerializeField] private Material shardMaterial;
    [SerializeField] private bool showDebugInfo = true;
    
    // Paramètres de fracturation réaliste
    [SerializeField] private float fractureExplosionForce = 15f;  // Force d'explosion lors fracturation
    [SerializeField] private float fractureAngularVelocity = 20f;  // Rotation initiale
    [SerializeField] private bool applyExplosionEffect = true;  // Activer l'effet explosion

    private List<RigidBody> shardBodies = new List<RigidBody>();
    private List<GameObject> shardGameObjects = new List<GameObject>();
    private List<Constraint> internalConstraints = new List<Constraint>();
    private PhysicsEngine physicsEngine;

    private void Start()
    {
        physicsEngine = PhysicsEngine.Instance;
        InitializeFracturedObject();
    }

    /// <summary>Initialise l'objet fracturé</summary>
    private void InitializeFracturedObject()
    {
        // Génère les shards
        var shards = GeometryManager.GenerateFracturedCube(objectSize, fractureSubdivisions);

        // Crée un corps rigide et GameObject pour chaque shard
        foreach (var shard in shards)
        {
            // Crée le corps rigide
            RigidBody body = physicsEngine.CreateRigidBody(shardMass);
            body.Position = shard.CenterOfMass;
            body.SetCubeInertia(new Vector3(
                objectSize.x / fractureSubdivisions,
                objectSize.y / fractureSubdivisions,
                objectSize.z / fractureSubdivisions
            ));
            shardBodies.Add(body);

            // Crée le GameObject pour le rendu
            GameObject shardGO = new GameObject($"Shard_{shard.ID}");
            shardGO.transform.parent = transform;
            shardGO.transform.position = body.Position;

            MeshFilter meshFilter = shardGO.AddComponent<MeshFilter>();
            meshFilter.mesh = GeometryManager.CreateMeshFromShard(shard);

            MeshRenderer meshRenderer = shardGO.AddComponent<MeshRenderer>();
            meshRenderer.material = shardMaterial ?? new Material(Shader.Find("Standard"));

            MeshCollider meshCollider = shardGO.AddComponent<MeshCollider>();
            meshCollider.enabled = false;  // On n'utilise pas les colliders Unity

            ShardController controller = shardGO.AddComponent<ShardController>();
            controller.Initialize(body);

            shardGameObjects.Add(shardGO);
        }

        // Crée les contraintes entre shards adjacents
        CreateConstraints();
        
        // Applique l'effet explosion de fracturation
        if (applyExplosionEffect)
            ApplyFractureExplosion();
    }

    /// <summary>Applique une explosion lors de la fracturation pour un effet réaliste</summary>
    private void ApplyFractureExplosion()
    {
        // Centre de l'objet
        Vector3 objectCenter = transform.position;

        foreach (var body in shardBodies)
        {
            // Direction d'explosion = direction du centre vers le shard
            Vector3 directionFromCenter = (body.Position - objectCenter).normalized;
            if (directionFromCenter.sqrMagnitude < 0.01f)
            {
                // Si au centre, direction aléatoire
                directionFromCenter = Random.onUnitSphere;
            }

            // Applique une impulsion radiale
            Vector3 explosionImpulse = directionFromCenter * fractureExplosionForce;
            body.AddImpulse(explosionImpulse);

            // Applique une rotation angulaire aléatoire
            Vector3 randomAxis = Random.onUnitSphere;
            Vector3 angularImpulse = randomAxis * fractureAngularVelocity;
            body.AddAngularImpulse(angularImpulse);

            // Debug
            Debug.DrawLine(body.Position, body.Position + directionFromCenter * 2f, Color.red, 1f);
        }
    }

    /// <summary>Crée les contraintes entre shards adjacents</summary>
    private void CreateConstraints()
    {
        int sPerAxis = fractureSubdivisions;

        for (int x = 0; x < sPerAxis; x++)
        {
            for (int y = 0; y < sPerAxis; y++)
            {
                for (int z = 0; z < sPerAxis; z++)
                {
                    int idx = x + y * sPerAxis + z * sPerAxis * sPerAxis;

                    // Contrainte avec le voisin à droite (+X)
                    if (x + 1 < sPerAxis)
                    {
                        int idxNext = (x + 1) + y * sPerAxis + z * sPerAxis * sPerAxis;
                        CreateConstraintBetween(idx, idxNext);
                    }

                    // Contrainte avec le voisin en haut (+Y)
                    if (y + 1 < sPerAxis)
                    {
                        int idxNext = x + (y + 1) * sPerAxis + z * sPerAxis * sPerAxis;
                        CreateConstraintBetween(idx, idxNext);
                    }

                    // Contrainte avec le voisin arrière (+Z)
                    if (z + 1 < sPerAxis)
                    {
                        int idxNext = x + y * sPerAxis + (z + 1) * sPerAxis * sPerAxis;
                        CreateConstraintBetween(idx, idxNext);
                    }
                }
            }
        }
    }

    /// <summary>Crée une contrainte entre deux shards</summary>
    private void CreateConstraintBetween(int shardAIdx, int shardBIdx)
    {
        RigidBody bodyA = shardBodies[shardAIdx];
        RigidBody bodyB = shardBodies[shardBIdx];

        // Les ancres sont aux centres des shards
        Vector3 anchorA = Vector3.zero;  // Centre local
        Vector3 anchorB = Vector3.zero;

        Constraint constraint = physicsEngine.CreateConstraint(
            bodyA, bodyB, anchorA, anchorB,
            constraintStiffness, constraintBreakingThreshold
        );

        internalConstraints.Add(constraint);
    }

    private void OnDestroy()
    {
        // Retire tous les corps de la simulation
        foreach (var body in shardBodies)
            physicsEngine.RemoveRigidBody(body);
    }

    /// <summary>Affiche les informations de debug</summary>
    private void OnGUI()
    {
        if (!showDebugInfo) return;

        int constraintsCount = internalConstraints.FindAll(c => !c.IsBroken).Count;
        int totalConstraints = internalConstraints.Count;

        GUI.Label(new Rect(10, 10, 300, 20), $"Shards: {shardBodies.Count}");
        GUI.Label(new Rect(10, 35, 300, 20), $"Constraints: {constraintsCount}/{totalConstraints}");
        GUI.Label(new Rect(10, 60, 300, 20), $"Bodies: {physicsEngine.GetAllBodies().Count}");

        if (GUI.Button(new Rect(10, 90, 100, 30), "Pause"))
            physicsEngine.Pause();

        if (GUI.Button(new Rect(120, 90, 100, 30), "Resume"))
            physicsEngine.Resume();
    }
}
