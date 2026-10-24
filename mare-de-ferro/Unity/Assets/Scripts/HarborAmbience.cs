using UnityEngine;
namespace MareDeFerro {
// Quiet original synthesized surf. No downloaded audio or licensed samples.
public class HarborAmbience:MonoBehaviour {
    void Start() {
        const int rate=22050,seconds=16;float[] data=new float[rate*seconds];
        var random=new System.Random(913);float low=0,soft=0;
        for(int i=0;i<data.Length;i++) {
            float t=i/(float)rate;float noise=(float)random.NextDouble()*2-1;
            low=Mathf.Lerp(low,noise,.075f);soft=Mathf.Lerp(soft,noise,.012f);
            float wave=.48f+.24f*Mathf.Sin(t*Mathf.PI*.5f)+.17f*Mathf.Sin(t*Mathf.PI*.25f+.8f);
            // The envelope and all periodic components wrap exactly at the loop boundary.
            data[i]=(low*.7f+soft*.3f)*wave*.22f;
        }
        var clip=AudioClip.Create("Brisa e arrebentação",data.Length,1,rate,false);clip.SetData(data,0);
        var source=gameObject.AddComponent<AudioSource>();source.clip=clip;source.loop=true;source.volume=.45f;source.spatialBlend=0;source.Play();
    }
}
}
