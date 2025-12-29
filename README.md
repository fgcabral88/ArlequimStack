# Sports Equipment API

API backend para controle de estoque e catálogo de equipamentos esportivos, desenvolvida com **Clean Architecture**.

---

## 📌 Stack Tecnológico

- **Arquitetura**: Clean Architecture (Domain, Application, Infrastructure, API)
- **Banco de dados**: MySQL (Pomelo.EntityFrameworkCore.MySql)
- **Autenticação/Autorização**: JWT com roles (`Administrator`, `Seller`)
- **Logs**: Serilog
- **Mensageria**: RabbitMQ via MassTransit
- **Observabilidade**: OpenTelemetry + Jaeger (opcional)
- **Testes**: xUnit, Moq, FluentAssertions
- **Containers**: Docker Compose

---

## ✅ Funcionalidades

### Core
- **Usuários**: Cadastro, login e JWT com roles
- **Produtos**: CRUD completo com preço modelado como Value Object
- **Estoque**: Controle de entrada/saída com nota fiscal
- **Pedidos**: Criação com validação de estoque e baixa automática

### Mensageria
- Publicação de eventos `OrderCreatedEvent` após commit de transação
- Transações atômicas garantindo consistência BD → Evento
- Consumer configurado com retry policies

### Administração
- Health check do RabbitMQ
- Monitoramento de filas (mensagens, consumidores)
- Purge de filas com permissão de administrador

---

## 🚀 Como Executar

### Opção 1: Docker Compose (Recomendado)

Executa toda a stack em containers (API + MySQL + RabbitMQ + Jaeger).

#### 1. Criar Migrations (apenas primeira vez)

```powershell
dotnet ef migrations add InitialCreate --project SportsEquipment.Infrastructure --startup-project SportsEquipment.Api
```

#### 2. Subir containers

```powershell
docker-compose up --build
```

#### 3. Acessar serviços

- **API/Swagger**: http://localhost:8080/swagger
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **Jaeger UI**: http://localhost:16686 (se habilitado)

#### 4. Parar

```powershell
docker-compose down
```

---

### Opção 2: Execução Local (Visual Studio)

Executa a API localmente, conectando nos serviços do Docker.

#### 1. Subir apenas os serviços (sem a API)

```powershell
docker-compose up mysql rabbitmq jaeger -d
```

#### 2. Aguardar serviços ficarem prontos (~30s)

```powershell
# Ver logs do MySQL
docker logs -f sports-mysql

# Aguarde: "mysqld: ready for connections"
```

#### 3. Aplicar migrations

```powershell
dotnet ef database update --project SportsEquipment.Infrastructure --startup-project SportsEquipment.Api
```

#### 4. Rodar API no Visual Studio

- Abra o projeto no Visual Studio
- Selecione o perfil **"SportsEquipment.Api"** ou **"http"**
- Clique em **Play** (F5)

Acesse: http://localhost:5069/swagger

#### 5. Parar serviços

```powershell
docker-compose stop
```

---

## 🗄️ Como Consultar o Banco de Dados

### MySQL Workbench

#### Conexão com o MySQL do Docker:

1. Abra o MySQL Workbench
2. Crie uma nova conexão com os seguintes dados:

```
Connection Name: SportsEquipment Docker
Hostname: 127.0.0.1
Port: 3307
Username: root
Password: Database@2026*
Default Schema: sports_equipment_db
```

3. Clique em **Test Connection**
4. Clique em **OK**
5. Conecte e navegue pelas tabelas

#### Queries úteis:

```sql
-- Ver todas as tabelas
SHOW TABLES;

-- Ver estrutura de uma tabela
DESCRIBE Users;

-- Consultar dados
SELECT * FROM Users;
SELECT * FROM Products;
SELECT * FROM Orders;
SELECT * FROM Stocks;

-- Ver pedidos com itens
SELECT o.Id, o.ClientDocument, o.TotalAmount, oi.Quantity, oi.UnitPrice
FROM Orders o
INNER JOIN OrderItems oi ON o.Id = oi.OrderId;
```

### Via Terminal (Alternativa)

```powershell
# Conectar no MySQL
mysql -h 127.0.0.1 -P 3307 -u root -p
# Senha: Database@2026*

# Dentro do MySQL
USE sports_equipment_db;
SHOW TABLES;
SELECT * FROM Users;
```

---

## 🔁 Fluxo de uso

### 1. Criar Admin e fazer login

```http
POST /api/users
{
  "name": "Admin",
  "email": "admin@example.com",
  "password": "Admin@123",
  "role": "Administrator"
}

POST /api/users/login
{
  "email": "admin@example.com",
  "password": "Admin@123"
}
```

### 2. Criar produto e adicionar estoque

```http
POST /api/products
{
  "name": "Bola de Futebol",
  "description": "Bola oficial",
  "price": 150.00,
  "category": "Futebol"
}

POST /api/stocks/{productId}/entries
{
  "quantity": 100,
  "invoiceNumber": "NF-001"
}
```

### 3. Criar Seller e pedido

```http
POST /api/users
{
  "name": "Vendedor",
  "email": "seller@example.com",
  "password": "Seller@123",
  "role": "Seller"
}

POST /api/orders
{
  "clientDocument": "12345678900",
  "sellerName": "João Silva",
  "items": [
    {
      "productId": "{id-do-produto}",
      "quantity": 2
    }
  ]
}
```

**Resultado**: Pedido criado → Estoque baixado → Evento publicado no RabbitMQ

---

## 🔧 Endpoints Administrativos (RabbitMQ)

| Endpoint                                  | Método | Auth         | Descrição      |
|-------------------------------------------|--------|--------------|----------------|
| `/api/admin/rabbitmq/health`              | GET    |       -      | Health check   |
| `/api/admin/rabbitmq/queues/orders`       | GET    | Admin/Seller | Info das filas |
| `/api/admin/rabbitmq/queues/orders/purge` | POST   | Admin        | Limpar filas   |

**Como validar que está funcionando:**

```bash
# Verificar filas
curl http://localhost:8080/api/admin/rabbitmq/queues/orders
```

**Resposta esperada (sucesso):**
```json
{
  "queues": [
    {
      "name": "order-created-queue",
      "messageCount": 0,
      "consumerCount": 1,
      "exists": true
    },
    {
      "name": "order-created-queue_error",
      "messageCount": 0,
      "exists": false
    }
  ]
}
```

---

## 📊 Fluxo de Mensageria

**Ordem garantida:**
1. Iniciar transação
2. Salvar pedido + atualizar estoque
3. **Commit** da transação
4. Publicar evento (somente se commit OK)
5. Consumer processa evento

**Garantias:**
- ✅ Eventos publicados apenas após commit bem-sucedido
- ✅ Validação e serialização antes da publicação
- ✅ Retry policies configuradas
- ✅ Logs detalhados

---

## 🧪 Testes

```powershell
# Executar testes
dotnet test

# Com cobertura
dotnet test /p:CollectCoverage=true
```

---

## ⚙️ Arquivos de Configuração

### appsettings.Development.json (Execução Local)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3307;Database=sports_equipment_db;User=root;Password=Database@2026*"
  },
  "RabbitMq": {
    "Uri": "rabbitmq://localhost",
    "User": "guest",
    "Password": "guest"
  }
}
```

### appsettings.Docker.json (Execução Docker)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=mysql;Port=3306;Database=sports_equipment_db;User=root;Password=Database@2026*"
  },
  "RabbitMq": {
    "Uri": "rabbitmq://rabbitmq",
    "User": "guest",
    "Password": "guest"
  }
}
```

**Diferenças:**
- **Local**: `localhost:3307` (porta externa do container)
- **Docker**: `mysql:3306` (nome do serviço na rede interna)

---

## 🔧 Troubleshooting

### Migrations não criadas

```powershell
dotnet ef migrations add InitialCreate --project SportsEquipment.Infrastructure --startup-project SportsEquipment.Api
```

### Tabelas não existem no banco

```powershell
# Aplicar migrations
dotnet ef database update --project SportsEquipment.Infrastructure --startup-project SportsEquipment.Api
```

Ou rode a API - as migrations são aplicadas automaticamente na inicialização.

### Erro: Host not allowed to connect

MySQL bloqueando conexão externa. Solução:

```powershell
docker-compose down -v
docker-compose up --build
```

O `docker-compose.yml` já está configurado com `MYSQL_ROOT_HOST: '%'`.

### RabbitMQ demora para conectar

Normal - leva ~30 segundos. A API faz retry automático.

### Ver logs

```powershell
docker logs -f sports-api
docker logs -f sports-mysql
docker logs -f sports-rabbitmq
```

### Limpar tudo e recomeçar

```powershell
docker-compose down -v
docker-compose up --build
```

---

## 📁 Estrutura do Projeto

```
SportsEquipment/
├── SportsEquipment.Domain/          # Entidades, Value Objects
├── SportsEquipment.Application/     # Casos de uso, DTOs
├── SportsEquipment.Infrastructure/  # EF Core, Repositories
├── SportsEquipment.Api/             # Controllers, JWT
├── SportsEquipment.Messaging/       # Events, Consumers
├── SportsEquipment.Tests/           # Testes
└── docker-compose.yml
```

---

## 🧠 Decisões Técnicas

- **Clean Architecture**: Separação clara de responsabilidades
- **Domain-Driven Design**: Domínio rico com Value Objects (Money, Email)
- **Unit of Work + Repository**: Abstração de persistência
- **Transações Atômicas**: Consistência garantida entre BD e mensageria
- **Event-Driven**: Mensageria assíncrona com RabbitMQ
- **Observabilidade**: Logs estruturados + tracing distribuído
- **Dual Mode**: Suporte para execução Docker e Local

---

## ✅ Status

✅ Execução via Docker Compose  
✅ Execução local via Visual Studio  
✅ Mensageria com transações atômicas  
✅ Endpoints administrativos  
✅ Pronto para produção  

---

## 👨‍💻 Autor

**Felipe Gabriel Cabral**

Projeto demonstrando: Clean Architecture, DDD, Event-Driven Architecture, Docker, RabbitMQ, Observability