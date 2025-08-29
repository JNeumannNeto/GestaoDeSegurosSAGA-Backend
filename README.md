# Sistema de Gestão de Seguros com Pattern SAGA

Sistema desenvolvido em .NET Core 8 utilizando arquitetura hexagonal (Ports & Adapters), abordagem baseada em microserviços e **implementação do Pattern SAGA** para gerenciar propostas de seguro e suas contratações com garantias de consistência eventual.

## 🏗️ Arquitetura

O sistema é composto por dois microserviços principais com infraestrutura SAGA:

### 1. PropostaService
Responsável por:
- Criar proposta de seguro
- Listar propostas
- Alterar status da proposta (Em Análise, Aprovada, Rejeitada)
- Publicar eventos de mudança de status
- Expor API REST

### 2. ContratacaoService
Responsável por:
- Processar contratações via **SAGA Pattern**
- Garantir consistência eventual nas transações
- Compensação automática em caso de falhas
- Comunicação assíncrona via mensageria
- Expor API REST

### 3. Shared.Saga (Nova Infraestrutura)
Biblioteca reutilizável que fornece:
- Abstrações para definição de SAGAs
- Orquestrador de execução
- Repositório de estado das SAGAs
- Modelos e extensões para DI

## 🎯 Pattern SAGA Implementado

### ContratarPropostaSaga
SAGA completa para processo de contratação com 4 steps sequenciais:

1. **ValidarPropostaStep**: Valida existência da proposta
2. **VerificarDisponibilidadeStep**: Verifica se está aprovada e disponível
3. **CriarContratacaoStep**: Cria a contratação (com compensação)
4. **PublicarEventoStep**: Publica evento de sucesso/falha

### Fluxo de Execução

#### ✅ Cenário de Sucesso
```
1. Comando ContratarPropostaCommand recebido
2. ContratarPropostaSagaHandler inicia a SAGA
3. SagaOrchestrator executa os steps em ordem:
   - ValidarPropostaStep: ✅ Proposta encontrada
   - VerificarDisponibilidadeStep: ✅ Proposta aprovada e disponível
   - CriarContratacaoStep: ✅ Contratação criada
   - PublicarEventoStep: ✅ Evento de sucesso publicado
4. SAGA marcada como Completed
```

#### ❌ Cenário de Falha com Compensação
```
1. Comando ContratarPropostaCommand recebido
2. Execução dos steps até falha
3. SagaOrchestrator inicia compensação automática:
   - PublicarEventoStep.CompensateAsync: ✅ Evento de falha publicado
   - CriarContratacaoStep.CompensateAsync: ✅ Contratação removida
4. SAGA marcada como Compensated
```

## 🚀 Benefícios da Implementação SAGA

### 1. **Consistência Eventual**
- Garantia de que o sistema chegará a um estado consistente
- Rollback automático em caso de falhas
- Rastreabilidade completa do processo

### 2. **Observabilidade**
- Logs detalhados de cada step
- Estado persistido da SAGA
- Métricas de sucesso/falha

### 3. **Resiliência**
- Compensação automática de operações
- Isolamento de falhas entre steps
- Recuperação de estados intermediários

### 4. **Extensibilidade**
- Fácil adição de novos steps
- Reutilização da infraestrutura para novas SAGAs
- Configuração flexível de dependências

## 📁 Estrutura do Projeto

```
GestaoDeSeguros-Backend/
├── Shared/
│   └── Shared.Saga/                     # 🆕 Infraestrutura SAGA
│       ├── Abstractions/                # Interfaces (ISaga, ISagaStep, etc.)
│       ├── Models/                      # Modelos (SagaInstance, SagaStatus, etc.)
│       ├── Orchestration/               # SagaOrchestrator
│       ├── Storage/                     # InMemorySagaRepository
│       └── Extensions/                  # ServiceCollectionExtensions
│
├── ContratacaoService/
│   ├── Application/
│   │   └── Sagas/                       # 🆕 Implementação SAGA
│   │       └── ContratarPropostaSaga/
│   │           ├── Models/              # ContratarPropostaData
│   │           ├── Steps/               # 4 Steps implementados
│   │           ├── Handlers/            # ContratarPropostaSagaHandler
│   │           └── ContratarPropostaSaga.cs
│   └── ...
│
└── PropostaService/
    └── ...
```

Para visualizar a estrutura detalhada do projeto, consulte o documento [ESTRUTURA_PROJETO.md](ESTRUTURA_PROJETO.md).

## 📋 Regras de Negócio

### Tipos de Cliente
- **Pessoa Física**: Pode contratar seguros de Vida, Saúde, Automóvel, Residencial
- **Pessoa Jurídica**: Pode contratar seguros Empresarial, de Cargas, de Frota, Condomínio, Vida Empresarial

### Status da Proposta
- **Em Análise**: Status inicial de toda proposta
- **Aprovada**: Proposta aprovada, pode ser contratada
- **Rejeitada**: Proposta rejeitada, não pode ser contratada

### Validações SAGA
- Apenas propostas aprovadas podem ser contratadas
- Uma proposta não pode ser contratada mais de uma vez
- Falhas em qualquer step acionam compensação automática
- Todos os steps são logados para auditoria

## 🛠️ Como Executar

### Pré-requisitos
- .NET 8.0
- RabbitMQ (para mensageria)

### Executando RabbitMQ
```bash
# Docker
docker run -d --hostname rabbitmq --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management

# Acesso ao Management UI: http://localhost:15672 (guest/guest)
```

### Executando os Microserviços

1. **PropostaService** (Porta 7001):
```bash
cd PropostaService/API
dotnet run
```
Acesse: https://localhost:7001/swagger

2. **ContratacaoService** (Porta 7002):
```bash
cd ContratacaoService/API
dotnet run
```
Acesse: https://localhost:7002/swagger

### Executando os Testes

```bash
# Testes do PropostaService
dotnet test PropostaService.Tests/

# Testes do ContratacaoService
dotnet test ContratacaoService.Tests/

# Todos os testes
dotnet test
```

### Compilando a Solução Completa

```bash
dotnet build GestaoDeSeguros.sln
```

## 🔌 APIs Disponíveis

### PropostaService (https://localhost:7001)

#### Propostas
- `GET /api/propostas` - Listar todas as propostas
- `GET /api/propostas/{id}` - Obter proposta por ID
- `POST /api/propostas` - Criar nova proposta
- `PUT /api/propostas/{id}/status` - Alterar status da proposta

### ContratacaoService (https://localhost:7002)

#### Contratações (Processamento via SAGA)
- `GET /api/contratacoes` - Listar todas as contratações
- `GET /api/contratacoes/{id}` - Obter contratação por ID
- `POST /api/contratacoes` - **Contratar proposta via SAGA** 🆕

## 📝 Exemplos de Uso

### Criar uma Proposta (Pessoa Física)
```json
POST /api/propostas
{
  "nomeCliente": "João Silva",
  "tipoCliente": 0,
  "tipoSeguro": 0,
  "valorCobertura": 100000.00,
  "valorPremio": 500.00
}
```

### Alterar Status para Aprovada
```json
PUT /api/propostas/{id}/status
{
  "novoStatus": 1
}
```

### Contratar uma Proposta (Processamento SAGA)
```json
POST /api/contratacoes
{
  "propostaId": "guid-da-proposta"
}
```

## 📊 Monitoramento e Logs

### Logs Estruturados da SAGA
```
[INFO] Starting saga ContratarProposta with ID {SagaId} and correlation ID {CorrelationId}
[INFO] Executing step ValidarProposta (1) for saga {SagaId}
[INFO] Step ValidarProposta completed successfully for saga {SagaId}
[INFO] Executing step VerificarDisponibilidade (2) for saga {SagaId}
[INFO] Step VerificarDisponibilidade completed successfully for saga {SagaId}
[INFO] Executing step CriarContratacao (3) for saga {SagaId}
[INFO] Step CriarContratacao completed successfully for saga {SagaId}
[INFO] Executing step PublicarEvento (4) for saga {SagaId}
[INFO] Step PublicarEvento completed successfully for saga {SagaId}
[INFO] Saga {SagaId} completed successfully
```

### RabbitMQ Management
- Acesse http://localhost:15672
- Monitore filas, exchanges e mensagens
- Visualize métricas de throughput

## 🧪 Dados Mocados

O sistema utiliza dados em memória com algumas propostas pré-cadastradas para facilitar os testes:

1. **João Silva** - Seguro de Vida (Aprovada)
2. **Maria Santos** - Seguro de Automóvel (Em Análise)
3. **Empresa ABC Ltda** - Seguro Empresarial (Rejeitada)

## 🏛️ Princípios Aplicados

- **Clean Code**: Código limpo e legível
- **SOLID**: Princípios de design orientado a objetos
- **DDD**: Domain-Driven Design
- **Arquitetura Hexagonal**: Separação clara entre domínio e infraestrutura
- **Microserviços**: Serviços independentes e especializados
- **SAGA Pattern**: Transações distribuídas com consistência eventual 🆕
- **Event-Driven Architecture**: Comunicação assíncrona via eventos 🆕
- **CQRS**: Separação entre comandos e consultas 🆕
- **Testes Unitários**: Cobertura de testes para regras de negócio

## 🔄 Comunicação entre Microserviços

### Comunicação Síncrona (HTTP)
- Validação de propostas
- Consultas de dados

### Comunicação Assíncrona (RabbitMQ) 🆕
- Processamento de comandos via SAGA
- Publicação de eventos de domínio
- Notificações entre serviços

### Eventos Implementados
- `PropostaStatusAlteradaEvent`: Quando status da proposta muda
- `ContratacaoProcessadaEvent`: Quando contratação é processada (sucesso/falha)

## 🚀 Próximos Passos

### Melhorias Futuras
1. **Persistência Durável**: Migrar de InMemory para SQL Server/PostgreSQL
2. **Retry Policies**: Implementar retry com backoff exponencial
3. **Circuit Breaker**: Adicionar circuit breaker para serviços externos
4. **Métricas**: Implementar métricas com Prometheus/Grafana
5. **Dashboard**: Criar dashboard para monitoramento das SAGAs

### Novas SAGAs
1. **ProcessamentoAutomaticoSaga**: Para aprovação e contratação automática
2. **CancelamentoContratacaoSaga**: Para cancelamentos com reembolso
3. **RenovacaoContratacaoSaga**: Para renovações automáticas

## 📚 Documentação Adicional

- [ANALISE_SAGA_PATTERN.md](ANALISE_SAGA_PATTERN.md) - Análise detalhada das oportunidades SAGA
- [IMPLEMENTACAO_SAGA_CONCLUIDA.md](IMPLEMENTACAO_SAGA_CONCLUIDA.md) - Documentação técnica da implementação
- [README-Mensageria.md](README-Mensageria.md) - Documentação do sistema de mensageria

---

## ✅ Status da Implementação

**SAGA Pattern**: ✅ **IMPLEMENTADO COM SUCESSO**

- ✅ Infraestrutura SAGA completa e reutilizável
- ✅ SAGA de contratação totalmente funcional
- ✅ Integração com sistema de mensageria existente
- ✅ Compensação automática em caso de falhas
- ✅ Logs estruturados e observabilidade
- ✅ Testes de compilação bem-sucedidos

O sistema agora está preparado para processar contratações de forma confiável, recuperar-se automaticamente de falhas e escalar horizontalmente conforme necessário.
