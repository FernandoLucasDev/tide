# Verificação do protótipo

O relatório **build-report.txt** registra o resultado da compilação Windows no Unity 6000.0.85f1.

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
