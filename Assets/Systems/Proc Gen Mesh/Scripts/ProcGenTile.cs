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
    [SerializeField] ProcGenConfig Config;
    [SerializeField] GameObject Modifiers;

    float TileSize => Config.TileSize;
    ETerrainResolution TerrainResolution => Config.TerrainResolution;
    float MaxHeight => Config.MaxHeight;
    EPaintingMode PaintingMode => Config.PaintingMode;
    ETextureResolution TextureResolution => Config.TextureResolution;

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

        int numVertsPerSide = (int)TerrainResolution;
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

        // generate the UVs
        for (int vertIndex = 0; vertIndex < totalNumVerts; ++vertIndex)
        {
            meshUVs[vertIndex] = new Vector2((vertices[vertIndex].x + halfTileSize) / TileSize, 
                                             (vertices[vertIndex].z + halfTileSize) / TileSize);
        }

        var allModifiers = Modifiers.GetComponents<BaseModifier>();

        // run the initial generation
        foreach(var modifier in allModifiers)
        {
            if (modifier.Phase == EProcGenPhase.Initial)
                modifier.Execute(numVertsPerSide, vertices, vertexColours, null, null, Config, this);
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

        // after the normal generation
        var vertexNormals = mesh.normals;
        foreach (var modifier in allModifiers)
        {
            if (modifier.Phase == EProcGenPhase.AfterNormals)
                modifier.Execute(numVertsPerSide, vertices, vertexColours, vertexNormals, null, Config, this);
        }

        mesh.SetColors(vertexColours);

        Texture2D finalTexture = null;
        if (PaintingMode == EPaintingMode.TextureBased)
        {
            // generate the initial texture
            Texture2D initialTexture = new Texture2D(numVertsPerSide, numVertsPerSide, TextureFormat.RGB24, true);
            initialTexture.SetPixels(vertexColours);
            initialTexture.Apply();

            finalTexture = initialTexture;

            // texture and terrain resolution are different?
            if ((int)TextureResolution != (int)TerrainResolution)
            {
                // setup the temporary render texture and switch to it
                RenderTexture targetRT = new RenderTexture((int)TextureResolution, (int)TextureResolution, 24);
                var previousRT = RenderTexture.active;
                RenderTexture.active = targetRT;

                Graphics.Blit(initialTexture, targetRT);

                // grab the texture
                finalTexture = new Texture2D((int)TextureResolution, (int)TextureResolution, TextureFormat.RGB24, true);
                finalTexture.ReadPixels(new Rect(0, 0, (int)TextureResolution, (int)TextureResolution), 0, 0);
                finalTexture.Apply();

                finalTexture.filterMode = FilterMode.Bilinear;
                finalTexture.wrapMode = TextureWrapMode.Clamp;

                RenderTexture.active = previousRT;

                LinkedMeshFilter.GetComponent<MeshRenderer>().material.SetTexture("_BaseMap", finalTexture);
            }
        }

        // final post processing
        foreach (var modifier in allModifiers)
        {
            if (modifier.Phase == EProcGenPhase.PostProcess)
                modifier.Execute(numVertsPerSide, vertices, vertexColours, vertexNormals, finalTexture, Config, this);
        }

        LinkedMeshFilter.mesh = mesh;
    }
}
