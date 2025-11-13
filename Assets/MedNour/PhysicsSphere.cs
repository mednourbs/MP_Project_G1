using UnityEngine;

/// <summary>
/// Sphère procédurale avec physique personnalisée
/// Utilise RK4 pour l'intégration et détection de collision manuelle
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PhysicsSphere : MonoBehaviour
{
    [Header("Propriétés physiques")]
    public float radius = 0.5f;
    public float mass = 1f;
    public float gravity = 9.81f;
    public float airResistance = 0.1f;
    public float rollingFriction = 0.3f;
    public float restitution = 0.6f;
    public float friction = 0.4f;
    
    [Header("Visuel")]
    public Color sphereColor = Color.blue;
    public int segments = 24;
    
    [Header("État initial")]
    public Vector3 initialPosition = new Vector3(0, 5, 0);
    public Vector3 initialVelocity = Vector3.zero;
    
    // Corps rigide personnalisé
    private CustomRigidBody rigidBody;
    
    // Mesh
    private Mesh mesh;
    private Vector3[] originalVertices;
    
    void Start()
    {
        // Créer le corps rigide
        rigidBody = new CustomRigidBody(initialPosition, radius, mass);
        rigidBody.velocity = initialVelocity;
        
        // Créer le mesh de la sphère
        CreateSphereMesh();
        
        // Configurer le renderer
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        renderer.material.color = sphereColor;
    }
    
    void FixedUpdate()
    {
        if (rigidBody == null) return;
        
        float dt = Time.fixedDeltaTime;
        
        // Calculer la force de résistance de l'air: F_drag = -0.5 * ρ * v² * C_d * A
        // Simplifié: F_drag = -airResistance * v * |v|
        Vector3 airDragForce = -airResistance * rigidBody.velocity.magnitude * rigidBody.velocity;
        
        // Intégration RK4 avec résistance de l'air
        Vector3 totalAcceleration = Vector3.down * gravity + (airDragForce / rigidBody.mass);
        rigidBody.IntegrateRK4(dt, totalAcceleration, 0f);
        
        // Mettre à jour la visualisation
        UpdateMesh();
    }
    
    /// <summary>
    /// Crée un mesh de sphère procédural
    /// </summary>
    void CreateSphereMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        // Générer les vertices de la sphère
        int vertexCount = (segments + 1) * (segments + 1);
        originalVertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        
        int index = 0;
        for (int lat = 0; lat <= segments; lat++)
        {
            float theta = lat * Mathf.PI / segments;
            float sinTheta = Mathf.Sin(theta);
            float cosTheta = Mathf.Cos(theta);
            
            for (int lon = 0; lon <= segments; lon++)
            {
                float phi = lon * 2 * Mathf.PI / segments;
                float sinPhi = Mathf.Sin(phi);
                float cosPhi = Mathf.Cos(phi);
                
                float x = cosPhi * sinTheta;
                float y = cosTheta;
                float z = sinPhi * sinTheta;
                
                originalVertices[index] = new Vector3(x, y, z) * radius;
                uv[index] = new Vector2((float)lon / segments, (float)lat / segments);
                index++;
            }
        }
        
        // Générer les triangles
        int triCount = segments * segments * 6;
        int[] triangles = new int[triCount];
        int triIndex = 0;
        
        for (int lat = 0; lat < segments; lat++)
        {
            for (int lon = 0; lon < segments; lon++)
            {
                int current = lat * (segments + 1) + lon;
                int next = current + segments + 1;
                
                triangles[triIndex++] = current;
                triangles[triIndex++] = next;
                triangles[triIndex++] = current + 1;
                
                triangles[triIndex++] = current + 1;
                triangles[triIndex++] = next;
                triangles[triIndex++] = next + 1;
            }
        }
        
        mesh.vertices = originalVertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
    
    /// <summary>
    /// Met à jour le mesh avec la transformation du corps rigide
    /// </summary>
    void UpdateMesh()
    {
        Matrix4x4 transform = rigidBody.GetTransformMatrix();
        
        Vector3[] transformedVertices = new Vector3[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector4 v = new Vector4(originalVertices[i].x, originalVertices[i].y, originalVertices[i].z, 1);
            Vector4 transformed = transform * v;
            transformedVertices[i] = new Vector3(transformed.x, transformed.y, transformed.z);
        }
        
        mesh.vertices = transformedVertices;
        mesh.RecalculateBounds();
    }
    
    /// <summary>
    /// Détecte et résout les collisions
    /// </summary>
    public void CheckCollisionWithPlane(Vector3 planePoint, Vector3 planeNormal)
    {
        if (rigidBody == null) return;
        
        CollisionDetection.CollisionInfo collision = 
            CollisionDetection.SphereToPlane(rigidBody.position, radius, planePoint, planeNormal);
        
        if (collision.hasCollision)
        {
            CollisionDetection.ResolveCollision(rigidBody, collision, restitution, rollingFriction);
        }
    }
    
    /// <summary>
    /// Détecte collision avec un mesh (pour la rampe)
    /// </summary>
    public void CheckCollisionWithMesh(Vector3[] vertices, int[] triangles)
    {
        CollisionDetection.CollisionInfo collision = 
            CollisionDetection.SphereToMesh(rigidBody.position, radius, vertices, triangles);
        
        if (collision.hasCollision)
        {
            CollisionDetection.ResolveCollision(rigidBody, collision, restitution, rollingFriction);
        }
    }
    
    /// <summary>
    /// Détecte collision avec une box
    /// </summary>
    public void CheckCollisionWithBox(Vector3 boxCenter, Vector3 boxSize, Matrix4x4 boxRotation)
    {
        CollisionDetection.CollisionInfo collision = 
            CollisionDetection.SphereToOrientedBox(rigidBody.position, radius, boxCenter, boxSize, boxRotation);
        
        if (collision.hasCollision)
        {
            CollisionDetection.ResolveCollision(rigidBody, collision, restitution, rollingFriction);
        }
    }
    
    /// <summary>
    /// Applique une force externe
    /// </summary>
    public void ApplyForce(Vector3 force)
    {
        rigidBody.ApplyForce(force * Time.fixedDeltaTime);
    }
    
    /// <summary>
    /// Applique une impulsion
    /// </summary>
    public void ApplyImpulse(Vector3 impulse)
    {
        rigidBody.ApplyImpulse(impulse, rigidBody.position);
    }
    
    public CustomRigidBody GetRigidBody()
    {
        return rigidBody;
    }
    
    public Vector3[] GetMeshVertices()
    {
        return mesh.vertices;
    }
    
    public int[] GetMeshTriangles()
    {
        return mesh.triangles;
    }
}
