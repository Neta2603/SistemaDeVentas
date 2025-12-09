# Script para generar datos consistentes de order_details
# Asegura que los ProductIDs en order_details coincidan con los de products.csv

import csv
import random
from datetime import datetime

print("Regenerando order_details.csv con ProductIDs coherentes...")

# Leer orders
print("Leyendo orders.csv...")
with open('data/orders.csv', 'r', encoding='utf-8-sig') as f:  # utf-8-sig para manejar BOM
    orders_reader = csv.DictReader(f)
    orders = list(orders_reader)
    
# Debug: mostrar las primeras claves
if orders:
    print(f"  Claves encontradas: {list(orders[0].keys())}")

print(f"  Total orders: {len(orders)}")

# ProductIDs válidos (1-50 según products.csv)
valid_product_ids = list(range(1, 51))

# Generar order_details coherentes
print("Generando order_details.csv...")
order_details = []

for order in orders:
    order_id = order['OrderID']
    # Cada orden tiene entre 1 y 5 productos
    num_products = random.randint(1, 5)
    
    # Seleccionar productos aleatorios SIN repetición para esta orden
    selected_products = random.sample(valid_product_ids, num_products)
    
    for product_id in selected_products:
        quantity = random.randint(1, 10)
        unit_price = round(random.uniform(10.0, 1000.0), 2)
        total_price = round(quantity * unit_price, 2)
        
        order_details.append({
            'OrderID': order_id,
            'ProductID': product_id,
            'Quantity': quantity,
            'TotalPrice': total_price
        })

print(f"  Total order_details generados: {len(order_details)}")

# Escribir el nuevo archivo
print("Escribiendo data/order_details_regenerated.csv...")
with open('data/order_details_regenerated.csv', 'w', newline='', encoding='utf-8') as f:
    writer = csv.DictWriter(f, fieldnames=['OrderID', 'ProductID', 'Quantity', 'TotalPrice'])
    writer.writeheader()
    writer.writerows(order_details)

print("")
print("COMPLETADO!")
print(f"  Archivo generado: data/order_details_regenerated.csv")
print(f"  Total registros: {len(order_details)}")
print("")
print("Para aplicar los cambios:")
print("  ren data\\order_details.csv order_details_old.csv")
print("  ren data\\order_details_regenerated.csv order_details.csv")
print("")
