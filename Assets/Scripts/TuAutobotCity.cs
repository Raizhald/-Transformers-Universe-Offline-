using System;
using System.IO;
using System.Collections;
using UnityEngine;

// Local recreation of the archived Autobot_Exterior_Tutorial environment.
public sealed class TuAutobotCity : MonoBehaviour
{
    [Serializable] sealed class Materials {public TuAutobotHangar.Surface[] materials=new TuAutobotHangar.Surface[0];}
    [Serializable] sealed class Sun {public Vector3 forward=Vector3.down;public Color color=Color.white;public float intensity=1;}
    [Serializable] sealed class EnvironmentData {public Color fog=Color.grey,ambient=Color.grey;public float fogDensity=.0002f;public Sun sun=new Sun();}
    public TuPackedModel Model {get;private set;}
    public TuExteriorTerrain Terrain {get;private set;}
    public int CollisionChunks {get;private set;}
    public int ParkedVehicleBlockers {get;private set;}
    EnvironmentData source;Material sky;Texture2D[] faces;AudioSource wind;
    public IEnumerator Load(TuHangarPlayer player,Action<float,string> progress)
    {
        progress(.15f,"Loading original Autobot exterior...");yield return null;
        Model=TuPackedModel.Load(Path.Combine(Application.streamingAssetsPath,"autobot-city.tupack"),"Autobot Exterior archive");Model.root.transform.SetParent(transform,false);
        foreach(Transform node in Model.nodes)node.gameObject.layer=8;
        TuCityVertexColors.Restore(Model,"autobot-city-colors.bin");
        progress(.4f,"Restoring original exterior materials...");yield return null;
        var materials=JsonUtility.FromJson<Materials>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"autobot-city-materials.json")));
        player.autobotHangar.Apply(Model,materials.materials);
        progress(.55f,"Preparing original city collisions...");yield return null;
        using(var b=new BinaryReader(File.OpenRead(Path.Combine(Application.streamingAssetsPath,"autobot-city-collision.tupack"))))
        {
            int total=b.ReadInt32();if(total<=0||total%3!=0||total>3000000)throw new InvalidDataException("Invalid Autobot city collisions");
            for(int done=0;done<total;)
            {
                int n=Math.Min(54000,total-done);Vector3[] vertices=new Vector3[n];int[] indices=new int[n*2];
                for(int i=0;i<n;i++){vertices[i]=new Vector3(b.ReadSingle(),b.ReadSingle(),b.ReadSingle());indices[i]=i;}
                for(int i=0;i<n;i+=3){indices[n+i]=i;indices[n+i+1]=i+2;indices[n+i+2]=i+1;}
                Mesh mesh=new Mesh{name="Original Autobot city collision"};mesh.vertices=vertices;mesh.triangles=indices;mesh.RecalculateBounds();
                GameObject go=new GameObject("Autobot street collision "+CollisionChunks++);go.layer=8;go.transform.SetParent(transform,false);go.AddComponent<MeshCollider>().sharedMesh=mesh;
                done+=n;progress(.55f+.2f*done/total,"Preparing original city collisions...");yield return null;
            }
            if(b.BaseStream.Position!=b.BaseStream.Length)throw new InvalidDataException("Unexpected city collision bytes");
        }
        Terrain=gameObject.AddComponent<TuExteriorTerrain>();IEnumerator terrain=Terrain.Load(progress,"AutobotTerrain");while(terrain.MoveNext())yield return terrain.Current;
        var vehicles=gameObject.AddComponent<TuParkedVehicles>();vehicles.Restore("autobot-city-vehicles.json");ParkedVehicleBlockers=vehicles.ColliderCount;
        source=JsonUtility.FromJson<EnvironmentData>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"autobot-city-area.json")));
        GameObject sun=new GameObject("Original Autobot exterior sun");sun.transform.SetParent(transform,false);sun.transform.rotation=Quaternion.LookRotation(source.sun.forward.normalized);
        Light light=sun.AddComponent<Light>();light.type=LightType.Directional;light.color=source.sun.color;light.intensity=source.sun.intensity;
        sky=new Material(Resources.Load<Shader>("TuAutobotSky"));faces=new Texture2D[6];string[] properties={"_FrontTex","_BackTex","_LeftTex","_RightTex","_UpTex","_DownTex"};
        for(int i=0;i<6;i++){var t=new Texture2D(2,2,TextureFormat.RGBA32,false);t.LoadImage(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,"AutobotSky",properties[i]+".png")));t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;faces[i]=t;sky.SetTexture(properties[i],t);}
        AudioClip clip=Resources.Load<AudioClip>("AutobotCityAudio/HighGrand_Wind_02");if(clip!=null){wind=gameObject.AddComponent<AudioSource>();wind.playOnAwake=false;wind.clip=clip;wind.loop=true;wind.volume=.14f;wind.spatialBlend=0;}
    }
    public void Enter(Camera camera)
    {
        RenderSettings.skybox=sky;RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=source.ambient;
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=source.fog;RenderSettings.fogDensity=source.fogDensity;
        camera.clearFlags=CameraClearFlags.Skybox;camera.farClipPlane=7000;camera.backgroundColor=source.fog;if(wind!=null&&!wind.isPlaying)wind.Play();
    }
    void OnDestroy(){if(sky!=null)Destroy(sky);if(faces!=null)foreach(Texture2D face in faces)if(face!=null)Destroy(face);}
}
