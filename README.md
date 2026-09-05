# Textbooker

A web application built with ASP.NET Core for textbook management and booking. 

## 🚀 Technology Stack

- **Language**: C# (primary), HTML, JavaScript, SCSS
- **Framework**: ASP.NET Core with Razor Pages
- **Interactive Components**: Blazor (Interactive Islands pattern for enhanced interactivity)
- **Database**: Entity Framework Core with Migrations
- **Frontend**: 
  - **CSS Framework**: PICO CSS with SCSS preprocessor
  - **JavaScript**: HTMX for SPA-like functionality without heavy JavaScript frameworks
- **Authentication/Authorization**: ASP.NET Core Identity

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

## 🤖 Apply PR review comments with Copilot CLI

This repository includes the script `..\.agents\skills\textbooker-pr-review\scripts\apply_review_comments_agent_v2.py` to fetch unresolved review comments from the open pull request for your current branch and hand them off to GitHub Copilot CLI.

### Prerequisites
Requires (minimum supported versions):
- Python 3.13.7
- git 2.53.0.windows3
- GitHub Copilot CLI 1.0.70+ authenticated with `copilot login`
- GitHub CLI 2.96.0 authenticated with `gh auth login`

### How it works

The script:

1. Detects the currently checked out branch
2. Finds the open pull request for that branch
3. Fetches unresolved review threads that are not outdated
4. Writes a prompt file to the repository root
5. Starts Copilot CLI in autopilot mode to apply the requested fixes

### Run the script

From the repository root:

```bash
python .\.agents\skills\textbooker-pr-review\scripts\apply_review_comments_agent_v2.py
```

### Useful options

- Only generate the prompt file without launching Copilot:

  ```bash
  python .\.agents\skills\textbooker-pr-review\scripts\apply_review_comments_agent_v2.py --no-copilot
  ```

- Explicitly specify the repository if auto-detection is not enough:

  ```bash
  python .\.agents\skills\textbooker-pr-review\scripts\apply_review_comments_agent_v2.py --repo TECH-MENTORING-EU/Textbooker
  ```

- Change the generated prompt file name:

  ```bash
  python .\.agents\skills\textbooker-pr-review\scripts\apply_review_comments_agent_v2.py --prompt-file my-review-prompt.txt
  ```

### Notes

- Run the script from a branch that already has an open pull request.
- The script does not create a commit.
- The script leaves changes uncommitted in your working tree.
- If there are no unresolved current review threads, the script exits without making changes.

## 📧 Contact

Organization: [TECH-MENTORING-EU](https://github.com/TECH-MENTORING-EU)

Project Link: [https://github.com/TECH-MENTORING-EU/Textbooker](https://github.com/TECH-MENTORING-EU/Textbooker)
