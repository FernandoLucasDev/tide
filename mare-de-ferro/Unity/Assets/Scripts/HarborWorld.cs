using System.Collections.Generic;
using UnityEngine;

namespace MareDeFerro {
public static class HarborWorld {
    static Transform root;
    static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
    public static Material wood, darkWood, plaster, roof, stone, teal, iron, brass, glass, cream, leaves, yellow, skin;
    public static Transform boat, player, mara;
    public static Vector3 DockSpawn = new Vector3(-4.5f, 1.6f, -4.5f);
    public static readonly Vector3[] FishingSpots = { new Vector3(-8,0,-29),new Vector3(12,0,-37),new Vector3(-23,0,-42) };
    public static Camera camera;

    public static Transform Create() {
        Random.InitState(7419); materials.Clear();
        root=new GameObject("Porto das Velas — cenário").transform;
        wood=Mat("Madeira salgada",new Color(.31f,.24f,.16f),.15f,0,1);
        darkWood=Mat("Vigas",new Color(.17f,.13f,.09f),.1f,0,1);
        plaster=Mat("Cal marfim",new Color(.78f,.70f,.54f),.08f,0,2);
        roof=Mat("Telha terracota",new Color(.45f,.16f,.09f),.18f,0,2);
        stone=Mat("Pedra costeira",new Color(.36f,.36f,.30f),.12f,0,2);
        teal=Mat("Pintura petróleo",new Color(.08f,.32f,.32f),.28f,.08f,2);
        iron=Mat("Ferro",new Color(.13f,.17f,.17f),.45f,.6f,2);
        brass=Mat("Latão",new Color(.64f,.40f,.15f),.55f,.5f);
        cream=Mat("Casco marfim",new Color(.74f,.70f,.54f),.28f,.1f,2);
        leaves=Mat("Folhagem",new Color(.18f,.29f,.16f),.05f);
        yellow=Mat("Impermeável mostarda",new Color(.86f,.48f,.07f),.27f);
        skin=Mat("Pele",new Color(.56f,.34f,.21f),.15f);
        glass=Mat("Luz âmbar",new Color(.95f,.56f,.16f),.65f,.15f);
        glass.EnableKeyword("_EMISSION"); glass.SetColor("_EmissionColor",new Color(1,.45f,.1f)*1.5f);
        Lighting(); Sea(); Landscape(); Harbor();
        if(Application.isPlaying) {
            var pieces=new List<GameObject>();
            foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>()) {
                if(renderer.GetComponent<TextMesh>()||renderer.sharedMaterial.shader.name=="MareDeFerro/Water")continue;
                pieces.Add(renderer.gameObject);
            }
            StaticBatchingUtility.Combine(pieces.ToArray(),root.gameObject);
        }
        boat=Boat(new Vector3(.6f,.10f,-10),180,"Andorinha");
        Boat(new Vector3(-14,.13f,-2),90,"Santa Brígida",1.15f);
        Boat(new Vector3(16,.15f,-5),-20,"Último Porto",1.35f);
        Boat(new Vector3(36,.1f,-32),20,"Aurora",.85f);
        player=Person("Você",DockSpawn,yellow,false);
        mara=Person("Mara",new Vector3(-5.6f,1.54f,3),Mat("Casaco de Mara",new Color(.22f,.31f,.28f)),true);
        Person("Tomé",new Vector3(1.2f,1.55f,8.2f),cream,true);
        Person("Nina",new Vector3(-15,1.55f,9.1f),teal,false);
        Person("Ivo",new Vector3(8,1.55f,7),Mat("Casaco coral",new Color(.53f,.20f,.13f)),false);
        for(int i=0;i<FishingSpots.Length;i++) Buoy(FishingSpots[i],i);
        var cg=new GameObject("Câmera do porto"); camera=cg.AddComponent<Camera>(); cg.tag="MainCamera";
        camera.nearClipPlane=.15f; camera.farClipPlane=300; camera.fieldOfView=43;
        camera.allowHDR=true; camera.allowMSAA=true;
        camera.transform.position=new Vector3(25,24,-34); camera.transform.LookAt(new Vector3(-1,3,5));
        camera.gameObject.AddComponent<AudioListener>(); camera.gameObject.AddComponent<HarborGrade>();
        return root;
    }
    public static Material Mat(string name,Color color,float smooth=.2f,float metal=0,int texture=0) {
        if(materials.TryGetValue(name,out var old)) return old;
        var baseMaterial=Resources.Load<Material>("HarborSurface");
        var m=baseMaterial?new Material(baseMaterial):new Material(Shader.Find("Standard")); m.name=name; m.color=color;
        m.SetFloat("_Glossiness",smooth); m.SetFloat("_Metallic",metal);
        if(texture>0) {
            int size=128; var t=new Texture2D(size,size,TextureFormat.RGB24,true); t.name=name+" — textura";
            Color[] pixels=new Color[size*size];
            for(int y=0;y<size;y++) for(int x=0;x<size;x++) {
                float n=Mathf.PerlinNoise(x*(texture==1?.12f:.13f),y*(texture==1?.018f:.13f));
                float grain=Random.value*.018f;
                float f=.86f+n*.23f+grain;
                if(texture==1) f-=Mathf.Pow(Mathf.Abs(Mathf.Sin(x*.25f+Mathf.Sin(y*.03f)*1.5f)),22)*.19f;
                pixels[y*size+x]=new Color(f,f,f);
            }
            t.SetPixels(pixels); t.Apply(); t.wrapMode=TextureWrapMode.Repeat; t.anisoLevel=4;
            m.mainTexture=t; m.mainTextureScale=new Vector2(2,2);
        }
        materials[name]=m; return m;
    }
    public static GameObject Shape(PrimitiveType type,string name,Vector3 pos,Vector3 size,Material mat,Transform parent=null,bool solid=true) {
        var g=GameObject.CreatePrimitive(type); g.name=name; g.transform.SetParent(parent?parent:root,false);
        g.transform.localPosition=pos; g.transform.localScale=size;
        g.GetComponent<Renderer>().sharedMaterial=mat;
        if(!solid) Object.DestroyImmediate(g.GetComponent<Collider>());
        return g;
    }
    public static GameObject Box(string name,Vector3 pos,Vector3 size,Material mat,Transform parent=null,bool solid=true) => Shape(PrimitiveType.Cube,name,pos,size,mat,parent,solid);
    public static GameObject Ball(string name,Vector3 pos,Vector3 size,Material mat,Transform parent=null,bool solid=false) => Shape(PrimitiveType.Sphere,name,pos,size,mat,parent,solid);
    public static GameObject Cylinder(string name,Vector3 pos,float radius,float height,Material mat,Transform parent=null,bool solid=false) => Shape(PrimitiveType.Cylinder,name,pos,new Vector3(radius*2,height*.5f,radius*2),mat,parent,solid);
    public static void Beam(string name,Vector3 a,Vector3 b,float width,Material mat,Transform parent=null) {
        var o=Box(name,(a+b)*.5f,new Vector3(width,(b-a).magnitude,width),mat,parent,false);
        o.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
    }
    public static void Rope(string name,Vector3 a,Vector3 b,float sag,Transform parent=null,Material material=null,float width=.035f) {
        var g=new GameObject(name); g.transform.SetParent(parent?parent:root,false);
        var l=g.AddComponent<LineRenderer>(); l.useWorldSpace=false; l.sharedMaterial=material?material:wood; l.startWidth=l.endWidth=width; l.positionCount=15;
        for(int i=0;i<15;i++){ float t=i/14f; l.SetPosition(i,Vector3.Lerp(a,b,t)+Vector3.down*Mathf.Sin(t*Mathf.PI)*sag); }
    }
    static void Lighting() {
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(.48f,.56f,.61f);
        RenderSettings.ambientEquatorColor=new Color(.43f,.42f,.35f);
        RenderSettings.ambientGroundColor=new Color(.19f,.22f,.21f);
        RenderSettings.ambientIntensity=1;
        var ambient=new UnityEngine.Rendering.SphericalHarmonicsL2();
        ambient.AddAmbientLight(new Color(.27f,.31f,.32f));RenderSettings.ambientProbe=ambient;
        RenderSettings.fog=true; RenderSettings.fogColor=new Color(.57f,.60f,.57f);
        RenderSettings.fogMode=FogMode.ExponentialSquared; RenderSettings.fogDensity=.0065f;
        var sky=Resources.Load<Shader>("HarborSky"); RenderSettings.skybox=new Material(sky);
        var sun=new GameObject("Sol de fim de tarde").AddComponent<Light>(); sun.transform.SetParent(root);
        sun.type=LightType.Directional; sun.color=new Color(1,.76f,.49f); sun.intensity=1.6f;
        sun.transform.rotation=Quaternion.Euler(24,-125,0); sun.shadows=LightShadows.Soft; sun.shadowStrength=.75f; sun.shadowBias=.03f;
        RenderSettings.sun=sun; RenderSettings.skybox.SetVector("_SunDir",-sun.transform.forward);
        QualitySettings.shadows=ShadowQuality.All; QualitySettings.shadowResolution=ShadowResolution.High;
        QualitySettings.shadowDistance=95; QualitySettings.antiAliasing=4; QualitySettings.vSyncCount=1;
    }
    static void Sea() {
        var mesh=new Mesh(); mesh.name="Superfície subdividida"; mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
        const int n=160; var v=new Vector3[(n+1)*(n+1)]; var uv=new Vector2[v.Length]; var tri=new int[n*n*6]; int k=0;
        for(int z=0;z<=n;z++) for(int x=0;x<=n;x++) { int i=z*(n+1)+x; v[i]=new Vector3((x-n/2)*6,0,(z-n/2)*6); uv[i]=new Vector2(x/(float)n,z/(float)n); }
        for(int z=0;z<n;z++) for(int x=0;x<n;x++){int i=z*(n+1)+x; tri[k++]=i;tri[k++]=i+n+1;tri[k++]=i+1;tri[k++]=i+1;tri[k++]=i+n+1;tri[k++]=i+n+2;}
        mesh.vertices=v;mesh.uv=uv;mesh.triangles=tri;mesh.RecalculateNormals();mesh.RecalculateBounds();
        var g=new GameObject("Mar — ondas e reflexos");g.transform.SetParent(root);g.transform.localPosition=new Vector3(0,-.04f,0);
        g.AddComponent<MeshFilter>().sharedMesh=mesh;
        g.AddComponent<MeshRenderer>().sharedMaterial=new Material(Resources.Load<Shader>("HarborWater"));
    }
    static void Landscape() {
        Box("Terreno do porto",new Vector3(-1,-.8f,22),new Vector3(48,4,34),stone);
        var grass=Mat("Mato seco",new Color(.31f,.36f,.20f),.1f,0,2);
        for(int i=0;i<50;i++) {
            float x=Random.Range(-34,28),z=Random.Range(29,61),h=Random.Range(5,17);
            Rock("Encosta",new Vector3(x,-1,z),new Vector3(Random.Range(10,21),h,Random.Range(10,20)),i%3==0?stone:grass);
        }
        for(int i=0;i<40;i++) {
            float x=i<20?Random.Range(-35,-23):Random.Range(23,31); float z=Random.Range(2,29);
            Rock("Rocha costeira",new Vector3(x,Random.Range(-1.3f,-.5f),z),new Vector3(Random.Range(2,7),Random.Range(3,7),Random.Range(3,8)),stone);
        }
        for(int i=0;i<55;i++) Tree(new Vector3(Random.Range(-27,23),Random.Range(3,6),Random.Range(29,48)),Random.Range(.8f,1.8f));
        for(int i=0;i<15;i++) {
            float x=Random.Range(-70,80),z=Random.Range(80,130);
            Rock("Ilhas no horizonte",new Vector3(x,-2,z),new Vector3(Random.Range(15,38),Random.Range(7,25),20),stone);
        }
        Rock("Ilha do farol",new Vector3(37,-2,24),new Vector3(16,7,14),stone);
        var lightRoot=new GameObject("Farol das Velas").transform; lightRoot.SetParent(root);lightRoot.position=new Vector3(37,3.6f,24);
        Cylinder("Torre",new Vector3(0,5,0),1.05f,10,plaster,lightRoot);
        Cylinder("Base",new Vector3(0,.3f,0),1.65f,.6f,stone,lightRoot);
        Cylinder("Faixa",new Vector3(0,6.7f,0),1.08f,.6f,roof,lightRoot);
        Cylinder("Varanda",new Vector3(0,9.4f,0),1.7f,.22f,iron,lightRoot);
        Cylinder("Lanterna",new Vector3(0,10.2f,0),.85f,1.4f,glass,lightRoot);
        Cylinder("Cobertura",new Vector3(0,11.0f,0),1.35f,.4f,teal,lightRoot);
        for(int i=0;i<10;i++){float a=i*Mathf.PI*.2f;Beam("Guarda-corpo",new Vector3(Mathf.Cos(a)*1.5f,9.4f,Mathf.Sin(a)*1.5f),new Vector3(Mathf.Cos(a)*1.5f,10.1f,Mathf.Sin(a)*1.5f),.045f,iron,lightRoot);}
        for(int i=0;i<12;i++) Ball("Espuma nas rochas",new Vector3(33+Random.Range(-5,9),.02f,18+Random.Range(-4,4)),new Vector3(3,.05f,.45f),cream);
    }
    static void Tree(Vector3 p,float s) {
        Cylinder("Tronco",p+Vector3.up*s, .15f*s,2.7f*s,darkWood);
        for(int j=0;j<3;j++) Ball("Copa",p+new Vector3(Random.Range(-.5f,.5f)*s,(2+j*.45f)*s,Random.Range(-.4f,.4f)*s),new Vector3(2.3f,1.8f,2)*s,leaves);
    }
    static void Rock(string name,Vector3 p,Vector3 size,Material mat) {
        const int sides=9;var verts=new List<Vector3>();var tris=new List<int>();
        for(int ring=0;ring<4;ring++)for(int j=0;j<sides;j++) {
            float a=j*Mathf.PI*2/sides;float radius=(ring==0?.53f:ring==1?.55f:ring==2?.36f:.08f)*Random.Range(.8f,1.15f);
            verts.Add(new Vector3(Mathf.Cos(a)*radius*size.x,(ring/3f+Random.Range(-.04f,.04f))*size.y,Mathf.Sin(a)*radius*size.z));
        }
        for(int ring=0;ring<3;ring++)for(int j=0;j<sides;j++){int a=ring*sides+j,b=ring*sides+(j+1)%sides,c=a+sides,d=b+sides;tris.AddRange(new[]{a,c,b,b,c,d});}
        for(int j=1;j<sides-1;j++)tris.AddRange(new[]{27,27+j,27+j+1});
        // Separate vertices produce broad weathered facets, rather than rounded boulders.
        var flat=new Vector3[tris.Count];for(int i=0;i<tris.Count;i++)flat[i]=verts[tris[i]];
        var indices=new int[flat.Length];for(int i=0;i<indices.Length;i++)indices[i]=i;
        var mesh=new Mesh();mesh.vertices=flat;mesh.triangles=indices;mesh.RecalculateNormals();mesh.RecalculateBounds();
        var g=new GameObject(name);g.transform.SetParent(root);g.transform.position=p;g.transform.rotation=Quaternion.Euler(0,Random.Range(0,360),0);
        g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=mat;
    }
    static void Harbor() {
        // The waterfront is a continuous walkable stone quay; the narrow pier extends into the sea.
        Box("Cais de pedra",new Vector3(-1,.75f,7),new Vector3(45,1.5f,4),stone);
        Box("Piso da vila",new Vector3(-1,1.24f,20),new Vector3(43,.3f,23),Mat("Pavimento",new Color(.48f,.44f,.34f),.2f,0,2));
        for(int i=0;i<45;i++) {
            float x=-23+i; Box("Pedras da borda",new Vector3(x,1.48f,5.15f),new Vector3(.94f,.28f,.58f),i%4==0?cream:stone);
            Box("Alvenaria do cais",new Vector3(x,.7f,5.02f),new Vector3(.91f,.9f,.17f),i%3==0?plaster:stone);
        }
        for(int row=0;row<3;row++)for(int i=0;i<53;i++) {
            Box("Cantaria do muro",new Vector3(-23+i*.86f+(row%2)*.4f,.23f+row*.43f,4.88f),new Vector3(.79f,.37f,.24f),i%7==0?plaster:stone,null,false);
        }
        for(int z=0;z<55;z++) {
            float zz=5.3f-z*.4f;
            Box("Tábua do píer",new Vector3(-4.5f,1.36f,zz),new Vector3(3.45f,.22f,.36f),z%6==0?darkWood:wood);
            if(z%7==0) for(int side=0;side<2;side++) {
                float x=side==0?-6.02f:-2.98f;
                Cylinder("Estaca",new Vector3(x,.52f,zz),.16f,3.1f,darkWood,null,true);
                Cylinder("Topo de estaca",new Vector3(x,2.08f,zz),.20f,.12f,wood);
                for(int r=0;r<3;r++) Cylinder("Corda enrolada",new Vector3(x,1.64f+r*.07f,zz),.18f,.035f,cream);
                if(z<49 && side==0) Rope("Corda lateral",new Vector3(x,1.85f,zz),new Vector3(x,1.85f,zz-2.8f),.25f);
            }
        }
        Box("Passarela de embarque",new Vector3(-1.7f,1.16f,-9.1f),new Vector3(2.2f,.13f,1.05f),wood);
        for(int i=0;i<6;i++) {
            float z=3-i*3.25f;
            Beam("Escora do píer",new Vector3(-6,.1f,z),new Vector3(-3,1.15f,z-1),.13f,darkWood);
            Beam("Travessa do píer",new Vector3(-6,.75f,z),new Vector3(-3,.75f,z),.18f,darkWood);
        }
        Building("Oficina de Nina",new Vector3(-15,1.4f,14),7,5,4.6f,teal,true);
        Building("Mercado de Tomé",new Vector3(1,1.4f,12),7,4.5f,4.2f,plaster,true);
        Building("Casa de Mara",new Vector3(-7,1.4f,16),5,5,5,plaster,false);
        Building("Venda de Dália",new Vector3(11,1.4f,14),6,6,5.2f,plaster,true);
        Building("Casa do alto",new Vector3(-14,3.4f,25),6,5,5.5f,plaster,false);
        Building("Casa verde",new Vector3(-4,3.6f,27),5,5,4.3f,Mat("Cal verde",new Color(.44f,.53f,.41f),.1f,0,2),false);
        Building("Pensão",new Vector3(7,3.6f,26),6,5,6,plaster,false);
        Building("Capela",new Vector3(-13,7.2f,38),6,8,5.5f,plaster,false);
        Box("Torre da capela",new Vector3(-9.4f,12.5f,36),new Vector3(2.6f,10,2.7f),plaster);
        Roof(new Vector3(-9.4f,17.5f,36),3,3,1.6f,root);
        Box("Sino",new Vector3(-9.4f,15.5f,34.6f),new Vector3(.9f,1.3f,.1f),darkWood);
        Beam("Cruz",new Vector3(-9.4f,18.6f,36),new Vector3(-9.4f,20,36),.13f,wood);
        Beam("Cruz",new Vector3(-9.95f,19.5f,36),new Vector3(-8.85f,19.5f,36),.13f,wood);
        for(int i=0;i<12;i++) Box("Escada da vila",new Vector3(-20,1.4f+i*.24f,10+i*.58f),new Vector3(2.5f,.25f,.65f),stone);
        for(int i=0;i<10;i++) Box("Escada do alto",new Vector3(19,1.4f+i*.24f,16+i*.65f),new Vector3(2.5f,.25f,.72f),stone);
        Sign("OFICINA NAVAL",new Vector3(-15,4.0f,11.42f),2.8f);
        Sign("PEIXE FRESCO",new Vector3(1,4.0f,9.66f),2.8f);
        Sign("DÁLIA • VENDA",new Vector3(11,4.5f,10.95f),2.8f);
        Sign("PORTO DAS VELAS",new Vector3(-9.6f,3.1f,5.0f),3.8f);
        // Market awning, legs, crates of fish, and an outdoor kitchen.
        for(int side=0;side<2;side++) Cylinder("Suporte do toldo",new Vector3(-2.6f+side*7.1f,2.8f,7),.065f,2.8f,wood);
        var awn=Box("Toldo",new Vector3(1,4.05f,8.4f),new Vector3(7.5f,.07f,3.4f),teal,null,false); awn.transform.rotation=Quaternion.Euler(-9,0,0);
        for(int i=0;i<8;i++) Box("Barra do toldo",new Vector3(-2.4f+i*.94f,3.85f,6.78f),new Vector3(.9f,.38f,.06f),i%2==0?teal:cream,null,false);
        Box("Balcão de peixe",new Vector3(1,2.05f,7.5f),new Vector3(5.7f,1.15f,1.05f),wood);
        for(int i=0;i<5;i++) {
            Box("Bandeja",new Vector3(-1.1f+i*1.05f,2.67f,7.5f),new Vector3(.9f,.12f,.8f),iron);
            for(int j=0;j<4;j++) {var f=Ball("Peixe",new Vector3(-1.4f+i*1.05f+j*.19f,2.8f,7.5f),new Vector3(.12f,.09f,.47f),j%2==0?cream:teal);f.transform.Rotate(0,Random.Range(-20,20),0);}
        }
        for(int i=0;i<8;i++) {float x=-18+i*5.2f; Lantern(new Vector3(x,3.4f,5.8f));}
        for(int i=0;i<5;i++) Lantern(new Vector3(-6,3.4f,3-i*4.2f));
        Rope("Varal de luzes",new Vector3(-15,6.1f,11),new Vector3(11,6.5f,11),2.0f);
        for(int i=0;i<12;i++) {float t=i/11f; Ball("Lâmpada de festa",new Vector3(Mathf.Lerp(-15,11,t),Mathf.Lerp(6.1f,6.5f,t)-Mathf.Sin(t*Mathf.PI)*2,11),Vector3.one*.13f,glass);}
        for(int i=0;i<14;i++) {
            float x=Random.Range(-21,20),z=Random.Range(5.9f,8.6f);
            if(Mathf.Abs(x-1)<4||Mathf.Abs(x+4.5f)<2) continue;
            Crate(new Vector3(x,1.55f,z),Random.Range(.65f,1.0f));
            if(i%2==0) Barrel(new Vector3(x+.9f,1.55f,z));
        }
        for(int i=0;i<4;i++) CrabPot(new Vector3(-5.3f,1.49f+i*.48f,-12.5f),root);
        CrabPot(new Vector3(-3.4f,1.49f,-2),root);
        for(int i=0;i<4;i++) {
            Cylinder("Barril no cais",new Vector3(-3.4f,1.9f,-5.5f-i*.75f),.27f,.8f,teal);
            Cylinder("Tampa",new Vector3(-3.4f,2.31f,-5.5f-i*.75f),.28f,.05f,wood);
        }
        for(int i=0;i<14;i++) {
            float x=Random.Range(-20,20);float z=Random.Range(12,27);
            Cylinder("Vaso",new Vector3(x,1.7f,z),.23f,.5f,roof);
            Ball("Flores",new Vector3(x,2.06f,z),new Vector3(.65f,.55f,.6f),i%2==0?leaves:Mat("Flores coral",new Color(.66f,.22f,.23f)));
        }
        // A few small birds in the distant sky, two angled wings each.
        for(int i=0;i<9;i++){Vector3 p=new Vector3(Random.Range(-30,35),Random.Range(14,24),Random.Range(20,55));Beam("Gaivota",p+new Vector3(-.35f,.12f,0),p,.035f,cream);Beam("Gaivota",p,p+new Vector3(.35f,.12f,0),.035f,cream);}
    }
    static void Building(string name,Vector3 p,float w,float d,float h,Material walls,bool shop) {
        var b=new GameObject(name).transform;b.SetParent(root);b.localPosition=p;
        if(p.y>1.5f)Box("Muro do terraço",new Vector3(0,-(p.y-1.4f)/2,0),new Vector3(w+1.4f,p.y-1.4f,d+1.6f),stone,b);
        Box("Paredes",new Vector3(0,h/2,0),new Vector3(w,h,d),walls,b);
        Box("Rodapé",new Vector3(0,.25f,0),new Vector3(w+.1f,.5f,d+.12f),stone,b);
        for(int i=-1;i<=1;i+=2) Box("Cunhal",new Vector3(i*(w/2-.1f),h/2,-d/2-.05f),new Vector3(.18f,h,.15f),cream,b,false);
        Roof(new Vector3(0,h,0),w+.7f,d+.7f,1.65f,b);
        Box("Porta",new Vector3(-w*.2f,1.1f,-d/2-.06f),new Vector3(1.05f,2.2f,.12f),teal,b);
        Box("Batente",new Vector3(-w*.2f,2.28f,-d/2-.12f),new Vector3(1.25f,.16f,.17f),wood,b,false);
        Ball("Maçaneta",new Vector3(-w*.2f+.34f,1.05f,-d/2-.16f),Vector3.one*.09f,brass,b);
        Window(new Vector3(w*.25f,2.1f,-d/2-.08f),b);
        // Timber lintels, plaster wear and corner stones break up large wall surfaces.
        for(int s=-1;s<=1;s+=2)for(int j=0;j<Mathf.FloorToInt(h/.55f);j++)Box("Pedra de quina",new Vector3(s*(w/2-.14f),.38f+j*.55f,-d/2-.09f),new Vector3(j%2==0?.38f:.27f,.22f,.17f),j%3==0?stone:cream,b,false);
        for(int i=0;i<7;i++) {
            float x=Random.Range(-w*.43f,w*.43f),y=Random.Range(.4f,h-.4f);
            if(Mathf.Abs(x+w*.2f)<.7f&&y<2.5f)continue;if(Mathf.Abs(x-w*.25f)<.8f&&Mathf.Abs(y-2.1f)<.9f)continue;
            Box("Cal desgastada",new Vector3(x,y,-d/2-.012f),new Vector3(Random.Range(.16f,.48f),Random.Range(.08f,.19f),.025f),cream,b,false);
        }
        Box("Viga frontal",new Vector3(0,h-.12f,-d/2-.1f),new Vector3(w+.2f,.16f,.17f),wood,b,false);
        if(h>4.7f) Window(new Vector3(-w*.20f,3.7f,-d/2-.08f),b);
        Box("Chaminé",new Vector3(w*.25f,h+1.2f,.4f),new Vector3(.75f,2,.7f),plaster,b);
        if(shop) Box("Degrau",new Vector3(0,.1f,-d/2-.45f),new Vector3(w,.2f,.85f),stone,b);
    }
    static void Window(Vector3 p,Transform parent) {
        Box("Moldura",p,new Vector3(1.25f,1.35f,.12f),darkWood,parent,false);
        Box("Janela acesa",p+new Vector3(0,0,-.08f),new Vector3(1.06f,1.16f,.04f),glass,parent,false);
        Box("Travessa",p+new Vector3(0,0,-.12f),new Vector3(.055f,1.2f,.06f),wood,parent,false);
        Box("Travessa",p+new Vector3(0,0,-.12f),new Vector3(1.1f,.055f,.06f),wood,parent,false);
        for(int s=-1;s<=1;s+=2) {
            var shutter=Box("Veneziana",p+new Vector3(s*.86f,0,0),new Vector3(.39f,1.35f,.09f),teal,parent,false);
            shutter.transform.localRotation=Quaternion.Euler(0,s*16,0);
        }
        Box("Peitoril",p+new Vector3(0,-.72f,-.12f),new Vector3(1.5f,.12f,.3f),cream,parent,false);
    }
    static void Roof(Vector3 p,float w,float d,float rise,Transform parent) {
        // Roof ridge runs along X. Individual tile courses create readable terracotta rhythm.
        float len=Mathf.Sqrt(d*d*.25f+rise*rise);float angle=Mathf.Atan2(rise,d*.5f)*Mathf.Rad2Deg;
        for(int side=-1;side<=1;side+=2) {
            var plane=Box("Plano do telhado",p+new Vector3(0,rise*.5f,side*d*.25f),new Vector3(w,.15f,len),roof,parent,false);
            plane.transform.localRotation=Quaternion.Euler(side*angle,0,0);
            int cols=Mathf.CeilToInt(w/.48f),rows=Mathf.CeilToInt(len/.46f);
            for(int r=0;r<rows;r++) for(int c=0;c<cols;c++) {
                float t=(r+.45f)/rows;
                var tile=Box("Telha",p+new Vector3(-w/2+(c+.5f)*w/cols,rise*(1-t)+.1f,side*d*.5f*t),new Vector3(w/cols-.025f,.065f,len/rows+.07f),c%5==0?Mat("Telha clara",new Color(.58f,.25f,.13f),.2f,0,2):roof,parent,false);
                tile.transform.localRotation=Quaternion.Euler(side*angle,0,0);
            }
        }
        Beam("Cumeeira",p+new Vector3(-w*.5f,rise+.10f,0),p+new Vector3(w*.5f,rise+.10f,0),.2f,roof,parent);
        // Close the triangular gable ends.
        var m=new Mesh();m.vertices=new[]{new Vector3(-w/2,0,-d/2),new Vector3(-w/2,rise,0),new Vector3(-w/2,0,d/2),new Vector3(w/2,0,-d/2),new Vector3(w/2,rise,0),new Vector3(w/2,0,d/2)};m.triangles=new[]{0,1,2,5,4,3};m.RecalculateNormals();
        var g=new GameObject("Empenas");g.transform.SetParent(parent,false);g.transform.localPosition=p;g.AddComponent<MeshFilter>().sharedMesh=m;g.AddComponent<MeshRenderer>().sharedMaterial=plaster;
    }
    static void Sign(string text,Vector3 p,float width) {
        Box("Placa",p,new Vector3(width,.62f,.13f),darkWood,null,false);
        var g=new GameObject(text);g.transform.SetParent(root);g.transform.position=p+new Vector3(0,0,-.08f);g.transform.rotation=Quaternion.Euler(0,180,0);
        // TextMesh front faces local -Z, so no Y rotation is required for the front quay.
        g.transform.rotation=Quaternion.identity;
        var t=g.AddComponent<TextMesh>();t.text=text;t.fontSize=48;t.characterSize=.075f;t.anchor=TextAnchor.MiddleCenter;t.color=new Color(.95f,.83f,.58f);
    }
    static void Lantern(Vector3 p) {
        Cylinder("Poste de luz",new Vector3(p.x,(p.y+1.4f)/2,p.z),.065f,p.y-1.4f,darkWood);
        Box("Base da lanterna",p,new Vector3(.38f,.10f,.35f),iron,null,false);
        Box("Vidro da lanterna",p+Vector3.up*.27f,new Vector3(.25f,.43f,.25f),glass,null,false);
        Box("Tampa da lanterna",p+Vector3.up*.52f,new Vector3(.42f,.1f,.42f),iron,null,false);
        var l=new GameObject("Luz quente").AddComponent<Light>();l.transform.SetParent(root);l.transform.position=p+Vector3.up*.25f;l.type=LightType.Point;l.range=5.2f;l.intensity=2.2f;l.color=new Color(1,.56f,.22f);l.shadows=LightShadows.None;
    }
    public static void Crate(Vector3 p,float s,Transform parent=null) {
        Box("Caixote",p+Vector3.up*s*.4f,new Vector3(s,s*.8f,s),wood,parent);
        for(int j=0;j<4;j++) Box("Ripa",p+new Vector3(0,.10f+j*s*.19f,-s*.51f),new Vector3(s+.05f,.055f,.04f),darkWood,parent,false);
        for(int j=-1;j<=1;j+=2) Box("Canto",p+new Vector3(j*s*.43f,s*.4f,-s*.53f),new Vector3(.08f,s*.82f,.06f),darkWood,parent,false);
    }
    static void Barrel(Vector3 p) {
        Cylinder("Barril",p+Vector3.up*.5f,.38f,1,wood);
        for(int i=0;i<3;i++) Cylinder("Aro",p+Vector3.up*(.12f+i*.37f),.40f,.055f,iron);
    }
    public static void CrabPot(Vector3 p,Transform parent) {
        var g=new GameObject("Covo").transform;g.SetParent(parent,false);g.localPosition=p;
        Vector3 sz=new Vector3(1.1f,.48f,.8f);
        for(int i=0;i<6;i++) {
            float x=-sz.x/2+i*sz.x/5;
            Beam("Grade",new Vector3(x,0,-.4f),new Vector3(x,.48f,-.4f),.025f,iron,g);
            Beam("Grade",new Vector3(x,0,.4f),new Vector3(x,.48f,.4f),.025f,iron,g);
            Beam("Grade superior",new Vector3(x,.48f,-.4f),new Vector3(x,.48f,.4f),.022f,iron,g);
        }
        for(int side=-1;side<=1;side+=2)for(int j=0;j<3;j++) Beam("Aro do covo",new Vector3(-.55f,j*.24f,side*.4f),new Vector3(.55f,j*.24f,side*.4f),.045f,wood,g);
        Beam("Travessa",new Vector3(-.55f,0,-.4f),new Vector3(-.55f,0,.4f),.04f,iron,g);
        Beam("Travessa",new Vector3(.55f,.48f,-.4f),new Vector3(.55f,.48f,.4f),.04f,iron,g);
    }
    public static Transform Boat(Vector3 p,float yaw,string name,float scale=1) {
        var b=new GameObject(name).transform;b.SetParent(root);b.position=p;b.rotation=Quaternion.Euler(0,yaw,0);b.localScale=Vector3.one*scale;
        float[] zz={-3.3f,-2.7f,1.8f,3.5f};float[] ww={.75f,1.25f,1.25f,.06f};
        var vs=new List<Vector3>();var ts=new List<int>();
        for(int i=0;i<4;i++) {vs.Add(new Vector3(-ww[i]*.58f,-.2f,zz[i]));vs.Add(new Vector3(-ww[i],.86f,zz[i]));vs.Add(new Vector3(ww[i],.86f,zz[i]));vs.Add(new Vector3(ww[i]*.58f,-.2f,zz[i]));}
        for(int i=0;i<3;i++)for(int j=0;j<4;j++){int a=i*4+j,bb=i*4+(j+1)%4,c=(i+1)*4+j,d=(i+1)*4+(j+1)%4;ts.AddRange(new[]{a,c,bb,bb,c,d});}
        ts.AddRange(new[]{0,1,2,0,2,3,12,14,13,12,15,14});
        var mesh=new Mesh();mesh.name="Casco da Andorinha";mesh.SetVertices(vs);mesh.SetTriangles(ts,0);mesh.RecalculateNormals();
        var hull=new GameObject("Casco");hull.transform.SetParent(b,false);hull.AddComponent<MeshFilter>().sharedMesh=mesh;hull.AddComponent<MeshRenderer>().sharedMaterial=teal;
        Box("Convés",new Vector3(0,.71f,-.45f),new Vector3(2.25f,.12f,5.4f),wood,b);
        for(int i=0;i<3;i++)for(int s=-1;s<=1;s+=2) {
            Vector3 a=new Vector3(s*ww[i],.91f,zz[i]),c=new Vector3(s*ww[i+1],.91f,zz[i+1]);
            Beam("Borda marfim",a,c,.14f,cream,b);
            Beam("Linha do casco",a-Vector3.up*.32f,c-Vector3.up*.32f,.07f,roof,b);
        }
        Box("Cabine",new Vector3(0,1.6f,.4f),new Vector3(1.85f,1.65f,2.0f),cream,b);
        Box("Teto da cabine",new Vector3(0,2.51f,.4f),new Vector3(2.15f,.18f,2.3f),teal,b);
        var wind=Mat("Vidro da cabine",new Color(.12f,.26f,.25f),.85f,.3f);
        Box("Para-brisa",new Vector3(0,1.94f,1.42f),new Vector3(1.55f,.68f,.05f),wind,b,false);
        Box("Moldura central",new Vector3(0,1.94f,1.46f),new Vector3(.08f,.76f,.05f),wood,b,false);
        for(int s=-1;s<=1;s+=2)Box("Janela lateral",new Vector3(s*.94f,1.93f,.6f),new Vector3(.045f,.62f,1.15f),wind,b,false);
        Cylinder("Chaminé",new Vector3(.55f,2.95f,.1f),.12f,1,iron,b);
        Beam("Mastro",new Vector3(-.45f,2.6f,.6f),new Vector3(-.45f,4.3f,.6f),.07f,wood,b);
        Beam("Antena",new Vector3(-1.05f,4,.6f),new Vector3(.45f,4,.6f),.045f,iron,b);
        Beam("Braço do guincho",new Vector3(.8f,1,-2),new Vector3(.8f,3,-2),.14f,iron,b);
        Beam("Braço do guincho",new Vector3(.8f,3,-2),new Vector3(1.7f,3,-2),.13f,iron,b);
        Rope("Cabo do guincho",new Vector3(1.7f,3,-2),new Vector3(1.7f,.8f,-2),0,b,iron,.02f);
        for(int i=0;i<3;i++)CrabPot(new Vector3(-.45f,.8f+i*.49f,-2),b);
        for(int s=-1;s<=1;s+=2)for(int i=0;i<3;i++) {
            float z=-2.6f+i*1.2f;var tire=Cylinder("Defensa",new Vector3(s*1.3f,.65f,z),.29f,.18f,iron,b);
            tire.transform.localRotation=Quaternion.Euler(0,0,90);
            Rope("Cabo da defensa",new Vector3(s*1.2f,1,z),new Vector3(s*1.3f,.65f,z),0,b);
        }
        for(int s=-1;s<=1;s+=2)Ball("Luz de navegação",new Vector3(s*.91f,2.6f,1.2f),Vector3.one*.14f,s==1?glass:roof,b);
        var bob=b.gameObject.AddComponent<BoatFloat>();bob.baseY=p.y;bob.phase=Random.value*6;
        return b;
    }
    public static Transform Person(string name,Vector3 p,Material coat,bool old) {
        var g=new GameObject(name).transform;g.SetParent(root);g.position=p;
        Box("Bota esquerda",new Vector3(-.16f,.12f,.05f),new Vector3(.23f,.24f,.42f),iron,g,false);
        Box("Bota direita",new Vector3(.16f,.12f,.05f),new Vector3(.23f,.24f,.42f),iron,g,false);
        Box("Perna esquerda",new Vector3(-.16f,.43f,0),new Vector3(.22f,.47f,.25f),old?iron:coat,g,false);
        Box("Perna direita",new Vector3(.16f,.43f,0),new Vector3(.22f,.47f,.25f),coat,g,false);
        Shape(PrimitiveType.Capsule,"Casaco",new Vector3(0,.95f,0),new Vector3(.65f,.43f,.39f),coat,g,false);
        Ball("Cabeça",new Vector3(0,1.53f,0),new Vector3(.39f,.43f,.39f),skin,g);
        Box("Nariz",new Vector3(0,1.52f,.20f),new Vector3(.09f,.09f,.09f),skin,g,false);
        Cylinder("Gorro",new Vector3(0,1.75f,0),.215f,.18f,old?cream:teal,g);
        for(int s=-1;s<=1;s+=2) {
            var arm=Shape(PrimitiveType.Capsule,s<0?"Braço esquerdo":"Braço direito",new Vector3(s*.36f,1,0),new Vector3(.20f,.30f,.20f),coat,g,false);
            arm.transform.localRotation=Quaternion.Euler(0,0,s*8);
            Ball("Mão",new Vector3(s*.39f,.71f,0),Vector3.one*.17f,skin,g);
        }
        if(name=="Você") {
            for(int s=-1;s<=1;s+=2)Box("Alça do impermeável",new Vector3(s*.17f,1.18f,.195f),new Vector3(.07f,.43f,.035f),darkWood,g,false);
        }
        return g;
    }
    static void Buoy(Vector3 p,int index) {
        var g=new GameObject("Boia "+(index+1)).transform;g.SetParent(root);g.position=p;
        Ball("Flutuador",new Vector3(0,.15f,0),new Vector3(.65f,.45f,.65f),roof,g);
        Cylinder("Faixa branca",new Vector3(0,.2f,0),.32f,.15f,cream,g);
        Beam("Haste",new Vector3(0,.2f,0),new Vector3(0,1.5f,0),.055f,wood,g);
        Box("Bandeirola",new Vector3(.22f,1.3f,0),new Vector3(.45f,.3f,.03f),yellow,g,false);
        var f=g.gameObject.AddComponent<BoatFloat>();f.baseY=0;f.phase=index*1.7f;
    }
}
public class BoatFloat:MonoBehaviour {
    public float baseY,phase;
    void Update() {var p=transform.position;p.y=baseY+Mathf.Sin(Time.time*.8f+phase)*.055f;transform.position=p;var e=transform.eulerAngles;transform.rotation=Quaternion.Euler(Mathf.Sin(Time.time*.7f+phase)*.7f,e.y,Mathf.Sin(Time.time*.9f+phase)*.9f);}
}
public class HarborGrade:MonoBehaviour {
    Material material;
    void OnRenderImage(RenderTexture source,RenderTexture destination) {
        if(!material){var s=Resources.Load<Shader>("HarborGrade");if(s)material=new Material(s);}
        bool oldSRGB=GL.sRGBWrite;GL.sRGBWrite=QualitySettings.activeColorSpace==ColorSpace.Linear;
        if(material)Graphics.Blit(source,destination,material);else Graphics.Blit(source,destination);
        GL.sRGBWrite=oldSRGB;
    }
    void OnDestroy(){if(material)DestroyImmediate(material);}
}
}
