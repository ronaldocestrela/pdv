# 🚀 Roadmap de Desenvolvimento — Sistema PDV + Estoque (MVP)

## 🎯 Objetivo
Construir um sistema de vendas (PDV) com controle de estoque, permissões granulares e arquitetura escalável usando .NET + React.

---

## 🔰 Fase 0 — Fundação do Projeto

### Backend
- Criar solução .NET
- Estruturar camadas:
  - Domain
  - Application
  - Infrastructure
  - API
- Configurar MediatR
- Configurar FluentValidation
- Configurar SQL Server + DbContext
- Criar migrations iniciais

### Frontend
- Criar projeto React + Vite
- Estrutura base:
  - services
  - hooks
  - store
- Configurar client HTTP

### ✅ Entrega
- Backend e frontend rodando
- Conexão com banco funcionando

---

## 🔐 Fase 1 — Autenticação + Autorização

### Backend
- Entidades:
  - User
  - Role
  - Permission
  - UserRole
  - RolePermission
- Login com JWT
- Refresh Token
- Seed inicial:
  - Super Admin
  - Permissões base

### Frontend
- Tela de login
- Armazenamento de token
- Middleware de autenticação
- Helper de permissão (`can()`)

### ✅ Entrega
- Login funcional
- Controle de acesso ativo

---

## 📦 Fase 2 — Produtos e Variações

### Backend
- Entidades:
  - Product
  - ProductVariation
- Commands:
  - CreateProduct
  - UpdateProduct
  - CreateVariation
  - UpdateVariation

### Regras
- Estoque por variação
- Barcode opcional

### Frontend
- Tela de produtos
- Tela de variações

### ✅ Entrega
- CRUD de produtos funcionando

---

## 📊 Fase 3 — Estoque

### Backend
- Entidade: StockMovement
- Command: AddStock

### Regras
- Sem estoque negativo
- Histórico obrigatório

### Frontend
- Tela de ajuste de estoque

### ✅ Entrega
- Controle de estoque confiável

---

## 💰 Fase 4 — PDV (Vendas)

### Backend
- Entidades:
  - Sale
  - SaleItem
  - CashFlow

- Command: CreateSale

### Fluxo
1. Validar estoque
2. Criar venda
3. Criar itens
4. Baixar estoque
5. Registrar movimentação
6. Registrar entrada no caixa

### Frontend
- Tela de PDV:
  - Busca de produtos
  - Seleção de variação
  - Carrinho
  - Finalização

### Pagamento
- Dinheiro
- Cartão
- PIX

### ✅ Entrega
- Venda completa funcionando

---

## 📈 Fase 5 — Relatórios

### Backend (Queries)
- Vendas por período
- Produtos mais vendidos
- Fluxo de caixa
- Estoque atual

### Frontend
- Tela de relatórios

### ✅ Entrega
- Visão gerencial básica

---

## 👥 Fase 6 — Usuários e Permissões

### Backend
- CRUD de roles
- Atribuição de permissões
- Atribuição de roles a usuários

### Frontend
- Tela de usuários
- Tela de roles

### ✅ Entrega
- Sistema administrável

---

## 🧪 Fase 7 — Testes (TDD)

### Backend
- Testes de:
  - Venda
  - Estoque
  - Autenticação

### Regra
- Nenhuma feature sem teste

### ✅ Entrega
- Base confiável

---

## 🧹 Fase 8 — Refino

- Melhorias de UX no PDV
- Otimização de queries
- Tratamento de erros
- Logs básicos

### ✅ Entrega
- Sistema pronto para uso real

---

## 📌 Ordem Recomendada

1. Fundação
2. Auth + Permissões
3. Produtos
4. Estoque
5. PDV
6. Relatórios
7. Usuários/Roles
8. Testes + Refino

---

## 🚧 Próximas Evoluções (Pós-MVP)

- Integração com gateway de pagamento
- Impressão térmica
- Multi-tenant
- NFC-e
- Offline-first

---

## ⚠️ Diretrizes

- Seguir CQRS rigorosamente
- Não misturar leitura e escrita
- Controllers sem regra de negócio
- Sempre validar regras de domínio
- Sempre manter testes atualizados

---

## ✅ Definição de Pronto (DoD)

Uma feature só está pronta quando:
- Código implementado
- Testes passando
- Permissões aplicadas
- Documentação atualizada
