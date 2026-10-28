using UnityEngine;

public sealed class TuHangarPlayer : MonoBehaviour
{
    public TuPackedModel model;
    public TuHangarEffects effects;
    public TuHud hud;
    public TuWorldTravel travel;
    public TuVehicleMode vehicle;
    public TuAutobotHangar autobotHangar;
    public Vector3 HangarSpawn=TuPrototypeBootstrap.Spawn;
    public float MinimumHangarY=100;
    public float VisualYaw {get{return vehicle!=null&&vehicle.Driving?vehicle.Body.rotation.eulerAngles.y+180:model.root.transform.eulerAngles.y;}}
    public bool MouseCaptured {get{return captured;}}
    public void SetMouseMode(bool mouseMode){Capture(!mouseMode);}
    bool testDriving,testRun,testJump;Vector3 testInput;
    CharacterController controller;
    Camera view;
    float yaw=90,pitch=12,distance=8,vertical,animationTime;
    bool captured;
    TuPackedModel.Motion motion;
    string state="Idle";
    Vector3 planarVelocity,lastSafePosition,cameraAnchor,anchorVelocity;
    float lastGrounded=-100,jumpPressed=-100,transitionAge=1,currentCameraDistance=8,cameraDistanceVelocity;
    bool cameraInitialized;
    Vector3[] blendPositions,blendScales;Quaternion[] blendRotations;
    public string CurrentAnimation {get{return state;}}
    public int LastMovementSubsteps {get;private set;}
    public float ActualSpeed {get;private set;}
    public float AnimationPhase {get{return motion!=null&&motion.duration>0?(animationTime%motion.duration)/motion.duration:0;}}
    public float CameraDistance {get{return currentCameraDistance;}}
    void Start()
    {
        controller=GetComponent<CharacterController>();GameObject go=new GameObject("Third person camera");view=go.AddComponent<Camera>();go.AddComponent<AudioListener>();view.fieldOfView=63;view.nearClipPlane=.15f;view.farClipPlane=1000;view.backgroundColor=new Color(.035f,.028f,.06f);view.clearFlags=CameraClearFlags.SolidColor;
        model.Pose(model.FindMotion("Idle"),0,1);
        motion=model.FindMotion("Idle");lastSafePosition=transform.position;
        blendPositions=new Vector3[model.nodes.Length];blendScales=new Vector3[model.nodes.Length];blendRotations=new Quaternion[model.nodes.Length];
        view.allowHDR=true;view.nearClipPlane=.12f;var bloom=go.AddComponent<TuBloom>();bloom.effects=effects;bloom.autobotHangar=autobotHangar;
    }
    void Capture(bool value)
    {
        captured=value;
#if UNITY_5_3_OR_NEWER
        Cursor.lockState=value?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!value;
#else
        Screen.lockCursor=value;
#endif
    }
    void OnApplicationFocus(bool focus){if(!focus)Capture(false);}
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))Capture(false);if(Input.GetMouseButtonDown(0)&&(hud==null||!hud.IsOverPanel(Input.mousePosition)))Capture(true);
        if(hud==null||!hud.Typing){
        if(Input.GetKeyDown(KeyCode.R))ResetPosition();
        if(Input.GetKeyDown(KeyCode.P))ReviewPortal();
        if(Input.GetKeyDown(KeyCode.H))ReviewGlobe();
        if(Input.GetKeyDown(KeyCode.V)){if(effects!=null)effects.SetEffects(!effects.EffectsVisible);if(autobotHangar!=null)autobotHangar.ToggleEffects();}
        if(Input.GetKeyDown(KeyCode.F2))UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
        if(captured&&Application.isFocused){Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}
        if(captured){yaw+=Input.GetAxis("Mouse X")*2.3f;pitch=Mathf.Clamp(pitch-Input.GetAxis("Mouse Y")*1.8f,-25,65);}
        if(captured)distance=Mathf.Clamp(distance-Input.GetAxis("Mouse ScrollWheel")*5,3,15);
        if(vehicle!=null&&(vehicle.Driving||vehicle.Transforming)){ActualSpeed=vehicle.Driving?vehicle.SpeedKph/3.6f:0;if(vehicle.Driving){transform.position=vehicle.Body.position;if(captured&&Mathf.Abs(Input.GetAxis("Mouse X"))<.01f)yaw=Mathf.LerpAngle(yaw,vehicle.Body.rotation.eulerAngles.y,Time.deltaTime*1.3f);}return;}
        Vector3 input=Vector3.zero;
        if(captured){if(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow))input.z+=1;if(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow))input.z-=1;if(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow))input.x+=1;if(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow))input.x-=1;}
        if(testDriving)input=testInput;
        bool run=testDriving?testRun:Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift);Vector3 direction=Quaternion.Euler(0,yaw,0)*input.normalized;
        float dt=Mathf.Min(Time.deltaTime,.1f);
        if(controller.isGrounded){lastGrounded=Time.time;lastSafePosition=transform.position;if(vertical<0)vertical=-2;}
        if((captured&&Input.GetKeyDown(KeyCode.Space))||(testDriving&&testJump))jumpPressed=Time.time;testJump=false;
        if(Time.time-jumpPressed<.12f&&Time.time-lastGrounded<.1f){vertical=9;jumpPressed=-100;lastGrounded=-100;}
        planarVelocity=Vector3.MoveTowards(planarVelocity,direction*(run?9:4),(input.sqrMagnitude>.01f?20:28)*dt);
        Vector3 previous=transform.position;LastMovementSubsteps=Mathf.Max(1,Mathf.CeilToInt(dt/(1f/60)));float step=dt/LastMovementSubsteps;
        for(int k=0;k<LastMovementSubsteps;k++){vertical-=24*step;CollisionFlags flags=controller.Move((planarVelocity+Vector3.up*vertical)*step);if((flags&CollisionFlags.Above)!=0&&vertical>0)vertical=0;if((flags&CollisionFlags.Below)!=0&&vertical<0)vertical=-2;}
        Vector3 moved=transform.position-previous;moved.y=0;ActualSpeed=moved.magnitude/Mathf.Max(dt,.0001f);
        if(ActualSpeed>.15f){Quaternion wanted=Quaternion.LookRotation(moved)*Quaternion.Euler(0,180,0);model.root.transform.rotation=Quaternion.Slerp(model.root.transform.rotation,wanted,1-Mathf.Exp(-12*dt));}
        string next=!controller.isGrounded&&Time.time-lastGrounded>.12f&&vertical<-.5f?"Fall":ActualSpeed>.2f?(ActualSpeed>5.3f?"Run":"Walk"):"Idle";
        if(next!=state)Transition(next);
        animationTime+=dt;transitionAge+=dt;model.Pose(motion,animationTime,1);
        if(transitionAge<.18f){float blend=Mathf.SmoothStep(0,1,transitionAge/.18f);for(int k=0;k<model.nodes.Length;k++){Transform bone=model.nodes[k];bone.localPosition=Vector3.Lerp(blendPositions[k],bone.localPosition,blend);bone.localRotation=Quaternion.Slerp(blendRotations[k],bone.localRotation,blend);bone.localScale=Vector3.Lerp(blendScales[k],bone.localScale,blend);}}
        if(transform.position.y<(travel!=null&&travel.IsExterior?travel.MinimumSafeY:MinimumHangarY))Teleport(lastSafePosition,yaw,pitch);
    }
    void Transition(string next)
    {
        for(int k=0;k<model.nodes.Length;k++){blendPositions[k]=model.nodes[k].localPosition;blendRotations[k]=model.nodes[k].localRotation;blendScales[k]=model.nodes[k].localScale;}
        bool preserve=(state=="Walk"||state=="Run")&&(next=="Walk"||next=="Run");float phase=preserve&&motion!=null&&motion.duration>0?animationTime%motion.duration/motion.duration:0;
        state=next;motion=model.FindMotion(next);animationTime=motion==null?0:phase*motion.duration;transitionAge=0;
    }
    public void ResetPosition(){if(travel!=null)travel.ResetCurrentArea();else Teleport(HangarSpawn,90,12);}
    public void ReviewPortal(){if(autobotHangar!=null){if(travel!=null&&!travel.TryReturnForReview())return;Teleport(new Vector3(-10.88759f,27.60535f,107.69558f),0,12);return;}if(travel!=null&&!travel.TryReturnForReview())return;Teleport(new Vector3(-519.09332f,155.08394f,-416.39398f),180,12);}
    public void ReviewGlobe(){if(autobotHangar!=null){if(travel!=null&&!travel.TryReturnForReview())return;Teleport(new Vector3(-50,14.36509f,25),0,-12);return;}if(travel!=null&&!travel.TryReturnForReview())return;Teleport(new Vector3(-510,155.08394f,-416),52,-8);}
    public Camera ViewCamera {get{return view;}}
    public void Teleport(Vector3 position,float cameraYaw,float cameraPitch){if(vehicle!=null)vehicle.ForceRobot();controller.enabled=false;transform.position=position;controller.enabled=true;vertical=0;planarVelocity=Vector3.zero;jumpPressed=-100;lastGrounded=-100;lastSafePosition=position;yaw=cameraYaw;pitch=cameraPitch;distance=8;cameraInitialized=false;model.root.transform.rotation=Quaternion.Euler(0,yaw+180,0);}
    public void SetTestDrive(Vector3 direction,bool run,bool jump){testDriving=true;testInput=direction;testRun=run;testJump=jump;}
    public void SetReviewCamera(Vector3 position,Vector3 target){view.transform.position=position;view.transform.LookAt(target);enabled=false;}
    void LateUpdate()
    {
        if(view==null)return;Vector3 target=transform.position+Vector3.up*(vehicle!=null&&vehicle.Driving?1.25f:2.6f);
        if(!cameraInitialized){cameraAnchor=target;anchorVelocity=Vector3.zero;currentCameraDistance=distance;cameraDistanceVelocity=0;cameraInitialized=true;}
        cameraAnchor=Vector3.SmoothDamp(cameraAnchor,target,ref anchorVelocity,.07f);
        if(Physics.CheckSphere(cameraAnchor,.28f,1<<8,QueryTriggerInteraction.Ignore))cameraAnchor=target;
        Vector3 backwards=Quaternion.Euler(pitch,yaw,0)*Vector3.back;RaycastHit hit;float allowed=distance;
        if(Physics.SphereCast(cameraAnchor,.28f,backwards,out hit,distance,1<<8,QueryTriggerInteraction.Ignore))allowed=Mathf.Max(.15f,hit.distance-.08f);
        if(allowed<currentCameraDistance){currentCameraDistance=allowed;cameraDistanceVelocity=0;}else currentCameraDistance=Mathf.SmoothDamp(currentCameraDistance,allowed,ref cameraDistanceVelocity,.18f);
        view.transform.position=cameraAnchor+backwards*currentCameraDistance;view.transform.LookAt(cameraAnchor);
    }
    void OnDestroy(){Capture(false);}
}
