FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto primeiro para cache de restore
COPY ["SportsEquipment.Api/SportsEquipment.Api.csproj", "SportsEquipment.Api/"]
COPY ["SportsEquipment.Application/SportsEquipment.Application.csproj", "SportsEquipment.Application/"]
COPY ["SportsEquipment.Domain/SportsEquipment.Domain.csproj", "SportsEquipment.Domain/"]
COPY ["SportsEquipment.Infrastructure/SportsEquipment.Infrastructure.csproj", "SportsEquipment.Infrastructure/"]
COPY ["SportsEquipment.Messaging/SportsEquipment.Messaging.csproj", "SportsEquipment.Messaging/"]

# Restaurar dependências
RUN dotnet restore "SportsEquipment.Api/SportsEquipment.Api.csproj"

# Copiar todo o código
COPY . .

# Build e publish
WORKDIR "/src/SportsEquipment.Api"
RUN dotnet build "SportsEquipment.Api.csproj" -c Release -o /app/build
RUN dotnet publish "SportsEquipment.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SportsEquipment.Api.dll"]