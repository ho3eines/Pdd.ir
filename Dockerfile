FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

ENV DOTNET_SYSTEM_NET_SECURITY_ALLOWRENEGOTIATION=false
ENV DOTNET_SYSTEM_NET_SECURITY_SSLCREDENTIALSCOLLECTIONSDEFAULTMAXCOUNT=0
ENV DOTNET_SYSTEM_NET_SECURITY_SSLREVOCATIONCHECKMODE=NoCheck
ENV DOTNET_SYSTEM_NET_SECURITY_TLSOPTIONS_MAXTLSVERSION=1.3

COPY Pdd.ir.slnx .
COPY Pdd.ir.Business/Pdd.ir.Business.csproj Pdd.ir.Business/
COPY Pdd.ir.Client/Pdd.ir.Client.csproj Pdd.ir.Client/
COPY Pdd.ir.Data/Pdd.ir.Data.csproj Pdd.ir.Data/
COPY Pdd.ir.Server/Pdd.ir.Server.csproj Pdd.ir.Server/
RUN dotnet nuget locals http-cache --clear 2>/dev/null; \
    dotnet restore --source https://api.nuget.org/v3/index.json

COPY . .
RUN dotnet publish Pdd.ir.Server/Pdd.ir.Server.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Pdd.ir.Server.dll"]
