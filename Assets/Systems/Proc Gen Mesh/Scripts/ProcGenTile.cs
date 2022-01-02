using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PerlinOctave
{
    public float Amplitude = 1f;
    public float Frequency = 1f;
}

[RequireComponent(typeof(MeshFilter))]
public class ProcGenTile : MonoBehaviour
{
    public enum EResolution
    {
        Size_64x64      = 64,
        Size_128x128    = 128,
        Size_256x256    = 256,
        Size_512x512    = 512,
    }

    [SerializeField] float TileSize = 200f;
    [SerializeField] EResolution Resolution = EResolution.Size_64x64;
    [SerializeField] float MaxHeight = 100f;

    [SerializeField] List<PerlinOctave> HeightNoise = new List<PerlinOctave>();
    [SerializeField] Vector2 HeightNoiseScale = new Vector2(8f, 8f);

    MeshFilter LinkedMeshFilter;

    private void Awake()
    {
        LinkedMeshFilter = GetComponent<MeshFilter>();
    }

    // Start is called before the first frame update
    void Start()
    {
        PerformMeshGeneration();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PerformMeshGeneration()
    {
        Vector3[] vertices;
        Vector2[] meshUVs;
        Color[] vertexColours;
        int[] triangleIndices;

        int numVertsPerSide = (int)Resolution;
        int totalNumVerts = numVertsPerSide * numVertsPerSide;
        float vertexSpacing = TileSize / (numVertsPerSide - 1);
        float halfTileSize = TileSize * 0.5f;

        // setup our arrays
        vertices = new Vector3[totalNumVerts];
        meshUVs = new Vector2[totalNumVerts];
        vertexColours = new Color[totalNumVerts];
        triangleIndices = new int[(numVertsPerSide - 1) * (numVertsPerSide - 1) * 2 * 3];

        // generate the base mesh
        for (int row = 0; row < numVertsPerSide; row++)
        {
            for (int col = 0; col < numVertsPerSide; col++)
            {
                int vertIndex = (row * numVertsPerSide) + col;

                // set the vertex position
                vertices[vertIndex] = new Vector3((col * vertexSpacing) - halfTileSize,
                                                  0f,
                                                  (row * vertexSpacing) - halfTileSize);

                // set the initial colour
                vertexColours[vertIndex] = Color.green;

                // set the indices
                if (row < (numVertsPerSide - 1) && col < (numVertsPerSide - 1))
                {
                    int baseIndex = (row * (numVertsPerSide - 1)) + col;

                    triangleIndices[baseIndex * 6 + 0] = vertIndex;
                    triangleIndices[baseIndex * 6 + 1] = vertIndex + numVertsPerSide;
                    triangleIndices[baseIndex * 6 + 2] = vertIndex + numVertsPerSide + 1;

                    triangleIndices[baseIndex * 6 + 3] = vertIndex;
                    triangleIndices[baseIndex * 6 + 4] = vertIndex + numVertsPerSide + 1;
                    triangleIndices[baseIndex * 6 + 5] = vertIndex + 1;
                }
            }
        }

        // Modify the terrain height
        PerformMeshGeneration_Height(numVertsPerSide, vertices);

        // generate the UVs
        for (int vertIndex = 0; vertIndex < totalNumVerts; ++vertIndex)
        {
            meshUVs[vertIndex] = new Vector2((vertices[vertIndex].x + halfTileSize) / TileSize, 
                                             (vertices[vertIndex].z + halfTileSize) / TileSize);
        }

        // Create the mesh
        Mesh mesh = new Mesh();

        // set the geometry
        mesh.indexFormat = triangleIndices.Length > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : 
                                                            UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, meshUVs);
        mesh.SetTriangles(triangleIndices, 0);

        // recalculate bounds and normals
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        // TODO - paint the mesh
        PerformMeshGeneration_Paint(numVertsPerSide, vertices, vertexColours, mesh.normals);

        mesh.SetColors(vertexColours);

        LinkedMeshFilter.mesh = mesh;
    }

    void PerformMeshGeneration_Height(int numVertsPerSide, Vector3[] vertices)
    {
        if (HeightNoise.Count == 0)
            HeightNoise.Add(new PerlinOctave() { Amplitude = 1f, Frequency = 1f });

        // apply the octaves
        for (int octave = 0; octave < HeightNoise.Count; octave++)
        {
            var octaveConfig = HeightNoise[octave];
            Vector2 currentScale = HeightNoiseScale * octaveConfig.Frequency;

            for (int row = 0; row < numVertsPerSide; row++)
            {
                float rowProgress = (transform.position.z / TileSize) + ((float)row / (numVertsPerSide - 1));
                rowProgress *= currentScale.x;

                for (int col = 0; col < numVertsPerSide; col++)
                {
                    float colProgress = (transform.position.x / TileSize) + ((float)col / (numVertsPerSide - 1));
                    colProgress *= currentScale.y;

                    int vertIndex = row * numVertsPerSide + col;

                    float height = MaxHeight * Mathf.PerlinNoise(rowProgress, colProgress);
                    vertices[vertIndex].y += height * octaveConfig.Amplitude;
                }
            }
        }
    }

    void PerformMeshGeneration_Paint(int numVertsPerSide, Vector3[] vertices, Color[] vertexColours, Vector3[] normals)
    {
        for (int row = 0; row < numVertsPerSide; row++)
        {
            for (int col = 0; col < numVertsPerSide; col++)
            {
                int vertIndex = row * numVertsPerSide + col;

                float heightProgress = vertices[vertIndex].y / MaxHeight;

                // paint based on height
                vertexColours[vertIndex] = Color.Lerp(Color.green, Color.white, heightProgress);

                // paint based on steepness
                //vertexColours[vertIndex] = Color.Lerp(Color.red, Color.white, normals[vertIndex].y);
            }
        }
    }
}
