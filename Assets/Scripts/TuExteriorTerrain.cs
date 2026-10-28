using System;
using System.IO;
using System.Collections;
using UnityEngine;

// Recovered Unity 4 TerrainData, converted to native Unity Terrain and TerrainCollider.
public sealed class TuExteriorTerrain : MonoBehaviour
{
    [Serializable] sealed class Layer {public string file="";public Vector2 tileSize=Vector2.one,tileOffset=Vector2.zero;}
    [Serializable] sealed class Source {public int heightResolution=0,alphaResolution=0;public Vector3 origin=Vector3.zero,size=Vector3.one;public float minimumY=0,maximumY=0;public Layer[] layers=new Layer[0];public string heightFile="",weightsFile="";}
    public Terrain Terrain {get;private set;}
    public int OriginalLayerCount {get;private set;}
    public float MinimumY {get;private set;}
    TerrainData data;TerrainLayer[] layers;Texture2D[] textures;
    public IEnumerator Load(Action<float,string> progress,string directory="ExteriorTerrain")
    {
        string folder=Path.Combine(Application.streamingAssetsPath,directory);var source=JsonUtility.FromJson<Source>(File.ReadAllText(Path.Combine(folder,"terrain.json")));
        if(source.heightResolution!=1025||source.alphaResolution!=1024||source.layers.Length!=8)throw new InvalidDataException("Unexpected original exterior terrain dimensions");
        MinimumY=source.minimumY;OriginalLayerCount=source.layers.Length;int n=source.heightResolution;
        var heights=new float[n,n];using(var b=new BinaryReader(File.OpenRead(Path.Combine(folder,source.heightFile))))
        {
            for(int z=0;z<n;z++){for(int x=0;x<n;x++){float value=b.ReadSingle()/source.size.y;if(float.IsNaN(value)||value<0||value>1)throw new InvalidDataException("Invalid original terrain height");heights[z,x]=value;}if(z%128==0){progress(.82f+.02f*z/n,"Restoring original terrain heights...");yield return null;}}
            if(b.BaseStream.Position!=b.BaseStream.Length)throw new InvalidDataException("Unexpected terrain height bytes");
        }
        data=new TerrainData{name="Original exterior heightfield",heightmapResolution=n,alphamapResolution=source.alphaResolution,baseMapResolution=1024};data.size=source.size;data.SetHeights(0,0,heights);
        layers=new TerrainLayer[source.layers.Length];textures=new Texture2D[layers.Length];
        for(int k=0;k<layers.Length;k++)
        {
            var image=new Texture2D(2,2,TextureFormat.RGBA32,true);if(!image.LoadImage(File.ReadAllBytes(Path.Combine(folder,source.layers[k].file))))throw new InvalidDataException("Unable to load original terrain layer");image.wrapMode=TextureWrapMode.Repeat;image.anisoLevel=8;textures[k]=image;
            layers[k]=new TerrainLayer{name="Original exterior surface "+k,diffuseTexture=image,tileSize=source.layers[k].tileSize,tileOffset=source.layers[k].tileOffset,metallic=0,smoothness=0};progress(.84f+.025f*k/layers.Length,"Loading original ground textures...");yield return null;
        }
        data.terrainLayers=layers;int a=source.alphaResolution;var weights=new float[a,a,layers.Length];using(var b=new BinaryReader(File.OpenRead(Path.Combine(folder,source.weightsFile))))
        {
            for(int z=0;z<a;z++){for(int x=0;x<a;x++){float sum=0;for(int k=0;k<layers.Length;k++){float value=b.ReadSingle();if(float.IsNaN(value)||value<0||value>1.001f)throw new InvalidDataException("Invalid terrain surface weight");weights[z,x,k]=value;sum+=value;}if(Mathf.Abs(sum-1)>.002f)throw new InvalidDataException("Unnormalized terrain surface weights");}if(z%128==0){progress(.865f+.02f*z/a,"Restoring original ground blending...");yield return null;}}
            if(b.BaseStream.Position!=b.BaseStream.Length)throw new InvalidDataException("Unexpected terrain blend bytes");
        }
        data.SetAlphamaps(0,0,weights);var go=UnityEngine.Terrain.CreateTerrainGameObject(data);go.name="Original exterior terrain and collision";go.layer=8;go.transform.SetParent(transform,false);go.transform.position=source.origin;Terrain=go.GetComponent<Terrain>();Terrain.heightmapPixelError=3;Terrain.basemapDistance=1500;Terrain.drawInstanced=true;Terrain.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;Terrain.Flush();
        var collider=go.GetComponent<TerrainCollider>();if(collider==null||collider.terrainData!=data)throw new InvalidDataException("Native terrain collider was not created");Physics.SyncTransforms();
    }
    public bool Contains(Vector3 position){return Terrain!=null&&position.x>=Terrain.transform.position.x&&position.x<=Terrain.transform.position.x+data.size.x&&position.z>=Terrain.transform.position.z&&position.z<=Terrain.transform.position.z+data.size.z;}
    public float GroundHeight(Vector3 position){if(!Contains(position))return float.NaN;return Terrain.SampleHeight(position)+Terrain.transform.position.y;}
    void OnDestroy(){if(data!=null)Destroy(data);if(layers!=null)foreach(var layer in layers)if(layer!=null)Destroy(layer);if(textures!=null)foreach(var texture in textures)if(texture!=null)Destroy(texture);}
}
