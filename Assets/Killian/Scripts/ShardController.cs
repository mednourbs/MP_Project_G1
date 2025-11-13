using UnityEngine;

/// <summary>
/// Contrôle la synchronisation entre un corps rigide physique et son GameObject.
/// Met à jour la position et rotation du GameObject basée sur la simulation physique.
/// </summary>
public class ShardController : MonoBehaviour
{
    private RigidBody rigidBody;

    public void Initialize(RigidBody body)
    {
        rigidBody = body;
    }

    private void Update()
    {
        if (rigidBody == null) return;

        // Synchronise la position et la rotation
        transform.position = rigidBody.Position;
        transform.rotation = rigidBody.Rotation;
    }

    /// <summary>Applique une force au shard</summary>
    public void ApplyForce(Vector3 force)
    {
        if (rigidBody != null)
            rigidBody.AddForce(force);
    }

    /// <summary>Applique une impulsion au shard</summary>
    public void ApplyImpulse(Vector3 impulse)
    {
        if (rigidBody != null)
            rigidBody.AddImpulse(impulse);
    }

    /// <summary>Retourne le corps rigide associé</summary>
    public RigidBody GetRigidBody() => rigidBody;
}
