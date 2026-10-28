using System;
using System.IO;
using UnityEngine;

public sealed class TuParkedVehicles : MonoBehaviour
{
    [Serializable] sealed class Box {public string name="";public Vector3 center=Vector3.zero,size=Vector3.one;public float yaw=0;}
    [Serializable] sealed class Catalog {public Box[] boxes=new Box[0];}
    public int ColliderCount {get;private set;}
    public void Restore(string file="parked-vehicle-collisions.json")
    {
        var data=JsonUtility.FromJson<Catalog>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,file)));
        foreach(var box in data.boxes){var go=new GameObject("Parked vehicle: "+box.name);go.layer=8;go.transform.SetParent(transform,false);go.transform.position=box.center;go.transform.rotation=Quaternion.Euler(0,box.yaw,0);go.AddComponent<BoxCollider>().size=box.size+Vector3.one*.03f;ColliderCount++;}
    }
}
public sealed class TuCityAmbience : MonoBehaviour
{
    [Serializable] sealed class Loop {public string name="",clip="";public Vector3 position=Vector3.zero;public float volume=.3f,range=80;}
    [Serializable] sealed class Catalog {public Loop[] loops=new Loop[0];}
    AudioSource[] sources;
    public void Restore()
    {
        var data=JsonUtility.FromJson<Catalog>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"city-ambient-loops.json")));sources=new AudioSource[data.loops.Length];
        for(int k=0;k<data.loops.Length;k++){var entry=data.loops[k];var clip=Resources.Load<AudioClip>("CityAudio/"+entry.clip);if(clip==null)continue;var go=new GameObject(entry.name);go.transform.SetParent(transform,false);go.transform.position=entry.position;var source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=true;source.clip=clip;source.volume=entry.volume;source.spatialBlend=1;source.minDistance=8;source.maxDistance=entry.range;source.rolloffMode=AudioRolloffMode.Linear;source.dopplerLevel=0;sources[k]=source;}
    }
    public void Resume(){if(sources!=null)foreach(var source in sources)if(source!=null&&!source.isPlaying)source.Play();}
}
