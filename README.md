# 🛠️ Abc2 ServiceDesk Management System

<div align="left">
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" alt="C#" />
  <img src="https://img.shields.io/badge/.NET_8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/.NET_MAUI-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET MAUI" />
  <img src="https://img.shields.io/badge/REST_API-00547E?style=for-the-badge&logo=json&logoColor=white" alt="REST API" />
  <img src="https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white" alt="SQL Server" />
  <img src="https://img.shields.io/badge/Entity_Framework_Core-3C8A3E?style=for-the-badge&logo=nuget&logoColor=white" alt="EF Core" />
  <img src="https://img.shields.io/badge/Google_Gemini_AI-8E75B2?style=for-the-badge&logo=google&logoColor=white" alt="Gemini AI" />
</div>

<br>

**Abc2 ServiceDesk** is a modular and multilingual (English & Portuguese) workshop and service management system designed for automotive, IT, electronics, and general equipment repair businesses. This cross-platform solution aims to streamline business workflows, from equipment intake and work order management to quotations and invoicing.

*This system was developed during a software engineering internship at **ABC Braga 2 Solutions** (Braga, Portugal).*

## 🚀 Core Modules

* **Customer & Equipment Management:** Management of customer information, service history, and equipment specifications.
* **Work Order Lifecycle:** End-to-end management of service requests, work order statuses, and technician assignments.
* **Inventory & Purchasing:** Inventory tracking, supplier management, and purchasing operations.
* **Financial Operations:** Generation of quotations, commercial estimates, and invoices using FastReport.
* **Dynamic Reporting:** Business reports and administrative analytics to support operational decision-making.
* **Multilingual Support:** Localized user interface in English and Portuguese, with potential for future expansion to multi-company and multi-branch environments.

## 💡 Advanced Implementations & Integrations

Beyond standard service management functionality, the system incorporates modern technologies and integrations:

* **🤖 AI Diagnostics Assistant:** Integration with the **Google Gemini API** to assist technicians with troubleshooting and provide context-aware repair suggestions.
* **📱 Cross-Platform Ecosystem:** Built with **.NET MAUI**, targeting Desktop and Mobile environments through a centralized **REST API**.
* **✍️ Digital Signatures:** Digital signature capture for customer approvals and work order sign-offs.
* **📷 Hardware Scanning:** Support for **QR Codes and Barcodes** to facilitate inventory management and equipment identification.

## 🏗️ Architecture & Technical Stack

The software follows a **Layered Architecture** designed to support maintainability, separation of concerns, and future development:

1. **Presentation Layer:** Cross-platform Desktop and Mobile user interface built with **.NET MAUI**.
2. **Business Layer:** Business logic, validation, and AI integration services.
3. **Data Access Layer:** Data access managed through **Entity Framework Core**, interacting with **SQL Server Express**.
4. **Domain Layer:** Core business entities and interfaces.
5. **Infrastructure Layer:** External API integrations, including Gemini AI, file system operations, and digital signature functionality.

### Technologies Used

| Category | Technology |
| :--- | :--- |
| **Languages** | C# |
| **Frameworks** | .NET 8, .NET MAUI |
| **Backend & API** | REST API architecture |
| **Database & ORM** | SQL Server Express, Entity Framework Core |
| **Integrations** | Google Gemini API, QR/Barcode Processing, Digital Signatures |
| **Tools & Analytics** | Rider, Git, JSON Configuration, FastReport |

## ⚙️ Configuration & Security Note
> 🔒 **Security Notice:** For security reasons, sensitive API keys (e.g., `Google Gemini API Key`) and database connection strings have been removed from the public repository. To run this project locally, ensure you provide your own API keys in the respective configuration files.

## 👨‍💻 Developer

**Vladyslav Koval** — *Junior Software Developer*  
[LinkedIn Profile](https://linkedin.com/in/vladyslav-koval2007)
