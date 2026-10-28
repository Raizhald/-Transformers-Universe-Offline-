using System.Collections.Generic;
using UnityEngine;

// Original Autobot ambience. Shared archive A004 metallic footsteps are a temporary fallback:
// the available A001 event table references missing _Clips0 audio bundles.
public sealed class TuAutobotAudio : MonoBehaviour
{
    public TuHangarPlayer player;
    public TuAutobotHangar hangar;
    [System.Serializable] sealed class LoopInfo {public string name="";public Vector3 position=Vector3.zero;public float volume=.1f,range=40;}
    [System.Serializable] sealed class Ambience {public LoopInfo[] audio=new LoopInfo[0];}
    public int LoadedClipCount {get{return clips.Count;}}
    public int StepsPlayed {get;private set;}
    readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
    AudioSource feet,mechanics;CharacterController controller;bool grounded,muted;float previousY,airTime;int previousHalf=-1,stepIndex;
    void Start()
    {
        foreach(AudioClip clip in Resources.LoadAll<AudioClip>("Audio"))clips[clip.name]=clip;
        foreach(AudioClip clip in Resources.LoadAll<AudioClip>("AutobotAudio"))clips[clip.name]=clip;
        controller=GetComponent<CharacterController>();feet=Source(gameObject,.8f,3,35);mechanics=Source(gameObject,.65f,3,24);grounded=controller.isGrounded;previousY=transform.position.y;
        var data=JsonUtility.FromJson<Ambience>(System.IO.File.ReadAllText(System.IO.Path.Combine(Application.streamingAssetsPath,"autobot-hangar.json")));
        foreach(LoopInfo item in data.audio)Loop(item.name,item.position,item.volume,item.range);
    }
    static AudioSource Source(GameObject go,float spatial,float near,float far){var s=go.AddComponent<AudioSource>();s.playOnAwake=false;s.spatialBlend=spatial;s.minDistance=near;s.maxDistance=far;s.rolloffMode=AudioRolloffMode.Linear;s.dopplerLevel=0;return s;}
    void Loop(string clip,Vector3 position,float volume,float range){AudioClip c;if(!clips.TryGetValue(clip,out c))return;var go=new GameObject("Archive audio: "+clip);go.transform.position=position;go.transform.SetParent(hangar.transform,true);var s=Source(go,1,8,range);s.loop=true;s.volume=volume;s.clip=c;s.Play();}
    void Play(AudioSource source,string name,float volume){AudioClip c;if(clips.TryGetValue(name,out c))source.PlayOneShot(c,volume);}
    void Update()
    {
        if(player==null||controller==null||!player.enabled)return;
        if((player.hud==null||!player.hud.Typing)&&Input.GetKeyDown(KeyCode.M)){muted=!muted;AudioListener.volume=muted?0:1;}
        if(player.vehicle!=null&&(player.vehicle.Driving||player.vehicle.Transforming))return;
        bool onFloor=controller.isGrounded;float delta=transform.position.y-previousY;previousY=transform.position.y;
        if(Mathf.Abs(delta)>2){grounded=onFloor;airTime=0;previousHalf=-1;return;}
        if(!onFloor){airTime+=Time.deltaTime;if(grounded&&delta>.01f)Play(mechanics,"a004_jump_0"+(Random.Range(0,2)+1),.4f);previousHalf=-1;}
        else{if(!grounded&&airTime>.15f)Play(feet,airTime>.7f?"a004_land_heavy_01":"a004_land_0"+(Random.Range(0,2)+1),.55f);airTime=0;}
        if(onFloor&&player.ActualSpeed>.3f)
        {
            int half=Mathf.FloorToInt(player.AnimationPhase*2);if(half!=previousHalf){StepsPlayed++;bool left=half==0;int variant=(stepIndex++/2)%4+1;feet.pitch=player.ActualSpeed>5.3f?1:.88f;Play(feet,"a004_run_"+(left?"left_":"right_")+variant.ToString("00"),.48f);if(stepIndex%2==0)Play(mechanics,"a004_foley_"+(stepIndex%4==0?"upper_":"lower_")+(stepIndex%3+1).ToString("00"),.16f);previousHalf=half;}
        }
        else previousHalf=-1;
        grounded=onFloor;
    }
    void OnDestroy(){AudioListener.volume=1;}
}
