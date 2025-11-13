using UnityEngine;
using System.Collections.Generic;

public class EggFraction : MonoBehaviour
{
    [Header("Global Physics")]
    [Tooltip("Gravity strength (downwards).")]
    public float gravity = 9.81f;

    [Tooltip("Linear damping coefficient (like your alpha).")]
    public float alpha = 0.4f;

    [Tooltip("Bounciness on collisions (0 = inelastic, 1 = bouncy).")]
    [Range(0f, 1f)]
    public float restitution = 0.15f;

    [Tooltip("Use FixedDeltaTime or a custom dt.")]
    public bool useFixedDeltaTime = true;

    [Tooltip("Custom dt if not using FixedDeltaTime.")]
    public float customDt = 0.002f;

    [Header("Main Egg")]
    public Vector3 mainStartPos = new Vector3(0f, 5f, 0f);
    public float eggHeight = 1.2f;
    public float eggWidth = 0.8f;
    public float mainMass = 1f;
    public Color eggShellColor = new Color(0.95f, 0.9f, 0.85f); // Off-white

    [Header("Egg Yolk (Inner Sphere)")]
    public bool spawnYolk = true;
    public Color yolkColor = new Color(1f, 0.8f, 0.2f); // Yellow-orange
    public float yolkRadius = 0.25f;

    [Header("Shell Fragments")]
    [Tooltip("Number of shell fragment pieces")]
    [Range(10, 100)]
    public int fragmentCount = 30;

    [Tooltip("Thickness of shell fragments")]
    public float shellThickness = 0.05f;

    [Tooltip("Density for fragments (mass = density * volume).")]
    public float fragmentDensity = 0.5f;

    [Tooltip("Total energy released at fracture (E = 1/2 k x^2).")]
    public float storedEnergy = 15f;

    [Tooltip("Multiplier for explosion speed.")]
    public float impulseMultiplier = 0.8f;

    public Color fragmentColor = new Color(0.95f, 0.9f, 0.85f);

    [Header("Ground")]
    public float groundY = 0f;
    public bool spawnGroundPlane = true;
    public Vector2 groundSize = new Vector2(50f, 50f);
    public Color groundColor = new Color(0.7f, 0.7f, 0.7f);

    // ---------- internal simulation struct ----------
    class SimObject
    {
        public GameObject go;
        public Vector3 pos;
        public Vector3 vel;
        public Vector3 angularVel;
        public Quaternion rotation;
        public Vector3 size;
        public float radius;
        public float mass;
        public bool isSphere;

        public SimObject(GameObject go, Vector3 pos, Vector3 size, float mass, bool isSphere = false)
        {
            this.go = go;
            this.pos = pos;
            this.size = size;
            this.mass = mass;
            this.isSphere = isSphere;
            this.radius = isSphere ? size.x * 0.5f : 0f;
            this.vel = Vector3.zero;
            this.angularVel = Vector3.zero;
            this.rotation = Quaternion.identity;
        }
    }

    SimObject mainEgg;
    SimObject yolk;
    List<SimObject> shards = new List<SimObject>();
    bool fractured = false;

    Mesh eggMesh;
    Mesh planeMesh;

    // =========================================================
    // Unity lifecycle
    // =========================================================
    void Start()
    {
        if (spawnGroundPlane)
            CreateGround();

        GameObject mainGO = CreateEggVisual("MainEgg", eggShellColor);
        mainEgg = new SimObject(mainGO, mainStartPos, new Vector3(eggWidth, eggHeight, eggWidth), mainMass);
        ApplyTransform(mainEgg);
    }

    void FixedUpdate()
    {
        float dt = useFixedDeltaTime ? Time.fixedDeltaTime : customDt;
        if (dt <= 0f) return;

        if (!fractured)
        {
            SyncFromScene(mainEgg);
            StepMainEgg(dt);
        }
        else
        {
            if (yolk != null)
                SyncFromScene(yolk);

            for (int i = 0; i < shards.Count; i++)
                SyncFromScene(shards[i]);

            StepShards(dt);
        }
    }

    // =========================================================
    // Sync when user moves objects in Scene view
    // =========================================================
    void SyncFromScene(SimObject obj)
    {
        if (obj == null || obj.go == null) return;

        Vector3 scenePos = obj.go.transform.position;
        if ((scenePos - obj.pos).sqrMagnitude > 1e-6f)
        {
            obj.pos = scenePos;
            obj.vel = Vector3.zero;
        }
    }

    // =========================================================
    // Main egg motion + impact detection
    // =========================================================
    void StepMainEgg(float dt)
    {
        if (mainEgg == null) return;

        Vector3 acc = new Vector3(0f, -gravity, 0f) - alpha * mainEgg.vel;
        mainEgg.vel += acc * dt;
        mainEgg.pos += mainEgg.vel * dt;

        // Add slight rotation as it falls
        mainEgg.angularVel += new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f)) * dt;
        mainEgg.rotation *= Quaternion.Euler(mainEgg.angularVel * dt * Mathf.Rad2Deg);

        float bottomY = mainEgg.pos.y - eggHeight * 0.5f;
        if (bottomY <= groundY)
        {
            mainEgg.pos.y = groundY + eggHeight * 0.5f;
            ApplyTransform(mainEgg);
            Fracture();
            return;
        }

        ApplyTransform(mainEgg);
    }

    // =========================================================
    // Fracture into shell pieces + yolk
    // =========================================================
    void Fracture()
    {
        if (fractured || mainEgg == null) return;
        fractured = true;

        Vector3 center = mainEgg.pos;
        Vector3 baseVel = mainEgg.vel;

        Destroy(mainEgg.go);
        mainEgg = null;

        // Create yolk (inner sphere)
        if (spawnYolk)
        {
            GameObject yolkGO = CreateSphereVisual("Yolk", yolkColor);
            float yolkMass = (4f / 3f) * Mathf.PI * yolkRadius * yolkRadius * yolkRadius * fragmentDensity;
            yolk = new SimObject(yolkGO, center, Vector3.one * yolkRadius * 2f, yolkMass, true);
            yolk.vel = baseVel * 0.5f; // Less initial velocity
            ApplyTransform(yolk);
        }

        // Create shell fragments
        int total = Mathf.Max(10, fragmentCount);
        float energyPerFrag = (total > 0) ? storedEnergy / total : 0f;

        shards.Clear();

        for (int i = 0; i < total; i++)
        {
            // Distribute fragments on egg surface
            float theta = Random.Range(0f, Mathf.PI * 2f);
            float phi = Random.Range(0f, Mathf.PI);

            // Egg-shaped distribution (ellipsoid)
            float x = eggWidth * 0.5f * Mathf.Sin(phi) * Mathf.Cos(theta);
            float y = eggHeight * 0.5f * Mathf.Cos(phi);
            float z = eggWidth * 0.5f * Mathf.Sin(phi) * Mathf.Sin(theta);

            Vector3 surfacePoint = new Vector3(x, y, z);
            Vector3 worldPos = center + surfacePoint;

            // Fragment size (small irregular pieces with RANDOM dimensions)
            Vector3 fragSize = new Vector3(
                Random.Range(0.08f, 0.25f),  // Random width
                Random.Range(0.08f, 0.25f),  // Random height
                Random.Range(0.02f, 0.08f)   // Random thickness (shell-like)
            );

            float volume = fragSize.x * fragSize.y * fragSize.z;
            float mass = Mathf.Max(fragmentDensity * volume, 1e-6f);

            // Random shape type for variety
            int shapeType = Random.Range(0, 3);
            GameObject go = CreateShellFragmentVisual("ShellShard", fragmentColor, fragSize, shapeType);
            SimObject shard = new SimObject(go, worldPos, fragSize, mass);

            // Inherit some velocity
            shard.vel = baseVel * 0.3f;

            // Explosion direction (outward from center)
            Vector3 radial = surfacePoint.normalized;
            if (radial == Vector3.zero) radial = Vector3.up;

            Vector3 randomDir = Random.onUnitSphere;
            Vector3 dir = (radial * 2f + randomDir).normalized;

            float v = 0f;
            if (energyPerFrag > 0f && mass > 0f)
                v = Mathf.Sqrt(2f * energyPerFrag / mass);

            v *= impulseMultiplier;
            v *= Random.Range(0.5f, 1.2f);

            shard.vel += dir * v;

            // Add random spin to fragments
            shard.angularVel = Random.onUnitSphere * Random.Range(2f, 8f);
            shard.rotation = Random.rotation;

            ApplyTransform(shard);
            shards.Add(shard);
        }
    }

    // =========================================================
    // Shards + yolk simulation
    // =========================================================
    void StepShards(float dt)
    {
        Vector3 g = new Vector3(0f, -gravity, 0f);

        // Update yolk
        if (yolk != null && yolk.go != null)
        {
            Vector3 acc = g - alpha * yolk.vel;
            yolk.vel += acc * dt;
            yolk.pos += yolk.vel * dt;

            if (yolk.pos.y - yolk.radius <= groundY)
            {
                yolk.pos.y = groundY + yolk.radius;
                if (yolk.vel.y < 0f)
                    yolk.vel.y = -yolk.vel.y * restitution * 0.5f; // Less bouncy
            }
            ApplyTransform(yolk);
        }

        // Update shell fragments
        for (int i = 0; i < shards.Count; i++)
        {
            SimObject s = shards[i];
            if (s == null || s.go == null) continue;

            Vector3 acc = g - alpha * s.vel;
            s.vel += acc * dt;
            s.pos += s.vel * dt;

            // Update rotation
            s.rotation *= Quaternion.Euler(s.angularVel * dt * Mathf.Rad2Deg);
            s.angularVel *= 0.99f; // Damping

            float halfY = s.size.y * 0.5f;
            if (s.pos.y - halfY <= groundY)
            {
                s.pos.y = groundY + halfY;
                if (s.vel.y < 0f)
                {
                    s.vel.y = -s.vel.y * restitution;
                    s.angularVel *= 0.8f; // Reduce spin on bounce
                }
            }
        }

        // Simple collision between fragments
        ResolveShardCollisions();

        for (int i = 0; i < shards.Count; i++)
            ApplyTransform(shards[i]);
    }

    void ResolveShardCollisions()
    {
        int count = shards.Count;
        for (int i = 0; i < count; i++)
        {
            SimObject A = shards[i];
            if (A == null || A.go == null) continue;

            for (int j = i + 1; j < count; j++)
            {
                SimObject B = shards[j];
                if (B == null || B.go == null) continue;

                Vector3 delta = B.pos - A.pos;
                float dist = delta.magnitude;
                float minDist = (A.size.magnitude + B.size.magnitude) * 0.3f;

                if (dist < minDist && dist > 0.0001f)
                {
                    Vector3 normal = delta / dist;
                    float overlap = minDist - dist;

                    A.pos -= normal * overlap * 0.5f;
                    B.pos += normal * overlap * 0.5f;

                    float vA = Vector3.Dot(A.vel, normal);
                    float vB = Vector3.Dot(B.vel, normal);
                    float vRel = vB - vA;

                    if (vRel < 0f)
                    {
                        float impulse = -vRel * restitution * 0.5f;
                        A.vel -= normal * impulse;
                        B.vel += normal * impulse;
                    }
                }
            }
        }
    }

    // =========================================================
    // Visuals
    // =========================================================
    void ApplyTransform(SimObject obj)
    {
        if (obj == null || obj.go == null) return;
        obj.go.transform.position = obj.pos;
        obj.go.transform.rotation = obj.rotation;
        obj.go.transform.localScale = obj.size;
    }

    GameObject CreateEggVisual(string name, Color color)
    {
        GameObject g = new GameObject(name);
        MeshFilter mf = g.AddComponent<MeshFilter>();
        MeshRenderer mr = g.AddComponent<MeshRenderer>();

        mf.sharedMesh = GetEggMesh();

        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");

        Material mat = new Material(sh);
        mat.color = color;
        mat.SetFloat("_Smoothness", 0.6f);
        mr.material = mat;

        return g;
    }

    GameObject CreateShellFragmentVisual(string name, Color color, Vector3 fragSize, int shapeType)
    {
        GameObject g;

        // Create different primitive shapes for variety
        switch (shapeType)
        {
            case 0: // Cube (rectangular piece)
                g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case 1: // Sphere (rounded chunk)
                g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            case 2: // Capsule (elongated piece)
                g = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                break;
            default:
                g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
        }

        g.name = name;

        Collider col = g.GetComponent<Collider>();
        if (col != null) Destroy(col);

        MeshRenderer mr = g.GetComponent<MeshRenderer>();

        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");

        Material mat = new Material(sh);

        // Slightly vary the color for each fragment
        float colorVariation = Random.Range(-0.05f, 0.05f);
        mat.color = new Color(
            Mathf.Clamp01(color.r + colorVariation),
            Mathf.Clamp01(color.g + colorVariation),
            Mathf.Clamp01(color.b + colorVariation),
            color.a
        );

        mr.material = mat;

        return g;
    }

    GameObject CreateSphereVisual(string name, Color color)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        g.name = name;

        Collider col = g.GetComponent<Collider>();
        if (col != null) Destroy(col);

        MeshRenderer mr = g.GetComponent<MeshRenderer>();

        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");

        Material mat = new Material(sh);
        mat.color = color;
        mat.SetFloat("_Smoothness", 0.3f);
        mr.material = mat;

        return g;
    }

    Mesh GetEggMesh()
    {
        if (eggMesh != null) return eggMesh;

        Mesh m = new Mesh();
        m.name = "EggMesh";

        // Create egg shape using UV sphere with modified y-coordinates
        int segments = 16;
        int rings = 16;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Generate vertices
        for (int ring = 0; ring <= rings; ring++)
        {
            float v = (float)ring / rings;
            float phi = v * Mathf.PI;

            for (int seg = 0; seg <= segments; seg++)
            {
                float u = (float)seg / segments;
                float theta = u * Mathf.PI * 2f;

                // Egg shape: wider at bottom, pointier at top
                float eggFactor = 1f - 0.3f * Mathf.Pow(v, 2f);

                float x = 0.5f * eggFactor * Mathf.Sin(phi) * Mathf.Cos(theta);
                float y = 0.5f * (Mathf.Cos(phi) - 0.2f * Mathf.Pow(Mathf.Cos(phi), 3f));
                float z = 0.5f * eggFactor * Mathf.Sin(phi) * Mathf.Sin(theta);

                vertices.Add(new Vector3(x, y, z));
            }
        }

        // Generate triangles
        for (int ring = 0; ring < rings; ring++)
        {
            for (int seg = 0; seg < segments; seg++)
            {
                int current = ring * (segments + 1) + seg;
                int next = current + segments + 1;

                triangles.Add(current);
                triangles.Add(next);
                triangles.Add(current + 1);

                triangles.Add(current + 1);
                triangles.Add(next);
                triangles.Add(next + 1);
            }
        }

        m.vertices = vertices.ToArray();
        m.triangles = triangles.ToArray();
        m.RecalculateNormals();

        eggMesh = m;
        return eggMesh;
    }

    void CreateGround()
    {
        GameObject g = new GameObject("Ground");
        MeshFilter mf = g.AddComponent<MeshFilter>();
        MeshRenderer mr = g.AddComponent<MeshRenderer>();

        mf.sharedMesh = GetPlaneMesh();

        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        Material mat = new Material(sh);
        mat.color = groundColor;
        mr.material = mat;

        g.transform.position = new Vector3(0f, groundY, 0f);
        g.transform.localScale = new Vector3(groundSize.x, 1f, groundSize.y);
    }

    Mesh GetPlaneMesh()
    {
        if (planeMesh != null) return planeMesh;

        Mesh m = new Mesh();
        m.name = "ProceduralPlane";

        Vector3[] verts = {
            new Vector3(-0.5f, 0f, -0.5f),
            new Vector3( 0.5f, 0f, -0.5f),
            new Vector3( 0.5f, 0f,  0.5f),
            new Vector3(-0.5f, 0f,  0.5f)
        };

        int[] tris = { 0, 2, 1, 0, 3, 2 };

        Vector3[] normals = {
            Vector3.up, Vector3.up, Vector3.up, Vector3.up
        };

        Vector2[] uv = {
            new Vector2(0,0),
            new Vector2(1,0),
            new Vector2(1,1),
            new Vector2(0,1)
        };

        m.vertices = verts;
        m.triangles = tris;
        m.normals = normals;
        m.uv = uv;

        planeMesh = m;
        return planeMesh;
    }
}