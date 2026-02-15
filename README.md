# BankMore - Fintech

Este projeto implementa uma plataforma bancária baseada em microsserviços, focada em escalabilidade, segurança e padrões de arquitetura modernos (DDD, CQRS), atendendo aos requisitos funcionais e não funcionais solicitados.

## Tabela de Requisitos e Implementação

Abaixo, listei cada requisito do projeto e onde ele foi implementado no código.

### 1. Arquitetura e Padrões (Time de Arquitetura)

**DDD (Domain-Driven Design)**
As entidades de domínio (`ContaCorrente`, `Movimento`, `Transferencia`) encapsulam as regras de negócio e não são anêmicas.
- [ContaCorrente.cs](src/BankMore.Account.API/Domain/ContaCorrente.cs)
- [Movimento.cs](src/BankMore.Account.API/Domain/Movimento.cs)
- [Transferencia.cs](src/BankMore.Transfer.API/Domain/Transferencia.cs)

**CQRS (Command Query Responsibility Segregation)**
Separação clara entre comandos (escrita) e consultas (leitura) usando MediatR.

Comandos:
- [CreateAccountHandler.cs](src/BankMore.Account.API/Application/Commands/CreateAccountHandler.cs)

Consultas:
- [GetBalanceHandler.cs](src/BankMore.Account.API/Application/Queries/GetBalanceHandler.cs)

### 2. Segurança (Time de Segurança)

**Autenticação via Token (JWT)**
Geração de tokens JWT com claims seguras e validação via middleware em todos os endpoints protegidos.

Geração:
- [TokenService.cs](src/BankMore.Account.API/Infrastructure/Services/TokenService.cs)

Validação:
- [Program.cs (Account)](src/BankMore.Account.API/Program.cs)

**Proteção de Dados Sensíveis** <br>
Senhas são hashadas com BCrypt antes de serem salvas. O CPF é validado e tratado como Value Object.

Hash de Senha:
- [PasswordService.cs](src/BankMore.Account.API/Infrastructure/Services/PasswordService.cs)

Validação CPF:
- [Cpf.cs](src/BankMore.Core/Domain/Cpf.cs)

### 3. Funcionalidades - API Conta Corrente

**Cadastro de Conta**
Valida CPF, cria hash da senha e persiste a conta. Retorna erro se CPF já existe.
- [CreateAccountHandler.cs](src/BankMore.Account.API/Application/Commands/CreateAccountHandler.cs)

**Login**
Autentica via Conta ou CPF + Senha e retorna Token JWT.
- [LoginHandler.cs](src/BankMore.Account.API/Application/Commands/LoginHandler.cs)

**Inativar Conta**
Exige senha e token. Inativa logicamente a conta (`Ativo = 0`).
- [InactivateAccountHandler.cs](src/BankMore.Account.API/Application/Commands/InactivateAccountHandler.cs)

**Movimentação (Débito/Crédito)**
Valida saldo (para débito), status da conta e tipo de transação para garantir a integridade da operação.
- [MakeTransactionHandler.cs](src/BankMore.Account.API/Application/Commands/MakeTransactionHandler.cs)

**Consulta de Saldo (com Cache)**
Calcula o saldo dinamicamente somando créditos e subtraindo débitos. Implementa Cache para otimizar consultas frequentes.
- [GetBalanceHandler.cs](src/BankMore.Account.API/Application/Queries/GetBalanceHandler.cs)

### 4. Funcionalidades - API Transferência

**Transferência entre Contas**
Orquestra o débito na origem e crédito no destino. Em caso de falha no crédito, realiza o estorno automático.
- [MakeTransferHandler.cs](src/BankMore.Transfer.API/Application/Commands/MakeTransferHandler.cs)

**Resiliência (Polly)**
Política de Retry (tentativa) configurada para chamadas HTTP entre APIs, garantindo robustez contra falhas de rede.
- [Program.cs (Transfer)](src/BankMore.Transfer.API/Program.cs)

**Idempotência**
Verifica se a requisição já foi processada antes de executar, evitando duplicidade em casos de retries do cliente.
- [TransferRepository.cs](src/BankMore.Transfer.API/Infrastructure/Repositories/TransferRepository.cs)

### 5. Worker de Tarifas (Opcional Implementado)

**Consumo de Kafka**
Consome mensagens do tópico `transferencias-realizadas` para calcular tarifas de forma assíncrona.
- [TransferenciaRealizadaConsumer.cs](src/BankMore.Tariff.Worker/Consumers/TransferenciaRealizadaConsumer.cs)

**Aplicação de Tarifa**
Após calcular, publica evento `tarifacoes-realizadas` que é consumido pela API de Contas para debitar o valor.
- [TarifacaoRealizadaConsumer.cs](src/BankMore.Account.API/Consumers/TarifacaoRealizadaConsumer.cs)

### 6. Infraestrutura e Qualidade

**Docker Compose**
Orquestração de todos os containers (APIs, Kafka, Zookeeper) prontos para rodar.
- [docker-compose.yaml](docker-compose.yaml)

**Testes Automatizados e de Integração**
Testes unitários cobrindo fluxos críticos de domínio e aplicação.
Testes de Integração (`IntegrationTests`) que validam o fluxo completo entre APIs (Conta e Transferência) em um ambiente isolado.
- [CreateAccountHandlerTests.cs](tests/BankMore.Tests/CreateAccountHandlerTests.cs)
- [MakeTransactionHandlerTests.cs](tests/BankMore.Tests/MakeTransactionHandlerTests.cs)
- [TransferIntegrationTests.cs](tests/BankMore.Tests/Integration/TransferIntegrationTests.cs)
- [AccountIntegrationTests.cs](tests/BankMore.Tests/Integration/AccountIntegrationTests.cs)

### 7. Diferenciais Técnicos e Decisões de Arquitetura

**Cache de Alta Performance**
Implementado `IMemoryCache` na consulta de saldo para reduzir carga no banco em requisições frequentes.
- [GetBalanceHandler.cs](src/BankMore.Account.API/Application/Queries/GetBalanceHandler.cs)

**Inicialização Automática de Banco (Zero Config)**
Scripts SQL de criação de tabelas são embarcados como recursos (`Embedded Resource`) e executados automaticamente na inicialização da API, garantindo que o ambiente suba pronto para uso sem scripts manuais.
- [DbInitializer.cs](src/BankMore.Account.API/Infrastructure/DbInitializer.cs)

**Mensageria Robusta com KafkaFlow**
Uso da biblioteca `KafkaFlow` para configuração fluida e consumidores Kafka.
- [KafkaConfiguration.cs](src/BankMore.Account.API/Configuration/KafkaConfiguration.cs)

---

## Como Executar

1. Certifique-se de ter o Docker instalado.

2. Na raiz do projeto, execute:
   docker-compose up -d --build

3. Acesse as documentações Swagger:
   - Account API: http://localhost:8080/swagger
   - Transfer API: http://localhost:8081/swagger
