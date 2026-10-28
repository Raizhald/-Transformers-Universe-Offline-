using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

// Static archive data only: original Autobot scene, materials, lights and hologram.
public sealed class TuAutobotHangar : MonoBehaviour
{
    [Serializable] public sealed class Surface
    {
        public string name="",shader="",diffuseFile="",detailFile="",illumFile="",normalFile="",screenFile="",liftFile="";
        public bool useTone=false;public Color toneA=Color.white,toneB=Color.white,screenTint=Color.white,fresnel=Color.white;
        public float fresnelPower=2,distortionPower=.025f;
        public Vector2 normalFileScale=Vector2.one,normalFileOffset=Vector2.zero,liftFileScale=Vector2.one,liftFileOffset=Vector2.zero;
        public Color color=Color.white,tint=Color.white,emission=Color.black;
        public Vector2 scale=Vector2.one,offset=Vector2.zero,detailScale=Vector2.one,detailOffset=Vector2.zero;
        public float shininess=.2f,reflectivity=0,cutoff=.5f;
    }
    [Serializable] public sealed class SourceLight
    { public string name="";public Vector3 position=Vector3.zero;public Color color=Color.white;public float intensity=1,range=10; }
    [Serializable] public sealed class Data
    {
        public Surface[] materials=new Surface[0];public SourceLight[] lights=new SourceLight[0];
        public Vector4 mapBounds=Vector4.zero;public Vector3 spawn=Vector3.zero,portal=Vector3.zero;
    }
    public Data Source {get;private set;}
    public int RestoredSurfaces {get;private set;}
    public int OriginalLights {get;private set;}
    public int LowerMeshes {get;private set;}
    public int DisabledLowerFades {get;private set;}
    public bool EffectsVisible {get;private set;}=true;
    readonly Dictionary<string,Texture2D> textures=new Dictionary<string,Texture2D>();
    readonly List<Renderer> luminous=new List<Renderer>();
    GameObject effectsRoot;
    public void Setup(TuPackedModel hangar)
    {
        Source=JsonUtility.FromJson<Data>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"autobot-hangar.json")));
        RestoreEnvironment(null);
        TuCityVertexColors.Restore(hangar,"autobot-hangar-colors.bin");Apply(hangar);
        // The source uses a black void below the roads. Finish that area using the
        // recovered plane mesh and the hangar's original concrete texture (see provenance JSON).
        TuPackedModel lower=TuPackedModel.Load(Path.Combine(Application.streamingAssetsPath,"autobot-lower-finished.tupack"),"Finished Autobot lower concrete floor and source tunnel grille");
        lower.root.transform.SetParent(transform,false);Apply(lower);LowerMeshes=lower.GeometryMeshes.Length;
        foreach(Renderer renderer in hangar.root.GetComponentsInChildren<Renderer>())
        {
            if(!renderer.name.Contains("UnderFloor")&&!renderer.name.Contains("ConcreteSupport"))continue;
            if(renderer.sharedMaterials.Length==0||!Array.TrueForAll(renderer.sharedMaterials,m=>m.name.Contains("BlackGradient")))continue;
            // These source overlays intentionally fade the walls to a black void;
            // they must not obscure the visible concrete floor in the finished version.
            renderer.enabled=false;DisabledLowerFades++;
        }
        foreach(MeshFilter filter in lower.root.GetComponentsInChildren<MeshFilter>())
        {
            filter.gameObject.layer=8;
            MeshCollider collider=filter.gameObject.AddComponent<MeshCollider>();collider.sharedMesh=filter.sharedMesh;
        }
        effectsRoot=new GameObject("Original Autobot lights and hologram");effectsRoot.transform.SetParent(transform);
        foreach(SourceLight item in Source.lights)
        {
            GameObject go=new GameObject(item.name);go.transform.SetParent(effectsRoot.transform);go.transform.position=item.position;
            Light light=go.AddComponent<Light>();light.type=LightType.Point;light.color=item.color;light.intensity=item.intensity;light.range=item.range;OriginalLights++;
        }
        TuPackedModel globe=TuPackedModel.Load(Path.Combine(Application.streamingAssetsPath,"autobot-globe.tupack"),"Original Autobot hologram");
        globe.root.transform.SetParent(effectsRoot.transform);Apply(globe);
        TuArchivePortal.Create(effectsRoot.transform,"autobot-portal.json");
        gameObject.AddComponent<TuAutobotSigns>();
        // Original lightmaps/custom lighting scripts are unavailable; a neutral fill lights the restored geometry.
        GameObject fill=new GameObject("Autobot reconstruction fill");fill.transform.SetParent(transform);fill.transform.rotation=Quaternion.Euler(55,-35,0);
        Light sun=fill.AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(.78f,.85f,1);sun.intensity=.55f;
    }
    public void RestoreEnvironment(Camera camera)
    {
        RenderSettings.skybox=null;RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight=new Color(.2255082f,.263897f,.3161765f);
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=50;RenderSettings.fogEndDistance=500;
        RenderSettings.fogColor=new Color(.3019608f,.3372549f,.3686275f);
        if(camera!=null){camera.clearFlags=CameraClearFlags.SolidColor;camera.farClipPlane=1000;camera.backgroundColor=new Color(.035f,.028f,.06f);}
    }
    Texture2D Texture(string file,bool linear=false)
    {
        if(string.IsNullOrEmpty(file))return null;Texture2D t;
        if(!textures.TryGetValue(file,out t)||t==null){t=new Texture2D(2,2,TextureFormat.RGBA32,true,linear);t.LoadImage(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,file)));t.wrapMode=TextureWrapMode.Repeat;t.anisoLevel=8;textures[file]=t;}return t;
    }
    public void Apply(TuPackedModel model,Surface[] surfaces=null)
    {
        foreach(Mesh mesh in model.GeometryMeshes)if(mesh.tangents.Length!=mesh.vertexCount)mesh.RecalculateTangents();
        TuCityVertexColors.EnsureWhiteDefaults(model);var visited=new HashSet<Material>();
        foreach(Renderer r in model.root.GetComponentsInChildren<Renderer>())foreach(Material m in r.sharedMaterials)
        {
            Surface e=Array.Find(surfaces??Source.materials,x=>x.name==m.name);if(e==null)continue;if(surfaces==null&&(e.shader=="Particle Add"||e.name.Contains("Hologram_Screen"))&&!luminous.Contains(r))luminous.Add(r);if(!visited.Add(m))continue;
            string resource="TuAutobotSurface";
            if(e.shader.Contains("Multiply"))resource="TuCityMultiply";
            else if(e.shader=="Particle Add")resource="TuCityAdditive";
            else if(e.shader.Contains("AlphaTest"))resource="TuAutobotCutout";
            else if(e.shader=="TransparentBackground")resource="TuArchiveBackdrop";
            else if(e.shader.Contains("FresnelRefraction"))resource="TuExteriorLift";
            else if(e.shader.StartsWith("Alpha-"))resource="TuAutobotAlphaLit";
            else if(e.shader.Contains("Alpha"))resource="TuAutobotAlpha";
            if(e.name.Contains("Hologram_Screen"))resource="TuCityAdditive";
            if(e.name.Contains("GlobemapAutobot"))resource="TuHologram";
            Shader shader=Resources.Load<Shader>(resource);if(shader==null)throw new InvalidDataException("Missing Autobot shader: "+resource);
            m.shader=shader;if(m.HasProperty("_Color"))m.color=e.shader=="Particle Add"?e.tint:e.color;
            Texture2D diffuse=Texture(e.diffuseFile);if(diffuse!=null)m.mainTexture=diffuse;
            m.mainTextureScale=e.scale;m.mainTextureOffset=e.offset;
            if(!string.IsNullOrEmpty(e.screenFile)){m.mainTexture=Texture(e.screenFile);m.color=e.screenTint*.5f;}
            if(resource=="TuExteriorLift"){m.SetTexture("_Emissive",Texture(e.liftFile));m.SetTextureScale("_Emissive",e.liftFileScale);m.SetTextureOffset("_Emissive",e.liftFileOffset);m.SetTexture("_Normal",Texture(e.normalFile,true));m.SetColor("_EmissiveColor",e.emission);m.SetColor("_FresnelColor",e.fresnel);m.SetFloat("_FresnelPower",e.fresnelPower);m.SetFloat("_DistortionPower",e.distortionPower);}
            if(resource=="TuHologram"){m.SetColor("_Tint",e.tint);m.SetColor("_OceanTint",e.tint);m.SetFloat("_HasTexture",m.mainTexture!=null?1:0);m.SetFloat("_Layer",e.name.Contains("Ocean")?1:e.name.Contains("Earth")?2:3);}
            if(m.HasProperty("_Cutoff"))m.SetFloat("_Cutoff",e.cutoff);
            if(m.HasProperty("_EmissionColor"))m.SetColor("_EmissionColor",Color.black);
            if(resource=="TuAutobotSurface"||resource=="TuAutobotCutout"||resource=="TuAutobotAlphaLit")
            {
                m.SetFloat("_UseTone",e.useTone?1:0);m.SetColor("_ToneA",e.toneA);m.SetColor("_ToneB",e.toneB);
                Texture2D normal=Texture(e.normalFile,true);m.SetFloat("_UseBump",normal!=null?1:0);if(normal!=null){m.SetTexture("_BumpMap",normal);m.SetTextureScale("_BumpMap",e.normalFileScale);m.SetTextureOffset("_BumpMap",e.normalFileOffset);}
                m.SetFloat("_Glossiness",Mathf.Clamp01(.2f+.6f*Mathf.Sqrt(Mathf.Max(0,e.shininess))));
                m.SetFloat("_Metallic",Mathf.Clamp(e.reflectivity*.35f,0,.35f));
                Texture2D detail=Texture(e.detailFile,true),illum=Texture(e.illumFile);
                m.SetFloat("_UseDetail",detail!=null?1:0);m.SetFloat("_GlossFromAlpha",e.shader.Contains("Gloss")&&detail==null?1:0);
                if(detail!=null){m.SetTexture("_DetailTex",detail);m.SetTextureScale("_DetailTex",e.detailScale);m.SetTextureOffset("_DetailTex",e.detailOffset);m.SetColor("_EmissionColor",e.emission);}
                else if(illum!=null){m.SetTexture("_EmissionMap",illum);m.SetColor("_EmissionColor",e.emission.maxColorComponent>0?e.emission:Color.white);}
            }
            if(surfaces==null&&resource=="TuCityAdditive"&&!luminous.Contains(r))luminous.Add(r);
            RestoredSurfaces++;
        }
    }
    public void ToggleEffects(){EffectsVisible=!EffectsVisible;effectsRoot.SetActive(EffectsVisible);foreach(Renderer r in luminous)if(r!=null)r.enabled=EffectsVisible;}
    void OnDestroy(){foreach(Texture2D t in textures.Values)Destroy(t);}
}
