using UnityEngine;

/// <summary>
/// Implémentation personnalisée des matrices 4×4 pour les transformations sans utiliser les fonctions prédéfinies Unity.
/// Utilisée pour la rotation, translation et composition des transformations.
/// </summary>
public struct Matrix4x4Custom
{
    public float m00, m01, m02, m03;
    public float m10, m11, m12, m13;
    public float m20, m21, m22, m23;
    public float m30, m31, m32, m33;

    /// <summary>Matrice identité</summary>
    public static Matrix4x4Custom Identity
    {
        get
        {
            Matrix4x4Custom m = new Matrix4x4Custom();
            m.m00 = m.m11 = m.m22 = m.m33 = 1f;
            return m;
        }
    }

    /// <summary>Crée une matrice de translation</summary>
    public static Matrix4x4Custom Translation(Vector3 position)
    {
        Matrix4x4Custom m = Identity;
        m.m03 = position.x;
        m.m13 = position.y;
        m.m23 = position.z;
        return m;
    }

    /// <summary>Crée une matrice de rotation autour de l'axe X (en radians)</summary>
    public static Matrix4x4Custom RotationX(float angle)
    {
        Matrix4x4Custom m = Identity;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        
        m.m11 = cos;
        m.m12 = -sin;
        m.m21 = sin;
        m.m22 = cos;
        return m;
    }

    /// <summary>Crée une matrice de rotation autour de l'axe Y (en radians)</summary>
    public static Matrix4x4Custom RotationY(float angle)
    {
        Matrix4x4Custom m = Identity;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        
        m.m00 = cos;
        m.m02 = sin;
        m.m20 = -sin;
        m.m22 = cos;
        return m;
    }

    /// <summary>Crée une matrice de rotation autour de l'axe Z (en radians)</summary>
    public static Matrix4x4Custom RotationZ(float angle)
    {
        Matrix4x4Custom m = Identity;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        
        m.m00 = cos;
        m.m01 = -sin;
        m.m10 = sin;
        m.m11 = cos;
        return m;
    }

    /// <summary>Crée une matrice de rotation à partir d'un quaternion</summary>
    public static Matrix4x4Custom FromQuaternion(Quaternion q)
    {
        Matrix4x4Custom m = Identity;
        
        float x = q.x, y = q.y, z = q.z, w = q.w;
        float xx = x * x, yy = y * y, zz = z * z;
        float xy = x * y, xz = x * z, yz = y * z;
        float wx = w * x, wy = w * y, wz = w * z;
        
        m.m00 = 1f - 2f * (yy + zz);
        m.m01 = 2f * (xy - wz);
        m.m02 = 2f * (xz + wy);
        
        m.m10 = 2f * (xy + wz);
        m.m11 = 1f - 2f * (xx + zz);
        m.m12 = 2f * (yz - wx);
        
        m.m20 = 2f * (xz - wy);
        m.m21 = 2f * (yz + wx);
        m.m22 = 1f - 2f * (xx + yy);
        
        return m;
    }

    /// <summary>Crée une matrice d'échelle</summary>
    public static Matrix4x4Custom Scale(Vector3 scale)
    {
        Matrix4x4Custom m = Identity;
        m.m00 = scale.x;
        m.m11 = scale.y;
        m.m22 = scale.z;
        return m;
    }

    /// <summary>Multiplie deux matrices</summary>
    public static Matrix4x4Custom operator *(Matrix4x4Custom a, Matrix4x4Custom b)
    {
        Matrix4x4Custom result = new Matrix4x4Custom();
        
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                float sum = 0f;
                for (int i = 0; i < 4; i++)
                {
                    sum += a.Get(row, i) * b.Get(i, col);
                }
                result.Set(row, col, sum);
            }
        }
        
        return result;
    }

    /// <summary>Transforme un vecteur 3D par la matrice</summary>
    public static Vector3 operator *(Matrix4x4Custom m, Vector3 v)
    {
        Vector3 result = new Vector3(
            m.m00 * v.x + m.m01 * v.y + m.m02 * v.z + m.m03,
            m.m10 * v.x + m.m11 * v.y + m.m12 * v.z + m.m13,
            m.m20 * v.x + m.m21 * v.y + m.m22 * v.z + m.m23
        );
        return result;
    }

    /// <summary>Obtient l'élément à la position (row, col)</summary>
    public float Get(int row, int col)
    {
        switch (row * 4 + col)
        {
            case 0: return m00; case 1: return m01; case 2: return m02; case 3: return m03;
            case 4: return m10; case 5: return m11; case 6: return m12; case 7: return m13;
            case 8: return m20; case 9: return m21; case 10: return m22; case 11: return m23;
            case 12: return m30; case 13: return m31; case 14: return m32; case 15: return m33;
            default: return 0f;
        }
    }

    /// <summary>Définit l'élément à la position (row, col)</summary>
    public void Set(int row, int col, float value)
    {
        switch (row * 4 + col)
        {
            case 0: m00 = value; break; case 1: m01 = value; break; case 2: m02 = value; break; case 3: m03 = value; break;
            case 4: m10 = value; break; case 5: m11 = value; break; case 6: m12 = value; break; case 7: m13 = value; break;
            case 8: m20 = value; break; case 9: m21 = value; break; case 10: m22 = value; break; case 11: m23 = value; break;
            case 12: m30 = value; break; case 13: m31 = value; break; case 14: m32 = value; break; case 15: m33 = value; break;
        }
    }

    /// <summary>Extrait la position de la matrice de transformation</summary>
    public Vector3 GetPosition()
    {
        return new Vector3(m03, m13, m23);
    }

    /// <summary>Extrait l'orientation (rotation) de la matrice</summary>
    public Quaternion GetRotation()
    {
        float trace = m00 + m11 + m22;
        Quaternion q = new Quaternion();
        
        if (trace > 0)
        {
            float s = 0.5f / Mathf.Sqrt(trace + 1.0f);
            q.w = 0.25f / s;
            q.x = (m21 - m12) * s;
            q.y = (m02 - m20) * s;
            q.z = (m10 - m01) * s;
        }
        else if (m00 > m11 && m00 > m22)
        {
            float s = 2.0f * Mathf.Sqrt(1.0f + m00 - m11 - m22);
            q.w = (m21 - m12) / s;
            q.x = 0.25f * s;
            q.y = (m01 + m10) / s;
            q.z = (m02 + m20) / s;
        }
        else if (m11 > m22)
        {
            float s = 2.0f * Mathf.Sqrt(1.0f + m11 - m00 - m22);
            q.w = (m02 - m20) / s;
            q.x = (m01 + m10) / s;
            q.y = 0.25f * s;
            q.z = (m12 + m21) / s;
        }
        else
        {
            float s = 2.0f * Mathf.Sqrt(1.0f + m22 - m00 - m11);
            q.w = (m10 - m01) / s;
            q.x = (m02 + m20) / s;
            q.y = (m12 + m21) / s;
            q.z = 0.25f * s;
        }
        
        return q;
    }

    /// <summary>Convertit en matrice Unity standard</summary>
    public UnityEngine.Matrix4x4 ToUnityMatrix()
    {
        UnityEngine.Matrix4x4 m = UnityEngine.Matrix4x4.zero;
        m.m00 = m00; m.m01 = m01; m.m02 = m02; m.m03 = m03;
        m.m10 = m10; m.m11 = m11; m.m12 = m12; m.m13 = m13;
        m.m20 = m20; m.m21 = m21; m.m22 = m22; m.m23 = m23;
        m.m30 = m30; m.m31 = m31; m.m32 = m32; m.m33 = m33;
        return m;
    }
}
