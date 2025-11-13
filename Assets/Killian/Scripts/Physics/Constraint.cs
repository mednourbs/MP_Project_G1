using UnityEngine;

/// <summary>
/// Représente une contrainte entre deux shards modélisée comme un ressort rigide.
/// Mesure la déformation et applique les forces de contrainte.
/// </summary>
public class Constraint
{
    public int ID { get; private set; }
    public RigidBody BodyA { get; private set; }
    public RigidBody BodyB { get; private set; }
    public Vector3 AnchorA { get; private set; }
    public Vector3 AnchorB { get; private set; }
    public float RestDistance { get; private set; }
    public float Stiffness { get; private set; }  // k (rigidité du ressort)
    public float BreakingThreshold { get; private set; }  // Seuil de rupture
    public bool IsBroken { get; private set; }

    public Constraint(int id, RigidBody bodyA, RigidBody bodyB, 
                     Vector3 anchorA, Vector3 anchorB, 
                     float stiffness, float breakingThreshold)
    {
        ID = id;
        BodyA = bodyA;
        BodyB = bodyB;
        AnchorA = anchorA;
        AnchorB = anchorB;
        RestDistance = Vector3.Distance(anchorA, anchorB);
        Stiffness = stiffness;
        BreakingThreshold = breakingThreshold;
        IsBroken = false;
    }

    /// <summary>Calcule la position actuelle de l'ancre A dans l'espace monde</summary>
    public Vector3 GetWorldAnchorA()
    {
        // Applique la rotation locale
        Vector3 rotatedAnchor = RotateVector(AnchorA, BodyA.Rotation);
        return BodyA.Position + rotatedAnchor;
    }

    /// <summary>Calcule la position actuelle de l'ancre B dans l'espace monde</summary>
    public Vector3 GetWorldAnchorB()
    {
        Vector3 rotatedAnchor = RotateVector(AnchorB, BodyB.Rotation);
        return BodyB.Position + rotatedAnchor;
    }

    /// <summary>Calcule la déformation (violation) de la contrainte</summary>
    public float GetDeformation()
    {
        float currentDistance = Vector3.Distance(GetWorldAnchorA(), GetWorldAnchorB());
        float deformation = currentDistance - RestDistance;
        return deformation;
    }

    /// <summary>Calcule l'énergie potentielle stockée : E = 0.5 * k * x²</summary>
    public float GetStoredEnergy()
    {
        float deformation = GetDeformation();
        return 0.5f * Stiffness * deformation * deformation;
    }

    /// <summary>Vérifie si la contrainte se casse et retourne l'énergie libérée</summary>
    public float UpdateAndCheckBreaking()
    {
        if (IsBroken) return 0f;

        float deformation = GetDeformation();
        if (Mathf.Abs(deformation) > BreakingThreshold)
        {
            IsBroken = true;
            return GetStoredEnergy();
        }
        return 0f;
    }

    /// <summary>Applique les forces de contrainte aux deux corps rigides</summary>
    public void ApplyConstraintForce()
    {
        if (IsBroken) return;

        Vector3 anchorA = GetWorldAnchorA();
        Vector3 anchorB = GetWorldAnchorB();
        Vector3 delta = anchorB - anchorA;
        float distance = delta.magnitude;

        if (distance < 0.0001f) return;

        // Force du ressort : F = -k * x
        float deformation = distance - RestDistance;
        float forceMagnitude = -Stiffness * deformation;
        Vector3 forceDirection = delta.normalized;
        Vector3 constraintForce = forceDirection * forceMagnitude;

        // Applique les forces aux ancres
        BodyA.AddForceAtPoint(constraintForce, anchorA);
        BodyB.AddForceAtPoint(-constraintForce, anchorB);
    }

    /// <summary>Applique une impulsion linéaire à un corps basée sur l'énergie stockée</summary>
    public static void ApplyImpulseFromEnergy(RigidBody body, Vector3 direction, float energy)
    {
        // ?v = sqrt(2E / m)
        float velocity = Mathf.Sqrt(2f * energy / body.Mass);
        Vector3 impulse = direction.normalized * (velocity * body.Mass);
        body.AddImpulse(impulse);
    }

    /// <summary>Fait tourner un vecteur selon un quaternion</summary>
    private static Vector3 RotateVector(Vector3 v, Quaternion q)
    {
        // v' = q * v * q^-1 (pour un vecteur pure)
        float x = q.x, y = q.y, z = q.z, w = q.w;
        float vx = v.x, vy = v.y, vz = v.z;

        float qvx = w * vx + y * vz - z * vy;
        float qvy = w * vy + z * vx - x * vz;
        float qvz = w * vz + x * vy - y * vx;
        float qvw = -x * vx - y * vy - z * vz;

        return new Vector3(
            qvx * w + qvw * -x + qvy * -z - qvz * -y,
            qvy * w + qvw * -y + qvz * -x - qvx * -z,
            qvz * w + qvw * -z + qvx * -y - qvy * -x
        );
    }
}
