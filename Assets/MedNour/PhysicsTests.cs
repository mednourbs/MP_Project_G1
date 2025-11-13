using UnityEngine;

/// <summary>
/// Tests unitaires pour vérifier les calculs physiques
/// Attacher à un GameObject et lancer pour voir les résultats dans la Console
/// </summary>
public class PhysicsTests : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== TESTS DES CALCULS PHYSIQUES ===");
        
        TestRigidBodyCreation();
        TestMatrixOperations();
        TestCollisionDetection();
        TestConstraintPhysics();
        
        Debug.Log("=== TOUS LES TESTS TERMINÉS ===");
    }
    
    void TestRigidBodyCreation()
    {
        Debug.Log("\n--- Test 1: Création de Corps Rigide ---");
        
        // Créer une sphère
        CustomRigidBody sphere = new CustomRigidBody(Vector3.zero, 1f, 2f);
        
        // Vérifier le tenseur d'inertie: I = (2/5) * m * r²
        float expectedI = (2f / 5f) * 2f * 1f * 1f; // = 0.8
        float actualI = sphere.inertiaTensor[0, 0];
        
        Debug.Log($"Tenseur d'inertie sphère: {actualI} (attendu: {expectedI})");
        Assert(Mathf.Approximately(actualI, expectedI), "Tenseur d'inertie sphère");
        
        // Créer une box
        CustomRigidBody box = new CustomRigidBody(Vector3.zero, new Vector3(2, 2, 2), 1f);
        float expectedIxx = (1f / 12f) * 1f * (2f * 2f + 2f * 2f); // = 0.666...
        float actualIxx = box.inertiaTensor[0, 0];
        
        Debug.Log($"Tenseur d'inertie box: {actualIxx} (attendu: {expectedIxx})");
        Assert(Mathf.Approximately(actualIxx, expectedIxx), "Tenseur d'inertie box");
    }
    
    void TestMatrixOperations()
    {
        Debug.Log("\n--- Test 2: Opérations Matricielles ---");
        
        // Test multiplication matrice-vecteur
        Matrix4x4 rotation = Matrix4x4.Rotate(Quaternion.Euler(0, 90, 0));
        Vector3 v = new Vector3(1, 0, 0);
        Vector3 result = CustomRigidBody.MultiplyMatrixVector3(rotation, v);
        
        Debug.Log($"Rotation 90° de (1,0,0): {result}");
        Assert(Mathf.Approximately(result.z, -1f), "Rotation matrice");
        
        // Test Gram-Schmidt
        Matrix4x4 testMatrix = Matrix4x4.identity;
        testMatrix[0, 0] = 1.1f; // Légère perturbation
        Matrix4x4 orthonormal = CustomRigidBody.GramSchmidt(testMatrix);
        
        Vector3 col1 = new Vector3(orthonormal[0, 0], orthonormal[1, 0], orthonormal[2, 0]);
        Debug.Log($"Norme colonne après Gram-Schmidt: {col1.magnitude}");
        Assert(Mathf.Approximately(col1.magnitude, 1f), "Gram-Schmidt normalisation");
    }
    
    void TestCollisionDetection()
    {
        Debug.Log("\n--- Test 3: Détection de Collision ---");
        
        // Test sphère-plan
        Vector3 spherePos = new Vector3(0, 0.3f, 0);
        float radius = 0.5f;
        Vector3 planePoint = Vector3.zero;
        Vector3 planeNormal = Vector3.up;
        
        CollisionDetection.CollisionInfo collision = 
            CollisionDetection.SphereToPlane(spherePos, radius, planePoint, planeNormal);
        
        Debug.Log($"Collision sphère-plan: {collision.hasCollision}");
        Debug.Log($"Pénétration: {collision.penetrationDepth}");
        Assert(collision.hasCollision, "Collision sphère-plan détectée");
        Assert(Mathf.Approximately(collision.penetrationDepth, 0.2f), "Pénétration correcte");
        
        // Test sphère-box
        Vector3 boxCenter = new Vector3(2, 0, 0);
        Vector3 boxSize = new Vector3(1, 1, 1);
        
        CollisionDetection.CollisionInfo boxCollision = 
            CollisionDetection.SphereToBox(spherePos, radius, boxCenter, boxSize);
        
        Debug.Log($"Collision sphère-box: {boxCollision.hasCollision}");
    }
    
    void TestConstraintPhysics()
    {
        Debug.Log("\n--- Test 4: Physique des Contraintes ---");
        
        // Créer deux fragments
        FracturedFragment f1 = new FracturedFragment(
            new Vector3(0, 0, 0), 
            Vector3.one * 0.5f, 
            1f, 
            Color.red
        );
        
        FracturedFragment f2 = new FracturedFragment(
            new Vector3(1, 0, 0), 
            Vector3.one * 0.5f, 
            1f, 
            Color.blue
        );
        
        // Créer une contrainte
        float stiffness = 100f;
        float threshold = 0.1f;
        FragmentConstraint constraint = new FragmentConstraint(f1, f2, stiffness, threshold);
        
        Debug.Log($"Longueur au repos: {constraint.restLength}");
        Debug.Log($"Déformation initiale: {constraint.GetDeformation()}");
        
        // Déplacer un fragment pour créer une déformation
        f2.rigidBody.position = new Vector3(1.15f, 0, 0);
        float deformation = constraint.GetDeformation();
        float energy = constraint.GetStoredEnergy();
        
        Debug.Log($"Déformation après déplacement: {deformation}");
        Debug.Log($"Énergie stockée (E = 1/2·k·x²): {energy}");
        
        float expectedEnergy = 0.5f * stiffness * deformation * deformation;
        Assert(Mathf.Approximately(energy, expectedEnergy), "Énergie potentielle correcte");
        
        // Test de rupture
        if (constraint.ShouldBreak())
        {
            Debug.Log("⚠️ La contrainte devrait se rompre!");
            constraint.Break();
            Assert(constraint.isBroken, "Contrainte rompue");
        }
        
        // Nettoyage
        f1.Destroy();
        f2.Destroy();
    }
    
    void Assert(bool condition, string testName)
    {
        if (condition)
        {
            Debug.Log($"✅ {testName}: PASSÉ");
        }
        else
        {
            Debug.LogError($"❌ {testName}: ÉCHOUÉ");
        }
    }
}
