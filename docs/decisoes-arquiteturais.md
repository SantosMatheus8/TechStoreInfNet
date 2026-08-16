# Decisões Arquiteturais — TechStore Cloud

## 1. Frontend Estático (Amazon S3)

**Decisão:** HTML/CSS/JS puro, sem frameworks.

**Justificativa:** Para um MVP acadêmico, um frontend sem dependências externas é mais simples de hospedar como site estático no S3 e não exige build pipeline. O código usa `fetch` nativo para consumir a API REST, mantendo zero dependências. A estrutura é responsiva e funcional para demonstrar o CRUD completo.

**Trade-off:** Sem componentização ou reatividade de um framework moderno (React, Vue). Em uma evolução real, migraríamos para um SPA com build otimizado.

## 2. API REST (.NET 8 / ASP.NET Core)

**Decisão:** ASP.NET Core Web API com arquitetura em camadas (Controllers → Services → Repositories).

**Justificativa:**
- **Controllers** recebem requisições HTTP e delegam para Services.
- **Services** contêm regras de negócio e validações.
- **Repositories** encapsulam o acesso a dados via Entity Framework Core.
- **DTOs** isolam o modelo de dados interno da representação externa na API.

Essa separação permite testar a camada de negócio isoladamente (via mocks do repositório) e facilita a manutenção.

**Padrões aplicados:**
- Injeção de dependência nativa do ASP.NET Core.
- Middleware centralizado para tratamento de exceções.
- Swagger/OpenAPI para documentação automática.

## 3. Banco de Dados (Amazon RDS / PostgreSQL)

**Decisão:** PostgreSQL via Entity Framework Core com Code-First migrations.

**Justificativa:** PostgreSQL é a opção mais robusta e sem custo de licença no RDS. O EF Core com migrations permite versionamento do schema junto com o código. A migration automática em desenvolvimento (`Database.Migrate()` no startup) simplifica o onboarding.

**Modelo de dados — campos adicionais:**
- `ImagemUrl`: permite associar uma imagem ao produto.
- `Ativo`: soft-delete / controle de visibilidade sem excluir dados.
- Índices em `Categoria` e `Ativo` para queries frequentes.

## 4. Hospedagem da API (Amazon EC2)

**Decisão:** Docker + docker-compose no EC2, com possibilidade de publicação self-contained.

**Justificativa:** Docker garante reprodutibilidade entre ambientes (local → EC2). O Dockerfile usa multi-stage build (SDK para build, runtime para execução) resultando em imagem leve (~220MB). O container roda com usuário não-root por segurança.

**Escalabilidade:** A arquitetura permite facilmente:
- Colocar um Application Load Balancer (ALB) na frente.
- Criar um Auto Scaling Group com múltiplas instâncias EC2.
- O banco no RDS já está separado, então escalar a API horizontalmente é direto.

## 5. Monitoramento (CloudWatch Logs)

**Decisão:** Serilog com sinks para Console, Arquivo e AWS CloudWatch.

**Justificativa:** Serilog fornece logging estruturado (propriedades como `Application`, `RequestId`, etc.) que facilita buscas no CloudWatch. Em ambiente local, os logs vão para console e arquivo (`logs/api-*.log`). Em produção na AWS, o sink `AWS.Logger.SeriLog` envia logs diretamente ao CloudWatch Logs.

**Configuração:** Via `appsettings.json` e variáveis de ambiente, sem necessidade de recompilar para trocar o nível de log.

## 6. Segurança

| Aspecto | Implementação |
|---------|--------------|
| Credenciais | Variáveis de ambiente (nunca hardcoded no código) |
| CORS | Origens permitidas configuráveis via `AllowedOrigins` |
| IAM | Role EC2 com apenas `CloudWatchLogsFullAccess` |
| Container | Execução com usuário não-root (`appuser`) |
| Banco | RDS em subnet privada, acessível apenas pelo EC2 |
| Validação | Validação de entrada na camada de Service |
| Erros | Middleware centralizado que não expõe stack traces em produção |

## 7. Infraestrutura como Código

**Decisão:** CloudFormation para a infraestrutura AWS.

**Justificativa:** É o serviço nativo da AWS, sem dependências externas. O template cria VPC, subnets, security groups, RDS, EC2 e CloudWatch Log Group de forma declarativa e reprodutível.

**Alternativa considerada:** Terraform — mais flexível e multi-cloud, mas CloudFormation é mais simples para um projeto 100% AWS e não exige instalação adicional.

## 8. Ambiente Local

**Decisão:** docker-compose orquestrando PostgreSQL, API e frontend (via nginx).

**Justificativa:** Um único `docker-compose up` sobe todo o ambiente de desenvolvimento, simulando a arquitetura de produção. O PostgreSQL simula o RDS, o nginx simula o S3 (servindo arquivos estáticos), e a API roda da mesma forma que rodaria no EC2.
