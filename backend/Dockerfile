FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["AccessFlow.Api/AccessFlow.Api.csproj", "AccessFlow.Api/"]
COPY ["AccessFlow.Application/AccessFlow.Application.csproj", "AccessFlow.Application/"]
COPY ["AccessFlow.Domain/AccessFlow.Domain.csproj", "AccessFlow.Domain/"]
COPY ["AccessFlow.Infrastructure/AccessFlow.Infrastructure.csproj", "AccessFlow.Infrastructure/"]

RUN dotnet restore "AccessFlow.Api/AccessFlow.Api.csproj"

COPY . .

RUN dotnet tool install --global dotnet-ef --version 8.0.11

ENV PATH="$PATH:/root/.dotnet/tools"

RUN dotnet ef migrations bundle --project AccessFlow.Infrastructure --startup-project AccessFlow.Infrastructure --output /app/efbundle

WORKDIR "/src/AccessFlow.Api"

RUN dotnet publish "AccessFlow.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS migrator

WORKDIR /app

COPY --from=build /app/efbundle .

ENTRYPOINT ["./efbundle"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AccessFlow.Api.dll"]