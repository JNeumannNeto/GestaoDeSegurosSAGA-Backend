FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["ContratacaoService/API/ContratacaoService.API.csproj", "ContratacaoService/API/"]
COPY ["ContratacaoService/Application/ContratacaoService.Application.csproj", "ContratacaoService/Application/"]
COPY ["ContratacaoService/Domain/ContratacaoService.Domain.csproj", "ContratacaoService/Domain/"]
COPY ["ContratacaoService/Infrastructure/ContratacaoService.Infrastructure.csproj", "ContratacaoService/Infrastructure/"]
COPY ["PropostaService/API/PropostaService.API.csproj", "PropostaService/API/"]
COPY ["PropostaService/Application/PropostaService.Application.csproj", "PropostaService/Application/"]
COPY ["PropostaService/Domain/PropostaService.Domain.csproj", "PropostaService/Domain/"]
COPY ["PropostaService/Infrastructure/PropostaService.Infrastructure.csproj", "PropostaService/Infrastructure/"]
COPY ["Shared/Shared.Messaging/Shared.Messaging.csproj", "Shared/Shared.Messaging/"]

# Restore packages
RUN dotnet restore "ContratacaoService/API/ContratacaoService.API.csproj"
RUN dotnet restore "PropostaService/API/PropostaService.API.csproj"

# Copy source code
COPY . .

# Build ContratacaoService
WORKDIR "/src/ContratacaoService/API"
RUN dotnet build "ContratacaoService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build/contratacao

# Build PropostaService
WORKDIR "/src/PropostaService/API"
RUN dotnet build "PropostaService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build/proposta

FROM build AS publish-contratacao
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/ContratacaoService/API"
RUN dotnet publish "ContratacaoService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish/contratacao /p:UseAppHost=false

FROM build AS publish-proposta
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/PropostaService/API"
RUN dotnet publish "PropostaService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish/proposta /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Copy both published applications
COPY --from=publish-contratacao /app/publish/contratacao ./contratacao/
COPY --from=publish-proposta /app/publish/proposta ./proposta/

# Create entrypoint scripts
RUN echo '#!/bin/bash\ncd /app/contratacao && dotnet ContratacaoService.API.dll' > /app/start-contratacao.sh && \
    echo '#!/bin/bash\ncd /app/proposta && dotnet PropostaService.API.dll' > /app/start-proposta.sh && \
    chmod +x /app/start-contratacao.sh && \
    chmod +x /app/start-proposta.sh

# Default entrypoint (can be overridden)
ENTRYPOINT ["/app/start-contratacao.sh"]
