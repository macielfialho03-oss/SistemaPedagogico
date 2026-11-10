FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/SistemaPedagogico.Web/SistemaPedagogico.Web.csproj -c Release -o /out
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV PORT=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SistemaPedagogico.Web.dll"]
