-- ============================================================
--  BillingApp - Database Initialization Script
--  Re-runnable (idempotent) — safe to execute multiple times
-- ============================================================

-- ------------------------------------------------------------
-- 1. DATABASE CREATION
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BillingAppDB')
BEGIN
    CREATE DATABASE BillingAppDB;
END
GO

USE BillingAppDB;
GO

-- ------------------------------------------------------------
-- 2. TABLE CREATION
-- ------------------------------------------------------------

-- HS_BILL_CUSTOMERS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'HS_BILL_CUSTOMERS' AND type = 'U')
BEGIN
    CREATE TABLE HS_BILL_CUSTOMERS (
        CUSTOMER_ID   INT            IDENTITY(1,1) PRIMARY KEY,
        FIRST_NAME    NVARCHAR(100)  NOT NULL,
        LAST_NAME     NVARCHAR(100)  NOT NULL,
        EMAIL         NVARCHAR(200)  NOT NULL,
        PHONE         NVARCHAR(20),
        ADDRESS       NVARCHAR(500),
        CREATED_DATE  DATETIME       DEFAULT GETDATE(),
        IS_ACTIVE     BIT            DEFAULT 1
    );

    -- Unique constraint on EMAIL
    ALTER TABLE HS_BILL_CUSTOMERS
        ADD CONSTRAINT UQ_HS_BILL_CUSTOMERS_EMAIL UNIQUE (EMAIL);

    PRINT 'Created table: HS_BILL_CUSTOMERS';
END
ELSE
BEGIN
    PRINT 'Table already exists: HS_BILL_CUSTOMERS';
END
GO

-- HS_BILL_PRODUCTS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'HS_BILL_PRODUCTS' AND type = 'U')
BEGIN
    CREATE TABLE HS_BILL_PRODUCTS (
        PRODUCT_ID    INT             IDENTITY(1,1) PRIMARY KEY,
        PRODUCT_NAME  NVARCHAR(200)   NOT NULL,
        DESCRIPTION   NVARCHAR(500),
        UNIT_PRICE    DECIMAL(18,2)   NOT NULL,
        IS_ACTIVE     BIT             DEFAULT 1
    );

    PRINT 'Created table: HS_BILL_PRODUCTS';
END
ELSE
BEGIN
    PRINT 'Table already exists: HS_BILL_PRODUCTS';
END
GO

-- HS_BILL_INVOICES
IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'HS_BILL_INVOICES' AND type = 'U')
BEGIN
    CREATE TABLE HS_BILL_INVOICES (
        INVOICE_ID      INT             IDENTITY(1,1) PRIMARY KEY,
        INVOICE_NUMBER  NVARCHAR(50)    NOT NULL,
        CUSTOMER_ID     INT             NOT NULL,
        INVOICE_DATE    DATETIME        NOT NULL,
        DUE_DATE        DATETIME        NOT NULL,
        NOTES           NVARCHAR(1000),
        STATUS          NVARCHAR(20)    NOT NULL DEFAULT 'Pending',
        CREATED_DATE    DATETIME        DEFAULT GETDATE(),

        CONSTRAINT UQ_HS_BILL_INVOICES_NUMBER
            UNIQUE (INVOICE_NUMBER),

        CONSTRAINT FK_HS_BILL_INVOICES_CUSTOMER
            FOREIGN KEY (CUSTOMER_ID)
            REFERENCES HS_BILL_CUSTOMERS(CUSTOMER_ID),

        CONSTRAINT CHK_HS_BILL_INVOICES_STATUS
            CHECK (STATUS IN ('Pending', 'Paid', 'Overdue', 'Cancelled'))
    );

    PRINT 'Created table: HS_BILL_INVOICES';
END
ELSE
BEGIN
    PRINT 'Table already exists: HS_BILL_INVOICES';
END
GO

-- HS_BILL_INVOICE_ITEMS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'HS_BILL_INVOICE_ITEMS' AND type = 'U')
BEGIN
    CREATE TABLE HS_BILL_INVOICE_ITEMS (
        INVOICE_ITEM_ID  INT            IDENTITY(1,1) PRIMARY KEY,
        INVOICE_ID       INT            NOT NULL,
        PRODUCT_ID       INT            NOT NULL,
        QUANTITY         INT            NOT NULL,
        UNIT_PRICE       DECIMAL(18,2)  NOT NULL,
        LINE_TOTAL       AS (QUANTITY * UNIT_PRICE),  -- Computed column

        CONSTRAINT FK_HS_BILL_INVOICE_ITEMS_INVOICE
            FOREIGN KEY (INVOICE_ID)
            REFERENCES HS_BILL_INVOICES(INVOICE_ID),

        CONSTRAINT FK_HS_BILL_INVOICE_ITEMS_PRODUCT
            FOREIGN KEY (PRODUCT_ID)
            REFERENCES HS_BILL_PRODUCTS(PRODUCT_ID),

        CONSTRAINT CHK_HS_BILL_INVOICE_ITEMS_QTY
            CHECK (QUANTITY >= 1)
    );

    PRINT 'Created table: HS_BILL_INVOICE_ITEMS';
END
ELSE
BEGIN
    PRINT 'Table already exists: HS_BILL_INVOICE_ITEMS';
END
GO

-- HS_BILL_PAYMENTS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'HS_BILL_PAYMENTS' AND type = 'U')
BEGIN
    CREATE TABLE HS_BILL_PAYMENTS (
        PAYMENT_ID      INT             IDENTITY(1,1) PRIMARY KEY,
        INVOICE_ID      INT             NOT NULL,
        PAYMENT_DATE    DATETIME        NOT NULL,
        AMOUNT          DECIMAL(18,2)   NOT NULL,
        PAYMENT_METHOD  NVARCHAR(50)    NOT NULL,
        REFERENCE       NVARCHAR(200),
        CREATED_DATE    DATETIME        DEFAULT GETDATE(),

        CONSTRAINT FK_HS_BILL_PAYMENTS_INVOICE
            FOREIGN KEY (INVOICE_ID)
            REFERENCES HS_BILL_INVOICES(INVOICE_ID),

        CONSTRAINT CHK_HS_BILL_PAYMENTS_METHOD
            CHECK (PAYMENT_METHOD IN ('Cash', 'Card', 'Bank Transfer', 'Cheque'))
    );

    PRINT 'Created table: HS_BILL_PAYMENTS';
END
ELSE
BEGIN
    PRINT 'Table already exists: HS_BILL_PAYMENTS';
END
GO

-- ------------------------------------------------------------
-- 3. SEED DATA  (MERGE ensures idempotency)
-- ------------------------------------------------------------

-- Customers
MERGE INTO HS_BILL_CUSTOMERS AS target
USING (
    VALUES
        (1, 'John',    'Doe',     'john@test.com',    '0771234567', '123 Main St, Colombo'),
        (2, 'Jane',    'Smith',   'jane@test.com',    '0777654321', '456 High St, Kandy'),
        (3, 'Michael', 'Johnson', 'michael@test.com', '0712345678', '789 Lake Rd, Galle'),
        (4, 'Emily',   'Brown',   'emily@test.com',   NULL,         NULL)
) AS source (CUSTOMER_ID, FIRST_NAME, LAST_NAME, EMAIL, PHONE, ADDRESS)
ON target.EMAIL = source.EMAIL
WHEN NOT MATCHED THEN
    INSERT (FIRST_NAME, LAST_NAME, EMAIL, PHONE, ADDRESS)
    VALUES (source.FIRST_NAME, source.LAST_NAME, source.EMAIL, source.PHONE, source.ADDRESS);
GO

-- Products
MERGE INTO HS_BILL_PRODUCTS AS target
USING (
    VALUES
        ('Web Design Package',      'Full website design and development',      1500.00),
        ('SEO Service',             'Monthly search engine optimisation',        300.00),
        ('Logo Design',             'Professional logo design with revisions',   250.00),
        ('Hosting (Annual)',        'Shared web hosting for one year',           120.00),
        ('Mobile App Development',  'Cross-platform mobile application build', 3500.00)
) AS source (PRODUCT_NAME, DESCRIPTION, UNIT_PRICE)
ON target.PRODUCT_NAME = source.PRODUCT_NAME
WHEN NOT MATCHED THEN
    INSERT (PRODUCT_NAME, DESCRIPTION, UNIT_PRICE)
    VALUES (source.PRODUCT_NAME, source.DESCRIPTION, source.UNIT_PRICE);
GO

-- Invoices
-- Use IF NOT EXISTS so repeat runs skip already-inserted invoices
IF NOT EXISTS (SELECT 1 FROM HS_BILL_INVOICES WHERE INVOICE_NUMBER = 'INV-0001')
BEGIN
    INSERT INTO HS_BILL_INVOICES (INVOICE_NUMBER, CUSTOMER_ID, INVOICE_DATE, DUE_DATE, STATUS, NOTES)
    VALUES (
        'INV-0001',
        (SELECT TOP 1 CUSTOMER_ID FROM HS_BILL_CUSTOMERS WHERE EMAIL = 'john@test.com'),
        GETDATE(),
        DATEADD(DAY, 30, GETDATE()),
        'Pending',
        'First invoice for John Doe'
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM HS_BILL_INVOICES WHERE INVOICE_NUMBER = 'INV-0002')
BEGIN
    INSERT INTO HS_BILL_INVOICES (INVOICE_NUMBER, CUSTOMER_ID, INVOICE_DATE, DUE_DATE, STATUS, NOTES)
    VALUES (
        'INV-0002',
        (SELECT TOP 1 CUSTOMER_ID FROM HS_BILL_CUSTOMERS WHERE EMAIL = 'jane@test.com'),
        GETDATE(),
        DATEADD(DAY, 15, GETDATE()),
        'Pending',
        'Web design project for Jane Smith'
    );
END
GO

-- Invoice Items
IF NOT EXISTS (
    SELECT 1 FROM HS_BILL_INVOICE_ITEMS
    WHERE INVOICE_ID = (SELECT TOP 1 INVOICE_ID FROM HS_BILL_INVOICES WHERE INVOICE_NUMBER = 'INV-0001')
)
BEGIN
    INSERT INTO HS_BILL_INVOICE_ITEMS (INVOICE_ID, PRODUCT_ID, QUANTITY, UNIT_PRICE)
    VALUES
    (
        (SELECT TOP 1 INVOICE_ID FROM HS_BILL_INVOICES  WHERE INVOICE_NUMBER = 'INV-0001'),
        (SELECT TOP 1 PRODUCT_ID FROM HS_BILL_PRODUCTS  WHERE PRODUCT_NAME   = 'Web Design Package'),
        1,
        (SELECT TOP 1 UNIT_PRICE FROM HS_BILL_PRODUCTS  WHERE PRODUCT_NAME   = 'Web Design Package')
    ),
    (
        (SELECT TOP 1 INVOICE_ID FROM HS_BILL_INVOICES  WHERE INVOICE_NUMBER = 'INV-0001'),
        (SELECT TOP 1 PRODUCT_ID FROM HS_BILL_PRODUCTS  WHERE PRODUCT_NAME   = 'Hosting (Annual)'),
        2,
        (SELECT TOP 1 UNIT_PRICE FROM HS_BILL_PRODUCTS  WHERE PRODUCT_NAME   = 'Hosting (Annual)')
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM HS_BILL_INVOICE_ITEMS
    WHERE INVOICE_ID = (SELECT TOP 1 INVOICE_ID FROM HS_BILL_INVOICES WHERE INVOICE_NUMBER = 'INV-0002')
)
BEGIN
    INSERT INTO HS_BILL_INVOICE_ITEMS (INVOICE_ID, PRODUCT_ID, QUANTITY, UNIT_PRICE)
    VALUES
    (
        (SELECT TOP 1 INVOICE_ID FROM HS_BILL_INVOICES  WHERE INVOICE_NUMBER = 'INV-0002'),
        (SELECT TOP 1 PRODUCT_ID FROM HS_BILL_PRODUCTS  WHERE PRODUCT_NAME   = 'Logo Design'),
        1,
        (SELECT TOP 1 UNIT_PRICE FROM HS_BILL_PRODUCTS  WHERE PRODUCT_NAME   = 'Logo Design')
    ),
    (
        (SELECT TOP 1 INVOICE_ID FROM HS_BILL_INVOICES  WHERE INVOICE_NUMBER = 'INV-0002'),
        (SELECT TOP 1 PRODUCT_ID FROM HS_BILL_PRODUCTS  WHERE PRODUCT_NAME   = 'SEO Service'),
        3,
        (SELECT TOP 1 UNIT_PRICE FROM HS_BILL_PRODUCTS  WHERE PRODUCT_NAME   = 'SEO Service')
    );
END
GO

-- Payments (partial payment on INV-0001)
IF NOT EXISTS (
    SELECT 1 FROM HS_BILL_PAYMENTS
    WHERE INVOICE_ID = (SELECT TOP 1 INVOICE_ID FROM HS_BILL_INVOICES WHERE INVOICE_NUMBER = 'INV-0001')
)
BEGIN
    INSERT INTO HS_BILL_PAYMENTS (INVOICE_ID, PAYMENT_DATE, AMOUNT, PAYMENT_METHOD, REFERENCE)
    VALUES (
        (SELECT TOP 1 INVOICE_ID FROM HS_BILL_INVOICES WHERE INVOICE_NUMBER = 'INV-0001'),
        GETDATE(),
        500.00,
        'Bank Transfer',
        'TXN-20260325-001'
    );
END
GO

-- ------------------------------------------------------------
-- 4. VERIFICATION QUERIES
-- ------------------------------------------------------------
PRINT '--- Verification ---';
SELECT 'HS_BILL_CUSTOMERS'    AS TableName, COUNT(*) AS Rows FROM HS_BILL_CUSTOMERS;
SELECT 'HS_BILL_PRODUCTS'     AS TableName, COUNT(*) AS Rows FROM HS_BILL_PRODUCTS;
SELECT 'HS_BILL_INVOICES'     AS TableName, COUNT(*) AS Rows FROM HS_BILL_INVOICES;
SELECT 'HS_BILL_INVOICE_ITEMS'AS TableName, COUNT(*) AS Rows FROM HS_BILL_INVOICE_ITEMS;
SELECT 'HS_BILL_PAYMENTS'     AS TableName, COUNT(*) AS Rows FROM HS_BILL_PAYMENTS;
GO