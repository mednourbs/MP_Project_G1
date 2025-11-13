using UnityEngine;

/// <summary>
/// Rampe de skate procédurale avec forme de demi-cylindre
/// Génère un mesh courbe pour simuler une vraie rampe
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SkateRamp : MonoBehaviour
{
    [Header("Dimensions de la rampe")]
    public float width = 20f;
    public float height = 30f;
    public float length = 15f;
    public float thickness = 4.4f; // Épaisseur de la rampe
    public int segments = 50;
    
    [Header("Position")]
    public Vector3 position = new Vector3(0, 0, 0);
    
    [Header("Visuel")]
    public Color rampColor = new Color(1f, 0.5f, 0f); // Orange vif par défaut
    
    private Mesh mesh;
    private Vector3[] worldVertices;
    private int[] triangles;
    
    void Start()
    {
        CreateRampMesh();
        
        // Configurer le renderer avec un matériau bien visible
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = rampColor;
        
        // Activer le mode double-face pour que la rampe soit visible des deux côtés
        mat.SetFloat("_Cull", 0); // 0 = Off (double-sided)
        
        // Ajouter un peu d'émission pour que ce soit plus visible
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", rampColor * 0.3f);
        
        renderer.material = mat;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;
    }
    
    /// <summary>
    /// Crée le mesh de la rampe courbe en forme de U (deux paraboles) - SIMPLE ET VISIBLE
    /// </summary>
    void CreateRampMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        // Coefficients pour parabole U: y = a(x - vertexX)²
        float a = 4f * height / (length * length);
        float vertexX = length / 2f;
        
        // Nombre de vertices: (segments + 1) * 2 (deux côtés de la largeur)
        // On crée DEUX surfaces identiques avec winding opposé pour double-sided
        int vertexCount = (segments + 1) * 2 * 2; // x2 pour les deux côtés
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        
        int backVertexOffset = (segments + 1) * 2;
        
        // Générer les vertices le long de la courbe en U
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float x = length * t;
            float y = a * (x - vertexX) * (x - vertexX);
            
            // Face avant (visible de dessus)
            vertices[i * 2] = new Vector3(x, y, -width / 2);
            vertices[i * 2 + 1] = new Vector3(x, y, width / 2);
            
            // Face arrière (même vertices, pour le backface)
            vertices[backVertexOffset + i * 2] = new Vector3(x, y, -width / 2);
            vertices[backVertexOffset + i * 2 + 1] = new Vector3(x, y, width / 2);
            
            uv[i * 2] = new Vector2(t, 0);
            uv[i * 2 + 1] = new Vector2(t, 1);
            uv[backVertexOffset + i * 2] = new Vector2(t, 0);
            uv[backVertexOffset + i * 2 + 1] = new Vector2(t, 1);
        }
        
        // Générer les triangles (face avant + face arrière)
        int triCount = segments * 2 * 3 * 2; // x2 pour double-sided
        triangles = new int[triCount];
        int triIndex = 0;
        
        // Face avant (winding normal)
        for (int i = 0; i < segments; i++)
        {
            int current = i * 2;
            int next = (i + 1) * 2;
            
            triangles[triIndex++] = current;
            triangles[triIndex++] = next;
            triangles[triIndex++] = current + 1;
            
            triangles[triIndex++] = current + 1;
            triangles[triIndex++] = next;
            triangles[triIndex++] = next + 1;
        }
        
        // Face arrière (winding inversé pour visibilité de l'autre côté)
        for (int i = 0; i < segments; i++)
        {
            int current = backVertexOffset + i * 2;
            int next = backVertexOffset + (i + 1) * 2;
            
            // Inverser l'ordre pour backface culling
            triangles[triIndex++] = current + 1;
            triangles[triIndex++] = next;
            triangles[triIndex++] = current;
            
            triangles[triIndex++] = next + 1;
            triangles[triIndex++] = next;
            triangles[triIndex++] = current + 1;
        }
        
        // Appliquer la transformation de position
        worldVertices = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            worldVertices[i] = vertices[i] + position;
        }
        
        mesh.vertices = worldVertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        // Debug info
        Debug.Log($"Rampe U créée: {vertices.Length} vertices, {triangles.Length / 3} triangles");
        Debug.Log($"Position: {position}, Taille: L={length}, H={height}, W={width}");
        Debug.Log($"Bounds: {mesh.bounds}");
    }
    
    /// <summary>
    /// Retourne les vertices dans l'espace monde
    /// </summary>
    public Vector3[] GetWorldVertices()
    {
        return worldVertices;
    }
    
    /// <summary>
    /// Retourne les triangles
    /// </summary>
    public int[] GetTriangles()
    {
        return triangles;
    }
    

    
    /// <summary>
    /// Vérifie la collision avec la rampe en U et retourne les informations
    /// </summary>
    public CollisionDetection.CollisionInfo CheckSphereCollision(Vector3 sphereCenter, float sphereRadius)
    {
        CollisionDetection.CollisionInfo info = new CollisionDetection.CollisionInfo();
        info.hasCollision = false;
        
        // Convertir en espace local
        Vector3 localPoint = sphereCenter - position;
        
        // Vérifier si le point est dans la zone de la rampe (en Z)
        if (localPoint.z < -width / 2 - sphereRadius || localPoint.z > width / 2 + sphereRadius)
            return info;
        
        // Vérifier si dans la zone X de la rampe
        if (localPoint.x < -sphereRadius || localPoint.x > length + sphereRadius)
            return info;
        
        // Coefficients de la parabole en U: y = a(x - vertexX)²
        float a = 4f * height / (length * length);
        float vertexX = length / 2f;
        
        // Trouver le point le plus proche sur la courbe
        float x = Mathf.Clamp(localPoint.x, 0, length);
        float curveY = a * (x - vertexX) * (x - vertexX);
        
        Vector3 curvePoint = new Vector3(x, curveY, localPoint.z);
        Vector3 toSphere = localPoint - curvePoint;
        float distance = toSphere.magnitude;
        
        if (distance < sphereRadius)
        {
            // Calculer la normale (dérivée de y = a(x - vertexX)²)
            // dy/dx = 2a(x - vertexX)
            float dy_dx = 2f * a * (x - vertexX);
            Vector3 tangent = new Vector3(1f, dy_dx, 0).normalized;
            Vector3 normal = new Vector3(-tangent.y, tangent.x, 0).normalized;
            
            info.hasCollision = true;
            info.contactPoint = curvePoint + position;
            info.normal = normal;
            info.penetrationDepth = sphereRadius - distance;
        }
        
        return info;
    }
}
