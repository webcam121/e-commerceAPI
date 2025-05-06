# E-commerce API

A robust RESTful API for an e-commerce platform built with .NET Core and PostgreSQL. This API provides endpoints for managing products, categories, attributes, and more.

## Features

- Product management with support for images and attributes
- Category management with customizable attributes
- Attribute value management for product specifications
- Product filtering and search capabilities
- Recommended products feature
- Base64 image support for product images

## Prerequisites

- .NET 8.0 SDK or later
- PostgreSQL 12.0 or later
- Visual Studio 2022 or Visual Studio Code

## Getting Started

1. Clone the repository:
```bash
git clone <repository-url>
cd ecommerceAPI
```

2. Update the database connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ecommerce;Username=your_username;Password=your_password"
  }
}
```

3. Run the database migrations:
```bash
dotnet ef database update
```

4. Start the application:
```bash
dotnet run
```

The API will be available at `http://localhost:5024`.

## API Endpoints

### Products

- `GET /api/product/all` - Get all products
- `GET /api/product/{id}` - Get a specific product
- `GET /api/product/category/{categoryId}` - Get products by category
- `POST /api/product` - Create a new product (form-data)
- `PUT /api/product/{id}` - Update a product (form-data)
- `DELETE /api/product/{id}` - Delete a product
- `POST /api/product/filter` - Filter products by category and attributes

### Categories

- `GET /api/category` - Get all categories
- `GET /api/category/{id}` - Get a specific category
- `POST /api/category` - Create a new category
- `PUT /api/category/{id}` - Update a category
- `DELETE /api/category/{id}` - Delete a category

### Attributes

- `GET /api/attribute` - Get all attributes
- `GET /api/attribute/{id}` - Get a specific attribute
- `POST /api/attribute` - Create a new attribute
- `PUT /api/attribute/{id}` - Update an attribute
- `DELETE /api/attribute/{id}` - Delete an attribute

### Database Seeding

- `POST /api/database/seed` - Seed the database with initial data
- `POST /api/database/reseed` - Clear and reseed the database

## Data Models

### Product
```json
{
  "id": 1,
  "name": "Product Name",
  "description": "Product Description",
  "price": 99.99,
  "stockQuantity": 100,
  "categoryId": 1,
  "imageBase64": "base64_encoded_image",
  "imageContentType": "image/jpeg",
  "isRecommended": false,
  "createdAt": "2024-03-20T10:00:00Z",
  "updatedAt": "2024-03-20T10:00:00Z"
}
```

### Category
```json
{
  "id": 1,
  "name": "Category Name",
  "description": "Category Description"
}
```

### Attribute
```json
{
  "id": 1,
  "name": "Attribute Name",
  "description": "Attribute Description",
  "categoryId": 1
}
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## API Documentation

### Product Endpoints

#### Create Product (POST /api/product)
Creates a new product with form-data.

**Request:**
- Content-Type: `multipart/form-data`
- Parameters:
  - `name` (required): Product name
  - `description` (optional): Product description
  - `price` (required): Product price
  - `stockQuantity` (optional): Product stock quantity
  - `categoryId` (required): Category ID
  - `image` (optional): Product image file (max 10MB)
  - `isRecommended` (optional): Whether the product is recommended
  - `attributeValueIds` (optional): List of attribute value IDs

**Response:**
```json
{
  "id": 1,
  "name": "Product Name",
  "description": "Product Description",
  "price": 99.99,
  "stockQuantity": 100,
  "categoryId": 1,
  "imageBase64": "base64_encoded_image",
  "imageContentType": "image/jpeg",
  "isRecommended": false,
  "createdAt": "2024-03-20T10:00:00Z",
  "updatedAt": "2024-03-20T10:00:00Z"
}
```

#### Update Product (PUT /api/product/{id})
Updates an existing product with form-data.

**Request:**
- Content-Type: `multipart/form-data`
- Parameters:
  - `name` (required): Product name
  - `description` (optional): Product description
  - `price` (required): Product price
  - `stockQuantity` (optional): Product stock quantity
  - `categoryId` (required): Category ID
  - `image` (optional): Product image file (max 10MB)
  - `isRecommended` (optional): Whether the product is recommended
  - `attributeValueIds` (optional): List of attribute value IDs

**Response:**
- Status: 204 No Content 