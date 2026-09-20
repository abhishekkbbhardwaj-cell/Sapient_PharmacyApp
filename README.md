<<<<<<< HEAD
# Sapient_PharmacyApp
=======
# Sapient Pharmacy App

A simple pharmacy application with an ASP.NET Core Web API and Angular UI.

## Structure


## Requirements


## Run the API

```bash
cd API
dotnet run --urls http://localhost:5010
```

The API runs at `http://localhost:5010`.

## Run the UI

```bash
cd UI
npm install
ng serve
```

The UI runs at `http://localhost:4200`.

## API Endpoints


Medicine data is stored in `API/Database/medicines.json`.

## Run Tests

### API tests

```bash
dotnet test API.Tests/PharmacyApp.Api.Tests.csproj
```

### UI tests

```bash
cd UI
ng test --no-watch --no-progress
```

## Build

```bash
cd API
dotnet build

cd ../UI
ng build
```
>>>>>>> 2bb68c6 (feat: add medicine management functionality with add and list pages)
