using UnityEngine;

namespace MareDeFerro {
public partial class HarborGame {
    Transform NewRoot(string name,Vector3 position){var t=new GameObject(name).transform;t.position=position;return t;}
    void BuildPrologue() {
        arrivalRoot=NewRoot("Chegada da balsa",Vector3.zero);
        HarborWorld.Box("Quadro de embarques",NoticePlace+new Vector3(1.1f,1,0),new Vector3(.12f,1.3f,1.4f),HarborWorld.wood,arrivalRoot,false);
        HarborWorld.Box("Aviso de atraso",NoticePlace+new Vector3(1.02f,1,0),new Vector3(.025f,.9f,1.1f),HarborWorld.cream,arrivalRoot,false);
        MarkerLabel("EMBARQUES",NoticePlace+Vector3.up*2,arrivalRoot);
        HarborWorld.Box("Sua bagagem",HarborWorld.DockSpawn+new Vector3(.8f,.3f,0),new Vector3(.6f,.5f,.4f),HarborWorld.teal,arrivalRoot,false);
        var ferry=HarborWorld.Boat(new Vector3(-39,.1f,-12),90,"Balsa Travessia",1.8f);ferry.SetParent(arrivalRoot,true);

        // A stable walkable training deck. Celina handles the crossing; the novice never pilots it.
        deckRoot=NewRoot("São Bento — convés de aprendizagem",DeckOrigin);
        HarborWorld.Box("Casco",new Vector3(0,.25f,0),new Vector3(8.6f,2.3f,16.4f),HarborWorld.teal,deckRoot);
        HarborWorld.Box("Convés",new Vector3(0,1.45f,0),new Vector3(8,.2f,16),HarborWorld.wood,deckRoot);
        BuildBow();
        for(int i=0;i<16;i++)HarborWorld.Box("Junta do piso",new Vector3(0,1.557f,-7.5f+i),new Vector3(8,.01f,.025f),HarborWorld.darkWood,deckRoot,false);
        for(int s=-1;s<=1;s+=2) {
            HarborWorld.Box("Amurada",new Vector3(s*4.1f,1.9f,0),new Vector3(.22f,1.1f,16.3f),HarborWorld.cream,deckRoot);
            for(int i=0;i<5;i++)HarborWorld.Ball("Defensa",new Vector3(s*4.35f,.8f,-6+i*3),new Vector3(.4f,1.1f,.8f),HarborWorld.iron,deckRoot);
        }
        HarborWorld.Box("Amurada de popa",new Vector3(0,1.9f,-8.1f),new Vector3(8.3f,1.1f,.22f),HarborWorld.cream,deckRoot);
        HarborWorld.Box("Cabine",new Vector3(0,3,6.5f),new Vector3(4.5f,3,2.4f),HarborWorld.cream,deckRoot);
        HarborWorld.Box("Teto",new Vector3(0,4.6f,6.5f),new Vector3(4.9f,.2f,2.8f),HarborWorld.teal,deckRoot);
        HarborWorld.Box("Janela",new Vector3(0,3.4f,5.27f),new Vector3(3.7f,.8f,.08f),HarborWorld.teal,deckRoot,false);
        HarborWorld.Beam("Mastro",new Vector3(0,4.7f,6.5f),new Vector3(0,7.5f,6.5f),.12f,HarborWorld.iron,deckRoot);
        HarborWorld.Rope("Antena",new Vector3(0,7.5f,6.5f),new Vector3(-4,2,-7),.25f,deckRoot,HarborWorld.iron);
        HarborWorld.DressTrainingVessel(deckRoot);
        TrainingTable("BANCADA DE ISCA",new Vector3(-2.5f,1.6f,-4));
        TrainingTable("TRIAGEM",new Vector3(-2.5f,1.6f,0));
        HarborWorld.Box("Porão e viveiro",new Vector3(-2.5f,1.95f,3),new Vector3(1.5f,.8f,1.2f),HarborWorld.teal,deckRoot);
        MarkerLabel("PORÃO / VIVEIRO",new Vector3(-2.5f,3,3),deckRoot);
        HarborWorld.Crate(new Vector3(2.5f,1.6f,3),1.1f,deckRoot);
        MarkerLabel("AMARRAR CARGA",new Vector3(2.5f,3,3),deckRoot);
        HarborWorld.Beam("Guincho",new Vector3(3.3f,1.6f,0),new Vector3(3.3f,4.3f,0),.15f,HarborWorld.iron,deckRoot);
        HarborWorld.Beam("Braço",new Vector3(3.3f,4.3f,0),new Vector3(4.3f,4.3f,-3),.15f,HarborWorld.iron,deckRoot);
        HarborWorld.Rope("Cabo",new Vector3(4.3f,4.3f,-3),new Vector3(3.7f,1,-3),0,deckRoot,HarborWorld.iron);
        MarkerLabel("GUINCHO",new Vector3(2.5f,3,0),deckRoot);
        MarkerLabel("ABASTECER COVO",new Vector3(2.5f,3,-4),deckRoot);
        questPot=NewRoot("Covo de trabalho",Vector3.zero);questPot.SetParent(deckRoot,false);
        HarborWorld.CrabPot(Vector3.zero,questPot);
        trainingCatch=NewRoot("Captura na mesa",Vector3.zero);trainingCatch.SetParent(deckRoot,false);trainingCatch.localPosition=new Vector3(-2.5f,2.6f,0);
        HarborWorld.Ball("Carapaça",Vector3.zero,new Vector3(.55f,.2f,.4f),HarborWorld.roof,trainingCatch);
        for(int s=-1;s<=1;s+=2)for(int i=0;i<3;i++)HarborWorld.Beam("Pata",new Vector3(s*.2f,0,-.15f+i*.15f),new Vector3(s*.5f,-.08f,-.25f+i*.23f),.045f,HarborWorld.roof,trainingCatch);
        porcelain=NewRoot("Ermitão na xícara azul",Vector3.zero);porcelain.SetParent(trainingCatch,false);
        HarborWorld.Cylinder("Xícara",new Vector3(0,.12f,0),.22f,.3f,HarborWorld.cream,porcelain);
        HarborWorld.Cylinder("Interior",new Vector3(0,.276f,0),.17f,.015f,HarborWorld.teal,porcelain);
        HarborWorld.Ball("Alça",new Vector3(.25f,.15f,0),new Vector3(.18f,.2f,.09f),HarborWorld.teal,porcelain);
        HarborWorld.Box("Faixa azul",new Vector3(0,.13f,-.22f),new Vector3(.24f,.07f,.025f),HarborWorld.teal,porcelain,false);
        carriedItem=NewRoot("O que você carrega",Vector3.zero);carriedItem.SetParent(captain,false);carriedItem.localPosition=new Vector3(0,.9f,.5f);
        HarborWorld.Box("Balde",Vector3.zero,new Vector3(.45f,.35f,.4f),HarborWorld.teal,carriedItem,false);
        celina=HarborWorld.Person("Celina",CelinaPlace,HarborWorld.teal,false);
        dalia=HarborWorld.Person("Dália",DaliaPlace,HarborWorld.roof,true);
        MarkerLabel("CELINA",new Vector3(0,2.4f,0),celina);
        MarkerLabel("DÁLIA",new Vector3(0,2.4f,0),dalia);
        tableRoot=NewRoot("Jantar da tripulação",DinnerPlace+Vector3.forward*.5f);
        HarborWorld.Box("Mesa",new Vector3(0,.8f,0),new Vector3(1.8f,.15f,1),HarborWorld.wood,tableRoot);
        for(int s=-1;s<=1;s+=2)HarborWorld.Box("Banco",new Vector3(0,.35f,s*.85f),new Vector3(1.8f,.15f,.4f),HarborWorld.wood,tableRoot);
        for(int i=0;i<3;i++)HarborWorld.Cylinder("Prato",new Vector3(-.6f+i*.6f,.9f,0),.22f,.04f,HarborWorld.cream,tableRoot);
        MarkerLabel("JANTAR",new Vector3(0,2.6f,0),tableRoot);
        radioProp=NewRoot("Rádio desligado",RadioPlace);
        HarborWorld.Box("Caixa do rádio",new Vector3(.9f,.6f,0),new Vector3(.5f,.5f,.25f),HarborWorld.iron,radioProp,false);
        HarborWorld.Beam("Antena",new Vector3(1,.8f,0),new Vector3(1,1.4f,0),.025f,HarborWorld.iron,radioProp);
        storyAudio=gameObject.AddComponent<AudioSource>();storyAudio.spatialBlend=0;
        knockClip=MakeStorySound("Pancada sob o casco",true);workClip=MakeStorySound("Trabalho no convés",false);
        SyncPrologueWorld();
    }
    void BuildBow() {
        var mesh=new Mesh{name="Proa do São Bento"};
        mesh.vertices=new[]{new Vector3(-4.3f,1.55f,8),new Vector3(4.3f,1.55f,8),new Vector3(0,1.55f,11),new Vector3(-3.4f,-.8f,8),new Vector3(3.4f,-.8f,8),new Vector3(0,-.5f,10)};
        mesh.triangles=new[]{0,2,1,0,3,5,0,5,2,2,5,4,2,4,1};mesh.RecalculateNormals();
        var g=new GameObject("Proa");g.transform.SetParent(deckRoot,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=HarborWorld.teal;g.AddComponent<MeshCollider>().sharedMesh=mesh;
        HarborWorld.Beam("Borda da proa",new Vector3(-4.3f,2.1f,8),new Vector3(0,2.1f,11),.16f,HarborWorld.cream,deckRoot);
        HarborWorld.Beam("Borda da proa",new Vector3(4.3f,2.1f,8),new Vector3(0,2.1f,11),.16f,HarborWorld.cream,deckRoot);
    }
    void TrainingTable(string labelText,Vector3 p) {
        HarborWorld.Box(labelText,p+Vector3.up*.8f,new Vector3(1.5f,.2f,1.3f),HarborWorld.wood,deckRoot);
        for(int s=-1;s<=1;s+=2)HarborWorld.Box("Pé",p+new Vector3(s*.5f,.4f,0),new Vector3(.12f,.8f,.9f),HarborWorld.iron,deckRoot);
        MarkerLabel(labelText,p+Vector3.up*1.7f,deckRoot);
    }
    void MarkerLabel(string text,Vector3 p,Transform parent) {
        var go=new GameObject(text);go.transform.SetParent(parent,false);go.transform.localPosition=p;
        var mesh=go.AddComponent<TextMesh>();mesh.text=text;mesh.characterSize=.045f;mesh.fontSize=42;mesh.anchor=TextAnchor.MiddleCenter;mesh.color=new Color(1,.82f,.5f);
        go.AddComponent<HarborWorldCaption>();
    }
    AudioClip MakeStorySound(string name,bool knock) {
        const int rate=22050;float[] samples=new float[rate/2];var random=new System.Random(91);
        for(int i=0;i<samples.Length;i++){float t=i/(float)rate;float noise=(float)random.NextDouble()*2-1;samples[i]=Mathf.Exp(-t*(knock?15:28))*(Mathf.Sin(t*(knock?95:260)*Mathf.PI*2)*.65f+noise*.2f);}
        var clip=AudioClip.Create(name,samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
    }
    void SyncPrologueWorld() {
        if(!arrivalRoot)return;
        arrivalRoot.gameObject.SetActive(Chapter<=Prologue.Mara);deckRoot.gameObject.SetActive(OnTrainingDeck);
        HarborWorld.mara.position=Chapter>=Prologue.Radio&&PrologueActive?RadioPlace+new Vector3(-.6f,0,-.6f):new Vector3(-5.6f,1.54f,3);
        celina.position=OnTrainingDeck?DeckOrigin+new Vector3(0,1.6f,5):Chapter==Prologue.Board?HarborNavigation.Landing+new Vector3(0,0,-1):CelinaPlace;
        if(Chapter==Prologue.Dinner)celina.position=DinnerPlace+new Vector3(1.2f,0,1.5f);
        if(Chapter==Prologue.Ending)celina.position=RadioPlace+new Vector3(.8f,0,1.4f);
        dalia.position=Chapter==Prologue.Dinner?DinnerPlace+new Vector3(-1.2f,0,1.5f):DaliaPlace;
        tableRoot.gameObject.SetActive(Chapter>=Prologue.Deliver);
        radioProp.gameObject.SetActive(Chapter>=Prologue.Radio);
        questPot.localPosition=Chapter==Prologue.Winch?new Vector3(3.7f,-1,-3):new Vector3(2.5f,1.7f,-4);
        trainingCatch.gameObject.SetActive(Chapter==Prologue.Sort||Chapter==Prologue.Stow);
        porcelain.gameObject.SetActive(save.trainingPots==2);
        carriedItem.gameObject.SetActive(Chapter==Prologue.LoadPot||Chapter==Prologue.Stow);
        if(RenderSettings.sun) {
            bool night=Chapter>=Prologue.Dinner&&PrologueActive;
            RenderSettings.sun.intensity=night?.38f:Chapter<=Prologue.Mara?1.15f:1.25f;
            RenderSettings.sun.color=night?new Color(.52f,.65f,1):new Color(1,.86f,.68f);
            RenderSettings.fogColor=night?new Color(.19f,.27f,.35f):new Color(.57f,.60f,.57f);
        }
    }
}
}
