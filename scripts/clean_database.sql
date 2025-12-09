-- ============================================================================
-- Script de Limpieza Completa del Data Warehouse
-- Elimina TODOS los datos de staging, dimensiones y hechos
-- ============================================================================

USE SalesDataWarehouse;

SET FOREIGN_KEY_CHECKS = 0;

-- ==================== LIMPIAR TABLAS DE HECHOS ====================
TRUNCATE TABLE FactSales;

-- ==================== LIMPIAR DIMENSIONES ====================
TRUNCATE TABLE DimCustomer;
TRUNCATE TABLE DimProduct;
TRUNCATE TABLE DimTime;
TRUNCATE TABLE DimStatus;

-- ==================== LIMPIAR STAGING ====================
TRUNCATE TABLE StagingCustomers;
TRUNCATE TABLE StagingProducts;
TRUNCATE TABLE StagingOrders;
TRUNCATE TABLE StagingOrderDetails;

SET FOREIGN_KEY_CHECKS = 1;

SELECT 'LIMPIEZA COMPLETA FINALIZADA' AS Estado;
SELECT 'Todas las tablas han sido limpiadas exitosamente' AS Mensaje;
