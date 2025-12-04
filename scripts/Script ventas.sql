DROP DATABASE IF EXISTS SalesDataWarehouse;
CREATE DATABASE SalesDataWarehouse
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE SalesDataWarehouse;


-- Staging: Clientes
CREATE TABLE StagingCustomers (
    CustomerID INT NOT NULL,
    FirstName VARCHAR(100),
    LastName VARCHAR(100),
    Email VARCHAR(150),
    Phone VARCHAR(50),
    City VARCHAR(100),
    Country VARCHAR(100),
    LoadDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_customerid (CustomerID),
    INDEX idx_loaddate (LoadDate)
) ENGINE=InnoDB COMMENT='Tabla staging para datos de clientes desde CSV/API';

-- Staging: Productos
CREATE TABLE StagingProducts (
    ProductID INT NOT NULL,
    ProductName VARCHAR(200),
    Category VARCHAR(100),
    Price DECIMAL(10, 2),
    Stock INT,
    LoadDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_productid (ProductID),
    INDEX idx_loaddate (LoadDate)
) ENGINE=InnoDB COMMENT='Tabla staging para datos de productos desde CSV/API';

-- Staging: Órdenes
CREATE TABLE StagingOrders (
    OrderID INT NOT NULL,
    CustomerID INT,
    OrderDate DATE,
    Status VARCHAR(50),
    LoadDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_orderid (OrderID),
    INDEX idx_customerid (CustomerID),
    INDEX idx_loaddate (LoadDate)
) ENGINE=InnoDB COMMENT='Tabla staging para encabezados de órdenes';

-- Staging: Detalles de Órdenes
CREATE TABLE StagingOrderDetails (
    OrderID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT,
    TotalPrice DECIMAL(10, 2),
    LoadDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_orderid (OrderID),
    INDEX idx_productid (ProductID),
    INDEX idx_loaddate (LoadDate)
) ENGINE=InnoDB COMMENT='Tabla staging para líneas de detalle de órdenes';

CREATE TABLE DimCustomer (
    CustomerKey BIGINT AUTO_INCREMENT PRIMARY KEY,
    CustomerID INT NOT NULL COMMENT 'ID de negocio del cliente',
    FirstName VARCHAR(100),
    LastName VARCHAR(100),
    Email VARCHAR(150),
    Phone VARCHAR(50),
    City VARCHAR(100),
    Country VARCHAR(100),
    -- Campos SCD Tipo 2
    StartDate DATE NOT NULL DEFAULT (CURRENT_DATE) COMMENT 'Fecha inicio validez registro',
    EndDate DATE DEFAULT '9999-12-31' COMMENT 'Fecha fin validez registro',
    IsCurrent BOOLEAN DEFAULT TRUE COMMENT 'Indica si es la versión actual del registro',
    -- Índices
    INDEX idx_customerid (CustomerID),
    INDEX idx_iscurrent (IsCurrent),
    INDEX idx_dates (StartDate, EndDate)
) ENGINE=InnoDB COMMENT='Dimensión de clientes con historial de cambios (SCD Tipo 2)';

CREATE TABLE DimProduct (
    ProductKey BIGINT AUTO_INCREMENT PRIMARY KEY,
    ProductID INT NOT NULL COMMENT 'ID de negocio del producto',
    ProductName VARCHAR(200),
    Category VARCHAR(100),
    Price DECIMAL(10, 2) COMMENT 'Precio histórico del producto',
    -- Campos SCD Tipo 2
    StartDate DATE NOT NULL DEFAULT (CURRENT_DATE) COMMENT 'Fecha inicio validez del precio',
    EndDate DATE DEFAULT '9999-12-31' COMMENT 'Fecha fin validez del precio',
    IsCurrent BOOLEAN DEFAULT TRUE COMMENT 'Indica si es el precio actual',
    -- Índices
    INDEX idx_productid (ProductID),
    INDEX idx_category (Category),
    INDEX idx_iscurrent (IsCurrent),
    INDEX idx_dates (StartDate, EndDate)
) ENGINE=InnoDB COMMENT='Dimensión de productos con historial de precios (SCD Tipo 2)';


CREATE TABLE DimTime (
    TimeKey INT PRIMARY KEY COMMENT 'Clave formato YYYYMMDD (ej: 20250101)',
    FullDate DATE NOT NULL UNIQUE,
    Year INT NOT NULL,
    Quarter INT NOT NULL COMMENT 'Trimestre (1-4)',
    Month INT NOT NULL COMMENT 'Mes (1-12)',
    MonthName VARCHAR(20) NOT NULL,
    Day INT NOT NULL COMMENT 'Día del mes (1-31)',
    DayOfWeek INT NOT NULL COMMENT 'Día de la semana (1=Lunes, 7=Domingo)',
    DayName VARCHAR(20) NOT NULL,
    IsWeekend BOOLEAN NOT NULL DEFAULT FALSE,
    IsHoliday BOOLEAN DEFAULT FALSE COMMENT 'Flag para días festivos (opcional)',
    -- Índices
    INDEX idx_fulldate (FullDate),
    INDEX idx_year_month (Year, Month),
    INDEX idx_year_quarter (Year, Quarter)
) ENGINE=InnoDB COMMENT='Dimensión de tiempo precalculada para análisis temporal';

CREATE TABLE DimStatus (
    StatusKey INT AUTO_INCREMENT PRIMARY KEY,
    StatusName VARCHAR(50) NOT NULL UNIQUE,
    StatusDescription VARCHAR(200),
    INDEX idx_statusname (StatusName)
) ENGINE=InnoDB COMMENT='Dimensión de estados de órdenes';

-- Insertar valores predefinidos de estado
INSERT INTO DimStatus (StatusName, StatusDescription) VALUES
    ('Pending', 'Orden creada, pendiente de procesamiento'),
    ('Shipped', 'Orden enviada al cliente'),
    ('Delivered', 'Orden entregada exitosamente'),
    ('Cancelled', 'Orden cancelada');


CREATE TABLE FactSales (
    SalesKey BIGINT AUTO_INCREMENT PRIMARY KEY,
    -- Claves foráneas a dimensiones
    CustomerKey BIGINT NOT NULL,
    ProductKey BIGINT NOT NULL,
    TimeKey INT NOT NULL,
    StatusKey INT NOT NULL,
    -- Clave de negocio
    OrderID INT NOT NULL COMMENT 'ID de orden original del sistema transaccional',
    -- Métricas (Medidas)
    Quantity INT NOT NULL COMMENT 'Cantidad de unidades vendidas',
    UnitPrice DECIMAL(10, 2) NOT NULL COMMENT 'Precio unitario en el momento de la venta',
    TotalPrice DECIMAL(10, 2) NOT NULL COMMENT 'Precio total de la línea (Quantity * UnitPrice)',
    -- Auditoría
    LoadDate DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT 'Fecha de carga en el DW',
    -- Constraints de integridad referencial
    CONSTRAINT fk_sales_customer FOREIGN KEY (CustomerKey) 
        REFERENCES DimCustomer(CustomerKey)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_sales_product FOREIGN KEY (ProductKey) 
        REFERENCES DimProduct(ProductKey)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_sales_time FOREIGN KEY (TimeKey) 
        REFERENCES DimTime(TimeKey)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_sales_status FOREIGN KEY (StatusKey) 
        REFERENCES DimStatus(StatusKey)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    -- Índices para optimización de consultas
    INDEX idx_customerkey (CustomerKey),
    INDEX idx_productkey (ProductKey),
    INDEX idx_timekey (TimeKey),
    INDEX idx_statuskey (StatusKey),
    INDEX idx_orderid (OrderID),
    INDEX idx_loaddate (LoadDate),
    -- Índice compuesto para análisis multidimensional
    INDEX idx_time_customer (TimeKey, CustomerKey),
    INDEX idx_time_product (TimeKey, ProductKey)
) ENGINE=InnoDB COMMENT='Tabla de hechos de ventas (grain: línea de detalle de orden)';

DELIMITER $$

CREATE PROCEDURE PopulateDimTime(
    IN startYear INT,
    IN endYear INT
)
BEGIN
    DECLARE currentDate DATE;
    DECLARE endDate DATE;
    
    SET currentDate = CONCAT(startYear, '-01-01');
    SET endDate = CONCAT(endYear, '-12-31');
    
    WHILE currentDate <= endDate DO
        INSERT IGNORE INTO DimTime (
            TimeKey,
            FullDate,
            Year,
            Quarter,
            Month,
            MonthName,
            Day,
            DayOfWeek,
            DayName,
            IsWeekend
        ) VALUES (
            DATE_FORMAT(currentDate, '%Y%m%d'),
            currentDate,
            YEAR(currentDate),
            QUARTER(currentDate),
            MONTH(currentDate),
            DATE_FORMAT(currentDate, '%M'),
            DAY(currentDate),
            DAYOFWEEK(currentDate),
            DATE_FORMAT(currentDate, '%W'),
            IF(DAYOFWEEK(currentDate) IN (1, 7), TRUE, FALSE)
        );
        
        SET currentDate = DATE_ADD(currentDate, INTERVAL 1 DAY);
    END WHILE;
END$$

DELIMITER ;

CALL PopulateDimTime(2020, 2030);

CREATE VIEW vw_CurrentSales AS
SELECT 
    fs.SalesKey,
    fs.OrderID,
    -- Dimensión Cliente
    dc.CustomerID,
    CONCAT(dc.FirstName, ' ', dc.LastName) AS CustomerName,
    dc.Email,
    dc.City,
    dc.Country,
    -- Dimensión Producto
    dp.ProductID,
    dp.ProductName,
    dp.Category,
    -- Dimensión Tiempo
    dt.FullDate AS OrderDate,
    dt.Year,
    dt.Quarter,
    dt.Month,
    dt.MonthName,
    -- Dimensión Estado
    ds.StatusName,
    -- Métricas
    fs.Quantity,
    fs.UnitPrice,
    fs.TotalPrice
FROM FactSales fs
INNER JOIN DimCustomer dc ON fs.CustomerKey = dc.CustomerKey AND dc.IsCurrent = TRUE
INNER JOIN DimProduct dp ON fs.ProductKey = dp.ProductKey AND dp.IsCurrent = TRUE
INNER JOIN DimTime dt ON fs.TimeKey = dt.TimeKey
INNER JOIN DimStatus ds ON fs.StatusKey = ds.StatusKey;

-- Vista: Resumen de ventas por producto
CREATE VIEW vw_SalesByProduct AS
SELECT 
    dp.ProductID,
    dp.ProductName,
    dp.Category,
    COUNT(DISTINCT fs.OrderID) AS TotalOrders,
    SUM(fs.Quantity) AS TotalQuantitySold,
    SUM(fs.TotalPrice) AS TotalRevenue,
    AVG(fs.UnitPrice) AS AvgUnitPrice
FROM FactSales fs
INNER JOIN DimProduct dp ON fs.ProductKey = dp.ProductKey AND dp.IsCurrent = TRUE
GROUP BY dp.ProductID, dp.ProductName, dp.Category;

-- Vista: Resumen de ventas por cliente
CREATE VIEW vw_SalesByCustomer AS
SELECT 
    dc.CustomerID,
    CONCAT(dc.FirstName, ' ', dc.LastName) AS CustomerName,
    dc.City,
    dc.Country,
    COUNT(DISTINCT fs.OrderID) AS TotalOrders,
    SUM(fs.Quantity) AS TotalItemsPurchased,
    SUM(fs.TotalPrice) AS TotalSpent,
    AVG(fs.TotalPrice) AS AvgOrderValue
FROM FactSales fs
INNER JOIN DimCustomer dc ON fs.CustomerKey = dc.CustomerKey AND dc.IsCurrent = TRUE
GROUP BY dc.CustomerID, dc.FirstName, dc.LastName, dc.City, dc.Country;

-- Vista: Ventas por periodo temporal
CREATE VIEW vw_SalesByTime AS
SELECT 
    dt.Year,
    dt.Quarter,
    dt.Month,
    dt.MonthName,
    COUNT(DISTINCT fs.OrderID) AS TotalOrders,
    SUM(fs.Quantity) AS TotalQuantitySold,
    SUM(fs.TotalPrice) AS TotalRevenue,
    COUNT(DISTINCT fs.CustomerKey) AS UniqueCustomers
FROM FactSales fs
INNER JOIN DimTime dt ON fs.TimeKey = dt.TimeKey
GROUP BY dt.Year, dt.Quarter, dt.Month, dt.MonthName
ORDER BY dt.Year, dt.Month;


-- Resumen de la estructura del Data Warehouse
SELECT 
    'SalesDataWarehouse' AS DatabaseName,
    '4 Staging Tables' AS StagingLayer,
    '4 Dimension Tables' AS DimensionTables,
    '1 Fact Table' AS FactTables,
    'Star Schema' AS ModelType,
    'SCD Type 2' AS ChangeTracking,
    'MySQL 8.0+' AS DatabaseEngine;
