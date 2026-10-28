using System;
using System.IO;
using UnityEngine;

// Contours are sampled from exact inner-rim vertices in the archived door mesh.
public sealed class TuHangarDetails : MonoBehaviour
{
    [Serializable] sealed class Door {public string name="";public Vector3 center=Vector3.zero,normal=Vector3.forward;public Vector3[] vertices=new Vector3[0];public Color tint=Color.white;}
    [Serializable] sealed class Catalog {public Door[] doors=new Door[0];}
    public static void Create()
    {
        var owner=new GameObject("Archive signs and restricted doors").AddComponent<TuHangarDetails>();
        var data=JsonUtility.FromJson<Catalog>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"doorways.json")));
        foreach(Door door in data.doors)owner.Shield(door);
        owner.gameObject.AddComponent<TuHangarSigns>();
    }
    void Shield(Door door)
    {
        if(door.vertices.Length!=6)throw new InvalidDataException("Door contour must contain six source vertices");
        var go=new GameObject(door.name);go.transform.parent=transform;go.transform.position=door.center;go.transform.rotation=Quaternion.LookRotation(door.normal,Vector3.up);go.layer=8;
        var block=go.AddComponent<BoxCollider>();block.size=new Vector3(18.5f,22,1.2f);
        Vector3[] vertices=new Vector3[7];Vector2[] uv=new Vector2[7];int[] indices=new int[18];uv[0]=new Vector2(.5f,.5f);
        for(int k=0;k<6;k++)
        {
            vertices[k+1]=go.transform.InverseTransformPoint(door.vertices[k]);uv[k+1]=new Vector2(vertices[k+1].x/18+.5f,vertices[k+1].y/14+.5f);
            indices[k*3]=0;indices[k*3+1]=k+1;indices[k*3+2]=(k+1)%6+1;
        }
        var mesh=new Mesh();mesh.name="Archive inner door rim";mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=indices;mesh.RecalculateNormals();mesh.RecalculateBounds();go.AddComponent<MeshFilter>().sharedMesh=mesh;
        var material=new Material(Resources.Load<Shader>("TuHexShield"));var texture=new Texture2D(2,2,TextureFormat.RGBA32,true);texture.LoadImage(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,"Barrier_MainTex.png")));texture.wrapMode=TextureWrapMode.Repeat;material.mainTexture=texture;material.SetColor("_Tint",door.tint);go.AddComponent<MeshRenderer>().sharedMaterial=material;
        // Solid collision stays active when decorative effects are disabled.
    }
}
