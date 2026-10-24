using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MareDeFerro {
[Serializable] public class Catch { public string name; public int value; public bool unusual; public Catch(string n,int v,bool u=false){name=n;value=v;unusual=u;} }
[Serializable] public class HarborSave { public int version=2, coins=40, totalCatches, sold; public List<Catch> hold=new List<Catch>(); public float[] potRemaining=new float[3]; }
public partial class HarborGame:MonoBehaviour {
    public const int HoldCapacity=8;
    public HarborSave save=new HarborSave();
    public bool aboard, fishing, started;
    public float tension=.4f,progress,speed;
    public string toast="";
    public Camera view;
    public Transform captain, vessel;
    public CharacterController walker;
    Vector3 lastSafe, cameraVelocity;
    float verticalVelocity, toastUntil, fishClock, danger, zoom=1, walkPhase;
    Vector2 inventoryScroll;
    readonly float[] readyAt=new float[3];
    bool paused, inventory, overview;
    string dialog="", dialogName="";
    int activeSpot;
    GUIStyle title, label, small, heading, body, button, tiny;
    Texture2D panel, accent, white;
    bool initialized, stylesReady;
    bool testing => Array.IndexOf(Environment.GetCommandLineArgs(),"--smoke-test")>=0;
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
        if(testing)StartCoroutine(RunValidation());
    }
    void Update() {
        if(!Application.isPlaying)return;
        if(testing)return;
        if(Input.GetKeyDown(KeyCode.F8)) Capture();
        if(Input.GetKeyDown(KeyCode.Escape)) {if(dialog!="")CloseDialog();else if(inventory)SetInventory(false);else if(started)SetPaused(!paused);}
        if(!started||paused)return;
        if(Input.GetKeyDown(KeyCode.Tab)&&dialog=="")SetInventory(!inventory);
        if(Input.GetKeyDown(KeyCode.C))overview=!overview;
        zoom=Mathf.Clamp(zoom-Input.mouseScrollDelta.y*.07f,.65f,1.45f);
        if(dialog!="") {if(Input.GetKeyDown(KeyCode.E)||Input.GetKeyDown(KeyCode.Return))CloseDialog();return;}
        if(inventory)return;
        if(fishing){TickFishing(Time.deltaTime,Input.GetKey(KeyCode.Space));if(Input.GetKeyDown(KeyCode.Q))CancelFishing();return;}
        if(aboard)Sail();else Walk();
        if(Input.GetKeyDown(KeyCode.E))Interact();
    }
    void LateUpdate() {
        if(!view||!started)return;
        Vector3 focus=aboard?vessel.position:captain.position;
        Vector3 offset=overview?(aboard?new Vector3(15,22,-24)*zoom:new Vector3(29,27,-36)):new Vector3(10,12.5f,-14)*zoom;
        Vector3 target=overview&&!aboard?new Vector3(-1,2.5f,7):focus+Vector3.up*.7f;
        if(aboard)target+=Vector3.ProjectOnPlane(vessel.forward,Vector3.up)*speed*.55f;
        view.transform.position=Vector3.SmoothDamp(view.transform.position,target+offset,ref cameraVelocity,.55f,Mathf.Infinity,Time.unscaledDeltaTime);
        view.transform.rotation=Quaternion.Slerp(view.transform.rotation,Quaternion.LookRotation(target-view.transform.position),Time.unscaledDeltaTime*5);
    }
    public void StartJourney() {
        started=true;overview=false;
        ShowDialog("MARA",save.totalCatches==0?"Hoje você pesca para comer. Amanhã a gente vê.\n\nA Andorinha está no fim do píer. Devagar no motor: deixei três covos nas boias amarelas da enseada. Recolha uma captura e leve ao balcão do Tomé.":"Bom te ver de volta. A Andorinha está pronta, e tem peixe na enseada.");
    }
    void Walk() {
        float h=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);
        float v=(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0);
        Vector3 forward=Vector3.ProjectOnPlane(view.transform.forward,Vector3.up).normalized,right=Vector3.ProjectOnPlane(view.transform.right,Vector3.up).normalized;
        WalkStep(forward*v+right*h,Input.GetKey(KeyCode.LeftShift),Time.deltaTime);
    }
    public void WalkStep(Vector3 direction,bool run,float dt) {
        if(aboard||paused||inventory||dialog!=""||dt<=0)return;
        Vector3 move=Vector3.ClampMagnitude(Vector3.ProjectOnPlane(direction,Vector3.up),1);float pace=run?4.2f:2.7f;
        if(walker.isGrounded){verticalVelocity=-2;lastSafe=captain.position;}else verticalVelocity-=16*dt;
        walker.Move((move*pace+Vector3.up*verticalVelocity)*dt);
        if(move.sqrMagnitude>.01f)captain.rotation=Quaternion.Slerp(captain.rotation,Quaternion.LookRotation(move),dt*12);
        walkPhase+=dt*pace*4*move.magnitude;
        for(int i=0;i<captain.childCount;i++){var part=captain.GetChild(i);if(part.name.StartsWith("Braço"))part.localRotation=Quaternion.Euler(Mathf.Sin(walkPhase)*(part.name.EndsWith("esquerdo")?1:-1)*22*move.magnitude,0,part.localPosition.x<0?-8:8);}
        if(captain.position.y<.35f){walker.enabled=false;captain.position=lastSafe.y>1?lastSafe:HarborWorld.DockSpawn;walker.enabled=true;verticalVelocity=0;Notify("Cuidado com a borda do cais.");}
    }
    void Sail() {
        float throttle=(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0);
        float rudder=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);
        SailStep(throttle,rudder,Input.GetKey(KeyCode.Space),Time.deltaTime);
    }
    public void SailStep(float throttle,float rudder,bool brake,float dt) {
        if(!aboard||fishing||paused||inventory||dialog!=""||dt<=0)return;
        throttle=Mathf.Clamp(throttle,-1,1);rudder=Mathf.Clamp(rudder,-1,1);
        speed=Mathf.MoveTowards(speed,brake?0:throttle*(throttle<0?2:6),dt*(brake?5.5f:throttle==0?2:1.6f));
        float yaw=vessel.eulerAngles.y+rudder*dt*40*Mathf.Clamp01(Mathf.Abs(speed)+.2f)*(speed<0?-1:1);
        Vector3 forward=Quaternion.Euler(0,yaw,0)*Vector3.forward;
        Vector3 next=vessel.position+forward*speed*dt;
        if(!HarborNavigation.CanOccupy(next,yaw)) {
            // Still allow a straight retreat if the attempted turn would put the bow into the pier.
            next=vessel.position+Vector3.ProjectOnPlane(vessel.forward,Vector3.up).normalized*speed*dt;yaw=vessel.eulerAngles.y;
            if(!HarborNavigation.CanOccupy(next,yaw)){speed=0;Notify("Passagem bloqueada. Use ré e o leme para se afastar.");return;}
        }
        vessel.position=next;vessel.rotation=Quaternion.Euler(0,yaw,0);
    }
    public bool TryBoard() {
        if(aboard||paused||inventory||dialog!=""||HarborNavigation.Distance(captain.position,HarborNavigation.Landing)>2.2f)return false;
        aboard=true;walker.enabled=false;captain.gameObject.SetActive(false);speed=0;Notify("Andorinha • W/S: motor · A/D: leme");return true;
    }
    public bool TryDock() {
        if(!aboard||fishing||paused||inventory||dialog!=""||HarborNavigation.Distance(vessel.position,HarborNavigation.Mooring)>5.5f||Mathf.Abs(speed)>1.2f)return false;
        vessel.position=HarborNavigation.Mooring;vessel.rotation=Quaternion.Euler(0,180,0);
        aboard=false;speed=0;verticalVelocity=0;captain.position=HarborNavigation.Landing;captain.gameObject.SetActive(true);walker.enabled=true;lastSafe=captain.position;Notify("Andorinha amarrada. Tomé compra a captura no mercado.");return true;
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
        if(!aboard||fishing||paused||inventory||dialog!=""||index<0||index>=3||Vector3.Distance(vessel.position,HarborWorld.FishingSpots[index])>=4.8f)return false;
        if(save.hold.Count>=HoldCapacity){Notify("Porão cheio. Volte ao mercado para vender a captura.");return false;}
        if(Time.time<readyAt[index]){Notify("Esse covo está de molho. Volte em "+Mathf.CeilToInt(readyAt[index]-Time.time)+" s.");return false;}
        if(Mathf.Abs(speed)>1.2f){Notify("Reduza o motor antes de recolher o covo.");return false;}
        fishing=true;activeSpot=index;speed=0;tension=.43f;progress=0;danger=0;fishClock=0;return true;
    }
    public void TickFishing(float dt,bool pull) {
        if(!fishing||paused||inventory||dialog!=""||dt<=0)return;fishClock+=dt;
        tension=Mathf.Clamp01(tension+(pull?.32f:-.27f)*dt+Mathf.Sin(fishClock*2.8f+activeSpot)*dt*.10f);
        bool band=tension>=.28f&&tension<=.76f;
        if(band&&pull)progress+=dt*.14f;
        if(tension>.93f)danger+=dt;else danger=Mathf.Max(0,danger-dt*.7f);
        if(danger>1.6f){fishing=false;readyAt[activeSpot]=Time.time+12;Store();Notify("O cabo escapou. O covo ficou no fundo; tente novamente em instantes.");return;}
        if(progress>=1)FinishCatch();
    }
    public void CancelFishing(){fishing=false;Notify("Guincho parado. O covo continua na água.");}
    public void FinishCatch() {
        if(!fishing||progress<1||save.hold.Count>=HoldCapacity)return;
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
        if(aboard||paused||inventory||dialog!=""||HarborNavigation.Distance(captain.position,HarborNavigation.Market)>3.3f)return 0;
        int count=save.hold.Count;if(count==0){ShowDialog("TOMÉ","Traga o que a enseada der. Até peixe que o mercado rejeita tem uma boa receita esperando.\n\nAs boias amarelas marcam os covos da Mara.");return 0;}
        int sum=0;foreach(var c in save.hold)sum+=c.value;
        save.coins+=sum;save.sold+=count;save.hold.Clear();Store();
        ShowDialog("TOMÉ","Bom trabalho. "+count+" captura(s), "+sum+" moedas.\n\n"+(save.sold<=3?"Já dá para uma refeição quente. Mara vai fingir que não está orgulhosa.\n\nVocê completou sua primeira viagem. Pode continuar explorando a enseada e encontrando as criaturas dos covos.":"Pode deixar no balcão. Tem gente esperando o jantar, e você ainda tem mar pela frente."));return sum;
    }
    void ShowDialog(string name,string text){dialogName=name;dialog=text;ApplyModalClock();}
    public void CloseDialog(){dialog="";ApplyModalClock();}
    public void SetPaused(bool value){paused=value;ApplyModalClock();}
    void SetInventory(bool value){inventory=value;ApplyModalClock();}
    void ApplyModalClock(){Time.timeScale=started&&(paused||inventory||dialog!="")?0:1;AudioListener.pause=paused;}
    void OnApplicationFocus(bool focused){if(started&&!focused&&!testing)SetPaused(true);}
    void OnApplicationQuit(){if(initialized&&started&&!testing)Store();}
    void OnDestroy(){Time.timeScale=1;AudioListener.pause=false;}
    public void Notify(string s){if(toast==s&&Time.unscaledTime<toastUntil)return;toast=s;toastUntil=Time.unscaledTime+4;}
    public bool Store() {
        try {for(int i=0;i<3;i++)save.potRemaining[i]=Mathf.Max(0,readyAt[i]-Time.time);HarborStorage.Save(SavePath,save);return true;}
        catch(Exception e){Debug.LogWarning("Não foi possível salvar: "+e.Message);Notify("Não foi possível salvar o progresso neste computador.");return false;}
    }
    void Load() {
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"--smoke-test")>=0)return;
        try{save=HarborStorage.Load(SavePath,out bool recovered);for(int i=0;i<3;i++)readyAt[i]=Time.time+save.potRemaining[i];if(recovered)Notify("Progresso recuperado da cópia de segurança.");}catch(Exception e){Debug.LogWarning("Save indisponível: "+e.Message);Notify("Não foi possível ler o progresso salvo.");}
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
            GUI.Label(new Rect(85,h*.22f+417,365,22),"PORTO DAS VELAS  /  PROTÓTIPO 0.2",tiny);
            GUI.Label(new Rect(w-370,h-43,335,26),"Arte e exploração • uma primeira enseada",small);return;
        }
        Panel(new Rect(28,25,340,96));GUI.Label(new Rect(48,39,300,30),"PORTO DAS VELAS",heading);
        GUI.Label(new Rect(48,79,300,25),aboard?"ENSEADA  /  ANDORINHA":"FIM DE TARDE  /  NO CAIS",small);
        Panel(new Rect(w-255,25,226,96));GUI.Label(new Rect(w-233,39,200,28),save.coins+"  moedas",heading);GUI.Label(new Rect(w-233,79,200,25),save.hold.Count+" / "+HoldCapacity+" espaços no porão",small);
        GUI.Label(new Rect(30,h-36,w-60,25),fishing?"ESPAÇO  puxar / soltar para aliviar    Q  cancelar recolhimento    ESC  pausa":aboard?"W/S  motor    A/D  leme    ESPAÇO  frear    E  interagir    C  câmera    TAB  porão    ESC  pausa":"WASD / setas  caminhar    SHIFT  correr    E  interagir    C  vista do porto    RODA  zoom    ESC  pausa",small);
        if(!fishing&&dialog==""&&!paused&&!inventory) {
            string prompt=Prompt();if(prompt!=""){Panel(new Rect(w/2-230,h-111,460,53));GUI.Label(new Rect(w/2-208,h-96,425,29),prompt,label);}
            if(aboard){GUI.Label(new Rect(w-230,140,210,27),Mathf.Abs(speed).ToString("0.0")+" • motor",label);DrawCompass(w,h);DrawChart(w);}
            if(save.sold==0||save.hold.Count>0){Panel(new Rect(28,140,310,92));GUI.Label(new Rect(48,152,275,22),"A PRIMEIRA VIAGEM",small);GUI.Label(new Rect(48,183,270,44),save.hold.Count>0?(aboard?"Volte ao embarque marcado\nna carta e venda ao Tomé.":"Leve a captura ao balcão\nde peixe do Tomé."):(aboard?"Siga até uma boia amarela.\nFreie com ESPAÇO e use E.":"Siga pelo píer até o embarque.\nUse E ao lado da Andorinha."),small);}
        }
        if(Time.unscaledTime<toastUntil&&dialog==""){Panel(new Rect(w/2-355,245,710,56));GUI.Label(new Rect(w/2-335,261,670,35),toast,small);}
        if(fishing)FishingUI(w,h);
        if(dialog!=""&&!paused) {
            float bh=286;Panel(new Rect(w/2-410,h-bh-65,820,bh));
            GUI.Label(new Rect(w/2-380,h-bh-43,740,32),dialogName,heading);
            GUI.Label(new Rect(w/2-380,h-bh+5,740,174),dialog,body);
            if(GUI.Button(new Rect(w/2+202,h-126,175,43),"CONTINUAR  [E]",button))CloseDialog();
        }
        if(inventory){Panel(new Rect(w/2-290,180,580,460));GUI.Label(new Rect(w/2-260,205,520,40),"DIÁRIO DE BORDO",heading);GUI.Label(new Rect(w/2-260,255,520,30),"Capturas: "+save.totalCatches+"  •  Vendidas: "+save.sold,small);if(save.hold.Count==0)GUI.Label(new Rect(w/2-260,310,520,45),"Porão vazio. Há três boias na enseada.",body);inventoryScroll=GUI.BeginScrollView(new Rect(w/2-260,305,520,265),inventoryScroll,new Rect(0,0,490,Mathf.Max(265,save.hold.Count*31)));for(int i=0;i<save.hold.Count;i++)GUI.Label(new Rect(0,i*31,490,30),save.hold[i].name+"  ·  "+save.hold[i].value+" moedas",small);GUI.EndScrollView();GUI.Label(new Rect(w/2-260,593,520,30),"TAB para fechar  •  F8 para fotografar",small);}
        if(paused){Panel(new Rect(w/2-230,h/2-160,460,320));GUI.Label(new Rect(w/2-200,h/2-125,400,40),"UM RESPIRO NO CAIS",heading);if(GUI.Button(new Rect(w/2-200,h/2-55,400,48),"VOLTAR AO MAR",button))SetPaused(false);if(GUI.Button(new Rect(w/2-200,h/2+8,400,48),"SALVAR PROGRESSO",button)&&Store())Notify("Progresso salvo.");if(GUI.Button(new Rect(w/2-200,h/2+71,400,48),"SAIR",button)&&Store())Application.Quit();}
    }
    string Prompt() {
        if(aboard){int i=NearestSpot();if(i>=0)return save.hold.Count>=HoldCapacity?"Porão cheio • volte ao mercado":Time.time<readyAt[i]?"Covo de molho • "+Mathf.CeilToInt(readyAt[i]-Time.time)+" s":Mathf.Abs(speed)>1.2f?"[ESPAÇO] Frear para recolher":"[E] Recolher covo";if(HarborNavigation.Distance(vessel.position,HarborNavigation.Mooring)<=5.5f)return Mathf.Abs(speed)>1.2f?"[ESPAÇO] Frear para atracar":"[E] Atracar e desembarcar";return save.hold.Count>0?"Porto • "+Mathf.RoundToInt(HarborNavigation.Distance(vessel.position,HarborNavigation.Mooring))+" m • siga a carta":"Procure as bandeirolas amarelas da enseada";}
        Vector3 p=captain.position;if(Vector3.Distance(p,HarborWorld.mara.position)<2.8f)return "[E]  Conversar com Mara";
        if(Vector3.Distance(p,new Vector3(1,1.55f,6.8f))<3.3f)return "[E]  Mercado • vender captura";
        if(Vector3.Distance(p,new Vector3(-15,1.55f,9.1f))<2.6f)return "[E]  Conversar com Nina";
        if(Vector3.Distance(p,new Vector3(8,1.55f,7))<2.6f)return "[E]  Conversar com Ivo";
        if(HarborNavigation.Distance(p,HarborNavigation.Landing)<=2.2f)return "[E]  Embarcar na Andorinha";return "";
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
            GUI.Label(new Rect(x-37,y,190,23),"COVO "+(i+1)+" · "+Mathf.RoundToInt(HarborNavigation.Distance(vessel.position,HarborWorld.FishingSpots[i]))+" m",small);
        }
    }
    void DrawChart(float w) {
        Rect map=new Rect(w-228,185,198,188);Panel(map);
        GUI.Label(new Rect(map.x+15,map.y+10,175,25),"CARTA DA ENSEADA",tiny);
        Vector2 origin=new Vector2(map.x+99,map.y+49);float sx=1.2f,sz=1.4f;
        GUI.color=new Color(.56f,.67f,.55f);GUI.DrawTexture(new Rect(map.x+16,map.y+36,166,8),white);GUI.color=Color.white;
        for(int i=0;i<3;i++) {
            Vector3 p=HarborWorld.FishingSpots[i];float x=origin.x+p.x*sx,y=origin.y-p.z*sz;
            GUI.color=Time.time<readyAt[i]?new Color(.45f,.52f,.48f):new Color(.97f,.71f,.30f);GUI.DrawTexture(new Rect(x-3,y-3,6,6),white);GUI.color=Color.white;
            GUI.Label(new Rect(x+6,y-8,30,20),(i+1).ToString(),tiny);
        }
        Vector3 b=vessel.position;float bx=origin.x+b.x*sx,by=origin.y-b.z*sz;
        bx=Mathf.Clamp(bx,map.x+10,map.xMax-13);by=Mathf.Clamp(by,map.y+40,map.yMax-20);
        GUI.color=new Color(.65f,.88f,.85f);GUI.DrawTexture(new Rect(bx-4,by-4,8,8),white);GUI.color=Color.white;
        Matrix4x4 oldMatrix=GUI.matrix;GUIUtility.RotateAroundPivot(vessel.eulerAngles.y,new Vector2(bx,by));GUI.color=new Color(.65f,.88f,.85f);GUI.DrawTexture(new Rect(bx-1,by-12,2,9),white);GUI.color=Color.white;GUI.matrix=oldMatrix;
        GUI.Label(new Rect(origin.x+5,origin.y+4,70,20),"PORTO",tiny);
        GUI.Label(new Rect(map.x+14,map.yMax-23,170,20),"N ↑   •   amarelo = covo",tiny);
    }
}
}
