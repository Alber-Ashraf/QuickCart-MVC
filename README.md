# 🛒 QuickCart-MVC

**QuickCart-MVC** is a robust and scalable e-commerce web application built using the ASP.NET Core MVC framework. It offers a comprehensive set of features tailored for both customers and administrators, ensuring a seamless online shopping experience.

---

## 🚀 Features

- 🧑‍💼 **User Authentication** – Register, Login, Logout
- 📦 **Product Management** – Create, Read, Update, Delete
- 🛒 **Shopping Cart** – Add/remove products, quantity control
- 💳 **Order Processing** – Checkout and view order summary
- 🔒 **Role-Based Authorization** – Admin vs Customer
- 📊 **Admin Dashboard** – Manage users, orders, and inventory
- 🌐 **Responsive Design** – Mobile-first layout using Bootstrap
- 📁 **Clean Architecture** – Follows SOLID principles and separation of concerns
- 🧪 **EF Core Code-First** – With migrations and seed data
- ☁️ **Azure Blob Storage Integration** – For product image uploads
- ⚙️ **Flexible Configuration** – Via `appsettings.json`

---

## 🛠️ Tech Stack

| Technology             | Description                          |
|------------------------|--------------------------------------|
| ASP.NET Core MVC       | Backend framework                    |
| Entity Framework Core  | ORM for data access                  |
| SQL Server             | Relational database engine           |
| Bootstrap 5            | Frontend styling and responsiveness |
| AutoMapper             | Object mapping between models/DTOs  |
| LINQ                   | Data querying                        |
| Azure Blob Storage     | Cloud file storage for images        |
| Dependency Injection   | Built-in via ASP.NET Core            |

---

## ⚙️ Setup Instructions

### 1. Clone the Repository

```bash
git clone https://github.com/Alber-Ashraf/QuickCart-MVC.git
cd QuickCart-MVC
```
### 2. Configure the Database
Update your appsettings.json:
```
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=QuickCart;Trusted_Connection=True;TrustServerCertificate=True"
}
```
### 3. Apply Migrations and Seed Data
```
Update-Database
```
### 4. Run the Application
```
dotnet run
```

## 📁 Folder Structure
```
📦 QuickCart.sln
│
├── 📁 QuickCart                            # ASP.NET Core Web Project (MVC)
│   ├── 📁 Areas
│   │   ├── 📁 Admin                        # Admin area (controllers, views, etc.)
│   │   ├── 📁 Customer                     # Customer area
│   │   └── 📁 Identity
│   │       └── 📁 Pages                    # Identity UI pages (login, register...)
│   │
│   ├── 📁 Properties                       # Project-specific settings (launchSettings.json)
│   ├── 📁 ViewComponents                   # Custom View Components
│   ├── 📁 Views                            # Shared views (e.g., _Layout, Error)
│   ├── 📁 wwwroot                          # Static files (CSS, JS, images)
│   ├── 📄 Program.cs                       # Application startup (entry point)
│   └── 📄 appsettings.json                 # Application configuration settings
│
├── 📁 QuickCart.DataAccess                 # Data Access Layer (DAL)
│   ├── 📁 Data                             # ApplicationDbContext and configuration
│   ├── 📁 DbInitializer                    # Seed data & DB initialization
│   ├── 📁 Migrations                       # EF Core database migrations
│   └── 📁 Repository                       # Repository pattern implementation
│
├── 📁 QuickCart.Models                     # Entity and ViewModel definitions
│   ├── 📁 ViewModels                       # ViewModel classes
│   ├── 📄 ApplicationUser.cs               # Custom identity user class
│   ├── 📄 Category.cs                      # Category entity
│   ├── 📄 Company.cs                       # Company entity
│   ├── 📄 ErrorViewModel.cs                # Error ViewModel
│   ├── 📄 OrderDetail.cs                   # Order detail entity
│   ├── 📄 OrderHeader.cs                   # Order header entity
│   ├── 📄 Product.cs                       # Product entity
│   ├── 📄 ProductImage.cs                  # Product image entity
│   ├── 📄 ShoppingCart.cs                  # Shopping cart entity
│   └── 📄 QuickCart.Models.csproj          # Project file
│
└── 📁 QuickCart.Utility                    # Utility classes and constants (e.g., SD.cs, helpers)
```
## 🧪 Contributing
```
- Fork the repo
- Create your branch: git checkout -b feature/YourFeature
- Commit your changes: git commit -m "Add new feature"
- Push to branch: git push origin feature/YourFeature
- Open Pull Request
```


## 💡 Author
Developed with ❤️ by Alber Ashraf
