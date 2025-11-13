using UnityEngine;

/// <summary>
/// Script simple à attacher à un GameObject vide dans la scène Unity
/// Lance automatiquement la simulation au démarrage
/// </summary>
public class SimulationStarter : MonoBehaviour
{
    [Header("Paramètres de simulation")]
    [Range(0f, 1f)]
    [Tooltip("Valeur alpha pour modifier les propriétés physiques de la sphère")]
    public float alpha = 0.5f;
    
    private PhysicsSimulationController controller;
    
    void Start()
    {
        // Créer le contrôleur de simulation
        GameObject controllerObj = new GameObject("PhysicsSimulationController");
        controller = controllerObj.AddComponent<PhysicsSimulationController>();
        
        // Transmettre alpha au contrôleur
        controller.alpha = alpha;
        
        Debug.Log("=== SIMULATION DE MOTEUR PHYSIQUE ===");
        Debug.Log("Tous les objets sont créés procéduralement");
        Debug.Log("Aucun Rigidbody ou Collider Unity n'est utilisé");
        Debug.Log("Intégration RK4 pour la physique");
        Debug.Log("Système de contraintes et fractures implémenté");
        Debug.Log($"Alpha: {alpha}");
        Debug.Log("=====================================");
    }
    
    void Update()
    {
        // Mettre à jour alpha en temps réel si modifié dans l'Inspector
        if (controller != null && controller.alpha != alpha)
        {
            controller.alpha = alpha;
            controller.UpdateSpherePhysics();
        }
    }
}
