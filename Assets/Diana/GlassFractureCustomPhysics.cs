using UnityEngine;
using System.Collections.Generic;

public class GlassFractureCustomPhysics : MonoBehaviour
{
    [Header("Global Physics")]
    public float gravity = 9.81f;
    [Range(0f, 1f)]
    public float restitution = 0.2f;
    public bool useFixedDeltaTime = true;
    public float customDt = 0.002f;

    [Header("Main Cube")]
    public Vector3 mainStartPos = new Vector3(0f, 5f, 0f);
    public Vector3 mainSize = Vector3.one;
    public float mainMass = 1f;
    public Color mainColor = Color.white;

    [Header("Fragments")]
    [Range(2, 10)]
    public int fragmentsPerAxis = 3;
    public float fragmentDensity = 1f;
    public float storedEnergy = 20f;
    public float impulseMultiplier = 1f;
    public Color fragmentColor = Color.red;

    [Header("Ground")]
    public float groundY = 0f;
    public bool spawnGroundPlane = true;
    public Vector2 groundSize = new Vector2(50f, 50f);
    public Color groundColor = new Color(0.7f, 0.7f, 0.7f);

    class SimCube
    {
        public GameObject go;
        public Vector3 pos;
        public Vector3 vel;
        public Vector3 size;
        public float mass;

        // New rotation properties
        public Vector3 rotationAngles;   // Euler angles in degrees
        public Vector3 angularVelocity;  // radians/sec

        public SimCube(GameObject go, Vector3 pos, Vector3 size, float mass)
        {
            this.go = go;
            this.pos = pos;
            this.size = size;
            this.mass = mass;
            this.vel = Vector3.zero;
            this.rotationAngles = Vector3.zero;
            this.angularVelocity = Vector3.zero;
        }
    }

    SimCube mainCube;
    List<SimCube> shards = new List<SimCube>();
    bool fractured = false;

    Mesh cubeMesh;
    Mesh planeMesh;

    void Start()
    {
        if (spawnGroundPlane) CreateGround();

        GameObject mainGO = CreateCubeVisual("MainCube", mainColor);
        mainCube = new SimCube(mainGO, mainStartPos, mainSize, mainMass);
        ApplyTransform(mainCube);
    }

    void FixedUpdate()
    {
        float dt = useFixedDeltaTime ? Time.fixedDeltaTime : customDt;
        if (dt <= 0f) return;

        if (!fractured)
        {
            SyncFromScene(mainCube);
            StepMainCube(dt);
        }
        else
        {
            for (int i = 0; i < shards.Count; i++)
                SyncFromScene(shards[i]);

            StepShards(dt);
        }
    }

    void SyncFromScene(SimCube c)
    {
        if (c == null || c.go == null) return;
        Vector3 scenePos = c.go.transform.position;
        if ((scenePos - c.pos).sqrMagnitude > 1e-6f)
        {
            c.pos = scenePos;
            c.vel = Vector3.zero;
        }
    }

    void StepMainCube(float dt)
    {
        if (mainCube == null) return;

        Vector3 acc = new Vector3(0f, -gravity, 0f) - 0.4f * mainCube.vel; // alpha=0.4
        mainCube.vel += acc * dt;
        mainCube.pos += mainCube.vel * dt;

        float halfY = mainCube.size.y * 0.5f;
        if (mainCube.pos.y - halfY <= groundY)
        {
            mainCube.pos.y = groundY + halfY;
            ApplyTransform(mainCube);
            Fracture();
            return;
        }

        ApplyTransform(mainCube);
    }

    void Fracture()
    {
        if (fractured || mainCube == null) return;
        fractured = true;

        Vector3 center = mainCube.pos;
        Vector3 baseVel = mainCube.vel;

        Destroy(mainCube.go);
        mainCube = null;

        int n = Mathf.Max(2, fragmentsPerAxis);
        int total = n * n * n;
        float energyPerFrag = (total > 0) ? storedEnergy / total : 0f;

        Vector3 full = mainSize;
        shards.Clear();

        for (int ix = 0; ix < n; ix++)
            for (int iy = 0; iy < n; iy++)
                for (int iz = 0; iz < n; iz++)
                {
                    Vector3 fragSize = new Vector3(full.x / n, full.y / n, full.z / n);
                    Vector3 localOffset = new Vector3(
                        -full.x * 0.5f + fragSize.x * 0.5f + ix * fragSize.x,
                        -full.y * 0.5f + fragSize.y * 0.5f + iy * fragSize.y,
                        -full.z * 0.5f + fragSize.z * 0.5f + iz * fragSize.z
                    );

                    Vector3 jitter = Random.insideUnitSphere * fragSize.x * 0.05f;
                    Vector3 worldPos = center + localOffset + jitter;

                    float volume = fragSize.x * fragSize.y * fragSize.z;
                    float mass = Mathf.Max(fragmentDensity * volume, 1e-6f);

                    GameObject go = CreateCubeVisual("Shard", fragmentColor);
                    SimCube shard = new SimCube(go, worldPos, fragSize, mass);

                    shard.vel = baseVel;

                    // explosion + random direction
                    Vector3 radial = (worldPos - center).normalized;
                    if (radial == Vector3.zero) radial = Vector3.up;

                    Vector3 randomDir = Random.onUnitSphere;
                    Vector3 dir = (radial + 0.8f * randomDir).normalized;

                    float v = 0f;
                    if (energyPerFrag > 0f && mass > 0f)
                        v = Mathf.Sqrt(2f * energyPerFrag / mass);

                    v *= impulseMultiplier;
                    v *= Random.Range(0.7f, 1.3f);

                    shard.vel += dir * v;

                    // --- NEW: assign random initial rotation ---
                    shard.rotationAngles = new Vector3(
                        Random.Range(0f, 360f),
                        Random.Range(0f, 360f),
                        Random.Range(0f, 360f)
                    );

                    // --- NEW: assign random angular velocity (radians/sec) ---
                    shard.angularVelocity = Random.insideUnitSphere * Mathf.Deg2Rad * 180f; // max 180 deg/sec

                    ApplyTransform(shard);
                    shards.Add(shard);
                }
    }

    void StepShards(float dt)
    {
        Vector3 g = new Vector3(0f, -gravity, 0f);

        for (int i = 0; i < shards.Count; i++)
        {
            SimCube s = shards[i];
            if (s == null || s.go == null) continue;

            Vector3 acc = g - 0.4f * s.vel;
            s.vel += acc * dt;
            s.pos += s.vel * dt;

            // --- Apply rotation ---
            s.rotationAngles += s.angularVelocity * Mathf.Rad2Deg * dt;
            s.rotationAngles.x %= 360f;
            s.rotationAngles.y %= 360f;
            s.rotationAngles.z %= 360f;

            s.angularVelocity *= 0.95f; // angular damping

            float halfY = s.size.y * 0.5f;
            if (s.pos.y - halfY <= groundY)
            {
                s.pos.y = groundY + halfY;
                if (s.vel.y < 0f) s.vel.y = -s.vel.y * restitution;
            }
        }

        ResolveShardCollisions();

        for (int i = 0; i < shards.Count; i++)
            ApplyTransform(shards[i]);
    }

    void ResolveShardCollisions()
    {
        int count = shards.Count;
        for (int i = 0; i < count; i++)
        {
            SimCube A = shards[i];
            if (A == null || A.go == null) continue;
            Vector3 halfA = A.size * 0.5f;

            for (int j = i + 1; j < count; j++)
            {
                SimCube B = shards[j];
                if (B == null || B.go == null) continue;
                Vector3 halfB = B.size * 0.5f;

                Vector3 delta = B.pos - A.pos;

                float overlapX = halfA.x + halfB.x - Mathf.Abs(delta.x);
                if (overlapX <= 0f) continue;

                float overlapY = halfA.y + halfB.y - Mathf.Abs(delta.y);
                if (overlapY <= 0f) continue;

                float overlapZ = halfA.z + halfB.z - Mathf.Abs(delta.z);
                if (overlapZ <= 0f) continue;

                float minOverlap = overlapX;
                int axis = 0;
                if (overlapY < minOverlap) { minOverlap = overlapY; axis = 1; }
                if (overlapZ < minOverlap) { minOverlap = overlapZ; axis = 2; }

                Vector3 axisDir;
                float sign;
                if (axis == 0) { axisDir = Vector3.right; sign = Mathf.Sign(delta.x); }
                else if (axis == 1) { axisDir = Vector3.up; sign = Mathf.Sign(delta.y); }
                else { axisDir = Vector3.forward; sign = Mathf.Sign(delta.z); }
                if (sign == 0f) sign = 1f;

                float correction = minOverlap * 0.5f;
                Vector3 corrDir = axisDir * sign;

                A.pos -= corrDir * correction;
                B.pos += corrDir * correction;

                float vA = Vector3.Dot(A.vel, axisDir);
                float vB = Vector3.Dot(B.vel, axisDir);
                float vRel = vB - vA;

                if (vRel < 0f)
                {
                    float vCM = 0.5f * (vA + vB);
                    float vAnew = vCM + 0.5f * restitution * vRel;
                    float vBnew = vCM - 0.5f * restitution * vRel;

                    A.vel += axisDir * (vAnew - vA);
                    B.vel += axisDir * (vBnew - vB);
                }
            }
        }
    }

    void ApplyTransform(SimCube c)
    {
        if (c == null || c.go == null) return;
        c.go.transform.position = c.pos;
        c.go.transform.localScale = c.size;
        c.go.transform.rotation = Quaternion.Euler(c.rotationAngles);
    }

    GameObject CreateCubeVisual(string name, Color color)
    {
        GameObject g = new GameObject(name);

        MeshFilter mf = g.AddComponent<MeshFilter>();
        MeshRenderer mr = g.AddComponent<MeshRenderer>();
        mf.sharedMesh = GetCubeMesh();

        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        Material mat = new Material(sh);
        mat.color = color;
        mr.material = mat;

        return g;
    }

    Mesh GetCubeMesh()
    {
        if (cubeMesh != null) return cubeMesh;

        Mesh m = new Mesh();
        m.name = "ProceduralCube";

        Vector3[] v = {
            new Vector3(-.5f,-.5f,-.5f), new Vector3(.5f,-.5f,-.5f),
            new Vector3(.5f,.5f,-.5f),  new Vector3(-.5f,.5f,-.5f),
            new Vector3(-.5f,-.5f,.5f), new Vector3(.5f,-.5f,.5f),
            new Vector3(.5f,.5f,.5f),  new Vector3(-.5f,.5f,.5f)
        };

        int[] t = { 0, 2, 1, 0, 3, 2, 4, 5, 6, 4, 6, 7, 0, 1, 5, 0, 5, 4, 2, 3, 7, 2, 7, 6, 1, 2, 6, 1, 6, 5, 0, 4, 7, 0, 7, 3 };
        m.vertices = v;
        m.triangles = t;
        m.RecalculateNormals();
        cubeMesh = m;
        return m;
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

        Vector3[] verts = { new Vector3(-0.5f, 0f, -0.5f), new Vector3(0.5f, 0f, -0.5f), new Vector3(0.5f, 0f, 0.5f), new Vector3(-0.5f, 0f, 0.5f) };
        int[] tris = { 0, 2, 1, 0, 3, 2 };
        Vector3[] normals = { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
        Vector2[] uv = { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };

        m.vertices = verts;
        m.triangles = tris;
        m.normals = normals;
        m.uv = uv;

        planeMesh = m;
        return m;
    }
}
