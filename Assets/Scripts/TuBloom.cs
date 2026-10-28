using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class TuBloom : MonoBehaviour
{
    public TuHangarEffects effects;
    public TuAutobotHangar autobotHangar;
    Material material;
    void OnRenderImage(RenderTexture source,RenderTexture destination)
    {
        if(!(effects!=null&&effects.EffectsVisible)&&!(autobotHangar!=null&&autobotHangar.EffectsVisible)){Graphics.Blit(source,destination);return;}
        if(material==null){Shader shader=Resources.Load<Shader>("TuBloom");if(shader==null||!shader.isSupported){Graphics.Blit(source,destination);return;}material=new Material(shader);material.hideFlags=HideFlags.HideAndDontSave;}
        RenderTextureFormat format=SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf)?RenderTextureFormat.ARGBHalf:RenderTextureFormat.ARGB32;
        RenderTexture a=RenderTexture.GetTemporary(Mathf.Max(1,source.width/4),Mathf.Max(1,source.height/4),0,format),b=RenderTexture.GetTemporary(Mathf.Max(1,source.width/4),Mathf.Max(1,source.height/4),0,format);
        try{Graphics.Blit(source,a,material,0);Graphics.Blit(a,b,material,1);Graphics.Blit(b,a,material,1);material.SetTexture("_BloomTex",a);Graphics.Blit(source,destination,material,2);}
        finally{RenderTexture.ReleaseTemporary(a);RenderTexture.ReleaseTemporary(b);}
    }
    void OnDestroy(){if(material!=null)Destroy(material);}
}
