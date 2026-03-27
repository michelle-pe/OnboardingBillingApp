# BillingApp

A Billing Management System built with ASP.NET MVC 5 (.NET Framework 4.8), Knockout.js, Bootstrap 5, and Microsoft SQL Server.

## Overview

BillingApp allows a business to manage the full billing lifecycle:

- **Customers** — maintain a customer directory with search and soft delete
- **Products** — manage a product/service catalogue with pricing
- **Invoices** — create invoices with dynamic line items, real-time totals, and status tracking (Pending / Paid / Overdue / Cancelled)
- **Payments** — record payments against invoices with automatic status updates
- **Dashboard** — view revenue totals, outstanding balances, and recent activity at a glance

There is no authentication — this is designed as a single-user internal tool.

---

## Tech Stack

| Layer       | Technology                          |
|-------------|-------------------------------------|
| Frontend    | Knockout.js 3.5, Bootstrap 5.3      |
| Backend     | ASP.NET MVC 5 (.NET Framework 4.8)  |
| Database    | Microsoft SQL Server 2019           |
| Data access | ADO.NET (no Entity Framework)       |
| Hosting     | IIS (Internet Information Services) |

---

## Architecture

The solution follows a three-layer architecture:

```
BillingApp.Web          → Presentation layer (Controllers + Razor Views)
BillingApp.Business     → Business logic layer (Services + Validation)
BillingApp.Data         → Data access layer (Repositories + Models)
```

- **Web** references **Business** only
- **Business** references **Data** only
- **Web** never references **Data** directly
- All SQL queries are stored in XML files under `BillingApp.Web/Queries/` — no hardcoded SQL in C#
- ADO.NET is used for all database operations via the Repository pattern
- All navigation URLs use `@Url.Action()` so they resolve correctly whether the app is hosted at the root or in a subfolder (e.g. `/BillingApp/`)

---

## Prerequisites

Before setting up the application, make sure the following are installed:

- Visual Studio 2019 or later
- .NET Framework 4.8
- Microsoft SQL Server 2019 (or later)
- SQL Server Management Studio (SSMS)
- IIS (for hosting — see deployment section below)

---

## Database Setup

### Step 1 — Create the data folder

Open **Command Prompt as Administrator** and run:

```
mkdir C:\SQL_DATA
```

> This is where the SQL Server database files (.mdf and .ldf) will be stored.
> Avoid storing database files on external or removable drives — if the drive
> becomes unavailable, SQL Server will throw error 21 (device not ready) and
> the database will be inaccessible until the drive is reconnected.

### Step 2 — Run the database script

1. Open **SQL Server Management Studio (SSMS)**
2. Connect to your SQL Server instance
3. Click **New Query**
4. Open the file `Database/init.sql` from the solution root
5. Paste the contents into the query window
6. Press **F5** to execute

The script will:
- Create the `BillingAppDB` database on `C:\SQL_DATA\`
- Create all 5 tables with correct constraints and relationships
- Insert sample/seed data for testing

The script is **idempotent** — it is safe to run multiple times without errors or duplicate data.

### Step 3 — Verify

Run the following in SSMS to confirm everything was created correctly:

```sql
USE BillingAppDB;
SELECT 'Customers'    AS TableName, COUNT(*) AS Rows FROM HS_BILL_CUSTOMERS
UNION ALL
SELECT 'Products',                  COUNT(*)          FROM HS_BILL_PRODUCTS
UNION ALL
SELECT 'Invoices',                  COUNT(*)          FROM HS_BILL_INVOICES
UNION ALL
SELECT 'Invoice Items',             COUNT(*)          FROM HS_BILL_INVOICE_ITEMS
UNION ALL
SELECT 'Payments',                  COUNT(*)          FROM HS_BILL_PAYMENTS;
```

Expected output:

```
Customers      4
Products       5
Invoices       2
Invoice Items  4
Payments       1
```

---

## Running Locally (Visual Studio)

### Step 1 — Configure the connection string

Open `BillingApp.Web/Web.config` and update the connection string to match your SQL Server instance:

```xml
<connectionStrings>
  <add name="BillingAppDB"
       connectionString="Server=localhost;Database=BillingAppDB;Integrated Security=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

Common server name values:

| Setup               | Server value             |
|---------------------|--------------------------|
| Default SQL Server  | `localhost`              |
| SQL Server Express  | `.\SQLEXPRESS`           |
| LocalDB             | `(localdb)\MSSQLLocalDB` |
| Named instance      | `.\SQLSERVER2019`        |

> Use the exact same server name you use when connecting in SSMS.

### Step 2 — Set startup project

In Visual Studio, right-click **BillingApp.Web** → **Set as Startup Project**

### Step 3 — Run

Press **F5** (with debugger) or **Ctrl+F5** (without debugger).

The browser will open at `http://localhost:{port}/` and land on the Dashboard.

---

## Deploying on IIS

### Step 1 — Enable IIS on Windows

1. Open **Control Panel** → **Programs** → **Turn Windows features on or off**
2. Tick **Internet Information Services**
3. Expand it and also tick:
   - **Web Management Tools** → IIS Management Console
   - **World Wide Web Services** → Application Development Features → **ASP.NET 4.8**
4. Click **OK** and wait for installation
5. Restart your computer if prompted

Verify IIS is running by opening a browser and going to `http://localhost` — you should see the IIS welcome page.

### Step 2 — Enable ASP.NET 4.8 in IIS

Open **Command Prompt as Administrator** and run:

```
dism /online /enable-feature /featurename:IIS-ASPNET45 /all
```

Then register ASP.NET with IIS:

```
%windir%\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe -i
```

> Important: the correct folder name is `v4.0.30319` — not `v4.0.30303`.
> If the path is not found, run `dir %windir%\Microsoft.NET\Framework64\`
> to find the correct folder name on your machine.

To verify ASP.NET is already registered (and skip this step if so):

```
%windir%\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe -lv
```

### Step 3 — Create the publish folder with correct permissions

Before publishing from Visual Studio, create the destination folder and grant
the necessary permissions. Open **Command Prompt as Administrator** and run
each command separately:

```
mkdir C:\inetpub\wwwroot\BillingApp
```

```
icacls "C:\inetpub\wwwroot\BillingApp" /grant "%USERNAME%":(OI)(CI)F /T
```

```
icacls "C:\inetpub\wwwroot\BillingApp" /grant "IIS_IUSRS":(OI)(CI)RX /T
```

```
icacls "C:\inetpub\wwwroot\BillingApp" /grant "IUSR":(OI)(CI)RX /T
```

> Skipping this step causes "Access Denied" errors during publish even when
> the solution builds successfully.

### Step 4 — Publish from Visual Studio

1. Right-click **BillingApp.Web** in Solution Explorer
2. Click **Publish**
3. Click **New Profile**
4. Select **Folder** as the publish target
5. Set the folder path to:
   ```
   C:\inetpub\wwwroot\BillingApp
   ```
6. Click **Finish** then **Publish**

Wait for the Output window to show:
```
Publish succeeded.
```

> If publish fails with "Build failed", run **Build → Clean Solution** then
> **Build → Rebuild Solution** and fix any errors before publishing again.

### Step 5 — Verify Query XML files were published

Check that this folder exists and contains all 5 files:

```
C:\inetpub\wwwroot\BillingApp\Queries\
    CustomerQueries.xml
    ProductQueries.xml
    InvoiceQueries.xml
    InvoiceItemQueries.xml
    PaymentQueries.xml
```

If the folder is missing, copy the files manually from `BillingApp.Web/Queries/`
in the solution.

To prevent this on future publishes, make sure each XML file in
`BillingApp.Web/Queries/` has these properties set in Visual Studio
(select the file and press F4):

```
Build Action             = Content
Copy to Output Directory = Copy if newer
```

### Step 6 — Create the IIS Application

1. Open **IIS Manager** (search for it in the Start menu)
2. In the left panel, expand your server name
3. Expand **Sites** → click on **Default Web Site**
4. In the right panel click **View Applications**
5. Click **Add Application** in the right panel

Fill in:
```
Alias         : BillingApp
Physical Path : C:\inetpub\wwwroot\BillingApp
```

6. Click **OK**

### Step 7 — Set Application Pool to .NET 4.0

1. In IIS Manager, click **Application Pools** in the left panel
2. Find **DefaultAppPool** (or the pool assigned to BillingApp)
3. Right-click it → **Basic Settings**
4. Set:
   ```
   .NET CLR Version : v4.0
   Managed Pipeline : Integrated
   ```
5. Click **OK**

### Step 8 — Grant database access to the IIS App Pool

If using Windows Authentication (Integrated Security) in the connection string,
the IIS App Pool identity needs permission to access the database.

**Option A — SQL Server Authentication (recommended for IIS):**

Update `Web.config` with a SQL login:

```xml
<connectionStrings>
  <add name="BillingAppDB"
       connectionString="Server=localhost;Database=BillingAppDB;
                         User Id=your_sql_user;Password=your_password;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

**Option B — Keep Windows Authentication:**

Run the following in SSMS to grant the App Pool identity access:

```sql
USE BillingAppDB;
CREATE LOGIN [IIS APPPOOL\DefaultAppPool] FROM WINDOWS;
CREATE USER  [IIS APPPOOL\DefaultAppPool] FOR LOGIN [IIS APPPOOL\DefaultAppPool];
EXEC sp_addrolemember 'db_datareader', 'IIS APPPOOL\DefaultAppPool';
EXEC sp_addrolemember 'db_datawriter', 'IIS APPPOOL\DefaultAppPool';
```

### Step 9 — Browse the application

Open your browser and go to:

```
http://localhost/BillingApp
```

You should land on the BillingApp Dashboard.

---

## Important Notes

### URL generation
All internal navigation links use `@Url.Action("Action", "Controller")` in
Razor views rather than hardcoded paths like `/Invoice/Detail/1`. This ensures
links resolve correctly whether the app runs locally (`http://localhost:port/`)
or is hosted in IIS under a subfolder (`http://localhost/BillingApp/`).

### Database file location
Store SQL Server database files on a reliable local drive (`C:\`). Storing
them on external, removable, or network drives can cause SQL Server error 21
(device not ready) if the drive becomes unavailable, making the database
completely inaccessible.

### Date handling
All date values from the database are serialized as `/Date(milliseconds)/` by
the ASP.NET JSON serializer. All JavaScript date functions use local date
methods (`getFullYear()`, `getMonth()`, `getDate()`) instead of
`toISOString()` which returns UTC and can shift dates back by one day in
timezones ahead of UTC (e.g. GMT+5:30).

---

## Project Structure

```
BillingApp/
├── README.md
├── Database/
│   └── init.sql                          ← Database creation + seed data
│
├── BillingApp.Data/
│   ├── Helpers/
│   │   └── QueryLoader.cs                ← Reads SQL from XML files at runtime
│   ├── Models/
│   │   ├── Customer.cs
│   │   ├── Product.cs
│   │   ├── Invoice.cs
│   │   ├── InvoiceItem.cs
│   │   └── Payment.cs
│   ├── Repositories/
│   │   ├── CustomerRepository.cs
│   │   ├── ProductRepository.cs
│   │   ├── InvoiceRepository.cs
│   │   ├── InvoiceItemRepository.cs
│   │   └── PaymentRepository.cs
│   └── Queries/
│       ├── CustomerQueries.xml           ← Source XML (keep in sync with Web/Queries)
│       ├── ProductQueries.xml
│       ├── InvoiceQueries.xml
│       ├── InvoiceItemQueries.xml
│       └── PaymentQueries.xml
│
├── BillingApp.Business/
│   └── Services/
│       ├── CustomerService.cs
│       ├── ProductService.cs
│       ├── InvoiceService.cs
│       ├── PaymentService.cs
│       └── DashboardService.cs
│
└── BillingApp.Web/
    ├── Controllers/
    │   ├── BaseController.cs
    │   ├── DashboardController.cs
    │   ├── CustomerController.cs
    │   ├── ProductController.cs
    │   ├── InvoiceController.cs
    │   └── PaymentController.cs
    ├── Queries/                          ← Runtime XML files (deployed with the app)
    │   ├── CustomerQueries.xml
    │   ├── ProductQueries.xml
    │   ├── InvoiceQueries.xml
    │   ├── InvoiceItemQueries.xml
    │   └── PaymentQueries.xml
    ├── Scripts/
    │   └── customer.js                   ← Knockout viewmodels (JS kept out of views)
    ├── Views/
    │   ├── web.config                    ← Razor config (namespaces, ViewBag, Url etc.)
    │   ├── _ViewStart.cshtml
    │   ├── Shared/
    │   │   └── _Layout.cshtml
    │   ├── Dashboard/
    │   │   └── Index.cshtml
    │   ├── Customer/
    │   │   └── Index.cshtml
    │   ├── Product/
    │   │   └── Index.cshtml
    │   └── Invoice/
    │       ├── Index.cshtml
    │       ├── Create.cshtml
    │       ├── Edit.cshtml
    │       └── Detail.cshtml
    └── Web.config
```

---

## Troubleshooting

**Query file not found**
→ The `Queries/` folder is missing from the published output. Copy all 5 XML
files from `BillingApp.Web/Queries/` to `C:\inetpub\wwwroot\BillingApp\Queries\`.
Also confirm each XML file has `Build Action = Content` and
`Copy to Output Directory = Copy if newer` set in Visual Studio.

**SQL connection error on startup**
→ Verify the server name in `Web.config` matches your SQL Server instance name
shown in SSMS. Confirm the database exists by running
`SELECT name FROM sys.databases WHERE name = 'BillingAppDB'` in SSMS.

**Error 21 — device not ready**
→ The database .mdf file is on a drive that is disconnected or unavailable.
Detach the broken database (`sp_detach_db`) and recreate it on `C:\SQL_DATA\`
by running `init.sql`. Never store database files on removable drives.

**Page loads but tables are empty**
→ Visit `http://localhost/BillingApp/Customer/GetAll` in the browser.
If it returns `{"success":false,"message":"..."}` the error message will
identify the exact problem.

**404 error when clicking View or Edit on invoices**
→ Navigation URLs are hardcoded (e.g. `/Invoice/Detail/1`) instead of using
`@Url.Action()`. All links must use `@Url.Action("Detail","Invoice")` so they
include the `/BillingApp/` prefix when hosted on IIS.

**Publish fails with Access Denied**
→ Run the `icacls` permission commands in Step 3 of the IIS deployment section
as Administrator before attempting to publish.

**Publish fails with Build failed — Razor errors**
→ The `Views/web.config` file is missing or has an incorrect
`System.Web.Mvc` version number. Check the version matches what is listed
under `BillingApp.Web → References → System.Web.Mvc` in Solution Explorer.

**Date shows one day behind**
→ The `formatDate()` or `toInputDate()` JavaScript functions are using
`toISOString()` (UTC) instead of local date methods. Replace with
`getFullYear()`, `getMonth() + 1`, and `getDate()` to use the local timezone.

**Dashboard Recent Payments — Invoice # column blank**
→ The `GetPaymentsByInvoiceId` query is missing the JOIN to `HS_BILL_INVOICES`.
Update the query in both `BillingApp.Data/Queries/PaymentQueries.xml` and
`BillingApp.Web/Queries/PaymentQueries.xml` to include
`INNER JOIN HS_BILL_INVOICES i ON i.INVOICE_ID = p.INVOICE_ID` and
select `i.INVOICE_NUMBER`.
