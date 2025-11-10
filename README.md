# Sistema de Análisis de Ventas - ETL

Sistema ETL (Extract, Transform, Load) para análisis de datos de ventas, desarrollado con .NET 8 y arquitectura limpia.

## 📋 Descripción

Este proyecto implementa un proceso ETL que extrae datos de ventas desde múltiples fuentes (CSV, API, Base de datos), los almacena en un área de staging y prepara los datos para su posterior transformación y carga en un Data Warehouse.

**Estado actual:** ✅ **Fase de Extracción (E) completada**

## 🏗️ Arquitectura

El proyecto sigue los principios de **Clean Architecture** y está dividido en 4 capas:

```
SistemaDeVentas/
├── src/
│   ├── SDV.Core/              # Entidades y contratos
│   ├── SDV.Application/       # Casos de uso y lógica de negocio
│   ├── SDV.Infrastructure/    # Implementaciones y acceso a datos
│   └── SDV.WorkerService/     # Servicio worker para ejecución
├── data/                      # Archivos CSV de origen
├── scripts/                   # Scripts SQL para base de datos
└── logs/                      # Logs de ejecución
```

### Capas del Proyecto

- **SDV.Core**: Entidades de dominio, interfaces y contratos
- **SDV.Application**: Lógica de negocio y orquestación de casos de uso
- **SDV.Infrastructure**: Implementación de extractores y repositorios
- **SDV.WorkerService**: Punto de entrada y ejecución del proceso ETL

## 🚀 Tecnologías

- **.NET 8.0** - Framework principal
- **Entity Framework Core 8.0** - ORM para acceso a datos
- **Pomelo.EntityFrameworkCore.MySql** - Provider MySQL/MariaDB
- **CsvHelper** - Lectura de archivos CSV
- **Serilog** - Sistema de logging estructurado
- **MySQL/MariaDB** - Base de datos staging

## 📊 Fuentes de Datos

El sistema extrae datos desde 3 fuentes diferentes:

1. **CSV**: Clientes (`data/customers.csv`)
2. **API Mock**: Productos (generados internamente)
3. **Database Mock**: Órdenes y detalles de órdenes (generados internamente)

## 🗄️ Staging Area

Tablas de staging en MySQL:

- `StagingCustomers` - Clientes extraídos desde CSV
- `StagingProducts` - Productos desde API
- `StagingOrders` - Órdenes desde base de datos
- `StagingOrderDetails` - Detalles de órdenes

## ⚙️ Configuración

### Prerrequisitos

- .NET 8.0 Runtime o SDK
- MySQL/MariaDB (puerto 3307)
- Usuario de base de datos configurado

### Configuración de Base de Datos

1. Ejecutar el script SQL para crear la base de datos:

```bash
mysql -h 127.0.0.1 -P 3307 -u root < "scripts\Script ventas.sql"
```

2. Configurar connection string en `src/SDV.WorkerService/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "StagingDb": "Server=127.0.0.1;Port=3307;Database=SalesDataWarehouse;User=su_usuario;Password=;"
  }
}
```

### Archivos CSV

Asegurarse de que los archivos CSV estén en la carpeta `data/`:

- `data/customers.csv`
- `data/products.csv`
- `data/orders.csv`

## 🏃‍♂️ Ejecución

### Compilar el proyecto

```bash
dotnet build
```

### Ejecutar el proceso ETL

```bash
dotnet run --project src\SDV.WorkerService\SDV.WorkerService.csproj
```

### Logs

Los logs se generan en:
- Consola (salida estándar)
- Archivo: `src/SDV.WorkerService/logs/etl-YYYYMMDD.txt`

## 📝 Proceso ETL Actual

### ✅ Extract (E) - Implementado

1. **Extracción de Clientes** desde CSV
   - Lee `data/customers.csv`
   - Mapea a `StagingCustomer`
   - Inserta en `StagingCustomers`

2. **Extracción de Productos** desde API Mock
   - Genera 50 productos de prueba
   - Mapea a `StagingProduct`
   - Inserta en `StagingProducts`

3. **Extracción de Órdenes** desde Database Mock
   - Genera 100 órdenes con detalles
   - Mapea a `StagingOrder` y `StagingOrderDetail`
   - Inserta en `StagingOrders` y `StagingOrderDetails`

### ⏳ Transform (T) - Pendiente

Transformaciones a implementar:
- Validación de datos
- Limpieza y normalización
- Cálculos y agregaciones
- Manejo de duplicados

### ⏳ Load (L) - Pendiente

Carga a implementar:
- Diseño del modelo dimensional (Fact/Dimension tables)
- Carga desde staging a Data Warehouse
- Actualización de dimensiones SCD (Slowly Changing Dimensions)


## 📚 Patrones de Diseño Utilizados

- **Repository Pattern**: Abstracción del acceso a datos
- **Strategy Pattern**: Diferentes estrategias de extracción (CSV, API, Database)
- **Dependency Injection**: Inyección de dependencias nativa de .NET
- **Use Case Pattern**: Encapsulación de lógica de negocio

## 🧪 Testing

_(Pendiente de implementación)_

## 📄 Licencia

Proyecto académico - ITLA Electiva 1

## 👥 Autor

Neftali Liriano
