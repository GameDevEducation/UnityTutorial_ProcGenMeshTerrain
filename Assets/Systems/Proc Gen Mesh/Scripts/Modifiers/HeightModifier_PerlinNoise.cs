using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightModifier_PerlinNoise : BaseModifier
{
    [SerializeField] List<PerlinOctave> HeightNoise = new List<PerlinOctave>();
    [SerializeField] Vector2 HeightNoiseScale = new Vector2(8f, 8f);

    public override void Execute(int numVertsPerSide, Vector3[] vertices, Color[] vertexColours, Vector3[] normals,
                                 Texture2D texture, ProcGenConfig config, ProcGenTile tile)
    {
        if (HeightNoise.Count == 0)
            HeightNoise.Add(new PerlinOctave() { Amplitude = 1f, Frequency = 1f });

        Vector3 originPoint = tile.transform.position;

        // apply the octaves
        for (int octave = 0; octave < HeightNoise.Count; octave++)
        {
            var octaveConfig = HeightNoise[octave];
            Vector2 currentScale = HeightNoiseScale * octaveConfig.Frequency;

            for (int row = 0; row < numVertsPerSide; row++)
            {
                float rowProgress = (originPoint.z / config.TileSize) + ((float)row / (numVertsPerSide - 1));
                rowProgress *= currentScale.x;

                for (int col = 0; col < numVertsPerSide; col++)
                {
                    float colProgress = (originPoint.x / config.TileSize) + ((float)col / (numVertsPerSide - 1));
                    colProgress *= currentScale.y;

                    int vertIndex = row * numVertsPerSide + col;

                    float height = config.MaxHeight * Mathf.PerlinNoise(rowProgress, colProgress);
                    vertices[vertIndex].y += height * octaveConfig.Amplitude;
                }
            }
        }
    }
}
