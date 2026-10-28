using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

// Channel meanings are recorded in the archived ReflectiveSpecularMat2 shader:
// red = gloss, green = two-tone mask, blue = emission. Lighting is reconstructed.
public static class TuOriginalMaterials
{
    [Serializable] sealed class Entry { public string name="",sourceShader="",detailFile="",diffuseFile="";public Vector2 scale=Vector2.one,offset=Vector2.zero,detailScale=Vector2.one,detailOffset=Vector2.zero;public float shininess=.2f,reflectivity=0;public Color emission=Color.black,color=Color.white; }
    [Serializable] sealed class Catalog { public Entry[] materials=new Entry[0]; }
    public static void Apply(TuPackedModel model)
    {
        Catalog catalog=JsonUtility.FromJson<Catalog>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"original-materials.json")));
        var entries=new Dictionary<string,Entry>();foreach(Entry e in catalog.materials)entries[e.name]=e;
        var visited=new HashSet<Material>();var textures=new Dictionary<string,Texture2D>();int restored=0;
        foreach(Renderer r in model.root.GetComponentsInChildren<Renderer>())foreach(Material m in r.sharedMaterials)
        {
            Entry e;if(!visited.Add(m)||!entries.TryGetValue(m.name,out e))continue;
            if(!string.IsNullOrEmpty(e.diffuseFile)){Texture2D diffuse;if(!textures.TryGetValue(e.diffuseFile,out diffuse)){diffuse=new Texture2D(2,2,TextureFormat.RGBA32,true);diffuse.LoadImage(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,e.diffuseFile)));diffuse.wrapMode=TextureWrapMode.Repeat;diffuse.anisoLevel=8;textures[e.diffuseFile]=diffuse;}m.mainTexture=diffuse;}
            m.color=e.color;
            if(e.sourceShader=="TransparentBackground"){m.shader=Resources.Load<Shader>("TuArchiveBackdrop");m.color=e.color;}
            m.mainTextureScale=e.scale;m.mainTextureOffset=e.offset;
            if(!e.sourceShader.Contains("Reflect"))continue;
            m.SetFloat("_Glossiness",Mathf.Clamp01(.2f+.6f*Mathf.Sqrt(e.shininess)));
            m.SetFloat("_GlossFromAlpha",string.IsNullOrEmpty(e.detailFile)?1:0);
            m.SetFloat("_Metallic",Mathf.Clamp(e.reflectivity*.35f,0,.35f));
            if(!string.IsNullOrEmpty(e.detailFile))
            {
                Texture2D t;if(!textures.TryGetValue(e.detailFile,out t)){t=new Texture2D(2,2,TextureFormat.RGBA32,true,true);t.LoadImage(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,e.detailFile)));t.wrapMode=TextureWrapMode.Repeat;t.anisoLevel=8;textures[e.detailFile]=t;}
                m.SetTexture("_DetailTex",t);m.SetTextureScale("_DetailTex",e.detailScale);m.SetTextureOffset("_DetailTex",e.detailOffset);m.SetFloat("_UseDetail",1);m.SetColor("_EmissionColor",e.emission);
            }
            restored++;
        }
        Debug.Log("TU_ORIGINAL_MATERIALS reflective="+restored+" detailTextures="+textures.Count);
    }
}
