using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintModifier_SimpleHeight : BaseModifier
{
    public override void Execute(int numVertsPerSide, Vector3[] vertices, Color[] vertexColours, Vector3[] normals,
                                 Texture2D texture, ProcGenConfig config, ProcGenTile tile)
    {
        for (int row = 0; row < numVertsPerSide; row++)
        {
            for (int col = 0; col < numVertsPerSide; col++)
            {
                int vertIndex = row * numVertsPerSide + col;

                float heightProgress = vertices[vertIndex].y / config.MaxHeight;

                // paint based on height
                vertexColours[vertIndex] = Color.Lerp(Color.green, Color.white, heightProgress);
            }
        }
    }
}
