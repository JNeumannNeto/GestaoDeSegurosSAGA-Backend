# Sistema de Gestão de Seguros

Sistema desenvolvido em .NET Core 8 utilizando arquitetura hexagonal (Ports & Adapters) e abordagem baseada em microserviços para gerenciar propostas de seguro e suas contratações.

## Arquitetura

O sistema é composto por dois microserviços principais:

### 1. PropostaService
Responsável por:
- Criar proposta de seguro
- Listar propostas
- Alterar status da proposta (Em Análise, Aprovada, Rejeitada)
- Expor API REST

### 2. ContratacaoService
Responsável por:
- Contratar uma proposta (somente se Aprovada)
- Armazenar informações da contratação
- Comunicar-se com o PropostaService para verificar status
- Expor API REST

## Estrutura do Projeto

Para visualizar a estrutura detalhada do projeto, consulte o documento [ESTRUTURA_PROJETO.md](ESTRUTURA_PROJETO.md).

## Regras de Negócio

### Tipos de Cliente
- **Pessoa Física**: Pode contratar seguros de Vida, Saúde, Automóvel, Residencial
- **Pessoa Jurídica**: Pode contratar seguros Empresarial, de Cargas, de Frota, Condomínio, Vida Empresarial

### Status da Proposta
- **Em Análise**: Status inicial de toda proposta
- **Aprovada**: Proposta aprovada, pode ser contratada
- **Rejeitada**: Proposta rejeitada, não pode ser contratada

### Validações
- Nome do cliente é obrigatório
- Valores de cobertura e prêmio devem ser maiores que zero
- Tipo de seguro deve ser compatível com o tipo de cliente
- Apenas propostas aprovadas podem ser contratadas
- Uma proposta não pode ser contratada mais de uma vez

## Como Executar

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

## APIs Disponíveis

### PropostaService (https://localhost:7001)

#### Propostas
- `GET /api/propostas` - Listar todas as propostas
- `GET /api/propostas/{id}` - Obter proposta por ID
- `POST /api/propostas` - Criar nova proposta
- `PUT /api/propostas/{id}/status` - Alterar status da proposta

### ContratacaoService (https://localhost:7002)

#### Contratações
- `GET /api/contratacoes` - Listar todas as contratações
- `GET /api/contratacoes/{id}` - Obter contratação por ID
- `POST /api/contratacoes` - Contratar uma proposta

## Exemplos de Uso

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

### Contratar uma Proposta
```json
POST /api/contratacoes
{
  "propostaId": "guid-da-proposta"
}
```

## Dados Mocados

O sistema utiliza dados em memória com algumas propostas pré-cadastradas para facilitar os testes:

1. **João Silva** - Seguro de Vida (Aprovada)
2. **Maria Santos** - Seguro de Automóvel (Em Análise)
3. **Empresa ABC Ltda** - Seguro Empresarial (Rejeitada)

## Princípios Aplicados

- **Clean Code**: Código limpo e legível
- **SOLID**: Princípios de design orientado a objetos
- **DDD**: Domain-Driven Design
- **Arquitetura Hexagonal**: Separação clara entre domínio e infraestrutura
- **Microserviços**: Serviços independentes e especializados
- **Testes Unitários**: Cobertura de testes para regras de negócio

## Comunicação entre Microserviços

O ContratacaoService se comunica com o PropostaService via HTTP para:
- Verificar se a proposta existe
- Validar se o status da proposta é "Aprovada"
- Obter dados da proposta para a contratação

A comunicação é implementada através do padrão HTTP Client com tratamento de erros e timeouts.
