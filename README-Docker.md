# Docker Setup - Gestão de Seguros

Este documento explica como executar o projeto usando Docker e Docker Compose.

## Pré-requisitos

- Docker
- Docker Compose

## Estrutura dos Serviços

O projeto contém os seguintes serviços:

- **PropostaService**: API para gerenciamento de propostas (porta 5001)
- **ContratacaoService**: API para gerenciamento de contratações (porta 5002)
- **RabbitMQ**: Message broker para comunicação entre serviços (porta 5672, management UI na porta 15672)

## Como executar

### 1. Construir e executar todos os serviços

```bash
docker-compose up --build
```

### 2. Executar em background

```bash
docker-compose up -d --build
```

### 3. Parar os serviços

```bash
docker-compose down
```

### 4. Parar e remover volumes

```bash
docker-compose down -v
```

## Acessos

- **PropostaService API**: http://localhost:5001
- **PropostaService Swagger**: http://localhost:5001/swagger
- **ContratacaoService API**: http://localhost:5002
- **ContratacaoService Swagger**: http://localhost:5002/swagger
- **RabbitMQ Management**: http://localhost:15672 (usuário: guest, senha: guest)

## Configuração

As configurações do RabbitMQ são definidas através de variáveis de ambiente no docker-compose.yml:

- `RabbitMq__HostName`: Nome do host do RabbitMQ (rabbitmq)
- `RabbitMq__Port`: Porta do RabbitMQ (5672)
- `RabbitMq__UserName`: Usuário (guest)
- `RabbitMq__Password`: Senha (guest)
- `RabbitMq__ExchangeName`: Nome do exchange (seguros.exchange)
- `RabbitMq__EventsQueueName`: Nome da fila de eventos (seguros.events)
- `RabbitMq__CommandsQueueName`: Nome da fila de comandos (seguros.commands)

## Logs

Para visualizar os logs de um serviço específico:

```bash
# Logs do PropostaService
docker-compose logs proposta-service

# Logs do ContratacaoService
docker-compose logs contratacao-service

# Logs do RabbitMQ
docker-compose logs rabbitmq

# Logs de todos os serviços
docker-compose logs
```

## Desenvolvimento

Para desenvolvimento, você pode executar apenas o RabbitMQ no Docker e os serviços .NET localmente:

```bash
# Executar apenas o RabbitMQ
docker-compose up rabbitmq -d

# Os serviços .NET podem ser executados localmente com:
# dotnet run --project PropostaService/API
# dotnet run --project ContratacaoService/API
```

## Melhorias para Docker

### RabbitMQ Connection Resilience

O projeto inclui um `RabbitMqConsumerImproved` que implementa:

- **Retry Logic**: Tenta reconectar ao RabbitMQ até 10 vezes com delay de 5 segundos
- **Connection Recovery**: Reconexão automática em caso de perda de conexão
- **Health Monitoring**: Monitora a saúde da conexão continuamente
- **Graceful Shutdown**: Fechamento adequado das conexões

### Health Checks

O RabbitMQ possui health check configurado que:

- Verifica conectividade na porta 5672
- Aguarda 30 segundos antes de iniciar os checks
- Tenta 10 vezes com intervalo de 10 segundos
- Os serviços só iniciam após RabbitMQ estar saudável

## Troubleshooting

### Problema: Serviços não conseguem conectar ao RabbitMQ

1. Verifique se o RabbitMQ está saudável:

```bash
docker-compose ps
```

2. Verifique os logs do RabbitMQ:

```bash
docker-compose logs rabbitmq
```

3. Verifique os logs dos serviços para ver tentativas de reconexão:

```bash
docker-compose logs contratacao-service
docker-compose logs proposta-service
```

### Problema: RabbitMQ demora para inicializar

O RabbitMQ pode demorar até 30 segundos para estar completamente pronto. Os serviços aguardarão automaticamente.

### Problema: Porta já está em uso

Altere as portas no docker-compose.yml se necessário:

```yaml
ports:
  - "5003:8080"  # Altere 5001 para 5003, por exemplo
```

### Problema: Build falha

Limpe o cache do Docker:

```bash
docker system prune -a
docker-compose build --no-cache
```

### Problema: Mensagens não são processadas

1. Verifique se os handlers estão registrados corretamente
2. Verifique os logs para erros de processamento
3. Acesse o RabbitMQ Management UI em http://localhost:15672 para verificar filas e mensagens
