# Verificação do protótipo

O relatório **build-report.txt** registra o resultado da compilação Windows no Unity 6000.0.85f1.

## Acabamento 3D do prólogo

`art-trabalho.png` mostra a postura de trabalho do novo personagem; `ui-interacao.png` mostra o cais e `prologo-conves.png` mostra o São Bento. O percurso automatizado verifica também a presença das texturas no executável, a malha de roupa e a resposta da animação durante uma tarefa. A inspeção visual inclui a orientação dos rótulos, o lado externo do casco e a legibilidade noturna. Esta é a primeira passagem de acabamento descrita em `ACABAMENTO-PROLOGO.md`, ainda aquém do detalhamento das pinturas de referência.

## Interface do prólogo

As capturas `ui-menu`, `ui-mara`, `ui-diario`, `ui-tripulacao`, `ui-portugues`, `ui-tripulacao-720p` e `ui-pausa-720p` mostram a aplicação da direção visual ao executável. Foram inspecionados retratos, leitura, recortes e disposição em 1440×900 e 1280×720. O teste do prólogo verifica também os assets, o idioma padrão, a troca EN/PT e a tradução de cada página de história. A troca de resolução revelou um erro no pivô da bússola; a transformação foi corrigida para preservar a escala da interface.

## Prólogo 0.3

`--prologue-test` percorre a chegada ao cais, os encontros no porto, o convés e o encerramento usando o controlador real de caminhada e as funções de interação do jogo. **prologue-test.txt** registra os resultados. O teste usa `porto-prologue-test.json`, separado do progresso do jogador.

- Cada estação precisa ser alcançável com os colisores reais.
- As tarefas respeitam alcance, tecla mantida, interrupção e pausa.
- Três covos produzem duas capturas comerciais e uma lembrança protegida.
- Etapas e lembrança sobrevivem à gravação/leitura em disco; uma retomada no convés restaura a isca transportada.
- O pagamento ocorre uma única vez; saves antigos conservam seu dinheiro.
- Uma regressão isolada simula queda do píer e verifica recuperação num ponto seguro do porto.
- As três pancadas precedem o encerramento, que libera a navegação da Andorinha.

**prologo-chegada.png**, **prologo-conves.png**, **prologo-ermitao.png**, **prologo-jantar.png** e **prologo-tres-pancadas.png** mostram a interface atual no executável. O teste automatiza os comandos, mas a avaliação de ritmo, leitura e sensação de trabalho continua sendo feita ao jogar.

## Base e exploração livre

O executável aceita `--smoke-test` para executar um percurso automatizado que exercita:

- Caminhada da posição inicial ao embarque usando o CharacterController e os colisores reais.
- Navegação até a boia e retorno ao porto usando aceleração, leme e colisão do casco, sem teletransportar o barco durante a viagem.
- Bloqueio da pesca fora do alcance e com motor rápido.
- Recolhimento com controle de tensão e inclusão da captura no porão.
- Intervalo obrigatório antes de recolher o mesmo covo.
- Retorno, desembarque, venda e atualização de moedas.
- Caminhada do desembarque ao mercado, venda e bloqueio da venda à distância.
- Escrita e leitura de progresso em disco, recuperação de arquivo danificado e compatibilidade com saves antigos.
- Pausa, inventário, limite do porão, cancelamento e sobrecarga do guincho.
- Pesca a passos de 30, 60 e 120 Hz.
- Atracação nos limites da área, reembarque e marcha à ré após colisão.

O teste fornece comandos às mesmas rotinas de movimento usadas pelo teclado. Isso verifica deslocamento e colisões, mas a sensação dos controles ainda deve ser avaliada jogando. Os cenários isolados de regressão, depois da viagem completa, podem posicionar objetos diretamente. O teste usa arquivos de salvamento separados do jogador e retorna código 1 com relatório em caso de falha.

**runtime-test.txt** registra cada verificação. **porto-runtime.png** registra a câmera do cenário; **porto-interface.png**, **pesca-interface.png**, **captura-interface.png** e **mercado-interface.png** mostram o executável. **porto-editor.png** é uma renderização auxiliar feita no editor. Essas imagens são do protótipo, não artes conceituais.
