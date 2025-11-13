using UnityEngine;

/// <summary>
/// Système de détection de collision personnalisé
/// Sans utiliser les colliders Unity
/// </summary>
public static class CollisionDetection
{
    public struct CollisionInfo
    {
        public bool hasCollision;
        public Vector3 normal;
        public Vector3 contactPoint;
        public float penetrationDepth;
    }
    
    /// <summary>
    /// Détecte collision entre une sphère et un plan
    /// Basé sur le calcul de distance point-plan (DistancePointToPlane.cs)
    /// </summary>
    public static CollisionInfo SphereToPlane(Vector3 sphereCenter, float sphereRadius, Vector3 planePoint, Vector3 planeNormal)
    {
        CollisionInfo info = new CollisionInfo();
        planeNormal.Normalize();
        
        // Distance du centre de la sphère au plan
        // Formule: d = |normal · (sphereCenter - planePoint)|
        float distance = Vector3.Dot(planeNormal, sphereCenter - planePoint);
        
        // Collision si distance < rayon
        if (distance < sphereRadius)
        {
            info.hasCollision = true;
            info.normal = planeNormal;
            info.penetrationDepth = sphereRadius - distance;
            info.contactPoint = sphereCenter - planeNormal * distance;
        }
        else
        {
            info.hasCollision = false;
        }
        
        return info;
    }
    
    /// <summary>
    /// Détecte collision entre une sphère et une box (AABB)
    /// </summary>
    public static CollisionInfo SphereToBox(Vector3 sphereCenter, float sphereRadius, Vector3 boxCenter, Vector3 boxSize)
    {
        CollisionInfo info = new CollisionInfo();
        
        // Trouve le point le plus proche sur la box
        Vector3 closestPoint = new Vector3(
            Mathf.Clamp(sphereCenter.x, boxCenter.x - boxSize.x / 2, boxCenter.x + boxSize.x / 2),
            Mathf.Clamp(sphereCenter.y, boxCenter.y - boxSize.y / 2, boxCenter.y + boxSize.y / 2),
            Mathf.Clamp(sphereCenter.z, boxCenter.z - boxSize.z / 2, boxCenter.z + boxSize.z / 2)
        );
        
        // Distance entre le centre de la sphère et le point le plus proche
        Vector3 diff = sphereCenter - closestPoint;
        float distance = diff.magnitude;
        
        if (distance < sphereRadius)
        {
            info.hasCollision = true;
            info.contactPoint = closestPoint;
            info.penetrationDepth = sphereRadius - distance;
            
            // Normal de collision
            if (distance > 0.0001f)
            {
                info.normal = diff.normalized;
            }
            else
            {
                // La sphère est exactement sur la box, utiliser la direction vers le centre
                info.normal = Vector3.up;
            }
        }
        else
        {
            info.hasCollision = false;
        }
        
        return info;
    }
    
    /// <summary>
    /// Détecte collision entre une sphère et une box orientée (OBB)
    /// </summary>
    public static CollisionInfo SphereToOrientedBox(Vector3 sphereCenter, float sphereRadius, 
                                                     Vector3 boxCenter, Vector3 boxSize, Matrix4x4 boxRotation)
    {
        // Transformer la sphère dans l'espace local de la box
        Matrix4x4 invRotation = boxRotation.transpose;
        Vector3 localSphereCenter = CustomRigidBody.MultiplyMatrixVector3(invRotation, sphereCenter - boxCenter);
        
        // Détection dans l'espace local (AABB)
        CollisionInfo localInfo = SphereToBox(localSphereCenter, sphereRadius, Vector3.zero, boxSize);
        
        if (localInfo.hasCollision)
        {
            // Transformer les résultats dans l'espace monde
            CollisionInfo worldInfo = new CollisionInfo();
            worldInfo.hasCollision = true;
            worldInfo.normal = CustomRigidBody.MultiplyMatrixVector3(boxRotation, localInfo.normal);
            worldInfo.contactPoint = CustomRigidBody.MultiplyMatrixVector3(boxRotation, localInfo.contactPoint) + boxCenter;
            worldInfo.penetrationDepth = localInfo.penetrationDepth;
            return worldInfo;
        }
        
        return localInfo;
    }
    
    /// <summary>
    /// Détecte collision sphère-sphère
    /// </summary>
    public static CollisionInfo SphereToSphere(Vector3 center1, float radius1, Vector3 center2, float radius2)
    {
        CollisionInfo info = new CollisionInfo();
        
        Vector3 diff = center2 - center1;
        float distance = diff.magnitude;
        float radiusSum = radius1 + radius2;
        
        if (distance < radiusSum)
        {
            info.hasCollision = true;
            info.penetrationDepth = radiusSum - distance;
            
            if (distance > 0.0001f)
            {
                info.normal = diff.normalized;
            }
            else
            {
                info.normal = Vector3.up;
            }
            
            info.contactPoint = center1 + info.normal * radius1;
        }
        else
        {
            info.hasCollision = false;
        }
        
        return info;
    }
    
    /// <summary>
    /// Projection d'un vecteur sur un autre (référence ProjectionDistance.cs)
    /// </summary>
    public static Vector3 ProjectVector(Vector3 u, Vector3 v)
    {
        float dotProductUV = Vector3.Dot(u, v);
        float dotProductVV = Vector3.Dot(v, v);
        
        if (dotProductVV < 0.0001f)
            return Vector3.zero;
        
        return (dotProductUV / dotProductVV) * v;
    }
    
    /// <summary>
    /// Distance d'un point à une droite
    /// </summary>
    public static float DistancePointToLine(Vector3 point, Vector3 linePoint, Vector3 lineDirection)
    {
        Vector3 v = point - linePoint;
        Vector3 projection = ProjectVector(v, lineDirection);
        Vector3 perpendicular = v - projection;
        return perpendicular.magnitude;
    }
    
    /// <summary>
    /// Détecte collision entre une sphère et une surface courbe (pour la rampe)
    /// Utilise une approximation par segments
    /// </summary>
    public static CollisionInfo SphereToMesh(Vector3 sphereCenter, float sphereRadius, Vector3[] vertices, int[] triangles)
    {
        CollisionInfo closestCollision = new CollisionInfo();
        closestCollision.hasCollision = false;
        float minPenetration = float.MaxValue;
        
        // Vérifier chaque triangle du mesh
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v0 = vertices[triangles[i]];
            Vector3 v1 = vertices[triangles[i + 1]];
            Vector3 v2 = vertices[triangles[i + 2]];
            
            // Calculer la normale du triangle
            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 normal = Vector3.Cross(edge1, edge2).normalized;
            
            // Distance point-plan du triangle
            float distance = Vector3.Dot(normal, sphereCenter - v0);
            
            // Si la sphère est trop loin, ignorer
            if (Mathf.Abs(distance) > sphereRadius)
                continue;
            
            // Point projeté sur le plan du triangle
            Vector3 projectedPoint = sphereCenter - normal * distance;
            
            // Vérifier si le point projeté est dans le triangle
            if (IsPointInTriangle(projectedPoint, v0, v1, v2, normal))
            {
                float penetration = sphereRadius - distance;
                if (distance < sphereRadius && penetration < minPenetration)
                {
                    closestCollision.hasCollision = true;
                    closestCollision.normal = normal;
                    closestCollision.contactPoint = projectedPoint;
                    closestCollision.penetrationDepth = penetration;
                    minPenetration = penetration;
                }
            }
        }
        
        return closestCollision;
    }
    
    /// <summary>
    /// Vérifie si un point est dans un triangle (coordonnées barycentriques)
    /// </summary>
    private static bool IsPointInTriangle(Vector3 p, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 normal)
    {
        // Test avec les produits vectoriels
        Vector3 edge0 = v1 - v0;
        Vector3 edge1 = v2 - v1;
        Vector3 edge2 = v0 - v2;
        
        Vector3 c0 = p - v0;
        Vector3 c1 = p - v1;
        Vector3 c2 = p - v2;
        
        if (Vector3.Dot(normal, Vector3.Cross(edge0, c0)) < 0) return false;
        if (Vector3.Dot(normal, Vector3.Cross(edge1, c1)) < 0) return false;
        if (Vector3.Dot(normal, Vector3.Cross(edge2, c2)) < 0) return false;
        
        return true;
    }
    
    /// <summary>
    /// Résolution de collision avec restitution et friction
    /// </summary>
    public static void ResolveCollision(CustomRigidBody body, CollisionInfo collision, 
                                       float restitution = 0.5f, float friction = 0.3f)
    {
        if (!collision.hasCollision) return;
        
        // Séparer les objets
        body.position += collision.normal * collision.penetrationDepth;
        
        // Vélocité relative au point de contact
        Vector3 relativeVelocity = body.velocity;
        
        // Composante normale de la vélocité
        float velocityAlongNormal = Vector3.Dot(relativeVelocity, collision.normal);
        
        // Ne résoudre que si les objets s'approchent
        if (velocityAlongNormal > 0) return;
        
        // Calculer l'impulsion
        float j = -(1 + restitution) * velocityAlongNormal;
        j /= (1f / body.mass);
        
        // Appliquer l'impulsion normale
        Vector3 impulse = j * collision.normal;
        body.velocity += impulse / body.mass;
        
        // Friction - composante tangentielle
        Vector3 tangent = relativeVelocity - velocityAlongNormal * collision.normal;
        float tangentMagnitude = tangent.magnitude;
        
        if (tangentMagnitude > 0.001f)
        {
            tangent.Normalize();
            
            // Appliquer friction proportionnelle à la force normale
            float frictionMagnitude = friction * Mathf.Abs(j);
            Vector3 frictionImpulse = -frictionMagnitude * tangent;
            
            // Limiter la friction pour ne pas inverser le mouvement
            if (frictionMagnitude > tangentMagnitude * body.mass)
            {
                frictionImpulse = -tangent * tangentMagnitude * body.mass;
            }
            
            body.velocity += frictionImpulse / body.mass;
        }
    }
}
