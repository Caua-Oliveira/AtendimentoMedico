FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY AtendimentoMedico/*.csproj .
RUN dotnet restore

COPY AtendimentoMedico/. .

RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AtendimentoMedico.dll"]