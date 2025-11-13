using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gère la géométrie des objets et leur fracturation en shards.
/// Chaque shard est un fragment indépendant avec sa propre géométrie.
/// </summary>
public class GeometryManager
{
    public struct Shard
    {
        public int ID;
        public Vector3[] Vertices;
        public int[] Triangles;
        public Vector3 CenterOfMass;
        public float Volume;
    }

    /// <summary>Génère un cube et le divise en shards</summary>
    public static List<Shard> GenerateFracturedCube(Vector3 size, int subdivisionsPerAxis = 2)
    {
        List<Shard> shards = new List<Shard>();
        
        float cellSizeX = size.x / subdivisionsPerAxis;
        float cellSizeY = size.y / subdivisionsPerAxis;
        float cellSizeZ = size.z / subdivisionsPerAxis;
        
        int shardID = 0;
        for (int x = 0; x < subdivisionsPerAxis; x++)
        {
            for (int y = 0; y < subdivisionsPerAxis; y++)
            {
                for (int z = 0; z < subdivisionsPerAxis; z++)
                {
                    Vector3 shardSize = new Vector3(cellSizeX, cellSizeY, cellSizeZ);
                    Vector3 shardPosition = new Vector3(
                        x * cellSizeX - size.x / 2f + cellSizeX / 2f,
                        y * cellSizeY - size.y / 2f + cellSizeY / 2f,
                        z * cellSizeZ - size.z / 2f + cellSizeZ / 2f
                    );
                    
                    Shard shard = CreateCubeShard(shardID++, shardPosition, shardSize);
                    shards.Add(shard);
                }
            }
        }
        
        return shards;
    }

    /// <summary>Crée un shard cubique</summary>
    private static Shard CreateCubeShard(int id, Vector3 position, Vector3 size)
    {
        Shard shard = new Shard();
        shard.ID = id;
        shard.CenterOfMass = position;
        shard.Volume = size.x * size.y * size.z;

        float hx = size.x / 2f;
        float hy = size.y / 2f;
        float hz = size.z / 2f;

        // Vertices d'un cube
        shard.Vertices = new Vector3[]
        {
            position + new Vector3(-hx, -hy, -hz),
            position + new Vector3(hx, -hy, -hz),
            position + new Vector3(hx, hy, -hz),
            position + new Vector3(-hx, hy, -hz),
            position + new Vector3(-hx, -hy, hz),
            position + new Vector3(hx, -hy, hz),
            position + new Vector3(hx, hy, hz),
            position + new Vector3(-hx, hy, hz),
        };

        // Triangles (12 triangles = 6 faces)
        shard.Triangles = new int[]
        {
            // Face avant
            0, 2, 1, 0, 3, 2,
            // Face arrière
            4, 5, 6, 4, 6, 7,
            // Face gauche
            0, 4, 7, 0, 7, 3,
            // Face droite
            1, 2, 6, 1, 6, 5,
            // Face bas
            0, 1, 5, 0, 5, 4,
            // Face haut
            3, 7, 6, 3, 6, 2
        };

        return shard;
    }

    /// <summary>Crée une mesh à partir d'un shard</summary>
    public static Mesh CreateMeshFromShard(Shard shard)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = shard.Vertices;
        mesh.triangles = shard.Triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
