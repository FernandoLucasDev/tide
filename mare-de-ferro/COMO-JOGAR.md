# Maré de Ferro — prólogo 0.3

Abra **Builds/Windows/MareDeFerro.exe** e clique em **Começar prólogo** ou **Continuar viagem**. Não é necessário abrir o Unity para jogar. O atalho **JOGAR.bat** também abre esse executável.

## Seu primeiro dia

Siga o objetivo à esquerda e o marcador de distância no cenário. Você chega por uma vaga no Boa Esperança. Confira seu atraso no quadro de embarques, converse com Mara e encontre Dália na venda e Celina no píer. **E** interage e avança cada página dos diálogos.

No São Bento, Celina assume a navegação. Você caminha pelo convés: prepare isca na bancada, leve-a ao covo, espere oito segundos, recolha pelo guincho, examine a captura e guarde-a no porão. **Segure E próximo da estação indicada** para trabalhar. Soltar a tecla ou se afastar interrompe o trabalho ainda não concluído. Após o primeiro covo, prenda o caixote antes de continuar.

O terceiro covo traz um ermitão numa xícara com o emblema do Boa Esperança, o barco desaparecido. Guarde-o no viveiro. Volte com Celina, receba **36 moedas** de Tomé, participe do jantar e busque o rádio no fim do píer. Espere a sequência das três pancadas e leia o encerramento.

**Tab** abre o diário com a tarefa atual. Cada etapa concluída é salva automaticamente. Ao retomar, você aparece num ponto seguro da área; uma espera de covo ainda em andamento reinicia em oito segundos. O ermitão é uma lembrança protegida e não entra na venda de capturas.

## Exploração livre depois do prólogo

1. Após concluir o prólogo, a Andorinha fica disponível para navegar pela enseada.
2. Caminhe pelo píer até a Andorinha. Quando aparecer a indicação, pressione **E** para embarcar.
3. Use **W/S** para o motor e **A/D** para o leme. Procure as bandeirolas amarelas na enseada.
4. Perto de uma boia, segure **Espaço** para frear. Quando a indicação mudar para recolher, pressione **E**.
5. Segure **Espaço** para puxar o covo. Solte para aliviar a tensão. Puxe na faixa verde até recolher a captura. **Q** cancela a operação.
6. Use a marca **PORTO** na carta para voltar. Freie perto do embarque e use **E** para atracar. O barco fica amarrado numa posição acessível para a próxima viagem.
7. Procure o balcão de peixe sob o toldo verde. Use **E** para vender a captura ao Tomé.

Na pesca livre, caranguejo da enseada, siri de pedra, caranguejo-de-panela e ermitão de porcelana aparecem num ciclo de quatro capturas, contando também o histórico anterior. Um covo recém-recolhido precisa ficar 35 segundos de molho. Esta modalidade ainda usa o minigame de tensão anterior; o prólogo usa trabalho por posição no convés.

O porão comporta **oito capturas**. Quando estiver cheio, volte e venda antes de recolher outro covo. Pausa, diálogos e inventário suspendem o tempo da partida, inclusive a espera dos covos. Trocar para outra janela abre a pausa.

## Comandos

| Comando | Ação |
|---|---|
| WASD ou setas | Caminhar / motor e leme no barco |
| Shift esquerdo | Correr em terra |
| E | Interagir, embarcar, desembarcar, conversar e continuar diálogo |
| Segurar E | Trabalhar na estação indicada durante o prólogo |
| Espaço | Frear navegando / puxar o guincho durante a pesca |
| Q | Cancelar recolhimento |
| C | Alternar vista próxima e ampla; no mar, ambas acompanham o barco |
| Roda do mouse | Ajustar distância da câmera |
| Tab | Abrir o diário e ver o porão |
| Esc | Fechar painel ou abrir pausa |
| F8 | Salvar fotografia do cenário sem interface |

## O que está nesta versão

Prólogo com chegada pela balsa, investigação da oferta de emprego, encontros no porto, convés do São Bento, três covos de treinamento, amarração de carga, ermitão, pagamento, jantar e encerramento sonoro. Inclui diário e persistência por etapa. A exploração livre preserva navegação, guincho, capturas e mercado da versão anterior.

**Etapa visual:** composição e cores iniciais inspiradas nas artes conceituais. Modelos, animações e texturas ainda são uma primeira construção procedural; a qualidade das ilustrações é a referência para as próximas iterações. O treinamento mostra uma captura e a xícara em modelos simples na mesa. A travessia com Celina é narrativa; o convés de treinamento é estável.

Frotas competitivas, temporadas, romances, demais mares, melhorias de barco e a campanha do Rei ainda pertencem ao brief. Esta versão permite avaliar a base jogável e a direção do cenário.

## Projeto e salvamento

- Projeto editável: **Unity/**. Abra a pasta pelo Unity Hub usando **Unity 6000.0.85f1**.
- Cena: **Assets/Scenes/PortoDasVelas.unity**. Pressione Play; o cenário é construído ao iniciar.
- Salvamento local: `%USERPROFILE%\AppData\LocalLow\Porto das Velas\Maré de Ferro\porto-save-v1.json`.
- O arquivo `.bak` guarda a versão anterior. Se o principal estiver danificado, o jogo tenta recuperar essa cópia. Saves da versão 0.1 continuam compatíveis.
- Fotografias: subpasta **Fotos** no mesmo diretório de salvamento.
- Para recomeçar do zero, feche o jogo e renomeie tanto o arquivo de salvamento quanto sua cópia `.bak`. Guarde ambos se quiser recuperar a partida anterior.
- Geração do executável: menu **Maré de Ferro → Criar cena e gerar Windows** no Unity.

Arte conceitual por geração de imagens; cenário deste protótipo por código próprio. Nenhum pacote de assets pago ou download externo é necessário.
