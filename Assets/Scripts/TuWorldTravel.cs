using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Local travel between recovered environments; no archived game code is executed.
public sealed class TuWorldTravel : MonoBehaviour
{
    [Serializable] public sealed class Area {public Vector3 spawn,hangarExit;public Vector4 mapBounds;public string name="DECEPTICON EXTERIOR";public float entryYaw=0;}
    [Serializable] sealed class Surface {public string name="",shader="";public Vector2 scale=Vector2.one,offset=Vector2.zero;public bool transparent=false,unlit=false;}
    [Serializable] sealed class SourceLight {public string name="";public Vector3 position=Vector3.zero;public Color color=Color.white;public float intensity=0,range=0;public int type=0;}
    [Serializable] sealed class Catalog {public Surface[] materials=new Surface[0];public SourceLight[] lights=new SourceLight[0];}
    public bool IsExterior {get;private set;}
    public bool IsLoading {get;private set;}
    public Transform HangarAudioRoot {get;private set;}
    public Vector3 ExteriorSpawn {get{return area.spawn;}}
    public Vector3 HangarExit {get{return area.hangarExit;}}
    public int ExteriorRendererCount {get;private set;}
    public int ExteriorCollisionChunks {get;private set;}
    public string LastError {get;private set;}
    Vector3 exteriorEntry;TuHangarPlayer player;Area area;GameObject[] hangar;TuPackedModel exterior;
    TuExteriorTerrain terrain;TuPackedModel recoveredBuildings,recoveredRoads;TuCityRestoration restoration;bool autobot;
    public TuExteriorTerrain ExteriorTerrain {get{return terrain;}}
    public float MinimumSafeY {get{return terrain!=null?terrain.MinimumY-15:8;}}
    GameObject cityRoot;bool previousCapture;float progress;string status="",error="";
    Texture2D backdrop,logo,ring1,ring2,ring3;GUIStyle text,heading;
    public void Initialize(TuHangarPlayer target,GameObject[] environment)
    {
        player=target;autobot=target.autobotHangar!=null;area=JsonUtility.FromJson<Area>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,autobot?"shared-city-autobot-area.json":"exterior-area.json")));
        exteriorEntry=area.spawn;var roots=new List<GameObject>(environment);var audio=new GameObject("Hangar ambient audio");HangarAudioRoot=audio.transform;roots.Add(audio);hangar=roots.ToArray();
        backdrop=Resources.Load<Texture2D>(autobot?"FrontEnd/Game_Loading_Screen_Test":"FrontEnd/Game_Loading_Screen_Decepticon");logo=Resources.Load<Texture2D>("FrontEnd/TitleLogo");
        ring1=Resources.Load<Texture2D>("FrontEnd/Loadingwheel_01");ring2=Resources.Load<Texture2D>("FrontEnd/Loadingwheel_02");ring3=Resources.Load<Texture2D>("FrontEnd/Loadingwheel_03");
    }
    bool NearExit {get{Vector3 delta=player.transform.position-area.hangarExit;return new Vector2(delta.x,delta.z).magnitude<15&&Mathf.Abs(delta.y)<16;}}
    void Update()
    {
        if(player!=null&&!player.enabled&&!IsLoading&&Input.GetKeyDown(KeyCode.F2)){UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);return;}
        if(player==null||IsLoading||player.hud==null||player.hud.Typing)return;
        if(Input.GetKeyDown(KeyCode.E)&&(IsExterior||NearExit))RequestTravel(!IsExterior);
    }
    public bool RequestTravel(bool outside)
    {
        if(IsLoading||outside==IsExterior||player==null)return false;
        StartCoroutine(Transition(outside));return true;
    }
    IEnumerator Transition(bool outside)
    {
        if(player.vehicle!=null)player.vehicle.ForceRobot();
        IsLoading=true;LastError="";error="";progress=0;previousCapture=player.MouseCaptured;player.SetMouseMode(true);player.enabled=false;player.hud.enabled=false;
        IEnumerator work=ChangeArea(outside);bool failed=false;
        while(true)
        {
            bool more;
            try{more=work.MoveNext();}catch(Exception ex){Debug.LogException(ex);LastError=ex.Message;failed=true;break;}
            if(!more)break;yield return work.Current;
        }
        if(failed)
        {
            ReleaseExterior();
            foreach(var go in hangar)if(go!=null)go.SetActive(true);
            IsExterior=false;HangarSettings();RestoreHangarMap();
            if(!ReturnPlayerToHangar()){IsLoading=false;player.enabled=false;player.hud.enabled=true;error="No clear hangar arrival point. Press F2 to return to the main menu.";yield break;}error="Unable to load the exterior. Returned to hangar. "+LastError;
        }
        IsLoading=false;player.enabled=true;player.hud.enabled=true;player.SetMouseMode(!previousCapture);
    }
    IEnumerator ChangeArea(bool outside)
    {
        status=outside?(autobot?"Loading shared city...":"Loading Decepticon Exterior..."):(autobot?"Returning to Autobot Hangar...":"Returning to Decepticon Hangar...");progress=.05f;yield return null;yield return null;
        if(outside&&exterior==null)
        {
            cityRoot=new GameObject("Exterior environment");
            status="Loading original buildings and textures...";progress=.15f;yield return null;
            exterior=TuPackedModel.Load(Path.Combine(Application.streamingAssetsPath,"exterior.tupack"),"Decepticon Exterior archive");exterior.root.transform.SetParent(cityRoot.transform,false);TuCityVertexColors.Restore(exterior,"exterior-vertex-colors.bin");
            Catalog data=JsonUtility.FromJson<Catalog>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"exterior-materials.json")));
            var source=new Dictionary<string,Surface>();foreach(var entry in data.materials)source[entry.name]=entry;
            var seen=new HashSet<Material>();int count=0;
            foreach(var node in exterior.nodes)
            {
                node.gameObject.layer=8;var renderer=node.GetComponent<MeshRenderer>();if(renderer==null)continue;
                renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;ExteriorRendererCount++;
                foreach(Material mat in renderer.sharedMaterials)if(seen.Add(mat))
                {
                    if(mat.name=="VFX_Gameplay_EnergonLift_Tunnel01_CS"){cityRoot.AddComponent<TuExteriorLift>().Restore(mat);continue;}
                    Surface s;if(!source.TryGetValue(mat.name,out s))continue;
                    if(s.transparent||s.unlit)
                    {
                        var texture=mat.mainTexture;Color color=mat.color;mat.shader=Resources.Load<Shader>("TuExteriorEffect");mat.mainTexture=texture;mat.color=color;
                        bool additive=s.shader.IndexOf("additive",StringComparison.OrdinalIgnoreCase)>=0;
                        mat.SetInt("_SrcBlend",(int)(s.transparent?BlendMode.SrcAlpha:BlendMode.One));mat.SetInt("_DstBlend",(int)(s.transparent?(additive?BlendMode.One:BlendMode.OneMinusSrcAlpha):BlendMode.Zero));mat.SetInt("_ZWrite",s.transparent?0:1);mat.renderQueue=s.transparent?3000:2000;
                    }
                    else mat.enableInstancing=true;
                    mat.mainTextureScale=s.scale;mat.mainTextureOffset=s.offset;
                }
                if(++count%400==0){progress=.35f+.15f*count/exterior.nodes.Length;yield return null;}
            }
            status="Restoring recovered building sections...";progress=.51f;yield return null;
            recoveredBuildings=TuPackedModel.Load(Path.Combine(Application.streamingAssetsPath,"exterior-buildings.tupack"),"Recovered original exterior buildings");recoveredBuildings.root.transform.SetParent(cityRoot.transform,false);
            foreach(var node in recoveredBuildings.nodes){node.gameObject.layer=8;var renderer=node.GetComponent<MeshRenderer>();if(renderer==null)continue;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;ExteriorRendererCount++;foreach(var mat in renderer.sharedMaterials)mat.enableInstancing=true;}
            status="Restoring original roads and atmosphere...";progress=.53f;yield return null;
            recoveredRoads=TuPackedModel.Load(Path.Combine(Application.streamingAssetsPath,"exterior-roads-effects.tupack"),"Original city roads and atmosphere");recoveredRoads.root.transform.SetParent(cityRoot.transform,false);
            foreach(var node in recoveredRoads.nodes){node.gameObject.layer=0;if(node.GetComponent<MeshRenderer>()!=null)ExteriorRendererCount++;}
            restoration=cityRoot.AddComponent<TuCityRestoration>();restoration.Initialize();restoration.Apply(exterior);restoration.Apply(recoveredBuildings);restoration.Apply(recoveredRoads);
            status="Preparing original street collisions...";progress=.55f;yield return null;
            using(var b=new BinaryReader(File.OpenRead(Path.Combine(Application.streamingAssetsPath,"exterior-collision.tupack"))))
            {
                int total=b.ReadInt32();if(total<0||total%3!=0||total>3000000)throw new InvalidDataException("Invalid exterior collision count");
                for(int done=0;done<total;)
                {
                    int n=Math.Min(54000,total-done);var vertices=new Vector3[n];var indices=new int[n*2];
                    for(int k=0;k<n;k++){vertices[k]=new Vector3(b.ReadSingle(),b.ReadSingle(),b.ReadSingle());indices[k]=k;}
                    for(int k=0;k<n;k+=3){indices[n+k]=k;indices[n+k+1]=k+2;indices[n+k+2]=k+1;}
                    var mesh=new Mesh{name="Original exterior collision chunk"};mesh.vertices=vertices;mesh.triangles=indices;mesh.RecalculateBounds();
                    var go=new GameObject("Exterior collision "+ExteriorCollisionChunks++);go.layer=8;go.transform.SetParent(cityRoot.transform,false);go.AddComponent<MeshCollider>().sharedMesh=mesh;
                    done+=n;progress=.55f+.25f*done/total;yield return null;
                }
                if(b.BaseStream.Position!=b.BaseStream.Length)throw new InvalidDataException("Trailing exterior collision data");
            }
            using(var b=new BinaryReader(File.OpenRead(Path.Combine(Application.streamingAssetsPath,"exterior-road-collision.tupack"))))
            {
                int n=b.ReadInt32();if(n<=0||n>60000||n%3!=0)throw new InvalidDataException("Invalid recovered road collision");var vertices=new Vector3[n];var indices=new int[n*2];for(int k=0;k<n;k++){vertices[k]=new Vector3(b.ReadSingle(),b.ReadSingle(),b.ReadSingle());indices[k]=k;}for(int k=0;k<n;k+=3){indices[n+k]=k;indices[n+k+1]=k+2;indices[n+k+2]=k+1;}var mesh=new Mesh{name="Recovered original road collision"};mesh.vertices=vertices;mesh.triangles=indices;mesh.RecalculateBounds();var go=new GameObject("Original road collision");go.layer=8;go.transform.SetParent(cityRoot.transform,false);go.AddComponent<MeshCollider>().sharedMesh=mesh;ExteriorCollisionChunks++;if(b.BaseStream.Position!=b.BaseStream.Length)throw new InvalidDataException("Unexpected road collision bytes");
            }
            terrain=cityRoot.AddComponent<TuExteriorTerrain>();IEnumerator restoreTerrain=terrain.Load(delegate(float amount,string label){progress=amount;status=label;});while(restoreTerrain.MoveNext())yield return restoreTerrain.Current;
            foreach(SourceLight entry in data.lights)
            {
                if(entry.type!=2||entry.range<=0)continue;var go=new GameObject(entry.name);go.transform.SetParent(cityRoot.transform,false);go.transform.position=entry.position;
                var light=go.AddComponent<Light>();light.type=LightType.Point;light.color=entry.color;light.range=entry.range;light.intensity=entry.intensity;light.shadows=LightShadows.None;
            }
            var parked=cityRoot.AddComponent<TuParkedVehicles>();parked.Restore();var ambience=cityRoot.AddComponent<TuCityAmbience>();ambience.Restore();
            restoration.CreateSun(cityRoot.transform);
        }
        status="Preparing destination...";progress=.9f;yield return null;
        foreach(var go in hangar)if(go!=null)go.SetActive(!outside);if(cityRoot!=null)cityRoot.SetActive(outside);
        Physics.SyncTransforms();
        if(outside)
        {
            ExteriorSettings();Vector3 spawn;
            if(!FindEntryFloor(out spawn))throw new InvalidDataException("No clear original collision floor at exterior entry");
            exteriorEntry=spawn;player.Teleport(spawn,area.entryYaw,12);player.hud.MapNorthUp=false;player.hud.SetAreaMap("ExteriorMinimap.png",area.mapBounds,area.spawn);
        }
        else{HangarSettings();if(!ReturnPlayerToHangar())throw new InvalidDataException("No supported, clear hangar arrival point");RestoreHangarMap();}
        IsExterior=outside;status=outside?(autobot?"Shared city ready":"Decepticon Exterior ready"):"Hangar ready";progress=1;yield return new WaitForSecondsRealtime(.35f);
    }
    bool FindEntryFloor(out Vector3 position){return TuSafeArrival.TryFind(area.spawn,player.GetComponent<CharacterController>(),out position);}
    public Vector3 LastHangarReturn {get;private set;}
    bool ReturnPlayerToHangar()
    {
        Vector3 safe;Vector3 preferred=autobot?player.HangarSpawn:area.hangarExit;var body=player.GetComponent<CharacterController>();
        if(!TuSafeArrival.TryFind(preferred,body,out safe)&&!TuSafeArrival.TryFind(player.HangarSpawn,body,out safe))return false;
        LastHangarReturn=safe;player.Teleport(safe,autobot?90:180,12);return true;
    }
    void ReleaseExterior()
    {
        GameObject partial=exterior!=null?exterior.root:GameObject.Find("Decepticon Exterior archive");ReleasePack(partial);if(recoveredBuildings!=null)ReleasePack(recoveredBuildings.root);if(recoveredRoads!=null)ReleasePack(recoveredRoads.root);
        if(cityRoot!=null){cityRoot.SetActive(false);foreach(var col in cityRoot.GetComponentsInChildren<MeshCollider>(true))if(col.sharedMesh!=null)Destroy(col.sharedMesh);Destroy(cityRoot);}
        cityRoot=null;exterior=null;recoveredBuildings=null;recoveredRoads=null;terrain=null;restoration=null;ExteriorRendererCount=0;ExteriorCollisionChunks=0;
    }
    static void ReleasePack(GameObject root)
    {
        if(root==null)return;var meshes=new HashSet<Mesh>();var materials=new HashSet<Material>();var textures=new HashSet<Texture>();foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))if(filter.sharedMesh!=null)meshes.Add(filter.sharedMesh);foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))foreach(var material in renderer.sharedMaterials)if(material!=null)materials.Add(material);foreach(var material in materials)if(material.mainTexture!=null)textures.Add(material.mainTexture);foreach(var mesh in meshes)Destroy(mesh);foreach(var material in materials)Destroy(material);foreach(var texture in textures)Destroy(texture);Destroy(root);
    }
    void ExteriorSettings(){restoration.Enter(player.ViewCamera);var ambience=cityRoot.GetComponent<TuCityAmbience>();if(ambience!=null)ambience.Resume();}
    void HangarSettings(){if(autobot){player.autobotHangar.RestoreEnvironment(player.ViewCamera);return;}RenderSettings.skybox=null;RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.10f,.11f,.16f);RenderSettings.fogColor=new Color(.045f,.035f,.07f);RenderSettings.fogDensity=.0018f;if(player.ViewCamera!=null){player.ViewCamera.clearFlags=CameraClearFlags.SolidColor;player.ViewCamera.farClipPlane=1000;player.ViewCamera.backgroundColor=new Color(.035f,.028f,.06f);}}
    void RestoreHangarMap(){player.hud.MapNorthUp=autobot;if(autobot)player.hud.SetAreaMap("AutobotMinimap.png",player.autobotHangar.Source.mapBounds,player.autobotHangar.Source.portal);else player.hud.SetAreaMap("OriginalMinimap.png",new Vector4(-673.6888f,-510.5042f,309.6907f,285.9803f),new Vector3(-519,168,-490));}
    public void ResetCurrentArea(){if(autobot&&!IsExterior){ReturnPlayerToHangar();return;}player.Teleport(IsExterior?exteriorEntry:player.HangarSpawn,IsExterior?0:90,12);}
    public bool TryReturnForReview(){if(!IsExterior)return true;RequestTravel(false);return false;}
    void Draw(Texture2D texture,Rect rect){if(texture!=null)GUI.DrawTexture(rect,texture,ScaleMode.StretchToFill,true);}
    void OnGUI()
    {
        if(player==null)return;
        if(text==null){text=new GUIStyle(GUI.skin.label){font=Resources.Load<Font>("Hud/OriginalUiRegular"),fontSize=22,alignment=TextAnchor.MiddleCenter,wordWrap=true};text.normal.textColor=Color.white;heading=new GUIStyle(text){font=Resources.Load<Font>("Hud/OriginalUiBold"),fontSize=32};}
        Matrix4x4 old=GUI.matrix;GUI.depth=-200;
        if(IsLoading)
        {
            GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.blackTexture);float scale=Mathf.Min(Screen.width/1920f,Screen.height/1080f);Vector2 origin=new Vector2((Screen.width-1920*scale)/2,(Screen.height-1080*scale)/2);GUI.matrix=Matrix4x4.TRS(origin,Quaternion.identity,new Vector3(scale,scale,1));
            Draw(backdrop,new Rect(0,0,1920,1080));Draw(logo,new Rect(580,670,760,155));GUI.Label(new Rect(380,880,1160,60),status,heading);
            Rect ring=new Rect(1690,870,190,190);Matrix4x4 canvas=GUI.matrix;GUI.matrix=TuUiSkin.Around(canvas,ring.center,Time.unscaledTime*28);Draw(ring1,ring);GUI.matrix=TuUiSkin.Around(canvas,ring.center,-Time.unscaledTime*48);Draw(ring2,new Rect(ring.x+9,ring.y+9,172,172));GUI.matrix=canvas;Draw(ring3,new Rect(ring.x+21,ring.y+21,148,148));GUI.Label(new Rect(ring.x,ring.center.y-18,190,60),"LOADING\n"+Mathf.RoundToInt(progress*100)+"%",text);
        }
        else
        {
            float scale=Mathf.Max(.5f,Screen.height/1080f);GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));float w=Screen.width/scale,h=Screen.height/scale;
            if(!player.hud.Typing&&(IsExterior||NearExit)){GUI.Box(new Rect(w/2-260,h-112,520,44),GUIContent.none);GUI.Label(new Rect(w/2-260,h-112,520,44),IsExterior?"[ E ] RETURN TO HANGAR":(autobot?"[ E ] ENTER CITY":"[ E ] ENTER CENTRAL CITY"),text);}
            if(IsExterior)GUI.Label(new Rect(w/2-230,52,460,36),area.name,text);
            if(error.Length>0){GUI.Box(new Rect(w/2-380,h/2-70,760,140),GUIContent.none);GUI.Label(new Rect(w/2-360,h/2-65,720,90),error,text);if(GUI.Button(new Rect(w/2-65,h/2+30,130,30),"CLOSE"))error="";}
        }
        GUI.matrix=old;
    }
}
