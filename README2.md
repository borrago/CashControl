Próximo Backlog Recomendado

  Eu seguiria nesta ordem:

  1. Fechar o modelo de tenancy.
     Definir claramente se todos os dados de negócio serão obrigatoriamente ligados a Tenant, como essa informação entra nas entidades e como será aplicada nas queries e comandos.
  2. Criar o núcleo financeiro mínimo.
     Começar por:

  - Categorias
  - Contas
  - Lançamentos
    Esses três módulos já permitem o primeiro fluxo real de produto.

  3. Definir regras de lançamento.
     Antes de construir muita tela, vale fechar:

  - receita vs despesa
  - status do lançamento
  - data de competência vs pagamento
  - recorrência
  - transferência entre contas

  4. Entregar um MVP navegável no frontend.
     Depois do núcleo:

  - listagem de contas
  - CRUD de categorias
  - CRUD de lançamentos
  - filtros básicos
  - resumo simples por período

  5. Adicionar auditoria e observabilidade.
     Principalmente para:

  - impersonação
  - exclusão
  - alterações administrativas
  - erros de autenticação/autorização

  6. Só então refinar permissões.
     Se o produto pedir, evoluir de roles simples para permissões por capacidade.

  Resumo direto: auth está suficientemente pronta; o melhor investimento agora é modelar bem tenant + contas + categorias + lançamentos. Se quiser, eu posso transformar isso no próximo passo em um backlog técnico objetivo de MVP com ordem de implementação.


  1. Definir navegação real para usuário logado: talvez Início, Perfil, Admin quando aplicável, e Sair.
  2. Melhorar home logada para virar dashboard inicial.
  3. Criar tratamento visual para “API indisponível” e “sessão expirada”.
  4. Revisar o PWA com estratégia de update/cache antes de produção.
  5. Adicionar testes de fluxo no frontend para login/logout/guard.

