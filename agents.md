# AGENTS.md — Sistema de Vendas (PDV + Estoque) MVP

## 1. Visão Geral
Sistema de vendas de produtos físicos (PDV) com gestão de estoque.

- Backend: .NET 10 (Controllers + CQRS com MediatR)
- Banco: SQL Server
- Frontend: React + Vite (Client-Server)
- Autenticação: JWT + Refresh Token
- Modelo: Multi-tenant com isolamento lógico (em implantação)

## 2. Escopo do MVP

### Funcionalidades principais
- Venda simples (produto → pagamento → finaliza)
- Controle de estoque (entrada/saída automática)
- Produtos com variações (estoque por variação)
- Cadastro de Fornecedores (CRUD completo com isolamento por tenant)
- Relatórios básicos
- Sistema de permissões granular por ação

### Fora do escopo (MVP)
- NFC-e
- Impressão
- Tempo real
- Offline

---

## 3. Arquitetura

### Backend (Monolito Modular + CQRS)
O backend é estruturado como um Monolito Modular composto por contextos delimitados (módulos) independentes. Cada módulo contém internamente suas próprias camadas (Domain, Application, Infrastructure, API Controllers) e isola suas regras de negócio.

A estrutura geral de diretórios do backend é:
```
/src
  /Pdv.Shared.Kernel       (Abstrações comuns, TenantContext, Exceptions, Behaviors)
  /Pdv.Modules.Identity    (Autenticação, gerenciamento de Usuários, Roles e Permissões)
  /Pdv.Modules.Catalog     (Cadastro de Produtos e Variações)
  /Pdv.Modules.Stock       (Movimentações de Estoque e ajustes)
  /Pdv.Modules.Sales       (Fluxo de Caixa e finalização de Vendas)
  /Pdv.Modules.Reports     (Consultas e dashboards de relatórios entre módulos)
  /Pdv.API                 (Host executável ASP.NET Core)
```

Dentro de cada módulo, a organização segue os princípios de Clean Architecture:
```
/Pdv.Modules.[Name]
  /Controllers          (Pontos de entrada da API HTTP do módulo)
  /Application          (Commands, Queries, Handlers, DTOs, Validators)
  /Domain               (Entidades, Enums, Value Objects e regras de negócio)
  /Infrastructure       (DbContext próprio, mapeamento de tabelas EF, Repositórios)
```

### Princípios de Comunicação e Design
- **CQRS:** Separação clara entre Commands (escrita) e Queries (leitura).
- **Desacoplamento:** Módulos não podem referenciar diretamente a implementação interna de outros módulos.
- **Comunicação Síncrona:** Feita enviando Queries/Commands do MediatR cujos contratos de dados (DTOs) são compartilhados.
- **Comunicação Assíncrona:** Feita via Eventos de Integração (MediatR Notifications) para propagar efeitos colaterais de forma reativa (ex: baixar estoque pós-venda).
- **Banco de Dados:** Cada módulo gerencia seu próprio `DbContext` contendo apenas suas tabelas. No MVP, compartilham o mesmo banco físico (SQL Server), mas com tabelas logicamente delimitadas.
- **Validações:** Regras de entrada validadas via FluentValidation.


---

## 4. Domínio

### Entidades principais

#### User
- Id
- TenantId
- Email
- PasswordHash
- IsActive

#### Role
- Id
- TenantId
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
- TenantId
- Name
- IsActive

#### ProductVariation
- Id
- TenantId
- ProductId
- Name (ex: Verde GG)
- Barcode (opcional)
- StockQuantity

#### StockMovement
- Id
- TenantId
- ProductVariationId
- Type (IN / OUT)
- Quantity
- CreatedAt

#### Sale
- Id
- TenantId
- CreatedAt
- TotalAmount
- PaymentMethod

#### SaleItem
- Id
- TenantId
- SaleId
- ProductVariationId
- Quantity
- UnitPrice

#### CashFlow
- Id
- TenantId
- Type (IN / OUT)
- Amount
- Description
- CreatedAt

#### Supplier
- Id
- TenantId
- Name (obrigatório, máx. 256)
- Document (opcional, CNPJ/CPF, único por tenant, máx. 32)
- Email (opcional, máx. 256)
- Phone (opcional, máx. 20)
- IsActive

#### Tenant
- Id
- Name (único, máx. 100 chars)
- IsActive
- CreatedAtUtc

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
- Claims adicionais de contexto: `tenant_id` e `is_super_admin`

Fluxo:
1. Login → gera JWT + RefreshToken
2. Expiração → usa refresh token
3. Sessão inclui `tenantId` na resposta de autenticação

---

## 7. Endpoints (exemplos)

Base URL da API (local): **`/api`**. Todas as rotas abaixo exigem `Authorization: Bearer <JWT>` exceto onde indicado.

Erros: respostas **Problem Details** (`application/problem+json`); ver [`docs/api-errors.md`](docs/api-errors.md).

### Auth
- `POST /api/auth/login`
- `POST /api/auth/refresh`

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

### Permissões — catálogo (Fase 6)
- `GET /api/permissions` — lista nomes de permissões no banco (policy composta `admin.roles.read`: claim `role.manage` **ou** `user.manage`)

### Roles (Fase 6)
- `GET /api/roles` — listagem com permissões por role (policy `admin.roles.read`)
- `GET /api/roles/{id}` — detalhe (policy `admin.roles.read`)
- `POST /api/roles` — `{ name }` (policy `role.manage`)
- `PUT /api/roles/{id}` — `{ name }` (policy `role.manage`); role **Super Admin** não altera nome
- `DELETE /api/roles/{id}` — (policy `role.manage`); **Super Admin** não pode ser excluída
- `PUT /api/roles/{id}/permissions` — `{ permissionNames: string[] }` substitui o conjunto (policy `role.manage`); **Super Admin** não permite edição manual de permissões

### Users — administração (Fase 6)
- `GET /api/users` — usuários com `roleIds` (policy `user.manage`)
- `POST /api/users` — `{ email, password, isActive? }` cria usuário com senha hasheada; senha mín. 6 caracteres (policy `user.manage`)
- `PUT /api/users/{id}/roles` — `{ roleIds: number[] }` substitui roles do usuário (policy `user.manage`)

### Suppliers (Fase 11)
- `GET /api/suppliers` — listagem de fornecedores (policy `supplier.view`)
- `GET /api/suppliers/{id}` — detalhe do fornecedor (policy `supplier.view`)
- `POST /api/suppliers` — `{ name, document | null, email | null, phone | null, isActive }` (policy `supplier.create`); `document` deve ser único por tenant quando informado
- `PUT /api/suppliers/{id}` — `{ name, document | null, email | null, phone | null, isActive }` (policy `supplier.update`)
- `DELETE /api/suppliers/{id}` — (policy `supplier.delete`)

### Tenants — cadastro e gestão (Fase 10)
- `POST /api/tenants/register` — `{ name, adminEmail, adminPassword }` auto-registro público (sem auth); cria tenant + role Super Admin + usuário admin
- `POST /api/tenants` — mesma payload; requer `tenant.manage` (criação via painel Super Admin)
- `GET /api/tenants` — lista todos os tenants (policy `tenant.manage`)
- `PUT /api/tenants/{id}/activate` — `{ isActive: bool }` ativa/desativa tenant (policy `tenant.manage`)

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
- **Fornecedores** (`/suppliers`) — Fase 11; CRUD de fornecedores (nome, CNPJ/CPF, e-mail, telefone, ativo); `supplier.view`, `supplier.create`, `supplier.update`, `supplier.delete`
- **PDV** (`/pdv`) — Fase 4; busca, carrinho, pagamento (dinheiro/cartão/PIX); `sale.create` / `sale.view`; requer `product.view` para catálogo
- **Relatórios** (`/reports`) — Fase 5; vendas por período, top produtos, fluxo de caixa, estoque atual; `report.view` / `cashflow.view` conforme seções visíveis
- **Usuários** (`/users`) — Fase 6; criar usuário (e-mail, senha, ativo), listagem e atribuição de roles; `user.manage`
- **Roles** (`/roles`) — Fase 6; CRUD de roles e atribuição de permissões (`role.manage`); leitura de lista/detalhe também com `user.manage` (somente leitura na UI sem `role.manage`)

Design de referência (Stitch MCP): [`docs/design/stitch-phase2-pdv-ui.md`](docs/design/stitch-phase2-pdv-ui.md), [`docs/design/stitch-phase3-stock-ui.md`](docs/design/stitch-phase3-stock-ui.md), [`docs/design/stitch-phase4-pdv-ui.md`](docs/design/stitch-phase4-pdv-ui.md), [`docs/design/stitch-phase5-reports-ui.md`](docs/design/stitch-phase5-reports-ui.md), [`docs/design/stitch-phase6-users-roles-ui.md`](docs/design/stitch-phase6-users-roles-ui.md), [`docs/design/stitch-phase7-tests-ui.md`](docs/design/stitch-phase7-tests-ui.md) (QA / mapa cenário ↔ teste).

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
- Administração (Fase 6): `user.manage`, `role.manage` (policy extra no backend: `admin.roles.read` para `GET /api/permissions`, `GET /api/roles`, `GET /api/roles/{id}` quando o usuário tem `user.manage` ou `role.manage`)
- Tenants (Fase 10): `tenant.manage` (Super Admin global)
- Fornecedores (Fase 11): `supplier.create`, `supplier.update`, `supplier.delete`, `supplier.view`
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
- Multi-tenant avançado (troca de tenant em sessão e UX administrativa cross-tenant)
- NFC-e
- Offline-first

---

## 13. Diretrizes para Agents/LLM

- Sempre utilizar .NET 10 no backend
- Sempre seguir CQRS
- Nunca misturar leitura com escrita
- Sempre validar regras de negócio
- Sempre criar testes
- Atualizar documentação ao final de cada alteração
- **.NET:** Todas as funções públicas devem possuir um comentário XML `<summary>` descrevendo seu propósito.
- **Frontend:** Todo código JavaScript/TypeScript deve ter comentários de bloco descrevendo a funcionalidade dos módulos e funções principais.
- **UUIDs/Guids:** Sempre utilizar UUIDs (tipo `Guid` no C# / `string` no TypeScript) para identificadores de todas as entidades do sistema (chaves primárias, estrangeiras e TenantIds). Não utilizar inteiros auto-incrementais para IDs.

---

## 14. Definição de Pronto (DoD)

Uma feature só está pronta se:
- Código implementado
- Testes passando
- Documentação atualizada
- Permissões aplicadas

---

## 15. Layout do repositório (Fase 0)

Após a migração para a arquitetura modular, o código vive em:

- `backend/` — solução .NET (`Pdv.slnx`):
  - `src/Pdv.Shared.Kernel`
  - `src/Pdv.Modules.Identity`
  - `src/Pdv.Modules.Catalog`
  - `src/Pdv.Modules.Stock`
  - `src/Pdv.Modules.Sales`
  - `src/Pdv.Modules.Reports`
  - `src/Pdv.API` (Host)
  - testes em `tests/Pdv.Tests`
- `frontend/pdv-web/` — React + Vite + TypeScript (`services/`, `hooks/`, `store/`)

Execução e variáveis de ambiente: ver [`README.md`](README.md) na raiz do projeto.

---

## 16. Observações finais

- Sistema focado em performance de PDV
- UX deve ser rápida e com poucos cliques
- Priorizar simplicidade no MVP

