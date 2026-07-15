FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Pdd.ir.slnx .
COPY Pdd.ir.Business/Pdd.ir.Business.csproj Pdd.ir.Business/
COPY Pdd.ir.Client/Pdd.ir.Client.csproj Pdd.ir.Client/
COPY Pdd.ir.Data/Pdd.ir.Data.csproj Pdd.ir.Data/
COPY Pdd.ir.Server/Pdd.ir.Server.csproj Pdd.ir.Server/
RUN dotnet restore

COPY . .
RUN dotnet publish Pdd.ir.Server/Pdd.ir.Server.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Pdd.ir.Server.dll"]
