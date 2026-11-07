# Persian Invoice Management System (Open Source)

A modern, lightweight, and full-stack invoice management web application designed specifically for the Iranian market. This project features a robust **ASP.NET Core Web API** backend and a **React (TypeScript)** frontend, fully containerized with **Docker** for seamless deployment.

## Project Purpose
Many small businesses in Iran rely on complex, over-engineered accounting software just to issue simple invoices. This open-source project provides a clean, fast, and elegant solution focusing purely on **creating, storing, and printing standard Iranian invoices (A4 format)** with full Right-To-Left (RTL) support.

## Key Features
*   **Full RTL Support:** Native Persian UI with beautiful typography using the Vazirmatn font.
*   **Automated Calculations:** Real-time calculation of row totals, discounts, taxes, and grand totals using both JavaScript (client-side) and C# (server-side).
*   **Print-Ready Layout:** Pixel-perfect A4 invoice template optimized for browser printing (`window.print()`).
*   **Modern Backend:** High-performance RESTful API built with C# and ASP.NET Core, using Entity Framework Core.
*   **Modern Frontend:** Single Page Application (SPA) built with React, TypeScript, and Vite.
*   **Interactive API Docs:** Integrated with **Scalar** for beautiful and modern API documentation (replacing traditional Swagger).
*   **Data Persistence:** Uses SQLite for a lightweight, file-based database with Docker volume mapping to prevent data loss.
*   **Fully Dockerized:** Multi-stage Dockerfiles and `docker-compose` for one-command deployment.

## Technology Stack
*   **Backend:** C# / .NET 8+ (ASP.NET Core Web API), Entity Framework Core, SQLite, Scalar (OpenAPI).
*   **Frontend:** React 18, TypeScript, Vite, Axios, React Router, Custom CSS.
*   **DevOps:** Docker, Docker Compose, Nginx (for serving frontend and reverse proxy).

## Quick Start (Production)

Deploying the entire stack is as simple as running one command:

1. Clone the repository:
   `git clone <your-repo-url>`
   `cd <your-repo-name>`
2. Build and run the containers:
   `docker-compose up -d --build`
3. Access the application:
   *   **Frontend UI:** `http://localhost` (or your server IP)
   *   **API Documentation (Scalar):** `http://localhost:5133/scalar` (Development only)

## Local Development Setup

If you want to run the project without Docker for development:

**Backend:**
`cd Api`
`dotnet restore`
`dotnet run`

**Frontend:**
`cd ui`
`npm install`
`npm run dev`
*(Note: The frontend Vite configuration includes a proxy to forward `/api` requests to the local .NET backend).*

## Contributing
This project is open-source. Feel free to fork it, submit pull requests, or open issues for bugs and feature requests.

## License
This project is licensed under the MIT License.