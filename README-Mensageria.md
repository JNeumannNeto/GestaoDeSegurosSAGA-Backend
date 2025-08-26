# Implementação de Mensageria - Sistema de Gestão de Seguros

## Visão Geral

Este documento descreve a implementação de mensageria no sistema de gestão de seguros, utilizando RabbitMQ para comunicação assíncrona entre os microserviços.

## Arquitetura Implementada

### Componentes Principais

1. **Shared.Messaging**: Biblioteca compartilhada com abstrações e implementações de mensageria
2. **PropostaService**: Publica eventos quando o status de propostas é alterado
3. **ContratacaoService**: Processa comandos de contratação assincronamente e publica eventos de resultado

### Padrões Implementados

- **Event-Driven Architecture**: Eventos são publicados quando mudanças importantes ocorrem
- **Command Query Responsibility Segregation (CQRS)**: Separação entre comandos e consultas
- **Asynchronous Processing**: Processamento assíncrono de operações pesadas

## Eventos e Comandos

### Eventos

#### PropostaStatusAlteradaEvent
Publicado quando o status de uma proposta é alterado.

```csharp
{
    "PropostaId": "guid",
    "NomeCliente": "string",
    "StatusAnterior": 0, // 0=EmAnalise, 1=Aprovada, 2=Rejeitada
    "NovoStatus": 1,
    "ValorCobertura": 50000.00,
    "ValorPremio": 500.00
}
```

#### ContratacaoProcessadaEvent
Publicado quando uma contratação é processada (sucesso ou falha).

```csharp
{
    "ContratacaoId": "guid",
    "PropostaId": "guid",
    "NomeCliente": "string",
    "ValorCobertura": 50000.00,
    "ValorPremio": 500.00,
    "DataContratacao": "2025-01-27T12:00:00Z",
    "Sucesso": true,
    "MensagemErro": null
}
```

### Comandos

#### ContratarPropostaCommand
Comando para processar uma contratação assincronamente.

```csharp
{
    "PropostaId": "guid"
}
```

## Configuração

### RabbitMQ Settings (appsettings.json)

```json
{
  "RabbitMq": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "ExchangeName": "seguros.exchange",
    "EventsQueueName": "seguros.events",
    "CommandsQueueName": "seguros.commands"
  }
}
```

## Benefícios Implementados

### 1. Desacoplamento
- Serviços não dependem mais de chamadas HTTP síncronas
- Redução do acoplamento entre PropostaService e ContratacaoService

### 2. Resiliência
- Retry automático em caso de falha no processamento
- Dead letter queues para mensagens que falharam múltiplas vezes
- Processamento assíncrono evita timeouts

### 3. Escalabilidade
- Processamento paralelo de comandos
- Múltiplos consumidores podem processar a mesma fila
- Balanceamento de carga automático

### 4. Auditoria e Observabilidade
- Todos os eventos são logados
- Rastreabilidade completa das operações
- Facilita debugging e monitoramento

### 5. Extensibilidade
- Novos handlers podem ser adicionados facilmente
- Novos serviços podem reagir aos eventos existentes
- Arquitetura preparada para crescimento

## Como Usar

### 1. Pré-requisitos
- RabbitMQ instalado e rodando
- .NET 8.0

### 2. Executar RabbitMQ
```bash
# Docker
docker run -d --hostname rabbitmq --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management

# Acesso ao Management UI: http://localhost:15672 (guest/guest)
```

### 3. Executar os Serviços
```bash
# Terminal 1 - PropostaService
cd PropostaService/API
dotnet run

# Terminal 2 - ContratacaoService
cd ContratacaoService/API
dotnet run
```

### 4. Testar a Implementação

#### Criar uma Proposta
```bash
POST https://localhost:7001/api/propostas
{
    "nomeCliente": "João Silva",
    "tipoCliente": 0,
    "tipoSeguro": 0,
    "valorCobertura": 50000,
    "valorPremio": 500
}
```

#### Alterar Status da Proposta (Gera Evento)
```bash
PUT https://localhost:7001/api/propostas/{id}/status
{
    "novoStatus": 1
}
```

#### Contratar Proposta (Processamento Assíncrono)
```bash
POST https://localhost:7002/api/contratacoes
{
    "propostaId": "{guid-da-proposta}"
}
```

## Monitoramento

### Logs
Os serviços logam todas as operações de mensageria:
- Publicação de eventos/comandos
- Processamento de mensagens
- Erros e falhas

### RabbitMQ Management
- Acesse http://localhost:15672
- Monitore filas, exchanges e mensagens
- Visualize métricas de throughput