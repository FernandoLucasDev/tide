# Base jogável — versão 0.2

Esta etapa concentra a validação na viagem **cais → barco → covo → porto → mercado**.

## Correções e ajustes

- Atracar agora amarra a Andorinha na posição do embarque. Antes, desembarcar na extremidade da área permitida podia deixar o barco fora do alcance para entrar novamente.
- Embarque exige proximidade da passarela. Venda exige estar em terra e perto do mercado.
- O barco tem freio no Espaço; colisões consideram comprimento e largura do casco. Ré permite se afastar de um obstáculo.
- A câmera ampla acompanha o barco no mar, e a carta informa porto, covos e direção da embarcação.
- Pausa, diálogos e inventário suspendem o tempo da partida. Sair da janela abre a pausa.
- O porão tem oito espaços. Capturas repetidas e vendas vazias não duplicam recompensas.
- O save usa substituição de arquivo com backup, recupera uma cópia válida se o principal estiver danificado e preserva a espera dos covos. O formato anterior continua aceito.
- As instruções do rodapé mudam entre navegação e guincho. O diário permite rolar capturas de saves anteriores que excedam o novo limite.

## Validação

O teste da viagem completa desloca personagem e barco pelas mesmas rotinas usadas nos controles. Ele atravessa o cenário com os colisores ativos. Depois, cenários isolados verificam atracação nos limites da área, sobrecarga, cancelamento, pausa, limite do porão, salvamento e pesca a passos de 30/60/120 Hz.

Resultados e imagens reais ficam em [verificacao](verificacao/LEIA-ME.md). O teste usa saves próprios e não altera o progresso do jogador.

## Avaliação ao jogar

Com a base técnica verificada, os próximos critérios são a sensação do leme, a facilidade de achar o caminho de volta e o ritmo do guincho. Esses pontos dependem de experimentar a versão. O trabalho visual detalhado e a expansão para temporadas continuam nas etapas seguintes.
