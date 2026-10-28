using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

// Reads only the data exported by this project. Original game assemblies are never loaded.
public sealed class TuPackedModel
{
    public GameObject root;
    public Transform[] nodes;
    public Mesh[] GeometryMeshes {get;private set;}
    public readonly List<Motion> motions = new List<Motion>();
    sealed class Geometry { public Mesh mesh; public int[] materials; }
    sealed class Node { public string name; public int parent, mesh, skin; public Vector3 position, scale; public Quaternion rotation; }
    sealed class Skin { public int[] joints; public Matrix4x4[] bindposes; }
    public sealed class Track { public int node, kind; public float[] times; public Vector4[] values; }
    public sealed class Motion { public string name; public float duration; public Track[] tracks; }
    static int Count(BinaryReader b, int max) { int n=b.ReadInt32(); if(n<0||n>max)throw new InvalidDataException("Invalid data count: "+n); return n; }
    static string Str(BinaryReader b) { return System.Text.Encoding.UTF8.GetString(b.ReadBytes(Count(b,1000000))); }
    static Vector3 V3(BinaryReader b) { return new Vector3(b.ReadSingle(),b.ReadSingle(),b.ReadSingle()); }
    static Vector4 V4(BinaryReader b) { return new Vector4(b.ReadSingle(),b.ReadSingle(),b.ReadSingle(),b.ReadSingle()); }
    static Quaternion Q(BinaryReader b) { Vector4 v=V4(b);return new Quaternion(v.x,v.y,v.z,v.w); }
    public static TuPackedModel Load(string path, string name)
    {
        TuPackedModel model=new TuPackedModel();model.root=new GameObject(name);
        using(BinaryReader b=new BinaryReader(File.OpenRead(path)))
        {
            if(System.Text.Encoding.ASCII.GetString(b.ReadBytes(8))!="TFUPACK1")throw new InvalidDataException("Unknown model format");
            Texture2D[] textures=new Texture2D[Count(b,10000)];
            for(int j=0;j<textures.Length;j++){byte[] png=b.ReadBytes(Count(b,100000000));Texture2D t=new Texture2D(2,2,TextureFormat.RGBA32,true);t.LoadImage(png);t.wrapMode=TextureWrapMode.Repeat;t.anisoLevel=4;textures[j]=t;}
            Material[] mats=new Material[Count(b,10000)];
            for(int j=0;j<mats.Length;j++)
            {
                string matName=Str(b);Vector4 c=V4(b);int tex=b.ReadInt32();float metal=b.ReadSingle(),smooth=b.ReadSingle();
                Shader shader=Resources.Load("ArchiveSurface",typeof(Shader)) as Shader;if(shader==null)shader=Shader.Find("Standard");if(shader==null)shader=Shader.Find("Diffuse");
                Material m=new Material(shader);m.name=matName;m.color=new Color(c.x,c.y,c.z,c.w);if(tex>=0)m.mainTexture=textures[tex];
                if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metal);if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",smooth);
                if(matName.IndexOf("Glow",StringComparison.OrdinalIgnoreCase)>=0||matName.IndexOf("Emis",StringComparison.OrdinalIgnoreCase)>=0)
                {
#if UNITY_5_3_OR_NEWER
                    m.EnableKeyword("_EMISSION");
#endif
                    if(m.HasProperty("_EmissionColor")){m.SetColor("_EmissionColor",new Color(.27f,.02f,.52f));m.SetTexture("_EmissionMap",m.mainTexture);}}
                mats[j]=m;
            }
            Geometry[] geometry=new Geometry[Count(b,10000)];model.GeometryMeshes=new Mesh[geometry.Length];
            for(int j=0;j<geometry.Length;j++)
            {
                Mesh m=new Mesh();m.name=Str(b);int n=Count(b,2000000);
#if UNITY_2017_3_OR_NEWER
                if(n>65535)m.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
#endif
                Vector3[] positions=new Vector3[n],normals=new Vector3[n];Vector2[] uv=new Vector2[n];
                for(int k=0;k<n;k++)positions[k]=V3(b);for(int k=0;k<n;k++)normals[k]=V3(b);for(int k=0;k<n;k++)uv[k]=new Vector2(b.ReadSingle(),b.ReadSingle());
                m.vertices=positions;m.normals=normals;m.uv=uv;
                if(b.ReadInt32()!=0)
                {
                    BoneWeight[] weights=new BoneWeight[n];
                    for(int k=0;k<n;k++){weights[k].boneIndex0=b.ReadInt32();weights[k].boneIndex1=b.ReadInt32();weights[k].boneIndex2=b.ReadInt32();weights[k].boneIndex3=b.ReadInt32();}
                    for(int k=0;k<n;k++){weights[k].weight0=b.ReadSingle();weights[k].weight1=b.ReadSingle();weights[k].weight2=b.ReadSingle();weights[k].weight3=b.ReadSingle();}
                    m.boneWeights=weights;
                }
                int sub=Count(b,1000);m.subMeshCount=sub;int[] materialIDs=new int[sub];
                for(int k=0;k<sub;k++){materialIDs[k]=b.ReadInt32();int[] indices=new int[Count(b,10000000)];for(int v=0;v<indices.Length;v++)indices[v]=b.ReadInt32();m.SetTriangles(indices,k);}
                m.RecalculateBounds();geometry[j]=new Geometry{mesh=m,materials=materialIDs};model.GeometryMeshes[j]=m;
            }
            Node[] definitions=new Node[Count(b,100000)];model.nodes=new Transform[definitions.Length];
            for(int j=0;j<definitions.Length;j++){Node n=new Node();n.name=Str(b);n.parent=b.ReadInt32();n.mesh=b.ReadInt32();n.skin=b.ReadInt32();n.position=V3(b);n.rotation=Q(b);n.scale=V3(b);definitions[j]=n;model.nodes[j]=new GameObject(n.name).transform;}
            for(int j=0;j<definitions.Length;j++){Node n=definitions[j];Transform t=model.nodes[j];t.parent=n.parent<0?model.root.transform:model.nodes[n.parent];t.localPosition=n.position;t.localRotation=n.rotation;t.localScale=n.scale;}
            Skin[] skins=new Skin[Count(b,1000)];
            for(int j=0;j<skins.Length;j++){int count=Count(b,1000);Skin s=new Skin{joints=new int[count],bindposes=new Matrix4x4[count]};for(int k=0;k<count;k++)s.joints[k]=b.ReadInt32();for(int k=0;k<count;k++){Matrix4x4 mat=new Matrix4x4();for(int r=0;r<4;r++)for(int c=0;c<4;c++)mat[r,c]=b.ReadSingle();s.bindposes[k]=mat;}skins[j]=s;}
            for(int j=0;j<definitions.Length;j++)
            {
                Node n=definitions[j];if(n.mesh<0)continue;Geometry g=geometry[n.mesh];Renderer renderer;
                if(n.skin>=0){Skin s=skins[n.skin];SkinnedMeshRenderer sk=model.nodes[j].gameObject.AddComponent<SkinnedMeshRenderer>();g.mesh.bindposes=s.bindposes;sk.sharedMesh=g.mesh;Transform[] bones=new Transform[s.joints.Length];for(int k=0;k<bones.Length;k++)bones[k]=model.nodes[s.joints[k]];sk.bones=bones;sk.rootBone=model.nodes[s.joints[0]];sk.updateWhenOffscreen=true;sk.quality=SkinQuality.Bone4;renderer=sk;}
                else{model.nodes[j].gameObject.AddComponent<MeshFilter>().sharedMesh=g.mesh;renderer=model.nodes[j].gameObject.AddComponent<MeshRenderer>();}
                Material[] selected=new Material[g.materials.Length];for(int k=0;k<selected.Length;k++)selected[k]=mats[g.materials[k]];renderer.sharedMaterials=selected;
            }
            int animationCount=Count(b,1000);
            for(int j=0;j<animationCount;j++)
            {
                Motion motion=new Motion();motion.name=Str(b);motion.tracks=new Track[Count(b,10000)];
                for(int k=0;k<motion.tracks.Length;k++){Track t=new Track();t.node=b.ReadInt32();t.kind=b.ReadInt32();int count=Count(b,1000000);t.times=new float[count];t.values=new Vector4[count];for(int v=0;v<count;v++)t.times[v]=b.ReadSingle();for(int v=0;v<count;v++){if(t.kind==1)t.values[v]=V4(b);else{Vector3 vec=V3(b);t.values[v]=new Vector4(vec.x,vec.y,vec.z,0);}}if(count>0)motion.duration=Mathf.Max(motion.duration,t.times[count-1]);motion.tracks[k]=t;}model.motions.Add(motion);
            }
            if(b.BaseStream.Position!=b.BaseStream.Length)throw new InvalidDataException("Unexpected trailing data");
        }
        return model;
    }
    public Motion FindMotion(string name) { return motions.Find(delegate(Motion m){return m.name.Equals(name,StringComparison.OrdinalIgnoreCase);}); }
    public void Pose(Motion motion,float time,float blend)
    {
        if(motion==null)return;float clock=motion.duration>0?time%motion.duration:0;
        foreach(Track t in motion.tracks)
        {
            if(t.times.Length==0)continue;int a=Array.BinarySearch(t.times,clock);if(a<0)a=Math.Max(0,~a-1);int b=Math.Min(a+1,t.times.Length-1);float mix=a==b?0:Mathf.InverseLerp(t.times[a],t.times[b],clock);Transform target=nodes[t.node];Vector4 v=Vector4.Lerp(t.values[a],t.values[b],mix);
            if(t.kind==1){Vector4 av=t.values[a],bv=t.values[b];Quaternion q=Quaternion.Slerp(new Quaternion(av.x,av.y,av.z,av.w),new Quaternion(bv.x,bv.y,bv.z,bv.w),mix);target.localRotation=Quaternion.Slerp(target.localRotation,q,blend);}
            else if(t.kind==0)target.localPosition=Vector3.Lerp(target.localPosition,new Vector3(v.x,v.y,v.z),blend);
            else target.localScale=Vector3.Lerp(target.localScale,new Vector3(v.x,v.y,v.z),blend);
        }
    }
}
