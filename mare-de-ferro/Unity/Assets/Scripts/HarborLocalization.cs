using System;
using System.Collections.Generic;
using UnityEngine;

namespace MareDeFerro {
public partial class HarborGame {
    [Serializable] class TranslationEntry { public string source,english; }
    [Serializable] class TranslationCatalog { public TranslationEntry[] entries; }
    Dictionary<string,string> translations;
    readonly Dictionary<TextMesh,string> originalLabels=new Dictionary<TextMesh,string>();
    void LoadTranslations() {
        if(translations!=null)return;
        translations=new Dictionary<string,string>();
        var asset=Resources.Load<TextAsset>("UI/English");
        if(asset)foreach(var entry in JsonUtility.FromJson<TranslationCatalog>(asset.text).entries)translations[entry.source]=entry.english;
        string[,] labels={
            {"OFICINA NAVAL","SHIP WORKSHOP"},{"PEIXE FRESCO","FRESH FISH"},
            {"UMA VAGA NO CAIS","A PLACE ON THE DOCK"},{"UM LUGAR À MESA","A PLACE AT THE TABLE"},
            {"QUADRO DE EMBARQUES","DEPARTURES BOARD"},{"UMA VOZ NO CAIS","A VOICE ON THE DOCK"},
            {"SÃO BENTO • PRIMEIRA SAÍDA","SÃO BENTO • FIRST TRIP"},{"TRÊS ONDAS AZUIS","THREE BLUE WAVES"},{"TRIAGEM DO COVO","SORTING THE CATCH"},
            {"EMBARQUES","DEPARTURES"},{"BANCADA DE ISCA","BAIT BENCH"},{"TRIAGEM","SORTING TABLE"},{"PORÃO / VIVEIRO","HOLD / LIVEWELL"},
            {"AMARRAR CARGA","SECURE CARGO"},{"GUINCHO","WINCH"},{"ABASTECER COVO","BAIT THE POT"},{"JANTAR","SUPPER"},
            {"Isca preparada. Leve-a ao covo do outro lado do convés.","Bait ready. Carry it to the pot across the deck."},
            {"Celina lança o covo. Ivo no rádio: «Cuida desse novato, Celina!»","Celina sets the pot. Ivo on the radio: ‘Look after that greenhorn, Celina!’"},
            {"Covo lançado. O mar leva alguns instantes para responder.","Pot set. Give the sea a moment to answer."},
            {"Covo a bordo. A captura está na mesa de triagem.","Pot aboard. The catch is on the sorting table."},
            {"Celina: «Antes do próximo, prende aquele caixote. Vem uma onda.»","Celina: ‘Before the next one, secure that crate. A wave is coming.’"},
            {"Prólogo concluído • a Andorinha está disponível para explorar a enseada.","Prologue complete • explore the cove aboard the Andorinha."},
            {"Progresso salvo.","Progress saved."}
        };
        for(int i=0;i<labels.GetLength(0);i++)translations[labels[i,0]]=labels[i,1];
    }
    string Localize(string source) {
        if(!english||string.IsNullOrEmpty(source))return source;
        LoadTranslations();
        if(translations.TryGetValue(source,out var translated))return translated;
        if(source.StartsWith("Prólogo retomado"))return "Voyage resumed • "+UiObjective();
        if(source.StartsWith("Captura guardada"))return "Catch stowed • "+save.trainingPots+" / 3 pots";
        return source;
    }
    void RefreshWorldLabels() {
        foreach(var mesh in FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None)) {
            if(!originalLabels.ContainsKey(mesh))originalLabels[mesh]=mesh.text;
            mesh.text=Localize(originalLabels[mesh]);
        }
    }
    string UiObjective() {
        if(!english)return Objective();
        if(Chapter==Prologue.Stow&&save.trainingPots==2)return "Take the hermit crab to the protected livewell.";
        string[] goals={
            "Look for the Boa Esperança on the pier’s departures board.",
            "Look for the Boa Esperança on the pier’s departures board.",
            "Ask Mara about the boat that offered you a job.",
            "Collect a work coat from Dália, right of the market.",
            "Speak to Celina on the pier. She has work for you.",
            "Meet Celina at the boarding point.",
            "Prepare bait at the bench on the left.",
            "Carry the bait to the pot on the right.",
            "Let the pot soak. Watch the sea and your crew.",
            "Haul the pot at the winch, with Celina’s guidance.",
            "Examine the catch at the sorting table.",
            "Stow the crabs in the hold on the left.",
            "Secure the loose crate before the next wave.",
            "Tell Celina: all three pots are safely stowed.",
            "Meet Tomé at the counter and collect your pay.",
            "Take a seat beside the market.",
            "Fetch Celina’s radio from the end of the pier.",
            "Listen. The radio is switched off.",
            "Prologue complete. The cove is open to explore."
        };
        return goals[Mathf.Clamp(save.prologueStage,0,goals.Length-1)];
    }
    string UiInteraction(bool close) {
        if(Chapter==Prologue.Soak)return T("Pot soaking","Covo de molho")+" • "+Mathf.CeilToInt(soakTime)+" s";
        if(Chapter==Prologue.Ending)return knocks==0?T("The sea has gone quiet.","O mar ficou quieto."):new string('•',knocks)+T("  A knock beneath the pier.","  Uma pancada sob o píer.");
        if(!close)return T("Follow the marker to your next task","Siga o marcador até sua próxima tarefa");
        if(HoldTask)return T("Hold to work","Segure para trabalhar")+" • "+Mathf.RoundToInt(taskProgress*100)+"%";
        switch(Chapter){
            case Prologue.Notice:return T("Read the departures board","Ler o quadro de embarques");
            case Prologue.Board:return T("Board with Celina","Embarcar com Celina");
            case Prologue.Sort:return T("Examine the catch","Examinar a captura");
            case Prologue.Dinner:return T("Sit down for supper","Sentar para jantar");
            case Prologue.Radio:return T("Examine the radio","Examinar o rádio");
            default:return T("Talk","Conversar");
        }
    }
}
}
