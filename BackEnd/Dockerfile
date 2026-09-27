FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/GoCare/GoCare.csproj src/GoCare/
RUN dotnet restore src/GoCare/GoCare.csproj

COPY src/GoCare/ src/GoCare/
RUN dotnet publish src/GoCare/GoCare.csproj -c Release -o /app/publish --no-restore

# --- Stage 2: runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "GoCare.dll"]