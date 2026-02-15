FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/BankMore.Account.API/BankMore.Account.API.csproj", "src/BankMore.Account.API/"]
COPY ["src/BankMore.Core/BankMore.Core.csproj", "src/BankMore.Core/"]
RUN dotnet restore "src/BankMore.Account.API/BankMore.Account.API.csproj"
COPY . .
WORKDIR "/src/src/BankMore.Account.API"
RUN dotnet build "BankMore.Account.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BankMore.Account.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BankMore.Account.API.dll"]
