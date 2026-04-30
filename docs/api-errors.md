# Erros da API (RFC 7807)

Respostas de erro da API seguem **Problem Details** (`Content-Type: application/problem+json`).

## Campos comuns

| Campo | Descrição |
|-------|-----------|
| `type` | URI de referência do tipo de problema |
| `title` | Título curto |
| `status` | HTTP status |
| `detail` | Mensagem para o usuário (principalmente validação com um único erro) |
| `code` | Código interno: `validation` ou `unexpected` |
| `correlationId` | Rastreio da requisição (também em `X-Correlation-Id` na resposta) |
| `errors` | Lista `{ field, message, code }` quando `code` = `validation` |

## Validação (400)

Disparada por regras de negócio e FluentValidation (`ValidationException`).

Exemplo resumido:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation failed",
  "status": 400,
  "detail": "Mensagem do primeiro erro",
  "code": "validation",
  "correlationId": "abc123",
  "errors": [
    { "field": "items", "message": "...", "code": "..." }
  ]
}
```

## Erro inesperado (500)

`code`: `unexpected`. Em **Production**, `detail` é genérico; em **Development**, pode incluir a mensagem da exceção.

## Cabeçalho de correlação

- Envie opcionalmente `X-Correlation-Id`; a API repete o mesmo valor na resposta.
- Se omitido, a API gera um id e devolve no header.

Middleware: `CorrelationIdMiddleware`, `ApiExceptionHandlingMiddleware` em `Pdv.API`.
