# Verificação do protótipo

O relatório **build-report.txt** registra o resultado da compilação Windows no Unity 6000.0.85f1.

O executável aceita `--smoke-test` para executar um percurso automatizado que exercita:

- Inicialização do mundo e embarque.
- Bloqueio da pesca fora do alcance e com motor rápido.
- Recolhimento com controle de tensão e inclusão da captura no porão.
- Intervalo obrigatório antes de recolher o mesmo covo.
- Retorno, desembarque, venda e atualização de moedas.
- Serialização e leitura do estado de progresso.
- Presença de colisores de piso no píer e no mercado.

O teste chama as rotinas de jogo diretamente; a navegação manual por teclado pode ser experimentada no executável. O modo de teste usa um arquivo de salvamento separado do jogador.

**runtime-test.txt** é escrito após as verificações. **porto-runtime.png** registra a câmera do cenário; **porto-interface.png** registra a tela com interface no executável. **porto-editor.png** é uma renderização auxiliar feita no editor. Essas imagens são do protótipo, não artes conceituais.
