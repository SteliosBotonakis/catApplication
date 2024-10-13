# Cat Application

## Description
This is a simple application that allows you to view a list of cats and their details.

## Requirements
- .Net 8.0
- Docker
- Docker Compose

## Setup
1. Clone the repository
2. **Build and run the application:**

```sh
docker-compose up --build
```

3. **Access the application:**
   Open your browser and navigate to `http://localhost:8080/index.html`.

## Configuration
The application uses an `appsettings.json` file for configuration. Ensure you have the correct API key and connection strings set up.

###Notes
- Database is deployed with the docker-compose file and can connect at the url: `jdbc:sqlserver://localhost:1433` with username `sa` and password `Password123!`
- Unit tests not implemented
- Minimal validation rules implemented