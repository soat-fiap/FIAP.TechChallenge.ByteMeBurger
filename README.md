# FIAP Tech Challenge - ByteMeBurger API

[![CI](https://github.com/soat-fiap/FIAP.TechChallenge.ByteMeBurger/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/soat-fiap/FIAP.TechChallenge.ByteMeBurger/actions/workflows/dotnet.yml)

## Description
This repository contains the source code for the ByteMeBurger API, part of the FIAP Tech Challenge. The API is designed to manage a burger restaurant's operations, including order placement, customer registration, and product management. The recent updates introduce a new endpoint for customer registration and enhance various aspects of the application, such as error handling, data models, and service configurations.

## Tech challenge deliverables
You can find all Phase 1 deliverables on the [Wiki page](https://github.com/soat-fiap/FIAP.TechChallenge.ByteMeBurger/wiki)
## Getting Started

### Prerequisites
- Docker
- .NET SDK
- Optionally, an IDE such as Visual Studio or VSCode

### Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/soat-fiap/FIAP.TechChallenge.ByteMeBurger.git
   cd FIAP.TechChallenge.ByteMeBurger
    ```

2. Start the services using Docker:

   ```bash
    docker-compose up -d
   ```
3. Testing
  To verify the existing endpoints go to

   > [http://localhost:8080/swagger/index.html](http://localhost:8080/swagger/index.html)

4. Service health
   > [http://localhost:8080/health](http://localhost:8080/health)

5. If you want to quickly seed the database with fake data and test some of the endpoints use the [FIAP_TechChallenge_ByteMeBurger-endpoints.http](FIAP_TechChallenge_ByteMeBurger-endpoints.http) file

6. Stop the services using Docker:

   ```bash
    docker-compose down
   ```

