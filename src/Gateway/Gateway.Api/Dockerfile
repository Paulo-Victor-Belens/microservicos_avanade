# Estágio 1: Restaurar dependências
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src

# Copia todos os arquivos de projeto para aproveitar o cache do Docker
COPY *.sln .
COPY src/Gateway/Gateway.Api/Gateway.Api.csproj ./src/Gateway/Gateway.Api/
COPY src/Services/StockService/Stock.Api/Stock.Api.csproj ./src/Services/StockService/Stock.Api/
COPY src/Services/OrderService/Order.Api/Order.Api.csproj ./src/Services/OrderService/Order.Api/
COPY src/Services/IdentityService/Identity.Api/Identity.Api.csproj ./src/Services/IdentityService/Identity.Api/
COPY src/Shared/Shared.Kernel/Shared.Kernel.csproj ./src/Shared/Shared.Kernel/

RUN dotnet restore "EcommerceSolution.sln"

# Estágio 2: Build e Publicação
FROM restore AS build
WORKDIR /src

# Copia todo o código-fonte
COPY src/ ./src/

# Publica o projeto específico do Gateway
WORKDIR /src/src/Gateway/Gateway.Api
RUN dotnet publish -c Release -o /app/publish

# Estágio 3: Imagem Final
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Gateway.Api.dll"]
