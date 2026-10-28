using UnityEngine;

public sealed class TuHangarSigns : MonoBehaviour
{
    readonly Vector3[] positions={new Vector3(-580.5f,156.26f,-369.48f),new Vector3(-518.98f,164.46f,-321f)};
    readonly string[] labels={"HANGAR UPLINK","CENTRAL CITY"};
    Texture2D frame,detail;GUIStyle panel,text;
    void Awake(){frame=Resources.Load<Texture2D>("Signage/HUD_Sinage_Frame");detail=Resources.Load<Texture2D>("Signage/HUD_Sinage_AnimatedPart1");}
    void OnGUI()
    {
        var player=FindAnyObjectByType<TuHangarPlayer>();if(player==null||!player.enabled||player.ViewCamera==null)return;
        if(panel==null){panel=new GUIStyle();panel.normal.background=frame;panel.border=new RectOffset(24,38,26,25);text=new GUIStyle(GUI.skin.label){font=Resources.Load<Font>("Signage/OriginalSignage"),fontSize=22,alignment=TextAnchor.MiddleCenter};text.normal.textColor=new Color(.85f,1,1);}
        Camera camera=player.ViewCamera;GUI.depth=10;
        for(int k=0;k<positions.Length;k++)
        {
            Vector3 p=positions[k],screen=camera.WorldToScreenPoint(p);float distance=Vector3.Distance(camera.transform.position,p);if(screen.z<.1f||distance>110)continue;
            Vector3 direction=(p-camera.transform.position).normalized;
            RaycastHit hit;if(Physics.Linecast(camera.transform.position,p-direction*.6f,out hit,1<<8,QueryTriggerInteraction.Ignore)&&hit.collider.GetComponentInParent<TuHangarDetails>()==null)continue;
            float scale=Screen.height/(2*Mathf.Tan(camera.fieldOfView*Mathf.Deg2Rad*.5f))* .08f/screen.z;
            Matrix4x4 old=GUI.matrix;GUI.matrix=Matrix4x4.TRS(new Vector3(screen.x-128*scale,Screen.height-screen.y-31*scale,0),Quaternion.identity,new Vector3(scale,scale,1));
            GUI.Box(new Rect(0,0,256,62),GUIContent.none,panel);GUI.Label(new Rect(20,15,216,30),labels[k],text);
            Color color=GUI.color;GUI.color=new Color(1,1,1,.45f+.45f*Mathf.Sin(Time.unscaledTime*2));GUI.DrawTexture(new Rect(240,45,7,7),detail);GUI.color=color;GUI.matrix=old;
        }
    }
}
