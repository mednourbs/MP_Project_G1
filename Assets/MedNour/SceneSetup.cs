using UnityEngine;

/// <summary>
/// Utilitaire pour créer la scène de simulation via l'éditeur Unity
/// Menu: Tools > Setup Physics Simulation Scene
/// </summary>
#if UNITY_EDITOR
using UnityEditor;

public class SceneSetup : MonoBehaviour
{
    [MenuItem("Tools/Setup Physics Simulation Scene")]
    static void SetupScene()
    {
        // Nettoyer la scène
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name != "Main Camera" && obj.name != "Directional Light")
            {
                DestroyImmediate(obj);
            }
        }
        
        // Créer le starter
        GameObject starter = new GameObject("SimulationStarter");
        starter.AddComponent<SimulationStarter>();
        
        // Positionner la caméra
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(0, 3, -10);
            mainCamera.transform.rotation = Quaternion.Euler(15, 0, 0);
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.5f, 0.7f, 0.9f);
        }
        
        // Configurer la lumière
        Light directionalLight = FindFirstObjectByType<Light>();
        if (directionalLight != null)
        {
            directionalLight.transform.rotation = Quaternion.Euler(50, -30, 0);
            directionalLight.intensity = 1.2f;
        }
        
        Debug.Log("✅ Scène configurée avec succès!");
        Debug.Log("Appuyez sur Play pour lancer la simulation.");
        
        EditorUtility.DisplayDialog(
            "Scène Configurée", 
            "La scène de simulation physique est prête!\n\n" +
            "Tous les objets seront créés automatiquement au démarrage.\n\n" +
            "Appuyez sur Play ▶️ pour voir la simulation.",
            "OK"
        );
    }
}
#endif
