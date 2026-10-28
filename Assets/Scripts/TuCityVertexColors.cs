using System;
using System.IO;
using UnityEngine;

public static class TuCityVertexColors
{
    public static void Restore(TuPackedModel model,string file)
    {
        using(var reader=new BinaryReader(File.OpenRead(Path.Combine(Application.streamingAssetsPath,file))))
        {
            int meshes=reader.ReadInt32();if(meshes!=model.GeometryMeshes.Length)throw new InvalidDataException("Source colour mesh count differs");
            for(int i=0;i<meshes;i++){int n=reader.ReadInt32();if(n!=model.GeometryMeshes[i].vertexCount)throw new InvalidDataException("Source colour vertex count differs");var colors=new Color32[n];for(int k=0;k<n;k++)colors[k]=new Color32(reader.ReadByte(),reader.ReadByte(),reader.ReadByte(),reader.ReadByte());model.GeometryMeshes[i].colors32=colors;}
            if(reader.BaseStream.Position!=reader.BaseStream.Length)throw new InvalidDataException("Unexpected source colour bytes");
        }
    }
    public static void EnsureWhiteDefaults(TuPackedModel model)
    {
        foreach(var mesh in model.GeometryMeshes)if(mesh.colors32.Length==0){var colors=new Color32[mesh.vertexCount];for(int k=0;k<colors.Length;k++)colors[k]=new Color32(255,255,255,255);mesh.colors32=colors;}
    }
}
