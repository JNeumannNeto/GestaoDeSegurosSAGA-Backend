# Estrutura do Projeto - Gestão de Seguros Backend

## Visão Geral
Este documento apresenta a estrutura atual do projeto Gestão de Seguros Backend, organizado em uma arquitetura de microsserviços com Clean Architecture.

## Estrutura de Diretórios

```
GestaoDeSeguros-Backend/
├── .dockerignore
├── .gitattributes
├── .gitignore
├── docker-compose.yml
├── Dockerfile
├── GestaoDeSeguros.sln
├── nuget.config
├── README.md
├── README-Docker.md
├── README-Mensageria.md
│
├── ContratacaoService/                    # Microsserviço de Contratação
│   ├── API/                              # Camada de Apresentação
│   │   ├── Controllers/
│   │   │   └── ContratacoesController.cs
│   │   ├── DTOs/
│   │   │   └── ErrorResponse.cs
│   │   ├── Middleware/
│   │   │   └── GlobalExceptionMiddleware.cs
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   ├── appsettings.json
│   │   ├── ContratacaoService.API.csproj
│   │   └── Program.cs
│   │
│   ├── Application/                      # Camada de Aplicação
│   │   ├── Common/
│   │   │   ├── PagedResult.cs
│   │   │   └── Result.cs
│   │   ├── DTOs/
│   │   │   ├── ContratacaoResponse.cs
│   │   │   └── ContratarPropostaRequest.cs
│   │   ├── Handlers/
│   │   │   └── ContratarPropostaCommandHandler.cs
│   │   ├── Services/
│   │   │   └── ContratacaoAppService.cs
│   │   └── ContratacaoService.Application.csproj
│   │
│   ├── Domain/                          # Camada de Domínio
│   │   ├── Common/
│   │   │   ├── PagedResult.cs
│   │   │   └── Result.cs
│   │   ├── Entities/
│   │   │   └── Contratacao.cs
│   │   ├── Enums/
│   │   │   └── StatusProposta.cs
│   │   ├── Models/
│   │   │   └── PropostaDto.cs
│   │   ├── Ports/
│   │   │   ├── IContratacaoRepository.cs
│   │   │   ├── IPropostaServiceClient.cs
│   │   │   └── IValidationService.cs
│   │   ├── Services/
│   │   └── ContratacaoService.Domain.csproj
│   │
│   ├── Infrastructure/                   # Camada de Infraestrutura
│   │   ├── Clients/
│   │   │   └── PropostaServiceClient.cs
│   │   ├── Repositories/
│   │   │   └── ContratacaoRepository.cs
│   │   ├── Services/
│   │   │   └── ValidationService.cs
│   │   └── ContratacaoService.Infrastructure.csproj
│   │
│   └── ContratacaoService.Tests/         # Testes
│       ├── Application/
│       │   ├── ContratacaoAppServiceTests.cs
│       │   └── ContratarPropostaCommandHandlerTests.cs
│       ├── Domain/
│       │   └── ContratacaoTests.cs
│       └── ContratacaoService.Tests.csproj
│
├── PropostaService/                      # Microsserviço de Propostas
│   ├── API/                             # Camada de Apresentação
│   │   ├── Controllers/
│   │   │   └── PropostasController.cs
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   ├── appsettings.json
│   │   ├── Program.cs
│   │   └── PropostaService.API.csproj
│   │
│   ├── Application/                     # Camada de Aplicação
│   │   ├── Commands/
│   │   │   ├── AlterarStatusPropostaCommand.cs
│   │   │   └── CriarPropostaCommand.cs
│   │   ├── DTOs/
│   │   │   ├── AlterarStatusRequest.cs
│   │   │   ├── CriarPropostaRequest.cs
│   │   │   └── PropostaResponse.cs
│   │   ├── Handlers/
│   │   │   ├── AlterarStatusPropostaCommandHandler.cs
│   │   │   ├── CriarPropostaCommandHandler.cs
│   │   │   ├── ICommandHandler.cs
│   │   │   ├── IQueryHandler.cs
│   │   │   ├── ListarPropostasQueryHandler.cs
│   │   │   └── ObterPropostaPorIdQueryHandler.cs
│   │   ├── Ports/
│   │   │   └── IPropostaAppService.cs
│   │   ├── Queries/
│   │   │   ├── ListarPropostasQuery.cs
│   │   │   └── ObterPropostaPorIdQuery.cs
│   │   ├── Services/
│   │   │   ├── IMediator.cs
│   │   │   └── PropostaAppService.cs
│   │   └── PropostaService.Application.csproj
│   │
│   ├── Domain/                          # Camada de Domínio
│   │   ├── Common/
│   │   │   └── EntityBase.cs
│   │   ├── Entities/
│   │   │   └── Proposta.cs
│   │   ├── Enums/
│   │   │   ├── StatusProposta.cs
│   │   │   ├── TipoCliente.cs
│   │   │   └── TipoSeguro.cs
│   │   ├── Events/
│   │   │   ├── IDomainEvent.cs
│   │   │   ├── PropostaCriadaEvent.cs
│   │   │   └── PropostaStatusAlteradoEvent.cs
│   │   ├── Ports/
│   │   │   ├── IDataSeeder.cs
│   │   │   ├── IPropostaRepository.cs
│   │   │   └── IValidationService.cs
│   │   ├── Services/
│   │   │   └── PropostaDomainService.cs
│   │   ├── ValueObjects/
│   │   │   ├── NomeCliente.cs
│   │   │   └── ValorMonetario.cs
│   │   └── PropostaService.Domain.csproj
│   │
│   ├── Infrastructure/                   # Camada de Infraestrutura
│   │   ├── Data/
│   │   │   └── PropostaSeedData.cs
│   │   ├── Repositories/
│   │   │   └── PropostaRepository.cs
│   │   ├── Services/
│   │   │   ├── Mediator.cs
│   │   │   └── ValidationService.cs
│   │   └── PropostaService.Infrastructure.csproj
│   │
│   └── PropostaService.Tests/            # Testes
│       ├── Application/
│       │   └── PropostaAppServiceTests.cs
│       ├── Domain/
│       │   └── PropostaTests.cs
│       ├── Integration/
│       │   ├── CustomWebApplicationFactory.cs
│       │   └── PropostaIntegrationTests.cs
│       └── PropostaService.Tests.csproj
│
├── Shared/                              # Componentes Compartilhados
│   └── Shared.Messaging/                # Sistema de Mensageria
│       ├── Abstractions/
│       │   ├── ICommandHandler.cs
│       │   ├── ICommandPublisher.cs
│       │   ├── IEventHandler.cs
│       │   └── IEventPublisher.cs
│       ├── Commands/
│       │   ├── ContratarPropostaCommand.cs
│       │   └── ICommand.cs
│       ├── Configuration/
│       │   └── RabbitMqSettings.cs
│       ├── Events/
│       │   ├── BaseEvent.cs
│       │   ├── ContratacaoProcessadaEvent.cs
│       │   ├── IEvent.cs
│       │   └── PropostaStatusAlteradaEvent.cs
│       ├── Extensions/
│       │   └── ServiceCollectionExtensions.cs
│       ├── Handlers/
│       │   └── NotificacaoEventHandler.cs
│       ├── RabbitMq/
│       │   ├── RabbitMqCommandPublisher.cs
│       │   ├── RabbitMqConsumer.cs
│       │   ├── RabbitMqConsumerImproved.cs
│       │   └── RabbitMqEventPublisher.cs
│       └── Shared.Messaging.csproj
│
└── GestaoDeSeguros/                     # Projeto Principal (Legacy)
    └── obj/
```

## Arquitetura Implementada

### Clean Architecture
Cada microsserviço segue os princípios da Clean Architecture com as seguintes camadas:

- **API**: Camada de apresentação com controllers e middleware
- **Application**: Camada de aplicação com services, handlers, commands e queries
- **Domain**: Camada de domínio com entidades, value objects, events e regras de negócio
- **Infrastructure**: Camada de infraestrutura com repositórios, clientes HTTP e serviços externos

### Padrões Implementados

#### CQRS (Command Query Responsibility Segregation)
- **Commands**: Operações de escrita (CriarPropostaCommand, AlterarStatusPropostaCommand)
- **Queries**: Operações de leitura (ObterPropostaPorIdQuery, ListarPropostasQuery)
- **Handlers**: Processadores específicos para cada comando/query

#### Domain-Driven Design (DDD)
- **Entities**: Objetos com identidade (Proposta, Contratacao)
- **Value Objects**: Objetos sem identidade (NomeCliente, ValorMonetario)
- **Domain Events**: Eventos de domínio (PropostaCriadaEvent, PropostaStatusAlteradoEvent)
- **Domain Services**: Serviços de domínio (PropostaDomainService)

#### Mediator Pattern
- Implementação de mediator para desacoplamento entre controllers e handlers
- Centralização do roteamento de commands e queries

#### Repository Pattern
- Abstração do acesso a dados através de interfaces
- Implementação em memória para desenvolvimento e testes

### Microsserviços

#### PropostaService
- Gerenciamento completo de propostas de seguro
- Implementação de CQRS com commands e queries
- Domain events para notificação de mudanças
- Validações de domínio com value objects

#### ContratacaoService
- Processamento de contratações de propostas aprovadas
- Comunicação HTTP com PropostaService
- Validações de negócio para contratação

### Sistema de Mensageria
- Infraestrutura compartilhada para comunicação assíncrona
- Implementação com RabbitMQ
- Suporte a commands e events
- Handlers para processamento de mensagens

### Testes
- **Testes Unitários**: Cobertura das regras de negócio
- **Testes de Integração**: Validação da comunicação entre camadas
- **Testes de Application Services**: Validação dos fluxos de aplicação

## Melhorias Implementadas

O projeto conta com documentação detalhada das melhorias implementadas:

- **MELHORIAS_IMPLEMENTADAS.md**: Melhorias gerais do sistema
- **MELHORIAS_CONTRATACAO_SERVICE.md**: Específicas do ContratacaoService
- **MELHORIAS_DOMINIO_RICO_IMPLEMENTADAS.md**: Implementação de domínio rico
- **MELHORIAS_OBJECT_CALISTHENICS_IMPLEMENTADAS.md**: Aplicação de Object Calisthenics
- **MELHORIAS_FINAIS_IMPLEMENTADAS.md**: Melhorias finais e refinamentos

## Tecnologias Utilizadas

- **.NET 8**: Framework principal
- **ASP.NET Core**: Para APIs REST
- **Entity Framework Core**: ORM (configurado para uso futuro)
- **RabbitMQ**: Sistema de mensageria
- **Docker**: Containerização
- **xUnit**: Framework de testes
- **Swagger/OpenAPI**: Documentação das APIs
