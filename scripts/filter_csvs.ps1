# ============================================================================
# Script optimizado para filtrar CSVs a primeras N órdenes
# Versión simplificada y confiable usando Import-Csv
# ============================================================================

param(
    [int]$NumOrders = 100,
    [string]$DataPath = "data",
    [switch]$AutoRename
)

$ErrorActionPreference = "Stop"
$StartTime = Get-Date

Write-Host ""
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "   FILTRADO DE CSVS - MODO OPTIMIZADO" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configuracion:" -ForegroundColor Yellow
Write-Host "  Numero de ordenes: $NumOrders"
Write-Host "  Ruta de datos: $DataPath"
Write-Host "  Auto-renombrar: $AutoRename"
Write-Host ""

# ==================== PASO 1: LEER ORDER_DETAILS ====================
Write-Host "[1/5] Leyendo order_details.csv..." -ForegroundColor Green
$orderDetailsPath = Join-Path $DataPath "order_details_backup.csv"

# Si no existe el backup, usar el original
if (-not (Test-Path $orderDetailsPath)) {
    $orderDetailsPath = Join-Path $DataPath "order_details.csv"
}

if (-not (Test-Path $orderDetailsPath)) {
    Write-Host "ERROR: No se encontro order_details.csv" -ForegroundColor Red
    exit 1
}

Write-Host "  Archivo: $orderDetailsPath"
$orderDetails = Import-Csv $orderDetailsPath
Write-Host "  OK Total lineas: $($orderDetails.Count)" -ForegroundColor Green

# ==================== PASO 2: OBTENER PRIMEROS N ORDER IDS ====================
Write-Host "[2/5] Obteniendo primeros $NumOrders OrderIDs unicos..." -ForegroundColor Green
$uniqueOrderIds = $orderDetails.OrderID | Select-Object -Unique | Select-Object -First $NumOrders
Write-Host "  OK OrderIDs unicos seleccionados: $($uniqueOrderIds.Count)" -ForegroundColor Green

# Convertir a HashSet para busquedas rapidas O(1)
$orderIdSet = New-Object System.Collections.Generic.HashSet[string]
foreach ($id in $uniqueOrderIds) {
    [void]$orderIdSet.Add($id)
}

# ==================== PASO 3: FILTRAR ORDER_DETAILS ====================
Write-Host "[3/5] Filtrando order_details..." -ForegroundColor Green
$filteredDetails = $orderDetails | Where-Object { $orderIdSet.Contains($_.OrderID) }
Write-Host "  OK Detalles filtrados: $($filteredDetails.Count)" -ForegroundColor Green

$filteredDetailsPath = Join-Path $DataPath "order_details_filtered.csv"
$filteredDetails | Export-Csv $filteredDetailsPath -NoTypeInformation -Encoding UTF8
Write-Host "  OK Archivo creado: $filteredDetailsPath" -ForegroundColor Green
Write-Host ""

# ==================== PASO 4: FILTRAR ORDERS ====================
$ordersPath = Join-Path $DataPath "orders_backup.csv"
if (-not (Test-Path $ordersPath)) {
    $ordersPath = Join-Path $DataPath "orders.csv"
}

if (Test-Path $ordersPath) {
    Write-Host "[4/5] Filtrando orders.csv..." -ForegroundColor Green
    Write-Host "  Archivo: $ordersPath"
    
    $orders = Import-Csv $ordersPath
    Write-Host "  Total ordenes: $($orders.Count)"
    
    $filteredOrders = $orders | Where-Object { $orderIdSet.Contains($_.OrderID) }
    Write-Host "  OK Orders filtradas: $($filteredOrders.Count)" -ForegroundColor Green
    
    $filteredOrdersPath = Join-Path $DataPath "orders_filtered.csv"
    $filteredOrders | Export-Csv $filteredOrdersPath -NoTypeInformation -Encoding UTF8
    Write-Host "  OK Archivo creado: $filteredOrdersPath" -ForegroundColor Green
} else {
    Write-Host "[4/5] orders.csv no encontrado (saltando)" -ForegroundColor Yellow
}
Write-Host ""

# ==================== PASO 5: AUTO-RENOMBRAR (OPCIONAL) ====================
if ($AutoRename) {
    Write-Host "[5/5] Respaldando y renombrando archivos..." -ForegroundColor Green
    
    $orderDetailsOriginal = Join-Path $DataPath "order_details.csv"
    $orderDetailsBackup = Join-Path $DataPath "order_details_backup.csv"
    
    # Backup order_details (solo si no existe ya)
    if ((Test-Path $orderDetailsOriginal) -and -not (Test-Path $orderDetailsBackup)) {
        Move-Item $orderDetailsOriginal $orderDetailsBackup -Force
        Write-Host "  OK Backup: order_details.csv -> order_details_backup.csv" -ForegroundColor Green
    }
    
    # Renombrar filtered
    if (Test-Path $filteredDetailsPath) {
        Move-Item $filteredDetailsPath $orderDetailsOriginal -Force
        Write-Host "  OK Renombrado: order_details_filtered.csv -> order_details.csv" -ForegroundColor Green
    }
    
    # Backup y renombrar orders
    $ordersOriginal = Join-Path $DataPath "orders.csv"
    $ordersBackup = Join-Path $DataPath "orders_backup.csv"
    $filteredOrdersPath = Join-Path $DataPath "orders_filtered.csv"
    
    if (Test-Path $filteredOrdersPath) {
        if ((Test-Path $ordersOriginal) -and -not (Test-Path $ordersBackup)) {
            Move-Item $ordersOriginal $ordersBackup -Force
            Write-Host "  OK Backup: orders.csv -> orders_backup.csv" -ForegroundColor Green
        }
        
        Move-Item $filteredOrdersPath $ordersOriginal -Force
        Write-Host "  OK Renombrado: orders_filtered.csv -> orders.csv" -ForegroundColor Green
    }
} else {
    Write-Host "[5/5] Auto-renombrar desactivado" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Para aplicar los cambios manualmente, ejecuta:" -ForegroundColor White
    Write-Host "  cd $DataPath" -ForegroundColor Gray
    Write-Host "  ren order_details.csv order_details_backup.csv" -ForegroundColor Gray
    Write-Host "  ren order_details_filtered.csv order_details.csv" -ForegroundColor Gray
    Write-Host "  ren orders.csv orders_backup.csv" -ForegroundColor Gray
    Write-Host "  ren orders_filtered.csv orders.csv" -ForegroundColor Gray
    Write-Host "  cd .." -ForegroundColor Gray
}
Write-Host ""

# ==================== RESUMEN ====================
$EndTime = Get-Date
$Duration = $EndTime - $StartTime

Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "RESUMEN FINAL" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "  OrderIDs unicos: $($uniqueOrderIds.Count)" -ForegroundColor White
Write-Host "  Detalles exportados: $($filteredDetails.Count)" -ForegroundColor White
Write-Host "  Tiempo de ejecucion: $($Duration.TotalSeconds.ToString("0.00"))s" -ForegroundColor White
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "OK Script completado exitosamente" -ForegroundColor Green
Write-Host ""

# ==================== EJEMPLOS DE USO ====================
<#
.SYNOPSIS
    Filtra archivos CSV de órdenes y detalles a un número específico de órdenes

.EXAMPLES
    # Filtrar a 100 órdenes (default)
    .\scripts\filter_csvs.ps1

    # Filtrar a 500 órdenes
    .\scripts\filter_csvs.ps1 -NumOrders 500

    # Filtrar y renombrar automáticamente
    .\scripts\filter_csvs.ps1 -AutoRename

    # Filtrar 200 órdenes con auto-renombrado
    .\scripts\filter_csvs.ps1 -NumOrders 200 -AutoRename
#>
