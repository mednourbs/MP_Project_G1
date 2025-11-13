using UnityEngine;

/// <summary>
/// Point d'entrée principal pour le moteur physique personnalisé.
/// Initialise et affiche les statistiques de la simulation physique.
/// </summary>
public class Physique : MonoBehaviour
{
    [SerializeField] private bool showPerformanceStats = true;

    void Start()
    {
        Debug.Log("Moteur Physique - Custom Physics Engine Ready!");
    }

    void OnGUI()
    {
        if (!showPerformanceStats) return;

        // Affiche les commandes et informations
        GUI.Label(new Rect(10, 10, 400, 25), "== MOTEUR PHYSIQUE CUSTOM ==", new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold });
        GUI.Label(new Rect(10, 40, 300, 20), "Commandes:");
        GUI.Label(new Rect(10, 60, 300, 20), "  SPACE - Appliquer une impulsion");
        GUI.Label(new Rect(10, 80, 300, 20), "  R - Réinitialiser la scène");
        GUI.Label(new Rect(10, 100, 300, 20), "  P - Pause/Resume");
        
        GUI.Label(new Rect(10, 140, 300, 20), $"FPS: {(1f / Time.deltaTime):F1}");
        GUI.Label(new Rect(10, 160, 300, 20), $"Time.deltaTime: {Time.deltaTime:F4}");
        GUI.Label(new Rect(10, 180, 300, 20), $"Time.timeScale: {Time.timeScale:F2}");
    }
}
