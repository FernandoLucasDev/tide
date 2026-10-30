using System.Collections.Generic;
using UnityEngine;

namespace MareDeFerro {
public static partial class HarborWorld {
    // Ring meshes keep clothing silhouettes continuous instead of assembling box torsos.
    static Transform Garment(string name,Transform parent,Material material,float[] heights,float[] widths,float[] depths) {
        const int sides=12;var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
        for(int r=0;r<heights.Length;r++)for(int s=0;s<=sides;s++) {
            float a=s*Mathf.PI*2/sides;
            vertices.Add(new Vector3(Mathf.Cos(a)*widths[r],heights[r],Mathf.Sin(a)*depths[r]));uv.Add(new Vector2(s/(float)sides,r/(float)(heights.Length-1)));
        }
        for(int r=0;r<heights.Length-1;r++)for(int s=0;s<sides;s++){int a=r*(sides+1)+s,b=a+sides+1;triangles.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
        for(int s=1;s<sides-1;s++){triangles.AddRange(new[]{0,s+1,s});int b=(heights.Length-1)*(sides+1);triangles.AddRange(new[]{b,b+s,b+s+1});}
        var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        var g=new GameObject(name).transform;g.SetParent(parent,false);g.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;g.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;return g;
    }
    static Transform Joint(string name,Transform parent,Vector3 p){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=p;return t;}
    static Transform DetailedPerson(string name,Vector3 p,Material coat,bool old) {
        var g=Joint(name,root,p);if(name!="Você")g.localRotation=Quaternion.Euler(0,180,0);
        Material trousers=Mat("Lona de trabalho",new Color(.12f,.17f,.18f),.08f,0,2);
        if(name=="Você")coat=trousers;
        Material boots=Mat("Borracha gasta",new Color(.075f,.08f,.075f),.24f);
        Material face=Mat("Pele "+name,name=="Tomé"?new Color(.32f,.19f,.12f):name=="Celina"?new Color(.47f,.29f,.19f):new Color(.65f,.44f,.3f),.16f);
        Material hair=Mat("Cabelo "+name,name=="Mara"?new Color(.6f,.58f,.5f):name=="Dália"?new Color(.32f,.13f,.065f):new Color(.075f,.055f,.04f),.08f);
        var torso=Joint("Respiração",g,Vector3.zero);
        Garment("Casaco costurado",torso,coat,new[]{.8f,.88f,1.05f,1.3f,1.39f,1.44f},new[]{.235f,.22f,.20f,.245f,.21f,.105f},new[]{.145f,.15f,.14f,.155f,.135f,.09f});
        Cylinder("Gola",new Vector3(0,1.44f,0),.115f,.10f,coat,torso);
        Cylinder("Pescoço",new Vector3(0,1.49f,0),.075f,.13f,face,torso);
        Ball("Cabeça",new Vector3(0,1.65f,.015f),new Vector3(.28f,.35f,.29f),face,torso);
        Ball("Nariz",new Vector3(0,1.64f,.158f),new Vector3(.065f,.085f,.065f),face,torso);
        for(int s=-1;s<=1;s+=2){
            Ball("Orelha",new Vector3(s*.14f,1.65f,.015f),new Vector3(.045f,.085f,.055f),face,torso);
            Ball("Olho",new Vector3(s*.059f,1.697f,.144f),new Vector3(.025f,.016f,.009f),boots,torso);
            Beam("Sobrancelha",new Vector3(s*.037f,1.727f,.139f),new Vector3(s*.083f,1.72f,.131f),.012f,hair,torso);
        }
        if(name=="Mara"||name=="Você") {
            Ball("Gorro de lã",new Vector3(0,1.81f,0),new Vector3(.30f,.20f,.31f),old?cream:teal,torso);
            Cylinder("Dobra do gorro",new Vector3(0,1.77f,0),.157f,.065f,old?cream:teal,torso);
        } else {
            Ball("Cabelo",new Vector3(0,1.785f,-.025f),new Vector3(.30f,.17f,.29f),hair,torso);
            if(name=="Celina"||name=="Dália"||name=="Nina")Ball("Coque",new Vector3(.02f,1.72f,-.16f),new Vector3(.19f,.2f,.14f),hair,torso);
            if(name=="Tomé")Ball("Barba",new Vector3(0,1.55f,.07f),new Vector3(.24f,.14f,.19f),hair,torso);
        }
        Beam("Costura frontal",new Vector3(0,.85f,.15f),new Vector3(0,1.35f,.16f),.012f,darkWood,torso);
        for(int i=0;i<4;i++)Ball("Botão",new Vector3(.025f,.98f+i*.095f,.157f),Vector3.one*.024f,brass,torso);
        for(int s=-1;s<=1;s+=2){
            Box("Bolso",new Vector3(s*.135f,1.04f,.143f),new Vector3(.13f,.12f,.025f),coat,torso,false);
            var leg=Joint(s<0?"Passo esquerdo":"Passo direito",g,new Vector3(s*.115f,.84f,0));
            Garment("Calça",leg,name=="Você"?yellow:trousers,new[]{-.65f,-.4f,-.26f,0f},new[]{.075f,.083f,.095f,.105f},new[]{.085f,.09f,.095f,.11f});
            Ball("Bota",new Vector3(0,-.75f,.055f),new Vector3(.18f,.19f,.32f),boots,leg);
            Cylinder("Cano da bota",new Vector3(0,-.61f,0),.086f,.22f,boots,leg);
            var arm=Joint(s<0?"Manga esquerda":"Manga direita",torso,new Vector3(s*.235f,1.35f,0));
            Garment("Manga",arm,coat,new[]{-.49f,-.36f,-.19f,0f},new[]{.065f,.073f,.085f,.10f},new[]{.07f,.075f,.085f,.10f});
            Ball("Mão",new Vector3(0,-.55f,.015f),new Vector3(.10f,.15f,.085f),face,arm);
            arm.localRotation=Quaternion.Euler(-5,0,s*7);
        }
        if(name=="Você"||name=="Tomé") {
            Material bib=name=="Você"?yellow:cream;
            Box("Peitilho",new Vector3(0,1.16f,.162f),new Vector3(.29f,.36f,.028f),bib,torso,false);
            for(int s=-1;s<=1;s+=2)Beam("Alça",new Vector3(s*.13f,1.15f,.18f),new Vector3(s*.16f,1.4f,.09f),.045f,bib,torso);
        }
        if(Application.isPlaying)g.gameObject.AddComponent<HarborActor>();
        return g;
    }
    static void DressHarbor() {
        Material petals=Mat("Bugainvília",new Color(.55f,.19f,.29f),.03f);
        Material rope=Mat("Cânhamo",new Color(.46f,.41f,.29f),.05f);
        foreach(Transform building in new List<Transform>(Children(root))) {
            if(!building.Find("Paredes"))continue;
            var walls=building.Find("Paredes");float w=walls.localScale.x,d=walls.localScale.z,h=walls.localScale.y;
            // Window boxes, broken patches of climbing vegetation and individual blooms.
            Box("Floreira",new Vector3(w*.25f,1.26f,-d*.5f-.35f),new Vector3(1.4f,.23f,.42f),wood,building,false);
            for(int j=0;j<13;j++) {
                Vector3 p=new Vector3(w*.25f+Random.Range(-.65f,.65f),1.43f+Random.Range(0,.2f),-d*.5f-.39f);
                Ball("Folhas da floreira",p,new Vector3(.28f,.22f,.24f),leaves,building);
                if(j%2==0)Ball("Flor",p+Vector3.up*.1f,Vector3.one*.13f,petals,building);
            }
            for(int j=0;j<18;j++) {
                float y=j*h/20;Vector3 p=new Vector3(-w*.48f+Mathf.Sin(j)*.16f,y,-d*.5f-.16f);
                Ball("Trepadeira",p,new Vector3(.44f,.33f,.15f),leaves,building);
                if(j%3==0)Ball("Flor na fachada",p+new Vector3(.15f,.1f,-.1f),Vector3.one*.14f,petals,building);
            }
            for(int s=-1;s<=1;s+=2)Beam("Mão francesa",new Vector3(s*w*.44f,h-.6f,-d*.5f),new Vector3(s*w*.44f,h-.05f,-d*.5f-.35f),.12f,darkWood,building);
        }
        // Nets are kept outside the walkable route; all dressing has no colliders.
        for(int k=0;k<3;k++) {
            var net=Joint("Rede secando",root,new Vector3(-18+k*1.3f,1.6f,6.5f));
            for(int j=0;j<9;j++) {
                float x=j*.12f;
                Rope("Malha",new Vector3(x,0,0),new Vector3(x,.95f,.45f),.08f,net,rope,.012f);
                Rope("Malha",new Vector3(0,j*.115f,j*.055f),new Vector3(.96f,j*.115f,j*.055f),.07f,net,rope,.012f);
            }
        }
        for(int i=0;i<9;i++) {
            Vector3 p=new Vector3(-6.02f,1.5f,3-i*2.1f);
            for(int r=0;r<3;r++) {
                var line=new GameObject("Cabo enrolado").AddComponent<LineRenderer>();line.transform.SetParent(root);line.useWorldSpace=true;line.sharedMaterial=rope;line.startWidth=line.endWidth=.027f;line.positionCount=25;
                for(int j=0;j<25;j++){float a=j*Mathf.PI/12;line.SetPosition(j,p+new Vector3(Mathf.Cos(a)*(.20f+r*.035f),.025f*r,Mathf.Sin(a)*(.20f+r*.035f)));}
            }
        }
    }
    static IEnumerable<Transform> Children(Transform parent){foreach(Transform child in parent)yield return child;}
    public static void DressTrainingVessel(Transform deck) {
        var hull=deck.Find("Casco");if(hull)hull.GetComponent<Renderer>().enabled=false;
        // The collision shell stays stable; curved visible hull narrows below the waterline.
        float[] zs={-8.2f,-7.4f,3.5f,8f};float[] widths={3.5f,4.3f,4.3f,4.3f};
        var v=new List<Vector3>();var t=new List<int>();
        for(int i=0;i<zs.Length;i++){v.Add(new Vector3(-widths[i],1.55f,zs[i]));v.Add(new Vector3(-widths[i]*.77f,-.7f,zs[i]));v.Add(new Vector3(widths[i]*.77f,-.7f,zs[i]));v.Add(new Vector3(widths[i],1.55f,zs[i]));}
        for(int i=0;i<zs.Length-1;i++)for(int j=0;j<3;j++){int a=i*4+j;t.AddRange(new[]{a,a+4,a+1,a+1,a+4,a+5});}
        t.AddRange(new[]{0,1,2,0,2,3});for(int i=0;i<t.Count;i+=3){int swap=t[i+1];t[i+1]=t[i+2];t[i+2]=swap;}var mesh=new Mesh{name="Casco curvo do São Bento"};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();
        var skin=Joint("Chapeamento do casco",deck,Vector3.zero);skin.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;skin.gameObject.AddComponent<MeshRenderer>().sharedMaterial=teal;
        for(int s=-1;s<=1;s+=2){
            Beam("Friso do casco",new Vector3(s*4.2f,.9f,-7.2f),new Vector3(s*4.2f,.9f,8),.12f,cream,deck);
            Beam("Corrimão",new Vector3(s*4.1f,2.51f,-7.8f),new Vector3(s*4.1f,2.51f,8),.095f,darkWood,deck);
            Box("Aro da janela",new Vector3(s*1.85f,3.4f,5.2f),new Vector3(.08f,.96f,.12f),darkWood,deck,false);
            Cylinder("Escape",new Vector3(s*1.6f,4.8f,7),.13f,1.9f,iron,deck);
        }
        Box("Travessa da janela",new Vector3(0,3.4f,5.18f),new Vector3(.1f,.88f,.12f),cream,deck,false);
        Box("Beiral da janela",new Vector3(0,3.91f,5.12f),new Vector3(4,.1f,.35f),darkWood,deck,false);
        Box("Porta da cabine",new Vector3(-1.35f,2.58f,5.21f),new Vector3(.7f,1.8f,.12f),teal,deck,false);
        Ball("Maçaneta da cabine",new Vector3(-1.12f,2.6f,5.12f),Vector3.one*.08f,brass,deck);
        var ring=new GameObject("Boia salva-vidas").AddComponent<LineRenderer>();ring.transform.SetParent(deck,false);ring.useWorldSpace=false;ring.sharedMaterial=cream;ring.startWidth=ring.endWidth=.12f;ring.positionCount=33;
        for(int j=0;j<33;j++){float a=j*Mathf.PI/16;ring.SetPosition(j,new Vector3(1.3f+Mathf.Cos(a)*.4f,2.6f+Mathf.Sin(a)*.4f,5.12f));}
        for(int i=0;i<7;i++)Box("Tábua longa do convés",new Vector3(-3.45f+i*1.15f,1.557f,0),new Vector3(.018f,.01f,15.7f),darkWood,deck,false);
        for(int i=0;i<4;i++)Cylinder("Barril de isca",new Vector3(-3.45f,1.94f,-6.8f+i*.67f),.27f,.7f,teal,deck);
        var drum=Cylinder("Tambor do guincho",new Vector3(3.3f,2.65f,.02f),.36f,.7f,iron,deck);drum.transform.localRotation=Quaternion.Euler(0,0,90);
        for(int i=0;i<7;i++){var winding=Cylinder("Cabo do tambor",new Vector3(3.03f+i*.09f,2.65f,.02f),.365f,.04f,wood,deck);winding.transform.localRotation=Quaternion.Euler(0,0,90);}
    }
}
public class HarborWorldCaption:MonoBehaviour {
    void LateUpdate(){if(HarborWorld.camera)transform.rotation=HarborWorld.camera.transform.rotation;}
}
// Art motion follows measured movement; it never moves the gameplay collider.
public class HarborActor:MonoBehaviour {
    Transform torso,leftLeg,rightLeg,leftArm,rightArm;Vector3 previous;float stride,weight;
    HarborGame game;float taskPose;
    void Start(){torso=transform.Find("Respiração");leftLeg=transform.Find("Passo esquerdo");rightLeg=transform.Find("Passo direito");leftArm=torso.Find("Manga esquerda");rightArm=torso.Find("Manga direita");previous=transform.position;if(name=="Você")game=FindFirstObjectByType<HarborGame>();}
    void LateUpdate(){
        float dt=Time.deltaTime;if(dt<=0){previous=transform.position;return;}
        float speed=Vector3.Distance(previous,transform.position)/dt;previous=transform.position;
        if(speed>10)speed=0;
        weight=Mathf.MoveTowards(weight,Mathf.Clamp01(speed/2),dt*6);stride+=dt*Mathf.Max(1,speed)*4;
        float step=Mathf.Sin(stride)*25*weight;
        leftLeg.localRotation=Quaternion.Euler(step,0,0);rightLeg.localRotation=Quaternion.Euler(-step,0,0);
        leftArm.localRotation=Quaternion.Euler(-step*.7f-5,0,-7);rightArm.localRotation=Quaternion.Euler(step*.7f-5,0,7);
        bool working=game&&game.WorkingOnDeck,carrying=game&&game.CarryingOnDeck;
        taskPose=Mathf.MoveTowards(taskPose,working||carrying?1:0,dt*5);
        if(taskPose>0){float action=working?Mathf.Sin(Time.time*7)*12:0;leftArm.localRotation=Quaternion.Slerp(leftArm.localRotation,Quaternion.Euler(-62+action,0,-8),taskPose);rightArm.localRotation=Quaternion.Slerp(rightArm.localRotation,Quaternion.Euler(-62-action,0,8),taskPose);}
        torso.localPosition=Vector3.up*(Mathf.Sin(Time.time*1.5f)*.006f+Mathf.Abs(Mathf.Sin(stride))*.014f*weight);
    }
}
}
