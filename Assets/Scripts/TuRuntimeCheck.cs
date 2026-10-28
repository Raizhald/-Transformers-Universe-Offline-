using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Runs only when explicitly requested from the editor menu or with --tu-verify.
public sealed class TuRuntimeCheck : MonoBehaviour
{
    [Serializable] public class Report { public bool overturnedRecoveryPassed,airborneRecoveryPassed;public bool additiveFogNeutralPassed;public bool vehicleTransformationPassed,vehicleAccelerationPassed,parkedVehicleCollisionsPassed;public bool terrainCollisionPassed;public bool exteriorTravelPassed,exteriorReturnPassed;public bool passed,startupFlowPassed,wallCollisionPassed,cameraCollisionSafe,blockedAnimationIdle,hudChecksPassed,restrictedDoorsPassed,doorRimFitPassed,crystalTransparencyPassed,audioClipsLoaded,audioStepsTriggered,uiSourceDimensionsPassed;public string unityVersion,graphicsDevice,error;public float walkDistance,runDistance,jumpHeight,portalFloorY;public int globeMeshes,lights,particles,emissions;public string[] errors; }
    readonly List<string> errors=new List<string>();Report report=new Report();string output;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Attach()
    {
        string output=null;string[] args=Environment.GetCommandLineArgs();for(int k=0;k<args.Length-1;k++)if(args[k]=="--tu-verify")output=args[k+1];
#if UNITY_EDITOR
        if(UnityEditor.SessionState.GetBool("TU.RunVerification",false)){UnityEditor.SessionState.SetBool("TU.RunVerification",false);output=Path.GetFullPath("RuntimeChecks");}
#endif
        if(output!=null){TuRuntimeCheck check=new GameObject("Explicit runtime verification").AddComponent<TuRuntimeCheck>();check.output=output;}
    }
    void OnEnable(){Application.logMessageReceived+=Log;}
    void OnDisable(){Application.logMessageReceived-=Log;}
    void Log(string condition,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(condition);}
    IEnumerator Start()
    {
        report.unityVersion=Application.unityVersion;report.graphicsDevice=SystemInfo.graphicsDeviceName;Directory.CreateDirectory(output);IEnumerator test=Run();
        while(true){bool more;try{more=test.MoveNext();}catch(Exception e){report.error=e.ToString();break;}if(!more)break;yield return test.Current;}
        report.errors=errors.ToArray();report.passed=string.IsNullOrEmpty(report.error)&&errors.Count==0;
        File.WriteAllText(Path.Combine(output,"runtime-verification.json"),JsonUtility.ToJson(report,true));Debug.Log("TU_VERIFICATION "+(report.passed?"PASSED":"FAILED")+" "+output);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying=false;
#else
        Application.Quit(report.passed?0:1);
#endif
    }
    static void Expect(bool value,string why){if(!value)throw new Exception(why);}
    IEnumerator Run()
    {
        CheckUiDimensions();report.uiSourceDimensionsPassed=true;CheckAdditiveFog();report.additiveFogNeutralPassed=true;
        TuFrontEnd front=UnityEngine.Object.FindAnyObjectByType<TuFrontEnd>();Expect(front!=null&&front.Flow.Current==TuStartupState.Screen.Launcher,"Game did not start at launcher");front.PressPlay();yield return new WaitForSecondsRealtime(1.3f);Expect(front.Flow.Current==TuStartupState.Screen.Login,"Splash did not lead to log-in");Expect(front.LoginAs(front.CommanderInputName),"Local log-in failed");Expect(!front.Flow.SelectFaction("Unknown"),"Invalid faction was selectable");Expect(front.ChooseDecepticons(),"Decepticons selection failed");float deadline=Time.realtimeSinceStartup+90;while(front.Flow.Current==TuStartupState.Screen.HangarLoading&&Time.realtimeSinceStartup<deadline)yield return null;Expect(front.Flow.Current==TuStartupState.Screen.Hangar,"Loading did not enter hangar");report.startupFlowPassed=true;yield return new WaitForSeconds(1);
        TuHangarPlayer p=UnityEngine.Object.FindAnyObjectByType<TuHangarPlayer>();Expect(p!=null,"Player missing");TuHangarEffects fx=p.effects;Expect(fx!=null,"Effects missing");
        TuRobotAudio sound=p.GetComponent<TuRobotAudio>();Expect(sound!=null&&sound.LoadedClipCount==23,"Original audio clips did not load");report.audioClipsLoaded=true;int initialSteps=sound.StepsPlayed;
        TuHud hud=p.hud;Expect(hud!=null,"HUD missing");int count=hud.LocalMessageCount;Expect(hud.SubmitChat("UI test")&&hud.LocalMessageCount==count+1,"Local chat failed");Expect(!hud.SubmitChat("   "),"Empty chat message accepted");hud.SelectMode(3);Expect(hud.SelectedMode==3,"Mode selection failed");hud.SelectMode(0);hud.SetHealth(.6f);Expect(Mathf.Abs(hud.Health-.6f)<.001f,"Health display failed");hud.SetHealth(1);hud.BeginChat();Expect(hud.Typing&&!p.MouseCaptured,"Chat captured gameplay mouse");hud.EndChat();report.hudChecksPassed=true;
        Expect(GameObject.Find("Original floor collision visual backing")==null,"Removed v9 floor overlay is still present");Expect(GameObject.Find("Central City restricted door")==null,"Unexpected Central City barrier");CheckRestrictedDoor("Hangar Uplink restricted door");CheckRestrictedDoor("Training restricted door");report.restrictedDoorsPassed=true;CheckDoorRim("Hangar Uplink restricted door");CheckDoorRim("Training restricted door");report.doorRimFitPassed=true;CheckGlowMaterials();report.crystalTransparencyPassed=true;
        CharacterController controller=p.GetComponent<CharacterController>();Expect(controller.isGrounded,"Player did not land at hangar start");Vector3 start=p.transform.position;
        p.SetTestDrive(Vector3.forward,false,false);yield return new WaitForSeconds(1);p.SetTestDrive(Vector3.zero,false,false);report.walkDistance=Vector3.Distance(start,p.transform.position);Expect(report.walkDistance>.5f&&report.walkDistance<6,"Walking blocked or too fast");Expect(sound.StepsPlayed>initialSteps,"Walking did not trigger step audio");report.audioStepsTriggered=true;
        start=p.transform.position;p.SetTestDrive(Vector3.forward,true,false);yield return new WaitForSeconds(1);p.SetTestDrive(Vector3.zero,false,false);report.runDistance=Vector3.Distance(start,p.transform.position);Expect(report.runDistance>report.walkDistance*1.4f,"Run did not increase speed");
        yield return new WaitForSeconds(.25f);float floor=p.transform.position.y;p.SetTestDrive(Vector3.zero,false,true);yield return new WaitForSeconds(.25f);report.jumpHeight=p.transform.position.y-floor;Expect(report.jumpHeight>.25f,"Jump failed");yield return new WaitForSeconds(1.3f);Expect(controller.isGrounded,"Jump did not land");
        RaycastHit wall=new RaycastHit();bool found=false;Vector3 origin=TuPrototypeBootstrap.Spawn+Vector3.up*2.6f;
        foreach(Vector3 direction in new Vector3[]{Vector3.forward,Vector3.back,Vector3.right,Vector3.left})
        {
            RaycastHit candidate;if(!Physics.Raycast(origin,direction,out candidate,40,1<<8,QueryTriggerInteraction.Ignore)||Mathf.Abs(candidate.normal.y)>.1f||candidate.distance<3)continue;
            Vector3 spot=candidate.point+candidate.normal*2;RaycastHit ground;
            if(!Physics.Raycast(new Vector3(spot.x,TuPrototypeBootstrap.Spawn.y+2,spot.z),Vector3.down,out ground,4,1<<8,QueryTriggerInteraction.Ignore))continue;
            spot.y=ground.point.y+.08f;wall=candidate;float facing=Mathf.Atan2(-wall.normal.x,-wall.normal.z)*Mathf.Rad2Deg;p.Teleport(spot,facing,12);found=true;break;
        }
        Expect(found,"No suitable wall for collision check");yield return new WaitForSeconds(.3f);p.SetTestDrive(Vector3.forward,true,false);yield return new WaitForSeconds(1.2f);
        report.wallCollisionPassed=Vector3.Dot(p.transform.position-wall.point,wall.normal)>.5f;report.blockedAnimationIdle=p.CurrentAnimation=="Idle";Expect(report.wallCollisionPassed,"Robot penetrated wall");Expect(report.blockedAnimationIdle,"Robot keeps running animation against wall");
        p.SetTestDrive(Vector3.zero,false,false);Vector3 wallPosition=p.transform.position;float away=Mathf.Atan2(wall.normal.x,wall.normal.z)*Mathf.Rad2Deg;p.Teleport(wallPosition,away,0);yield return new WaitForSeconds(.4f);
        report.cameraCollisionSafe=!Physics.CheckSphere(p.ViewCamera.transform.position,.12f,1<<8,QueryTriggerInteraction.Ignore)&&p.CameraDistance<7.9f;Expect(report.cameraCollisionSafe,"Camera clips wall or failed to retract");
        p.ReviewPortal();yield return new WaitForSeconds(1);report.portalFloorY=p.transform.position.y;Expect(controller.isGrounded&&Mathf.Abs(report.portalFloorY-155.004f)<.2f,"Portal viewpoint is not on the floor");
        report.globeMeshes=fx.GlobeMeshes;report.lights=fx.OriginalLights;report.particles=fx.ParticleSystems;report.emissions=fx.EmissiveSurfaces;
        Expect(report.globeMeshes==18&&report.lights==39&&report.particles==7&&report.emissions>0,"Missing effect data");float energy=fx.PortalEnergy;yield return new WaitForSeconds(.4f);Expect(Mathf.Abs(fx.PortalEnergy-energy)>.002f,"Portal light is not animated");
        foreach(Renderer r in UnityEngine.Object.FindObjectsByType<Renderer>())foreach(Material m in r.sharedMaterials)Expect(m!=null&&m.shader!=null&&m.shader.isSupported,"Unsupported shader on "+r.name);
        Capture(p.ViewCamera,"unity-v17-portal-on.png");yield return new WaitForSeconds(.3f);fx.SetEffects(false);yield return null;Expect(!fx.EffectsVisible,"Effects toggle off failed");CheckGlowMaterials();Capture(p.ViewCamera,"unity-v17-portal-off.png");yield return new WaitForSeconds(.3f);fx.SetEffects(true);yield return new WaitForSeconds(.3f);Expect(fx.EffectsVisible,"Effects toggle on failed");
        p.ReviewGlobe();yield return new WaitForSeconds(1);Expect(controller.isGrounded,"Hologram viewpoint is not on the floor");Capture(p.ViewCamera,"unity-v17-hologram.png");yield return new WaitForSeconds(.3f);
        var travel=p.travel;Expect(travel!=null,"World travel is missing");Expect(travel.RequestTravel(true),"Exterior travel request refused");
        deadline=Time.realtimeSinceStartup+180;while(travel.IsLoading&&Time.realtimeSinceStartup<deadline)yield return null;
        Expect(!travel.IsLoading&&travel.IsExterior,"Exterior load failed: "+travel.LastError);yield return new WaitForSeconds(1);
        Expect(travel.ExteriorRendererCount==11204&&travel.ExteriorCollisionChunks==11,"Exterior content count mismatch");Expect(controller.isGrounded,"Robot did not land on exterior collision");Expect(p.transform.position.y<100,"Exterior height safety incorrectly returned to hangar");
        foreach(Renderer renderer in UnityEngine.Object.FindObjectsByType<Renderer>())foreach(Material material in renderer.sharedMaterials)Expect(material!=null&&material.shader!=null&&material.shader.isSupported,"Unsupported exterior shader on "+renderer.name);
        var city=UnityEngine.Object.FindAnyObjectByType<TuCityRestoration>();Expect(city!=null&&city.RestoredMaterials>140,"City material restoration did not apply");Expect(city.MultiplyMaterials>=3,"Original multiply decals did not get their dedicated shader");Expect(city.AdditiveMaterials>=7,"Original additive effects did not get their black-fog shader");Expect(RenderSettings.skybox!=null&&RenderSettings.skybox.shader.isSupported,"Original city sky is missing or unsupported");Expect(GameObject.Find("Original road collision")!=null,"Recovered road collision is missing");Expect(Resources.Load<AudioClip>("CityAudio/HighGrand_Wind_02")!=null,"Original city wind loop is missing");
        report.exteriorTravelPassed=true;
        var terrain=travel.ExteriorTerrain;Expect(terrain!=null&&terrain.Terrain!=null&&terrain.OriginalLayerCount==8,"Original terrain is missing");Expect(terrain.Terrain.terrainData.heightmapResolution==1025,"Original heightfield resolution changed");
        var terrainCollider=terrain.Terrain.GetComponent<TerrainCollider>();Expect(terrainCollider!=null,"Original terrain collider is missing");
        Vector3[] samples={new Vector3(-50.078064f,0,-93),new Vector3(-600.078064f,0,-100),new Vector3(-512.078064f,0,-492),new Vector3(-900.078064f,0,-900)};
        foreach(var sample in samples)
        {
            float ground=terrain.GroundHeight(sample);RaycastHit terrainHit;Expect(!float.IsNaN(ground)&&terrainCollider.Raycast(new Ray(sample+Vector3.up*(ground+10),Vector3.down),out terrainHit,20),"Source terrain collision is absent at "+sample);
            p.Teleport(new Vector3(sample.x,ground+1,sample.z),0,12);p.SetTestDrive(Vector3.zero,false,false);yield return new WaitForSeconds(1);
            Expect(controller.isGrounded&&p.transform.position.y>=terrain.GroundHeight(p.transform.position)-.2f,"Robot fell through original terrain at "+sample);
            p.SetTestDrive(Vector3.forward,false,false);for(int step=0;step<20;step++){yield return new WaitForSeconds(.05f);Expect(p.transform.position.y>=terrain.GroundHeight(p.transform.position)-.2f,"Walking penetrated terrain");}p.SetTestDrive(Vector3.zero,false,false);
        }
        report.terrainCollisionPassed=true;travel.ResetCurrentArea();yield return new WaitForSeconds(.4f);
        var parked=UnityEngine.Object.FindAnyObjectByType<TuParkedVehicles>();Expect(parked!=null&&parked.ColliderCount==599,"Parked vehicle colliders missing");var carBoxes=parked.GetComponentsInChildren<BoxCollider>();Expect(carBoxes.Length==599,"Parked collider count mismatch");RaycastHit carHit;var first=carBoxes[0];Expect(!first.isTrigger&&first.Raycast(new Ray(first.transform.position+first.transform.forward*(first.size.z/2+2),-first.transform.forward),out carHit,first.size.z+4),"Parked vehicle is not solid");report.parkedVehicleCollisionsPassed=true;
        var vehicle=p.vehicle;Expect(vehicle!=null&&vehicle.Toggle(),"Robot-to-vehicle request rejected");deadline=Time.realtimeSinceStartup+8;while(vehicle.Transforming&&Time.realtimeSinceStartup<deadline)yield return null;Expect(vehicle.Driving&&vehicle.Wheels.Length==4,"Vehicle rig or four wheels missing");yield return new WaitForSeconds(1);Expect(vehicle.GroundedWheelCount>=2,"Vehicle did not settle on its wheels");
        vehicle.SetTestControls(1,0,false);yield return new WaitForSeconds(1.5f);Expect(vehicle.SpeedKph>5,"Vehicle did not accelerate");report.vehicleAccelerationPassed=true;vehicle.SetTestControls(0,0,true);yield return new WaitForSeconds(3);vehicle.ClearTestControls();Expect(vehicle.SpeedKph<7,"Vehicle did not brake");Capture(p.ViewCamera,"unity-v17-driving.png");Expect(vehicle.Toggle(),"Vehicle-to-robot request rejected");deadline=Time.realtimeSinceStartup+8;while(vehicle.Transforming&&Time.realtimeSinceStartup<deadline)yield return null;Expect(!vehicle.Driving&&controller.enabled,"Vehicle did not return to robot");report.vehicleTransformationPassed=true;
        travel.ResetCurrentArea();yield return new WaitForSeconds(.5f);Expect(vehicle.Toggle(),"Recovery test could not enter car");deadline=Time.realtimeSinceStartup+8;while(vehicle.Transforming&&Time.realtimeSinceStartup<deadline)yield return null;Expect(vehicle.Driving,"Recovery test failed to finish car entry");yield return new WaitForSeconds(.5f);vehicle.Body.rotation=Quaternion.Euler(0,45,180);vehicle.Body.linearVelocity=Vector3.forward*15;Physics.SyncTransforms();Expect(vehicle.Toggle(),"Overturned moving car refused robot mode");Expect(!vehicle.Driving&&!vehicle.Transforming&&controller.enabled,"Overturned recovery did not restore robot immediately");yield return new WaitForSeconds(.5f);Expect(controller.isGrounded,"Overturned recovery placed robot off the ground");report.overturnedRecoveryPassed=true;
        travel.ResetCurrentArea();yield return new WaitForSeconds(.5f);Expect(vehicle.Toggle(),"Air recovery test could not enter car");deadline=Time.realtimeSinceStartup+8;while(vehicle.Transforming&&Time.realtimeSinceStartup<deadline)yield return null;Expect(vehicle.Driving,"Recovery test failed to finish car entry");vehicle.Body.position+=Vector3.up*6;vehicle.Body.linearVelocity=Vector3.forward*12;Physics.SyncTransforms();Expect(vehicle.Toggle(),"Airborne moving car refused robot mode");Expect(!vehicle.Driving&&!vehicle.Transforming&&controller.enabled,"Airborne recovery did not restore robot immediately");report.airborneRecoveryPassed=true;
        travel.ResetCurrentArea();yield return new WaitForSeconds(.4f);Capture(p.ViewCamera,"unity-v17-exterior.png");Expect(travel.RequestTravel(false),"Hangar return request refused");
        deadline=Time.realtimeSinceStartup+15;while(travel.IsLoading&&Time.realtimeSinceStartup<deadline)yield return null;Expect(!travel.IsLoading&&!travel.IsExterior,"Hangar return failed");yield return new WaitForSeconds(1);
        Expect(controller.isGrounded&&Vector3.Distance(p.transform.position,travel.HangarExit)<5,"Hangar return position is invalid");report.exteriorReturnPassed=true;

    }
    static void CheckAdditiveFog()
    {
        bool oldFog=RenderSettings.fog;FogMode oldMode=RenderSettings.fogMode;Color oldColor=RenderSettings.fogColor;float oldStart=RenderSettings.fogStartDistance,oldEnd=RenderSettings.fogEndDistance;RenderTexture oldTarget=RenderTexture.active;
        GameObject cameraObject=null,quad=null;Material material=null;Texture2D black=null,readback=null;Mesh mesh=null;RenderTexture target=null;
        try
        {
            Shader shader=Resources.Load<Shader>("TuCityAdditive");Expect(shader!=null&&shader.isSupported,"Black-fog additive shader is unsupported");
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=new Color(.2f,.5f,.75f);RenderSettings.fogStartDistance=0;RenderSettings.fogEndDistance=.25f;
            cameraObject=new GameObject("Additive sky-overlay pixel test");var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.transform.position=new Vector3(0,0,-2);camera.orthographic=true;camera.orthographicSize=1;camera.nearClipPlane=.1f;camera.farClipPlane=10;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.12f,.25f,.37f);camera.cullingMask=1<<31;camera.allowHDR=false;camera.allowMSAA=false;
            target=new RenderTexture(8,8,16,RenderTextureFormat.ARGB32);target.Create();camera.targetTexture=target;readback=new Texture2D(8,8,TextureFormat.RGBA32,false);
            quad=GameObject.CreatePrimitive(PrimitiveType.Quad);quad.name="Black edge of original glow texture";quad.layer=31;quad.transform.localScale=new Vector3(2,2,1);var renderer=quad.GetComponent<MeshRenderer>();mesh=UnityEngine.Object.Instantiate(quad.GetComponent<MeshFilter>().sharedMesh);var colors=new Color32[mesh.vertexCount];for(int k=0;k<colors.Length;k++)colors[k]=new Color32(255,255,255,255);mesh.colors32=colors;quad.GetComponent<MeshFilter>().sharedMesh=mesh;
            black=new Texture2D(1,1,TextureFormat.RGBA32,false);black.SetPixel(0,0,new Color(0,0,0,1));black.Apply();material=new Material(shader);material.mainTexture=black;material.color=Color.white;renderer.sharedMaterial=material;
            renderer.enabled=false;camera.Render();RenderTexture.active=target;readback.ReadPixels(new Rect(0,0,8,8),0,0);readback.Apply();Color before=readback.GetPixel(4,4);
            renderer.enabled=true;camera.Render();RenderTexture.active=target;readback.ReadPixels(new Rect(0,0,8,8),0,0);readback.Apply();Color after=readback.GetPixel(4,4);
            Expect(Mathf.Abs(before.r-after.r)<.015f&&Mathf.Abs(before.g-after.g)<.015f&&Mathf.Abs(before.b-after.b)<.015f,"Fog made a black additive texture edge visible over its backdrop");
        }
        finally
        {
            RenderTexture.active=oldTarget;RenderSettings.fog=oldFog;RenderSettings.fogMode=oldMode;RenderSettings.fogColor=oldColor;RenderSettings.fogStartDistance=oldStart;RenderSettings.fogEndDistance=oldEnd;
            if(cameraObject!=null)UnityEngine.Object.Destroy(cameraObject);if(quad!=null)UnityEngine.Object.Destroy(quad);if(material!=null)UnityEngine.Object.Destroy(material);if(black!=null)UnityEngine.Object.Destroy(black);if(readback!=null)UnityEngine.Object.Destroy(readback);if(mesh!=null)UnityEngine.Object.Destroy(mesh);if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}
        }
    }
    static void CheckRestrictedDoor(string name)
    {
        GameObject door=GameObject.Find(name);Expect(door!=null,"Missing restricted door "+name);Collider collider=door.GetComponent<Collider>();Expect(collider!=null&&!collider.isTrigger,"Door has no solid collider");
        for(int side=-1;side<=1;side+=2){GameObject probe=new GameObject("Door collision probe");try{Vector3 direction=door.transform.forward*side;probe.transform.position=door.transform.position+direction*4;CharacterController cc=probe.AddComponent<CharacterController>();cc.height=3.3f;cc.radius=.7f;cc.center=Vector3.zero;Physics.SyncTransforms();for(int n=0;n<16;n++)cc.Move(-direction*.5f);float remaining=Vector3.Dot(probe.transform.position-door.transform.position,direction);Expect(remaining>.5f,"Robot can cross "+name);}finally{UnityEngine.Object.Destroy(probe);}}
    }
    static void CheckGlowMaterials()
    {
        int count=0;foreach(Renderer r in UnityEngine.Object.FindObjectsByType<Renderer>())foreach(Material m in r.sharedMaterials)if(m.name.StartsWith("ConeLampGlow01")){Expect(m.shader.name=="TU/ArchiveAdditive","Crystal glow became opaque on "+r.name);count++;}Expect(count>0,"No crystal glow surfaces checked");
    }
    static void CheckDoorRim(string name)
    {
        Transform door=GameObject.Find(name).transform;Mesh m=door.GetComponent<MeshFilter>().sharedMesh;
        foreach(Vector3 corner in m.vertices){if(corner.sqrMagnitude<.01f)continue;Vector3 world=door.TransformPoint(corner);float nearest=float.MaxValue;foreach(MeshFilter frame in UnityEngine.Object.FindObjectsByType<MeshFilter>()){if(frame.sharedMesh==null||frame.sharedMesh.name!="HangarDoorSurround_01")continue;foreach(Vector3 v in frame.sharedMesh.vertices)nearest=Mathf.Min(nearest,Vector3.Distance(world,frame.transform.TransformPoint(v)));}Expect(nearest<.002f,"Shield corner is outside the archived doorway rim: "+name);}
    }
    [Serializable] sealed class UiTexture {public string path="";public int width=0,height=0;}
    [Serializable] sealed class UiTextureList {public UiTexture[] textures=new UiTexture[0];}
    static void CheckUiDimensions()
    {
        var data=JsonUtility.FromJson<UiTextureList>(Resources.Load<TextAsset>("ui-source-dimensions").text);foreach(var entry in data.textures){Texture2D texture=Resources.Load<Texture2D>(entry.path);Expect(texture!=null&&texture.width==entry.width&&texture.height==entry.height,"UI import resized original atlas crop: "+entry.path);}
    }
    void Capture(Camera camera,string name)
    {
        ScreenCapture.CaptureScreenshot(Path.Combine(output,name));
    }
}
