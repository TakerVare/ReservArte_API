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
-- TABLA CUSTOMERS (Id = User.Id para unificar identificador)
-- ============================================
CREATE TABLE Customers (
    Id INT NOT NULL PRIMARY KEY,
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
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (Id) REFERENCES Users(Id) ON DELETE CASCADE
);

-- ============================================
-- TABLA EMPLOYEES (Id = User.Id para unificar identificador)
-- ============================================
CREATE TABLE Employees (
    Id INT NOT NULL PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Phone NVARCHAR(20) NULL,
    Rol NVARCHAR(50) NOT NULL DEFAULT 'employee',
    ProfileImageUrl NVARCHAR(500) NULL,
    HireDate DATE NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (Id) REFERENCES Users(Id) ON DELETE CASCADE
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
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ServiceId) REFERENCES Services(Id)
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
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id),
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
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (ServicePackageId) REFERENCES ServicePackages(Id),
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
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
);

CREATE TABLE EmployeeExceptions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    EmployeeId INT NOT NULL,
    StartDateTime DATETIME2 NOT NULL,
    EndDateTime DATETIME2 NOT NULL,
    Reason NVARCHAR(500) NULL,
    Type NVARCHAR(50) NOT NULL CHECK (Type IN ('vacation', 'sick_leave', 'personal', 'training', 'other')),
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
);

CREATE TABLE EmployeeServices (
    EmployeeId INT NOT NULL,
    ServiceId INT NOT NULL,
    ProficiencyLevel INT NOT NULL DEFAULT 1 CHECK (ProficiencyLevel BETWEEN 1 AND 5),
    IsActive BIT NOT NULL DEFAULT 1,
    PRIMARY KEY (EmployeeId, ServiceId),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    FOREIGN KEY (ServiceId) REFERENCES Services(Id)
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
    IsActive BIT NOT NULL DEFAULT 1,
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
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
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
    IsActive BIT NOT NULL DEFAULT 1,
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
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
);

CREATE TABLE CustomerAllergies (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    AllergyDescription NVARCHAR(500) NOT NULL,
    Severity NVARCHAR(50) NOT NULL CHECK (Severity IN ('Low', 'Medium', 'High')),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);

CREATE TABLE CustomerConsents (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    ConsentType NVARCHAR(100) NOT NULL,
    IsGranted BIT NOT NULL,
    GrantedAt DATETIME2 NULL,
    RevokedAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
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
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id),
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
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    NotifiedAt DATETIME2 NULL,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
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
    Language NVARCHAR(10) NOT NULL DEFAULT 'es',
    IsActive BIT NOT NULL DEFAULT 1
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
-- DATOS INICIALES (para pruebas)
-- ============================================

-- USUARIOS (para autenticación). Id 1=admin, 2-3=empleados, 4-6=clientes.
INSERT INTO Users (FirstName, LastName, Email, Password, Rol, Phone) VALUES
('Guillermo', 'Admin', 'guille@svalero.com', 'Admin1234!', 'admin', '+34600000001'),
('María', 'García', 'maria.garcia@reservarte.com', 'Maria123!', 'employee', '+34600000002'),
('Laura', 'Martínez', 'laura.martinez@reservarte.com', 'Laura123!', 'employee', '+34600000003'),
('Ana', 'López', 'ana.lopez@email.com', 'Cliente123!', 'client', '+34600000004'),
('Carmen', 'Rodríguez', 'carmen.rodriguez@email.com', 'Cliente123!', 'client', '+34600000005'),
('Isabel', 'Sánchez', 'isabel.sanchez@email.com', 'Cliente123!', 'client', '+34600000006');

-- EMPLOYEES: Id = User.Id (2=María, 3=Laura)
INSERT INTO Employees (Id, FirstName, LastName, Email, Phone, Rol, HireDate, IsActive) VALUES
(2, 'María', 'García', 'maria.garcia@reservarte.com', '+34600000002', 'employee', '2023-01-15', 1),
(3, 'Laura', 'Martínez', 'laura.martinez@reservarte.com', '+34600000003', 'employee', '2023-03-01', 1);

-- CUSTOMERS: Id = User.Id (4=Ana, 5=Carmen, 6=Isabel)
INSERT INTO Customers (Id, FirstName, LastName, Email, Phone, Rol, Category, LoyaltyPoints, PreferredContactMethod, MarketingConsent) VALUES
(4, 'Ana', 'López', 'ana.lopez@email.com', '+34600000004', 'client', 'regular', 0, 'email', 1),
(5, 'Carmen', 'Rodríguez', 'carmen.rodriguez@email.com', '+34600000005', 'client', 'vip', 150, 'whatsapp', 1),
(6, 'Isabel', 'Sánchez', 'isabel.sanchez@email.com', '+34600000006', 'client', 'regular', 50, 'email', 0);

-- CONSENTIMIENTOS: SavedCards concedido para clientes de prueba (permite guardar tarjetas)
INSERT INTO CustomerConsents (CustomerId, ConsentType, IsGranted, GrantedAt) VALUES
(4, 'SavedCards', 1, GETUTCDATE()),
(5, 'SavedCards', 1, GETUTCDATE()),
(6, 'SavedCards', 1, GETUTCDATE());

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

-- DISPONIBILIDAD EMPLEADOS (EmployeeId 2=María, 3=Laura)
INSERT INTO EmployeeAvailabilities (EmployeeId, DayOfWeek, StartTime, EndTime, IsRecurring) VALUES
(2, 1, '09:00', '18:00', 1), -- María Lunes
(2, 2, '09:00', '18:00', 1), -- María Martes
(2, 3, '09:00', '18:00', 1), -- María Miércoles
(2, 4, '09:00', '18:00', 1), -- María Jueves
(2, 5, '09:00', '14:00', 1), -- María Viernes
(3, 1, '10:00', '19:00', 1), -- Laura Lunes
(3, 2, '10:00', '19:00', 1), -- Laura Martes
(3, 3, '10:00', '19:00', 1), -- Laura Miércoles
(3, 4, '10:00', '19:00', 1), -- Laura Jueves
(3, 5, '10:00', '15:00', 1); -- Laura Viernes

-- POLÍTICA DE CANCELACIÓN
INSERT INTO CancellationPolicies (MinHoursBeforeCancel, PenaltyPercentage, MaxNoShowsBeforeBlock, VipMinHoursBeforeCancel, VipPenaltyPercentage, IsActive) VALUES
(24, 50, 3, 12, 25, 1);

-- CITAS (Appointments) - datos de ejemplo
-- Clientes: 4=Ana López, 5=Carmen Rodríguez, 6=Isabel Sánchez
-- Empleadas: 2=María García, 3=Laura Martínez
INSERT INTO Appointments (CustomerId, EmployeeId, AppointmentDate, StartTime, EndTime, Status, TotalPrice, DepositAmount, Notes) VALUES
(4, 2, '2026-03-10', '10:00', '10:30', 'confirmed', 25.00, 0, N'Corte mujer - primera visita'),
(5, 2, '2026-03-10', '11:00', '13:00', 'confirmed', 65.00, 20.00, N'Tinte completo'),
(6, 3, '2026-03-10', '10:00', '10:20', 'pending', 15.00, 0, NULL),
(4, 3, '2026-03-11', '12:00', '12:45', 'pending', 35.00, 0, N'Hidratación profunda'),
(5, 2, '2026-03-11', '09:30', '10:00', 'pending', 25.00, 0, NULL),
(6, 3, '2026-03-12', '16:00', '16:30', 'completed', 25.00, 0, N'Corte realizado'),
(4, 2, '2026-03-12', '11:00', '13:30', 'completed', 85.00, 30.00, N'Mechas - cliente satisfecha'),
(6, 2, '2026-03-13', '12:00', '12:20', 'cancelled', 15.00, 0, NULL);

UPDATE Appointments SET CancellationReason = N'Cliente no pudo asistir', CancelledAt = GETUTCDATE(), CancelledByType = 'customer', UpdatedAt = GETUTCDATE() WHERE Id = 8;

-- Detalle de servicios por cita (AppointmentServiceItems). ServiceVariationId NULL = variación estándar.
INSERT INTO AppointmentServiceItems (AppointmentId, ServiceId, ServiceVariationId, Price, DurationMinutes, [Order]) VALUES
(1, 1, NULL, 25.00, 30, 1),
(2, 3, NULL, 65.00, 120, 1),
(3, 2, NULL, 15.00, 20, 1),
(4, 6, NULL, 35.00, 45, 1),
(5, 1, NULL, 25.00, 30, 1),
(6, 1, NULL, 25.00, 30, 1),
(7, 4, NULL, 85.00, 150, 1),
(8, 2, NULL, 15.00, 20, 1);

-- PAGOS (Payments) - datos de ejemplo (CustomerId 4=Ana, 5=Carmen, 6=Isabel)
INSERT INTO Payments (AppointmentId, CustomerId, Amount, Currency, PaymentMethodType, Status, RedsysOrderNumber, ProcessedAt) VALUES
(1, 4, 25.00, 'EUR', 'card', 'captured', 'DEMO-PAY-001', GETUTCDATE()),
(2, 5, 65.00, 'EUR', 'card', 'captured', 'DEMO-PAY-002', GETUTCDATE()),
(6, 6, 25.00, 'EUR', 'cash', 'captured', 'DEMO-PAY-003', GETUTCDATE()),
(7, 4, 85.00, 'EUR', 'card', 'captured', 'DEMO-PAY-004', GETUTCDATE());

PRINT 'Verificando tablas creadas:';
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;

PRINT 'Inicialización completada exitosamente';
GO