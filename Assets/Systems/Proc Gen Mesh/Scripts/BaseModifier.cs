using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EProcGenPhase
{
    Initial,
    AfterNormals,
    PostProcess
}

public abstract class BaseModifier : MonoBehaviour
{
    [SerializeField] EProcGenPhase _Phase;
    [SerializeField] [Range(0f, 1f)] float Intensity = 1f;

    public EProcGenPhase Phase => _Phase;

    public abstract void Execute(int numVertsPerSide, Vector3[] vertices, Color[] vertexColours, Vector3[] normals,
                                 Texture2D texture, ProcGenConfig config, ProcGenTile tile);
}
