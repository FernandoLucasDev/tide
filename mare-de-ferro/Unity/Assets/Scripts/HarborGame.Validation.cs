using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MareDeFerro {
public partial class HarborGame {
    readonly List<string> checks=new List<string>();
    string ValidationDirectory => Path.GetFullPath(Path.Combine(Application.dataPath,"..","..","..","verificacao"));
    void Check(bool condition,string description) {
        if(!condition)throw new Exception(description);
        checks.Add("PASS: "+description);Debug.Log("QA: "+description);
    }
    IEnumerator RunValidation() {
        Directory.CreateDirectory(ValidationDirectory);
        string report=Path.Combine(ValidationDirectory,"runtime-test.txt");
        File.WriteAllText(report,"RUNNING: base validation\n");
        var stack=new Stack<IEnumerator>();stack.Push(ValidateBase());
        while(stack.Count>0) {
            object next=null;bool moved=false;Exception failure=null;
            try {moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){File.WriteAllText(report,string.Join("\n",checks)+"\nFAIL: "+failure);Debug.LogError(failure);Time.timeScale=1;Application.Quit(1);yield break;}
            if(!moved){stack.Pop();continue;}
            if(next is IEnumerator nested){stack.Push(nested);continue;}
            yield return next;
        }
        File.WriteAllText(report,string.Join("\n",checks)+"\n"+DateTime.UtcNow.ToString("O"));
        Debug.Log("MARE_QA_PASS: "+checks.Count+" checks");Time.timeScale=1;Application.Quit(0);
    }
    IEnumerator WalkRoute(params Vector3[] waypoints) {
        foreach(Vector3 point in waypoints) {
            float deadline=Time.realtimeSinceStartup+20;
            while(HarborNavigation.Distance(captain.position,point)>.18f) {
                if(Time.realtimeSinceStartup>deadline)throw new Exception("Walking route blocked near "+captain.position+"; target "+point);
                WalkStep((point-captain.position).normalized,true,Mathf.Min(Time.unscaledDeltaTime,.05f));
                yield return null;
            }
        }
    }
    IEnumerator SailRoute(params Vector3[] waypoints) {
        foreach(Vector3 point in waypoints) {
            float deadline=Time.realtimeSinceStartup+40;
            while(true) {
                Vector3 to=point-vessel.position;to.y=0;float distance=to.magnitude;
                if(distance<2&&Mathf.Abs(speed)<.15f)break;
                if(Time.realtimeSinceStartup>deadline)throw new Exception("Sailing route blocked near "+vessel.position+"; target "+point+"; speed "+speed);
                float yaw=Mathf.Atan2(to.x,to.z)*Mathf.Rad2Deg,turn=Mathf.DeltaAngle(vessel.eulerAngles.y,yaw);
                float throttle=Mathf.Abs(turn)>45?.2f:Mathf.Clamp01((distance-1.2f)/7);
                SailStep(throttle,Mathf.Clamp(turn/22,-1,1),distance<1.9f,Mathf.Min(Time.unscaledDeltaTime,.05f));
                if(!HarborNavigation.CanOccupy(vessel.position,vessel.eulerAngles.y))throw new Exception("Hull crossed an obstacle");
                yield return null;
            }
        }
    }
    IEnumerator Photograph(string name) {
        yield return new WaitForSecondsRealtime(.3f);
        ScreenCapture.CaptureScreenshot(Path.Combine(ValidationDirectory,name+".png"));
        yield return new WaitForSecondsRealtime(.3f);
    }
    IEnumerator ValidateBase() {
        yield return null;StartJourney();CloseDialog();
        Check(!TryBoard(),"Cannot board from the starting position");
        yield return WalkRoute(HarborNavigation.Landing);
        Check(TryBoard(),"Walk from spawn to gangway with CharacterController, then board");
        Check(!BeginFishing(0),"Cannot collect a distant pot");
        yield return SailRoute(new Vector3(.6f,0,-23),HarborWorld.FishingSpots[0]);
        Check(HarborNavigation.Distance(vessel.position,HarborWorld.FishingSpots[0])<2,"Sail to first buoy using acceleration, rudder and hull collision checks");
        speed=3;Check(!BeginFishing(0),"Cannot use winch while moving fast");
        for(int i=0;i<60;i++)SailStep(0,0,true,1f/60);
        Check(Mathf.Abs(speed)<.01f,"Space brake stops boat from 3 m/s within one second");
        Check(BeginFishing(0),"Begin fishing after braking");
        SetPaused(true);float before=Time.time,oldProgress=progress;
        TickFishing(2,true);yield return new WaitForSecondsRealtime(.2f);
        Check(Mathf.Abs(Time.time-before)<.01f&&progress==oldProgress,"Pause freezes simulation clock and winch");SetPaused(false);
        SetInventory(true);TickFishing(2,true);Check(progress==oldProgress,"Inventory suspends fishing");SetInventory(false);
        float fishingDeadline=Time.realtimeSinceStartup+30;
        while(fishing) {
            if(Time.realtimeSinceStartup>fishingDeadline)throw new Exception("Winch did not finish");
            TickFishing(Mathf.Min(Time.deltaTime,.05f),tension<.63f);
            if(progress>.45f&&progress<.47f)ScreenCapture.CaptureScreenshot(Path.Combine(ValidationDirectory,"pesca-interface.png"));
            yield return null;
        }
        Check(save.hold.Count==1&&save.hold[0].value==24,"Winch yields exactly one catch");
        FinishCatch();Check(save.hold.Count==1,"Repeated completion cannot duplicate catch");
        yield return Photograph("captura-interface");CloseDialog();
        Check(!BeginFishing(0),"Pot cooldown blocks immediate recollection");
        Check(Sell()==0&&save.hold.Count==1,"Cannot sell at sea");
        yield return SailRoute(new Vector3(-8,0,-24),new Vector3(3,0,-23),new Vector3(.6f,0,-13));
        Check(TryDock(),"Return to port under rudder control and dock");
        Check(HarborNavigation.Distance(vessel.position,HarborNavigation.Mooring)<.01f,"Docking snaps boat to reachable mooring");
        Check(TryBoard()&&TryDock(),"Can reboard immediately after docking");
        Check(Sell()==0,"Cannot sell from gangway");
        yield return WalkRoute(new Vector3(-4.5f,1.5f,5.8f),HarborNavigation.Market);
        Check(Sell()==24&&save.coins==64&&save.hold.Count==0,"Walk along pier to market and sell for correct amount");
        yield return Photograph("mercado-interface");CloseDialog();
        Check(Sell()==0&&save.coins==64,"Empty sale cannot duplicate currency");CloseDialog();
        Store();var disk=HarborStorage.Load(SavePath,out bool recovered);
        Check(!recovered&&disk.coins==64&&disk.sold==1&&disk.hold.Count==0,"Progress survives actual disk save and reload");

        // Isolated regressions below may place objects directly; the complete journey above does not.
        string isolated=Path.Combine(Application.persistentDataPath,"porto-validation-storage.json");
        HarborStorage.Save(isolated,new HarborSave{coins=71});HarborStorage.Save(isolated,new HarborSave{coins=89});
        File.WriteAllText(isolated,"{broken");var backup=HarborStorage.Load(isolated,out recovered);
        Check(recovered&&backup.coins==71,"Corrupted primary save recovers last valid backup");
        HarborStorage.Save(isolated,backup);Check(HarborStorage.Load(isolated,out recovered).coins==71,"Recovered save can be saved again");
        File.WriteAllText(isolated,"{\"coins\":44,\"totalCatches\":0,\"sold\":0,\"hold\":[]}");
        Check(HarborStorage.Load(isolated,out recovered).potRemaining.Length==3,"Original v1 save remains compatible");
        var savedJourney=JsonUtility.ToJson(save);
        captain.gameObject.SetActive(false);walker.enabled=false;aboard=true;
        foreach(Vector3 edge in new[]{new Vector3(5.9f,.1f,-10),new Vector3(.6f,.1f,-15.2f)}) {
            vessel.position=edge;speed=0;
            Check(TryDock()&&TryBoard(),"Docking edge remains reboardable: "+edge);
        }
        vessel.position=new Vector3(.6f,.1f,-21);vessel.rotation=Quaternion.Euler(0,0,0);speed=6;
        for(int i=0;i<360;i++)SailStep(1,0,false,1f/60);
        Check(HarborNavigation.CanOccupy(vessel.position,vessel.eulerAngles.y)&&vessel.position.z<1.5f,"Hull cannot pass through waterfront");
        float blockedZ=vessel.position.z;for(int i=0;i<120;i++)SailStep(-1,0,false,1f/60);
        Check(vessel.position.z<blockedZ-1,"Reverse escapes collision without getting stuck");
        Check(!HarborNavigation.CanOccupy(new Vector3(-2.5f,0,-10),90),"Hull width rejects pier intersection");

        foreach(int fps in new[]{30,60,120}) {
            save=new HarborSave();vessel.position=HarborWorld.FishingSpots[1];speed=0;readyAt[1]=0;
            Check(BeginFishing(1),"Start controlled winch at "+fps+" Hz");
            for(int i=0;i<fps*35&&fishing;i++)TickFishing(1f/fps,tension<.63f);
            Check(save.hold.Count==1&&!fishing,"Winch succeeds at "+fps+" Hz");CloseDialog();
        }
        save=new HarborSave();readyAt[1]=0;Check(BeginFishing(1),"Start overload scenario");
        for(int i=0;i<1000&&fishing;i++)TickFishing(1f/60,true);
        Check(!fishing&&save.hold.Count==0&&readyAt[1]>Time.time,"Overload fails safely, grants no catch and sets retry delay");
        var cooldownSave=HarborStorage.Load(SavePath,out recovered);Check(cooldownSave.potRemaining[1]>0,"Retry delay is persisted");
        readyAt[1]=0;Check(BeginFishing(1),"Start cancellation scenario");CancelFishing();Check(save.hold.Count==0&&!fishing,"Cancellation grants no catch");
        for(int i=0;i<HoldCapacity;i++)save.hold.Add(new Catch("Siri",10));
        Check(!BeginFishing(1)&&save.hold.Count==HoldCapacity,"Full hold blocks ninth catch without discarding inventory");

        save=JsonUtility.FromJson<HarborSave>(savedJourney);Array.Clear(readyAt,0,3);
        vessel.position=HarborNavigation.Mooring;speed=0;Check(TryDock(),"Restore final playable port state");
        Store();overview=true;toast="";
        yield return new WaitForSecondsRealtime(1.5f);
        RenderPhoto(view,Path.Combine(ValidationDirectory,"porto-runtime.png"),1600,1000);
        yield return Photograph("porto-interface");
        Check(true,"Runtime screenshots captured for catch, market and harbor");
    }
}
}
