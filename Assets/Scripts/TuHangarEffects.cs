using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public sealed class TuHangarEffects : MonoBehaviour
{
    [Serializable] public class SourceLight { public string name; public Vector3 position; public Color color; public float intensity,range; }
    [Serializable] public class SourceEmitter { public string name; public Vector3 position; public bool mist; }
    [Serializable] public class Emission { public string name,file; public Color color; }
    [Serializable] public class GlobeMaterial { public string name; public Vector2 scale,offset; }
    [Serializable] sealed class AdditiveSource {public string name="",file="";public Color tint=Color.white;public Vector2 scale=Vector2.one,offset=Vector2.zero;}
    [Serializable] sealed class AdditiveCatalog {public AdditiveSource[] materials=new AdditiveSource[0];}
    [Serializable] public class SourceData { public Vector3 portalPosition,portalRotation; public SourceLight[] lights; public SourceEmitter[] emitters; public Emission[] emissions; public GlobeMaterial[] globeMaterials; }
    sealed class SurfaceChange { public Renderer renderer; public Material[] before,after; }
    readonly List<SurfaceChange> surfaces=new List<SurfaceChange>();
    readonly List<ParticleSystem> particles=new List<ParticleSystem>();
    readonly Dictionary<string,Texture2D> textures=new Dictionary<string,Texture2D>();
    GameObject visuals;Light portalLight;SourceData data;
    public bool EffectsVisible { get; private set; }
    public int OriginalLights { get; private set; }
    public int GlobeMeshes { get; private set; }
    public int EmissiveSurfaces { get; private set; }
    public int ParticleSystems { get { return particles.Count; } }
    public float PortalEnergy { get { return portalLight==null?0:portalLight.intensity; } }
    public void Setup(TuPackedModel hangar)
    {
        data=JsonUtility.FromJson<SourceData>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"effects.json")));
        visuals=new GameObject("Portal, hologram and particles");visuals.transform.parent=transform;
        foreach(SourceLight item in data.lights){PointLight(item.name,item.position,item.color,item.range,Mathf.Min(item.intensity*.4f,3));OriginalLights++;}
        RestoreSurfaces(hangar);
        TuPackedModel globe=TuPackedModel.Load(Path.Combine(Application.streamingAssetsPath,"globe.tupack"),"Original hologram geometry");globe.root.transform.parent=visuals.transform;
        foreach(Renderer r in globe.root.GetComponentsInChildren<Renderer>())
        {
            Material[] materials=r.sharedMaterials;
            for(int k=0;k<materials.Length;k++)
            {
                Material old=materials[k];Material m=new Material(FxShader("TuHologram"));m.name=old.name;m.mainTexture=old.mainTexture;
                foreach(GlobeMaterial item in data.globeMaterials)if(item.name==old.name){m.mainTextureScale=item.scale;m.mainTextureOffset=item.offset;break;}
                m.SetFloat("_HasTexture",old.mainTexture!=null?1:0);m.SetColor("_Tint",new Color(1,.47f,.025f));
                m.SetFloat("_Layer",old.name.Contains("Ocean")?1:old.name.Contains("Earth")?2:old.name.Contains("Rays")?3:0);materials[k]=m;
            }
            r.sharedMaterials=materials;GlobeMeshes++;
        }
        GameObject portal=new GameObject("GroundBridge portal");portal.transform.parent=visuals.transform;portal.transform.position=data.portalPosition;portal.transform.rotation=Quaternion.Euler(data.portalRotation);
        TuArchivePortal.Create(portal.transform);
        portalLight=PointLight("Pulsing portal light",data.portalPosition,new Color(.65f,.08f,1),24,2.1f);
        CreateParticles(data.portalPosition,false,true,"GroundBridge sparks",portal.transform.rotation);
        foreach(SourceEmitter item in data.emitters)CreateParticles(item.position,item.mist,false,item.name,Quaternion.identity);
        SetEffects(true);
        Debug.Log("TU_EFFECTS_READY globeMeshes="+GlobeMeshes+" lights="+OriginalLights+" particles="+ParticleSystems+" emissions="+EmissiveSurfaces);
    }
    static Shader FxShader(string name){Shader shader=Resources.Load<Shader>(name);if(shader==null)throw new InvalidDataException("Missing effect shader "+name);return shader;}
    Texture2D Texture(string file)
    {
        Texture2D t;if(textures.TryGetValue(file,out t))return t;t=new Texture2D(2,2,TextureFormat.RGBA32,true);t.LoadImage(File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,file)));t.wrapMode=TextureWrapMode.Repeat;textures[file]=t;return t;
    }
    void RestoreSurfaces(TuPackedModel hangar)
    {
        Dictionary<Material,Material> replacements=new Dictionary<Material,Material>();
        var disabledGlows=new Dictionary<Material,Material>();
        var additive=JsonUtility.FromJson<AdditiveCatalog>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"additive-materials.json")));
        var source=new Dictionary<string,AdditiveSource>();foreach(var item in additive.materials)source[item.name]=item;
        foreach(Renderer renderer in hangar.root.GetComponentsInChildren<Renderer>())
        {
            Material[] before=renderer.sharedMaterials,after=(Material[])before.Clone();bool changed=false;
            for(int k=0;k<before.Length;k++)
            {
                Material old=before[k],replacement;
                if(replacements.TryGetValue(old,out replacement)){after[k]=replacement;Material quiet;if(disabledGlows.TryGetValue(old,out quiet))before[k]=quiet;changed=true;EmissiveSurfaces++;continue;}
                AdditiveSource glow;if(source.TryGetValue(old.name,out glow))
                {
                    replacement=new Material(FxShader("TuArchiveAdditive"));replacement.name=old.name;replacement.mainTexture=Texture(glow.file);replacement.mainTextureScale=glow.scale;replacement.mainTextureOffset=glow.offset;replacement.SetColor("_TintColor",glow.tint);var quiet=new Material(replacement);quiet.SetColor("_TintColor",Color.clear);disabledGlows[old]=quiet;before[k]=quiet;replacements[old]=replacement;after[k]=replacement;changed=true;continue;
                }
                if(old.name.Contains("VFX")&&(old.name.Contains("Shield")||old.name.Contains("EnergonLift")))
                {
                    replacement=new Material(FxShader("TuBarrier"));replacement.name=old.name;replacement.SetColor("_Tint",old.name.Contains("Shield")?new Color(1,.035f,.08f):new Color(.05f,.65f,1));replacements[old]=replacement;after[k]=replacement;changed=true;continue;
                }
                foreach(Emission item in data.emissions)if(item.name==old.name)
                {
                    replacement=new Material(old);replacement.name=old.name;Color c=item.color;
                    if(old.name.Contains("_PURPEM"))c=new Color(.55f,.07f,1);if(old.name.Contains("_REDEM"))c=new Color(1,.025f,.06f);
                    replacement.SetTexture("_EmissionMap",Texture(item.file));replacement.SetColor("_EmissionColor",old.name.Contains("Crystal")?Color.white*1.8f:c*.8f);replacement.EnableKeyword("_EMISSION");replacements[old]=replacement;after[k]=replacement;changed=true;EmissiveSurfaces++;break;
                }
            }
            if(changed)surfaces.Add(new SurfaceChange{renderer=renderer,before=before,after=after});
        }
    }
    Light PointLight(string name,Vector3 pos,Color color,float range,float intensity)
    {
        GameObject go=new GameObject(name);go.transform.parent=visuals.transform;go.transform.position=pos;Light light=go.AddComponent<Light>();light.type=LightType.Point;light.color=color;light.range=range;light.intensity=intensity;light.shadows=LightShadows.None;return light;
    }
    void Quad(Transform parent,float size,bool halo)
    {
        GameObject go=GameObject.CreatePrimitive(PrimitiveType.Quad);go.name=halo?"Portal halo":"Portal energy";Destroy(go.GetComponent<Collider>());go.transform.parent=parent;go.transform.localPosition=new Vector3(0,0,halo?.04f:0);go.transform.localRotation=Quaternion.identity;go.transform.localScale=new Vector3(size,size,1);
        Material m=new Material(FxShader("TuPortal"));m.SetFloat("_Halo",halo?1:0);m.SetColor("_EnergyColor",new Color(.65f,.06f,1));go.GetComponent<Renderer>().sharedMaterial=m;
    }
    void CreateParticles(Vector3 position,bool mist,bool portal,string title,Quaternion rotation)
    {
        GameObject go=new GameObject(title);go.transform.parent=visuals.transform;go.transform.position=position;go.transform.rotation=rotation;ParticleSystem ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=ps.main;main.loop=true;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=mist?48:portal?160:64;main.startLifetime=mist?7:2.4f;main.startSpeed=new ParticleSystem.MinMaxCurve(.15f,mist?.6f:1.6f);main.startSize=new ParticleSystem.MinMaxCurve(mist?2:.035f,mist?5.5f:.13f);main.startColor=Color.white;
        var emission=ps.emission;emission.enabled=true;emission.rateOverTime=mist?5:portal?58:20;
        var shape=ps.shape;shape.enabled=true;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=mist?new Vector3(24,.5f,14):portal?new Vector3(9.4f,9.4f,.4f):new Vector3(4,.6f,4);
        if(portal){shape.shapeType=ParticleSystemShapeType.Circle;shape.radius=5.4f;shape.radiusThickness=.15f;shape.scale=Vector3.one;}
        // Local emission axis points up; portal particles retain the portal plane.
        var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=new ParticleSystem.MinMaxCurve(-.3f,.3f);velocity.y=new ParticleSystem.MinMaxCurve(.1f,.55f);velocity.z=new ParticleSystem.MinMaxCurve(-.3f,.3f);
        Gradient fade=new Gradient();Color color=mist?new Color(.5f,.025f,.04f,.16f):new Color(1,.4f,.08f,.9f);
        fade.SetKeys(new GradientColorKey[]{new GradientColorKey(color,0),new GradientColorKey(color,1)},new GradientAlphaKey[]{new GradientAlphaKey(0,0),new GradientAlphaKey(color.a,.18f),new GradientAlphaKey(color.a*.4f,.75f),new GradientAlphaKey(0,1)});
        var lifetime=ps.colorOverLifetime;lifetime.enabled=true;lifetime.color=fade;
        ParticleSystemRenderer renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Billboard;renderer.sharedMaterial=new Material(FxShader("TuParticles"));renderer.sharedMaterial.SetFloat("_DstBlend",mist?(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha:(float)UnityEngine.Rendering.BlendMode.One);renderer.maxParticleSize=mist?.5f:.15f;
        particles.Add(ps);ps.Simulate(3,true,true);ps.Play();
    }
    public void SetEffects(bool value)
    {
        EffectsVisible=value;if(visuals!=null)visuals.SetActive(value);
        foreach(SurfaceChange change in surfaces)change.renderer.sharedMaterials=value?change.after:change.before;
        foreach(ParticleSystem ps in particles){if(value)ps.Play();else ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
    }
    void Update(){if(EffectsVisible&&portalLight!=null)portalLight.intensity=2.1f+Mathf.Sin(Time.time*2.3f)*.3f;}
}
