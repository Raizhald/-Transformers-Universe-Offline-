using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class TuHud : MonoBehaviour
{
    public TuHangarPlayer player;
    public string PlayerName="A004",FactionName="Decepticons",RobotName="A004";
    public bool MapNorthUp;
    public bool ChatVisible=true,ModeVisible=false;
    [Serializable] sealed class SpriteInfo {public string name="";public int left=0,top=0,right=0,bottom=0;}
    [Serializable] sealed class SpriteCatalog {public SpriteInfo[] sprites=new SpriteInfo[0];}
    readonly Dictionary<string,GUIStyle> slices=new Dictionary<string,GUIStyle>();
    Vector4 mapBounds=new Vector4(-673.6888f,-510.5042f,309.6907f,285.9803f);Vector3 mapPortal=new Vector3(-519,168,-490);
    Texture2D mapPlan,localMap;Color32[] mapPixels,localPixels;
    public bool Typing {get;private set;}
    public int SelectedMode {get;private set;}
    public int LocalMessageCount {get{return messages.Count;}}
    public float Health {get;private set;}=1;
    public string[] ModeNames={"FIRST AVAILABLE (PVP)","ELIMINATION (PVP)","METEOR STORM (PVP)","CRISIS (PVE)"};
    readonly Dictionary<string,Texture2D> images=new Dictionary<string,Texture2D>();
    readonly List<string> messages=new List<string>();
    GUIStyle body,title,small,chat,inputStyle;string input="",notice="";float noticeUntil;bool help,roster,focusChat,clearFocus;int chatOpenedFrame;
    Vector2 LogicalMouse {get{return new Vector2(Input.mousePosition.x,Screen.height-Input.mousePosition.y)/Mathf.Max(.1f,Scale);}}
    float Scale {get{return Mathf.Min(Screen.width/1920f,Screen.height/1080f);}}
    float Width {get{return Screen.width/Mathf.Max(.1f,Scale);}}
    float Height {get{return Screen.height/Mathf.Max(.1f,Scale);}}
    public Rect ChatRect {get{return player!=null&&player.MouseCaptured?new Rect(14,Height-258,480,174):new Rect(4,Height-292,505,245);}}
    public Rect ModeRect {get{return new Rect(Width-334,Height-410,330,360);}}
    Rect RadarRect {get{return new Rect(Width-302,57,296,272);}}
    void Awake()
    {
        foreach(Texture2D image in Resources.LoadAll<Texture2D>("Hud"))images[image.name]=image;
        TextAsset metadata=Resources.Load<TextAsset>("Hud/skin-metadata");if(metadata!=null)foreach(SpriteInfo s in JsonUtility.FromJson<SpriteCatalog>(metadata.text).sprites){Texture2D image;if((s.left+s.top+s.right+s.bottom)>0&&images.TryGetValue(s.name,out image)){slices[s.name]=TuUiSkin.Create(image,s.left,s.top,s.right,s.bottom);}}
        SetAreaMap("OriginalMinimap.png",mapBounds,mapPortal);
        AddMessage("TELETRAAN MILITARY OPERATING SYSTEM online…");AddMessage("Connected to local hangar. Communications open.");AddMessage("Enter – local chat   /help – controls");
    }
    public void SetAreaMap(string file,Vector4 bounds,Vector3 portal)
    {
        if(mapPlan!=null)Destroy(mapPlan);if(localMap!=null)Destroy(localMap);mapBounds=bounds;mapPortal=portal;
        mapPlan=new Texture2D(2,2,TextureFormat.RGBA32,false);mapPlan.LoadImage(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,file)));mapPixels=mapPlan.GetPixels32();localMap=new Texture2D(256,256,TextureFormat.RGBA32,false);localPixels=new Color32[256*256];RefreshMap();
    }
    void OnDestroy(){if(mapPlan!=null)Destroy(mapPlan);if(localMap!=null)Destroy(localMap);}
    void Start(){ShowNotice((FactionName=="Autobots"?"AUTOBOT":"DECEPTICON")+" BASE  •  UNITY v23",4);}
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftControl)&&!Typing)player.SetMouseMode(player.MouseCaptured);
        if(Input.GetKeyDown(KeyCode.Return)){if(!Typing)BeginChat();else if(Time.frameCount>chatOpenedFrame){SubmitChat(input);EndChat();}}
        if(Input.GetKeyDown(KeyCode.Escape)&&Typing)EndChat();
        if(!Typing&&Input.GetKeyDown(KeyCode.F1)){help=!help;roster=false;if(help)player.SetMouseMode(true);}
    }
    public void SetHealth(float value){Health=Mathf.Clamp01(value);}
    public void BeginChat(){ChatVisible=true;Typing=true;focusChat=true;chatOpenedFrame=Time.frameCount;player.SetMouseMode(true);}
    public void EndChat(){Typing=false;input="";clearFocus=true;}
    public bool SubmitChat(string text)
    {
        text=(text??"").Trim();if(text.Length==0)return false;if(text.Length>180)text=text.Substring(0,180);
        if(text.Equals("/help",StringComparison.OrdinalIgnoreCase)){help=true;AddMessage("P portal · H hologram · V effects · M audio · R reset · E travel");}
        else if(text.Equals("/clear",StringComparison.OrdinalIgnoreCase))messages.Clear();
        else AddMessage(PlayerName+": "+text);
        input="";return true;
    }
    public void SelectMode(int index){SelectedMode=Mathf.Clamp(index,0,ModeNames.Length-1);}
    public void QueueSelected(){AddMessage("Selected "+ModeNames[SelectedMode]+". Matchmaking is not connected — this version runs offline.");ShowNotice("OFFLINE  •  MODE SELECTED",4);}
    void AddMessage(string text){messages.Add(text);if(messages.Count>60)messages.RemoveAt(0);}
    void ShowNotice(string text,float seconds){notice=text;noticeUntil=Time.unscaledTime+seconds;}
    public Vector2 MapPosition(Vector3 world,Rect area)
    {
        Vector2 offset=new Vector2((world.x-mapBounds.x)/mapBounds.z-.5f,(world.z-mapBounds.y)/mapBounds.w-.5f);if(MapNorthUp)offset.y=-offset.y;offset=Vector2.ClampMagnitude(offset,.55f);return area.center+Vector2.Scale(offset,area.size);
    }
    public bool IsOverPanel(Vector2 screenPoint)
    {
        Vector2 p=new Vector2(screenPoint.x,Screen.height-screenPoint.y)/Mathf.Max(.1f,Scale);
        return (ChatVisible&&ChatRect.Contains(p))||(ModeVisible&&ModeRect.Contains(p))||new Rect(0,Height-43,420,43).Contains(p)||new Rect(Width-270,Height-43,270,43).Contains(p)||new Rect(Width-310,0,310,360).Contains(p)||new Rect(0,0,280,52).Contains(p)||help||roster;
    }
    void Styles()
    {
        if(body!=null)return;Font normal=Resources.Load<Font>("Hud/OriginalUiRegular")??Font.CreateDynamicFontFromOSFont("Arial",16);Font condensed=Resources.Load<Font>("Hud/OriginalUiBold")??Font.CreateDynamicFontFromOSFont("Impact",24);
        body=new GUIStyle(GUI.skin.label){font=normal,fontSize=17,alignment=TextAnchor.MiddleLeft,richText=false};body.normal.textColor=new Color(.85f,.91f,.92f);
        title=new GUIStyle(body){font=condensed,fontSize=24,alignment=TextAnchor.MiddleCenter};title.normal.textColor=Color.white;
        small=new GUIStyle(body){fontSize=12};small.normal.textColor=new Color(.58f,.66f,.68f);
        chat=new GUIStyle(body){fontSize=15,wordWrap=true,alignment=TextAnchor.UpperLeft};chat.normal.textColor=new Color(.85f,.49f,.20f);
        inputStyle=new GUIStyle(GUI.skin.textField){font=normal,fontSize=15};inputStyle.normal.background=null;inputStyle.focused.background=null;inputStyle.normal.textColor=new Color(.7f,.94f,.97f);inputStyle.focused.textColor=inputStyle.normal.textColor;inputStyle.padding=new RectOffset(6,5,3,2);
        foreach(GUIStyleState state in new GUIStyleState[]{inputStyle.normal,inputStyle.hover,inputStyle.active,inputStyle.focused,inputStyle.onNormal,inputStyle.onHover,inputStyle.onActive,inputStyle.onFocused}){state.background=null;state.textColor=new Color(.7f,.94f,.97f);}
    }
    void Image(string name,Rect rect,Color tint)
    {
        Texture2D image;if(!images.TryGetValue(name,out image))return;Color old=GUI.color;GUI.color=tint;GUIStyle skin;if(slices.TryGetValue(name,out skin))GUI.Box(rect,GUIContent.none,skin);else GUI.DrawTexture(rect,image,ScaleMode.StretchToFill,true);GUI.color=old;
    }
    void Image(string name,Rect rect){Image(name,rect,Color.white);}
    void Fill(Rect r,Color color){Color old=GUI.color;GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
    void Line(Vector2 a,Vector2 b,Color color,float thickness=1)
    {
        Matrix4x4 old=GUI.matrix;float angle=Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg;GUIUtility.RotateAroundPivot(angle,a);Fill(new Rect(a.x,a.y,(b-a).magnitude,thickness),color);GUI.matrix=old;
    }
    bool Button(Rect r,string label,string frame="HUD_ButtonBG",GUIStyle style=null)
    {
        bool hover=r.Contains(LogicalMouse)&&!player.MouseCaptured;Image(frame,r,hover?new Color(.8f,1,1):Color.white);GUI.Label(r,label,style??title);
        bool old=GUI.enabled;GUI.enabled=old&&!player.MouseCaptured;bool click=GUI.Button(r,GUIContent.none,GUIStyle.none);GUI.enabled=old;return click;
    }
    void OnGUI()
    {
        if(player==null)return;Styles();Matrix4x4 matrix=GUI.matrix;GUI.matrix=Matrix4x4.Scale(new Vector3(Scale,Scale,1));GUI.depth=-20;
        GUI.Label(new Rect(Width-86,Height-67,80,22),"BUILD v23",small);
        if(clearFocus){GUI.FocusControl(null);clearFocus=false;}
        bool oldEnabled=GUI.enabled;GUI.enabled=oldEnabled&&!(help||roster);Profile();Radar();if(ChatVisible)ChatPanel();if(ModeVisible)ModePanel();BottomBar();GUI.enabled=oldEnabled;
        if(Time.unscaledTime<noticeUntil){GUIStyle centered=new GUIStyle(small){alignment=TextAnchor.MiddleCenter};GUI.Label(new Rect(Width/2-320,18,640,30),notice,centered);}
        if(help)InfoPanel(false);else if(roster)InfoPanel(true);
        GUI.matrix=matrix;
    }
    void Profile()
    {
        Image("Hangar_HeaderWidgetFrame",new Rect(3,4,274,50));Image("Social_Icon_GroupLeader",new Rect(11,17,23,24));GUI.Label(new Rect(39,21,22,24),"1",body);GUI.Label(new Rect(66,5,192,23),PlayerName,body);
        Fill(new Rect(67,28,178,13),new Color(.06f,.12f,.14f));Fill(new Rect(70,31,172*Health,7),new Color(.20f,.61f,.65f));
        float x=Width-298;Image("Hangar_HeaderWidgetFrame",new Rect(x,4,294,48));Image("HUD_Currency_Raw",new Rect(x+24,15,25,25));GUI.Label(new Rect(x+67,12,40,25),"0",body);Image("HUD_Currency_Relic",new Rect(x+164,15,25,25));GUI.Label(new Rect(x+211,12,40,25),"0",body);
    }
    void Radar()
    {
        Rect r=RadarRect;DrawMap(r);
    }
    void DrawMap(Rect r)
    {
        Image("HUD_Radar_BG",r);Rect circle=new Rect(r.x+2,r.y+2,r.width*.89f,r.width*.89f);Image("HUD_Radar_Overlay",circle);GUI.DrawTexture(circle,localMap,ScaleMode.StretchToFill,true);
        Rect mapArea=new Rect(circle.center.x-circle.width*.37f,circle.center.y-circle.height*.37f,circle.width*.74f,circle.height*.74f);
        Vector2 portal=MapPosition(mapPortal,mapArea);Image("HUD_Radar_Dot",new Rect(portal.x-5,portal.y-5,10,10),new Color(.8f,.2f,1));
        Vector2 position=MapPosition(player.transform.position,mapArea);Image("HUD_Radar_Dot",new Rect(position.x-9,position.y-9,18,18),new Color(.1f,.9f,1,.8f));
        Matrix4x4 old=GUI.matrix;GUI.matrix=TuUiSkin.Around(old,position,-player.VisualYaw);Image("HUD_Radar_Player",new Rect(position.x-7,position.y-8,14,16));GUI.matrix=old;
    }
    void RefreshMap()
    {
        int w=mapPlan.width,h=mapPlan.height;
        for(int y=0;y<256;y++)for(int x=0;x<256;x++)
        {
            float u=(x+.5f)/256,v=(y+.5f)/256,dx=u-.5f,dy=v-.5f;int index=y*256+x;
            if(dx*dx+dy*dy>.2025f){localPixels[index]=new Color32(0,0,0,0);continue;}
            float mu=dx/.74f+.5f,mv=dy/.74f+.5f;int sx=Mathf.FloorToInt(mu*w),sy=Mathf.FloorToInt(mv*h);Color32 sample=sx>=0&&sx<w&&sy>=0&&sy<h?mapPixels[sy*w+sx]:new Color32(0,0,0,0);
            float alpha=Mathf.Clamp01(sample.a/255f*2.4f);localPixels[index]=new Color32((byte)Mathf.Lerp(3,sample.r,alpha),(byte)Mathf.Lerp(9,sample.g,alpha),(byte)Mathf.Lerp(12,sample.b,alpha),255);
        }
        localMap.SetPixels32(localPixels);localMap.Apply(false);
    }
    void ChatPanel()
    {
        Rect r=ChatRect;Image(player.MouseCaptured?"Social_Chat_Frame":"Social_PanelBG",r);
        if(player.MouseCaptured){GUI.BeginGroup(new Rect(r.x+14,r.y+10,r.width-28,r.height-18));int start=Mathf.Max(0,messages.Count-7);float lineY=0;for(int k=start;k<messages.Count;k++){float lineH=chat.CalcHeight(new GUIContent(messages[k]),r.width-28);GUI.Label(new Rect(0,lineY,r.width-28,lineH),messages[k],chat);lineY+=lineH+3;}GUI.EndGroup();return;}
        GUI.Label(new Rect(r.x+60,r.y+5,r.width-120,35),"LOCAL CHAT",title);
        if(Button(new Rect(r.xMax-31,r.y+10,19,19),"","Chat_Close",body)){ChatVisible=false;EndChat();}
        GUI.BeginGroup(new Rect(r.x+24,r.y+51,r.width-48,151));int from=Mathf.Max(0,messages.Count-7);float y=0;
        for(int k=from;k<messages.Count;k++){float h=Mathf.Max(21,chat.CalcHeight(new GUIContent(messages[k]),r.width-50));GUI.Label(new Rect(0,y,r.width-50,h),messages[k],chat);y+=h+2;}
        GUI.EndGroup();Rect box=new Rect(r.x+22,r.yMax-33,r.width-65,23);Image("Social_TextBox",box,new Color(.16f,.28f,.3f));
        bool old=GUI.enabled;GUI.enabled=old&&!player.MouseCaptured;GUI.SetNextControlName("TuChatInput");input=GUI.TextField(box,input,180,inputStyle);GUI.enabled=old;
        if(focusChat){GUI.FocusControl("TuChatInput");focusChat=false;}
        if(Event.current.type==EventType.MouseDown&&box.Contains(LogicalMouse)&&!player.MouseCaptured){Typing=true;chatOpenedFrame=Time.frameCount;}
        if(Button(new Rect(r.xMax-37,r.yMax-34,25,25),"","Social_Chat_Button_Enter",body)){SubmitChat(input);EndChat();}
    }
    void ModePanel()
    {
        Rect r=ModeRect;Image("Social_PanelBGFull",r);GUI.Label(new Rect(r.x+42,r.y+5,r.width-85,36),"SELECT MODE",title);
        if(Button(new Rect(r.xMax-31,r.y+11,19,19),"×","HUD_ButtonBG",body))ModeVisible=false;
        GUI.Label(new Rect(r.x+26,r.y+51,r.width-40,20),"PRESS LCTRL TO ENTER MOUSE MODE",small);
        Color[] colors={Color.white,new Color(1,.45f,.12f),new Color(1,.8f,.15f),new Color(.17f,.75f,1)};
        for(int k=0;k<ModeNames.Length;k++)
        {
            Rect option=new Rect(r.x+28,r.y+78+k*56,274,51);if(Button(option,"",k==SelectedMode?"Social_PanelSelected":"Social_PanelBGFull"))SelectMode(k);
            Image(k==0?"Grounbridge_Icon_Random":k==1?"Grounbridge_Icon_TDMGM":k==2?"Grounbridge_Icon_ERushGM":"Grounbridge_Icon_EQuestGM",new Rect(option.x+10,option.y+11,30,30));
            GUI.Label(new Rect(option.x+48,option.y+5,222,22),ModeNames[k],new GUIStyle(body){fontSize=16,fontStyle=FontStyle.Bold});GUI.Label(new Rect(option.x+48,option.y+28,220,17),"OFFLINE  •  LOCAL SELECTION",small);
        }
        if(Button(new Rect(r.x+18,r.yMax-47,135,35),"CANCEL","Queue_ButtonBG",new GUIStyle(title){fontSize=17}))ModeVisible=false;
        if(Button(new Rect(r.x+176,r.yMax-47,135,35),"QUEUE","Queue_ButtonBG",new GUIStyle(title){fontSize=17}))QueueSelected();
    }
    void BottomBar()
    {
        string[] icons={"Hangar_Icon_08","Hangar_Icon_01","Hangar_Icon_02","Hangar_Icon_03","Hangar_Icon_04"};
        for(int k=0;k<5;k++){Rect r=new Rect(6+k*83,Height-39,78,34);if(Button(r,"")){if(k==0){ChatVisible=!ChatVisible;if(!ChatVisible)EndChat();}else if(k>=3){help=!help;roster=false;}else{roster=!roster;help=false;}}Image(icons[k],new Rect(r.center.x-12,r.y+6,24,24));}
        if(Button(new Rect(Width-263,Height-39,84,34),"◉","Queue_ButtonBG"))ModeVisible=!ModeVisible;
        if(Button(new Rect(Width-175,Height-39,84,34),"", "HUD_ButtonBG"))roster=!roster;Image("Social_Button_AddtoGroup",new Rect(Width-148,Height-33,25,25));
        if(Button(new Rect(Width-87,Height-39,81,34),"?","HUD_ButtonBG"))help=!help;
    }
    void InfoPanel(bool players)
    {
        Rect r=new Rect(Width/2-260,Height/2-200,520,370);Image("Social_Chat_Frame",r);GUI.Label(new Rect(r.x+50,r.y+6,420,35),players?"SQUAD / PLAYERS":"CONTROLS",title);
        string text=players?"1 PLAYER IN LOCAL SESSION\n\n"+PlayerName+"  •  "+RobotName+"\n\n"+(FactionName=="Autobots"?"Autobot":"Decepticon")+" Hangar":"WASD / arrows — move\nShift — run   Space — jump\nMouse — camera   Wheel — zoom\nLCTRL / Esc — release mouse\nEnter — local chat   M — mute audio\nP — portal   H — hologram\nV — effects   R — reset   F2 — main menu"+(FactionName=="Autobots"?"\nE — enter city / return to hangar":"\nE — travel   F — transform outside\nWASD — drive   Space — handbrake   Shift — boost")+"";
        GUI.Label(new Rect(r.x+32,r.y+65,460,250),text,new GUIStyle(body){wordWrap=true,alignment=TextAnchor.UpperLeft});if(Button(new Rect(r.xMax-36,r.y+10,22,22),"×","HUD_ButtonBG",body)){help=false;roster=false;}
    }

}
