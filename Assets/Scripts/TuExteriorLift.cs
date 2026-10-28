using System;
using System.IO;
using UnityEngine;

public sealed class TuExteriorLift : MonoBehaviour
{
    [Serializable] sealed class TextureRecord {public string property="",file="";public Vector2 scale=Vector2.one,offset=Vector2.zero;}
    [Serializable] sealed class Values {public float _DistortionPower=0,_FresnelPower=1;public Color _EmissiveColor=Color.black,_FresnelColor=Color.black;}
    [Serializable] sealed class Source {public string name="";public TextureRecord[] textures=new TextureRecord[0];public Values values=new Values();}
    Material material;Texture2D[] textures;
    public void Restore(Material target)
    {
        string folder=Path.Combine(Application.streamingAssetsPath,"ExteriorLift");var source=JsonUtility.FromJson<Source>(File.ReadAllText(Path.Combine(folder,"material.json")));material=target;
        var shader=Resources.Load<Shader>("TuExteriorLift");if(shader==null)throw new InvalidDataException("Exterior lift shader is missing");material.shader=shader;material.renderQueue=3010;
        material.SetFloat("_DistortionPower",source.values._DistortionPower);material.SetFloat("_FresnelPower",source.values._FresnelPower);material.SetColor("_EmissiveColor",source.values._EmissiveColor);material.SetColor("_FresnelColor",source.values._FresnelColor);
        textures=new Texture2D[source.textures.Length];for(int k=0;k<textures.Length;k++){var entry=source.textures[k];var image=new Texture2D(2,2,TextureFormat.RGBA32,true);image.LoadImage(File.ReadAllBytes(Path.Combine(folder,entry.file)));image.wrapMode=TextureWrapMode.Repeat;image.anisoLevel=4;textures[k]=image;material.SetTexture(entry.property,image);material.SetTextureScale(entry.property,entry.scale);material.SetTextureOffset(entry.property,entry.offset);}
    }
    void OnDestroy(){if(textures!=null)foreach(var texture in textures)if(texture!=null)Destroy(texture);}
}
