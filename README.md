# Textbooker

A web application built with ASP.NET Core for textbook management and booking. 

## 🚀 Technology Stack

- **Language**: C# (primary), HTML, JavaScript, SCSS
- **Framework**: ASP.NET Core with Razor Pages
- **Database**: Entity Framework Core with Migrations
- **Frontend**: Bootstrap (via LibMan)

## 📁 Project Structure

```
Booker/
├── Areas/              # Feature areas for organizing related functionality
├── Authorization/      # Authentication and authorization logic
├── Data/              # Database context and data models
├── Migrations/        # Entity Framework database migrations
├── Pages/             # Razor Pages for UI
├── Resources/         # Localization and resource files
├── Services/          # Business logic and services
├── TagHelpers/        # Custom ASP.NET Tag Helpers
├── Utilities/         # Helper functions and utilities
└── wwwroot/           # Static files (CSS, JS, images)
```

## 🛠️ Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version specified in project file)
- SQL Server or compatible database
- Visual Studio 2022 / VS Code / Rider (recommended)

### Installation

1. Clone the repository: 
   ```bash
   git clone https://github.com/TECH-MENTORING-EU/Textbooker.git
   cd Textbooker/Booker
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Update the database connection string in `appsettings.json`

4. Apply database migrations:
   ```bash
   dotnet ef database update
   ```

5. Run the application:
   ```bash
   dotnet run
   ```

6. Open your browser and navigate to `https://localhost:5001` (or the port specified in the console output)

## 🔧 Configuration

Configuration settings can be found in: 
- `appsettings.json` - Production settings
- `appsettings.Development.json` - Development environment settings

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is part of TECH-MENTORING-EU organization. 

## 📧 Contact

Organization: [TECH-MENTORING-EU](https://github.com/TECH-MENTORING-EU)

Project Link: [https://github.com/TECH-MENTORING-EU/Textbooker](https://github.com/TECH-MENTORING-EU/Textbooker)
