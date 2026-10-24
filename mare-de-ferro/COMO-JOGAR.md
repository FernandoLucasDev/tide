# Maré de Ferro — primeiro protótipo

Abra **Builds/Windows/MareDeFerro.exe** e clique em **Iniciar viagem**. Não é necessário abrir o Unity para jogar.

## Sua primeira viagem

1. Leia a orientação de Mara e feche o diálogo com **E** ou **Continuar**.
2. Caminhe pelo píer até a Andorinha. Quando aparecer a indicação, pressione **E** para embarcar.
3. Use **W/S** para o motor e **A/D** para o leme. Procure as bandeirolas amarelas na enseada.
4. Perto de uma boia, solte o motor e espere o barco reduzir. Pressione **E**.
5. Segure **Espaço** para puxar o covo. Solte para aliviar a tensão. Puxe na faixa verde até recolher a captura. **Q** cancela a operação.
6. Volte ao ponto de embarque ao lado do píer e reduza a velocidade. Use **E** para desembarcar.
7. Procure o balcão de peixe sob o toldo verde. Use **E** para vender a captura ao Tomé.

O terceiro recolhimento bem-sucedido encontra um caranguejo-de-panela. O quarto encontra um ermitão de porcelana. O ciclo se repete para permitir experimentar as capturas. Um covo recém-recolhido precisa ficar 35 segundos de molho.

## Comandos

| Comando | Ação |
|---|---|
| WASD ou setas | Caminhar / motor e leme no barco |
| Shift esquerdo | Correr em terra |
| E | Interagir, embarcar, desembarcar, conversar e continuar diálogo |
| Espaço | Puxar o guincho durante a pesca |
| Q | Cancelar recolhimento |
| C | Alternar vista próxima e panorama do porto |
| Roda do mouse | Ajustar distância da câmera |
| Tab | Abrir o diário e ver o porão |
| Esc | Fechar painel ou abrir pausa |
| F8 | Salvar fotografia do cenário sem interface |

## O que está nesta versão

Um porto 3D construído com geometria e materiais procedurais, iluminação de fim de tarde, mar animado, barcos flutuando, personagem controlável, navegação, três covos, minigame de tensão, quatro capturas, diálogos iniciais de Mara, Nina, Ivo e Tomé, venda e persistência de moedas e inventário.

**Etapa visual:** composição e cores iniciais inspiradas nas artes conceituais. Modelos, animações e texturas ainda são uma primeira construção procedural; a qualidade das ilustrações é a referência para as próximas iterações. As criaturas aparecem como descobertas no diálogo e inventário; ainda não têm modelos próprios no convés.

Frotas competitivas, temporadas, romances, demais mares, melhorias de barco e a campanha do Rei ainda pertencem ao brief. Esta versão permite avaliar a base jogável e a direção do cenário.

## Projeto e salvamento

- Projeto editável: **Unity/**. Abra a pasta pelo Unity Hub usando **Unity 6000.0.85f1**.
- Cena: **Assets/Scenes/PortoDasVelas.unity**. Pressione Play; o cenário é construído ao iniciar.
- Salvamento local: `%USERPROFILE%\AppData\LocalLow\Porto das Velas\Maré de Ferro\porto-save-v1.json`.
- Fotografias: subpasta **Fotos** no mesmo diretório de salvamento.
- Para recomeçar, feche o jogo e renomeie o arquivo de salvamento.
- Geração do executável: menu **Maré de Ferro → Criar cena e gerar Windows** no Unity.

Arte conceitual por geração de imagens; cenário deste protótipo por código próprio. Nenhum pacote de assets pago ou download externo é necessário.
