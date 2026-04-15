# Timer PPT Standalone (Overlay)

Aplicativo Windows (WinForms) que exibe um timer em formato de overlay transparente na tela, com configurações persistentes, operação via cliques do mouse e controle via bandeja do sistema.

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

## Requisitos

- Windows 10/11
- .NET Framework 4.7.2 (runtime)

## Como usar

- **Clique esquerdo**: iniciar/pausar.
- **Duplo clique esquerdo**: reset (volta para o tempo inicial configurado).
- **Clique direito**: abre o menu:
  - **Iniciar/Pausar**
  - **Reset**
  - **Configurar...**
  - **Fechar** (encerra o aplicativo)
- **Mover na tela**: clique e arraste o timer.

## Comportamento ao zerar

- Ao chegar em `00:00`, o timer continua para **contagem negativa** (`-00:01`, `-00:02`...) em **vermelho**.
- Se habilitado em **Configurar...**, toca um **aviso sonoro** ao passar por `00:00`.

## Bandeja do sistema (tray)

- Um ícone do aplicativo aparece na bandeja do sistema.
- **Duplo clique** no ícone: Mostrar/Ocultar.
- **Clique direito** no ícone: menu com **Mostrar/Ocultar** e **Sair**.

## Configurações

- As configurações são salvas no Registro do Windows em:
  - `HKCU\Software\TimerPPT`
- Itens disponíveis em **Configurar...**:
  - Tempo padrão
  - Tamanho da fonte
  - Cor do timer
  - Aviso sonoro ao terminar
  - Lembrar posição do timer

## Build (Visual Studio)

1. Abra a solução: `src/Timer PPT.slnx`
2. Defina o projeto **Timer PPT Standalone** como Startup Project
3. Compile e execute (Debug/Release)

Saída padrão:
- `src/Timer PPT Standalone/bin/Debug`
- `src/Timer PPT Standalone/bin/Release`

## Distribuição (sugestão)

- Para reduzir alertas do Windows/SmartScreen, assinar o `.exe` com **certificado público de Code Signing**.

## Logs

Em caso de falha ao iniciar, o aplicativo grava um log em:
- `%LOCALAPPDATA%\TimerPPT\TimerPPTOverlay.log`

O aplicativo também tenta exibir uma mensagem com o caminho do log.

## Observações técnicas

- O aplicativo é **instância única**: se abrir o `.exe` novamente, ele sinaliza a instância existente para aparecer.
- O ícone da bandeja é carregado de um recurso embutido (`logo.ico`) dentro do executável.
