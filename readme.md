# Entendendo Desafio Técnico - Microserviços

## Descrição do Desafio

Desenvolver uma aplicação com arquitetura de **microserviços** para gerenciamento de **estoque de produtos** e **vendas** em uma plataforma de e-commerce. O sistema será composto por dois microserviços: um para gerenciar o **estoque** de produtos e outro para gerenciar as **vendas**, com comunicação entre os serviços via **API Gateway**.

**Tecnologias:** .NET Core, C#, Entity Framework, RESTful API, RabbitMQ (para comunicação entre microserviços), JWT (para autenticação) e banco de dados relacional.

---

## Arquitetura Proposta

- **Microserviço 1 (Gestão de Estoque):** Responsável por cadastrar produtos, controlar o estoque e fornecer informações sobre a quantidade disponível.
- **Microserviço 2 (Gestão de Vendas):** Responsável por gerenciar os pedidos e interagir com o serviço de estoque para verificar a disponibilidade de produtos ao realizar uma venda.
- **API Gateway:** Roteamento das requisições para os microserviços adequados. Este serviço atua como o ponto de entrada para todas as chamadas de API.
- **RabbitMQ:** Usado para comunicação assíncrona entre os microserviços, como notificações de vendas que impactam o estoque.
- **Autenticação com JWT:** Garantir que somente usuários autenticados possam realizar ações de vendas ou consultar o estoque.

---

## Funcionalidades Requeridas

### Microserviço 1 (Gestão de Estoque):
- **Cadastro de Produtos:** Adicionar novos produtos com nome, descrição, preço e quantidade em estoque.
- **Consulta de Produtos:** Permitir que o usuário consulte o catálogo de produtos e a quantidade disponível em estoque.
- **Atualização de Estoque:** O estoque deve ser atualizado quando ocorrer uma venda (integração com o Microserviço de Vendas).

### Microserviço 2 (Gestão de Vendas):
- **Criação de Pedidos:** Permitir que o cliente faça um pedido de venda, com a validação do estoque antes de confirmar a compra.
- **Consulta de Pedidos:** Permitir que o usuário consulte o status dos pedidos realizados.
- **Notificação de Venda:** Quando um pedido for confirmado, o serviço de vendas deve notificar o serviço de estoque sobre a redução do estoque.

### Comum aos dois microserviços:
- **Autenticação via JWT:** Apenas usuários autenticados podem interagir com os sistemas de vendas ou consultar o estoque.
- **API Gateway:** Usar um gateway para centralizar o acesso à API, garantindo que as requisições sejam direcionadas ao microserviço correto.

---

## Contexto do Negócio

A aplicação simula um sistema para uma plataforma de e-commerce, onde empresas precisam gerenciar seu estoque de produtos e realizar vendas de forma eficiente. A solução deve ser escalável e robusta, com separação clara entre as responsabilidades de **estoque** e **vendas**, utilizando boas práticas de **arquitetura de microserviços**. Esse tipo de sistema é comum em empresas que buscam flexibilidade e alta disponibilidade em ambientes com grande volume de transações.

---

## Requisitos Técnicos e Critérios de Aceitação

- **Tecnologia:** .NET Core (C#) para construir as APIs.
- **Banco de Dados:** Usar **Entity Framework** com banco de dados relacional.
- **Microserviços:**
    - O serviço de **Gestão de Estoque** deve permitir cadastrar produtos, consultar estoque e atualizar quantidades.
    - O serviço de **Gestão de Vendas** deve validar a disponibilidade de produtos, criar pedidos e reduzir o estoque.
- **Comunicação:** Usar **RabbitMQ** для comunicação assíncrona.
- **Autenticação:** Implementar autenticação via **JWT** para proteger os endpoints.
- **API Gateway:** Usar um **API Gateway** para centralizar e redirecionar as requisições.
- **Boas Práticas:** Seguir boas práticas de design de API RESTful, tratamento de exceções e validações.
- **Critérios de Aceitação:** O sistema deve permitir o cadastro de produtos, a criação de pedidos com validação de estoque, comunicação via RabbitMQ, e ser seguro com autenticação JWT e permissões específicas.

# Desafio Técnico: Arquitetura de Microsserviços para E-commerce

Este repositório contém a implementação de uma solução de back-end para uma plataforma de e-commerce, desenvolvida como parte de um desafio técnico. O projeto demonstra uma arquitetura robusta e moderna baseada em microsserviços, com foco em escalabilidade, segurança e observabilidade.

## Sobre o Projeto

A finalidade deste projeto é simular o núcleo de um sistema de e-commerce, separando as responsabilidades de gerenciamento de produtos/estoque e o processamento de vendas em serviços independentes e resilientes. A comunicação entre os serviços é feita de forma síncrona e assíncrona, seguindo as melhores práticas de arquiteturas distribuídas.

Além dos requisitos funcionais, foram implementadas funcionalidades extras de nível profissional, como um stack completo de observabilidade (logs, métricas e dashboards) e proteção de segurança a nível de gateway.

## Arquitetura

O sistema é composto por um conjunto de serviços orquestrados via Docker Compose. O **API Gateway** atua como um *Back-end for Front-end* (BFF), sendo o ponto de entrada único para todas as requisições do cliente e roteando o tráfego para os três microsserviços principais: Identidade, Estoque e Pedidos.

- **API Gateway (`gateway-api`)**: Roteamento (Ocelot), autenticação, rate limiting e agregação de documentação Swagger.
- **Serviço de Identidade (`identity-api`)**: Registro e autenticação de usuários (Clientes e Admins) e emissão de tokens JWT (Duende IdentityServer).
- **Serviço de Estoque (`stock-api`)**: Gerenciamento do catálogo e quantidade de produtos.
- **Serviço de Pedidos (`order-api`)**: Gerenciamento de pedidos, validação de estoque e publicação de eventos de venda.
- **RabbitMQ**: Message broker (MassTransit) para comunicação assíncrona (ex: notificar o `stock-api` após a criação de um pedido).
- **Banco de Dados (MySQL)**: Cada microsserviço possui seu próprio banco de dados isolado.
- **Stack de Observabilidade**:
    - **Seq**: Servidor para centralização de logs estruturados.
    - **Prometheus**: Coleta de métricas de performance.
    - **Grafana**: Visualização das métricas em dashboards.

## Tecnologias e Padrões Utilizados

- **Back-end**: .NET 9, C#, ASP.NET Core
- **Banco de Dados**: MySQL, Entity Framework Core
- **Mensageria**: RabbitMQ com MassTransit
- **Segurança**:
    - Duende IdentityServer para OpenID Connect / OAuth 2.0
    - Autenticação com JSON Web Tokens (JWT)
    - Autorização baseada em Papéis (Roles: Admin vs. Customer)
    - Rate Limiting (proteção contra DoS) no API Gateway
- **Observabilidade**: Serilog, Seq, Prometheus-net, Grafana
- **Infraestrutura**: Docker & Docker Compose
- **Padrões**: SOLID, Injeção de Dependência, Padrão de Repositório, Options Pattern, Métodos de Extensão.

## Pré-requisitos

- [Docker](https://www.docker.com/products/docker-desktop/)
- [Docker Compose](https://docs.docker.com/compose/install/)

## Como Executar o Projeto

1.  **Clone o repositório:**
    ```bash
    git clone https://github.com/Paulo-Victor-Belens/microservicos_avanade.git
    cd deploy
    ```

2.  **Crie o arquivo de ambiente:**
    Este projeto usa um arquivo `.env` para gerenciar as variáveis de ambiente. Um arquivo de exemplo `.env.example` está incluído. Na pasta que contém o `docker-compose.yml`, crie uma cópia dele e renomeie-a para `.env`.

    ```bash
    # Exemplo no Linux/macOS ou Git Bash
    cp .env.example .env
    ```
    > **Importante:** Abra o arquivo `.env` e altere a variável `JWT_KEY` para um segredo forte e único.

3.  **Suba os containers:**
    Execute o comando abaixo na pasta que contém o `docker-compose.yml`. Isso irá construir as imagens das APIs e iniciar todos os containers.
    ```bash
    docker-compose up --build -d
    ```

## Acessando os Serviços

Após a inicialização, os seguintes serviços estarão disponíveis:

| Serviço | URL de Acesso | Credenciais (usuário / senha) |
| :--- | :--- | :--- |
| **API Gateway (Swagger)**| `http://localhost:8000/swagger` | N/A |
| **Seq (Logs)** | `http://localhost:5341` | `admin` / O que estiver em `SEQ_ADMIN_PASSWORD` |
| **Grafana (Métricas)** | `http://localhost:3000` | `admin` / `admin` (ou o que estiver no `.env`) |
| **RabbitMQ UI** | `http://localhost:15672` | `guest` / `guest` |
| **Prometheus**| `http://localhost:9090` | N/A |

## Fluxo de Teste Sugerido

1.  **Faça login como Admin:** Chame `POST http://localhost:8000/api/account/login` (via Gateway) com as credenciais `admin@ecommerce.com` e a senha definida no `IdentityDbSeeder` para obter um token JWT.
2.  **Cadastre um Produto:** Com o token de admin, chame `POST http://localhost:8000/api/products` para criar um novo produto. A requisição deve ser bem-sucedida (`201 Created`).
3.  **Registre um Cliente:** Chame `POST http://localhost:8000/api/account/register` com dados de um novo cliente.
4.  **Faça login como Cliente:** Obtenha um token JWT para o cliente recém-criado.
5.  **Tente cadastrar um produto como Cliente:** Chame `POST http://localhost:8000/api/products` com o token do cliente. A requisição deve falhar (`403 Forbidden`).
6.  **Crie um Pedido:** Com o token do cliente, chame `POST http://localhost:8000/api/orders`. A requisição deve ser bem-sucedida (`201 Created`).
7.  **Verifique os Logs:** Acesse o **Seq** e veja os logs estruturados de toda a transação passando pelos microsserviços.

## Próximas Implementações

- **Notificações em Tempo Real com WebSocket:** Implementar uma comunicação via WebSocket (provavelmente utilizando SignalR). Quando o `order-api` confirmar um pedido, ele poderia enviar uma mensagem em tempo real para o cliente (front-end), atualizando a interface do usuário sem a necessidade de recarregar a página.