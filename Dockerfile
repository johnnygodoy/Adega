# ===== Build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copie a solução e o csproj da RAIZ
COPY *.sln ./
COPY *.csproj ./
RUN dotnet restore

# copie o restante e publique
COPY . .
RUN dotnet publish -c Release -o /app/publish

# ===== Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# pasta gravável p/ SQLite
RUN mkdir -p /data

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Adega.dll"]
