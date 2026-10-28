using System;
using System.IO;
using System.Collections;
using UnityEngine;

// Original A004 rigs and sampled transformation clips; newly authored WheelCollider driving.
public sealed class TuVehicleMode : MonoBehaviour
{
    [Serializable] sealed class Timing {public float duration=1.7f,switchTime=.32f;}
    [Serializable] sealed class Timings {public Timing r2v=new Timing(),v2r=new Timing();}
    [Serializable] sealed class WheelRecord {public string name="";public Vector3 position=Vector3.zero;public bool front=false;}
    [Serializable] sealed class Config {public Timings timings=new Timings();public WheelRecord[] wheels=new WheelRecord[0];}
    public TuHangarPlayer player;
    public bool Driving {get;private set;}
    public bool Transforming {get;private set;}
    public float SpeedKph {get{return body!=null?body.linearVelocity.magnitude*3.6f:0;}}
    public Rigidbody Body {get{return body;}}
    public WheelCollider[] Wheels {get{return wheels;}}
    public int GroundedWheelCount {get{return GroundedWheels();}}
    bool testControls;float testThrottle,testSteer;bool testBrake;
    public void SetTestControls(float throttle,float steer,bool brake){testControls=true;testThrottle=throttle;testSteer=steer;testBrake=brake;}
    public void ClearTestControls(){testControls=false;}
    Config config;TuPackedModel car,preview;GameObject bodyRoot;Rigidbody body;WheelCollider[] wheels;Transform[] wheelBones;Quaternion[] boneRotations;float[] spin;float wheelbase=2.212f;
    CharacterController character;AudioSource transformSound;string notice="";float noticeUntil;Vector3 transitionPosition;Quaternion transitionRotation;
    public void Initialize(TuHangarPlayer owner)
    {
        player=owner;character=owner.GetComponent<CharacterController>();config=JsonUtility.FromJson<Config>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"a004-car-config.json")));
        transformSound=gameObject.AddComponent<AudioSource>();transformSound.playOnAwake=false;transformSound.spatialBlend=.3f;transformSound.volume=.65f;
    }
    void Notice(string text){notice=text;noticeUntil=Time.unscaledTime+3;}
    void Update()
    {
        if(player==null||!player.enabled)return;
        if((player.hud==null||!player.hud.Typing)&&Input.GetKeyDown(KeyCode.F))Toggle();
        if(Driving){player.transform.position=body.position;AnimateWheels();if(body.position.y<player.travel.MinimumSafeY)player.travel.ResetCurrentArea();}
    }
    void LoadModels()
    {
        if(car!=null)return;
        bodyRoot=new GameObject("A004 drive physics");bodyRoot.layer=9;bodyRoot.SetActive(false);body=bodyRoot.AddComponent<Rigidbody>();body.mass=2000;body.centerOfMass=new Vector3(0,.25f,.1f);body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;body.maxAngularVelocity=3;
        var lower=bodyRoot.AddComponent<BoxCollider>();lower.center=new Vector3(0,.72f,.18f);lower.size=new Vector3(1.95f,.72f,3.75f);var cabin=bodyRoot.AddComponent<BoxCollider>();cabin.center=new Vector3(0,1.32f,-.08f);cabin.size=new Vector3(1.7f,.8f,2.25f);
        car=TuPackedModel.Load(Path.Combine(Application.streamingAssetsPath,"a004-car.tupack"),"Original A004 vehicle");car.root.transform.SetParent(bodyRoot.transform,false);car.root.transform.localRotation=Quaternion.Euler(0,180,0);car.Pose(car.FindMotion("CarIdle"),0,1);
        wheels=new WheelCollider[config.wheels.Length];wheelBones=new Transform[wheels.Length];boneRotations=new Quaternion[wheels.Length];spin=new float[wheels.Length];
        for(int k=0;k<wheels.Length;k++)
        {
            var source=config.wheels[k];var go=new GameObject("Physics "+source.name);go.layer=9;go.transform.SetParent(bodyRoot.transform,false);go.transform.localPosition=source.position+Vector3.up*.15f;var wheel=go.AddComponent<WheelCollider>();wheel.mass=30;wheel.radius=.37f;wheel.suspensionDistance=.22f;wheel.forceAppPointDistance=.25f;wheel.ConfigureVehicleSubsteps(8,8,12);wheel.suspensionSpring=new JointSpring{spring=40000,damper=6000,targetPosition=.5f};var friction=wheel.forwardFriction;friction.stiffness=1.8f;wheel.forwardFriction=friction;friction=wheel.sidewaysFriction;friction.stiffness=2;wheel.sidewaysFriction=friction;wheels[k]=wheel;
            foreach(var node in car.nodes)if(node.name==source.name){wheelBones[k]=node;boneRotations[k]=Quaternion.Inverse(body.rotation)*node.rotation;break;}
        }
        preview=TuPackedModel.Load(Path.Combine(Application.streamingAssetsPath,"a004-transform.tupack"),"Original A004 transformation");preview.root.SetActive(false);
    }
    public bool Toggle()
    {
        if(Driving||Transforming){ReturnToRobot();return true;}
        if(player.travel==null||!player.travel.IsExterior||player.travel.IsLoading){Notice("VEHICLE MODE IS AVAILABLE OUTSIDE");return false;}
        if(!Driving&&!character.isGrounded){Notice("LAND BEFORE TRANSFORMING");return false;}
        Quaternion facing=Quaternion.Euler(0,player.model.root.transform.eulerAngles.y+180,0);Vector3 foot=player.transform.position;
        bool blocked=Physics.CheckBox(foot+facing*new Vector3(0,1,.2f),new Vector3(1.04f,.62f,2.05f),facing,1<<8,QueryTriggerInteraction.Ignore);
        if(blocked){Notice("NOT ENOUGH ROOM TO TRANSFORM");return false;}
        LoadModels();transitionPosition=foot;transitionRotation=facing*Quaternion.Euler(0,180,0);StartCoroutine(Change(!Driving));return true;
    }
    public void ReturnToRobot()
    {
        if(!Driving&&!Transforming)return;
        Vector3 origin=Driving?body.position:transitionPosition;Vector3 forward=Driving?body.transform.forward:transitionRotation*Vector3.back;forward.y=0;float yaw=forward.sqrMagnitude>.01f?Quaternion.LookRotation(forward).eulerAngles.y:0;
        Vector3 place;bool clear=FindRobotSpace(origin,out place);
        if(Driving&&clear&&SpeedKph<7&&Vector3.Dot(body.transform.up,Vector3.up)>.8f&&GroundedWheels()>=2&&Vector3.Distance(place,body.position)<1.5f)
        {
            transitionPosition=place;transitionRotation=Quaternion.Euler(0,yaw+180,0);StartCoroutine(Change(false));return;
        }
        ForceRobot();if(clear)player.Teleport(place,yaw,12);else if(player.travel!=null)player.travel.ResetCurrentArea();else player.Teleport(TuPrototypeBootstrap.Spawn,90,12);
        var clip=Resources.Load<AudioClip>("VehicleAudio/a004_transform_v2r");if(clip!=null)transformSound.PlayOneShot(clip);Notice(clear?"ROBOT MODE":"ROBOT MODE - RECOVERED AT ENTRY");
    }
    bool FindRobotSpace(Vector3 origin,out Vector3 place)
    {
        for(int ring=0;ring<=8;ring++)for(int side=0;side<(ring==0?1:16);side++)
        {
            float angle=side*Mathf.PI/8;Vector3 ray=origin+new Vector3(Mathf.Cos(angle)*ring*1.25f,5,Mathf.Sin(angle)*ring*1.25f);RaycastHit hit;
            if(!Physics.Raycast(ray,Vector3.down,out hit,20,1<<8,QueryTriggerInteraction.Ignore)||hit.normal.y<.64f)continue;
            Vector3 candidate=hit.point+Vector3.up*.1f;if(Physics.CheckCapsule(candidate+Vector3.up*.78f,candidate+Vector3.up*2.62f,.68f,1<<8,QueryTriggerInteraction.Ignore))continue;
            place=candidate;return true;
        }
        place=Vector3.zero;return false;
    }
    IEnumerator Change(bool toCar)
    {
        Transforming=true;character.enabled=false;if(Driving){body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;body.isKinematic=true;bodyRoot.SetActive(false);}Driving=false;player.model.root.SetActive(false);preview.root.transform.SetPositionAndRotation(transitionPosition,transitionRotation);preview.root.SetActive(true);
        string alias=toCar?"R2V":"V2R";Timing timing=toCar?config.timings.r2v:config.timings.v2r;var motion=preview.FindMotion(alias);var clip=Resources.Load<AudioClip>("VehicleAudio/a004_transform_"+(toCar?"r2v":"v2r"));if(clip!=null)transformSound.PlayOneShot(clip);
        float time=0;while(time<timing.duration)
        {
            preview.Pose(motion,Mathf.Min(time,motion.duration-.00001f),1);bool vehicleVisible=toCar?time>=timing.switchTime:time<timing.switchTime;foreach(var node in preview.nodes){if(node.name=="Robot")node.gameObject.SetActive(!vehicleVisible);if(node.name=="Vozidlo")node.gameObject.SetActive(vehicleVisible);}time+=Time.deltaTime;yield return null;
        }
        preview.root.SetActive(false);Transforming=false;
        if(toCar){bodyRoot.transform.SetPositionAndRotation(transitionPosition+Vector3.up*.15f,transitionRotation*Quaternion.Euler(0,180,0));bodyRoot.SetActive(true);body.isKinematic=false;body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;Driving=true;player.transform.position=body.position;}
        else{player.model.root.SetActive(true);player.Teleport(transitionPosition,transitionRotation.eulerAngles.y+180,12);}
    }
    int GroundedWheels(){int n=0;if(wheels!=null)foreach(var wheel in wheels)if(wheel.isGrounded)n++;return n;}
    void FixedUpdate()
    {
        if(!Driving||Transforming||!player.enabled)return;
        bool control=testControls||player.MouseCaptured&&!player.hud.Typing&&Application.isFocused;float throttle=control&&Input.GetKey(KeyCode.W)?1:control&&Input.GetKey(KeyCode.S)?-1:0,steer=control&&Input.GetKey(KeyCode.A)?-1:control&&Input.GetKey(KeyCode.D)?1:0;
        if(testControls){throttle=testThrottle;steer=testSteer;}
        float forward=Vector3.Dot(body.linearVelocity,body.transform.forward),limit=control&&Input.GetKey(KeyCode.LeftShift)?34:26;bool braking=throttle*forward<-.6f;float motor=braking?0:throttle*1100;if(forward>limit&&motor>0||forward<-8&&motor<0)motor=0;
        float angle=TuDriveMath.SteeringLimit(forward,wheelbase)*steer;bool handbrake=testControls?testBrake:control&&Input.GetKey(KeyCode.Space);
        for(int k=0;k<wheels.Length;k++){var wheel=wheels[k];wheel.steerAngle=config.wheels[k].front?angle:0;wheel.motorTorque=motor;wheel.brakeTorque=!control||braking?3500:handbrake&&!config.wheels[k].front?4500:throttle==0?100:0;var side=wheel.sidewaysFriction;side.stiffness=handbrake&&!config.wheels[k].front?.65f:2;wheel.sidewaysFriction=side;}
        ApplyAntiRoll(0,2);ApplyAntiRoll(1,3);
        if(GroundedWheels()>1)body.AddForce(-body.transform.up*(body.mass*.008f*body.linearVelocity.sqrMagnitude));
    }
    void ApplyAntiRoll(int left,int right)
    {
        WheelHit a,b;bool ga=wheels[left].GetGroundHit(out a),gb=wheels[right].GetGroundHit(out b);
        float ta=ga?(-wheels[left].transform.InverseTransformPoint(a.point).y-wheels[left].radius)/wheels[left].suspensionDistance:1;
        float tb=gb?(-wheels[right].transform.InverseTransformPoint(b.point).y-wheels[right].radius)/wheels[right].suspensionDistance:1;
        float force=Mathf.Clamp(ta-tb,-1,1)*4500;if(ga)body.AddForceAtPosition(wheels[left].transform.up*-force,wheels[left].transform.position);if(gb)body.AddForceAtPosition(wheels[right].transform.up*force,wheels[right].transform.position);
    }
    void AnimateWheels()
    {
        for(int k=0;k<wheels.Length;k++){if(wheelBones[k]==null)continue;Vector3 position;Quaternion rotation;wheels[k].GetWorldPose(out position,out rotation);spin[k]+=wheels[k].rpm*6*Time.deltaTime;wheelBones[k].position=position;wheelBones[k].rotation=body.rotation*Quaternion.Euler(spin[k],wheels[k].steerAngle,0)*boneRotations[k];}
    }
    public void ForceRobot()
    {
        if(!Driving&&!Transforming)return;StopAllCoroutines();Transforming=false;Driving=false;if(bodyRoot!=null){body.isKinematic=true;bodyRoot.SetActive(false);}if(preview!=null)preview.root.SetActive(false);player.model.root.SetActive(true);character.enabled=true;
    }
    void OnGUI()
    {
        if(player==null||!player.enabled)return;GUI.depth=-40;var style=new GUIStyle(GUI.skin.label){font=Resources.Load<Font>("Hud/OriginalUiRegular"),fontSize=Mathf.Max(15,Screen.height/48),alignment=TextAnchor.MiddleCenter};style.normal.textColor=Color.white;
        string text=Time.unscaledTime<noticeUntil?notice:Transforming?"TRANSFORMING":Driving?Mathf.RoundToInt(SpeedKph)+" KM/H   ·   [ F ] ROBOT   ·   SPACE HANDBRAKE":player.travel!=null&&player.travel.IsExterior?"[ F ] TRANSFORM TO VEHICLE":"";if(text.Length>0)GUI.Label(new Rect(Screen.width/2-360,Screen.height-160,720,40),text,style);
    }
    void OnDestroy(){if(bodyRoot!=null)Destroy(bodyRoot);if(preview!=null)Destroy(preview.root);}
}
