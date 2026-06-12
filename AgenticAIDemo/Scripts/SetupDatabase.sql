-- ============================================================
-- Agentic AI Demo - SQL Server Setup Script
-- Run this in SQL Server Management Studio (SSMS) or sqlcmd
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'AgenticAIDemo')
BEGIN
    CREATE DATABASE AgenticAIDemo;
END
GO

USE AgenticAIDemo;
GO

-- Drop tables if re-running script
IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID('dbo.Customers', 'U') IS NOT NULL DROP TABLE dbo.Customers;
GO

CREATE TABLE dbo.Customers
(
    CustomerId   INT IDENTITY(1,1) PRIMARY KEY,
    FullName     NVARCHAR(100) NOT NULL,
    Email        NVARCHAR(150) NOT NULL,
    City         NVARCHAR(80)  NOT NULL,
    Membership   NVARCHAR(20)  NOT NULL,  -- Gold, Silver, Bronze
    JoinedDate   DATE          NOT NULL
);
GO

CREATE TABLE dbo.Orders
(
    OrderId      INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId   INT           NOT NULL,
    ProductName  NVARCHAR(120) NOT NULL,
    Quantity     INT           NOT NULL,
    Amount       DECIMAL(10,2) NOT NULL,
    OrderDate    DATE          NOT NULL,
    CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(CustomerId)
);
GO

INSERT INTO dbo.Customers (FullName, Email, City, Membership, JoinedDate) VALUES
(N'Alice Johnson',  N'alice@email.com',   N'Sydney',    N'Gold',   '2022-03-15'),
(N'Bob Smith',      N'bob@email.com',     N'Melbourne', N'Silver', '2023-01-10'),
(N'Carol White',    N'carol@email.com',   N'Brisbane',  N'Gold',   '2021-11-20'),
(N'David Brown',    N'david@email.com',   N'Perth',     N'Bronze', '2024-06-01'),
(N'Emma Davis',     N'emma@email.com',    N'Sydney',    N'Silver', '2023-08-22'),
(N'Frank Miller',   N'frank@email.com',   N'Adelaide',  N'Gold',   '2020-05-30'),
(N'Grace Wilson',   N'grace@email.com',   N'Melbourne', N'Bronze', '2024-02-14'),
(N'Henry Taylor',   N'henry@email.com',   N'Canberra',  N'Silver', '2022-09-05');
GO

INSERT INTO dbo.Orders (CustomerId, ProductName, Quantity, Amount, OrderDate) VALUES
(1, N'Laptop',           1,  1299.00, '2025-01-05'),
(1, N'Wireless Mouse',   2,    49.98, '2025-01-05'),
(2, N'Office Chair',     1,   349.00, '2025-02-12'),
(3, N'Smartphone',       1,   899.00, '2025-03-01'),
(3, N'Phone Case',       1,    29.99, '2025-03-01'),
(4, N'USB Cable',        3,    35.97, '2025-04-18'),
(5, N'Monitor',          1,   459.00, '2025-05-02'),
(6, N'Tablet',           1,   649.00, '2025-06-10'),
(6, N'Keyboard',         1,    89.00, '2025-06-10'),
(7, N'Headphones',       1,   199.00, '2025-07-22'),
(8, N'Webcam',           1,   129.00, '2025-08-15'),
(2, N'Desk Lamp',        1,    59.00, '2025-09-01');
GO

PRINT 'Database AgenticAIDemo is ready!';
PRINT 'Tables: Customers (8 rows), Orders (12 rows)';
GO
