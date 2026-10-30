using System;
using System.Collections.Generic;
using UnityEngine;

namespace MareDeFerro {
public partial class HarborGame {
    // Checkpoints are explicit: closing a conversation commits its outcome once.
    public enum Prologue { Arrival, Notice, Mara, Dalia, Celina, Board, Bait, LoadPot, Soak, Winch, Sort, Stow, Secure, Return, Deliver, Dinner, Radio, Ending, Complete }
    public bool PrologueActive => save.prologueStage < (int)Prologue.Complete;
    Prologue Chapter => (Prologue)save.prologueStage;
    bool OnTrainingDeck => Chapter>=Prologue.Bait && Chapter<=Prologue.Return;
    Vector3 PrologueSafeSpawn => OnTrainingDeck?DeckOrigin+new Vector3(0,1.65f,-5):HarborWorld.DockSpawn;
    static readonly Vector3 NoticePlace=new Vector3(-4.5f,1.55f,-10.8f),DeckOrigin=new Vector3(-85,0,-30);
    static readonly Vector3 DaliaPlace=new Vector3(13,1.55f,8),CelinaPlace=new Vector3(-4.5f,1.55f,-2.2f);
    static readonly Vector3 DinnerPlace=new Vector3(5,1.55f,6),RadioPlace=new Vector3(-4.5f,1.55f,-13.8f);
    Transform arrivalRoot,deckRoot,celina,dalia,tableRoot,questPot,trainingCatch,carriedItem,porcelain,radioProp;
    float taskProgress,soakTime,knockTime;
    int knocks;
    bool requireRelease;
    AudioSource storyAudio;
    AudioClip knockClip,workClip;
    readonly Queue<string> storyLines=new Queue<string>();
    Action afterStory;
    string PrologueTime => Chapter<=Prologue.Mara?"DIA 1 / DESEMBARQUE DA BALSA":Chapter<=Prologue.Board?"DIA 1 / MANHÃ NO PORTO":OnTrainingDeck?"DIA 1 / TARDE / NOVATO":Chapter<Prologue.Radio?"DIA 1 / ANOITECER":"DIA 1 / NOITE / TRÊS PANCADAS";

    void Story(Action complete,params string[] lines) {
        storyLines.Clear();foreach(var line in lines)storyLines.Enqueue(line);
        afterStory=complete;requireRelease=true;NextStoryLine();
    }
    void NextStoryLine(){string line=storyLines.Dequeue();int split=line.IndexOf('|');ShowDialog(line.Substring(0,split),line.Substring(split+1));}
    bool AdvanceStoryDialog() {
        if(storyLines.Count>0){NextStoryLine();return true;}
        if(afterStory==null)return false;
        var done=afterStory;afterStory=null;dialog="";ApplyModalClock();done();return true;
    }
    void PlacePlayer(Vector3 p) {
        walker.enabled=false;captain.gameObject.SetActive(true);captain.position=p;walker.enabled=true;
        aboard=false;speed=0;verticalVelocity=0;lastSafe=p;cameraVelocity=Vector3.zero;
        overview=false;view.transform.position=p+new Vector3(10,13.2f,-14)*zoom;
        view.transform.LookAt(p+Vector3.up*.7f);
    }
    void SetChapter(Prologue next) {
        save.prologueStage=(int)next;taskProgress=0;requireRelease=true;
        if(next==Prologue.Soak)soakTime=8;
        if(next==Prologue.Ending){knockTime=0;knocks=0;}
        SyncPrologueWorld();Store();
    }
    void ResumePrologue() {
        SyncPrologueWorld();
        if(Chapter<=Prologue.Mara)PlacePlayer(HarborWorld.DockSpawn);
        else if(OnTrainingDeck)PlacePlayer(DeckOrigin+new Vector3(0,1.65f,-5));
        else if(Chapter>=Prologue.Radio)PlacePlayer(RadioPlace+Vector3.forward*2);
        else PlacePlayer(HarborWorld.DockSpawn);
        if(Chapter==Prologue.Soak)soakTime=8;
        if(Chapter==Prologue.Ending){knocks=0;knockTime=0;}
        if(Chapter==Prologue.Arrival)Story(()=>SetChapter(Prologue.Notice),
            "UMA VAGA NO CAIS|A passagem de balsa levou suas últimas economias. No bolso, uma promessa: trabalho na temporada de caranguejos, a bordo do Boa Esperança.\n\nCapitão Raul. Apresentar-se no primeiro dia da temporada.",
            "PORTO DAS VELAS|A balsa chega. O Boa Esperança, não.\n\nVocê confere o endereço mais uma vez. Há um quadro de embarques no píer.\n\nWASD ou setas para caminhar. Aproxime-se do quadro e pressione E.");
        else Notify("Prólogo retomado • "+Objective());
    }
    Vector3 TaskTarget() {
        switch(Chapter) {
            case Prologue.Arrival:case Prologue.Notice:return NoticePlace;
            case Prologue.Mara:return HarborWorld.mara.position;
            case Prologue.Dalia:return DaliaPlace;
            case Prologue.Celina:return CelinaPlace;
            case Prologue.Board:return HarborNavigation.Landing;
            case Prologue.Bait:return DeckOrigin+new Vector3(-2.5f,1.6f,-4);
            case Prologue.LoadPot:return DeckOrigin+new Vector3(2.5f,1.6f,-4);
            case Prologue.Soak:case Prologue.Winch:return DeckOrigin+new Vector3(2.5f,1.6f,0);
            case Prologue.Sort:return DeckOrigin+new Vector3(-2.5f,1.6f,0);
            case Prologue.Stow:return DeckOrigin+new Vector3(-2.5f,1.6f,3);
            case Prologue.Secure:return DeckOrigin+new Vector3(2.5f,1.6f,3);
            case Prologue.Return:return DeckOrigin+new Vector3(0,1.6f,5);
            case Prologue.Deliver:return HarborNavigation.Market;
            case Prologue.Dinner:return DinnerPlace;
            default:return RadioPlace;
        }
    }
    string Objective() {
        switch(Chapter) {
            case Prologue.Arrival:case Prologue.Notice:return "Confira o Boa Esperança no quadro de embarques do píer.";
            case Prologue.Mara:return "Pergunte a Mara pelo barco que ofereceu o emprego.";
            case Prologue.Dalia:return "Busque um impermeável com Dália, à direita do mercado.";
            case Prologue.Celina:return "Fale com Celina no píer. Ela oferece trabalho.";
            case Prologue.Board:return "Encontre Celina no embarque do píer.";
            case Prologue.Bait:return "Prepare a isca na bancada à esquerda.";
            case Prologue.LoadPot:return "Leve a isca ao covo à direita.";
            case Prologue.Soak:return "O covo está de molho. Observe o mar e a tripulação.";
            case Prologue.Winch:return "Recolha o covo no guincho, sob orientação de Celina.";
            case Prologue.Sort:return "Examine a captura na mesa de triagem.";
            case Prologue.Stow:return save.trainingPots==2?"Leve o ermitão ao viveiro protegido.":"Guarde os caranguejos no porão à esquerda.";
            case Prologue.Secure:return "Prenda o caixote solto antes da próxima onda.";
            case Prologue.Return:return "Avise Celina: os três covos estão guardados.";
            case Prologue.Deliver:return "Encontre Tomé no balcão e receba sua diária.";
            case Prologue.Dinner:return "Sente-se à mesa ao lado do mercado.";
            case Prologue.Radio:return "Busque o rádio que Celina esqueceu no fim do píer.";
            case Prologue.Ending:return "Escute. O rádio está desligado.";
            default:return "Prólogo concluído. A enseada está aberta.";
        }
    }
    bool HoldTask => Chapter==Prologue.Bait||Chapter==Prologue.LoadPot||Chapter==Prologue.Winch||Chapter==Prologue.Stow||Chapter==Prologue.Secure;
    string ProloguePrompt() {
        if(Chapter==Prologue.Soak)return "Covo de molho • "+Mathf.CeilToInt(soakTime)+" s";
        if(Chapter==Prologue.Ending)return knocks==0?"O mar ficou quieto.":new string('•',knocks)+"  Uma pancada sob o píer.";
        float distance=HarborNavigation.Distance(captain.position,TaskTarget());
        if(distance>2)return "Objetivo • "+Mathf.CeilToInt(distance)+" m • siga o marcador";
        return HoldTask?"Segure [E] para trabalhar • "+Mathf.RoundToInt(taskProgress*100)+"%":"[E]  "+(Chapter==Prologue.Board?"Embarcar com Celina":Chapter==Prologue.Dinner?"Sentar para jantar":Chapter==Prologue.Radio?"Examinar o rádio":"Interagir");
    }
    void TickPrologue(float dt,bool held,bool pressed) {
        if(!PrologueActive||!started||paused||inventory||dialog!=""||dt<=0)return;
        if(!held)requireRelease=false;
        if(Chapter==Prologue.Soak) {
            soakTime=Mathf.Max(0,soakTime-dt);questPot.localPosition=new Vector3(3.7f,Mathf.Lerp(-1,1.7f,soakTime/8),-3);
            if(soakTime<=0)SetChapter(Prologue.Winch);return;
        }
        if(Chapter==Prologue.Ending) {
            knockTime+=dt;
            if(knocks<3&&knockTime>=1+knocks*1.4f){storyAudio.PlayOneShot(knockClip,.85f);knocks++;}
            if(knocks==3&&knockTime>5.2f)Story(()=>{SetChapter(Prologue.Complete);PlacePlayer(HarborWorld.DockSpawn);Notify("Prólogo concluído • a Andorinha está disponível para explorar a enseada.");},
                "MARA|Não toca nesse botão.\n\n[Ela segura seu pulso. Pela primeira vez, você percebe que sua mão está tremendo.]",
                "CELINA|Mara... esse rádio nem está ligado.\n\n[Do mercado, Tomé chama vocês. O jantar ainda está quente. Ninguém responde.]",
                "UM LUGAR À MESA|Você chegou sem conhecer ninguém. Agora alguém espera sua volta.\n\nPRÓLOGO CONCLUÍDO\nNovato do São Bento • lembrança: ermitão da xícara azul.\nAmanhã começa a primeira temporada.");
            return;
        }
        if(requireRelease)return;
        if(HarborNavigation.Distance(captain.position,TaskTarget())>2){taskProgress=0;return;}
        if(HoldTask) {
            taskProgress=held?Mathf.Clamp01(taskProgress+dt/(Chapter==Prologue.Winch?3:1.6f)):0;
            if(Chapter==Prologue.Winch)questPot.localPosition=new Vector3(3.7f,Mathf.Lerp(-1,1.7f,taskProgress),-3);
            if(taskProgress>=1){storyAudio.PlayOneShot(workClip,.35f);DoPrologueAction();}
        } else if(pressed)DoPrologueAction();
    }
    void DoPrologueAction() {
        switch(Chapter) {
            case Prologue.Notice:
                Story(()=>SetChapter(Prologue.Mara),"QUADRO DE EMBARQUES|BOA ESPERANÇA — RETORNO ATRASADO HÁ TRÊS DIAS.\n\nNa sua oferta de emprego, três ondas azuis cercam o nome do barco. No quadro, alguém riscou a hora de chegada. Não escreveu outra.","UMA VOZ NO CAIS|«Você é a pessoa que Raul contratou?»\n\nA mulher junto à entrada do píer olha para o papel na sua mão.\n\n«Sou Mara. Vem cá. Precisamos conversar.»");break;
            case Prologue.Mara:
                Story(()=>SetChapter(Prologue.Dalia),
                    "MARA|A vaga era de verdade. Raul estava procurando alguém para começar do zero.\n\nO barco não voltou da última viagem. Estão procurando, mas ninguém ouviu mais nada. Para você era um emprego. Para nós... são pessoas que faltam à mesa.",
                    "MARA|Gastou tudo para chegar, não foi? Tem uma cama na minha casa. Dália empresta um impermeável.\n\nCelina está no píer. Talvez consiga te levar numa saída curta. Não é o trabalho que prometeram, mas é um começo.");break;
            case Prologue.Dalia:
                Story(()=>SetChapter(Prologue.Celina),"DÁLIA|Esse impermeável vai ficar grande. Melhor sobrar pano do que faltar calor.\n\nNão vou pôr na sua conta. Primeiro você aprende os nossos nomes. Depois a gente conversa sobre dinheiro.","DÁLIA|Celina está no píer, de casaco azul. Se ela oferecer trabalho, pode confiar.\n\nE apareça no jantar. Aqui a gente percebe quando uma cadeira fica vazia.");break;
            case Prologue.Celina:
                Story(()=>SetChapter(Prologue.Board),"CELINA|Mara disse que você quer trabalhar. Hoje são três covos, aqui na enseada. Eu conduzo o São Bento.\n\nVocê prepara as iscas e ajuda com as capturas. Uma tarefa de cada vez. Ninguém nasce sabendo.","CELINA|Trinta e seis moedas pela saída, mais jantar. Se precisar parar, avise.\n\nO leme fica comigo. O seu primeiro comando é sobre as próprias mãos: saiba onde elas estão quando um cabo se move. Encontre-me no embarque.");break;
            case Prologue.Board:
                Story(()=>{SetChapter(Prologue.Bait);PlacePlayer(DeckOrigin+new Vector3(0,1.65f,-5));},"SÃO BENTO • PRIMEIRA SAÍDA|Celina solta as amarras. O porto fica para trás enquanto ela leva o barco ao pesqueiro.\n\nNo convés: bancada de isca à esquerda, covo à direita. Aproxime-se da bancada e segure E para preparar a primeira isca.");break;
            case Prologue.Bait:SetChapter(Prologue.LoadPot);Notify("Isca preparada. Leve-a ao covo do outro lado do convés.");break;
            case Prologue.LoadPot:
                SetChapter(Prologue.Soak);Notify(save.trainingPots==0?"Celina lança o covo. Ivo no rádio: «Cuida desse novato, Celina!»":"Covo lançado. O mar leva alguns instantes para responder.");break;
            case Prologue.Winch:SetChapter(Prologue.Sort);Notify("Covo a bordo. A captura está na mesa de triagem.");break;
            case Prologue.Sort:
                if(save.trainingPots==2)Story(()=>SetChapter(Prologue.Stow),"TRÊS ONDAS AZUIS|Uma pinça aparece pela alça de uma xícara. Sob o sal, você reconhece três ondas azuis: o emblema da sua oferta de emprego.\n\nBOA ESPERANÇA.\n\nO barco não chegou. Uma de suas xícaras, sim.","CELINA|Pode ter caído de bordo em outra viagem. Ainda não sabemos. Vou marcar o lugar deste covo e avisar Mara.\n\nO pequeno fica no viveiro. Guarde com cuidado. Quanto ao nome, eu evitaria Capitão: Ivo já se acha demais.");
                else Story(()=>SetChapter(Prologue.Stow),"TRIAGEM DO COVO|"+(save.trainingPots==0?"Um caranguejo da enseada e dois muito pequenos. Celina devolve os pequenos à água e mostra como segurar o maior sem machucá-lo.":"Um siri de pedra se agarra à grade. Você espera, muda a pegada e ele solta. Desta vez, Celina não precisa corrigir suas mãos.")+"\n\nLeve a captura ao compartimento à esquerda, próximo da cabine.");break;
            case Prologue.Stow:
                save.trainingPots++;save.totalCatches++;
                if(save.trainingPots==3)save.porcelainMemento=true;
                SetChapter(save.trainingPots==3?Prologue.Return:save.trainingPots==1?Prologue.Secure:Prologue.Bait);
                Notify(save.trainingPots==1?"Celina: «Antes do próximo, prende aquele caixote. Vem uma onda.»":"Captura guardada • "+save.trainingPots+" / 3 covos");break;
            case Prologue.Secure:
                Story(()=>SetChapter(Prologue.Bait),"CELINA|Isso. Duas voltas, testa o nó.\n\n[A onda chega. O caixote segura. Celina olha para ele, depois para você.]\n\nUm bom dia de pesca começa com todo mundo continuando de pé.");break;
            case Prologue.Return:
                Story(()=>{SetChapter(Prologue.Deliver);PlacePlayer(HarborNavigation.Landing);},"CELINA|Três covos. Nenhum dedo perdido. E um passageiro que não estava na lista.\n\nPode respirar. Eu levo a gente para casa. Entregamos o pescado ao Tomé e você recebe sua diária.");break;
            case Prologue.Deliver:
                Story(()=>{save.coins+=36;save.sold+=2;SetChapter(Prologue.Dinner);},"TOMÉ|Celina já acertou a entrega. Duas boas capturas e um ermitão que claramente não está no cardápio.\n\nAqui estão suas 36 moedas. A mesa está ali do lado. Hoje o jantar é por minha conta.");break;
            case Prologue.Dinner:
                Story(()=>SetChapter(Prologue.Radio),"UM LUGAR À MESA|Dália puxa uma cadeira. Tomé coloca um prato diante de você antes de perguntar se está com fome. O ermitão observa a mesa de dentro do viveiro.","IVO|Ouvi dizer que a nova tripulação trouxe a própria xícara. Amanhã vai pedir sobremesa?\n\nCELINA: Trouxe três covos. Você trouxe alguma coisa além de conversa?",
                    "MARA|[Ela ri, até notar as três ondas na porcelana.]\n\nRaul mandou pintar um jogo inteiro dessas. Disse que até café ruim merecia uma xícara bonita.\n\n[Celina entrega a posição do covo. Mara guarda o papel ao lado da sua oferta de emprego.]",
                    "CELINA|Você tem lugar no São Bento, se quiser voltar amanhã.\n\nEsqueci o rádio portátil no fim do píer. Busca para mim? Depois disso, acabou o trabalho. Prometo.");break;
            case Prologue.Radio:SetChapter(Prologue.Ending);break;
        }
    }
}
}
