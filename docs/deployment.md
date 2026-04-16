# Deployment Guidelines

## Local Deployment
Refer to `docker.md` for running the stack locally during development.

## Production Checklist (Azure / Digital Ocean)
1. **Migrations**: Verify all EF Core migrations run automatically on startup via retry-patterns.
2. **Gateway Verification**: The frontend must point its `.env` configuration purely towards the `YARP` API Gateway proxy, not the underlying microservices directly.
3. **NGINX**: Ensure reverse proxy settings are passing standard headers (`X-Forwarded-For`).
4. **Secrets**: Update all Docker environment configurations to utilize vault secrets rather than localized connection strings.
