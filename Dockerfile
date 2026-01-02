# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Zumra/Zumra.csproj", "Zumra/"]
RUN dotnet restore "Zumra/Zumra.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Zumra"
RUN dotnet build "Zumra.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Zumra.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create a non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published files
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "Zumra.dll"]
