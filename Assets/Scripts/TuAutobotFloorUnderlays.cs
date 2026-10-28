using System;
using System.IO;
using UnityEngine;

// Visible underlays follow existing source FloorCOL tops. Existing road meshes stay above them.
public sealed class TuAutobotFloorUnderlays : MonoBehaviour
{
    [Serializable] sealed class Surface {public int sourceCollider=0;public bool addCollision=false;public string name="";public Vector3[] vertices=new Vector3[0];}
    [Serializable] sealed class Catalog {public string material="";public Surface[] surfaces=new Surface[0];}
    public int SurfaceCount {get;private set;}
    public void Restore(TuPackedModel hangar)
    {
        var data=JsonUtility.FromJson<Catalog>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"autobot-floor-underlays.json")));
        Material source=null;foreach(Renderer r in hangar.root.GetComponentsInChildren<Renderer>())foreach(Material m in r.sharedMaterials)if(m.name==data.material)source=m;
        if(source==null)throw new InvalidDataException("Original Autobot floor material missing");
        Material floor=new Material(source);floor.name="Recovered asphalt on supported floor underlays";floor.mainTextureScale=Vector2.one;floor.mainTextureOffset=Vector2.zero;
        foreach(Surface item in data.surfaces)
        {
            if(item.vertices.Length<3)throw new InvalidDataException("Invalid source FloorCOL top");
            var mesh=new Mesh{name="Source FloorCOL "+item.sourceCollider};mesh.vertices=item.vertices;
            var uv=new Vector2[item.vertices.Length];var colors=new Color[item.vertices.Length];for(int i=0;i<colors.Length;i++)colors[i]=Color.white;for(int i=0;i<uv.Length;i++)uv[i]=new Vector2(item.vertices[i].x,item.vertices[i].z)/8;
            mesh.uv=uv;mesh.colors=colors;var triangles=new int[(item.vertices.Length-2)*3];for(int i=0;i<item.vertices.Length-2;i++){triangles[i*3]=0;triangles[i*3+1]=i+2;triangles[i*3+2]=i+1;}mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
            GameObject go=new GameObject("Supported floor underlay "+item.sourceCollider);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=floor;if(item.addCollision){go.layer=8;go.AddComponent<MeshCollider>().sharedMesh=mesh;}SurfaceCount++;
        }
    }
}
