using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Système optionnel d'affichage des gizmos de débogage.
/// Visualise les forces, contraintes et collisions en éditeur et runtime.
/// </summary>
public class DebugVisualizer : MonoBehaviour
{
    [SerializeField] private bool showConstraints = true;
    [SerializeField] private bool showCollisions = true;
    [SerializeField] private bool showForces = false;
    [SerializeField] private float forceArrowScale = 0.1f;
    [SerializeField] private float constraintThickness = 0.05f;

    private PhysicsEngine physicsEngine;

    private void Start()
    {
        physicsEngine = PhysicsEngine.Instance;
    }

    private void OnDrawGizmos()
    {
        if (physicsEngine == null) return;

        if (showConstraints)
            DrawConstraints();

        if (showCollisions)
            DrawCollisions();
    }

    /// <summary>Dessine les contraintes actives</summary>
    private void DrawConstraints()
    {
        var constraints = physicsEngine.GetAllConstraints();

        foreach (var constraint in constraints)
        {
            if (constraint.IsBroken) continue;

            Vector3 anchorA = constraint.GetWorldAnchorA();
            Vector3 anchorB = constraint.GetWorldAnchorB();
            float deformation = constraint.GetDeformation();

            // Couleur basée sur la déformation
            Color color = Color.Lerp(
                Color.green,
                Color.red,
                Mathf.Clamp01(Mathf.Abs(deformation) / constraint.GetStoredEnergy())
            );

            Gizmos.color = color;
            Gizmos.DrawLine(anchorA, anchorB);

            // Affiche les sphères aux ancres
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(anchorA, 0.1f);
            Gizmos.DrawWireSphere(anchorB, 0.1f);
        }
    }

    /// <summary>Dessine les collisions détectées</summary>
    private void DrawCollisions()
    {
        var collisions = physicsEngine.GetLastFrameCollisions();

        foreach (var collision in collisions)
        {
            // Point de contact
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(collision.ContactPoint, 0.15f);

            // Normal de collision
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(
                collision.ContactPoint,
                collision.ContactPoint + collision.Normal * 0.5f
            );
        }
    }

    /// <summary>Affiche les informations détaillées en GUI</summary>
    private void OnGUI()
    {
        if (physicsEngine == null) return;

        // Onglet de débogage
        if (GUI.Button(new Rect(10, 280, 100, 30), "Debug Info"))
        {
            showConstraints = !showConstraints;
        }

        if (showConstraints)
        {
            DrawDebugInfo();
        }
    }

    private void DrawDebugInfo()
    {
        var bodies = physicsEngine.GetAllBodies();
        var constraints = physicsEngine.GetAllConstraints();

        int yPos = 320;

        GUI.Label(new Rect(10, yPos, 300, 20), "=== BODY INFO ===");
        yPos += 25;

        for (int i = 0; i < Mathf.Min(bodies.Count, 3); i++)  // Affiche les 3 premiers
        {
            var body = bodies[i];
            GUI.Label(new Rect(10, yPos, 300, 20),
                $"Body {i}: Vel=({body.LinearVelocity.magnitude:F2}) Pos=({body.Position.x:F1},{body.Position.y:F1})");
            yPos += 20;
        }

        yPos += 10;
        GUI.Label(new Rect(10, yPos, 300, 20), "=== CONSTRAINT INFO ===");
        yPos += 25;

        int brokenCount = 0;
        float totalEnergy = 0f;

        foreach (var constraint in constraints)
        {
            if (constraint.IsBroken)
                brokenCount++;
            else
                totalEnergy += constraint.GetStoredEnergy();
        }

        GUI.Label(new Rect(10, yPos, 300, 20), $"Total Constraints: {constraints.Count}");
        yPos += 20;
        GUI.Label(new Rect(10, yPos, 300, 20), $"Broken: {brokenCount}");
        yPos += 20;
        GUI.Label(new Rect(10, yPos, 300, 20), $"Total Stored Energy: {totalEnergy:F2}J");
        yPos += 20;

        // Affiche les 2 premières contraintes actives
        int constraintCount = 0;
        foreach (var constraint in constraints)
        {
            if (constraint.IsBroken) continue;
            if (constraintCount >= 2) break;

            float deformation = constraint.GetDeformation();
            float energy = constraint.GetStoredEnergy();
            GUI.Label(new Rect(10, yPos, 300, 20),
                $"Constraint {constraint.ID}: Deform={deformation:F3} Energy={energy:F2}J");
            yPos += 20;
            constraintCount++;
        }
    }
}
