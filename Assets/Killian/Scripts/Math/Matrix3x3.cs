using UnityEngine;

/// <summary>
/// Implémentation personnalisée d'une matrice 3×3 pour les tenseurs d'inertie et transformations 3D.
/// </summary>
public struct Matrix3x3
{
    public float m00, m01, m02;
    public float m10, m11, m12;
    public float m20, m21, m22;

    public static Matrix3x3 Identity
    {
        get
        {
            Matrix3x3 m = new Matrix3x3();
            m.m00 = m.m11 = m.m22 = 1f;
            return m;
        }
    }

    public Matrix3x3(float m00, float m01, float m02,
                     float m10, float m11, float m12,
                     float m20, float m21, float m22)
    {
        this.m00 = m00; this.m01 = m01; this.m02 = m02;
        this.m10 = m10; this.m11 = m11; this.m12 = m12;
        this.m20 = m20; this.m21 = m21; this.m22 = m22;
    }

    /// <summary>Calcule le déterminant de la matrice</summary>
    public float Determinant()
    {
        return m00 * (m11 * m22 - m12 * m21) -
               m01 * (m10 * m22 - m12 * m20) +
               m02 * (m10 * m21 - m11 * m20);
    }

    /// <summary>Retourne l'inverse de la matrice</summary>
    public Matrix3x3 Inverse()
    {
        float det = Determinant();
        if (Mathf.Abs(det) < 0.0001f)
            return Identity;

        float invDet = 1f / det;
        Matrix3x3 result = new Matrix3x3(
            (m11 * m22 - m12 * m21) * invDet,
            -(m01 * m22 - m02 * m21) * invDet,
            (m01 * m12 - m02 * m11) * invDet,
            -(m10 * m22 - m12 * m20) * invDet,
            (m00 * m22 - m02 * m20) * invDet,
            -(m00 * m12 - m02 * m10) * invDet,
            (m10 * m21 - m11 * m20) * invDet,
            -(m00 * m21 - m01 * m20) * invDet,
            (m00 * m11 - m01 * m10) * invDet
        );
        return result;
    }

    /// <summary>Multiplie la matrice par un vecteur</summary>
    public static Vector3 operator *(Matrix3x3 m, Vector3 v)
    {
        return new Vector3(
            m.m00 * v.x + m.m01 * v.y + m.m02 * v.z,
            m.m10 * v.x + m.m11 * v.y + m.m12 * v.z,
            m.m20 * v.x + m.m21 * v.y + m.m22 * v.z
        );
    }

    /// <summary>Multiplie deux matrices 3×3</summary>
    public static Matrix3x3 operator *(Matrix3x3 a, Matrix3x3 b)
    {
        return new Matrix3x3(
            a.m00 * b.m00 + a.m01 * b.m10 + a.m02 * b.m20,
            a.m00 * b.m01 + a.m01 * b.m11 + a.m02 * b.m21,
            a.m00 * b.m02 + a.m01 * b.m12 + a.m02 * b.m22,
            a.m10 * b.m00 + a.m11 * b.m10 + a.m12 * b.m20,
            a.m10 * b.m01 + a.m11 * b.m11 + a.m12 * b.m21,
            a.m10 * b.m02 + a.m11 * b.m12 + a.m12 * b.m22,
            a.m20 * b.m00 + a.m21 * b.m10 + a.m22 * b.m20,
            a.m20 * b.m01 + a.m21 * b.m11 + a.m22 * b.m21,
            a.m20 * b.m02 + a.m21 * b.m12 + a.m22 * b.m22
        );
    }

    public float Get(int row, int col)
    {
        switch (row * 3 + col)
        {
            case 0: return m00; case 1: return m01; case 2: return m02;
            case 3: return m10; case 4: return m11; case 5: return m12;
            case 6: return m20; case 7: return m21; case 8: return m22;
            default: return 0f;
        }
    }

    public void Set(int row, int col, float value)
    {
        switch (row * 3 + col)
        {
            case 0: m00 = value; break;
            case 1: m01 = value; break;
            case 2: m02 = value; break;
            case 3: m10 = value; break;
            case 4: m11 = value; break;
            case 5: m12 = value; break;
            case 6: m20 = value; break;
            case 7: m21 = value; break;
            case 8: m22 = value; break;
        }
    }
}
