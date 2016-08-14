using UnityEngine;
using System.Collections;
using UnityEditor;

public class PostProcessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        var imp = (TextureImporter) assetImporter;
        if (assetPath.Contains("_point"))
            imp.filterMode = FilterMode.Point;
        if (assetPath.Contains("_decal"))
        {
            imp.isReadable = true;
            imp.textureFormat = TextureImporterFormat.ARGB32;
            imp.textureType = TextureImporterType.Advanced;
        }
    }
}
