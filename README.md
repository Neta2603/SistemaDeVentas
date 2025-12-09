# Sistema de Análisis de Ventas - Proceso ETL

Sistema completo de extracción, transformación y carga (ETL) desarrollado en .NET 8 con arquitectura limpia, implementando un Data Warehouse en modelo Star Schema para análisis de ventas.

### Características Principales

- ✅ **Arquitectura Clean Architecture** con separación de capas (Core, Application, Infrastructure, Worker)
- ✅ **SOLID Principles** aplicados en toda la solución
- ✅ **Star Schema** con 4 dimensiones y 1 tabla de hechos
- ✅ **SCD Type 2** para tracking histórico de cambios en dimensiones
- ✅ **Proceso ETL en 3 Fases** completamente automatizado
- ✅ **Múltiples fuentes de datos** (CSV, REST API, Database)
- ✅ **Logging estructurado** con Serilog
- ✅ **Carga en batch optimizada** para alto rendimiento

## 🏗️ Arquitectura del Sistema

```
┌─────────────────────────────────────────────────────────────┐
│                    FUENTES DE DATOS                         │
├─────────────────────────────────────────────────────────────┤
│  📄 CSV Files  │  🌐 REST APIs  │  💾 External Database    │
└─────────┬───────────────┬────────────────┬─────────────────┘
          │               │                │
          ▼               ▼                ▼
┌─────────────────────────────────────────────────────────────┐
│                    STAGING LAYER                            │
│  • StagingCustomers     • StagingProducts                   │
│  • StagingOrders        • StagingOrderDetails               │
└──────────────────────────┬──────────────────────────────────┘
                           │ ETL Transformación
                           ▼
┌─────────────────────────────────────────────────────────────┐
│              DATA WAREHOUSE (Star Schema)                   │
├─────────────────────────────────────────────────────────────┤
│                    DIMENSIONES                              │
│  • DimCustomer (SCD Type 2)                                 │
│  • DimProduct (SCD Type 2)                                  │
│  • DimTime (Precalculada 2020-2030)                         │
│  • DimStatus (Estados de órdenes)                           │
├─────────────────────────────────────────────────────────────┤
│                   TABLA DE HECHOS                           │
│  • FactSales (Grain: línea de detalle por orden)            │
│    - CustomerKey, ProductKey, TimeKey, StatusKey (FKs)      │
│    - Quantity, UnitPrice, TotalPrice (Métricas)             │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
                    Power BI Dashboards
```

## 📁 Estructura del Proyecto

```
SistemaDeVentas/
│
├── src/
│   ├── SDV.Core/                          # Capa de dominio
│   │   ├── Entities/
│   │   │   ├── Staging/                   # Entidades staging
│   │   │   ├── Dimensions/                # Entidades dimensiones
│   │   │   └── Facts/                     # Entidades hechos
│   │   └── Interfaces/                    # Contratos e interfaces
│   │
│   ├── SDV.Application/                   # Capa de aplicación
│   │   └── UseCases/
│   │       ├── ExtractDataUseCase.cs      # Fase 1: Extracción
│   │       ├── LoadDimensionsUseCase.cs   # Fase 2: Dimensiones
│   │       └── LoadFactsUseCase.cs        # Fase 3: Facts
│   │
│   ├── SDV.Infrastructure/                # Capa de infraestructura
│   │   ├── Data/
│   │   │   ├── StagingDbContext.cs        # EF Core DbContext
│   │   │   └── Repositories/              # Repositorios
│   │   ├── Extractors/
│   │   │   ├── Csv/                       # Extractores CSV
│   │   │   ├── Api/                       # Extractores API
│   │   │   └── Database/                  # Extractores Database
│   │   └── Loaders/                       # Loaders de dimensiones y facts
│   │
│   └── SDV.WorkerService/                 # Worker Service (punto de entrada)
│       ├── Program.cs                     # Configuración DI
│       ├── EtlWorker.cs                   # Orquestador ETL
│       └── appsettings.json               # Configuración
│
├── data/                                  # Archivos CSV de entrada
│   ├── customers.csv
│   ├── products.csv
│   ├── orders.csv
│   └── order_details.csv
│
└── scripts/
   └── Script_ventas.sql                  # Script DDL del Data Warehouse
```

## 🚀 Requisitos

- **.NET 8 SDK** o superior
- **MySQL 8.0+** (puerto 3307 por defecto)
- **Visual Studio 2022** o **VS Code** con extensión C#
- Cliente MySQL

## ⚙️ Configuración Inicial

### 1. Clonar el Repositorio

```bash
git clone https://github.com/Neta2603/SistemaDeVentas.git
cd SistemaDeVentas
```

### 2. Configurar Base de Datos

Editar `src/SDV.WorkerService/appsettings.json` con tus credenciales MySQL:

```json
{
  "ConnectionStrings": {
    "StagingDb": "Server=127.0.0.1;Port=3307;Database=SalesDataWarehouse;User=TuUsuario;Password=TuPassword;"
  }
}
```

### 3. Crear el Data Warehouse

Ejecutar el script SQL para crear la estructura completa:

```bash
mysql -h 127.0.0.1 -P 3307 -u TuUsuario -p < scripts/Script_ventas.sql
```

### 4. Preparar Datos de Entrada

Verificar que los archivos CSV estén en la carpeta `data/`:

```bash
data/
├── customers.csv       # Datos de clientes
├── products.csv        # Catálogo de productos
├── orders.csv          # Órdenes de venta
└── order_details.csv   # Líneas de detalle de órdenes
```

## Ejecución del Proceso ETL

```bash
# Compilar el proyecto
dotnet build

# Ejecutar el Worker Service
dotnet run --project src/SDV.WorkerService/SDV.WorkerService.csproj
```4

## Fases del Proceso ETL

### Fase 1: Extracción (E)
Extrae datos desde múltiples fuentes hacia tablas Staging:

- **CSV → StagingCustomers** 
- **REST API → StagingProducts** 
- **Database → StagingOrders** 
- **CSV → StagingOrderDetails** 

### Fase 2: Carga de Dimensiones (L)
Transforma y carga datos desde Staging hacia Dimensiones con SCD Type 2:

- **DimStatus**
- **DimTime** 
- **DimCustomer** 
- **DimProduct** 

### Fase 3: Carga de Facts (F)
Consolida hechos de ventas integrando todas las dimensiones:

1. **Limpieza:** TRUNCATE de FactSales (migrate:fresh)
2. **Transformación:** JOIN de staging con lookups a dimensiones
3. **Carga:** Inserción batch optimizada de FactSales

# Limpiar y reconstruir
dotnet clean
dotnet build --no-incremental
```

## Dependencias Principales

- **Microsoft.EntityFrameworkCore** - ORM para acceso a datos
- **Pomelo.EntityFrameworkCore.MySql** - Provider MySQL para EF Core
- **Serilog** - Logging estructurado
- **CsvHelper** - Lectura/escritura de archivos CSV

## Autor

**Edward Neftali Liriano Gomez - 2022-0437**  
Electiva 1: Big Data - Profesor Francis Ramírez

