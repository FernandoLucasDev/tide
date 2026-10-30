# Uma vaga no cais — prólogo jogável 0.3

Implementação na interface existente, com geometria procedural e os mesmos painéis, fontes e controles do protótipo. A bíblia narrativa continua definindo a campanha; esta entrega cobre seu prólogo.

**Idioma:** inglês é o padrão do prólogo e da nova interface, com português disponível nos botões EN/PT do menu inicial e da pausa. As falas do prólogo estão traduzidas; não há dublagem. A exploração livre legada ainda contém textos em português. Veja `UI-DIRECAO.md` para o escopo visual e os assets.

## Percurso

1. Desembarcar da balsa com uma oferta de emprego e descobrir no quadro de embarques que o Boa Esperança está desaparecido.
2. Conversar com Mara e aceitar seu acolhimento.
3. Buscar um impermeável de trabalho com Dália.
4. Conhecer Celina e combinar o primeiro trabalho: 36 moedas e jantar.
5. Embarcar no São Bento, conduzido por Celina.
6. Caminhar até a bancada, preparar uma isca e levá-la ao covo.
7. Esperar o lançamento, ajudar no guincho, examinar e guardar a captura.
8. Prender uma carga antes de continuar o trabalho.
9. Completar os três covos; no último, encontrar o ermitão numa xícara com o emblema do Boa Esperança. Celina registra o local da descoberta; a peça sozinha não confirma um naufrágio.
10. Retornar ao porto, receber o pagamento e participar do jantar com a tripulação.
11. Buscar o rádio no píer, ouvir três pancadas e presenciar a reação de Mara.
12. Concluir o prólogo e liberar a exploração existente com a Andorinha.

## Interações e persistência

- WASD/setas movem o personagem. E conversa ou examina. Segurar E executa o trabalho quando o personagem está próximo da estação correta.
- Soltar E ou se afastar interrompe uma tarefa ainda não concluída. O jogador precisa caminhar entre as estações; não há seleção remota por menu.
- Celina pilota a primeira saída. A travessia é uma transição narrativa entre porto e convés, sem controle de navegação pelo novato. O convés de treinamento é estável; ondas físicas e equilíbrio dinâmico ficam para uma próxima etapa.
- Cada etapa concluída é salva. Uma tarefa interrompida pode ser refeita; a espera do covo reinicia em oito segundos ao retomar esse ponto. A posição exata é substituída por um ponto seguro da área.
- Diálogos não lidos até o fim são repetidos ao interagir novamente. A introdução e a sequência final são retomadas automaticamente em seus respectivos pontos.
- O progresso anterior de moedas e capturas é preservado. Saves antigos começam o novo prólogo, sem apagar o inventário existente.
- Os dois crustáceos comerciais do treinamento são contabilizados na entrega. A diária é paga uma única vez. O ermitão fica registrado como lembrança permanente e não pode ser vendido.
- Pausa, diálogos e diário suspendem espera, tarefas e sequência final. Os sons das pancadas são sintetizados localmente, sem assets externos.

## Verificação

O executável aceita `--prologue-test` para executar a jornada com as mesmas funções de movimento, interação e diálogo usadas pelo jogador. Usa um arquivo de salvamento exclusivo de teste. O relatório fica em `verificacao/prologue-test.txt`; as capturas visuais possuem prefixo `prologo-`.

O teste cobre alcance físico das estações, pausas, interrupção de tarefas, três covos, leitura dos diálogos, retomada de isca transportada, persistência de cada etapa, três pancadas, pagamento único, lembrança protegida e desbloqueio da Andorinha. `--smoke-test` preserva a verificação anterior de navegação, pesca livre, mercado e salvamentos.

Não há dublagem, animações finais de trabalho, personalização de personagem ou primeiro capítulo completo nesta entrega. A pesca livre após o prólogo mantém o sistema de tensão anterior; o trabalho do prólogo usa a nova interação por posição no convés.
