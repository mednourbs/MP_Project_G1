using UnityEngine;

/// <summary>
/// Représente un corps rigide dans la simulation physique avec propriétés dynamiques.
/// Gère la masse, l'inertie, la vélocité, et la rotation.
/// </summary>
public class RigidBody
{
    public int ID { get; private set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 LinearVelocity { get; set; }
    public Vector3 AngularVelocity { get; set; }
    public Vector3 LinearAcceleration { get; set; }
    public Vector3 AngularAcceleration { get; set; }
    public float Mass { get; set; }
    public Matrix3x3 InertiaTensor { get; set; }
    public bool IsKinematic { get; set; }
    public bool UseGravity { get; set; }

    // Paramètres d'amortissement
    private float linearDamping = 0.95f;  // Moins d'amortissement = plus de mouvement
    private float angularDamping = 0.92f;  // Moins d'amortissement = plus de rotation

    private Vector3 accumulatedForce = Vector3.zero;
    private Vector3 accumulatedTorque = Vector3.zero;

    public RigidBody(int id, float mass = 1f)
    {
        ID = id;
        Mass = mass;
        Position = Vector3.zero;
        Rotation = Quaternion.identity;
        LinearVelocity = Vector3.zero;
        AngularVelocity = Vector3.zero;
        LinearAcceleration = Vector3.zero;
        AngularAcceleration = Vector3.zero;
        InertiaTensor = Matrix3x3.Identity;
        IsKinematic = false;
        UseGravity = true;
    }

    /// <summary>Ajoute une force au centre de masse</summary>
    public void AddForce(Vector3 force)
    {
        if (IsKinematic) return;
        accumulatedForce += force;
    }

    /// <summary>Ajoute une force à un point spécifique (génère un couple)</summary>
    public void AddForceAtPoint(Vector3 force, Vector3 worldPoint)
    {
        if (IsKinematic) return;
        AddForce(force);
        Vector3 relativePoint = worldPoint - Position;
        AddTorque(Vector3.Cross(relativePoint, force));
    }

    /// <summary>Ajoute un couple (torque)</summary>
    public void AddTorque(Vector3 torque)
    {
        if (IsKinematic) return;
        accumulatedTorque += torque;
    }

    /// <summary>Ajoute une impulsion linéaire</summary>
    public void AddImpulse(Vector3 impulse)
    {
        if (IsKinematic) return;
        LinearVelocity += impulse / Mass;
    }

    /// <summary>Ajoute une impulsion angulaire</summary>
    public void AddAngularImpulse(Vector3 impulse)
    {
        if (IsKinematic) return;
        Vector3 angularChange = InertiaTensor.Inverse() * impulse;
        AngularVelocity += angularChange;
    }

    /// <summary>Intègre les forces et mises à jour les accélérations</summary>
    public void IntegrateForces(float deltaTime)
    {
        if (IsKinematic) return;

        // Accélération linéaire
        LinearAcceleration = accumulatedForce / Mass;
        
        // Ajoute la gravité
        if (UseGravity)
            LinearAcceleration += Physics.gravity;

        // Accélération angulaire
        Vector3 inverseInertiaTorque = InertiaTensor.Inverse() * accumulatedTorque;
        AngularAcceleration = inverseInertiaTorque;

        // Intégration de la vélocité
        LinearVelocity += LinearAcceleration * deltaTime;
        AngularVelocity += AngularAcceleration * deltaTime;

        // Amortissement pour la stabilité (RÉDUIT pour plus de mouvement)
        LinearVelocity *= linearDamping;
        AngularVelocity *= angularDamping;

        // Réinitialise les forces
        accumulatedForce = Vector3.zero;
        accumulatedTorque = Vector3.zero;
    }

    /// <summary>Intègre la position et la rotation</summary>
    public void IntegrateVelocity(float deltaTime)
    {
        // Mise à jour de la position
        Position += LinearVelocity * deltaTime;

        // Mise à jour de la rotation
        if (AngularVelocity.sqrMagnitude > 0.00001f)
        {
            float angle = AngularVelocity.magnitude * deltaTime;
            Vector3 axis = AngularVelocity.normalized;
            Quaternion deltaRotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis);
            Rotation = deltaRotation * Rotation;
            Rotation = Quaternion.Normalize(Rotation);
        }
    }

    /// <summary>Définit le tenseur d'inertie pour un cube</summary>
    public void SetCubeInertia(Vector3 size)
    {
        float x2 = size.x * size.x;
        float y2 = size.y * size.y;
        float z2 = size.z * size.z;
        float m12 = Mass / 12f;

        InertiaTensor = new Matrix3x3(
            m12 * (y2 + z2), 0, 0,
            0, m12 * (x2 + z2), 0,
            0, 0, m12 * (x2 + y2)
        );
    }

    /// <summary>Réinitialise les forces et accélérations</summary>
    public void ResetForces()
    {
        accumulatedForce = Vector3.zero;
        accumulatedTorque = Vector3.zero;
    }

    /// <summary>Retourne la vélocité linéaire maximale possible</summary>
    public float GetMaxLinearVelocity() => LinearVelocity.magnitude;

    /// <summary>Retourne la vélocité angulaire maximale possible</summary>
    public float GetMaxAngularVelocity() => AngularVelocity.magnitude;
}
