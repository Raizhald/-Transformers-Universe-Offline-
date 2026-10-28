using UnityEngine;
using UnityEditor;

public sealed class TuUiTextureImport : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if(!assetPath.StartsWith("Assets/Resources/Hud/")&&!assetPath.StartsWith("Assets/Resources/FrontEnd/")&&!assetPath.StartsWith("Assets/Resources/Signage/"))return;
        var t=(TextureImporter)assetImporter;t.npotScale=TextureImporterNPOTScale.None;t.mipmapEnabled=false;t.alphaIsTransparency=true;t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;t.textureCompression=TextureImporterCompression.Uncompressed;t.maxTextureSize=8192;
    }
}
