# Stage 1: build/publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution + csproj first (better layer caching)
COPY Santander.BestStories.sln ./

COPY Santander.BestStories/Santander.BestStories.Api.csproj Santander.BestStories/
COPY Santander.BestStories.Application/Santander.BestStories.Application.csproj Santander.BestStories.Application/
COPY Santander.BestStories.InfraStructure/Santander.BestStories.InfraStructure.csproj Santander.BestStories.InfraStructure/

# Restore
RUN dotnet restore Santander.BestStories/Santander.BestStories.Api.csproj

# Copy the remaining source
COPY . .

# Publish
RUN dotnet publish Santander.BestStories/Santander.BestStories.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Default container port
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Santander.BestStories.Api.dll"]
