# Sistema de Análisis de Ventas (SDV) - ETL Data Warehouse

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![MySQL](https://img.shields.io/badge/MySQL-8.0-4479A1?logo=mysql&logoColor=white)](https://www.mysql.com/)
[![Entity Framework](https://img.shields.io/badge/EF%20Core-8.0-512BD4)](https://docs.microsoft.com/ef/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Sistema ETL (Extract, Transform, Load) para análisis de ventas implementado con Clean Architecture en .NET 8. El proyecto construye un Data Warehouse con modelo Star Schema para análisis dimensional.

## 📋 Tabla de Contenidos

- [Arquitectura](#-arquitectura)
- [Fases del Proyecto](#-fases-del-proyecto)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Modelo de Datos](#-modelo-de-datos)
- [Requisitos](#-requisitos)
- [Instalación](#-instalación)
- [Ejecución](#-ejecución)
- [Tecnologías](#-tecnologías)

## 🏗 Arquitectura

El proyecto implementa **Clean Architecture** con las siguientes capas:

```
┌─────────────────────────────────────┐
│ Presentation (SDV.WorkerService)    │ ← Worker Service, DI Container
├─────────────────────────────────────┤
│ Application (SDV.Application)       │ ← Casos de uso, Orquestación
├─────────────────────────────────────┤
│ Core (SDV.Core)                     │ ← Entidades, Interfaces
├─────────────────────────────────────┤
│ Infrastructure (SDV.Infrastructure) │ ← EF Core, Repositorios, Extractors
└─────────────────────────────────────┘
```

### Patrones Implementados

| Patrón | Implementación |
|--------|----------------|
| **Strategy** | Extractores y Loaders intercambiables |
| **Repository** | Abstracción de acceso a datos |
| **Dependency Injection** | Constructor injection en todas las capas |
| **Unit of Work** | DbContext de Entity Framework |

## 📊 Fases del Proyecto

### ✅ Fase 1: Extracción (E) - Completada
Extracción de datos desde múltiples fuentes hacia tablas Staging:

| Fuente | Extractor | Destino |
|--------|-----------|---------|
| CSV (customers.csv) | `CsvCustomerExtractor` | StagingCustomers |
| API REST (Mock) | `ApiProductExtractor` | StagingProducts |
| Base de Datos | `DatabaseOrderExtractor` | StagingOrders |

### ✅ Fase 2: Carga de Dimensiones (L) - Completada
Carga de datos desde Staging hacia tablas de dimensiones del Data Warehouse:

| Loader | Dimensión | Tipo |
|--------|-----------|------|
| `CustomerDimensionLoader` | DimCustomer | SCD Tipo 2 |
| `ProductDimensionLoader` | DimProduct | SCD Tipo 2 |
| `TimeDimensionLoader` | DimTime | Verificación |
| `StatusDimensionLoader` | DimStatus | Verificación |

### 🔲 Fase 3: Transformación y Carga de Hechos (T+L) - Pendiente
- Transformación de datos
- Carga de tabla de hechos `FactSales`

## 📁 Estructura del Proyecto

```
SistemaDeVentas/
├── data/                          # Archivos CSV de datos fuente
│   ├── customers.csv
│   ├── products.csv
│   └── orders.csv
├── scripts/
│   └── Script ventas.sql          # DDL del Data Warehouse
├── src/
│   ├── SDV.Core/                  # Capa de dominio
│   │   ├── Entities/
│   │   │   ├── Staging/           # Entidades de staging
│   │   │   │   ├── StagingCustomer.cs
│   │   │   │   ├── StagingProduct.cs
│   │   │   │   ├── StagingOrder.cs
│   │   │   │   └── StagingOrderDetail.cs
│   │   │   └── Dimensions/        # Entidades de dimensiones
│   │   │       ├── DimCustomer.cs
│   │   │       ├── DimProduct.cs
│   │   │       ├── DimTime.cs
│   │   │       └── DimStatus.cs
│   │   └── Interfaces/
│   │       ├── IDataExtractor.cs
│   │       ├── IStagingRepository.cs
│   │       ├── IDimensionRepository.cs
│   │       └── IDimensionLoader.cs
│   │
│   ├── SDV.Application/           # Capa de aplicación
│   │   └── UseCases/
│   │       ├── ExtractDataUseCase.cs
│   │       └── LoadDimensionsUseCase.cs
│   │
│   ├── SDV.Infrastructure/        # Capa de infraestructura
│   │   ├── Data/
│   │   │   ├── StagingDbContext.cs
│   │   │   └── Repositories/
│   │   │       ├── StagingRepository.cs
│   │   │       └── DimensionRepository.cs
│   │   ├── Extractors/            # Extractores de datos
│   │   │   ├── Csv/
│   │   │   │   └── CsvCustomerExtractor.cs
│   │   │   ├── Api/
│   │   │   │   └── ApiProductExtractor.cs
│   │   │   └── Database/
│   │   │       └── DatabaseOrderExtractor.cs
│   │   └── Loaders/               # Cargadores de dimensiones
│   │       ├── CustomerDimensionLoader.cs
│   │       ├── ProductDimensionLoader.cs
│   │       ├── TimeDimensionLoader.cs
│   │       └── StatusDimensionLoader.cs
│   │
│   └── SDV.WorkerService/         # Capa de presentación
│       ├── Program.cs
│       ├── EtlWorker.cs
│       └── appsettings.json
│
└── SistemaDeVentas.sln
```

## 🗄 Modelo de Datos

### Star Schema

```
                    ┌─────────────┐
                    │  DimTime    │
                    │─────────────│
                    │ TimeKey(PK) │
                    │ FullDate    │
                    │ Year        │
                    │ Quarter     │
                    │ Month       │
                    └──────┬──────┘
                           │
┌─────────────┐    ┌───────┴───────┐    ┌─────────────┐
│ DimCustomer │    │   FactSales   │    │ DimProduct  │
│─────────────│    │───────────────│    │─────────────│
│CustomerKey  │◄───│ CustomerKey   │───►│ ProductKey  │
│ CustomerID  │    │ ProductKey    │    │ ProductID   │
│ FirstName   │    │ TimeKey       │    │ ProductName │
│ LastName    │    │ StatusKey     │    │ Category    │
│ Email       │    │ Quantity      │    │ Price       │
│ City        │    │ TotalPrice    │    │ IsCurrent   │
│ IsCurrent   │    └───────┬───────┘    └─────────────┘
└─────────────┘            │
                    ┌──────┴──────┐
                    │  DimStatus  │
                    │─────────────│
                    │ StatusKey   │
                    │ StatusName  │
                    └─────────────┘
```

### SCD Tipo 2 (Slowly Changing Dimension)

Las dimensiones `DimCustomer` y `DimProduct` implementan SCD Tipo 2 para rastrear cambios históricos:

| Campo | Descripción |
|-------|-------------|
| `StartDate` | Fecha de inicio de validez |
| `EndDate` | Fecha de fin (9999-12-31 = activo) |
| `IsCurrent` | Flag de registro actual |

## 📋 Requisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MySQL 8.0+](https://dev.mysql.com/downloads/)
- Visual Studio 2022 / VS Code / Rider

## 🚀 Instalación

1. **Clonar el repositorio**
```bash
git clone https://github.com/Neta2603/SistemaDeVentas.git
cd SistemaDeVentas
```

2. **Crear la base de datos**
```bash
mysql -u root -p < scripts/Script\ ventas.sql
```

3. **Configurar conexión** en `src/SDV.WorkerService/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "StagingDb": "Server=127.0.0.1;Port=3307;Database=SalesDataWarehouse;User=tu_usuario;Password=tu_password;"
  }
}
```

4. **Restaurar paquetes**
```bash
dotnet restore
```

5. **Compilar proyecto**
```bash
dotnet build
```

## ▶ Ejecución

```bash
dotnet run --project src/SDV.WorkerService/SDV.WorkerService.csproj
```

## 🛠 Tecnologías

| Tecnología | Versión | Uso |
|------------|---------|-----|
| .NET | 8.0 | Framework principal |
| Entity Framework Core | 8.0 | ORM |
| Pomelo.EntityFrameworkCore.MySql | 8.0 | Proveedor MySQL |
| Serilog | Latest | Logging estructurado |
| CsvHelper | Latest | Parsing de CSV |
| MySQL | 8.0 | Base de datos |

## 👤 Autor

**Edward Neftalí Liriano Gómez**
- Matrícula: 2022-0437
- GitHub: [@Neta2603](https://github.com/Neta2603)

## 📄 Licencia

Este proyecto es parte de la asignatura **Electiva 1 (Big Data)** del ITLA.
Profesor: Francis Ramírez
