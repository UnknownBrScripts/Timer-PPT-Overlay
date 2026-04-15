# Timer PPT Standalone (Overlay)

Aplicativo Windows (WinForms) que exibe um timer em formato de overlay transparente na tela, com configurações persistentes, operação via cliques do mouse e controle via bandeja do sistema.

README principal: [../README.md](../README.md)

## Principais recursos

- Overlay sempre visível (TopMost) com transparência por cor (chroma key)
- Contagem regressiva com **contagem negativa** após zerar (em vermelho)
- Controles por mouse:
  - Clique esquerdo: iniciar/pausar
  - Duplo clique esquerdo: reset
  - Clique direito: menu
- Ícone na bandeja do sistema (tray) com Mostrar/Ocultar e Sair
- Instância única (abrir o `.exe` novamente traz a janela existente para frente)
- Configurações persistidas no Registro do Windows (HKCU)

## Build (Visual Studio)

1. Abra a solução: `Timer PPT.slnx`
2. Defina o projeto **Timer PPT Standalone** como Startup Project
3. Compile e execute (Debug/Release)
