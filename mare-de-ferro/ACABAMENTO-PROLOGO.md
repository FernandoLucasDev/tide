# Acabamento visual do prólogo — passagem 1

Esta entrega inicia o acabamento do mundo jogável. Ainda não representa a qualidade final das pinturas aprovadas. A campanha permanece restrita ao conteúdo já implementado, para que o prólogo seja a área de validação visual.

## Mudanças implementadas

- Personagens com malhas contínuas de roupas, proporções revistas, rostos, cabelos e acessórios distintos. Mara usa gorro claro; Celina, Dália e Nina têm cabelos presos; Tomé tem barba e avental. O jogador usa roupa escura e equipamento amarelo.
- Animação de passos, braços e respiração baseada no movimento real, sem deslocar os colisores. Durante o trabalho, o personagem se orienta para a estação e movimenta os braços. A postura de transporte acompanha a isca.
- Câmera padrão mais próxima e um pouco mais baixa, preservando zoom e vista ampla.
- Texturas de madeira salgada e reboco gasto geradas para esta passagem, cores menos saturadas, telhas levemente irregulares e rochas com normais suavizadas. Origem e prompts em `PROMPTS-TEXTURAS.md`.
- Floreiras, vegetação nas fachadas, redes secando e cabos enrolados no cais. Esses detalhes não bloqueiam a circulação.
- Luz diurna menos alaranjada, janelas menos emissivas e tratamento de altas luzes no pós-processamento. A mudança narrativa para a noite continua ativa.
- Água com malha mais densa, ondulação revista e menos espuma procedural. Não há reflexão planar nem simulação oceânica física nesta passagem.
- São Bento com casco visual curvo, frisos, corrimãos, janela emoldurada, porta, escapes, boia salva-vidas, barris e tambor do guincho. A plataforma de treinamento mantém seus limites de colisão.
- Rótulos do prólogo menores e voltados para a câmera, incluindo quando os NPCs mudam de orientação.

## Verificação e limites

O percurso `--prologue-test` exercita caminhada, estações, pausas, captura, entrega, jantar, encerramento e persistência. Também verifica a presença do novo personagem e a resposta da animação ao trabalho. `art-trabalho.png`, `ui-interacao.png` e `prologo-conves.png` mostram o executável real.

As malhas e animações desta passagem são produzidas em código; madeira e reboco usam imagens geradas. Ainda faltam modelos artísticos mais elaborados, animações específicas para cada ferramenta, efeitos costeiros e uma passagem de composição ambiental para aproximar o conjunto do detalhamento das referências. A travessia de Celina continua narrativa. Não há dublagem nem expansão dos mares.
