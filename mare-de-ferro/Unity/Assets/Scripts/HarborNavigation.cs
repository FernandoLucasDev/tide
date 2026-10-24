using UnityEngine;

namespace MareDeFerro {
// Shared by the steering controller and its runtime journey tests.
public static class HarborNavigation {
    public static readonly Vector3 Mooring = new Vector3(.6f,.1f,-10);
    public static readonly Vector3 Landing = new Vector3(-4.5f,1.6f,-8.8f);
    public static readonly Vector3 Market = new Vector3(1,1.5f,6.1f);
    public static float Distance(Vector3 a,Vector3 b) => Vector2.Distance(new Vector2(a.x,a.z),new Vector2(b.x,b.z));
    public static bool CanOccupy(Vector3 position,float yaw) {
        Vector3 forward=Quaternion.Euler(0,yaw,0)*Vector3.forward;
        for(int i=-1;i<=1;i++) {
            Vector3 p=position+forward*(i*2.1f);
            const float r=1.3f;
            if(p.x-r < -65 || p.x+r > 65 || p.z-r < -75 || p.z+r > 4.8f)return false;
            float x=Mathf.Clamp(p.x,-6.3f,-2.7f),z=Mathf.Clamp(p.z,-16.6f,5.5f);
            if(Vector2.Distance(new Vector2(p.x,p.z),new Vector2(x,z))<r)return false;
            if(Distance(p,new Vector3(-14,0,-2))<4.4f || Distance(p,new Vector3(16,0,-5))<5.0f || Distance(p,new Vector3(36,0,-32))<3.7f)return false;
            // Coastal rocks on both sides of the quay.
            if(p.z > -1 && (p.x < -21 || p.x > 21))return false;
        }
        return true;
    }
}
}
