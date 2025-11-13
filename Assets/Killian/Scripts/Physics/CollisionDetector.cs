using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Détecte et résout les collisions entre corps rigides.
/// Utilise une détection de sphère simplifiée pour la performance.
/// Génère des couples pour les rotations réalistes.
/// </summary>
public class CollisionDetector
{
    public struct CollisionInfo
    {
        public RigidBody BodyA;
        public RigidBody BodyB;
        public Vector3 ContactPoint;
        public Vector3 Normal;
        public float Depth;
        public Vector3 RelativeContactPointA;
        public Vector3 RelativeContactPointB;
    }

    /// <summary>Détecte toutes les collisions entre corps avec rayon de collision sphérique</summary>
    public static List<CollisionInfo> DetectCollisions(List<RigidBody> bodies, float sphereRadius)
    {
        List<CollisionInfo> collisions = new List<CollisionInfo>();

        for (int i = 0; i < bodies.Count; i++)
        {
            for (int j = i + 1; j < bodies.Count; j++)
            {
                if (CheckCollision(bodies[i], bodies[j], sphereRadius, out CollisionInfo info))
                {
                    collisions.Add(info);
                }
            }
        }

        return collisions;
    }

    /// <summary>Vérifie la collision entre deux sphères</summary>
    private static bool CheckCollision(RigidBody bodyA, RigidBody bodyB, float radius, out CollisionInfo info)
    {
        info = new CollisionInfo();
        
        Vector3 delta = bodyB.Position - bodyA.Position;
        float distance = delta.magnitude;
        float minDistance = radius * 2f;

        if (distance < minDistance && distance > 0.0001f)
        {
            info.BodyA = bodyA;
            info.BodyB = bodyB;
            info.Normal = delta.normalized;
            info.ContactPoint = bodyA.Position + info.Normal * radius;
            info.Depth = minDistance - distance;
            
            // Points de contact relatifs pour le couple
            info.RelativeContactPointA = info.ContactPoint - bodyA.Position;
            info.RelativeContactPointB = info.ContactPoint - bodyB.Position;
            
            return true;
        }

        return false;
    }

    /// <summary>Résout une collision avec réponse élastique et génère du couple</summary>
    public static void ResolveCollision(CollisionInfo collision, float restitution = 0.8f)
    {
        RigidBody a = collision.BodyA;
        RigidBody b = collision.BodyB;

        if (a.IsKinematic && b.IsKinematic) return;

        // Vecteur relatif de vélocité au point de contact (incluant rotation)
        Vector3 velA = a.LinearVelocity + Vector3.Cross(a.AngularVelocity, collision.RelativeContactPointA);
        Vector3 velB = b.LinearVelocity + Vector3.Cross(b.AngularVelocity, collision.RelativeContactPointB);
        Vector3 vRel = velB - velA;
        
        float velAlongNormal = Vector3.Dot(vRel, collision.Normal);

        // N'agis que si les objets se rapprochent
        if (velAlongNormal >= 0) return;

        // Calcule l'impulsion (incluant inertie de rotation)
        float invMassA = a.IsKinematic ? 0 : 1f / a.Mass;
        float invMassB = b.IsKinematic ? 0 : 1f / b.Mass;

        // Contribution du moment d'inertie
        Vector3 rAcrossN = Vector3.Cross(collision.RelativeContactPointA, collision.Normal);
        Vector3 rBcrossN = Vector3.Cross(collision.RelativeContactPointB, collision.Normal);
        
        float invInertiaA = 0;
        float invInertiaB = 0;
        if (!a.IsKinematic)
            invInertiaA = Vector3.Dot(rAcrossN, a.InertiaTensor.Inverse() * rAcrossN);
        if (!b.IsKinematic)
            invInertiaB = Vector3.Dot(rBcrossN, b.InertiaTensor.Inverse() * rBcrossN);

        float j = -(1f + restitution) * velAlongNormal / (invMassA + invMassB + invInertiaA + invInertiaB);

        Vector3 impulse = collision.Normal * j;

        // Applique les impulsions linéaires et angulaires
        if (!a.IsKinematic)
        {
            a.AddImpulse(-impulse);
            a.AddAngularImpulse(-Vector3.Cross(collision.RelativeContactPointA, impulse));
        }
        if (!b.IsKinematic)
        {
            b.AddImpulse(impulse);
            b.AddAngularImpulse(Vector3.Cross(collision.RelativeContactPointB, impulse));
        }
    }

    /// <summary>Sépare les corps qui se chevauchent</summary>
    public static void SeparateOverlapping(CollisionInfo collision)
    {
        RigidBody a = collision.BodyA;
        RigidBody b = collision.BodyB;

        if (a.IsKinematic && b.IsKinematic) return;

        float totalMass = a.Mass + b.Mass;
        float ratio = a.IsKinematic ? 1f : (a.Mass / totalMass);

        if (!a.IsKinematic)
            a.Position -= collision.Normal * collision.Depth * ratio;
        if (!b.IsKinematic)
            b.Position += collision.Normal * collision.Depth * (1f - ratio);
    }
}
