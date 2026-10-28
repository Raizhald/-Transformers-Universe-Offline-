using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class TuFrontEnd : MonoBehaviour
{
    public readonly TuStartupState Flow=new TuStartupState();
    public TuPrototypeBootstrap bootstrap;
    [Serializable] sealed class Slice {public string name="";public int left=0,right=0,top=0,bottom=0;}
    [Serializable] sealed class Slices {public Slice[] sprites=new Slice[0];}
    readonly Dictionary<string,GUIStyle> skins=new Dictionary<string,GUIStyle>();
    readonly Dictionary<string,Texture2D> images=new Dictionary<string,Texture2D>();
    Camera backgroundCamera;GUIStyle label,heading,small,field;Texture2D centerDisc;
    string commander="Commander",password="",localMessage="",loadingText="Preparing hangar…";float progress;bool requestLogin;
    public void Initialize(TuPrototypeBootstrap owner)
    {
        bootstrap=owner;foreach(Texture2D t in Resources.LoadAll<Texture2D>("FrontEnd"))images[t.name]=t;
        foreach(Texture2D t in Resources.LoadAll<Texture2D>("Hud"))if(!images.ContainsKey(t.name))images[t.name]=t;
        foreach(Texture2D t in Resources.LoadAll<Texture2D>("Signage"))if(!images.ContainsKey(t.name))images[t.name]=t;
        TextAsset skinData=Resources.Load<TextAsset>("FrontEnd/v7-sources");foreach(TextAsset data in new TextAsset[]{skinData,Resources.Load<TextAsset>("Hud/skin-metadata")})foreach(Slice slice in JsonUtility.FromJson<Slices>(data.text).sprites){Texture2D image;if(images.TryGetValue(slice.name,out image)&&slice.left+slice.right+slice.top+slice.bottom>0){skins[slice.name]=TuUiSkin.Create(image,slice.left,slice.top,slice.right,slice.bottom);}}
        GameObject go=new GameObject("Startup background camera");backgroundCamera=go.AddComponent<Camera>();backgroundCamera.cullingMask=0;backgroundCamera.clearFlags=CameraClearFlags.SolidColor;backgroundCamera.backgroundColor=Color.black;backgroundCamera.depth=100;
        commander=PlayerPrefs.GetString("TU.CommanderName","Commander");Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
        centerDisc=new Texture2D(128,128,TextureFormat.RGBA32,false);Color32[] pixels=new Color32[128*128];for(int y=0;y<128;y++)for(int x=0;x<128;x++){float r=Vector2.Distance(new Vector2(x,y),new Vector2(63.5f,63.5f));pixels[y*128+x]=new Color32(9,14,18,(byte)(r<62?255:0));}centerDisc.SetPixels32(pixels);centerDisc.Apply();
    }
    void Update()
    {
        if(requestLogin){requestLogin=false;Login();}
        if(Input.GetKeyDown(KeyCode.Return)){if(Flow.Current==TuStartupState.Screen.Launcher)PressPlay();else if(Flow.Current==TuStartupState.Screen.Login)Login();}
        if(Input.GetKeyDown(KeyCode.Escape))Flow.Back();
    }
    public void PressPlay(){if(Flow.Play())StartCoroutine(Splash());}
    IEnumerator Splash(){yield return new WaitForSecondsRealtime(1.1f);Flow.FinishSplash();}
    public bool Login()
    {
        bool success=Flow.Login(commander);password="";if(success){PlayerPrefs.SetString("TU.CommanderName",Flow.CommanderName);PlayerPrefs.Save();localMessage="";}return success;
    }
    public bool LoginAs(string name){commander=name;return Login();}
    public string CommanderInputName {get{return commander;}}
    public bool ChooseFaction(string faction){if(!Flow.SelectFaction(faction))return false;StartCoroutine(LoadHangar());return true;}
    public bool ChooseDecepticons(){return ChooseFaction("Decepticons");}
    public bool ChooseAutobots(){return ChooseFaction("Autobots");}
    IEnumerator LoadHangar()
    {
        float started=Time.realtimeSinceStartup;
        IEnumerator load=bootstrap.LoadHangar(Flow.CommanderName,Flow.Faction,delegate(float amount,string text){progress=amount;loadingText=text;});
        while(true){bool more;try{more=load.MoveNext();}catch(Exception e){Flow.Fail("Unable to load the hangar. "+e.Message);Debug.LogException(e);yield break;}if(!more)break;yield return load.Current;}
        float remaining=1.2f-(Time.realtimeSinceStartup-started);if(remaining>0)yield return new WaitForSecondsRealtime(remaining);
        Flow.FinishLoading();Destroy(backgroundCamera.gameObject);enabled=false;
    }
    void Styles()
    {
        if(label!=null)return;Font normal=Resources.Load<Font>("Hud/OriginalUiRegular")??Font.CreateDynamicFontFromOSFont("Arial",22),heavy=Resources.Load<Font>("Hud/OriginalUiBold")??Font.CreateDynamicFontFromOSFont("Impact",44);
        label=new GUIStyle(GUI.skin.label){font=normal,fontSize=24,alignment=TextAnchor.MiddleCenter};label.normal.textColor=Color.white;
        heading=new GUIStyle(label){font=heavy,fontSize=44};small=new GUIStyle(label){fontSize=18};small.normal.textColor=new Color(.64f,.7f,.73f);
        field=new GUIStyle(GUI.skin.textField){font=normal,fontSize=25,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(14,12,6,6)};field.normal.background=null;field.focused.background=null;field.normal.textColor=Color.white;field.focused.textColor=Color.white;
    }
    void Image(string name,Rect r,Color tint){Texture2D t;if(!images.TryGetValue(name,out t))return;Color old=GUI.color;GUI.color=tint;GUIStyle skin;if(skins.TryGetValue(name,out skin))GUI.Box(r,GUIContent.none,skin);else GUI.DrawTexture(r,t,ScaleMode.StretchToFill,true);GUI.color=old;}
    void Image(string name,Rect r){Image(name,r,Color.white);}
    void Fill(Rect r,Color c){Color old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
    bool Button(Rect r,string text,string art="Patcher_Hangar_Button_Yellow")
    {
        Image(art,r);GUI.Label(r,text,new GUIStyle(label){fontSize=28,fontStyle=FontStyle.Bold});return GUI.Button(r,GUIContent.none,GUIStyle.none);
    }
    void Backdrop(bool loading)
    {
        // Use one covering transform for every art layer, independently of the
        // fitted controls. This prevents foreground robots overflowing a smaller BG.
        Matrix4x4 old=GUI.matrix;float cover=Mathf.Max(Screen.width/1920f,Screen.height/1080f);
        GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1920*cover)/2,(Screen.height-1080*cover)/2,0),Quaternion.identity,new Vector3(cover,cover,1));
        if(loading){bool autobot=Flow.Faction=="Autobots";Image(autobot?"Game_Loading_Screen_Test":"Game_Loading_Screen_Decepticon",new Rect(-58,0,2036,1080));Image(autobot?"Game_Loading_Screen_Warrior_Macro":"Game_Loading_Screen_Warrior_Firebreaker",new Rect(224,137,1513,943));}
        else{Texture2D bg=images["Login_BG_New"];GUI.DrawTexture(new Rect(0,0,1920,1080),bg,ScaleMode.ScaleAndCrop,true);Image("Login_Autobot_New",new Rect(-650,137,1920,943));Image("Login_Decepticon_New",new Rect(780,15,1920,1065));}
        GUI.matrix=old;
    }
    void Background(){Image("TitleLogo",new Rect(485,88,950,193));}
    void Spinner(Rect r,bool percent)
    {
        Matrix4x4 matrix=GUI.matrix;GUI.matrix=TuUiSkin.Around(matrix,r.center,Time.unscaledTime*28);Image("Loadingwheel_01",r);GUI.matrix=matrix;
        GUI.matrix=TuUiSkin.Around(matrix,r.center,-Time.unscaledTime*48);Image("Loadingwheel_02",new Rect(r.x+9,r.y+9,r.width-18,r.height-18));GUI.matrix=matrix;
        Image("Loadingwheel_03",new Rect(r.x+21,r.y+21,r.width-42,r.height-42));
        GUI.DrawTexture(new Rect(r.center.x-r.width*.31f,r.center.y-r.height*.31f,r.width*.62f,r.height*.62f),centerDisc);
        GUI.Label(new Rect(r.x,r.center.y-19,r.width,28),"LOADING",new GUIStyle(label){fontSize=percent?19:15,fontStyle=FontStyle.Bold});
        if(percent)GUI.Label(new Rect(r.x,r.center.y+6,r.width,27),Mathf.RoundToInt(progress*100)+"%",new GUIStyle(label){fontSize=20});
    }
    void OnGUI()
    {
        Styles();float scale=Mathf.Min(Screen.width/1920f,Screen.height/1080f);Vector2 origin=new Vector2((Screen.width-1920*scale)/2,(Screen.height-1080*scale)/2);Matrix4x4 old=GUI.matrix;GUI.depth=-100;Fill(new Rect(0,0,Screen.width,Screen.height),Color.black);if(Flow.Current==TuStartupState.Screen.Login||Flow.Current==TuStartupState.Screen.FactionSelect||Flow.Current==TuStartupState.Screen.HangarLoading)Backdrop(Flow.Current==TuStartupState.Screen.HangarLoading);GUI.matrix=Matrix4x4.TRS(origin,Quaternion.identity,new Vector3(scale,scale,1));
        switch(Flow.Current)
        {
            case TuStartupState.Screen.Launcher:
                Image("HUD_Sinage_Frame",new Rect(466,194,988,642));Image("Patcher_Patcher_Background",new Rect(476,204,968,622));Image("Patcher_Patch",new Rect(494,260,932,403));
                Fill(new Rect(746,207,430,102),new Color(.035f,.045f,.052f));Image("Patcher_Patcher_Logo",new Rect(760,222,400,78));
                Image("Patcher_Patcher_InfoBoxBG",new Rect(494,675,932,35));GUI.Label(new Rect(505,676,900,32),"STATUS: Local files ready. Press PLAY to start.",new GUIStyle(label){fontSize=21,alignment=TextAnchor.MiddleLeft});
                Image("Patcher_Patcher_ProgressionBarBG",new Rect(494,714,932,43));Image("Patcher_Patcher_ProgressionBar",new Rect(499,719,922,33));
                if(Button(new Rect(844,785,232,45),"PLAY"))PressPlay();break;
            case TuStartupState.Screen.Splash:
                Image("TitleLogo",new Rect(710,489,500,102));Spinner(new Rect(1765,928,128,128),false);break;
            case TuStartupState.Screen.Login:
                Background();GUI.Label(new Rect(795,285,330,53),"[ COMMANDER ]",new GUIStyle(heading){fontSize=38});GUI.Label(new Rect(770,334,380,90),"LOG-IN",new GUIStyle(heading){fontSize=80});
                Image("Login_TextBox",new Rect(764,457,394,88));Image("Login_TextBox",new Rect(764,572,394,88));
                foreach(float bottom in new float[]{540,655}){Color corner=new Color(.17f,.43f,.46f);Fill(new Rect(771,bottom-9,2,10),corner);Fill(new Rect(771,bottom,10,2),corner);Fill(new Rect(1147,bottom-9,2,10),corner);Fill(new Rect(1139,bottom,10,2),corner);}
                GUI.SetNextControlName("CommanderName");commander=GUI.TextField(new Rect(766,459,390,84),commander,24,field);
                GUI.SetNextControlName("CommanderPassword");password=GUI.PasswordField(new Rect(766,574,390,84),password,'•',64,field);if(password.Length==0)GUI.Label(new Rect(778,577,366,80),"Password",new GUIStyle(field){normal={textColor=new Color(.4f,.43f,.45f)}});
                if(Button(new Rect(814,770,292,75),"LOG IN"))requestLogin=true;
                if(GUI.Button(new Rect(785,849,170,32),"Create an Account",small))localMessage="Choose a commander name, then LOG IN to create a local profile.";
                if(GUI.Button(new Rect(970,849,180,32),"Forgot Password?",small))localMessage="Local profiles do not require a password.";
                GUI.Label(new Rect(620,895,680,36),string.IsNullOrEmpty(Flow.Message)?localMessage:Flow.Message,new GUIStyle(small){wordWrap=true});GUI.Label(new Rect(720,943,480,26),"LOCAL PROFILE  •  OFFLINE",small);
                if(Button(new Rect(1860,0,60,60),"","Hangar_Button_Close"))Flow.Back();break;
            case TuStartupState.Screen.FactionSelect:
                Background();
                GUI.Label(new Rect(240,804,490,80),"AUTOBOTS",new GUIStyle(heading){fontSize=61});GUI.Label(new Rect(240,878,490,30),"'TIL ALL ARE ONE",new GUIStyle(label){fontSize=22});if(Button(new Rect(310,942,350,56),"ENTER HANGAR"))ChooseAutobots();
                if(GUI.Button(new Rect(70,315,870,600),GUIContent.none,GUIStyle.none))ChooseAutobots();
                GUI.Label(new Rect(1110,804,680,80),"DECEPTICONS",new GUIStyle(heading){fontSize=61});GUI.Label(new Rect(1110,878,680,30),"PEACE THROUGH TYRANNY",new GUIStyle(label){fontSize=22,normal={textColor=new Color(.1f,.72f,.86f)}});
                if(Button(new Rect(1270,942,350,56),"ENTER HANGAR"))ChooseDecepticons();
                if(GUI.Button(new Rect(980,315,870,600),GUIContent.none,GUIStyle.none))ChooseDecepticons();
                if(Button(new Rect(1860,0,60,60),"","Hangar_Button_Close"))Flow.Back();break;
            case TuStartupState.Screen.HangarLoading:
                Image("TitleLogo",new Rect(580,677,760,155));GUI.Label(new Rect(650,644,620,30),"K I L L E R   T E A M   C O M B A T",small);Spinner(new Rect(1704,866,194,194),true);GUI.Label(new Rect(1490,1040,400,30),loadingText,new GUIStyle(small){fontSize=16});break;
            case TuStartupState.Screen.Error:
                GUI.Label(new Rect(470,390,980,120),Flow.Message,new GUIStyle(label){wordWrap=true});if(Button(new Rect(810,580,300,65),"RESTART"))UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);break;
        }
        GUI.matrix=old;
    }
    void OnDestroy(){if(centerDisc!=null)Destroy(centerDisc);if(backgroundCamera!=null)Destroy(backgroundCamera.gameObject);}
}
