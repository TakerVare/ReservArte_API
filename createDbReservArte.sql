USE master;
GO

-- Eliminar BD si existe
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'ReservArteDB')
BEGIN
    ALTER DATABASE ReservArteDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ReservArteDB;
END
GO

CREATE DATABASE ReservArteDB;
GO

USE ReservArteDB;
GO

PRINT 'Base de datos ReservArteDB creada correctamente';

-- ============================================
-- TABLA USERS
-- ============================================
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Password NVARCHAR(100) NOT NULL,
    Rol NVARCHAR(50) NOT NULL CHECK (Rol IN ('admin', 'employee', 'client')),
    Phone NVARCHAR(20) NULL,
    ProfileImageUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================
-- TABLA CUSTOMERS
-- ============================================
CREATE TABLE Customers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Phone NVARCHAR(20) NULL,
    Rol NVARCHAR(50) NOT NULL DEFAULT 'client',
    ProfileImageUrl NVARCHAR(500) NULL,
    BirthDate DATE NULL,
    Category NVARCHAR(50) NOT NULL DEFAULT 'regular' CHECK (Category IN ('regular', 'vip', 'blocked')),
    LoyaltyPoints INT NOT NULL DEFAULT 0,
    IsBlocked BIT NOT NULL DEFAULT 0,
    BlockedReason NVARCHAR(500) NULL,
    PreferredContactMethod NVARCHAR(50) NOT NULL DEFAULT 'email' CHECK (PreferredContactMethod IN ('email', 'phone', 'sms', 'whatsapp')),
    MarketingConsent BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================
-- TABLA EMPLOYEES
-- ============================================
CREATE TABLE Employees (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Phone NVARCHAR(20) NULL,
    Rol NVARCHAR(50) NOT NULL DEFAULT 'employee',
    ProfileImageUrl NVARCHAR(500) NULL,
    HireDate DATE NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- ============================================
-- RESTO DE TABLAS
-- ============================================

CREATE TABLE ServiceCategories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Color NVARCHAR(20) NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Services (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    DurationMinutes INT NOT NULL,
    BasePrice DECIMAL(10,2) NOT NULL,
    CategoryId INT NULL,
    ImageUrl NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    RequiresAllergyTest BIT NOT NULL DEFAULT 0,
    AllergyTestHoursBefore INT NOT NULL DEFAULT 48,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    FOREIGN KEY (CategoryId) REFERENCES ServiceCategories(Id)
);

CREATE TABLE ServiceVariations (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ServiceId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    PriceModifier DECIMAL(10,2) NOT NULL DEFAULT 0,
    DurationModifier INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE
);

CREATE TABLE ServicePricings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ServiceId INT NOT NULL,
    EmployeeLevel NVARCHAR(50) NOT NULL CHECK (EmployeeLevel IN ('Junior', 'Senior', 'Expert')),
    Price DECIMAL(10,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE
);

CREATE TABLE ProductCategories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Brand NVARCHAR(100) NULL,
    Sku NVARCHAR(100) NULL UNIQUE,
    Price DECIMAL(10,2) NOT NULL DEFAULT 0,
    Stock INT NOT NULL DEFAULT 0,
    MinStockAlert INT NOT NULL DEFAULT 5,
    ImageUrl NVARCHAR(500) NULL,
    CategoryId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    FOREIGN KEY (CategoryId) REFERENCES ProductCategories(Id)
);

CREATE TABLE ServiceProducts (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ServiceId INT NOT NULL,
    ProductId INT NOT NULL,
    QuantityUsed DECIMAL(10,2) NULL,
    Notes NVARCHAR(500) NULL,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

CREATE TABLE ServicePackages (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    TotalPrice DECIMAL(10,2) NOT NULL,
    DiscountPercentage DECIMAL(5,2) NOT NULL DEFAULT 0,
    ImageUrl NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL
);

CREATE TABLE ServicePackageItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ServicePackageId INT NOT NULL,
    ServiceId INT NOT NULL,
    [Order] INT NOT NULL,
    FOREIGN KEY (ServicePackageId) REFERENCES ServicePackages(Id) ON DELETE CASCADE,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id)
);

CREATE TABLE ServicePromotions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ServiceId INT NULL,
    ServicePackageId INT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    DiscountPercentage DECIMAL(5,2) NOT NULL,
    DiscountAmount DECIMAL(10,2) NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    IsSeasonalService BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ServiceId) REFERENCES Services(Id),
    FOREIGN KEY (ServicePackageId) REFERENCES ServicePackages(Id),
    CHECK ((ServiceId IS NOT NULL AND ServicePackageId IS NULL) OR (ServiceId IS NULL AND ServicePackageId IS NOT NULL))
);

CREATE TABLE EmployeeAvailabilities (
    Id INT PRIMARY KEY IDENTITY(1,1),
    EmployeeId INT NOT NULL,
    DayOfWeek INT NOT NULL CHECK (DayOfWeek BETWEEN 0 AND 6),
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    IsRecurring BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE
);

CREATE TABLE EmployeeExceptions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    EmployeeId INT NOT NULL,
    StartDateTime DATETIME2 NOT NULL,
    EndDateTime DATETIME2 NOT NULL,
    Reason NVARCHAR(500) NULL,
    Type NVARCHAR(50) NOT NULL CHECK (Type IN ('vacation', 'sick_leave', 'personal', 'training', 'other')),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE
);

CREATE TABLE EmployeeServices (
    EmployeeId INT NOT NULL,
    ServiceId INT NOT NULL,
    ProficiencyLevel INT NOT NULL DEFAULT 1 CHECK (ProficiencyLevel BETWEEN 1 AND 5),
    PRIMARY KEY (EmployeeId, ServiceId),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE
);

CREATE TABLE Appointments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    EmployeeId INT NOT NULL,
    AppointmentDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'pending' CHECK (Status IN ('pending', 'confirmed', 'in_progress', 'completed', 'cancelled', 'cancelled_by_customer', 'cancelled_by_business', 'no_show')),
    TotalPrice DECIMAL(10,2) NOT NULL,
    DepositAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
    RedsysOrderNumber NVARCHAR(50) NULL,
    RedsysPreAuthToken NVARCHAR(200) NULL,
    PaymentMethodId INT NULL,
    CancellationReason NVARCHAR(500) NULL,
    CancelledAt DATETIME2 NULL,
    CancelledById INT NULL,
    CancelledByType NVARCHAR(50) NULL CHECK (CancelledByType IN ('customer', 'business', 'system')),
    Notes NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
);

CREATE TABLE AppointmentServiceItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    AppointmentId INT NOT NULL,
    ServiceId INT NOT NULL,
    ServiceVariationId INT NULL,
    Price DECIMAL(10,2) NOT NULL,
    DurationMinutes INT NOT NULL,
    [Order] INT NOT NULL,
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id) ON DELETE CASCADE,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id),
    FOREIGN KEY (ServiceVariationId) REFERENCES ServiceVariations(Id)
);

CREATE TABLE CustomerPaymentMethods (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    RedsysToken NVARCHAR(200) NOT NULL,
    RedsysCofTxnid NVARCHAR(100) NULL,
    CardLast4 NVARCHAR(4) NOT NULL,
    CardBrand NVARCHAR(50) NOT NULL,
    CardExpiry NVARCHAR(4) NOT NULL,
    IsDefault BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
);

CREATE TABLE Payments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    AppointmentId INT NULL,
    CustomerId INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    Currency NVARCHAR(3) NOT NULL DEFAULT 'EUR',
    PaymentMethodType NVARCHAR(50) NOT NULL CHECK (PaymentMethodType IN ('card', 'cash', 'transfer', 'bizum')),
    Status NVARCHAR(50) NOT NULL DEFAULT 'pending' CHECK (Status IN ('pending', 'authorized', 'captured', 'failed', 'cancelled', 'refunded', 'partially_refunded')),
    RedsysOrderNumber NVARCHAR(50) NULL UNIQUE,
    RedsysAuthCode NVARCHAR(50) NULL,
    RedsysResponse NVARCHAR(MAX) NULL,
    RedsysTransactionType NVARCHAR(10) NULL,
    RedsysCardNumber NVARCHAR(19) NULL,
    CustomerPaymentMethodId INT NULL,
    ProcessedAt DATETIME2 NULL,
    RefundedAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
    RefundedAt DATETIME2 NULL,
    Metadata NVARCHAR(MAX) NULL,
    Notes NVARCHAR(500) NULL,
    RegisteredById INT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id),
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    FOREIGN KEY (CustomerPaymentMethodId) REFERENCES CustomerPaymentMethods(Id)
);

CREATE TABLE CustomerNotes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    EmployeeId INT NOT NULL,
    Note NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
);

CREATE TABLE CustomerAllergies (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    AllergyDescription NVARCHAR(500) NOT NULL,
    Severity NVARCHAR(50) NOT NULL CHECK (Severity IN ('mild', 'moderate', 'severe')),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
);

CREATE TABLE CustomerConsents (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    ConsentType NVARCHAR(100) NOT NULL,
    IsGranted BIT NOT NULL,
    GrantedAt DATETIME2 NULL,
    RevokedAt DATETIME2 NULL,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
);

CREATE TABLE ServicePhotos (
    Id INT PRIMARY KEY IDENTITY(1,1),
    AppointmentId INT NOT NULL,
    Type NVARCHAR(50) NOT NULL CHECK (Type IN ('before', 'after', 'process')),
    S3Key NVARCHAR(500) NOT NULL,
    S3Bucket NVARCHAR(200) NOT NULL,
    UploadedBy INT NOT NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsPublic BIT NOT NULL DEFAULT 0,
    ExpiresAt DATETIME2 NOT NULL,
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id) ON DELETE CASCADE,
    FOREIGN KEY (UploadedBy) REFERENCES Employees(Id)
);

CREATE TABLE CancellationPolicies (
    Id INT PRIMARY KEY IDENTITY(1,1),
    MinHoursBeforeCancel INT NOT NULL DEFAULT 24,
    PenaltyPercentage INT NOT NULL DEFAULT 0 CHECK (PenaltyPercentage BETWEEN 0 AND 100),
    MaxNoShowsBeforeBlock INT NOT NULL DEFAULT 3,
    VipMinHoursBeforeCancel INT NULL,
    VipPenaltyPercentage INT NULL CHECK (VipPenaltyPercentage BETWEEN 0 AND 100),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL
);

CREATE TABLE WaitingList (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    ServiceId INT NOT NULL,
    PreferredEmployeeId INT NULL,
    PreferredDate DATETIME2 NULL,
    DateRangeStart DATETIME2 NOT NULL,
    DateRangeEnd DATETIME2 NOT NULL,
    Priority INT NOT NULL DEFAULT 1000,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    NotifiedAt DATETIME2 NULL,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id),
    FOREIGN KEY (PreferredEmployeeId) REFERENCES Employees(Id)
);

CREATE TABLE ProductSales (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NULL,
    AppointmentId INT NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Completed', 'Cancelled')),
    PaymentMethod NVARCHAR(50) NULL,
    Notes NVARCHAR(MAX) NULL,
    SoldBy INT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id),
    FOREIGN KEY (SoldBy) REFERENCES Employees(Id)
);

CREATE TABLE ProductSaleItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SaleId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    Subtotal DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (SaleId) REFERENCES ProductSales(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

CREATE TABLE InventoryMovements (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    MovementType NVARCHAR(50) NOT NULL CHECK (MovementType IN ('purchase', 'sale', 'adjustment', 'waste', 'return')),
    ReferenceId INT NULL,
    Notes NVARCHAR(500) NULL,
    CreatedBy INT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ProductId) REFERENCES Products(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Employees(Id)
);

CREATE TABLE MessageTemplates (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Type NVARCHAR(100) NOT NULL CHECK (Type IN ('email', 'sms', 'whatsapp', 'push')),
    Subject NVARCHAR(500) NULL,
    Body NVARCHAR(MAX) NOT NULL,
    Language NVARCHAR(10) NOT NULL DEFAULT 'es'
);

CREATE TABLE ReminderConfigurations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ReminderOrder INT NOT NULL,
    HoursBeforeAppointment INT NOT NULL,
    Channel NVARCHAR(50) NOT NULL CHECK (Channel IN ('email', 'sms', 'whatsapp', 'push')),
    IsActive BIT NOT NULL DEFAULT 1,
    MessageTemplateId UNIQUEIDENTIFIER NOT NULL,
    AllowedSendStartTime TIME NULL,
    AllowedSendEndTime TIME NULL,
    FOREIGN KEY (MessageTemplateId) REFERENCES MessageTemplates(Id)
);

CREATE TABLE ReminderLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AppointmentId INT NOT NULL,
    ReminderConfigurationId UNIQUEIDENTIFIER NOT NULL,
    Channel NVARCHAR(50) NOT NULL,
    SentAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Status NVARCHAR(50) NOT NULL CHECK (Status IN ('pending', 'sent', 'failed', 'delivered', 'read')),
    ExternalMessageId NVARCHAR(200) NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id) ON DELETE CASCADE,
    FOREIGN KEY (ReminderConfigurationId) REFERENCES ReminderConfigurations(Id)
);

CREATE TABLE ConfirmationTokens (
    Token NVARCHAR(200) PRIMARY KEY,
    AppointmentId INT NOT NULL,
    Action NVARCHAR(50) NOT NULL CHECK (Action IN ('confirm', 'cancel')),
    ExpiresAt DATETIME2 NOT NULL,
    UsedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id) ON DELETE CASCADE
);

PRINT 'Base de datos ReservArteDB inicializada correctamente';

-- ============================================
-- DATOS INICIALES
-- ============================================

-- USUARIOS (para autenticación)
INSERT INTO Users (FirstName, LastName, Email, Password, Rol, Phone) VALUES
('Guillermo', 'Admin', 'guille@svalero.com', '1234', 'admin', '+34600000001'),
('María', 'García', 'maria.garcia@reservarte.com', 'Maria123!', 'employee', '+34600000002'),
('Laura', 'Martínez', 'laura.martinez@reservarte.com', 'Laura123!', 'employee', '+34600000003'),
('Ana', 'López', 'ana.lopez@email.com', 'Cliente123!', 'client', '+34600000004'),
('Carmen', 'Rodríguez', 'carmen.rodriguez@email.com', 'Cliente123!', 'client', '+34600000005'),
('Isabel', 'Sánchez', 'isabel.sanchez@email.com', 'Cliente123!', 'client', '+34600000006');

-- EMPLOYEES (tabla independiente)
INSERT INTO Employees (FirstName, LastName, Email, Phone, Rol, HireDate, IsActive) VALUES
('María', 'García', 'maria.garcia@reservarte.com', '+34600000002', 'employee', '2023-01-15', 1),
('Laura', 'Martínez', 'laura.martinez@reservarte.com', '+34600000003', 'employee', '2023-03-01', 1);

-- CUSTOMERS (tabla independiente)
INSERT INTO Customers (FirstName, LastName, Email, Phone, Rol, Category, LoyaltyPoints, PreferredContactMethod, MarketingConsent) VALUES
('Ana', 'López', 'ana.lopez@email.com', '+34600000004', 'client', 'regular', 0, 'email', 1),
('Carmen', 'Rodríguez', 'carmen.rodriguez@email.com', '+34600000005', 'client', 'vip', 150, 'whatsapp', 1),
('Isabel', 'Sánchez', 'isabel.sanchez@email.com', '+34600000006', 'client', 'regular', 50, 'email', 0);

-- CATEGORÍAS DE SERVICIOS
INSERT INTO ServiceCategories (Name, Description, Color, DisplayOrder, IsActive) VALUES
('Corte', 'Servicios de corte de cabello', '#FF6B6B', 1, 1),
('Color', 'Tintes y coloración', '#4ECDC4', 2, 1),
('Tratamientos', 'Tratamientos capilares', '#95E1D3', 3, 1);

-- SERVICIOS
INSERT INTO Services (Name, Description, DurationMinutes, BasePrice, CategoryId, IsActive, RequiresAllergyTest) VALUES
('Corte Mujer', 'Corte de cabello para mujer', 30, 25.00, 1, 1, 0),
('Corte Hombre', 'Corte de cabello para hombre', 20, 15.00, 1, 1, 0),
('Tinte Completo', 'Coloración completa del cabello', 120, 65.00, 2, 1, 1),
('Mechas', 'Mechas californianas o balayage', 150, 85.00, 2, 1, 1),
('Tratamiento Keratina', 'Tratamiento alisador de keratina', 180, 120.00, 3, 1, 0),
('Hidratación Profunda', 'Mascarilla hidratante intensiva', 45, 35.00, 3, 1, 0);

-- VARIACIONES DE SERVICIOS (ServiceVariations)
-- ServiceId: 1=Corte Mujer, 2=Corte Hombre, 3=Tinte Completo, 4=Mechas, 5=Tratamiento Keratina, 6=Hidratación
INSERT INTO ServiceVariations (ServiceId, Name, PriceModifier, DurationModifier, IsActive) VALUES
(1, N'Solo corte', 0, 0, 1),
(1, N'Corte + lavado', 3.00, 10, 1),
(1, N'Corte + secado', 8.00, 15, 1),
(1, N'Corte + lavado + secado', 10.00, 25, 1),
(2, N'Corte clásico', 0, 0, 1),
(2, N'Corte + barba', 5.00, 10, 1),
(2, N'Corte degradado', 3.00, 5, 1),
(3, N'Tinte raíces', -25.00, -60, 1),
(3, N'Retoque', -15.00, -30, 1),
(3, N'Tinte + mascarilla', 12.00, 20, 1),
(4, N'Mechas completas', 0, 0, 1),
(4, N'Babylights', 15.00, 30, 1),
(4, N'Balayage', 20.00, 45, 1),
(5, N'Keratina express', -30.00, -60, 1),
(5, N'Keratina brasileña', 0, 0, 1),
(6, N'Hidratación estándar', 0, 0, 1),
(6, N'Hidratación + secado', 5.00, 15, 1);

-- DISPONIBILIDAD EMPLEADOS
INSERT INTO EmployeeAvailabilities (EmployeeId, DayOfWeek, StartTime, EndTime, IsRecurring) VALUES
(1, 1, '09:00', '18:00', 1), -- María Lunes
(1, 2, '09:00', '18:00', 1), -- María Martes
(1, 3, '09:00', '18:00', 1), -- María Miércoles
(1, 4, '09:00', '18:00', 1), -- María Jueves
(1, 5, '09:00', '14:00', 1), -- María Viernes
(2, 1, '10:00', '19:00', 1), -- Laura Lunes
(2, 2, '10:00', '19:00', 1), -- Laura Martes
(2, 3, '10:00', '19:00', 1), -- Laura Miércoles
(2, 4, '10:00', '19:00', 1), -- Laura Jueves
(2, 5, '10:00', '15:00', 1); -- Laura Viernes

-- POLÍTICA DE CANCELACIÓN
INSERT INTO CancellationPolicies (MinHoursBeforeCancel, PenaltyPercentage, MaxNoShowsBeforeBlock, VipMinHoursBeforeCancel, VipPenaltyPercentage, IsActive) VALUES
(24, 50, 3, 12, 25, 1);

-- CITAS (Appointments) - datos de ejemplo
-- CustomerId: 1=Ana, 2=Carmen, 3=Isabel | EmployeeId: 1=María, 2=Laura
INSERT INTO Appointments (CustomerId, EmployeeId, AppointmentDate, StartTime, EndTime, Status, TotalPrice, DepositAmount, Notes) VALUES
(1, 1, CAST('2026-03-06' AS DATE), '10:00', '10:30', 'confirmed', 25.00, 0, 'Corte mujer - primera visita'),
(2, 1, CAST('2026-03-07' AS DATE), '11:00', '13:00', 'confirmed', 65.00, 20.00, 'Tinte completo'),
(3, 2, CAST('2026-03-06' AS DATE), '10:00', '10:20', 'pending', 15.00, 0, NULL),
(1, 2, CAST('2026-03-06' AS DATE), '12:00', '12:45', 'pending', 35.00, 0, 'Hidratación profunda'),
(2, 1, CAST('2026-03-07' AS DATE), '09:30', '10:00', 'pending', 25.00, 0, NULL),
(3, 2, CAST('2026-03-06' AS DATE), '16:00', '16:30', 'completed', 25.00, 0, 'Corte realizado'),
(1, 1, CAST('2026-03-09' AS DATE), '11:00', '14:00', 'completed', 85.00, 30.00, 'Mechas - cliente satisfecha'),
(3, 1, CAST('2026-03-10' AS DATE), '17:00', '17:20', 'cancelled', 15.00, 0, NULL);

-- Actualizar cancelada con motivo (opcional)
UPDATE Appointments SET CancellationReason = N'Cliente no pudo asistir', CancelledAt = GETUTCDATE(), CancelledByType = 'customer', UpdatedAt = GETUTCDATE() WHERE Id = 8;

-- ============================================
-- DEMO: Tablas que estaban vacías
-- ============================================

-- ServicePricings (precio por nivel: Services 1-6, niveles Junior/Senior/Expert)
INSERT INTO ServicePricings (ServiceId, EmployeeLevel, Price) VALUES
(1, 'Junior', 22.00), (1, 'Senior', 25.00), (1, 'Expert', 28.00),
(2, 'Junior', 13.00), (2, 'Senior', 15.00), (2, 'Expert', 17.00),
(3, 'Junior', 58.00), (3, 'Senior', 65.00), (3, 'Expert', 72.00),
(4, 'Junior', 75.00), (4, 'Senior', 85.00), (4, 'Expert', 95.00),
(5, 'Junior', 105.00), (5, 'Senior', 120.00), (5, 'Expert', 135.00),
(6, 'Junior', 30.00), (6, 'Senior', 35.00), (6, 'Expert', 40.00);

-- ProductCategories
INSERT INTO ProductCategories (Name, Description, IsActive) VALUES
(N'Champú y acondicionador', N'Productos de lavado', 1),
(N'Tintes', N'Coloración y tintes', 1),
(N'Tratamientos', N'Mascarillas y keratina', 1),
(N'Estilizado', N'Fijadores y secado', 1);

-- Products (CategoryId 1-4)
INSERT INTO Products (Name, Description, Brand, Sku, Price, Stock, MinStockAlert, CategoryId, IsActive) VALUES
(N'Champú hidratante', N'Champú para cabello seco', 'Loreal', 'CH-HID-001', 12.50, 25, 5, 1, 1),
(N'Acondicionador reparador', N'Acondicionador sin aclarado', 'Loreal', 'AC-REP-001', 14.00, 20, 5, 1, 1),
(N'Tinte castaño 5', N'Tinte permanente castaño medio', 'Koleston', 'TIN-CAS5-001', 8.50, 50, 10, 2, 1),
(N'Oxidante 20 vol', N'Developer 20 vol 1L', 'Wella', 'OXI-20-001', 4.00, 30, 5, 2, 1),
(N'Mascarilla keratina', N'Mascarilla profesional keratina', 'Loreal', 'MAS-KER-001', 25.00, 15, 3, 3, 1),
(N'Aceite nutritivo', N'Aceite capilar 100 ml', 'Kérastase', 'ACE-NUT-001', 32.00, 18, 3, 3, 1),
(N'Laca fijación fuerte', N'Laca 400 ml', 'Schwarzkopf', 'LAC-FIJ-001', 9.00, 40, 5, 4, 1),
(N'Spray termoprotector', N'Protector calor 250 ml', 'Redken', 'SPR-TER-001', 18.00, 22, 5, 4, 1);

-- ServiceProducts (qué producto usa cada servicio; ServiceId 1-6, ProductId 1-8)
INSERT INTO ServiceProducts (ServiceId, ProductId, QuantityUsed, Notes) VALUES
(1, 1, 0.05, N'Champú lavado previo'), (1, 2, 0.03, NULL), (1, 7, 0.02, NULL),
(2, 1, 0.04, NULL), (2, 7, 0.01, NULL),
(3, 3, 1, N'Unidad por aplicación'), (3, 4, 0.15, NULL), (3, 1, 0.08, NULL),
(4, 3, 0.5, NULL), (4, 4, 0.2, NULL), (4, 5, 0.1, NULL),
(5, 5, 0.25, NULL), (5, 1, 0.1, NULL),
(6, 5, 0.15, NULL), (6, 1, 0.06, NULL), (6, 6, 0.02, NULL);

-- ServicePackages
INSERT INTO ServicePackages (Name, Description, TotalPrice, DiscountPercentage, IsActive) VALUES
(N'Pack bienvenida', N'Corte + lavado + hidratación express', 55.00, 10, 1),
(N'Pack color completo', N'Tinte + mascarilla + secado', 78.00, 5, 1),
(N'Pack premium', N'Mechas + hidratación + secado', 110.00, 8, 1);

-- ServicePackageItems (PackageId 1-3, ServiceId)
INSERT INTO ServicePackageItems (ServicePackageId, ServiceId, [Order]) VALUES
(1, 1, 1), (1, 6, 2),
(2, 3, 1), (2, 6, 2),
(3, 4, 1), (3, 6, 2);

-- ServicePromotions (por servicio o por paquete; fechas futuras)
INSERT INTO ServicePromotions (ServiceId, ServicePackageId, Name, Description, DiscountPercentage, StartDate, EndDate, IsActive) VALUES
(1, NULL, N'Promo corte mujer', N'Descuento corte mujer', 15, CAST('2026-09-01' AS DATE), CAST('2026-10-31' AS DATE), 1),
(NULL, 1, N'Promo pack bienvenida', N'Pack bienvenida -20%', 20, CAST('2026-09-01' AS DATE), CAST('2026-10-31' AS DATE), 1);

-- EmployeeExceptions (vacaciones/ausencias; EmployeeId 1=María, 2=Laura)
INSERT INTO EmployeeExceptions (EmployeeId, StartDateTime, EndDateTime, Reason, Type) VALUES
(1, DATEADD(dd, 14, CAST(GETUTCDATE() AS DATE)), DATEADD(dd, 21, CAST(GETUTCDATE() AS DATE)), N'Vacaciones', 'vacation'),
(2, DATEADD(dd, 7, CAST(GETUTCDATE() AS DATE)), DATEADD(dd, 8, CAST(GETUTCDATE() AS DATE)), N'Formación', 'training');

-- EmployeeServices (qué servicios hace cada empleado; ProficiencyLevel 1-5)
INSERT INTO EmployeeServices (EmployeeId, ServiceId, ProficiencyLevel) VALUES
(1, 1, 5), (1, 2, 4), (1, 3, 5), (1, 4, 5), (1, 5, 4), (1, 6, 5),
(2, 1, 5), (2, 2, 5), (2, 3, 4), (2, 4, 4), (2, 5, 3), (2, 6, 5);

-- AppointmentServiceItems (detalle de servicios por cita; Appointments 1-8)
INSERT INTO AppointmentServiceItems (AppointmentId, ServiceId, ServiceVariationId, Price, DurationMinutes, [Order]) VALUES
(1, 1, 2, 28.00, 40, 1),
(2, 3, 9, 65.00, 120, 1),
(3, 2, 5, 15.00, 20, 1),
(4, 6, 16, 35.00, 45, 1),
(5, 1, 1, 25.00, 30, 1),
(6, 1, 1, 25.00, 30, 1),
(7, 4, 12, 85.00, 150, 1),
(8, 2, 5, 15.00, 20, 1);

-- CustomerPaymentMethods (datos demo; RedsysToken ficticio)
INSERT INTO CustomerPaymentMethods (CustomerId, RedsysToken, CardLast4, CardBrand, CardExpiry, IsDefault) VALUES
(1, 'DEMO_TOKEN_ANA_001', '4242', 'Visa', '1228', 1),
(2, 'DEMO_TOKEN_CARMEN_001', '5555', 'Mastercard', '0130', 1);

-- Payments (pagos asociados a citas/clientes)
INSERT INTO Payments (AppointmentId, CustomerId, Amount, Currency, PaymentMethodType, Status, ProcessedAt) VALUES
(1, 1, 25.00, 'EUR', 'card', 'captured', GETUTCDATE()),
(2, 2, 65.00, 'EUR', 'card', 'captured', GETUTCDATE()),
(6, 3, 25.00, 'EUR', 'cash', 'captured', GETUTCDATE()),
(7, 1, 85.00, 'EUR', 'card', 'captured', GETUTCDATE());

-- CustomerNotes (notas de empleados sobre clientes)
INSERT INTO CustomerNotes (CustomerId, EmployeeId, Note) VALUES
(1, 1, N'Cliente prefiere secado con difusor.'),
(2, 1, N'VIP - Preferencia por Laura para mechas.'),
(3, 2, N'Primera visita - resultado corte muy bueno.'),
(1, 2, N'Sensible al calor - usar temperatura media.');

-- CustomerAllergies
INSERT INTO CustomerAllergies (CustomerId, AllergyDescription, Severity) VALUES
(2, N'PPD (parafenilendiamina) - prueba realizada OK', 'mild'),
(3, N'Níquel - evitar accesorios con níquel', 'moderate');

-- CustomerConsents
INSERT INTO CustomerConsents (CustomerId, ConsentType, IsGranted, GrantedAt) VALUES
(1, 'marketing_email', 1, GETUTCDATE()), (1, 'data_retention', 1, GETUTCDATE()),
(2, 'marketing_email', 1, GETUTCDATE()), (2, 'marketing_whatsapp', 1, GETUTCDATE()), (2, 'data_retention', 1, GETUTCDATE()),
(3, 'data_retention', 1, GETUTCDATE());

-- ServicePhotos (fotos demo; S3Key/Bucket y ExpiresAt ficticios)
INSERT INTO ServicePhotos (AppointmentId, Type, S3Key, S3Bucket, UploadedBy, IsPublic, ExpiresAt) VALUES
(6, 'before', 'demos/apt6-before.jpg', 'reservarte-demo', 2, 0, DATEADD(dd, 365, GETUTCDATE())),
(6, 'after', 'demos/apt6-after.jpg', 'reservarte-demo', 2, 0, DATEADD(dd, 365, GETUTCDATE())),
(7, 'before', 'demos/apt7-before.jpg', 'reservarte-demo', 1, 0, DATEADD(dd, 365, GETUTCDATE()));

-- WaitingList (lista de espera)
INSERT INTO WaitingList (CustomerId, ServiceId, PreferredEmployeeId, DateRangeStart, DateRangeEnd, Priority) VALUES
(3, 4, 1, CAST(GETUTCDATE() AS DATE), DATEADD(dd, 14, CAST(GETUTCDATE() AS DATE)), 100),
(1, 3, NULL, DATEADD(dd, 7, CAST(GETUTCDATE() AS DATE)), DATEADD(dd, 21, CAST(GETUTCDATE() AS DATE)), 500);

-- ProductSales (ventas de productos; SoldBy = EmployeeId)
INSERT INTO ProductSales (CustomerId, AppointmentId, TotalAmount, Status, PaymentMethod, SoldBy) VALUES
(1, 1, 21.50, 'Completed', 'card', 1),
(2, NULL, 74.00, 'Completed', 'card', 1),
(NULL, 6, 9.00, 'Completed', 'cash', 2);

-- ProductSaleItems (detalle de ventas; SaleId 1-3, ProductId 1-8)
INSERT INTO ProductSaleItems (SaleId, ProductId, Quantity, UnitPrice, Subtotal) VALUES
(1, 1, 1, 12.50, 12.50), (1, 7, 1, 9.00, 9.00),
(2, 3, 2, 8.50, 17.00), (2, 5, 1, 25.00, 25.00), (2, 6, 1, 32.00, 32.00),
(3, 7, 1, 9.00, 9.00);

-- InventoryMovements (movimientos de stock; MovementType: purchase, sale, adjustment, waste, return)
INSERT INTO InventoryMovements (ProductId, Quantity, MovementType, Notes, CreatedBy) VALUES
(1, 50, 'purchase', N'Pedido proveedor', 1), (2, 40, 'purchase', N'Pedido proveedor', 1),
(3, -2, 'sale', N'Venta SaleId 2', 1), (5, -1, 'sale', N'Venta SaleId 2', 1), (6, -1, 'sale', N'Venta SaleId 2', 1),
(1, -1, 'sale', N'Venta SaleId 1', 1), (7, -2, 'sale', N'Ventas SaleId 1 y 3', NULL),
(4, 5, 'adjustment', N'Ajuste inventario', 2);

-- MessageTemplates (plantillas para recordatorios)
INSERT INTO MessageTemplates (Name, Type, Subject, Body, Language) VALUES
(N'Recordatorio cita - Email', 'email', N'Recordatorio: tu cita en ReservArte', N'Hola {CustomerName}, te recordamos tu cita el {AppointmentDate} a las {AppointmentTime}. Saludos, ReservArte.', 'es'),
(N'Recordatorio cita - SMS', 'sms', NULL, N'ReservArte: Cita el {AppointmentDate} a las {AppointmentTime}. Confirmar: {ConfirmUrl}', 'es'),
(N'Recordatorio cita - WhatsApp', 'whatsapp', NULL, N'Hola {CustomerName}, tu cita es el {AppointmentDate} a las {AppointmentTime}. ¿Confirmas?', 'es');

-- ReminderConfigurations (orden y canal; MessageTemplateId se obtiene por nombre vía subconsulta o usamos los Ids generados)
-- En SQL Server los GUIDs se generan con NEWID(); como INSERT anterior usa DEFAULT NEWID(), necesitamos referenciar por Id.
-- Usaremos variables o insertamos ReminderConfigurations referenciando por el orden de inserción.
-- MessageTemplates tiene Id UNIQUEIDENTIFIER DEFAULT NEWID() - no conocemos el Id hasta después del INSERT.
-- Opción: insertar ReminderConfigurations en un bloque que use SELECT Id FROM MessageTemplates WHERE Name = '...'
INSERT INTO ReminderConfigurations (ReminderOrder, HoursBeforeAppointment, Channel, IsActive, MessageTemplateId)
SELECT 1, 24, 'email', 1, Id FROM MessageTemplates WHERE Name = N'Recordatorio cita - Email';
INSERT INTO ReminderConfigurations (ReminderOrder, HoursBeforeAppointment, Channel, IsActive, MessageTemplateId)
SELECT 2, 2, 'sms', 1, Id FROM MessageTemplates WHERE Name = N'Recordatorio cita - SMS';

-- ReminderLogs (ejemplo de recordatorio enviado para cita 1)
INSERT INTO ReminderLogs (AppointmentId, ReminderConfigurationId, Channel, Status)
SELECT 1, Id, 'email', 'sent' FROM ReminderConfigurations WHERE Channel = 'email' AND ReminderOrder = 1;

-- ConfirmationTokens (tokens de confirmación/cancelación para citas pendientes)
INSERT INTO ConfirmationTokens (Token, AppointmentId, Action, ExpiresAt) VALUES
('demo-confirm-apt3', 3, 'confirm', DATEADD(hh, 48, GETUTCDATE())),
('demo-cancel-apt4', 4, 'cancel', DATEADD(hh, 24, GETUTCDATE()));

PRINT 'Verificando tablas creadas:';
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;

PRINT 'Inicialización completada exitosamente';
GO