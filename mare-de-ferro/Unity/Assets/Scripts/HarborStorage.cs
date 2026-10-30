using System;
using System.IO;
using UnityEngine;

namespace MareDeFerro {
public static class HarborStorage {
    static HarborSave Parse(string json) {
        if(string.IsNullOrWhiteSpace(json)||!json.TrimStart().StartsWith("{")||!json.Contains("\"coins\""))throw new InvalidDataException("Arquivo incompleto.");
        var data=JsonUtility.FromJson<HarborSave>(json);
        if(data==null||data.coins<0||data.totalCatches<0||data.sold<0)throw new InvalidDataException("Progresso inválido.");
        if(data.prologueStage<0||data.prologueStage>(int)HarborGame.Prologue.Complete||data.trainingPots<0||data.trainingPots>3)throw new InvalidDataException("Etapa do prólogo inválida.");
        data.version=3;
        if(data.hold==null)data.hold=new System.Collections.Generic.List<Catch>();
        foreach(var item in data.hold)if(item==null||string.IsNullOrWhiteSpace(item.name)||item.value<0)throw new InvalidDataException("Captura inválida.");
        if(data.potRemaining==null||data.potRemaining.Length!=3)data.potRemaining=new float[3];
        for(int i=0;i<3;i++)if(float.IsNaN(data.potRemaining[i])||float.IsInfinity(data.potRemaining[i])||data.potRemaining[i]<0)data.potRemaining[i]=0;
        return data;
    }
    public static HarborSave Load(string path,out bool recovered) {
        recovered=false;
        if(!File.Exists(path)&&!File.Exists(path+".bak"))return new HarborSave();
        try{return Parse(File.ReadAllText(path));}
        catch(Exception primary) {
            if(!File.Exists(path+".bak"))throw new InvalidDataException("Não foi possível ler o progresso.",primary);
            var data=Parse(File.ReadAllText(path+".bak"));recovered=true;return data;
        }
    }
    public static void Save(string path,HarborSave data) {
        string json=JsonUtility.ToJson(data,true);Parse(json);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string temporary=path+".tmp";File.WriteAllText(temporary,json);
        if(File.Exists(path)) {
            bool healthy=true;try{Parse(File.ReadAllText(path));}catch{healthy=false;}
            File.Replace(temporary,path,healthy?path+".bak":null);
        } else File.Move(temporary,path);
    }
}
}
