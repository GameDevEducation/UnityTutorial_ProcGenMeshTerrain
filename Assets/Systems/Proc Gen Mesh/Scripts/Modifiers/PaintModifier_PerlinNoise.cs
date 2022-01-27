using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ColouredPerlinNoiseConfig
{
    public Color NoiseColour;
    public float Scale = 1f;
    [Range(0f, 1f)] public float Intensity = 1f;
    [Range(0f, 1f)] public float Threshold = 1f;
}

public class PaintModifier_PerlinNoise : BaseModifier
{
    [SerializeField] Color BaseColour;
    [SerializeField] List<ColouredPerlinNoiseConfig> Configs;

    public override void Execute(int numVertsPerSide, Vector3[] vertices, Color[] vertexColours, Vector3[] normals,
                                 Texture2D texture, ProcGenConfig config, ProcGenTile tile)
    {
        Vector3 originPoint = tile.transform.position;

        for (int row = 0; row < numVertsPerSide; row++)
        {
            float rowProgress = (originPoint.z / config.TileSize) + ((float)row / (numVertsPerSide - 1));

            for (int col = 0; col < numVertsPerSide; col++)
            {
                float colProgress = (originPoint.x / config.TileSize) + ((float)col / (numVertsPerSide - 1));
                int vertIndex = row * numVertsPerSide + col;

                Color vertexColour = BaseColour;

                // apply each colour
                foreach(var colourConfig in Configs)
                {
                    float noise = Mathf.PerlinNoise(colourConfig.Scale * rowProgress,
                                                    colourConfig.Scale * colProgress);

                    if (noise >= colourConfig.Threshold)
                        vertexColour = Color.Lerp(vertexColour, colourConfig.NoiseColour, colourConfig.Intensity);
                }

                // paint based on height
                vertexColours[vertIndex] = vertexColour;
            }
        }
    }
}
