# Interface do prólogo — primeira aplicação da direção final

A interface usa as pinturas aprovadas como referência de atmosfera: azul-petróleo, latão, papel marfim e retratos com pinceladas visíveis. O cenário 3D continua procedural; esta entrega aplica a direção visual à interface, sem representar a finalização dos modelos e animações.

## Implementação

- Menu com a pintura de Porto das Velas já aprovada.
- HUD de localização, período narrativo do dia, moedas e covos guardados. O período acompanha as etapas do prólogo; não é um relógio de simulação contínua.
- Objetivo em cartão de papel, marcador espacial e progresso da tarefa junto à indicação de interação.
- Diálogos com seis retratos, nome e função; páginas longas têm rolagem. E ou o botão continuam a conversa.
- Diário com abas de viagem, capturas e pessoas. A pista da porcelana só aparece depois da descoberta.
- Pausa com salvar, retomar, sair e seleção EN/PT. Idioma padrão inglês; preferência é mantida entre sessões. A tradução cobre o prólogo e sua interface; a exploração livre legada ainda contém textos em português.
- Escala proporcional com verificação visual em 1440×900 e 1280×720. Fontes de sistema, com fallback para a fonte do Unity; nenhuma fonte comercial é redistribuída.

## Arte e reprodução

`Unity/Assets/Resources/UI/HarborCover.png` é uma cópia da arte aprovada `imagens/01-porto-das-velas.png`.

`Unity/Assets/Resources/UI/CrewPortraits.png` foi gerado pela ferramenta integrada de geração de imagens, em uma única imagem de 1536×1024, com recortes por coordenadas UV no jogo. Ordem: Mara, Celina, Dália; Ivo, Tomé, Nina. O original permanece na pasta de imagens geradas do Codex.

Direção do prompt: atlas de seis bustos em grade exata de três colunas e duas linhas, células quadradas iguais, todos centralizados com margem, pintura em gouache com pinceladas visíveis, luz âmbar quente e fundo azul-petróleo uniforme. Mara: mais velha, pele bronzeada, cabelos grisalhos, gorro creme e casaco verde. Celina: cerca de 45 anos, pele castanha, cabelos escuros cacheados e casaco azul-petróleo. Dália: cerca de 50 anos, pele oliva, cabelos acobreados e cardigan ferrugem. Ivo: cerca de 28 anos, pele castanha, expressão travessa e jaqueta ferrugem. Tomé: cerca de 42 anos, pele castanha escura, barba e avental creme. Nina: cerca de 27 anos, pele bronzeada, coque, camisa de trabalho azul-petróleo e óculos de proteção. Sem texto, molduras, marcas-d’água ou bordas entre as células.

Painéis, texturas leves de papel, ícones e botões são desenhados em código. As imagens não contêm texto de interface.

## Verificação

`--prologue-test` percorre fisicamente o prólogo, verifica traduções de todas as páginas, troca EN/PT, recursos carregados, persistência, tarefas e encerramento. Também captura menu, diário, retratos e pausa em duas resoluções. Os testes usam saves separados e não alteram a preferência de idioma do jogador. As capturas `verificacao/ui-*.png` e `verificacao/prologo-*.png` documentam a execução real; não são montagens.
