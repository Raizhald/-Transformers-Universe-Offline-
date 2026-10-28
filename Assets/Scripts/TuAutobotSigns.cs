using System;
using System.IO;
using UnityEngine;

public sealed class TuAutobotSigns : MonoBehaviour
{
    [Serializable] sealed class Sign {public string text="";public Vector3 position=Vector3.zero;public float worldWidth=20.48f,worldHeight=4.96f;}
    [Serializable] sealed class Catalog {public Sign[] signs=new Sign[0];}
    Sign[] signs;GUIStyle panel,label;TuHangarPlayer player;
    public int SourceSignCount {get{return signs==null?0:signs.Length;}}
    void Awake(){signs=JsonUtility.FromJson<Catalog>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"autobot-signs.json"))).signs;}
    void OnGUI()
    {
        if(player==null)player=FindAnyObjectByType<TuHangarPlayer>();
        if(player==null||!player.enabled||player.ViewCamera==null)return;
        if(panel==null){panel=TuUiSkin.Create(Resources.Load<Texture2D>("Signage/HUD_Sinage_Frame"),24,26,38,25);label=new GUIStyle(GUI.skin.label){font=Resources.Load<Font>("Signage/OriginalSignage"),fontSize=22,alignment=TextAnchor.MiddleCenter};label.normal.textColor=new Color(.85f,1,1);}
        Camera camera=player.ViewCamera;GUI.depth=10;
        foreach(Sign sign in signs)
        {
            Vector3 p=camera.WorldToScreenPoint(sign.position);if(p.z<.1f||Vector3.Distance(camera.transform.position,sign.position)>150)continue;
            Vector3 dir=(sign.position-camera.transform.position).normalized;
            if(Physics.Linecast(camera.transform.position,sign.position-dir*.5f,1<<8,QueryTriggerInteraction.Ignore))continue;
            float scale=Screen.height/(2*Mathf.Tan(camera.fieldOfView*Mathf.Deg2Rad*.5f))*sign.worldWidth/256/p.z;
            Matrix4x4 old=GUI.matrix;GUI.matrix=Matrix4x4.TRS(new Vector3(p.x-128*scale,Screen.height-p.y-31*scale,0),Quaternion.identity,new Vector3(scale,scale,1));
            GUI.Box(new Rect(0,0,256,62),GUIContent.none,panel);GUI.Label(new Rect(12,15,232,30),sign.text,label);GUI.matrix=old;
        }
    }
}
