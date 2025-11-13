using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Fragment d'un objet fracturé avec son propre corps rigide
/// </summary>
public class FracturedFragment
{
    public CustomRigidBody rigidBody;
    public GameObject gameObject;
    public Mesh mesh;
    public Vector3[] localVertices;
    public int[] triangles;
    public bool isActive = true;
    
    public FracturedFragment(Vector3 center, Vector3 size, float mass, Color color)
    {
        rigidBody = new CustomRigidBody(center, size, mass);
        
        // Créer le GameObject
        gameObject = new GameObject("Fragment");
        gameObject.transform.position = Vector3.zero;
        
        // Ajouter les composants
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        
        // Créer le mesh du cube
        CreateCubeMesh(size);
        meshFilter.mesh = mesh;
        
        // Matériau
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        meshRenderer.material = mat;
    }
    
    void CreateCubeMesh(Vector3 size)
    {
        mesh = new Mesh();
        
        float hx = size.x / 2;
        float hy = size.y / 2;
        float hz = size.z / 2;
        
        localVertices = new Vector3[8]
        {
            new Vector3(-hx, -hy, -hz),
            new Vector3(hx, -hy, -hz),
            new Vector3(hx, hy, -hz),
            new Vector3(-hx, hy, -hz),
            new Vector3(-hx, -hy, hz),
            new Vector3(hx, -hy, hz),
            new Vector3(hx, hy, hz),
            new Vector3(-hx, hy, hz)
        };
        
        triangles = new int[]
        {
            0,2,1, 0,3,2, // Front
            4,5,6, 4,6,7, // Back
            0,1,5, 0,5,4, // Bottom
            2,3,7, 2,7,6, // Top
            1,2,6, 1,6,5, // Right
            0,4,7, 0,7,3  // Left
        };
        
        mesh.vertices = localVertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
    
    public void UpdateMesh()
    {
        Matrix4x4 transform = rigidBody.GetTransformMatrix();
        
        Vector3[] transformedVertices = new Vector3[localVertices.Length];
        for (int i = 0; i < localVertices.Length; i++)
        {
            Vector4 v = new Vector4(localVertices[i].x, localVertices[i].y, localVertices[i].z, 1);
            Vector4 transformed = transform * v;
            transformedVertices[i] = new Vector3(transformed.x, transformed.y, transformed.z);
        }
        
        mesh.vertices = transformedVertices;
        mesh.RecalculateBounds();
    }
    
    public void Destroy()
    {
        if (gameObject != null)
            Object.Destroy(gameObject);
    }
}

/// <summary>
/// Contrainte entre deux fragments (ressort rigide)
/// </summary>
public class FragmentConstraint
{
    public FracturedFragment fragment1;
    public FracturedFragment fragment2;
    
    public float restLength; // Longueur au repos
    public float stiffness = 1000f; // Rigidité du ressort
    public float breakThreshold = 10f; // Seuil de rupture
    
    public bool isBroken = false;
    
    public FragmentConstraint(FracturedFragment f1, FracturedFragment f2, float k, float threshold)
    {
        fragment1 = f1;
        fragment2 = f2;
        restLength = Vector3.Distance(f1.rigidBody.position, f2.rigidBody.position);
        stiffness = k;
        breakThreshold = threshold;
    }
    
    /// <summary>
    /// Calcule la déformation actuelle de la contrainte
    /// </summary>
    public float GetDeformation()
    {
        float currentLength = Vector3.Distance(fragment1.rigidBody.position, fragment2.rigidBody.position);
        return Mathf.Abs(currentLength - restLength);
    }
    
    /// <summary>
    /// Calcule l'énergie potentielle stockée: E = 1/2 * k * x²
    /// </summary>
    public float GetStoredEnergy()
    {
        float deformation = GetDeformation();
        return 0.5f * stiffness * deformation * deformation;
    }
    
    /// <summary>
    /// Vérifie si la contrainte doit se rompre
    /// </summary>
    public bool ShouldBreak()
    {
        return !isBroken && GetDeformation() > breakThreshold;
    }
    
    /// <summary>
    /// Applique la force de contrainte aux deux fragments
    /// </summary>
    public void ApplyConstraintForce()
    {
        if (isBroken) return;
        
        Vector3 direction = fragment2.rigidBody.position - fragment1.rigidBody.position;
        float currentLength = direction.magnitude;
        
        if (currentLength < 0.0001f) return;
        
        direction.Normalize();
        
        // Force du ressort: F = k * (currentLength - restLength)
        float extension = currentLength - restLength;
        float forceMagnitude = stiffness * extension;
        
        Vector3 force = direction * forceMagnitude;
        
        // Appliquer la force aux deux fragments
        fragment1.rigidBody.ApplyForce(force);
        fragment2.rigidBody.ApplyForce(-force);
    }
    
    /// <summary>
    /// Rompt la contrainte et applique l'énergie stockée comme impulsion
    /// ΔV = sqrt(2E / m)
    /// </summary>
    public void Break()
    {
        if (isBroken) return;
        
        isBroken = true;
        
        // Calculer l'énergie stockée
        float energy = GetStoredEnergy();
        
        // Direction de l'impulsion
        Vector3 direction = (fragment2.rigidBody.position - fragment1.rigidBody.position).normalized;
        
        // Calculer la magnitude de l'impulsion pour chaque fragment
        // ΔV = sqrt(2E / m)
        float impulse1Magnitude = Mathf.Sqrt(2f * energy / fragment1.rigidBody.mass);
        float impulse2Magnitude = Mathf.Sqrt(2f * energy / fragment2.rigidBody.mass);
        
        // Appliquer les impulsions dans des directions opposées
        Vector3 impulse1 = direction * impulse1Magnitude;
        Vector3 impulse2 = -direction * impulse2Magnitude;
        
        fragment1.rigidBody.ApplyImpulse(impulse1, fragment1.rigidBody.position);
        fragment2.rigidBody.ApplyImpulse(impulse2, fragment2.rigidBody.position);
        
        Debug.Log($"Contrainte rompue! Énergie: {energy}, Impulsions: {impulse1Magnitude}, {impulse2Magnitude}");
    }
}

/// <summary>
/// Mur pré-fracturé avec contraintes
/// </summary>
public class FracturedWall : MonoBehaviour
{
    [Header("Configuration du mur")]
    public Vector3 wallPosition = new Vector3(0, 1, 10);
    public Vector3 wallSize = new Vector3(4, 3, 0.5f);
    public int fracturesX = 4;
    public int fracturesY = 3;
    
    [Header("Propriétés physiques")]
    public float fragmentMass = 0.5f;
    public float constraintStiffness = 500f;
    public float breakThreshold = 0.3f;
    
    [Header("Visuel")]
    public Color fragmentColor = new Color(0.8f, 0.6f, 0.4f);
    
    private List<FracturedFragment> fragments = new List<FracturedFragment>();
    private List<FragmentConstraint> constraints = new List<FragmentConstraint>();
    
    private bool isDestroyed = false;
    
    void Start()
    {
        CreateFracturedWall();
    }
    
    /// <summary>
    /// Crée le mur pré-fracturé avec tous ses fragments et contraintes
    /// </summary>
    void CreateFracturedWall()
    {
        Vector3 fragmentSize = new Vector3(
            wallSize.x / fracturesX,
            wallSize.y / fracturesY,
            wallSize.z
        );
        
        // Créer les fragments
        for (int y = 0; y < fracturesY; y++)
        {
            for (int x = 0; x < fracturesX; x++)
            {
                Vector3 fragmentPosition = wallPosition + new Vector3(
                    (x - fracturesX / 2f + 0.5f) * fragmentSize.x,
                    (y - fracturesY / 2f + 0.5f) * fragmentSize.y,
                    0
                );
                
                FracturedFragment fragment = new FracturedFragment(
                    fragmentPosition,
                    fragmentSize,
                    fragmentMass,
                    fragmentColor
                );
                
                fragments.Add(fragment);
            }
        }
        
        // Créer les contraintes entre fragments adjacents
        for (int y = 0; y < fracturesY; y++)
        {
            for (int x = 0; x < fracturesX; x++)
            {
                int index = y * fracturesX + x;
                
                // Contrainte horizontale (droite)
                if (x < fracturesX - 1)
                {
                    int rightIndex = index + 1;
                    FragmentConstraint constraint = new FragmentConstraint(
                        fragments[index],
                        fragments[rightIndex],
                        constraintStiffness,
                        breakThreshold
                    );
                    constraints.Add(constraint);
                }
                
                // Contrainte verticale (haut)
                if (y < fracturesY - 1)
                {
                    int topIndex = index + fracturesX;
                    FragmentConstraint constraint = new FragmentConstraint(
                        fragments[index],
                        fragments[topIndex],
                        constraintStiffness,
                        breakThreshold
                    );
                    constraints.Add(constraint);
                }
            }
        }
    }
    
    void FixedUpdate()
    {
        if (isDestroyed)
        {
            float dt = Time.fixedDeltaTime;
            Vector3 gravity = Vector3.down * 9.81f;
            
            // Appliquer les contraintes
            foreach (var constraint in constraints)
            {
                if (!constraint.isBroken)
                {
                    constraint.ApplyConstraintForce();
                    
                    // Vérifier si la contrainte doit se rompre
                    if (constraint.ShouldBreak())
                    {
                        constraint.Break();
                    }
                }
            }
            
            // Mettre à jour chaque fragment
            foreach (var fragment in fragments)
            {
                if (fragment.isActive)
                {
                    fragment.rigidBody.IntegrateRK4(dt, gravity, 0.05f);
                    
                    // Collision avec le sol
                    CollisionDetection.CollisionInfo collision = 
                        CollisionDetection.SphereToPlane(fragment.rigidBody.position, 0.2f, Vector3.zero, Vector3.up);
                    
                    if (collision.hasCollision)
                    {
                        CollisionDetection.ResolveCollision(fragment.rigidBody, collision, 0.3f, 0.5f);
                    }
                    
                    fragment.UpdateMesh();
                }
            }
        }
    }
    
    /// <summary>
    /// Détecte collision avec une sphère et déclenche la destruction
    /// </summary>
    public bool CheckCollisionWithSphere(Vector3 sphereCenter, float sphereRadius, Vector3 sphereVelocity)
    {
        if (isDestroyed) return false;
        
        // Vérifier collision avec la zone du mur
        Vector3 closestPoint = new Vector3(
            Mathf.Clamp(sphereCenter.x, wallPosition.x - wallSize.x / 2, wallPosition.x + wallSize.x / 2),
            Mathf.Clamp(sphereCenter.y, wallPosition.y - wallSize.y / 2, wallPosition.y + wallSize.y / 2),
            Mathf.Clamp(sphereCenter.z, wallPosition.z - wallSize.z / 2, wallPosition.z + wallSize.z / 2)
        );
        
        float distance = Vector3.Distance(sphereCenter, closestPoint);
        
        if (distance < sphereRadius)
        {
            // Collision détectée!
            TriggerDestruction(sphereCenter, sphereVelocity);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Déclenche la destruction du mur
    /// </summary>
    void TriggerDestruction(Vector3 impactPoint, Vector3 impactVelocity)
    {
        isDestroyed = true;
        
        // Appliquer une impulsion initiale aux fragments proches de l'impact
        foreach (var fragment in fragments)
        {
            float distanceToImpact = Vector3.Distance(fragment.rigidBody.position, impactPoint);
            
            // Plus le fragment est proche, plus l'impulsion est forte
            float impulseFactor = Mathf.Clamp01(1f - distanceToImpact / wallSize.magnitude);
            Vector3 impulse = impactVelocity * impulseFactor * 0.5f;
            
            fragment.rigidBody.ApplyImpulse(impulse, fragment.rigidBody.position);
            
            // Ajouter une vélocité angulaire aléatoire
            fragment.rigidBody.angularMomentum = Random.insideUnitSphere * impulseFactor * 2f;
        }
        
        Debug.Log("Mur détruit!");
    }
    
    void OnDestroy()
    {
        foreach (var fragment in fragments)
        {
            fragment.Destroy();
        }
    }
    
    public bool IsDestroyed()
    {
        return isDestroyed;
    }
}
