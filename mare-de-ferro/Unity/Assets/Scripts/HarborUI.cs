using System;
using UnityEngine;

namespace MareDeFerro {
public partial class HarborGame {
    readonly Color ink=new Color(.09f,.19f,.21f),ivory=new Color(.92f,.87f,.76f),gold=new Color(.78f,.62f,.36f),seafoam=new Color(.55f,.73f,.66f);
    Texture2D paperTexture,portraitAtlas,coverTexture,circleTexture,fadeTexture;
    GUIStyle serif,caption,paperBody,paperSmall,centered,frameStyle,paperStyle,portraitFrame;
    bool english=true;
    int journalTab;
    Vector2 dialogueScroll;
    string T(string en,string pt)=>english?en:pt;
    GUIStyle Style(int size,Color color,bool wrap=false,Font font=null,FontStyle weight=FontStyle.Normal) => new GUIStyle{font=font??label.font,fontSize=size,wordWrap=wrap,fontStyle=weight,normal={textColor=color}};
    void EnsureStyles() {
        if(stylesReady)return;stylesReady=true;
        english=testing||PlayerPrefs.GetString("ui.language","en")=="en";
        Font font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Font display=Font.CreateDynamicFontFromOSFont(new[]{"Georgia","Cambria","Times New Roman"},32);if(!display)display=font;
        label=new GUIStyle{font=font,fontSize=19,normal={textColor=ivory}};
        small=Style(16,ivory);tiny=Style(12,gold);caption=Style(13,gold);heading=Style(26,ivory,false,display);serif=Style(32,ink,false,display);
        title=Style(76,ivory,false,display);body=Style(21,ivory,true);paperBody=Style(21,ink,true);paperSmall=Style(16,ink,true);
        centered=Style(16,ivory);centered.alignment=TextAnchor.MiddleCenter;
        white=Solid(Color.white);accent=Solid(gold);panel=Solid(new Color(.06f,.14f,.16f,.97f));
        paperTexture=Paper();circleTexture=Disc();fadeTexture=Fade();
        coverTexture=Resources.Load<Texture2D>("UI/HarborCover");portraitAtlas=Resources.Load<Texture2D>("UI/CrewPortraits");
        frameStyle=new GUIStyle{normal={background=Rounded(new Color(.055f,.14f,.16f,.97f),gold)},border=new RectOffset(12,12,12,12)};
        paperStyle=new GUIStyle{normal={background=Rounded(ivory,gold)},border=new RectOffset(12,12,12,12)};
        portraitFrame=new GUIStyle(frameStyle);
        button=new GUIStyle(GUI.skin.button){font=font,fontSize=16,alignment=TextAnchor.MiddleCenter,border=new RectOffset(12,12,12,12),normal={background=Rounded(new Color(.12f,.25f,.26f),gold),textColor=ivory},hover={background=Rounded(new Color(.22f,.36f,.34f),ivory),textColor=Color.white},active={background=Rounded(gold,ivory),textColor=ink},focused={background=Rounded(new Color(.22f,.36f,.34f),ivory),textColor=ivory}};
        RefreshWorldLabels();
    }
    static Texture2D Solid(Color c){var t=new Texture2D(1,1);t.SetPixel(0,0,c);t.Apply();return t;}
    Texture2D Rounded(Color fill,Color edge) {
        var t=new Texture2D(32,32,TextureFormat.RGBA32,false);t.wrapMode=TextureWrapMode.Clamp;
        for(int y=0;y<32;y++)for(int x=0;x<32;x++) {
            float dx=Mathf.Max(8-x,Mathf.Max(0,x-23)),dy=Mathf.Max(8-y,Mathf.Max(0,y-23));float d=Mathf.Sqrt(dx*dx+dy*dy);
            bool border=x<2||y<2||x>29||y>29||d>6;
            t.SetPixel(x,y,d>8?Color.clear:border?edge:fill);
        }t.Apply();return t;
    }
    Texture2D Paper() {
        var t=new Texture2D(128,128);var random=new System.Random(72);
        for(int y=0;y<128;y++)for(int x=0;x<128;x++){float n=(float)random.NextDouble()*.045f;Color c=ivory-new Color(n,n,n,0);t.SetPixel(x,y,c);}t.Apply();return t;
    }
    Texture2D Disc(){var t=new Texture2D(64,64,TextureFormat.RGBA32,false);for(int y=0;y<64;y++)for(int x=0;x<64;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f));t.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(31.5f-d)));}t.Apply();return t;}
    Texture2D Fade(){var t=new Texture2D(128,1,TextureFormat.RGBA32,false);t.wrapMode=TextureWrapMode.Clamp;for(int x=0;x<128;x++)t.SetPixel(x,0,new Color(.018f,.05f,.065f,Mathf.Lerp(.94f,0,x/127f)));t.Apply();return t;}
    void Fill(Rect r,Color c){Color old=GUI.color;GUI.color=c;GUI.DrawTexture(r,white);GUI.color=old;}
    void Circle(Rect r,Color c){Color old=GUI.color;GUI.color=c;GUI.DrawTexture(r,circleTexture);GUI.color=old;}
    void Rule(float x,float y,float width,Color c){Fill(new Rect(x,y,width,1),c);}
    void Panel(Rect r){Frame(r,false);}
    void Frame(Rect r,bool paper) {
        Color old=GUI.color;GUI.color=new Color(0,0,0,.35f);frameStyle.Draw(new Rect(r.x+3,r.y+5,r.width,r.height),false,false,false,false);GUI.color=old;
        (paper?paperStyle:frameStyle).Draw(r,false,false,false,false);
        if(paper)GUI.DrawTexture(new Rect(r.x+9,r.y+9,r.width-18,r.height-18),paperTexture,ScaleMode.StretchToFill);
        foreach(Vector2 p in new[]{new Vector2(r.x+8,r.y+8),new Vector2(r.xMax-12,r.y+8),new Vector2(r.x+8,r.yMax-12),new Vector2(r.xMax-12,r.yMax-12)})Circle(new Rect(p.x,p.y,4,4),gold);
    }
    void Key(Rect r,string text,bool light=false){Fill(r,light?new Color(.79f,.75f,.65f):new Color(.16f,.26f,.27f));Rule(r.x,r.yMax-2,r.width,gold);var s=new GUIStyle(centered);s.normal.textColor=light?ink:ivory;GUI.Label(r,text,s);}
    void CompassRose(Rect r,Color color) {
        Circle(r,new Color(color.r,color.g,color.b,.2f));Vector2 c=r.center;float radius=r.width*.38f;
        for(int i=0;i<8;i++){float a=i*45;Matrix4x4 m=GUI.matrix;GUI.matrix=m*Matrix4x4.TRS(new Vector3(c.x,c.y,0),Quaternion.Euler(0,0,a),Vector3.one)*Matrix4x4.Translate(new Vector3(-c.x,-c.y,0));Fill(new Rect(c.x-1,c.y-radius,2,radius*(i%2==0?2:1.55f)),color);GUI.matrix=m;}
        Circle(new Rect(c.x-4,c.y-4,8,8),color);
    }
    bool UiButton(Rect r,string text,bool primary=false) {
        Color old=GUI.backgroundColor;if(primary)GUI.backgroundColor=new Color(1.6f,1.3f,.8f);
        bool hit=GUI.Button(r,text,button);GUI.backgroundColor=old;return hit;
    }
    void LanguageButtons(float x,float y) {
        if(UiButton(new Rect(x,y,64,32),"EN",english))SetLanguage(true);
        if(UiButton(new Rect(x+72,y,64,32),"PT",!english))SetLanguage(false);
    }
    void SetLanguage(bool value){english=value;if(!testing){PlayerPrefs.SetString("ui.language",english?"en":"pt");PlayerPrefs.Save();}RefreshWorldLabels();}
    void OnGUI() {
        EnsureStyles();float scale=Mathf.Min(Screen.width/1440f,Screen.height/900f);GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one*scale);
        float w=Screen.width/scale,h=Screen.height/scale;
        if(!started){DrawTitle(w,h);return;}
        DrawHud(w,h);
        if(!paused&&!inventory&&dialog=="") {
            if(PrologueActive){DrawQuestCard();DrawTaskMarker(w,h);}else if(aboard){DrawCompass(w,h);DrawChart(w);}
            DrawInteraction(w,h);
            if(fishing)FishingUI(w,h);
            if(Time.unscaledTime<toastUntil)DrawToast(w,h);
        }
        if(dialog!=""&&!paused&&!inventory)DrawDialogue(w,h);
        if(inventory&&!paused)DrawJournal(w,h);
        if(paused)DrawPause(w,h);
    }
    void DrawTitle(float w,float h) {
        if(coverTexture)GUI.DrawTexture(new Rect(0,0,w,h),coverTexture,ScaleMode.ScaleAndCrop);
        GUI.DrawTexture(new Rect(0,0,w*.8f,h),fadeTexture);
        Fill(new Rect(0,h-80,w,80),new Color(.03f,.09f,.11f,.78f));
        float x=80,y=Mathf.Max(65,h*.16f);
        CompassRose(new Rect(x,y,50,50),gold);GUI.Label(new Rect(x+68,y+16,400,28),T("A LIFE BUILT ON THE TIDE","UMA VIDA CONSTRUÍDA NO MAR"),caption);
        GUI.Label(new Rect(x-5,y+85,570,190),"MARÉ\nDE FERRO",title);
        Rule(x,y+295,90,gold);
        GUI.Label(new Rect(x,y+319,460,90),T("You came for a job on a fishing boat.\nThe boat never came back.","Você veio por uma vaga num barco.\nO barco não voltou."),body);
        if(UiButton(new Rect(x,y+430,350,58),save.prologueStage==0?T("BEGIN YOUR STORY   →","COMEÇAR HISTÓRIA   →"):T("CONTINUE YOUR VOYAGE   →","CONTINUAR VIAGEM   →"),true))StartJourney();
        GUI.Label(new Rect(x,y+510,450,28),T("PROLOGUE  /  A PLACE ON THE DOCK","PRÓLOGO  /  UMA VAGA NO CAIS"),caption);
        LanguageButtons(80,h-57);GUI.Label(new Rect(w-390,h-49,330,25),"PORTO DAS VELAS  ·  01",small);
    }
    void DrawHud(float w,float h) {
        Frame(new Rect(28,26,330,80),false);CompassRose(new Rect(43,40,48,48),gold);
        GUI.Label(new Rect(108,39,230,31),OnTrainingDeck?"São Bento":"Porto das Velas",heading);
        GUI.Label(new Rect(109,76,230,20),OnTrainingDeck?T("GREENHORN  /  DECK CREW","NOVATO  /  TRIPULAÇÃO"):T("VELAS COVE  /  HOME PORT","ENSEADA  /  PORTO"),tiny);
        Frame(new Rect(w/2-108,27,216,65),false);
        string phase=Chapter<=Prologue.Board?T("Morning","Manhã"):OnTrainingDeck?T("Afternoon","Tarde"):Chapter<Prologue.Radio?T("Dusk","Anoitecer"):T("Night","Noite");
        GUI.Label(new Rect(w/2-90,37,190,25),T("DAY 01","DIA 01")+"   /   "+phase,small);
        Rule(w/2-90,68,178,gold);GUI.Label(new Rect(w/2-90,74,190,18),T("CRAB SEASON • FIRST ARRIVAL","TEMPORADA • PRIMEIRA CHEGADA"),Style(10,gold));
        Frame(new Rect(w-230,26,202,80),false);Circle(new Rect(w-211,43,28,28),gold);GUI.Label(new Rect(w-208,47,25,22),"¢",Style(18,ink));
        GUI.Label(new Rect(w-171,38,130,31),save.coins.ToString("N0"),heading);
        GUI.Label(new Rect(w-209,78,174,19),PrologueActive?save.trainingPots+T(" / 3 pots stowed"," / 3 covos guardados"):save.hold.Count+" / "+HoldCapacity+T(" hold slots"," espaços no porão"),tiny);
        if(dialog==""&&!inventory&&!paused) {
            Fill(new Rect(0,h-54,w,54),new Color(.025f,.07f,.09f,.83f));
            Key(new Rect(30,h-41,57,27),"WASD");GUI.Label(new Rect(97,h-37,210,25),aboard?T("Throttle & rudder","Motor e leme"):T("Move","Caminhar"),small);
            Key(new Rect(246,h-41,28,27),"C");GUI.Label(new Rect(285,h-37,150,25),T("Camera","Câmera"),small);
            if(UiButton(new Rect(w/2-92,h-45,184,35),T("TAB   Captain’s log","TAB   Diário de bordo")))SetInventory(true);
            if(UiButton(new Rect(w-168,h-45,140,35),T("ESC   Pause","ESC   Pausa")))SetPaused(true);
        }
    }
    void DrawQuestCard() {
        Rect r=new Rect(28,138,292,218);Frame(r,true);
        GUI.Label(new Rect(r.x+22,r.y+17,250,22),T("CHAPTER 00","CAPÍTULO 00"),Style(12,new Color(.48f,.35f,.2f)));
        GUI.Label(new Rect(r.x+22,r.y+42,250,33),T("A place on the dock","Uma vaga no cais"),Style(23,ink,false,serif.font));
        Rule(r.x+22,r.y+85,247,new Color(.69f,.61f,.46f));
        GUI.Label(new Rect(r.x+22,r.y+102,247,76),UiObjective(),Style(18,ink,true));
        for(int i=0;i<3;i++)Circle(new Rect(r.x+22+i*18,r.y+188,8,8),i<save.trainingPots?ink:new Color(.68f,.64f,.54f));
        GUI.Label(new Rect(r.x+91,r.y+184,175,23),OnTrainingDeck?T("Your first three pots","Seus primeiros três covos"):T("A new beginning","Um novo começo"),Style(12,ink));
    }
    void DrawTaskMarker(float w,float h) {
        Vector3 screen=view.WorldToScreenPoint(TaskTarget()+Vector3.up*2.2f);if(screen.z<=0)return;
        float scale=Mathf.Min(Screen.width/1440f,Screen.height/900f);float x=Mathf.Clamp(screen.x/scale,355,w-245),y=Mathf.Clamp((Screen.height-screen.y)/scale,123,h-185);
        float pulse=1+Mathf.Sin(Time.unscaledTime*2)*.06f;Circle(new Rect(x-13*pulse,y-13*pulse,26*pulse,26*pulse),new Color(.96f,.74f,.38f,.24f));
        Circle(new Rect(x-5,y-5,10,10),gold);GUI.Label(new Rect(x+18,y-10,85,24),Mathf.CeilToInt(HarborNavigation.Distance(captain.position,TaskTarget()))+" m",small);
    }
    void DrawInteraction(float w,float h) {
        if(fishing)return;
        bool close=PrologueActive&&HarborNavigation.Distance(captain.position,TaskTarget())<=2;
        string text=PrologueActive?UiInteraction(close):Localize(Prompt());
        Rect r=new Rect(w/2-245,h-145,490,65);Frame(r,false);
        string key=PrologueActive?(Chapter==Prologue.Soak||Chapter==Prologue.Ending?"…":close?"E":"↑"):"E";
        Key(new Rect(r.x+17,r.y+15,35,35),key);GUI.Label(new Rect(r.x+67,r.y+13,405,42),text,new GUIStyle(small){wordWrap=true,alignment=TextAnchor.MiddleLeft});
        if(PrologueActive&&HoldTask&&taskProgress>0)Fill(new Rect(r.x+9,r.yMax-5,(r.width-18)*taskProgress,3),gold);
        if(PrologueActive&&Chapter==Prologue.Soak)Fill(new Rect(r.x+9,r.yMax-5,(r.width-18)*(1-soakTime/8),3),seafoam);
    }
    void DrawToast(float w,float h){Rect r=new Rect(w/2-330,115,660,55);Frame(r,false);GUI.Label(new Rect(r.x+20,r.y+12,r.width-40,36),Localize(toast),new GUIStyle(small){wordWrap=true});}
    int PortraitIndex(string who){switch(who){case "MARA":return 0;case "CELINA":return 1;case "DÁLIA":return 2;case "IVO":return 3;case "TOMÉ":return 4;case "NINA":return 5;default:return -1;}}
    void Portrait(Rect r,int index) {
        Frame(r,false);
        Rect inner=new Rect(r.x+6,r.y+6,r.width-12,r.height-12);
        if(portraitAtlas&&index>=0)GUI.DrawTextureWithTexCoords(inner,portraitAtlas,new Rect(index%3/3f,index<3?.5f:0,1/3f,.5f));
        else CompassRose(new Rect(r.x+r.width*.2f,r.y+r.height*.2f,r.width*.6f,r.height*.6f),gold);
    }
    void DrawDialogue(float w,float h) {
        Rect r=new Rect(w/2-550,h-334,1100,285);Frame(r,true);
        int index=PortraitIndex(dialogName);Portrait(new Rect(r.x+22,r.y+25,221,231),index);
        string name=index>=0?new[]{"Mara","Celina","Dália","Ivo","Tomé","Nina"}[index]:Localize(dialogName);
        string role=index>=0?T(new[]{"RETIRED CAPTAIN","CAPTAIN • SÃO BENTO","SHOPKEEPER","CAPTAIN • MARÉ LIVRE","HARBOR COOK","MARINE MECHANIC"}[index],new[]{"CAPITÃ APOSENTADA","CAPITÃ • SÃO BENTO","VENDEDORA","CAPITÃO • MARÉ LIVRE","COZINHEIRO DO PORTO","MECÂNICA NAVAL"}[index]):T("FROM YOUR LOGBOOK","DO SEU DIÁRIO");
        GUI.Label(new Rect(r.x+270,r.y+19,785,38),name,serif);GUI.Label(new Rect(r.x+272,r.y+63,785,21),role,Style(11,new Color(.46f,.36f,.23f)));
        Rule(r.x+271,r.y+91,797,new Color(.71f,.64f,.51f));
        string text=Localize(dialog);float needed=paperBody.CalcHeight(new GUIContent(text),775);
        dialogueScroll=GUI.BeginScrollView(new Rect(r.x+271,r.y+104,804,118),dialogueScroll,new Rect(0,0,775,Mathf.Max(118,needed)));
        GUI.Label(new Rect(0,0,775,Mathf.Max(118,needed)),text,paperBody);GUI.EndScrollView();
        GUI.Label(new Rect(r.x+272,r.y+242,500,22),needed>118?T("Scroll to read  •  Take your time","Role para ler  •  Sem pressa"):T("PORTO DAS VELAS  /  YOUR FIRST DAY","PORTO DAS VELAS  /  SEU PRIMEIRO DIA"),Style(11,new Color(.46f,.36f,.23f)));
        if(UiButton(new Rect(r.xMax-199,r.yMax-49,174,33),T("CONTINUE   [E]","CONTINUAR   [E]")))CloseDialog();
    }
    void DrawJournal(float w,float h) {
        Fill(new Rect(0,0,w,h),new Color(.01f,.035f,.04f,.65f));Rect r=new Rect(w/2-520,h/2-320,1040,640);Frame(r,true);
        Fill(new Rect(r.x+510,r.y+15,3,r.height-30),new Color(.63f,.55f,.42f));
        GUI.Label(new Rect(r.x+35,r.y+25,430,44),T("Captain’s log","Diário de bordo"),serif);
        GUI.Label(new Rect(r.x+37,r.y+78,430,22),T("PORTO DAS VELAS  /  DAY 01","PORTO DAS VELAS  /  DIA 01"),Style(12,ink));
        for(int i=0;i<3;i++)if(UiButton(new Rect(r.x+35+i*146,r.y+120,136,36),T(new[]{"VOYAGE","CATCH","PEOPLE"}[i],new[]{"VIAGEM","CAPTURAS","PESSOAS"}[i]),journalTab==i))journalTab=i;
        if(journalTab==0) {
            GUI.Label(new Rect(r.x+35,r.y+191,435,60),T("A place on the dock","Uma vaga no cais"),Style(29,ink,true,serif.font));
            GUI.Label(new Rect(r.x+35,r.y+262,428,102),PrologueActive?UiObjective():T("You have a place aboard the São Bento. Explore the cove with the Andorinha.","Você tem lugar no São Bento. Explore a enseada com a Andorinha."),paperBody);
            Rule(r.x+35,r.y+386,427,gold);
            GUI.Label(new Rect(r.x+35,r.y+414,420,120),save.porcelainMemento?T("A real job offer. A missing boat.\nThree blue waves on a porcelain cup.\n\nSomebody here knows more.","Uma oferta real. Um barco desaparecido.\nTrês ondas azuis numa xícara.\n\nAlguém aqui sabe mais."):T("A real job offer. A missing boat.\n\nFind work. Learn their names.\nKeep Raul’s letter.","Uma oferta real. Um barco desaparecido.\n\nEncontre trabalho. Aprenda seus nomes.\nGuarde a carta de Raul."),paperSmall);
        } else if(journalTab==1) {
            GUI.Label(new Rect(r.x+35,r.y+195,430,36),T("In the hold","No porão"),Style(29,ink,false,serif.font));
            inventoryScroll=GUI.BeginScrollView(new Rect(r.x+35,r.y+250,435,300),inventoryScroll,new Rect(0,0,410,Mathf.Max(300,save.hold.Count*49)));
            if(save.hold.Count==0)GUI.Label(new Rect(0,0,400,80),T("The commercial hold is empty.\nYour training catch is delivered by Celina.","O porão comercial está vazio.\nCelina entrega as capturas do treinamento."),paperSmall);
            for(int i=0;i<save.hold.Count;i++){GUI.Label(new Rect(0,i*49,300,40),Localize(save.hold[i].name),paperSmall);GUI.Label(new Rect(325,i*49,70,40),save.hold[i].value+" ¢",paperSmall);}GUI.EndScrollView();
        } else {
            string[] names={"Mara","Celina","Dália","Ivo","Tomé","Nina"};
            for(int i=0;i<6;i++){float x=r.x+35+(i%3)*146,y=r.y+195+(i/3)*180;Portrait(new Rect(x,y,120,130),i);GUI.Label(new Rect(x,y+136,130,26),names[i],paperSmall);}
        }
        GUI.Label(new Rect(r.x+552,r.y+43,420,38),T("First day at sea","Primeiro dia no mar"),Style(29,ink,false,serif.font));
        Rule(r.x+552,r.y+99,425,gold);
        string[] steps={T("A job that vanished","O emprego que sumiu"),T("A place aboard","Um lugar a bordo"),T("Three pots, safely stowed","Três covos guardados"),T("Supper with the crew","Jantar com a tripulação")};
        int[] thresholds={(int)Prologue.Dalia,(int)Prologue.Bait,(int)Prologue.Deliver,(int)Prologue.Radio};
        for(int i=0;i<4;i++){bool done=save.prologueStage>=thresholds[i];Circle(new Rect(r.x+554,r.y+135+i*55,16,16),done?ink:gold);GUI.Label(new Rect(r.x+585,r.y+128+i*55,390,40),steps[i],paperSmall);}
        Frame(new Rect(r.x+550,r.y+380,435,155),false);CompReceipt(r.x+570,r.y+395);
        GUI.Label(new Rect(r.x+35,r.yMax-43,500,25),T("Saved at each completed step","Salvo a cada etapa concluída"),Style(12,ink));
        if(UiButton(new Rect(r.xMax-173,r.yMax-53,137,34),T("CLOSE   [TAB]","FECHAR   [TAB]")))SetInventory(false);
    }
    void CompReceipt(float x,float y) {
        GUI.Label(new Rect(x,y,390,26),save.porcelainMemento?T("A LITTLE PASSENGER","UM PEQUENO PASSAGEIRO"):T("THE BOA ESPERANÇA","O BOA ESPERANÇA"),caption);
        GUI.Label(new Rect(x,y+40,385,85),save.porcelainMemento?T("A hermit crab in a cup marked with three blue waves. Safe in the livewell. Not for sale.","Um ermitão numa xícara com três ondas azuis. Protegido no viveiro. Não está à venda."):T("Captain Raul offered you a job. His boat is three days overdue. Keep the letter.","O capitão Raul ofereceu trabalho. Seu barco está atrasado há três dias. Guarde a carta."),new GUIStyle(body){fontSize=18});
    }
    void DrawPause(float w,float h) {
        Fill(new Rect(0,0,w,h),new Color(.01f,.03f,.045f,.72f));Rect r=new Rect(w/2-245,h/2-250,490,500);Frame(r,false);
        CompassRose(new Rect(r.center.x-25,r.y+28,50,50),gold);GUI.Label(new Rect(r.x+38,r.y+103,420,48),T("A moment ashore","Um respiro no cais"),heading);
        if(UiButton(new Rect(r.x+38,r.y+177,414,49),T("RESUME VOYAGE","RETOMAR VIAGEM"),true))SetPaused(false);
        if(UiButton(new Rect(r.x+38,r.y+239,414,45),T("SAVE PROGRESS","SALVAR PROGRESSO"))&&Store())Notify(T("Progress saved.","Progresso salvo."));
        if(UiButton(new Rect(r.x+38,r.y+297,414,45),T("SAVE & QUIT","SALVAR E SAIR"))&&Store())Application.Quit();
        Rule(r.x+38,r.y+370,414,gold);GUI.Label(new Rect(r.x+38,r.y+402,200,28),T("TEXT LANGUAGE","IDIOMA DOS TEXTOS"),caption);LanguageButtons(r.x+300,r.y+392);
    }
}
}
