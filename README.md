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
Update-Database
```

## 📁 Folder Structure
```
QuickCart/
│
├── Controllers/            # MVC Controllers
├── Models/                 # Domain models
├── Views/                  # Razor views
├── Data/                  
│   └── QuickCartDbContext.cs
├── Services/               # Interfaces & business logic
├── wwwroot/                # Static files
├── appsettings.json        # Configuration
└── Program.cs / Startup.cs # App entry
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
