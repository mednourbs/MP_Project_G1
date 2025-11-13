using UnityEngine;

/// <summary>
/// Script simple à attacher à un GameObject vide dans la scène Unity
/// Lance automatiquement la simulation au démarrage
/// </summary>
public class SimulationStarter : MonoBehaviour
{
    void Start()
    {
        // Créer le contrôleur de simulation
        GameObject controllerObj = new GameObject("PhysicsSimulationController");
        PhysicsSimulationController controller = controllerObj.AddComponent<PhysicsSimulationController>();
        
        Debug.Log("=== SIMULATION DE MOTEUR PHYSIQUE ===");
        Debug.Log("Tous les objets sont créés procéduralement");
        Debug.Log("Aucun Rigidbody ou Collider Unity n'est utilisé");
        Debug.Log("Intégration RK4 pour la physique");
        Debug.Log("Système de contraintes et fractures implémenté");
        Debug.Log("=====================================");
    }
}
