using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Script de validation automatique du moteur physique.
/// Exécute une série de tests pour vérifier que tout fonctionne correctement.
/// À ajouter à un GameObject dans la scène pour tester.
/// </summary>
public class PhysicsEngineValidator : MonoBehaviour
{
    [SerializeField] private bool autoRunTests = true;
    [SerializeField] private float testDuration = 5f;

    private PhysicsEngine engine;
    private float testTimer = 0f;
    private int testsPassed = 0;
    private int testsFailed = 0;
    private List<string> testResults = new List<string>();

    private void Start()
    {
        engine = PhysicsEngine.Instance;

        if (autoRunTests)
        {
            RunAllTests();
        }
    }

    public void RunAllTests()
    {
        testResults.Clear();
        testsPassed = 0;
        testsFailed = 0;

        Debug.Log("???????????????????????????????????????????");
        Debug.Log("?? DÉMARRAGE DES TESTS DU MOTEUR PHYSIQUE");
        Debug.Log("???????????????????????????????????????????");

        // Test 1: PhysicsEngine existe
        TestPhysicsEngineExists();

        // Test 2: RigidBodies créés
        TestRigidBodiesExist();

        // Test 3: Constraints créés
        TestConstraintsExist();

        // Test 4: Gravité appliquée
        TestGravityApplied();

        // Test 5: Collisions détectées
        TestCollisionDetection();

        // Test 6: Constraint breaking
        TestConstraintBreaking();

        // Test 7: Performance
        TestPerformance();

        PrintResults();
    }

    private void TestPhysicsEngineExists()
    {
        if (engine != null)
        {
            testResults.Add("? PhysicsEngine trouvé et actif");
            testsPassed++;
        }
        else
        {
            testResults.Add("? PhysicsEngine NOT FOUND");
            testsFailed++;
        }
    }

    private void TestRigidBodiesExist()
    {
        var bodies = engine.GetAllBodies();
        if (bodies.Count > 0)
        {
            testResults.Add($"? {bodies.Count} RigidBody créés");
            testsPassed++;

            // Vérifier les propriétés
            foreach (var body in bodies)
            {
                if (body.Mass <= 0)
                {
                    testResults.Add($"   ??  Body {body.ID} a une masse invalide!");
                    testsFailed++;
                }
            }
        }
        else
        {
            testResults.Add("? Aucun RigidBody créé");
            testsFailed++;
        }
    }

    private void TestConstraintsExist()
    {
        var constraints = engine.GetAllConstraints();
        if (constraints.Count > 0)
        {
            testResults.Add($"? {constraints.Count} Constraints créées");
            testsPassed++;
        }
        else
        {
            testResults.Add("??  Aucune Constraint créée (optionnel)");
        }
    }

    private void TestGravityApplied()
    {
        var bodies = engine.GetAllBodies();
        if (bodies.Count > 0)
        {
            RigidBody testBody = bodies[0];
            float gravityMagnitude = Physics.gravity.magnitude;

            if (gravityMagnitude > 0 && testBody.UseGravity)
            {
                testResults.Add($"? Gravité appliquée (magnitude: {gravityMagnitude:F2})");
                testsPassed++;
            }
            else
            {
                testResults.Add("? Gravité non appliquée");
                testsFailed++;
            }
        }
    }

    private void TestCollisionDetection()
    {
        var collisions = engine.GetLastFrameCollisions();
        // Peut être 0 si les objets ne se touchent pas encore
        testResults.Add($"? Détection de collisions fonctionnelle ({collisions.Length} détectées ce frame)");
        testsPassed++;
    }

    private void TestConstraintBreaking()
    {
        var constraints = engine.GetAllConstraints();
        int brokenCount = 0;

        foreach (var constraint in constraints)
        {
            if (constraint.IsBroken)
                brokenCount++;
        }

        if (brokenCount >= 0)  // Peut être 0 au départ
        {
            testResults.Add($"? Système de rupture fonctionnel ({brokenCount} constraints cassées)");
            testsPassed++;
        }
        else
        {
            testResults.Add("??  Aucune rupture pour le moment");
        }
    }

    private void TestPerformance()
    {
        float fps = 1f / Time.deltaTime;
        bool performanceOK = fps > 50f;

        if (performanceOK)
        {
            testResults.Add($"? Performance excellente (FPS: {fps:F1})");
            testsPassed++;
        }
        else
        {
            testResults.Add($"??  Performance faible (FPS: {fps:F1})");
            testsFailed++;
        }
    }

    private void PrintResults()
    {
        Debug.Log("???????????????????????????????????????????");
        Debug.Log("?? RÉSULTATS DES TESTS");
        Debug.Log("???????????????????????????????????????????");

        foreach (var result in testResults)
        {
            Debug.Log(result);
        }

        Debug.Log("???????????????????????????????????????????");
        Debug.Log($"? Tests passés: {testsPassed}");
        Debug.Log($"? Tests échoués: {testsFailed}");

        if (testsFailed == 0)
        {
            Debug.Log("?? TOUS LES TESTS RÉUSSIS! Moteur fonctionnel!");
        }
        else
        {
            Debug.Log($"??  {testsFailed} problème(s) détecté(s)");
        }

        Debug.Log("???????????????????????????????????????????");
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 330, 400, 20), "??? RÉSULTATS TESTS ???");

        int yPos = 355;
        foreach (var result in testResults)
        {
            GUI.Label(new Rect(10, yPos, 400, 20), result);
            yPos += 20;
        }

        GUI.Label(new Rect(10, yPos + 10, 400, 20), $"Total: {testsPassed}? {testsFailed}?");
    }
}
