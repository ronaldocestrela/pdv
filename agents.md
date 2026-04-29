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

Base URL da API (local): **`/api`**. Todas as rotas abaixo exigem `Authorization: Bearer <JWT>` exceto onde indicado.

### Auth
- `POST /auth/login`
- `POST /auth/refresh`

### Products (Fase 2)
- `GET /api/products` — listagem (policy `product.view`)
- `GET /api/products/{id}` — detalhe com variações (policy `product.view`)
- `POST /api/products` — corpo `{ name, isActive }` (policy `product.create`)
- `PUT /api/products/{id}` — `{ name, isActive }` (policy `product.update`)
- `DELETE /api/products/{id}` — (policy `product.delete`)

### Variations (Fase 2)
- `POST /api/variations` — `{ productId, name, barcode | null, stockQuantity, unitPrice }` (policy `variation.create`)
- `PUT /api/variations/{id}` — `{ name, barcode | null, stockQuantity, unitPrice }` (policy `variation.update`)
- `DELETE /api/variations/{id}` — (policy `variation.delete`)

(O claim `variation.view` existe para consistência futura; leituras de variações ocorrem via `GET /api/products/{id}`.)

### Stock (Fase 3)
- `POST /api/stock/adjust` — `{ productVariationId, quantity, reason | null }` (policy `stock.adjust`)
- `GET /api/stock/movements` — query opcional `variationId`, `take` (default 100, máx. 500) (policy `stock.view`)

### Sales (Fase 4)
- `POST /api/sales` — `{ items: [{ productVariationId, quantity }], paymentMethod }` — `paymentMethod`: `cash` \| `card` \| `pix`; preço unitário é o cadastrado na variação (`GET /api/products/{id}` → `variations[].unitPrice`) (policy `sale.create`)
- `GET /api/sales` — query opcional `take` (policy `sale.view`)

### Reports (Fase 5)
- `GET /api/reports/sales` — query `fromUtc`, `toUtc` (policy `report.view`) → `{ saleCount, totalAmount }`
- `GET /api/reports/top-products` — query `fromUtc`, `toUtc`, `take` (default 20, máx. 100) (policy `report.view`)
- `GET /api/reports/cashflow` — query `fromUtc`, `toUtc`, `take` (default 100, máx. 500) (policy `cashflow.view`)
- `GET /api/reports/stock` — query `take` (default 500, máx. 500) (policy `report.view`)

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
- Login (`/login`)
- Home (`/`)
- **Produtos** (`/products`) — Fase 2; catálogo + CRUD; link para variações
- **Variações do produto** (`/products/:productId/variations`) — Fase 2; CRUD de variações (preço unitário, estoque, barcode opcional)
- **Estoque** (`/stock`) — Fase 3; entrada de estoque + histórico; `stock.adjust` / `stock.view`
- **PDV** (`/pdv`) — Fase 4; busca, carrinho, pagamento (dinheiro/cartão/PIX); `sale.create` / `sale.view`; requer `product.view` para catálogo
- **Relatórios** (`/reports`) — Fase 5; vendas por período, top produtos, fluxo de caixa, estoque atual; `report.view` / `cashflow.view` conforme seções visíveis
- Gestão de usuários/roles

Design de referência (Stitch MCP): [`docs/design/stitch-phase2-pdv-ui.md`](docs/design/stitch-phase2-pdv-ui.md), [`docs/design/stitch-phase3-stock-ui.md`](docs/design/stitch-phase3-stock-ui.md), [`docs/design/stitch-phase4-pdv-ui.md`](docs/design/stitch-phase4-pdv-ui.md), [`docs/design/stitch-phase5-reports-ui.md`](docs/design/stitch-phase5-reports-ui.md).

### PDV
- Busca de produtos
- Adição rápida ao carrinho
- Seleção de variação
- Finalização com método de pagamento

---

## 9. Permissões (Frontend)

- Controladas por ação (JWT claims espelham `KnownPermissions`)
- Produtos/variações (Fase 2): `product.view`, `product.create`, `product.update`, `product.delete`; `variation.create`, `variation.update`, `variation.delete`; `variation.view` reservado
- Estoque (Fase 3): `stock.adjust`, `stock.view`
- Vendas / PDV (Fase 4): `sale.create`, `sale.view`
- Relatórios (Fase 5): `report.view`, `cashflow.view`
- Helpers: `usePermission` / `can('product.create')` etc.

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

## 15. Layout do repositório (Fase 0)

Após a fundação, o código vive em:

- `backend/` — solução .NET (`Pdv.sln`): Domain, Application, Infrastructure, API; testes em `tests/Pdv.Tests`
- `frontend/pdv-web/` — React + Vite + TypeScript (`services/`, `hooks/`, `store/`)

Execução e variáveis de ambiente: ver [`README.md`](README.md) na raiz do projeto.

---

## 16. Observações finais

- Sistema focado em performance de PDV
- UX deve ser rápida e com poucos cliques
- Priorizar simplicidade no MVP

