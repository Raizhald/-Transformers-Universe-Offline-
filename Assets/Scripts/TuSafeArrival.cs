using UnityEngine;

// Uses the actual controller capsule and a supported footprint, not a point-only floor test.
public static class TuSafeArrival
{
    public static bool IsClear(Vector3 position,CharacterController body)
    {
        float radius=body.radius;Vector3 center=position+body.center;
        Vector3 bottom=center-Vector3.up*(body.height*.5f-radius),top=center+Vector3.up*(body.height*.5f-radius);
        return !Physics.CheckCapsule(bottom,top,radius+.025f,1<<8,QueryTriggerInteraction.Ignore);
    }
    public static bool TryFind(Vector3 preferred,CharacterController body,out Vector3 position)
    {
        Physics.SyncTransforms();
        for(int ring=0;ring<=8;ring++)for(int side=0;side<(ring==0?1:16);side++)
        {
            float angle=side*Mathf.PI/8;
            Vector3 point=preferred+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*ring*1.5f;RaycastHit floor;
            if(!Physics.Raycast(point+Vector3.up*2,Vector3.down,out floor,6,1<<8,QueryTriggerInteraction.Ignore))continue;
            if(floor.normal.y<Mathf.Cos(body.slopeLimit*Mathf.Deg2Rad)||Mathf.Abs(floor.point.y-preferred.y)>1.2f)continue;
            bool supported=true;float spread=body.radius*.9f;
            for(int sample=0;sample<8;sample++)
            {
                float a=sample*Mathf.PI/4;Vector3 probe=floor.point+new Vector3(Mathf.Cos(a)*spread,.8f,Mathf.Sin(a)*spread);RaycastHit hit;
                if(!Physics.Raycast(probe,Vector3.down,out hit,1.6f,1<<8,QueryTriggerInteraction.Ignore)||Mathf.Abs(hit.point.y-floor.point.y)>.55f){supported=false;break;}
            }
            if(!supported)continue;point.y=floor.point.y+.09f;
            if(!IsClear(point,body))continue;
            position=point;return true;
        }
        position=Vector3.zero;return false;
    }
}
