using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ETerrainResolution
{
    Size_64x64 = 64,
    Size_128x128 = 128,
    Size_256x256 = 256,
    Size_512x512 = 512,
}

public enum ETextureResolution
{
    Resolution_64x64 = 64,
    Resolution_128x128 = 128,
    Resolution_256x256 = 256,
    Resolution_512x512 = 512
}

public enum EPaintingMode
{
    VertexColour,
    TextureBased
}

[CreateAssetMenu(menuName = "ProcGen/Config", fileName = "ProcGenConfig")]
public class ProcGenConfig : ScriptableObject
{
    [Header("Common")]
    public float TileSize = 200f;
    public ETerrainResolution TerrainResolution = ETerrainResolution.Size_64x64;

    [Header("Height")]
    public float MaxHeight = 100f;

    [Header("Painting")]
    public EPaintingMode PaintingMode = EPaintingMode.VertexColour;
    public ETextureResolution TextureResolution = ETextureResolution.Resolution_512x512;
}
