# Stitch — tela de login (Fase 1)

## Nota de contexto (multitenancy)

- O layout de login permanece o mesmo.
- Após autenticação, a sessão inclui `tenantId` junto de tokens/permissões para escopo lógico de dados.

Gerado pelo MCP Stitch (`generate_screen_from_text`), projeto Google **Responsive UI Builder**, `projectId=17300137737028814945`, sessão `1432264250308378931`.

- **Paleta:** fundo off-white `#fbf8fa`, primário navy `#091426`, ação `#2170e4` (secondary/container), erro `#ba1a1a`.
- **Tipografia:** Inter.

A implementação React em [`src/pages/Login.tsx`](../src/pages/Login.tsx) segue esses tokens para fidelidade visual ao design gerado.
