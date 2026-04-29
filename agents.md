# AGENTS.md — Sistema de Vendas (PDV + Estoque) MVP

## 1. Visão Geral
Sistema de vendas de produtos físicos (PDV) com gestão de estoque.

- Backend: .NET (Controllers + CQRS com MediatR)
- Banco: SQL Server
- Frontend: React + Vite (Client-Server)
- Autenticação: JWT + Refresh Token
- Modelo: Single-tenant

## 2. Escopo do MVP

### Funcionalidades principais
- Venda simples (produto → pagamento → finaliza)
- Controle de estoque (entrada/saída automática)
- Produtos com variações (estoque por variação)
- Relatórios básicos
- Sistema de permissões granular por ação

### Fora do escopo (MVP)
- NFC-e
- Impressão
- Tempo real
- Offline

---

## 3. Arquitetura

### Backend (CQRS)
Separação clara entre:
- Commands (escrita)
- Queries (leitura)

Estrutura:
```
/src
  /Application
    /Commands
    /Queries
    /Handlers
    /DTOs
  /Domain
    /Entities
    /Enums
    /ValueObjects
  /Infrastructure
    /Persistence
    /Repositories
  /API
    /Controllers
```

### Princípios
- Cada operação de escrita = 1 Command
- Cada leitura = 1 Query
- Handlers NÃO acessam diretamente o banco (usar repositórios)
- Validações via FluentValidation

---

## 4. Domínio

### Entidades principais

#### User
- Id
- Email
- PasswordHash
- IsActive

#### Role
- Id
- Name

#### Permission
- Id
- Name (ex: product.create)

#### RolePermission
- RoleId
- PermissionId

#### UserRole
- UserId
- RoleId

#### Product
- Id
- Name
- IsActive

#### ProductVariation
- Id
- ProductId
- Name (ex: Verde GG)
- Barcode (opcional)
- StockQuantity

#### StockMovement
- Id
- ProductVariationId
- Type (IN / OUT)
- Quantity
- CreatedAt

#### Sale
- Id
- CreatedAt
- TotalAmount
- PaymentMethod

#### SaleItem
- Id
- SaleId
- ProductVariationId
- Quantity
- UnitPrice

#### CashFlow
- Id
- Type (IN / OUT)
- Amount
- Description
- CreatedAt

---

## 5. Regras de Negócio

### Venda
- Não permitir venda com estoque insuficiente
- Ao finalizar venda:
  - Criar Sale + Items
  - Baixar estoque automaticamente
  - Registrar StockMovement (OUT)
  - Registrar entrada no CashFlow

### Estoque
- Toda movimentação deve gerar histórico
- Não permitir estoque negativo

### Permissões
- Baseadas em ações (string)
- Exemplo:
  - product.create
  - product.update
  - sale.create
  - sale.view

### Super Admin
- Pode criar/editar roles
- Pode atribuir permissões
- Acesso total ao sistema

---

## 6. Autenticação

- JWT para acesso
- Refresh Token persistido no banco

Fluxo:
1. Login → gera JWT + RefreshToken
2. Expiração → usa refresh token

---

## 7. Endpoints (exemplos)

### Auth
- POST /auth/login
- POST /auth/refresh

### Products
- POST /products
- GET /products
- PUT /products/{id}

### Variations
- POST /variations
- PUT /variations/{id}

### Sales
- POST /sales
- GET /sales

### Reports
- GET /reports/sales
- GET /reports/top-products
- GET /reports/cashflow
- GET /reports/stock

---

## 8. Frontend

### Estrutura
```
/src
  /pages
  /components
  /services
  /hooks
  /store
```

### Telas
- Login
- PDV
- Produtos
- Estoque
- Relatórios
- Gestão de usuários/roles

### PDV
- Busca de produtos
- Adição rápida ao carrinho
- Seleção de variação
- Finalização com método de pagamento

---

## 9. Permissões (Frontend)

- Controladas por ação
- Exemplo:
```
if (can('product.create')) { ... }
```

---

## 10. Testes (TDD obrigatório)

### Backend
- Testes de Command/Handler
- Testes de regras de negócio

### Frontend
- Testes de componentes críticos

Regra:
- Nenhuma feature sem teste

---

## 11. Convenções

- Commands: CreateProductCommand
- Queries: GetProductsQuery
- Handlers: CreateProductHandler

- Controllers apenas orquestram
- Nunca colocar regra de negócio no Controller

---

## 12. Roadmap pós-MVP

- Integração com gateway (Stone)
- Impressão térmica
- Multi-tenant
- NFC-e
- Offline-first

---

## 13. Diretrizes para Agents/LLM

- Sempre seguir CQRS
- Nunca misturar leitura com escrita
- Sempre validar regras de negócio
- Sempre criar testes
- Atualizar documentação ao final de cada alteração

---

## 14. Definição de Pronto (DoD)

Uma feature só está pronta se:
- Código implementado
- Testes passando
- Documentação atualizada
- Permissões aplicadas

---

## 15. Observações finais

- Sistema focado em performance de PDV
- UX deve ser rápida e com poucos cliques
- Priorizar simplicidade no MVP

