# API Test Inputs

## Services

| Service | Port | Swagger UI |
|---------|------|------------|
| Catalog | 5050 | http://localhost:5050/swagger |
| Basket | 6060 | http://localhost:6060/swagger |
| Ordering | 5070 | http://localhost:5070/swagger |

---

## How to Start Services

Open 3 separate terminals in VS Code and run:

```powershell
# Terminal 1 - Catalog
cd C:\Projects\EShopMicroservices
dotnet run --project src\Services\Catalog\EShop.Catalog.API\EShop.Catalog.API.csproj --urls "http://localhost:5050"

# Terminal 2 - Basket
cd C:\Projects\EShopMicroservices
dotnet run --project src\Services\Basket\EShop.Basket.API\EShop.Basket.API.csproj --urls "http://localhost:6060"

# Terminal 3 - Ordering
cd C:\Projects\EShopMicroservices
dotnet run --project src\Services\Ordering\EShop.Ordering.API\EShop.Ordering.API.csproj --urls "http://localhost:5070"
```

---

## CATALOG API (http://localhost:5050)

### GET All Products

**URL:** `GET http://localhost:5050/api/v1/catalog/products`  
**Body:** None

---

### GET Product by ID

**URL:** `GET http://localhost:5050/api/v1/catalog/products/{id}`  
**Example:** `GET http://localhost:5050/api/v1/catalog/products/69f4c1b5875a5814b20aded1`  
**Body:** None

---

### GET Products by Category

**URL:** `GET http://localhost:5050/api/v1/catalog/products/category/{category}`  
**Example:** `GET http://localhost:5050/api/v1/catalog/products/category/Smartphones`  
**Body:** None

---

### POST Create Product

**URL:** `POST http://localhost:5050/api/v1/catalog/products`  
**Body:**
```json
{
  "name": "AirPods Pro 2",
  "category": ["Audio", "Electronics"],
  "description": "Active noise cancellation earbuds",
  "imageFile": "product-7.png",
  "price": 249.99
}
```

---

### PUT Update Product

**URL:** `PUT http://localhost:5050/api/v1/catalog/products`  
**Body:** *(replace id with actual product id from GET)*
```json
{
  "id": "PASTE-PRODUCT-ID-HERE",
  "name": "iPhone 15 Pro Max",
  "category": ["Smartphones", "Electronics"],
  "description": "Updated - Apple's largest flagship smartphone",
  "imageFile": "product-1.png",
  "price": 1199.99
}
```

---

### DELETE Product

**URL:** `DELETE http://localhost:5050/api/v1/catalog/products/{id}`  
**Example:** `DELETE http://localhost:5050/api/v1/catalog/products/69f4c1b5875a5814b20aded1`  
**Body:** None

---

## BASKET API (http://localhost:6060)

### POST Store/Update Cart

**URL:** `POST http://localhost:6060/api/v1/basket`  
**Body:**
```json
{
  "userName": "john_doe",
  "items": [
    {
      "productId": "69f4c1b5875a5814b20aded1",
      "productName": "iPhone 15 Pro",
      "quantity": 1,
      "price": 999.99,
      "color": "Black"
    },
    {
      "productId": "69f4c1b5875a5814b20aded5",
      "productName": "Logitech MX Master 3S",
      "quantity": 2,
      "price": 99.99,
      "color": "Grey"
    }
  ]
}
```

---

### GET Cart

**URL:** `GET http://localhost:6060/api/v1/basket/{userName}`  
**Example:** `GET http://localhost:6060/api/v1/basket/john_doe`  
**Body:** None

---

### DELETE Cart

**URL:** `DELETE http://localhost:6060/api/v1/basket/{userName}`  
**Example:** `DELETE http://localhost:6060/api/v1/basket/john_doe`  
**Body:** None

---

### POST Checkout Basket

**URL:** `POST http://localhost:6060/api/v1/basket/checkout`  
**Body:**
```json
{
  "userName": "john_doe",
  "customerId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "firstName": "John",
  "lastName": "Doe",
  "emailAddress": "john@example.com",
  "addressLine": "123 Main St",
  "country": "USA",
  "state": "NY",
  "zipCode": "10001",
  "cardName": "John Doe",
  "cardNumber": "4111111111111111",
  "expiration": "12/25",
  "cvv": "123",
  "paymentMethod": 1
}
```

---

## ORDERING API (http://localhost:5070)

### GET All Orders

**URL:** `GET http://localhost:5070/api/v1/ordering/orders`  
**Body:** None

---

### GET Orders by Customer

**URL:** `GET http://localhost:5070/api/v1/ordering/orders/customer/{customerId}`  
**Example:** `GET http://localhost:5070/api/v1/ordering/orders/customer/d290f1ee-6c54-4b01-90e6-d701748f0851`  
**Body:** None

---

### POST Create Order

**URL:** `POST http://localhost:5070/api/v1/ordering/orders`  
**Body:**
```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "orderName": "ORD-2024-001",
  "customerId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "shippingAddress": {
    "firstName": "John",
    "lastName": "Doe",
    "emailAddress": "john@example.com",
    "addressLine": "123 Main St",
    "country": "USA",
    "state": "NY",
    "zipCode": "10001"
  },
  "payment": {
    "cardName": "John Doe",
    "cardNumber": "4111111111111111",
    "expiration": "12/25",
    "cvv": "123",
    "paymentMethod": 1
  },
  "status": 0,
  "orderItems": [
    {
      "productId": "a290f1ee-6c54-4b01-90e6-d701748f0851",
      "productName": "iPhone 15 Pro",
      "quantity": 1,
      "price": 999.99
    },
    {
      "productId": "b290f1ee-6c54-4b01-90e6-d701748f0851",
      "productName": "Logitech MX Master 3S",
      "quantity": 2,
      "price": 99.99
    }
  ],
  "totalPrice": 1199.97
}
```

---

### PUT Update Order

**URL:** `PUT http://localhost:5070/api/v1/ordering/orders`  
**Body:** *(replace id with actual order id from GET/POST)*
```json
{
  "id": "PASTE-YOUR-ORDER-ID-HERE",
  "orderName": "ORD-2024-001-UPDATED",
  "customerId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "shippingAddress": {
    "firstName": "John",
    "lastName": "Doe",
    "emailAddress": "john@example.com",
    "addressLine": "456 Oak Avenue",
    "country": "USA",
    "state": "CA",
    "zipCode": "90001"
  },
  "payment": {
    "cardName": "John Doe",
    "cardNumber": "4111111111111111",
    "expiration": "12/26",
    "cvv": "456",
    "paymentMethod": 1
  },
  "status": 2,
  "orderItems": [
    {
      "productId": "a290f1ee-6c54-4b01-90e6-d701748f0851",
      "productName": "iPhone 15 Pro",
      "quantity": 1,
      "price": 999.99
    }
  ],
  "totalPrice": 999.99
}
```

---

### DELETE Order

**URL:** `DELETE http://localhost:5070/api/v1/ordering/orders/{orderId}`  
**Example:** `DELETE http://localhost:5070/api/v1/ordering/orders/PASTE-YOUR-ORDER-ID-HERE`  
**Body:** None

---

## Recommended Test Flow

1. **GET** `/catalog/products` → See all available products
2. **POST** `/basket` → Add items to cart
3. **GET** `/basket/john_doe` → View cart (items + total price)
4. **POST** `/basket/checkout` → Checkout (publishes event, clears cart)
5. **GET** `/basket/john_doe` → Confirm cart is empty
6. **GET** `/ordering/orders` → See the created order
