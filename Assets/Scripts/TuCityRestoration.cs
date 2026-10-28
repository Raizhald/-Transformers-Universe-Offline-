using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Adapts archived material inputs and scene environment to Unity 6.
public sealed class TuCityRestoration : MonoBehaviour
{
    [Serializable] sealed class Map {public string property="",file="";public Vector2 scale=Vector2.one,offset=Vector2.zero;}
    [Serializable] sealed class Surface {public string name="",shader="";public Color color=Color.white;public float cutoff=.5f,shininess=.05f;public Vector2 scale=Vector2.one,offset=Vector2.zero;public Map[] maps=new Map[0];}
    [Serializable] sealed class EnvironmentData {public int m_Fog=1,m_FogMode=3;public Color m_FogColor=Color.gray,m_AmbientLight=Color.gray;public float m_FogDensity=.001f,m_LinearFogStart=50,m_LinearFogEnd=500;}
    [Serializable] sealed class Sky {public string file="";public int size=0;public float rotation=180;}
    [Serializable] sealed class Sun {public Color color=Color.white;public float intensity=1;public Vector3 forward=Vector3.down,up=Vector3.up;}
    [Serializable] sealed class Sources {public Surface[] materials=new Surface[0];public EnvironmentData environment=new EnvironmentData();public Sky sky=new Sky();public Sun sun=new Sun();}
    Sources source;readonly Dictionary<string,Surface> surfaces=new Dictionary<string,Surface>();readonly Dictionary<string,Texture2D> textures=new Dictionary<string,Texture2D>();readonly HashSet<Material> restored=new HashSet<Material>();
    readonly HashSet<Mesh> tangentMeshes=new HashSet<Mesh>();
    AudioSource wind;
    Texture2D skyPanorama;Material skyMaterial;string folder;
    public int RestoredMaterials {get;private set;}
    public int MultiplyMaterials {get;private set;}
    public int AdditiveMaterials {get;private set;}
    public void Initialize()
    {
        folder=Path.Combine(Application.streamingAssetsPath,"CityRestoration");source=JsonUtility.FromJson<Sources>(File.ReadAllText(Path.Combine(folder,"sources.json")));foreach(var s in source.materials)surfaces[s.name]=s;
        skyPanorama=new Texture2D(2,2,TextureFormat.RGBA32,true);if(!skyPanorama.LoadImage(File.ReadAllBytes(Path.Combine(folder,"OriginalSkyPanorama.png"))))throw new InvalidDataException("Original city panorama is missing");skyPanorama.wrapModeU=TextureWrapMode.Repeat;skyPanorama.wrapModeV=TextureWrapMode.Clamp;
        skyMaterial=new Material(Resources.Load<Shader>("TuCitySky"));skyMaterial.SetTexture("_MainTex",skyPanorama);skyMaterial.SetFloat("_Rotation",source.sky.rotation);var clip=Resources.Load<AudioClip>("CityAudio/HighGrand_Wind_02");if(clip!=null){wind=gameObject.AddComponent<AudioSource>();wind.playOnAwake=false;wind.clip=clip;wind.loop=true;wind.spatialBlend=0;wind.volume=.4786301f;}
    }
    Texture2D Texture(string file)
    {
        Texture2D texture;if(textures.TryGetValue(file,out texture))return texture;texture=new Texture2D(2,2,TextureFormat.RGBA32,true,true);if(!texture.LoadImage(File.ReadAllBytes(Path.Combine(folder,file))))throw new InvalidDataException("Invalid original surface texture "+file);texture.wrapMode=TextureWrapMode.Repeat;texture.anisoLevel=8;textures.Add(file,texture);return texture;
    }
    public void Apply(TuPackedModel model)
    {
        TuCityVertexColors.EnsureWhiteDefaults(model);
        foreach(var node in model.nodes){var renderer=node.GetComponent<Renderer>();if(renderer==null)continue;foreach(var material in renderer.sharedMaterials)
        {
            if(!restored.Add(material)||material.name=="VFX_Gameplay_EnergonLift_Tunnel01_CS")continue;Surface s;if(!surfaces.TryGetValue(material.name,out s))continue;
            string shader=s.shader.ToLowerInvariant();bool particle=shader.Contains("particle"),add=particle&&shader.Contains("add"),multiply=shader.Contains("multiply"),cutout=shader.Contains("alphatest"),alpha=!cutout&&(shader.Contains("alpha")||shader.Contains("transparent"));
            var diffuse=material.mainTexture;material.shader=Resources.Load<Shader>(multiply?"TuCityMultiply":add?"TuCityAdditive":particle||shader.Contains("unlit")?"TuExteriorEffect":alpha?"TuCityTransparent":"TuCitySurface");material.mainTexture=diffuse;material.color=multiply?Color.white:s.color;material.mainTextureScale=s.scale;material.mainTextureOffset=s.offset;
            if(multiply){material.renderQueue=2990;MultiplyMaterials++;}
            else if(add){material.renderQueue=3000;AdditiveMaterials++;}
            else if(particle||shader.Contains("unlit"))
            {
                material.SetInt("_SrcBlend",(int)(multiply?BlendMode.DstColor:BlendMode.SrcAlpha));material.SetInt("_DstBlend",(int)(multiply?BlendMode.Zero:add?BlendMode.One:BlendMode.OneMinusSrcAlpha));material.SetInt("_ZWrite",0);material.renderQueue=multiply?2990:3000;
            }
            else
            {
                material.SetFloat("_Cutoff",s.cutoff);material.SetFloat("_UseCutout",cutout?1:0);material.SetFloat("_Glossiness",Mathf.Clamp01(Mathf.Sqrt(Mathf.Max(0,s.shininess))));material.SetFloat("_Metallic",0);material.SetFloat("_UseNormal",0);material.SetFloat("_UseIllum",0);material.renderQueue=alpha?3000:cutout?2450:2000;material.enableInstancing=!alpha;
                foreach(var map in s.maps)
                {
                    string property=map.property=="_BumpMap"?"_BumpMap":"_Illum";material.SetTexture(property,Texture(map.file));material.SetTextureScale(property,map.scale);material.SetTextureOffset(property,map.offset);material.SetFloat(property=="_BumpMap"?"_UseNormal":"_UseIllum",1);
                }
            }
            RestoredMaterials++;
        }
        bool physical=false,normal=false;foreach(var material in renderer.sharedMaterials){if(material.shader.name=="TU/CitySurface")physical=true;if(material.HasProperty("_UseNormal")&&material.GetFloat("_UseNormal")>.5f)normal=true;}
        renderer.shadowCastingMode=physical?ShadowCastingMode.On:ShadowCastingMode.Off;renderer.receiveShadows=physical;
        var filter=node.GetComponent<MeshFilter>();if(normal&&filter!=null&&tangentMeshes.Add(filter.sharedMesh))filter.sharedMesh.RecalculateTangents();
        }
    }
    public void CreateSun(Transform parent)
    {
        var go=new GameObject("Original exterior sun");go.transform.SetParent(parent,false);go.transform.rotation=Quaternion.LookRotation(source.sun.forward,source.sun.up);var light=go.AddComponent<Light>();light.type=LightType.Directional;light.color=source.sun.color;light.intensity=source.sun.intensity;light.shadows=LightShadows.Soft;light.shadowStrength=.7f;light.shadowBias=.05f;
    }
    public void Enter(Camera camera)
    {
        if(wind!=null&&!wind.isPlaying)wind.Play();var e=source.environment;RenderSettings.fog=e.m_Fog!=0;RenderSettings.fogMode=(FogMode)e.m_FogMode;RenderSettings.fogColor=e.m_FogColor;RenderSettings.fogDensity=e.m_FogDensity;RenderSettings.fogStartDistance=e.m_LinearFogStart;RenderSettings.fogEndDistance=e.m_LinearFogEnd;RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=e.m_AmbientLight;RenderSettings.skybox=skyMaterial;if(camera!=null){camera.clearFlags=CameraClearFlags.Skybox;camera.backgroundColor=e.m_FogColor;camera.farClipPlane=1800;}
    }
    void OnDestroy(){foreach(var texture in textures.Values)if(texture!=null)Destroy(texture);if(skyMaterial!=null)Destroy(skyMaterial);if(skyPanorama!=null)Destroy(skyPanorama);}
}

