#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5100


ENV ASPNETCORE_URLS=http://+:5100

RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser


FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["BudgetBook.PaymentCollection/BudgetBook.PaymentCollection.csproj", "BudgetBook.PaymentCollection/"]


#GitHub
RUN --mount=type=secret,id=GH_OWNER,dst=/GH_OWNER --mount=type=secret,id=GH_PAT,dst=/GH_PAT \
    dotnet nuget add source --username USERNAME --password `cat /GH_PAT` --store-password-in-clear-text --name github "https://nuget.pkg.github.com/`cat /GH_OWNER`/index.json"


RUN dotnet restore "BudgetBook.PaymentCollection/BudgetBook.PaymentCollection.csproj"
COPY . .
WORKDIR "/src/BudgetBook.PaymentCollection"
RUN dotnet build "BudgetBook.PaymentCollection.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BudgetBook.PaymentCollection.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BudgetBook.PaymentCollection.dll"]