# Technical Documentation & Architecture Guide

This document provides a deep dive into the architecture, database schema, and design decisions of the Persian Invoice Management System.

## Architecture Overview

The system follows a classic **Decoupled Client-Server Architecture**:
1.  **Client (Frontend):** React + TypeScript SPA. Communicates with the backend via REST API using Axios.
2.  **Server (Backend):** ASP.NET Core Web API. Handles business logic, data validation, and database operations.
3.  **Database:** SQLite. A lightweight, serverless database engine. It is stored in a Docker volume (`./data`) to ensure persistence across container restarts.

## Database Schema (Entity Framework Core)

The database consists of three main entities defined in C#:

### 1. `Party` (Seller / Buyer)
Stores information about individuals or companies.
*   `Id` (PK)
*   `Name`, `EconomicCode`, `RegistrationNumber`, `NationalId`
*   `Province`, `City`, `Address`, `PostalCode`, `Phone`, `Mobile`

### 2. `InvoiceItem`
Stores individual line items of an invoice.
*   `Id` (PK)
*   `Description`, `Quantity`, `UnitPrice`, `Discount`, `Tax`
*   `TotalPrice` **(Read-Only Computed Property)**: `(Quantity * UnitPrice) - Discount + Tax`

### 3. `Invoice`
The main invoice entity linking sellers, buyers, and items.
*   `Id` (PK)
*   `InvoiceNumber`, `Date` (DateTime), `PaymentType`, `Notes`
*   `SellerId` (FK to Party), `BuyerId` (FK to Party)
*   `Items` (List of `InvoiceItem`)
*   `TotalAmount` **(Read-Only Computed Property)**: `Items.Sum(i => i.TotalPrice)`

> **Important Note on Computed Properties:** 
> `TotalPrice` and `TotalAmount` are C# expression-bodied properties (read-only). They are calculated automatically by the server during serialization and are **not** mapped to database columns directly. This ensures data integrity.

## Backend (API) Details

*   **API Routing:** Controllers are mapped with `[Route("api/[controller]")]`. Hence, the main endpoint is `/api/Invoices`.
*   **CORS Policy:** Configured in `Program.cs` to allow requests from `localhost:3000` (React dev server) and `localhost:5173` (Vite default).
*   **HTTPS Redirection:** `app.UseHttpsRedirection()` is **commented out** to allow the Nginx reverse proxy (inside Docker) to communicate over standard HTTP without SSL certificate conflicts.
*   **API Documentation:** Uses **Scalar** (modern replacement for Swagger). Endpoint: `/scalar`.

## Frontend Details

*   **State Management:** Uses standard React `useState` and `useEffect` hooks. No external state manager (like Redux) is needed for this scale.
*   **Routing:** Managed by `react-router-dom` (Routes: `/`, `/invoices`, `/invoices/new`, `/invoices/:id`).
*   **Styling:** Pure CSS (`App.css`). The design is separated into two main concerns:
    1.  **UI Styling:** Modern, clean dashboard look.
    2.  **Print Styling:** `@media print` rules that hide navigation bars, buttons, and sidebars, and strictly enforce an `A4` layout (`210mm x 297mm`) for the `InvoiceTemplate` component.
*   **Data Fetching:** Centralized in `src/services/api.ts` using an Axios instance with a base URL of `/api`.

## Docker Deployment Details

The project uses a `docker-compose.yml` to orchestrate two services:

1.  **`ui` (Nginx Container):**
    *   Built using a multi-stage Dockerfile (Node -> Nginx).
    *   Serves the static React files.
    *   Contains a `nginx.conf` that acts as a reverse proxy, forwarding all `/api/*` requests to the backend container.
2.  **`api` (.NET Container):**
    *   Built using a multi-stage Dockerfile (SDK -> ASP.NET Runtime).
    *   Listens on port `8080`.
    *   Mounts a local directory (`./data`) to `/app/data` to persist the SQLite database file (`invoices.db`).

### Environment Variables
In production, the `ASPNETCORE_ENVIRONMENT` is set to `Production` to disable development-only features (like Scalar docs and detailed exception pages).

## Troubleshooting Common Issues

*   **502 Bad Gateway:** This happens if Nginx cannot reach the API. Ensure the `api` container is running and healthy (`docker ps`).
*   **400 Bad Request on POST:** Ensure the date format sent from React matches C# `DateTime` parsing rules. (Use ISO 8601 format: `YYYY-MM-DD`).
*   **Database is locked:** SQLite allows only one writer at a time. Ensure you are not running multiple API containers simultaneously trying to access the same file.
*   **CORS Errors:** If developing locally, ensure the ports in `builder.Services.AddCors` match your Vite/React dev server ports.