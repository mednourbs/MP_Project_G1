using UnityEngine;

/// <summary>
/// Corps rigide personnalisé avec matrices de transformation manuelles
/// Basé sur les principes de la dynamique des corps rigides
/// </summary>
public class CustomRigidBody
{
    // Propriétés physiques
    public float mass = 1f;
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 angularVelocity;
    
    // Momentum
    public Vector3 linearMomentum;  // P = m * v
    public Vector3 angularMomentum; // L
    
    // Matrices de rotation et d'inertie
    public Matrix4x4 rotationMatrix = Matrix4x4.identity;
    public Matrix4x4 inertiaTensor;
    public Matrix4x4 inertiaTensorInverse;
    
    // Propriétés géométriques
    public float radius; // Pour sphère
    public Vector3 dimensions; // Pour box
    
    public bool isActive = true;
    
    /// <summary>
    /// Constructeur pour une sphère
    /// </summary>
    public CustomRigidBody(Vector3 initialPosition, float sphereRadius, float sphereMass)
    {
        position = initialPosition;
        radius = sphereRadius;
        mass = sphereMass;
        velocity = Vector3.zero;
        angularVelocity = Vector3.zero;
        linearMomentum = Vector3.zero;
        angularMomentum = Vector3.zero;
        
        // Tenseur d'inertie pour une sphère: I = (2/5) * m * r²
        float I = (2f / 5f) * mass * radius * radius;
        inertiaTensor = Matrix4x4.identity;
        inertiaTensor[0, 0] = I;
        inertiaTensor[1, 1] = I;
        inertiaTensor[2, 2] = I;
        
        inertiaTensorInverse = Matrix4x4.identity;
        inertiaTensorInverse[0, 0] = 1f / I;
        inertiaTensorInverse[1, 1] = 1f / I;
        inertiaTensorInverse[2, 2] = 1f / I;
    }
    
    /// <summary>
    /// Constructeur pour une box (fragment de mur)
    /// </summary>
    public CustomRigidBody(Vector3 initialPosition, Vector3 boxDimensions, float boxMass)
    {
        position = initialPosition;
        dimensions = boxDimensions;
        mass = boxMass;
        velocity = Vector3.zero;
        angularVelocity = Vector3.zero;
        linearMomentum = Vector3.zero;
        angularMomentum = Vector3.zero;
        
        // Tenseur d'inertie pour une box
        float Ixx = (1f / 12f) * mass * (boxDimensions.y * boxDimensions.y + boxDimensions.z * boxDimensions.z);
        float Iyy = (1f / 12f) * mass * (boxDimensions.x * boxDimensions.x + boxDimensions.z * boxDimensions.z);
        float Izz = (1f / 12f) * mass * (boxDimensions.x * boxDimensions.x + boxDimensions.y * boxDimensions.y);
        
        inertiaTensor = Matrix4x4.identity;
        inertiaTensor[0, 0] = Ixx;
        inertiaTensor[1, 1] = Iyy;
        inertiaTensor[2, 2] = Izz;
        
        inertiaTensorInverse = Matrix4x4.identity;
        inertiaTensorInverse[0, 0] = 1f / Ixx;
        inertiaTensorInverse[1, 1] = 1f / Iyy;
        inertiaTensorInverse[2, 2] = 1f / Izz;
    }
    
    /// <summary>
    /// Applique une force au centre de masse
    /// </summary>
    public void ApplyForce(Vector3 force)
    {
        linearMomentum += force;
    }
    
    /// <summary>
    /// Applique un torque (couple)
    /// </summary>
    public void ApplyTorque(Vector3 torque)
    {
        angularMomentum += torque;
    }
    
    /// <summary>
    /// Applique une impulsion instantanée
    /// </summary>
    public void ApplyImpulse(Vector3 impulse, Vector3 contactPoint)
    {
        // Impulsion linéaire
        linearMomentum += impulse;
        
        // Impulsion angulaire
        Vector3 r = contactPoint - position;
        Vector3 angularImpulse = Vector3.Cross(r, impulse);
        angularMomentum += angularImpulse;
    }
    
    /// <summary>
    /// Intégration RK4 pour la position et vélocité
    /// </summary>
    public void IntegrateRK4(float dt, Vector3 gravity, float damping = 0f)
    {
        if (!isActive) return;
        
        // k1
        Vector3 k1_v = ComputeAcceleration(velocity, gravity, damping);
        Vector3 k1_p = velocity;
        
        // k2
        Vector3 k2_v = ComputeAcceleration(velocity + 0.5f * dt * k1_v, gravity, damping);
        Vector3 k2_p = velocity + 0.5f * dt * k1_v;
        
        // k3
        Vector3 k3_v = ComputeAcceleration(velocity + 0.5f * dt * k2_v, gravity, damping);
        Vector3 k3_p = velocity + 0.5f * dt * k2_v;
        
        // k4
        Vector3 k4_v = ComputeAcceleration(velocity + dt * k3_v, gravity, damping);
        Vector3 k4_p = velocity + dt * k3_v;
        
        // Update
        position += dt / 6f * (k1_p + 2f * k2_p + 2f * k3_p + k4_p);
        velocity += dt / 6f * (k1_v + 2f * k2_v + 2f * k3_v + k4_v);
        
        // Update linear momentum
        linearMomentum = mass * velocity;
        
        // Update angular velocity
        Matrix4x4 I_inv_world = rotationMatrix * inertiaTensorInverse * rotationMatrix.transpose;
        angularVelocity = MultiplyMatrixVector3(I_inv_world, angularMomentum);
        
        // Update rotation matrix
        UpdateRotation(dt);
    }
    
    private Vector3 ComputeAcceleration(Vector3 vel, Vector3 gravity, float damping)
    {
        return gravity - damping * vel;
    }
    
    /// <summary>
    /// Met à jour la matrice de rotation basée sur la vélocité angulaire
    /// </summary>
    private void UpdateRotation(float dt)
    {
        // Matrice Omega (skew-symmetric)
        Matrix4x4 Omega = Matrix4x4.zero;
        Omega[0, 1] = -angularVelocity.z;
        Omega[0, 2] = angularVelocity.y;
        Omega[1, 0] = angularVelocity.z;
        Omega[1, 2] = -angularVelocity.x;
        Omega[2, 0] = -angularVelocity.y;
        Omega[2, 1] = angularVelocity.x;
        
        // R' = Omega * R
        Matrix4x4 Rupdate = AddMatrices(Matrix4x4.identity, MultiplyScalar(Omega, dt));
        rotationMatrix = MultiplyMatrices(Rupdate, rotationMatrix);
        
        // Orthonormalisation Gram-Schmidt
        rotationMatrix = GramSchmidt(rotationMatrix);
    }
    
    /// <summary>
    /// Crée une matrice de transformation 4x4 complète
    /// </summary>
    public Matrix4x4 GetTransformMatrix()
    {
        Matrix4x4 T = rotationMatrix;
        T[0, 3] = position.x;
        T[1, 3] = position.y;
        T[2, 3] = position.z;
        T[3, 3] = 1f;
        return T;
    }
    
    // ============ Utilitaires mathématiques ============
    
    public static Vector3 MultiplyMatrixVector3(Matrix4x4 M, Vector3 v)
    {
        return new Vector3(
            M[0, 0] * v.x + M[0, 1] * v.y + M[0, 2] * v.z,
            M[1, 0] * v.x + M[1, 1] * v.y + M[1, 2] * v.z,
            M[2, 0] * v.x + M[2, 1] * v.y + M[2, 2] * v.z
        );
    }
    
    public static Matrix4x4 GramSchmidt(Matrix4x4 R)
    {
        Vector3 x = new Vector3(R[0, 0], R[1, 0], R[2, 0]);
        Vector3 y = new Vector3(R[0, 1], R[1, 1], R[2, 1]);
        
        Vector3 u1 = x.normalized;
        Vector3 u2 = (y - Vector3.Dot(y, u1) * u1).normalized;
        Vector3 u3 = Vector3.Cross(u1, u2);
        
        Matrix4x4 Rnew = Matrix4x4.identity;
        Rnew[0, 0] = u1.x; Rnew[1, 0] = u1.y; Rnew[2, 0] = u1.z;
        Rnew[0, 1] = u2.x; Rnew[1, 1] = u2.y; Rnew[2, 1] = u2.z;
        Rnew[0, 2] = u3.x; Rnew[1, 2] = u3.y; Rnew[2, 2] = u3.z;
        
        return Rnew;
    }
    
    public static Matrix4x4 MultiplyScalar(Matrix4x4 M, float s)
    {
        Matrix4x4 result = Matrix4x4.zero;
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                result[i, j] = M[i, j] * s;
        return result;
    }
    
    public static Matrix4x4 AddMatrices(Matrix4x4 A, Matrix4x4 B)
    {
        Matrix4x4 result = Matrix4x4.zero;
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                result[i, j] = A[i, j] + B[i, j];
        return result;
    }
    
    public static Matrix4x4 MultiplyMatrices(Matrix4x4 A, Matrix4x4 B)
    {
        Matrix4x4 result = Matrix4x4.zero;
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                for (int k = 0; k < 4; k++)
                    result[i, j] += A[i, k] * B[k, j];
        return result;
    }
}
