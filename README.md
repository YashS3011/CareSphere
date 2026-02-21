# CareSphere Starter Application

A basic hospital management enterprise system built with Blazor Server (.NET 9) and PostgreSQL (via Supabase), using EF Core.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Supabase Project (or a local PostgreSQL database)

## Setup Instructions

1. **Update Connection String:**  
   Open `appsettings.json` and replace the `DefaultConnection` string with your Supabase or local PostgreSQL connection string.  
   *Note: If using Supabase, look for the Transaction pooler connection string or standard connection string.*
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=your-supabase-url...;Database=postgres;Username=postgres;Password=your-password;"
   }
   ```

2. **Generate and Apply Migrations:**  
   To set up the database schema, open your terminal at the root of the `CareSphere` folder and run:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

3. **Run the Application:**
   ```bash
   dotnet run
   ```
   Navigate to the URL provided in the console output to access the CareSphere dashboard.

## Features Currently Implemented

*   **Patient Management Module:**
    *   Dashboard showing total patients
    *   Search patients by Name or MRN
    *   Register new patient with auto-generated MRN (`MRN-YYYYMMDD-XXXX`)
    *   View patient details and demographics
    *   Edit patient records
    *   Delete patient (with confirmation)

## Tech Stack Overview

*   **Frontend / UI:** Blazor Server (Interactive Server Mode) with Bootstrap 5
*   **Backend:** ASP.NET Core within the same Blazor web application
*   **Database:** Supabase (PostgreSQL)
*   **ORM:** Entity Framework Core (via Npgsql)
*   **Architecture:** Clean minimal Service pattern (`IPatientService` -> `ApplicationDbContext`)
