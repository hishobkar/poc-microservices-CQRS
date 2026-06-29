
```
dotnet add reference ../../Shared/Shared.csproj
```

```
dotnet add package Swashbuckle.AspNetCore
```

```
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

```
dotnet add package MediatR
```

```
dotnet add package MassTransit
```

### RabitMQ Community edition:
```
dotnet add package MassTransit.RabbitMQ --version 7.3.1
```

##  install the Ocelot NuGet package in your ApiGateway project.
```
dotnet add package Ocelot
```


# Build all services
docker-compose build

# Run all services
docker-compose up -d

# View logs for specific service
docker-compose logs -f articleservice

# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v




--- 
### rebuild and restart:
---

```
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```


















### Build and Run

# Build all services
docker-compose build

# Start all services including Kafka
docker-compose up -d

# Check Kafka topics
docker exec -it kafka kafka-topics --list --bootstrap-server localhost:9092

# View logs
docker-compose logs -f

# Test creating an article
curl -X POST http://localhost:5000/api/articles \
  -H "Content-Type: application/json" \
  -d '{"title":"Kafka Test","content":"Testing Kafka integration","author":"Developer"}'

# Check query service - should see the article
curl http://localhost:5000/api/articles



```
docker-compose logs articleservice
docker-compose logs queryservice
docker-compose logs notificationservice
docker-compose logs apigateway
```


### Just filter error logs
```
docker-compose logs | findstr "fail\|error\|Error\|Exception"
```











# Step 1 — stop everything and remove old images:
```
docker-compose down --rmi local
```

# Step 2 — rebuild from scratch (no cache):
```
docker-compose build --no-cache
```

# Watch this output carefully. If there's a compile error during the dotnet build step it will appear here. Share any ERROR lines from this step if it fails.

# Step 3 — start fresh:
```
docker-compose up -d
```

# Step 4 — confirm images are new:
```
docker images | findstr "backend"
```

# The timestamps should show today's date.





# to rebuild just the API Gateway
```
docker-compose build --no-cache apigateway
```
