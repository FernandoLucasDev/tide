using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace MareDeFerro {
public partial class HarborGame {
    void ReadStory(){int limit=0;while(dialog!=""&&limit++<20){Check(Localize(dialog)!=dialog,"English translation exists for story page: "+dialogName);CloseDialog();}Check(dialog=="","Conversation completes without trapping input");}
    IEnumerator ApproachTask() {
        Vector3 target=TaskTarget();
        if(OnTrainingDeck) {
            float z=target.z-DeckOrigin.z;
            yield return WalkRoute(DeckOrigin+new Vector3(0,1.65f,captain.position.z-DeckOrigin.z),DeckOrigin+new Vector3(0,1.65f,Mathf.Min(z,3.5f)));
            if(Mathf.Abs(target.x-DeckOrigin.x)>1)yield return WalkRoute(new Vector3(target.x+(target.x<DeckOrigin.x?1.5f:-1.5f),1.65f,target.z));
        } else if(Chapter==Prologue.Mara)yield return WalkRoute(new Vector3(-4.5f,1.6f,1.7f));
        else if(Chapter==Prologue.Dalia)yield return WalkRoute(new Vector3(-4.5f,1.6f,6),new Vector3(13,1.6f,6.5f));
        else if(Chapter==Prologue.Celina)yield return WalkRoute(new Vector3(13,1.6f,6),new Vector3(-4.5f,1.6f,6),CelinaPlace+Vector3.forward);
        else if(Chapter==Prologue.Deliver)yield return WalkRoute(new Vector3(-4.5f,1.6f,6),HarborNavigation.Market);
        else if(Chapter==Prologue.Dinner)yield return WalkRoute(new Vector3(3.5f,1.6f,6));
        else if(Chapter==Prologue.Radio)yield return WalkRoute(new Vector3(-4.5f,1.6f,6),RadioPlace+Vector3.forward);
        else yield return WalkRoute(target+Vector3.back*1.3f);
    }
    IEnumerator ValidatePrologue() {
        yield return null;
        yield return Photograph("ui-menu");
        Check(english&&portraitAtlas&&coverTexture,"English default, crew atlas and approved harbor cover loaded");
        Check(captain.GetComponent<HarborActor>()&&captain.Find("Respiração/Casaco costurado"),"Detailed player rig and continuous clothing mesh loaded");
        Check(Resources.Load<Texture2D>("Art/WeatheredWood")&&Resources.Load<Texture2D>("Art/CoastalPlaster"),"Painted wood and plaster textures included in standalone build");
        StartJourney();
        string firstPage=dialog;
        Check(Localize(firstPage)!=firstPage,"Arrival is localized into English");
        SetLanguage(false);Check(Localize(firstPage)==firstPage,"Portuguese preserves original story text");
        yield return Photograph("ui-portugues");SetLanguage(true);
        Check(Chapter==Prologue.Arrival&&dialog!="","Fresh journey opens arrival for a job aboard the missing Boa Esperanca");
        yield return Photograph("prologo-chegada");ReadStory();
        yield return Photograph("ui-exploracao");
        SetInventory(true);yield return Photograph("ui-diario");journalTab=2;yield return Photograph("ui-tripulacao");
        Screen.SetResolution(1280,720,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);
        yield return Photograph("ui-tripulacao-720p");SetInventory(false);SetPaused(true);
        yield return Photograph("ui-pausa-720p");SetPaused(false);
        Screen.SetResolution(1440,900,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);
        Check(Chapter==Prologue.Notice&&!TryBoard(),"Novice checks departures and cannot bypass training by boarding Andorinha");
        // Isolated recovery regression before the physically walked journey.
        PlacePlayer(new Vector3(-4.5f,-2,-4.5f));WalkStep(Vector3.zero,false,.02f);
        Check(captain.position.y>1&&HarborNavigation.Distance(captain.position,HarborWorld.DockSpawn)<1,"Falling off pier recovers at safe harbor spawn");
        Check(save.coins==0,"New arrival has spent their savings on the ferry passage");
        int visits=0;
        while(PrologueActive&&visits++<45) {
            Prologue stage=Chapter;
            if(stage==Prologue.Soak) {
                float old=soakTime;SetPaused(true);TickPrologue(20,false,false);Check(soakTime==old,"Pause freezes soaking timer");SetPaused(false);
                SetInventory(true);TickPrologue(20,false,false);Check(soakTime==old,"Journal freezes soaking timer");SetInventory(false);
                for(int i=0;i<90;i++){TickPrologue(.1f,false,false);yield return null;}
                Check(Chapter==Prologue.Winch,"Soaking finishes and unlocks supervised winch");continue;
            }
            if(stage==Prologue.Ending) {
                SetPaused(true);TickPrologue(10,false,false);Check(knocks==0,"Pause cannot skip final sound sequence");SetPaused(false);
                for(int i=0;i<60;i++){TickPrologue(.1f,false,false);yield return new WaitForSecondsRealtime(.1f);}
                Check(knocks==3&&dialog!="","Exactly three audible knocks lead to the ending conversation");
                yield return Photograph("prologo-tres-pancadas");ReadStory();break;
            }
            // Use actual movement and physical collision for every task, never teleport between stations.
            yield return ApproachTask();
            if(stage==Prologue.Mara)yield return Photograph("ui-interacao");
            Check(HarborNavigation.Distance(captain.position,TaskTarget())<=2,"Walk to reachable task: "+stage+" / pot "+save.trainingPots);
            TickPrologue(.02f,false,false);
            if(HoldTask) {
                TickPrologue(.3f,true,true);float partial=taskProgress;
                if(stage==Prologue.Bait&&save.trainingPots==0){yield return Photograph("art-trabalho");Check(Quaternion.Angle(Quaternion.identity,captain.Find("Respiração/Manga esquerda").localRotation)>20,"Work animation responds to active deck task");}
                SetPaused(true);TickPrologue(10,true,false);Check(taskProgress==partial,"Pause freezes manual work: "+stage);SetPaused(false);
                TickPrologue(.1f,false,false);Check(taskProgress==0,"Releasing E resets unfinished work: "+stage);
                for(int i=0;i<200&&Chapter==stage&&dialog=="";i++)TickPrologue(.02f,true,i==0);
            } else TickPrologue(.02f,true,true);
            if(stage==Prologue.Sort&&save.trainingPots==2)yield return Photograph("prologo-ermitao");
            if(stage==Prologue.Dinner)yield return Photograph("prologo-jantar");
            if(stage==Prologue.Mara)yield return Photograph("ui-mara");
            ReadStory();
            Check(Chapter!=stage,"Task advances exactly once: "+stage);
            Store();var disk=HarborStorage.Load(SavePath,out bool recovered);
            Check(!recovered&&disk.prologueStage==save.prologueStage&&disk.trainingPots==save.trainingPots&&disk.porcelainMemento==save.porcelainMemento,"Checkpoint roundtrip: "+Chapter);
            if(Chapter==Prologue.LoadPot&&save.trainingPots==1) {
                save=disk;ResumePrologue();
                Check(Chapter==Prologue.LoadPot&&carriedItem.gameObject.activeSelf,"Reload mid-task restores held bait and training location");
                yield return Photograph("prologo-conves");
            }
        }
        Check(Chapter==Prologue.Complete,"Entire prologue reaches free exploration");
        Check(save.coins==36&&save.trainingPots==3&&save.totalCatches==3&&save.sold==2,"Three pots and 36-coin wage awarded exactly once");
        Check(save.porcelainMemento&&save.hold.Count==0,"Story hermit is protected from sale and does not occupy commercial hold");
        int coins=save.coins;CloseDialog();TickPrologue(10,true,true);Check(save.coins==coins,"Repeated completion cannot duplicate wages");
        yield return WalkRoute(HarborNavigation.Landing);Check(TryBoard()&&TryDock(),"Finishing prologue unlocks existing free sailing");
        string legacy=Path.Combine(Application.persistentDataPath,"porto-prologue-legacy-test.json");
        File.WriteAllText(legacy,"{\"version\":2,\"coins\":123,\"totalCatches\":5,\"sold\":4,\"hold\":[]}");
        var migrated=HarborStorage.Load(legacy,out bool backup);
        Check(migrated.coins==123&&migrated.prologueStage==0&&migrated.version==3,"Legacy economy preserved while new prologue becomes available");
        // Reload the concluding checkpoint: no repeated ending or rewards.
        save=HarborStorage.Load(SavePath,out backup);Check(save.prologueStage==(int)Prologue.Complete&&save.porcelainMemento,"Ending and keepsake survive disk reload");
        Check(true,"Full prologue exercised using movement, interactions, dialog, audio timing and persistence");
    }
}
}
