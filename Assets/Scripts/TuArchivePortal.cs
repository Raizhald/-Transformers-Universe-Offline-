using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public sealed class TuArchivePortal : MonoBehaviour
{
    [Serializable] sealed class SourceMaterial {public string name="",file="";public bool multiply=false;public Color tint=Color.white;public Vector2 scale=Vector2.one,offset=Vector2.zero;}
    [Serializable] sealed class Subset {public int[] indices=new int[0];}
    [Serializable] sealed class Part {public string name="";public Vector3[] vertices=new Vector3[0];public Vector2[] uv=new Vector2[0];public Color[] colors=new Color[0];public Subset[] subsets=new Subset[0];public SourceMaterial[] materials=new SourceMaterial[0];public Vector2 scroll=Vector2.zero;}
    [Serializable] sealed class Catalog {public Part[] parts=new Part[0];}
    public static void Create(Transform parent,string file="original-portal.json")
    {
        var data=JsonUtility.FromJson<Catalog>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,file)));var textures=new Dictionary<string,Texture2D>();
        foreach(Part p in data.parts)
        {
            var go=new GameObject(p.name);go.transform.SetParent(parent,false);var mesh=new Mesh();mesh.name=p.name;mesh.vertices=p.vertices;mesh.uv=p.uv;if(p.colors.Length==p.vertices.Length)mesh.colors=p.colors;mesh.subMeshCount=p.subsets.Length;for(int k=0;k<p.subsets.Length;k++)mesh.SetTriangles(p.subsets[k].indices,k);mesh.RecalculateNormals();mesh.RecalculateBounds();go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var materials=new Material[p.materials.Length];for(int k=0;k<materials.Length;k++){var s=p.materials[k];var m=new Material(Resources.Load<Shader>(s.multiply?"TuPortalMultiply":"TuPortalArchive"));m.name=s.name;Texture2D texture;if(!textures.TryGetValue(s.file,out texture)){texture=new Texture2D(2,2,TextureFormat.RGBA32,true);texture.LoadImage(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,s.file)));texture.wrapMode=TextureWrapMode.Repeat;textures[s.file]=texture;}m.mainTexture=texture;m.mainTextureScale=s.scale;m.mainTextureOffset=s.offset;m.SetColor("_TintColor",s.tint);m.SetVector("_Scroll",new Vector4(p.scroll.x,p.scroll.y,0,0));materials[k]=m;}go.AddComponent<MeshRenderer>().sharedMaterials=materials;
        }
    }
}
