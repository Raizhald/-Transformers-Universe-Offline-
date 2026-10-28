using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public sealed class TuPrototypeBootstrap : MonoBehaviour
{
    public static readonly Vector3 Spawn=new Vector3(-577.46545f,139.08319f,-370.12122f);
    void Awake()
    {
        Application.targetFrameRate=90;Application.runInBackground=true;
        new GameObject("Startup flow").AddComponent<TuFrontEnd>().Initialize(this);
    }
    public IEnumerator LoadHangar(string commander,string faction,Action<float,string> progress)
    {
        if(faction=="Autobots")return LoadAutobotHangar(commander,progress);
        if(faction!="Decepticons")throw new ArgumentException("Unknown faction: "+faction);
        return LoadHangar(commander,progress);
    }
    public IEnumerator LoadHangar(string commander,Action<float,string> progress)
    {
        Application.targetFrameRate=90;Application.runInBackground=true;
        var existingRoots=new HashSet<GameObject>(SceneManager.GetActiveScene().GetRootGameObjects());
        progress(.05f,"Loading Decepticon baseâ€¦");yield return null;
        RenderSettings.ambientLight=new Color(.10f,.11f,.16f);
        RenderSettings.fog=true;RenderSettings.fogColor=new Color(.045f,.035f,.07f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0018f;
        TuPackedModel hangar=TuPackedModel.Load(Path.Combine(Application.streamingAssetsPath,"hangar.tupack"),"Decepticon Hangar");
        foreach(Transform t in hangar.nodes)t.gameObject.layer=8;
        TuOriginalMaterials.Apply(hangar);
        progress(.35f,"Restoring base effectsâ€¦");yield return null;
        TuHangarEffects effects=new GameObject("Base effects controller").AddComponent<TuHangarEffects>();effects.Setup(hangar);
        progress(.55f,"Preparing hangar collisionsâ€¦");yield return null;
        using(BinaryReader b=new BinaryReader(File.OpenRead(Path.Combine(Application.streamingAssetsPath,"collision.tupack"))))
        {
            int n=b.ReadInt32();if(n<0||n>2000000||n%3!=0)throw new InvalidDataException("Invalid collision mesh");
            Vector3[] verts=new Vector3[n];int[] indices=new int[n*2];
            for(int k=0;k<n;k++){verts[k]=new Vector3(b.ReadSingle(),b.ReadSingle(),b.ReadSingle());indices[k]=k;}
            for(int k=0;k<n;k+=3){indices[n+k]=k;indices[n+k+1]=k+2;indices[n+k+2]=k+1;}
            Mesh m=new Mesh();m.name="Original archive collisions";m.vertices=verts;m.triangles=indices;m.RecalculateBounds();
            GameObject col=new GameObject("Hangar collisions");col.layer=8;col.AddComponent<MeshCollider>().sharedMesh=m;
        }
        MakeLight("Hangar fill",new Vector3(-519,157,-406),new Color(.4f,.22f,.8f),75,.9f);
        TuHangarDetails.Create();
        MakeLight("Hangar entry",new Vector3(-571,146,-370),new Color(.42f,.53f,1),35,1.5f);
        MakeLight("Hangar exit",new Vector3(-458,146,-370),new Color(.55f,.25f,1),35,1.2f);
        GameObject sun=new GameObject("Ceiling light");Light dl=sun.AddComponent<Light>();dl.type=LightType.Directional;dl.color=new Color(.66f,.7f,.9f);dl.intensity=.35f;sun.transform.rotation=Quaternion.Euler(48,-35,0);
        var environment=new List<GameObject>();foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())if(!existingRoots.Contains(root))environment.Add(root);
        progress(.7f,"Loading A004â€¦");yield return null;
        GameObject player=new GameObject("A004 Player");player.layer=9;player.transform.position=Spawn;
        CharacterController cc=player.AddComponent<CharacterController>();cc.height=3.3f;cc.radius=.7f;cc.center=new Vector3(0,1.65f,0);cc.stepOffset=.45f;cc.slopeLimit=50;
#if UNITY_5_3_OR_NEWER
        cc.skinWidth=.04f;
#endif
        TuPackedModel robot=TuPackedModel.Load(Path.Combine(Application.streamingAssetsPath,"robot.tupack"),"A004 Visual");robot.root.transform.parent=player.transform;robot.root.transform.localPosition=Vector3.zero;robot.root.transform.localRotation=Quaternion.Euler(0,270,0);
        TuHangarPlayer movement=player.AddComponent<TuHangarPlayer>();movement.model=robot;movement.effects=effects;
        player.AddComponent<TuRobotAudio>().player=movement;
        movement.enabled=false;
        TuHud hud=new GameObject("Transformers Universe HUD").AddComponent<TuHud>();hud.player=movement;movement.hud=hud;hud.PlayerName=commander;hud.enabled=false;
        var travel=new GameObject("World travel").AddComponent<TuWorldTravel>();movement.travel=travel;travel.Initialize(movement,environment.ToArray());
        var vehicle=player.AddComponent<TuVehicleMode>();movement.vehicle=vehicle;vehicle.Initialize(movement);
        progress(.95f,"Initializing commander interfaceâ€¦");yield return null;
        movement.enabled=true;hud.enabled=true;yield return null;
        progress(1,"Hangar ready");yield return new WaitForSecondsRealtime(.35f);
        Debug.Log("TU_READY hangar=Decepticon robot=A004 meshes=357 animations=Idle,Walk,Run,Fall");
    }
    IEnumerator LoadAutobotHangar(string commander,Action<float,string> progress)
    {
        var existingRoots=new HashSet<GameObject>(SceneManager.GetActiveScene().GetRootGameObjects());
        progress(.05f,"Loading Autobot baseâ€¦");yield return null;
        TuPackedModel hangar=TuPackedModel.Load(Path.Combine(Application.streamingAssetsPath,"autobot-hangar.tupack"),"Autobot Hangar");
        foreach(Transform node in hangar.nodes)node.gameObject.layer=8;
        progress(.3f,"Restoring Autobot materials and hologramâ€¦");yield return null;
        TuAutobotHangar baseData=new GameObject("Autobot base restoration").AddComponent<TuAutobotHangar>();baseData.Setup(hangar);
        progress(.5f,"Preparing original hangar collisionsâ€¦");yield return null;
        using(var reader=new BinaryReader(File.OpenRead(Path.Combine(Application.streamingAssetsPath,"autobot-collision.tupack"))))
        {
            int n=reader.ReadInt32();if(n<0||n>2000000||n%3!=0)throw new InvalidDataException("Invalid Autobot collisions");
            Vector3[] vertices=new Vector3[n];int[] indices=new int[n*2];
            for(int k=0;k<n;k++){vertices[k]=new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());indices[k]=k;}
            for(int k=0;k<n;k+=3){indices[n+k]=k;indices[n+k+1]=k+2;indices[n+k+2]=k+1;}
            var mesh=new Mesh();mesh.name="Original Autobot BoxCollider and MeshCollider surfaces";
            if(n>65535)mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices=vertices;mesh.triangles=indices;mesh.RecalculateBounds();
            GameObject collision=new GameObject("Autobot hangar collisions");collision.layer=8;collision.AddComponent<MeshCollider>().sharedMesh=mesh;
        }
        var environment=new List<GameObject>();foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())if(!existingRoots.Contains(root))environment.Add(root);
        progress(.7f,"Loading Catapult (A001)â€¦");yield return null;
        GameObject player=new GameObject("Catapult A001 Player");player.layer=9;player.transform.position=baseData.Source.spawn;
        CharacterController cc=player.AddComponent<CharacterController>();cc.height=3.3f;cc.radius=.7f;cc.center=new Vector3(0,1.65f,0);cc.stepOffset=.45f;cc.slopeLimit=50;cc.skinWidth=.04f;
        TuPackedModel robot=TuPackedModel.Load(Path.Combine(Application.streamingAssetsPath,"autobot-robot.tupack"),"Catapult A001 Visual");
        baseData.Apply(robot);robot.root.transform.SetParent(player.transform,false);robot.root.transform.localRotation=Quaternion.Euler(0,270,0);
        TuHangarPlayer movement=player.AddComponent<TuHangarPlayer>();movement.model=robot;movement.autobotHangar=baseData;movement.HangarSpawn=baseData.Source.spawn;movement.MinimumHangarY=-10;movement.enabled=false;
        var audio=player.AddComponent<TuAutobotAudio>();audio.player=movement;audio.hangar=baseData;
        TuHud hud=new GameObject("Autobot commander HUD").AddComponent<TuHud>();hud.player=movement;movement.hud=hud;hud.PlayerName=commander;hud.FactionName="Autobots";hud.RobotName="Catapult (A001)";hud.MapNorthUp=true;hud.enabled=false;
        hud.SetAreaMap("AutobotMinimap.png",baseData.Source.mapBounds,baseData.Source.portal);
        var travel=new GameObject("Autobot world travel").AddComponent<TuWorldTravel>();movement.travel=travel;travel.Initialize(movement,environment.ToArray());
        progress(.95f,"Initializing Autobot commander interfaceâ€¦");yield return null;
        movement.enabled=true;hud.enabled=true;yield return null;
        progress(1,"Autobot hangar ready");yield return new WaitForSecondsRealtime(.35f);
        Debug.Log("TU_READY hangar=Autobot robot=Catapult/A001 meshes=639 animations=Idle,Walk,Run,Fall");
    }
    static void MakeLight(string name,Vector3 pos,Color color,float range,float energy){GameObject go=new GameObject(name);go.transform.position=pos;Light l=go.AddComponent<Light>();l.type=LightType.Point;l.color=color;l.range=range;l.intensity=energy;}
}
public sealed class TuErrorScreen : MonoBehaviour
{
    public string message;
    void OnGUI(){GUI.Box(new Rect(20,20,Screen.width-40,Screen.height-40),"Unable to load project");GUI.Label(new Rect(40,60,Screen.width-80,Screen.height-100),message);}
}
