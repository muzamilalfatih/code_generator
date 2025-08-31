# Code Generatorm

A C# tool that automates API scaffolding based on an existing SQL Server database. It reads your database schema and generates a fully structured 3-layer application with API, Business, and DataAccess layers, including classes for each table.

## Architecture Overview

The application follows a Three-Tier Architecture, comprising:

1. Presentation Layer – Exposes RESTful endpoints for interaction   
2. Business Logic Layer – Handles the core functionality and rules  
3. Data Access Layer – Manages database interactions                                

## Technologies Used

- Language: C#  
- Framework: .NET Core , ADO.net, ASP.net
- Architecture: Three-Tier  
- Database: (SQL Server)

## Getting Started

### Prerequisites

- Visual Studio 2022 or later  
- .NET Framework (4.8)  
- SQL Server installed and running
- Access to SQL Server Management Studio (SSMS) or any SQL query tool

### Installation

1. Clone the repository  
   https://github.com/muzamilalfatih/code_generator.git

3. Open the solution  
   Open the .sln file in Visual Studio.

4. Restore NuGet packages  
   Go to Tools > NuGet Package Manager > Manage NuGet Packages for Solution and restore missing packages.
5. Edit the connection string 
6. Build and run  
   Use Build > Build Solution and Debug > Start Debugging to launch the application.
7. The system will:
  - Connect to your SQL Server.
  - List all available databases.
  - Let you select a database.
  - Read the schema and generate the 3-layer project automatically

## Features

- Lists all databases in a connected SQL Server instance.
- Lets the user select a database.
- Reads the selected database schema (tables, columns, types, relationships). 
- Automatically generates a 3-layer architecture:
  - API Layer – controllers and endpoints
  - Business Layer – business logic classes
  - DataAccess Layer – repository classes for database operations
- Generates C# classes for each table with properties mapped to columns.

## Contributing

Contributions are welcome!

1. Fork the repo  
2. Create a branch: git checkout -b feature/YourFeature  
3. Commit your changes: git commit -m "Add feature"  
4. Push the branch: git push origin feature/YourFeature  
5. Open a Pull Request

## Contact

If you have any questions or feedback, feel free to reach out:

- Email: muzamilalfatih123@gmail.com 
- GitHub: https://github.com/muzamilalfatih
