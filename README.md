# 📚 EBook - Digital Library & Payment Management System

An ASP.NET Core MVC web application designed for managing a digital ebook library, handling file uploads, tracking user payments, and enforcing role-based authorization.

---

## 🚀 Features

### 📖 Library & File Management
* **File Storage**: Uploads PDFs, images, and documents directly to `wwwroot/uploads` with unique GUID prefixes to prevent file name collisions.
* **In-Browser Reader**: Integrated Bootstrap 5 modal reader allowing users to view PDFs, plain text, and images directly in the browser without downloading.
* **Automatic File Cleanup**: Replacing or deleting a library record automatically deletes the associated physical file from the server disk.

### 💳 Payment Tracking
* **Role-Based Filtering**: Regular users only see their personal payment history, while Administrators have access to view and manage all system payments.
* **User Account Association**: Admins can assign payment records to specific registered users via email address lookups.

### 🔐 Authentication & Security
* **ASP.NET Core Identity**: User registration, login, and account confirmation workflows built on top of standard Identity claims.
* **Role-Based Access Control (RBAC)**: Protects administrative routes (`Create`, `Edit`, `Delete`) using `[Authorize(Roles = "Admin")]`.

### 📧 Email Integration
* **MailKit Integration**: Modern SMTP email sender implementation using `MailKit.Net.Smtp.SmtpClient`.
* **Gmail SMTP**: Configured for sending account confirmations and transactional emails via Google App Passwords over TLS (Port 587).

---

## 🛠️ Tech Stack

* **Framework**: ASP.NET Core MVC (.NET 8+)
* **Database / ORM**: Entity Framework Core with SQL Server / LocalDB
* **Authentication**: ASP.NET Core Identity Framework
* **Email Client**: MailKit & MimeKit
* **Frontend**: Bootstrap 5, Bootstrap Icons, HTML5, Modern JavaScript (ES6 Async/Fetch)

---

## 📂 Project Structure

```text
EBook/
├── Controllers/
│   ├── LibrariesController.cs   # Manages library entity, upload logic, and file preview stream
│   └── PaymentsController.cs    # Manages payment creation and role-filtered index views
├── Data/
│   └── ApplicationDbContext.cs  # EF Core Database Context
├── Models/
│   ├── Library.cs               # Entity for library items and storage relative paths
│   └── Payment.cs               # Entity for user payments linked to IdentityUser
├── Services/
│   └── EmailService.cs          # Custom IEmailSender service using MailKit SMTP
├── Views/
│   ├── Libraries/               # Library views (Index with Reader Modal, Form Views)
│   ├── Payments/                # Payment views (Role-adjusted Index, Create, Edit)
│   └── Shared/                  # Common layout and navigation partials
├── wwwroot/
│   └── uploads/                 # Target folder for physically stored files
├── appsettings.json             # Configuration (Database string & SMTP settings)
└── Program.cs                   # Dependency injection and HTTP pipeline setup