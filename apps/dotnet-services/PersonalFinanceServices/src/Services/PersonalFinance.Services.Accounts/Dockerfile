# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and source
COPY PersonalFinanceServices.sln ./
COPY src/ ./src/

# Restore and build
RUN dotnet restore
RUN dotnet build -c Release

# Publish
RUN dotnet publish src/Services/PersonalFinance.Services.Accounts/PersonalFinance.Services.Accounts.csproj \
    -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

#EXPOSE PORTS
EXPOSE 5200

ENTRYPOINT ["dotnet", "PersonalFinance.Services.Accounts.dll"]
