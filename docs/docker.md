# Docker Environment

The easiest way to bootstrap the entire monorepo is through Docker Compose.

## Spinning up the Stack
Navigate to the root directory `d:\DataSSD\PersonalFinanceApp\personal-finance-app` and execute:
```bash
docker-compose -p finance-flow up --build -d
```

## Containers Managed
- **finance-sqlserver**: MSSQL Database Host
- **finance-rabbitmq**: ActiveMQ messaging broker for cross-service events
- **finance-apigateway**: The YARP core entry point (`localhost:5000`)
- **finance-usermanagement**: User/Auth domain service
- **finance-accounts**: Core banking simulation
- **finance-obligations**: Loans and subscriptions logic
- **finance-transactions**: In/Out ledger data logic
- **NextJS web app**: The root application frontend.
