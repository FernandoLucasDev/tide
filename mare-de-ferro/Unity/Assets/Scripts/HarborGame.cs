using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MareDeFerro {
[Serializable] public class Catch { public string name; public int value; public bool unusual; public Catch(string n,int v,bool u=false){name=n;value=v;unusual=u;} }
[Serializable] public class HarborSave { public int coins=40, totalCatches, sold; public List<Catch> hold=new List<Catch>(); }
public class HarborGame:MonoBehaviour {
    public HarborSave save=new HarborSave();
    public bool aboard, fishing, started;
    public float tension=.4f,progress,speed;
    public string toast="";
    public Camera view;
    public Transform captain, vessel;
    public CharacterController walker;
    Vector3 lastSafe, cameraVelocity;
    float verticalVelocity, toastUntil, fishClock, danger, cooldown, zoom=1, walkPhase;
    readonly float[] readyAt=new float[3];
    bool paused, inventory, overview;
    string dialog="", dialogName="";
    int activeSpot;
    GUIStyle title, label, small, heading, body, button, tiny;
    Texture2D panel, accent, white;
    bool initialized, stylesReady;
    public string SavePath => Path.Combine(Application.persistentDataPath,Array.IndexOf(Environment.GetCommandLineArgs(),"--smoke-test")>=0?"porto-test-save.json":"porto-save-v1.json");
    void Awake(){Initialize();}
    public void Initialize() {
        if(initialized)return; initialized=true;
        Application.targetFrameRate=60;
        HarborWorld.Create(); captain=HarborWorld.player;vessel=HarborWorld.boat;view=HarborWorld.camera;
        walker=captain.gameObject.AddComponent<CharacterController>();walker.height=1.82f;walker.radius=.27f;walker.center=new Vector3(0,.92f,0);walker.stepOffset=.32f;walker.slopeLimit=48;
        lastSafe=HarborWorld.DockSpawn;
        gameObject.AddComponent<HarborAmbience>();
        Load();
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"--smoke-test")>=0)StartCoroutine(SmokeTest());
    }
    void Update() {
        if(!Application.isPlaying)return;
        if(Input.GetKeyDown(KeyCode.F8)) Capture();
        if(Input.GetKeyDown(KeyCode.Escape)) {if(dialog!="")dialog="";else if(inventory)inventory=false;else if(started)paused=!paused;}
        if(!started||paused)return;
        if(Input.GetKeyDown(KeyCode.Tab))inventory=!inventory;
        if(Input.GetKeyDown(KeyCode.C))overview=!overview;
        zoom=Mathf.Clamp(zoom-Input.mouseScrollDelta.y*.07f,.65f,1.45f);
        if(dialog!="") {if(Input.GetKeyDown(KeyCode.E)||Input.GetKeyDown(KeyCode.Return))dialog="";return;}
        if(inventory)return;
        if(fishing){TickFishing(Time.deltaTime,Input.GetKey(KeyCode.Space));if(Input.GetKeyDown(KeyCode.Q))CancelFishing();return;}
        if(aboard)Sail();else Walk();
        if(Input.GetKeyDown(KeyCode.E))Interact();
    }
    void LateUpdate() {
        if(!view||!started)return;
        Vector3 focus=aboard?vessel.position:captain.position;
        Vector3 offset=overview?new Vector3(29,27,-36):new Vector3(10,12.5f,-14)*zoom;
        Vector3 target=overview?new Vector3(-1,2.5f,7):focus+Vector3.up*.7f;
        view.transform.position=Vector3.SmoothDamp(view.transform.position,target+offset,ref cameraVelocity,.55f);
        view.transform.rotation=Quaternion.Slerp(view.transform.rotation,Quaternion.LookRotation(target-view.transform.position),Time.deltaTime*5);
    }
    public void StartJourney() {
        started=true;overview=false;
        ShowDialog("MARA",save.totalCatches==0?"Hoje você pesca para comer. Amanhã a gente vê.\n\nA Andorinha está no fim do píer. Devagar no motor: deixei três covos nas boias amarelas da enseada. Recolha uma captura e leve ao balcão do Tomé.":"Bom te ver de volta. A Andorinha está pronta, e tem peixe na enseada.");
    }
    void Walk() {
        float h=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);
        float v=(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0);
        Vector3 forward=Vector3.ProjectOnPlane(view.transform.forward,Vector3.up).normalized,right=Vector3.ProjectOnPlane(view.transform.right,Vector3.up).normalized;
        Vector3 move=Vector3.ClampMagnitude(forward*v+right*h,1);float pace=Input.GetKey(KeyCode.LeftShift)?4.2f:2.7f;
        if(walker.isGrounded){verticalVelocity=-2;lastSafe=captain.position;}else verticalVelocity-=16*Time.deltaTime;
        walker.Move((move*pace+Vector3.up*verticalVelocity)*Time.deltaTime);
        if(move.sqrMagnitude>.01f)captain.rotation=Quaternion.Slerp(captain.rotation,Quaternion.LookRotation(move),Time.deltaTime*12);
        walkPhase+=Time.deltaTime*pace*4*move.magnitude;
        for(int i=0;i<captain.childCount;i++){var part=captain.GetChild(i);if(part.name.StartsWith("Braço"))part.localRotation=Quaternion.Euler(Mathf.Sin(walkPhase)*(part.name.EndsWith("esquerdo")?1:-1)*22*move.magnitude,0,part.localPosition.x<0?-8:8);}
        if(captain.position.y<.35f){walker.enabled=false;captain.position=lastSafe.y>1?lastSafe:HarborWorld.DockSpawn;walker.enabled=true;verticalVelocity=0;Notify("Cuidado com a borda do cais.");}
    }
    void Sail() {
        float throttle=(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0);
        float rudder=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);
        speed=Mathf.MoveTowards(speed,throttle*(throttle<0?2:6),Time.deltaTime*(throttle==0?2:1.6f));
        vessel.Rotate(0,rudder*Time.deltaTime*40*Mathf.Clamp01(Mathf.Abs(speed)+.2f)*(speed<0?-1:1),0,Space.World);
        Vector3 next=vessel.position+vessel.forward*speed*Time.deltaTime;
        bool coast=next.z>2.2f||Vector2.Distance(new Vector2(next.x,next.z),new Vector2(37,24))<11;
        bool pier=next.x>-7.4f&&next.x< -2.35f&&next.z>-19;
        bool boats=Vector2.Distance(new Vector2(next.x,next.z),new Vector2(-14,-2))<4||Vector2.Distance(new Vector2(next.x,next.z),new Vector2(16,-5))<4;
        if(coast||pier||boats){speed=-speed*.2f;Notify("Água rasa. Afaste o barco do cais.");return;}
        if(next.x< -65||next.x>65||next.z< -75){speed=0;Notify("Mar aberto — disponível numa próxima etapa.");return;}
        vessel.position=next;
    }
    public bool TryBoard() {
        if(aboard||Vector3.Distance(captain.position,vessel.position)>6.5f)return false;
        aboard=true;walker.enabled=false;captain.gameObject.SetActive(false);speed=0;Notify("Andorinha • W/S: motor · A/D: leme");return true;
    }
    public bool TryDock() {
        if(!aboard||Vector2.Distance(new Vector2(vessel.position.x,vessel.position.z),new Vector2(.6f,-10))>6||Mathf.Abs(speed)>1.2f)return false;
        aboard=false;speed=0;captain.position=new Vector3(-4.5f,1.6f,-8.8f);captain.gameObject.SetActive(true);walker.enabled=true;lastSafe=captain.position;Notify("De volta ao porto. Tomé compra a captura no mercado.");return true;
    }
    void Interact() {
        if(aboard) {
            int nearest=NearestSpot();if(nearest>=0){BeginFishing(nearest);return;}
            if(!TryDock())Notify("Aproxime-se de uma boia ou volte devagar ao embarque.");
            return;
        }
        if(Vector3.Distance(captain.position,HarborWorld.mara.position)<2.8f){ShowDialog("MARA",save.totalCatches==0?"Não brigue com o guincho. Segure espaço para puxar, solte para aliviar. Se o cabo cantar, já passou da hora de tirar a mão.\n\nE volte. Peixe nenhum paga um barco que não volta.":"Esse porto já viu muito capitão bom. Os melhores sempre sabiam a hora de voltar.\n\nEstá ouvindo? Hoje o mar está deixando a gente trabalhar.");return;}
        if(Vector3.Distance(captain.position,new Vector3(1,1.55f,6.8f))<3.3f){Sell();return;}
        if(Vector3.Distance(captain.position,new Vector3(-15,1.55f,9.1f))<2.6f){ShowDialog("NINA","Motor pequeno também merece respeito. Dei um jeito no da Andorinha.\n\nAinda vou desenhar um barco inteiro. Um daqueles que conseguem atravessar as Agulhas. Por enquanto, tenho esse guincho pra consertar.");return;}
        if(Vector3.Distance(captain.position,new Vector3(8,1.55f,7))<2.6f){ShowDialog("IVO","Três covos? É um começo.\n\nSe encontrar uma panela no seu, pode ficar com ela. Se encontrar um caranguejo dentro da panela… também pode ficar. Eu não encosto naquilo.");return;}
        if(!TryBoard())Notify("Explore o cais. Mara está perto da entrada do píer.");
    }
    public int NearestSpot() {
        if(!aboard)return -1;
        for(int i=0;i<3;i++) if(Vector3.Distance(vessel.position,HarborWorld.FishingSpots[i])<4.8f)return i;
        return -1;
    }
    public bool BeginFishing(int index) {
        if(!aboard||fishing||index<0||index>=3||Vector3.Distance(vessel.position,HarborWorld.FishingSpots[index])>=4.8f)return false;
        if(Time.time<readyAt[index]){Notify("Esse covo está de molho. Volte em "+Mathf.CeilToInt(readyAt[index]-Time.time)+" s.");return false;}
        if(Mathf.Abs(speed)>1.2f){Notify("Reduza o motor antes de recolher o covo.");return false;}
        fishing=true;activeSpot=index;speed=0;tension=.43f;progress=0;danger=0;fishClock=0;return true;
    }
    public void TickFishing(float dt,bool pull) {
        if(!fishing)return;fishClock+=dt;
        tension=Mathf.Clamp01(tension+(pull?.32f:-.27f)*dt+Mathf.Sin(fishClock*2.8f+activeSpot)*dt*.10f);
        bool band=tension>=.28f&&tension<=.76f;
        if(band&&pull)progress+=dt*.14f;
        if(tension>.93f)danger+=dt;else danger=Mathf.Max(0,danger-dt*.7f);
        if(danger>1.6f){fishing=false;readyAt[activeSpot]=Time.time+12;Notify("O cabo escapou. O covo ficou no fundo; tente novamente em instantes.");return;}
        if(progress>=1)FinishCatch();
    }
    public void CancelFishing(){fishing=false;Notify("Guincho parado. O covo continua na água.");}
    public void FinishCatch() {
        fishing=false;progress=1;readyAt[activeSpot]=Time.time+35;
        Catch c;
        int n=save.totalCatches%4;
        if(n==2)c=new Catch("Caranguejo-de-panela",58,true);
        else if(n==3)c=new Catch("Ermitão de porcelana",42,true);
        else c=new Catch(n==0?"Caranguejo da enseada":"Siri de pedra",n==0?24:31);
        save.hold.Add(c);save.totalCatches++;Store();
        ShowDialog(c.unusual?"UMA SURPRESA NO COVO":"CAPTURA RECOLHIDA",c.name+"\n\n"+(c.unusual?(n==2?"Uma panela começa a andar pelo convés. Debaixo dela, duas pinças defendem uma casa que já teve dias melhores.":"Uma pequena pinça aparece pela alça de uma xícara azul. Alguém no porto talvez reconheça a louça."):"O primeiro peso do mar chegou ao convés. Tomé pode comprar sua captura no mercado.")+"\n\nValor no mercado: "+c.value+" moedas.");
    }
    public int Sell() {
        int count=save.hold.Count;if(count==0){ShowDialog("TOMÉ","Traga o que a enseada der. Até peixe que o mercado rejeita tem uma boa receita esperando.\n\nAs boias amarelas marcam os covos da Mara.");return 0;}
        int sum=0;foreach(var c in save.hold)sum+=c.value;
        save.coins+=sum;save.sold+=count;save.hold.Clear();Store();
        ShowDialog("TOMÉ","Bom trabalho. "+count+" captura(s), "+sum+" moedas.\n\n"+(save.sold<=3?"Já dá para uma refeição quente. Mara vai fingir que não está orgulhosa.\n\nVocê completou sua primeira viagem. Pode continuar explorando a enseada e encontrando as criaturas dos covos.":"Pode deixar no balcão. Tem gente esperando o jantar, e você ainda tem mar pela frente."));return sum;
    }
    void ShowDialog(string name,string text){dialogName=name;dialog=text;}
    public void Notify(string s){if(toast==s&&Time.time<toastUntil)return;toast=s;toastUntil=Time.time+4;}
    public void Store() {
        try {Directory.CreateDirectory(Application.persistentDataPath);string tmp=SavePath+".tmp";File.WriteAllText(tmp,JsonUtility.ToJson(save,true));File.Copy(tmp,SavePath,true);File.Delete(tmp);}
        catch(Exception e){Debug.LogWarning("Não foi possível salvar: "+e.Message);Notify("Não foi possível salvar o progresso neste computador.");}
    }
    void Load() {
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"--smoke-test")>=0)return;
        try{if(File.Exists(SavePath)){var loaded=JsonUtility.FromJson<HarborSave>(File.ReadAllText(SavePath));if(loaded!=null){save=loaded;if(save.hold==null)save.hold=new List<Catch>();}}}catch(Exception e){Debug.LogWarning("Save indisponível: "+e.Message);}
    }
    void Capture() {
        string folder=Path.Combine(Application.persistentDataPath,"Fotos");Directory.CreateDirectory(folder);string file=Path.Combine(folder,"porto-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".png");
        RenderPhoto(view,file,1920,1080);Notify("Foto salva em "+folder);
    }
    public static void RenderPhoto(Camera cam,string path,int width,int height) {
        var rt=new RenderTexture(width,height,24);var previous=RenderTexture.active;var target=cam.targetTexture;
        cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
        File.WriteAllBytes(path,image.EncodeToPNG());cam.targetTexture=target;RenderTexture.active=previous;DestroyImmediate(image);rt.Release();DestroyImmediate(rt);
    }
    void EnsureStyles() {
        if(stylesReady)return;stylesReady=true;
        panel=Solid(new Color(.045f,.105f,.115f,.96f));accent=Solid(new Color(.87f,.60f,.27f));white=Solid(Color.white);
        Font font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label=new GUIStyle{font=font,fontSize=19,normal={textColor=new Color(.95f,.90f,.77f)}};
        small=new GUIStyle(label){fontSize=15};tiny=new GUIStyle(label){fontSize=12};
        heading=new GUIStyle(label){fontSize=24,fontStyle=FontStyle.Bold};
        title=new GUIStyle(label){fontSize=71,fontStyle=FontStyle.Bold};
        body=new GUIStyle(label){fontSize=19,wordWrap=true};
        button=new GUIStyle(GUI.skin.button){font=font,fontSize=17,padding=new RectOffset(18,18,12,12),normal={background=Solid(new Color(.16f,.28f,.27f)),textColor=new Color(.97f,.90f,.72f)},hover={background=Solid(new Color(.27f,.39f,.34f)),textColor=Color.white},active={background=accent,textColor=new Color(.05f,.1f,.1f)}};
    }
    static Texture2D Solid(Color c){var t=new Texture2D(1,1);t.SetPixel(0,0,c);t.Apply();return t;}
    void Panel(Rect r){GUI.DrawTexture(r,panel);GUI.DrawTexture(new Rect(r.x,r.y,3,r.height),accent);}
    void OnGUI() {
        EnsureStyles();float scale=Mathf.Min(Screen.width/1440f,Screen.height/900f);GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one*scale);
        float w=Screen.width/scale,h=Screen.height/scale;
        if(!started){
            Panel(new Rect(55,h*.22f,450,465));
            GUI.Label(new Rect(85,h*.22f+28,380,25),"UM PORTO. UMA VIDA. UM MAR INTEIRO.",tiny);
            GUI.Label(new Rect(81,h*.22f+70,410,180),"MARÉ\nDE FERRO",title);
            GUI.Label(new Rect(85,h*.22f+258,365,60),"Você chegou com a maré.\nAgora encontre seu lugar no cais.",body);
            if(GUI.Button(new Rect(85,h*.22f+350,300,49),"INICIAR VIAGEM  →",button))StartJourney();
            GUI.Label(new Rect(85,h*.22f+417,365,22),"PORTO DAS VELAS  /  PROTÓTIPO 0.1",tiny);
            GUI.Label(new Rect(w-370,h-43,335,26),"Arte e exploração • uma primeira enseada",small);return;
        }
        Panel(new Rect(28,25,340,96));GUI.Label(new Rect(48,39,300,30),"PORTO DAS VELAS",heading);
        GUI.Label(new Rect(48,79,300,25),aboard?"ENSEADA  /  ANDORINHA":"FIM DE TARDE  /  NO CAIS",small);
        Panel(new Rect(w-255,25,226,96));GUI.Label(new Rect(w-233,39,200,28),save.coins+"  moedas",heading);GUI.Label(new Rect(w-233,79,200,25),save.hold.Count+" captura(s) no porão",small);
        GUI.Label(new Rect(30,h-36,w-60,25),aboard?"W/S  motor    A/D  leme    E  interagir    C  vista do porto    TAB  porão    ESC  pausa":"WASD / setas  caminhar    SHIFT  correr    E  interagir    C  vista do porto    RODA  zoom    ESC  pausa",small);
        if(!fishing&&dialog==""&&!paused&&!inventory) {
            string prompt=Prompt();if(prompt!=""){Panel(new Rect(w/2-230,h-111,460,53));GUI.Label(new Rect(w/2-208,h-96,425,29),prompt,label);}
            if(aboard){GUI.Label(new Rect(w-230,140,210,27),Mathf.Abs(speed).ToString("0.0")+" • motor",label);DrawCompass(w,h);DrawChart(w);}
            if(save.sold==0){Panel(new Rect(28,140,310,92));GUI.Label(new Rect(48,152,275,22),"A PRIMEIRA VIAGEM",small);GUI.Label(new Rect(48,183,270,44),save.hold.Count>0?"Volte e venda a captura ao Tomé.":"Embarque e recolha um covo\nnas boias amarelas.",small);}
        }
        if(Time.time<toastUntil&&dialog==""){Panel(new Rect(w/2-355,140,710,56));GUI.Label(new Rect(w/2-335,156,670,35),toast,small);}
        if(fishing)FishingUI(w,h);
        if(dialog!="") {
            float bh=286;Panel(new Rect(w/2-410,h-bh-65,820,bh));
            GUI.Label(new Rect(w/2-380,h-bh-43,740,32),dialogName,heading);
            GUI.Label(new Rect(w/2-380,h-bh+5,740,174),dialog,body);
            if(GUI.Button(new Rect(w/2+202,h-126,175,43),"CONTINUAR  [E]",button))dialog="";
        }
        if(inventory){Panel(new Rect(w/2-290,180,580,460));GUI.Label(new Rect(w/2-260,205,520,40),"DIÁRIO DE BORDO",heading);GUI.Label(new Rect(w/2-260,255,520,30),"Capturas: "+save.totalCatches+"  •  Vendidas: "+save.sold,small);if(save.hold.Count==0)GUI.Label(new Rect(w/2-260,310,520,45),"Porão vazio. Há três boias na enseada.",body);for(int i=0;i<save.hold.Count&&i<8;i++)GUI.Label(new Rect(w/2-260,307+i*31,520,30),save.hold[i].name+"  ·  "+save.hold[i].value+" moedas",small);GUI.Label(new Rect(w/2-260,593,520,30),"TAB para fechar  •  F8 para fotografar",small);}
        if(paused){Panel(new Rect(w/2-230,h/2-160,460,320));GUI.Label(new Rect(w/2-200,h/2-125,400,40),"UM RESPIRO NO CAIS",heading);if(GUI.Button(new Rect(w/2-200,h/2-55,400,48),"VOLTAR AO MAR",button))paused=false;if(GUI.Button(new Rect(w/2-200,h/2+8,400,48),"SALVAR PROGRESSO",button)){Store();Notify("Progresso salvo.");}if(GUI.Button(new Rect(w/2-200,h/2+71,400,48),"SAIR",button)){Store();Application.Quit();}}
    }
    string Prompt() {
        if(aboard){int i=NearestSpot();if(i>=0)return Time.time<readyAt[i]?"Covo de molho • "+Mathf.CeilToInt(readyAt[i]-Time.time)+" s":"[E]  Recolher covo • reduza o motor";if(Vector2.Distance(new Vector2(vessel.position.x,vessel.position.z),new Vector2(.6f,-10))<6)return "[E]  Desembarcar • motor devagar";return "Procure as bandeirolas amarelas da enseada";}
        Vector3 p=captain.position;if(Vector3.Distance(p,HarborWorld.mara.position)<2.8f)return "[E]  Conversar com Mara";
        if(Vector3.Distance(p,new Vector3(1,1.55f,6.8f))<3.3f)return "[E]  Mercado • vender captura";
        if(Vector3.Distance(p,new Vector3(-15,1.55f,9.1f))<2.6f)return "[E]  Conversar com Nina";
        if(Vector3.Distance(p,new Vector3(8,1.55f,7))<2.6f)return "[E]  Conversar com Ivo";
        if(Vector3.Distance(p,vessel.position)<6.5f)return "[E]  Embarcar na Andorinha";return "";
    }
    void FishingUI(float w,float h) {
        Panel(new Rect(w/2-320,h-260,640,195));GUI.Label(new Rect(w/2-290,h-239,580,35),"O PESO DO FUNDO",heading);
        GUI.Label(new Rect(w/2-290,h-196,580,27),"Segure ESPAÇO para puxar. Solte para aliviar.",small);
        Rect bar=new Rect(w/2-290,h-151,580,19);GUI.color=new Color(.22f,.28f,.27f);GUI.DrawTexture(bar,white);GUI.color=new Color(.33f,.57f,.43f);GUI.DrawTexture(new Rect(bar.x+580*.28f,bar.y,580*.48f,bar.height),white);
        GUI.color=tension>.88f?new Color(.95f,.30f,.22f):new Color(1,.78f,.35f);GUI.DrawTexture(new Rect(bar.x+Mathf.Clamp01(tension)*573,bar.y-5,7,29),white);GUI.color=Color.white;
        GUI.Label(new Rect(bar.x,h-119,580,25),"COVO  "+Mathf.RoundToInt(progress*100)+"%"+(danger>.5f?"  •  ALIVIE O CABO!":"  •  mantenha a tensão na faixa verde")+"     Q cancelar",small);
    }
    void DrawCompass(float w,float h) {
        // Screen-space markers stay attached to the physical buoys, making them easy to find.
        for(int i=0;i<3;i++) {
            Vector3 pos=view.WorldToScreenPoint(HarborWorld.FishingSpots[i]+Vector3.up*2);
            if(pos.z<=0)continue;float scale=Mathf.Min(Screen.width/1440f,Screen.height/900f);float x=pos.x/scale,y=(Screen.height-pos.y)/scale;
            if(x<20||x>w-140||y<130||y>h-160)continue;
            GUI.Label(new Rect(x-37,y,160,23),"COVO "+(i+1),small);
        }
    }
    void DrawChart(float w) {
        Rect map=new Rect(w-228,185,198,188);Panel(map);
        GUI.Label(new Rect(map.x+15,map.y+10,175,25),"CARTA DA ENSEADA",tiny);
        Vector2 origin=new Vector2(map.x+99,map.y+49);float s=2.1f;
        GUI.color=new Color(.56f,.67f,.55f);GUI.DrawTexture(new Rect(map.x+16,map.y+36,166,8),white);GUI.color=Color.white;
        for(int i=0;i<3;i++) {
            Vector3 p=HarborWorld.FishingSpots[i];float x=origin.x+p.x*s,y=origin.y-p.z*s;
            GUI.color=Time.time<readyAt[i]?new Color(.45f,.52f,.48f):new Color(.97f,.71f,.30f);GUI.DrawTexture(new Rect(x-3,y-3,6,6),white);GUI.color=Color.white;
            GUI.Label(new Rect(x+6,y-8,30,20),(i+1).ToString(),tiny);
        }
        Vector3 b=vessel.position;float bx=origin.x+b.x*s,by=origin.y-b.z*s;
        bx=Mathf.Clamp(bx,map.x+10,map.xMax-13);by=Mathf.Clamp(by,map.y+40,map.yMax-20);
        GUI.color=new Color(.65f,.88f,.85f);GUI.DrawTexture(new Rect(bx-4,by-4,8,8),white);GUI.color=Color.white;
        GUI.Label(new Rect(map.x+14,map.yMax-23,170,20),"N ↑   •   amarelo = covo",tiny);
    }
    IEnumerator SmokeTest() {
        yield return null;started=true;dialog="";
        walker.enabled=false;captain.position=new Vector3(-4.5f,1.6f,-9);walker.enabled=true;
        if(!TryBoard())throw new Exception("QA: embarque falhou");
        if(BeginFishing(0))throw new Exception("QA: covo acessível à distância");
        vessel.position=HarborWorld.FishingSpots[0];speed=0;
        speed=3;if(BeginFishing(0))throw new Exception("QA: guincho permitido com barco rápido");speed=0;
        if(!BeginFishing(0))throw new Exception("QA: início do guincho falhou");
        for(int i=0;i<1600&&fishing;i++)TickFishing(.02f,tension<.64f);
        if(save.hold.Count!=1||fishing)throw new Exception("QA: captura falhou");
        if(BeginFishing(0))throw new Exception("QA: espera do covo ignorada");
        dialog="";vessel.position=new Vector3(.6f,.1f,-10);speed=0;
        if(!TryDock())throw new Exception("QA: desembarque falhou");
        if(Sell()!=24||save.coins!=64||save.hold.Count!=0)throw new Exception("QA: venda falhou");
        string roundtrip=JsonUtility.ToJson(save);var restored=JsonUtility.FromJson<HarborSave>(roundtrip);
        if(restored.coins!=64||restored.sold!=1)throw new Exception("QA: persistência falhou");
        Physics.SyncTransforms();
        if(!Physics.Raycast(new Vector3(-4.5f,4,-5),Vector3.down,4))throw new Exception("QA: píer sem piso");
        if(!Physics.Raycast(new Vector3(1,4,6),Vector3.down,4))throw new Exception("QA: mercado sem piso");
        dialog="";overview=true;
        yield return new WaitForSeconds(2);
        string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","..","..","verificacao"));Directory.CreateDirectory(dir);
        RenderPhoto(view,Path.Combine(dir,"porto-runtime.png"),1600,1000);
        ScreenCapture.CaptureScreenshot(Path.Combine(dir,"porto-interface.png"));
        yield return new WaitForSeconds(.7f);
        File.WriteAllText(Path.Combine(dir,"runtime-test.txt"),"PASS: world initialized; boarding; fishing distance and speed restrictions; fishing minigame; catch; pot cooldown; docking; sale; save roundtrip; pier and market floor colliders; runtime and HUD screenshots.\n"+DateTime.UtcNow.ToString("O"));
        Debug.Log("MARE_QA_PASS");Application.Quit(0);
    }
}
}
