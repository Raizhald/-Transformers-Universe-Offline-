using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Opt-in native check; never reports success without running in the Unity player.
public sealed class TuAutobotRuntimeCheck : MonoBehaviour
{
    [Serializable] sealed class Report
    {
        public bool cityEntered=false,cityGrounded=false,cityReturn=false,details=false;
        public bool passed=false,menu=false,grounded=false,movement=false,jump=false,reset=false,map=false;
        public int hangarMeshes=0,robotMeshes=0,animations=0,sourceLights=0;
        public float walkDistance=0,runDistance=0;
        public string error="",unityVersion="";public string[] errors=new string[0];
    }
    string output="";readonly List<string> errors=new List<string>();Report report=new Report();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Attach()
    {
        string output=null;string[] args=Environment.GetCommandLineArgs();
        for(int i=0;i<args.Length-1;i++)if(args[i]=="--tu-verify-autobot")output=args[i+1];
#if UNITY_EDITOR
        if(UnityEditor.SessionState.GetBool("TU.RunAutobotVerification",false)){UnityEditor.SessionState.SetBool("TU.RunAutobotVerification",false);output=Path.GetFullPath("RuntimeChecks/Autobot");}
#endif
        if(output!=null)new GameObject("Explicit Autobot verification").AddComponent<TuAutobotRuntimeCheck>().output=output;
    }
    void OnEnable(){Application.logMessageReceived+=Log;}
    void OnDisable(){Application.logMessageReceived-=Log;}
    void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
    static void Expect(bool condition,string message){if(!condition)throw new Exception(message);}
    IEnumerator Start()
    {
        Directory.CreateDirectory(output);report.unityVersion=Application.unityVersion;
        IEnumerator run=Run();while(true){bool more;try{more=run.MoveNext();}catch(Exception e){report.error=e.ToString();break;}if(!more)break;yield return run.Current;}
        report.errors=errors.ToArray();report.passed=report.error.Length==0&&errors.Count==0;
        File.WriteAllText(Path.Combine(output,"autobot-runtime-verification.json"),JsonUtility.ToJson(report,true));
        Debug.Log("TU_AUTOBOT_VERIFICATION "+(report.passed?"PASSED":"FAILED"));
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying=false;
#else
        Application.Quit(report.passed?0:1);
#endif
    }
    IEnumerator Run()
    {
        yield return null;TuFrontEnd front=UnityEngine.Object.FindAnyObjectByType<TuFrontEnd>();
        Expect(front!=null,"Launcher missing");front.PressPlay();yield return new WaitForSecondsRealtime(1.3f);
        Expect(front.LoginAs("Autobot Test"),"Local login failed");Expect(front.ChooseAutobots(),"Autobots unavailable");
        float deadline=Time.realtimeSinceStartup+90;
        while(front.Flow.Current==TuStartupState.Screen.HangarLoading&&Time.realtimeSinceStartup<deadline)yield return null;
        Expect(front.Flow.Current==TuStartupState.Screen.Hangar&&front.Flow.Faction=="Autobots","Autobot loading failed");report.menu=true;
        yield return new WaitForSeconds(1);
        TuHangarPlayer p=UnityEngine.Object.FindAnyObjectByType<TuHangarPlayer>();
        Expect(p!=null&&p.autobotHangar!=null&&p.travel!=null&&p.vehicle==null,"Wrong faction controllers");
        Expect(p.hud.FactionName=="Autobots"&&p.hud.RobotName=="Catapult (A001)","Wrong commander HUD");
        report.robotMeshes=p.model.GeometryMeshes.Length;report.animations=p.model.motions.Count;
        report.hangarMeshes=GameObject.Find("Autobot Hangar").GetComponentsInChildren<MeshRenderer>().Length;report.sourceLights=p.autobotHangar.OriginalLights;
        Expect(report.hangarMeshes==639&&report.robotMeshes==3&&report.animations==4,"Archive geometry or animation missing");
        CharacterController controller=p.GetComponent<CharacterController>();Expect(controller.isGrounded,"Spawn has no supporting collider");report.grounded=true;
        Vector3 start=p.transform.position;p.SetTestDrive(Vector3.forward,false,false);yield return new WaitForSeconds(1);p.SetTestDrive(Vector3.zero,false,false);
        report.walkDistance=Vector3.Distance(start,p.transform.position);Expect(report.walkDistance>1,"Walk did not move");
        p.ResetPosition();yield return new WaitForSeconds(.3f);start=p.transform.position;p.SetTestDrive(Vector3.forward,true,false);yield return new WaitForSeconds(1);p.SetTestDrive(Vector3.zero,false,false);
        report.runDistance=Vector3.Distance(start,p.transform.position);Expect(report.runDistance>report.walkDistance*1.3f,"Run speed did not increase");report.movement=true;
        p.ResetPosition();yield return new WaitForSeconds(.3f);float y=p.transform.position.y;p.SetTestDrive(Vector3.zero,false,true);yield return new WaitForSeconds(.25f);
        Expect(p.transform.position.y>y+.3f,"Jump failed");yield return new WaitForSeconds(1.5f);Expect(controller.isGrounded,"Robot did not land");report.jump=true;
        p.Teleport(p.HangarSpawn+Vector3.right*5,90,12);p.ResetPosition();yield return new WaitForSeconds(.5f);
        Expect(Vector3.Distance(p.transform.position,p.HangarSpawn)<.3f,"Reset used Decepticon coordinates");report.reset=true;
        GameObject obstacle=new GameObject("Arrival regression obstacle");obstacle.layer=8;obstacle.transform.position=p.HangarSpawn+Vector3.up*1.5f;obstacle.AddComponent<BoxCollider>().size=new Vector3(1.6f,3,1.6f);Physics.SyncTransforms();Vector3 replacement;
        Expect(!TuSafeArrival.IsClear(p.HangarSpawn,controller),"Occupied arrival was accepted");Expect(TuSafeArrival.TryFind(p.HangarSpawn,controller,out replacement)&&Vector3.Distance(replacement,p.HangarSpawn)>1,"No safe alternative to occupied arrival");obstacle.SetActive(false);Destroy(obstacle);Physics.SyncTransforms();
        Rect area=new Rect(0,0,256,256);Vector2 marker=p.hud.MapPosition(p.transform.position,area);Expect(area.Contains(marker),"Player is outside Autobot radar");report.map=true;
        var signs=UnityEngine.Object.FindAnyObjectByType<TuAutobotSigns>();Expect(signs!=null&&signs.SourceSignCount==3,"Original Autobot signs missing");Expect(GameObject.Find("VFX_Mesh_LevelObjects_Portal_ADD")!=null,"Original Autobot portal missing");var baseData=UnityEngine.Object.FindAnyObjectByType<TuAutobotHangar>();Expect(baseData!=null&&baseData.LowerMeshes==2&&baseData.DisabledLowerFades>0,"Finished lower floor, source tunnel grille or fade removal missing");Expect(UnityEngine.Object.FindAnyObjectByType<TuAutobotFloorUnderlays>()==null,"Obsolete asphalt patches are active");report.details=true;
        if(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null)ScreenCapture.CaptureScreenshot(Path.Combine(output,"autobot-hangar.png"));yield return new WaitForSeconds(.2f);
        Expect(p.travel.RequestTravel(true),"Autobot city request rejected");deadline=Time.realtimeSinceStartup+90;while(p.travel.IsLoading&&Time.realtimeSinceStartup<deadline)yield return null;
        Expect(p.travel.IsExterior&&!p.travel.IsLoading,"Autobot city transition failed: "+p.travel.LastError);report.cityEntered=true;yield return new WaitForSeconds(1);
        Expect(controller.isGrounded,"City entry has no floor contact");Expect(p.travel.ExteriorTerrain!=null&&!float.IsNaN(p.travel.ExteriorTerrain.GroundHeight(p.transform.position)),"Shared city terrain missing");report.cityGrounded=true;
        Expect(UnityEngine.Object.FindAnyObjectByType<TuAutobotCity>()==null,"Tutorial exterior was loaded instead of the shared city");var parked=UnityEngine.Object.FindAnyObjectByType<TuParkedVehicles>();Expect(parked!=null&&parked.ColliderCount==599,"Shared city vehicle blockers missing");Expect(p.travel.ExteriorRendererCount==11204,"Shared city render geometry missing");
        start=p.transform.position;p.SetTestDrive(Vector3.forward,false,false);yield return new WaitForSeconds(1);p.SetTestDrive(Vector3.zero,false,false);Expect(Vector3.Distance(start,p.transform.position)>1,"City walking failed");
        if(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null)ScreenCapture.CaptureScreenshot(Path.Combine(output,"autobot-city.png"));yield return new WaitForSeconds(.2f);
        Expect(p.travel.RequestTravel(false),"City return request rejected");deadline=Time.realtimeSinceStartup+20;while(p.travel.IsLoading&&Time.realtimeSinceStartup<deadline)yield return null;yield return new WaitForSeconds(.5f);
        Expect(!p.travel.IsExterior&&!p.travel.IsLoading,"Autobot return did not finish");Expect(controller.isGrounded&&Vector3.Distance(p.transform.position,p.HangarSpawn)<.5f&&TuSafeArrival.IsClear(p.transform.position,controller),"Return position or floor is incorrect");report.cityReturn=true;
    }
}
