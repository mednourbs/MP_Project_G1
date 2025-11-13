using UnityEngine;

/// <summary>
/// Contrôleur principal de la simulation
/// Gère la sphère qui roule sur la rampe et détruit le mur
/// </summary>
public class PhysicsSimulationController : MonoBehaviour
{
    [Header("Références")]
    public PhysicsSphere sphere;
    public SkateRamp ramp;
    public FracturedWall wall;
    
    [Header("Configuration de la simulation")]
    [Range(0.1f, 5f)]
    public float sphereRadius = 0.5f;
    [Range(0.5f, 10f)]
    public float sphereMass = 2f;
    [Range(0f, 1f)]
    public float sphereAirResistance = 0.1f;
    [Range(0f, 1f)]
    public float sphereFriction = 0.3f;
    [Range(0f, 1f)]
    public float sphereRestitution = 0.5f;
    
    [Header("Configuration de la rampe")]
    public Vector3 rampPosition = new Vector3(0, 0.5f, 0);
    [Range(1f, 10f)]
    public float rampHeight = 3f;
    [Range(5f, 30f)]
    public float rampLength = 15f;
    [Range(2f, 15f)]
    public float rampWidth = 8f;
    
    [Header("Gravité")]
    [Range(1f, 20f)]
    public float gravity = 9.81f;
    
    [Header("Contrôle")]
    public bool restartSimulation = false;
    
    private float lastMass;
    private float lastAirResistance;
    private float lastFriction;
    private float lastRestitution;
    private float lastGravity;
    private float lastRampHeight;
    private float lastRampLength;
    
    [Header("Configuration du mur")]
    public Vector3 wallPosition = new Vector3(5, 1.5f, 0);
    public Vector3 wallSize = new Vector3(3, 3, 0.5f);
    public int wallFracturesX = 4;
    public int wallFracturesY = 3;
    public bool enableWall = false; // Désactiver le mur
    
    [Header("Sol")]
    public Vector3 groundNormal = Vector3.up;
    public float groundHeight = 0f;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private bool wallDestroyed = false;
    
    void Start()
    {
        InitializeSimulation();
        SaveCurrentParameters();
    }
    
    void Update()
    {
        // Vérifier si les paramètres ont changé ou si restart est demandé
        if (restartSimulation || ParametersChanged())
        {
            RestartSimulation();
            restartSimulation = false;
        }
    }
    
    /// <summary>
    /// Initialise tous les éléments de la simulation
    /// </summary>
    void InitializeSimulation()
    {
        // Créer la rampe d'abord
        GameObject rampObj = new GameObject("SkateRamp");
        ramp = rampObj.AddComponent<SkateRamp>();
        ramp.position = rampPosition;
        ramp.height = rampHeight;
        ramp.length = rampLength;
        ramp.width = rampWidth;
        ramp.segments = 50;
        ramp.rampColor = new Color(1f, 0.7f, 0.2f);
        
        // Position de départ de la sphère = haut de la rampe
        Vector3 actualStartPos = rampPosition + new Vector3(0, rampHeight, 0);
        
        // Créer la sphère
        GameObject sphereObj = new GameObject("PhysicsSphere");
        sphere = sphereObj.AddComponent<PhysicsSphere>();
        sphere.radius = sphereRadius;
        sphere.mass = sphereMass;
        sphere.initialPosition = actualStartPos;
        sphere.sphereColor = new Color(0.2f, 0.5f, 1f);
        sphere.gravity = gravity;
        sphere.rollingFriction = sphereFriction;
        sphere.restitution = sphereRestitution;
        sphere.airResistance = sphereAirResistance;
        
        // Créer le mur seulement si activé
        if (enableWall)
        {
            GameObject wallObj = new GameObject("FracturedWall");
            wall = wallObj.AddComponent<FracturedWall>();
            wall.wallPosition = wallPosition;
            wall.wallSize = wallSize;
            wall.fracturesX = wallFracturesX;
            wall.fracturesY = wallFracturesY;
            wall.fragmentMass = 0.3f;
            wall.constraintStiffness = 800f;
            wall.breakThreshold = 0.25f;
            wall.fragmentColor = new Color(0.8f, 0.5f, 0.3f);
        }
        
        // Créer un sol visuel (optionnel)
        CreateGround();
        
        Debug.Log("Simulation initialisée!");
        Debug.Log($"Sphère démarre au sommet de la rampe: {actualStartPos}");
        Debug.Log($"Masse: {sphereMass} kg, Résistance air: {sphereAirResistance}");
        Debug.Log("La sphère va glisser sur la rampe avec physique réaliste");
    }
    
    void SaveCurrentParameters()
    {
        lastMass = sphereMass;
        lastAirResistance = sphereAirResistance;
        lastFriction = sphereFriction;
        lastRestitution = sphereRestitution;
        lastGravity = gravity;
        lastRampHeight = rampHeight;
        lastRampLength = rampLength;
    }
    
    bool ParametersChanged()
    {
        return lastMass != sphereMass ||
               lastAirResistance != sphereAirResistance ||
               lastFriction != sphereFriction ||
               lastRestitution != sphereRestitution ||
               lastGravity != gravity ||
               lastRampHeight != rampHeight ||
               lastRampLength != rampLength;
    }
    
    void RestartSimulation()
    {
        // Détruire les objets existants
        if (sphere != null) Destroy(sphere.gameObject);
        if (ramp != null) Destroy(ramp.gameObject);
        if (wall != null) Destroy(wall.gameObject);
        
        // Réinitialiser
        InitializeSimulation();
        SaveCurrentParameters();
        
        Debug.Log("=== SIMULATION REDÉMARRÉE ===");
    }
    
    void FixedUpdate()
    {
        if (sphere == null) return;
        
        CustomRigidBody sphereRB = sphere.GetRigidBody();
        if (sphereRB == null) return;
        
        // 1. Collision avec le sol
        Vector3 groundPoint = new Vector3(0, groundHeight, 0);
        sphere.CheckCollisionWithPlane(groundPoint, groundNormal);
        
        // 2. Collision avec la rampe
        if (ramp != null)
        {
            CollisionDetection.CollisionInfo rampCollision = 
                ramp.CheckSphereCollision(sphereRB.position, sphere.radius);
            
            if (rampCollision.hasCollision)
            {
                CollisionDetection.ResolveCollision(sphereRB, rampCollision, 
                    sphere.restitution, sphere.rollingFriction);
            }
        }
        
        // Debug info
        if (showDebugInfo && Time.frameCount % 30 == 0)
        {
            Debug.Log($"Sphère - Position: {sphereRB.position}, Vitesse: {sphereRB.velocity.magnitude:F2} m/s");
        }
    }
    
    /// <summary>
    /// Crée un sol visuel
    /// </summary>
    void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0, groundHeight, 0);
        ground.transform.localScale = new Vector3(5, 1, 5);
        
        Material groundMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        groundMat.color = new Color(0.3f, 0.5f, 0.3f);
        ground.GetComponent<Renderer>().material = groundMat;
        
        // Supprimer le collider par défaut (on n'utilise pas les colliders Unity)
        Collider collider = ground.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
        // Visualiser la sphère
        if (sphere != null)
        {
            CustomRigidBody rb = sphere.GetRigidBody();
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(rb.position, sphere.radius);
            
            // Vélocité
            Gizmos.color = Color.red;
            Gizmos.DrawLine(rb.position, rb.position + rb.velocity);
        }
        
        // Visualiser le sol
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(-10, groundHeight, 0), new Vector3(10, groundHeight, 0));
        
        // Visualiser la zone du mur
        if (wall != null && !wallDestroyed)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(wallPosition, wallSize);
        }
    }
}
