# Texturas do prólogo

Método: ferramenta integrada `image_gen`, geração de novos assets. As imagens são copiadas para Resources do Unity; não dependem da pasta de imagens do Codex durante a execução.

## Madeira

Destino: `Unity/Assets/Resources/Art/WeatheredWood.png`.

Prompt:

> Square seamless tileable game albedo texture, flat orthographic scan of aged salt-weathered timber grain, continuous wood surface without plank divisions. Long fine vertical grain, subtle knots, worn pale gray-brown fibers, restrained hand-painted gouache realism matching a warm Mediterranean fishing harbor adventure. Neutral diffuse illumination, no cast shadows, no directional lighting, no perspective, no objects, no border, no text. Low contrast pale neutral gray tones so the game can tint this material, intricate fine surface detail, not dark, no large swirls. Texture fills whole square edge to edge.

## Reboco

Destino: `Unity/Assets/Resources/Art/CoastalPlaster.png`.

Prompt:

> Square seamless tileable game albedo texture, flat orthographic scan of old coastal lime plaster. Pale ivory and light warm gray, worn layers of limewash, small irregular exposed patches, delicate salt weathering, fine hairline cracks very sparse, restrained hand-painted gouache realism for Mediterranean fishing village. Neutral diffuse illumination, no cast shadows or directional lighting, no perspective, no bricks, no doors or windows, no objects, no border, no text. Low contrast, light neutral material so game can tint it. Surface detail evenly distributed, no focal point, fills square edge to edge.

Os prompts pedem repetição contínua; isso não garante continuidade matemática. A inspeção relevante é a aplicação nas malhas do executável. Cores e brilho são ajustados pelo material do jogo, sem incorporar sombras da cena nas imagens.
