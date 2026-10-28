using UnityEngine;

public static class TuUiSkin
{
    public static GUIStyle Create(Texture2D image,int left,int top,int right,int bottom)
    {
        int width=Mathf.Max(1,image.width-1),height=Mathf.Max(1,image.height-1);
        if(left+right>width){float scale=width/(float)(left+right);left=Mathf.FloorToInt(left*scale);right=width-left;}
        if(top+bottom>height){float scale=height/(float)(top+bottom);top=Mathf.FloorToInt(top*scale);bottom=height-top;}
        var result=new GUIStyle();result.normal.background=image;result.border=new RectOffset(left,right,top,bottom);return result;
    }
    public static Matrix4x4 Around(Matrix4x4 canvas,Vector2 center,float angle)
    {
        return canvas*Matrix4x4.Translate(new Vector3(center.x,center.y,0))*Matrix4x4.Rotate(Quaternion.Euler(0,0,angle))*Matrix4x4.Translate(new Vector3(-center.x,-center.y,0));
    }
}
