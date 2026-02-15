# Imagen base para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia el archivo .csproj y restaura dependencias
COPY ["ReservArte_API/ReservArte_API.csproj", "ReservArte_API/"]
RUN dotnet restore "ReservArte_API/ReservArte_API.csproj"

# Copia todo el código fuente
COPY ReservArte_API/ ReservArte_API/

# Compila la aplicación
WORKDIR /src/ReservArte_API
RUN dotnet build "ReservArte_API.csproj" -c Release -o /app/build

# Publica la aplicación
FROM build AS publish
RUN dotnet publish "ReservArte_API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final para ejecutar
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5000
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReservArte_API.dll"]
