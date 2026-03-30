# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Accept build configuration as argument (default to Release for production)
ARG CONFIGURATION=Release

# Copy solution and source
COPY PersonalFinanceServices.sln ./
COPY src/ ./src/

# Restore and build
RUN dotnet restore
RUN dotnet build -c ${CONFIGURATION}

# Publish
RUN dotnet publish src/Services/PersonalFinance.Services.Transactions/PersonalFinance.Services.Transactions.csproj \
    -c ${CONFIGURATION} -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install debugger for Debug builds
ARG CONFIGURATION=Release
RUN if [ "$CONFIGURATION" = "Debug" ] ; then \
      apt-get update && \
      apt-get install -y --no-install-recommends unzip && \
      rm -rf /var/lib/apt/lists/*; \
    fi

COPY --from=build /app/publish .

#EXPOSE PORTS
EXPOSE 5100

ENTRYPOINT ["dotnet", "PersonalFinance.Services.Transactions.dll"]
