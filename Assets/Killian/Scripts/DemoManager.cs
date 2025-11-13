using UnityEngine;

/// <summary>
/// Gestionnaire de la scène de démonstration.
/// Initialise les objets fracturés et applique des forces/impulsions pour tester la simulation.
/// </summary>
public class DemoManager : MonoBehaviour
{
    [SerializeField] private Vector3 initialForce = new Vector3(5f, 0f, 0f);
    [SerializeField] private float forceDuration = 2f;
    [SerializeField] private float impactForce = 20f;

    private PhysicsEngine physicsEngine;
    private float forceTimer = 0f;
    private bool forceApplied = false;

    private void Start()
    {
        physicsEngine = PhysicsEngine.Instance;

        // Attend que les objets fractionnés soient créés
        Invoke("ApplyInitialForce", 0.1f);
    }

    /// <summary>Applique une force initiale aux shards</summary>
    private void ApplyInitialForce()
    {
        var bodies = physicsEngine.GetAllBodies();
        if (bodies.Count > 0)
        {
            // Applique une force au premier shard
            bodies[0].AddForce(initialForce);
            Debug.Log("Initial force applied to first shard");
        }
    }

    private void Update()
    {
        HandleUserInput();
        DisplayDebugInfo();
    }

    /// <summary>Gère les entrées utilisateur</summary>
    private void HandleUserInput()
    {
        // Appui sur SPACE pour appliquer une impulsion au premier shard
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var bodies = physicsEngine.GetAllBodies();
            if (bodies.Count > 0)
            {
                bodies[0].AddImpulse(Vector3.up * impactForce);
                Debug.Log("Impact impulse applied!");
            }
        }

        // R pour réinitialiser la scène
        if (Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }

        // P pour mettre en pause
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (Time.timeScale > 0)
                physicsEngine.Pause();
            else
                physicsEngine.Resume();
        }
    }

    /// <summary>Affiche les informations de debug</summary>
    private void DisplayDebugInfo()
    {
        var bodies = physicsEngine.GetAllBodies();
        var collisions = physicsEngine.GetLastFrameCollisions();

        Debug.DrawLine(Vector3.zero, Vector3.right * 5, Color.red);
        Debug.DrawLine(Vector3.zero, Vector3.up * 5, Color.green);
        Debug.DrawLine(Vector3.zero, Vector3.forward * 5, Color.blue);

        // Dessine les contraintes actives
        var constraints = physicsEngine.GetAllConstraints();
        foreach (var constraint in constraints)
        {
            if (!constraint.IsBroken)
            {
                Vector3 anchorA = constraint.GetWorldAnchorA();
                Vector3 anchorB = constraint.GetWorldAnchorB();
                Debug.DrawLine(anchorA, anchorB, Color.cyan);

                // Affiche l'énergie stockée
                float energy = constraint.GetStoredEnergy();
                if (energy > 0.01f)
                {
                    Debug.DrawLine(anchorA, anchorA + Vector3.up * energy * 0.1f, Color.yellow);
                }
            }
        }
    }
}
