USE master;
GO

-- Crear base de datos solo si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ReservArteDB')
BEGIN
    CREATE DATABASE ReservArteDB;
    PRINT 'Base de datos ReservArteDB creada correctamente';
END
ELSE
BEGIN
    PRINT 'Base de datos ReservArteDB ya existe';
END
GO

-- Verificar que la base de datos se creó correctamente
SELECT name, database_id, create_date 
FROM sys.databases 
WHERE name = 'ReservArteDB';
GO

-- Usar la base de datos
USE ReservArteDB;
GO

-- ================================
-- CREAR TABLAS (Orden: Master -> Detail)
-- ================================

-- Tabla Users (Base para Customers, Employees, Admins)
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Phone NVARCHAR(20),
    Rol NVARCHAR(50) NOT NULL,
    ProfileImageUrl NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Tabla Customers (Hereda de Users)
CREATE TABLE Customers (
    Id INT PRIMARY KEY,
    BirthDate DATETIME,
    Category NVARCHAR(50) DEFAULT 'regular',
    LoyaltyPoints INT DEFAULT 0,
    IsBlocked BIT DEFAULT 0,
    BlockedReason NVARCHAR(500),
    PreferredContactMethod NVARCHAR(50) DEFAULT 'email',
    MarketingConsent BIT DEFAULT 0,
    FOREIGN KEY (Id) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Tabla Employees (Hereda de Users)
CREATE TABLE Employees (
    Id INT PRIMARY KEY,
    HireDate DATETIME,
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (Id) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Tabla ServiceCategories
CREATE TABLE ServiceCategories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    Color NVARCHAR(20),
    DisplayOrder INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Tabla Services
CREATE TABLE Services (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    DurationMinutes INT NOT NULL CHECK (DurationMinutes > 0),
    BasePrice DECIMAL(10,2) NOT NULL CHECK (BasePrice >= 0),
    CategoryId INT,
    ImageUrl NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    RequiresAllergyTest BIT DEFAULT 0,
    AllergyTestHoursBefore INT DEFAULT 48,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    FOREIGN KEY (CategoryId) REFERENCES ServiceCategories(Id)
);

-- Tabla ServiceVariations
CREATE TABLE ServiceVariations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ServiceId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    PriceModifier DECIMAL(10,2) DEFAULT 0,
    DurationModifier INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE
);

-- Tabla ServicePricings (Precios por nivel de empleado)
CREATE TABLE ServicePricings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ServiceId INT NOT NULL,
    EmployeeLevel NVARCHAR(50) NOT NULL,
    Price DECIMAL(10,2) NOT NULL CHECK (Price >= 0),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE
);

-- Tabla Products
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    Brand NVARCHAR(100),
    Sku NVARCHAR(100),
    Price DECIMAL(10,2) NOT NULL DEFAULT 0,
    Stock INT NOT NULL DEFAULT 0,
    MinStockAlert INT DEFAULT 10,
    ImageUrl NVARCHAR(500),
    CategoryId INT,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME
);

-- Tabla ProductCategories
CREATE TABLE ProductCategories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Tabla ServiceProducts (Relación N:N entre Services y Products)
CREATE TABLE ServiceProducts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ServiceId INT NOT NULL,
    ProductId INT NOT NULL,
    QuantityUsed DECIMAL(10,2),
    Notes NVARCHAR(500),
    FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- Tabla CustomerPaymentMethods
CREATE TABLE CustomerPaymentMethods (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    RedsysToken NVARCHAR(500) NOT NULL,
    RedsysCofTxnid NVARCHAR(100),
    CardLast4 NVARCHAR(4) NOT NULL,
    CardBrand NVARCHAR(50) NOT NULL,
    CardExpiry NVARCHAR(4) NOT NULL,
    IsDefault BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
);

-- Tabla Appointments
CREATE TABLE Appointments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    EmployeeId INT NOT NULL,
    AppointmentDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'pending',
    TotalPrice DECIMAL(10,2) NOT NULL DEFAULT 0,
    DepositAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
    RedsysOrderNumber NVARCHAR(12),
    RedsysPreAuthToken NVARCHAR(500),
    PaymentMethodId INT,
    CancellationReason NVARCHAR(1000),
    CancelledAt DATETIME,
    CancelledById INT,
    CancelledByType NVARCHAR(50),
    Notes NVARCHAR(2000),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    FOREIGN KEY (PaymentMethodId) REFERENCES CustomerPaymentMethods(Id)
);

-- Tabla AppointmentServiceItems
CREATE TABLE AppointmentServiceItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT NOT NULL,
    ServiceId INT NOT NULL,
    ServiceVariationId INT,
    Price DECIMAL(10,2) NOT NULL,
    DurationMinutes INT NOT NULL,
    [Order] INT NOT NULL,
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id) ON DELETE CASCADE,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id),
    FOREIGN KEY (ServiceVariationId) REFERENCES ServiceVariations(Id)
);

-- Tabla Payments
CREATE TABLE Payments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT,
    CustomerId INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    Currency NVARCHAR(3) DEFAULT 'EUR',
    PaymentMethodType NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    RedsysOrderNumber NVARCHAR(12),
    RedsysAuthCode NVARCHAR(50),
    RedsysResponse NVARCHAR(MAX),
    RedsysTransactionType NVARCHAR(10),
    RedsysCardNumber NVARCHAR(20),
    CustomerPaymentMethodId INT,
    ProcessedAt DATETIME,
    RefundedAmount DECIMAL(10,2) DEFAULT 0,
    RefundedAt DATETIME,
    Metadata NVARCHAR(MAX),
    Notes NVARCHAR(1000),
    RegisteredById INT,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id),
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    FOREIGN KEY (CustomerPaymentMethodId) REFERENCES CustomerPaymentMethods(Id)
);

-- Tabla CancellationPolicies
CREATE TABLE CancellationPolicies (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MinHoursBeforeCancel INT NOT NULL,
    PenaltyPercentage INT NOT NULL,
    MaxNoShowsBeforeBlock INT NOT NULL,
    VipMinHoursBeforeCancel INT,
    VipPenaltyPercentage INT,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME
);

-- Tabla CustomerNotes
CREATE TABLE CustomerNotes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    EmployeeId INT NOT NULL,
    Note NVARCHAR(2000) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    EmployeeName NVARCHAR(200),
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
);

-- Tabla CustomerAllergies
CREATE TABLE CustomerAllergies (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    AllergyDescription NVARCHAR(500) NOT NULL,
    Severity NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
);

-- Tabla CustomerConsents
CREATE TABLE CustomerConsents (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    ConsentType NVARCHAR(100) NOT NULL,
    IsGranted BIT NOT NULL,
    GrantedAt DATETIME,
    RevokedAt DATETIME,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
);

-- Tabla EmployeeAvailabilities
CREATE TABLE EmployeeAvailabilities (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    DayOfWeek INT NOT NULL CHECK (DayOfWeek >= 0 AND DayOfWeek <= 6),
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    IsRecurring BIT DEFAULT 1,
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE
);

-- Tabla EmployeeExceptions
CREATE TABLE EmployeeExceptions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    StartDateTime DATETIME NOT NULL,
    EndDateTime DATETIME NOT NULL,
    Reason NVARCHAR(500),
    Type NVARCHAR(50) NOT NULL,
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE
);

-- Tabla EmployeeServices
CREATE TABLE EmployeeServices (
    EmployeeId INT NOT NULL,
    ServiceId INT NOT NULL,
    ProficiencyLevel INT DEFAULT 1,
    PRIMARY KEY (EmployeeId, ServiceId),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE
);

-- Tabla WaitingList
CREATE TABLE WaitingList (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    ServiceId INT NOT NULL,
    PreferredEmployeeId INT,
    PreferredDate DATETIME,
    DateRangeStart DATETIME NOT NULL,
    DateRangeEnd DATETIME NOT NULL,
    Priority INT DEFAULT 1000,
    CreatedAt DATETIME DEFAULT GETDATE(),
    NotifiedAt DATETIME,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id),
    FOREIGN KEY (PreferredEmployeeId) REFERENCES Employees(Id)
);

-- Tabla ServicePhotos
CREATE TABLE ServicePhotos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    S3Key NVARCHAR(500) NOT NULL,
    S3Bucket NVARCHAR(200) NOT NULL,
    UploadedBy INT NOT NULL,
    UploadedAt DATETIME DEFAULT GETDATE(),
    IsPublic BIT DEFAULT 0,
    ExpiresAt DATETIME NOT NULL,
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id) ON DELETE CASCADE,
    FOREIGN KEY (UploadedBy) REFERENCES Employees(Id)
);

-- Tabla ServicePackages
CREATE TABLE ServicePackages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    TotalPrice DECIMAL(10,2) NOT NULL,
    DiscountPercentage DECIMAL(5,2) DEFAULT 0,
    ImageUrl NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME
);

-- Tabla ServicePackageItems
CREATE TABLE ServicePackageItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ServicePackageId INT NOT NULL,
    ServiceId INT NOT NULL,
    [Order] INT NOT NULL,
    FOREIGN KEY (ServicePackageId) REFERENCES ServicePackages(Id) ON DELETE CASCADE,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id)
);

-- Tabla ServicePromotions
CREATE TABLE ServicePromotions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ServiceId INT,
    ServicePackageId INT,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    DiscountPercentage DECIMAL(5,2) NOT NULL,
    DiscountAmount DECIMAL(10,2),
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    IsSeasonalService BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ServiceId) REFERENCES Services(Id),
    FOREIGN KEY (ServicePackageId) REFERENCES ServicePackages(Id)
);

-- Tablas de Sistema de Recordatorios
CREATE TABLE MessageTemplates (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    Subject NVARCHAR(200),
    Body NVARCHAR(MAX) NOT NULL,
    Language NVARCHAR(10) DEFAULT 'es'
);

CREATE TABLE ReminderConfigurations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ReminderOrder INT NOT NULL,
    HoursBeforeAppointment INT NOT NULL,
    Channel NVARCHAR(50) NOT NULL,
    IsActive BIT DEFAULT 1,
    MessageTemplateId UNIQUEIDENTIFIER NOT NULL,
    AllowedSendStartTime TIME,
    AllowedSendEndTime TIME,
    FOREIGN KEY (MessageTemplateId) REFERENCES MessageTemplates(Id)
);

CREATE TABLE ReminderLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AppointmentId INT NOT NULL,
    ReminderConfigurationId UNIQUEIDENTIFIER NOT NULL,
    Channel NVARCHAR(50) NOT NULL,
    SentAt DATETIME NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    ExternalMessageId NVARCHAR(200),
    ErrorMessage NVARCHAR(MAX),
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id) ON DELETE CASCADE,
    FOREIGN KEY (ReminderConfigurationId) REFERENCES ReminderConfigurations(Id)
);

CREATE TABLE ConfirmationTokens (
    Token NVARCHAR(100) PRIMARY KEY,
    AppointmentId INT NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    UsedAt DATETIME,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id) ON DELETE CASCADE
);

-- Tablas de Sistema de Inventario
CREATE TABLE InventoryMovements (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    MovementType NVARCHAR(50) NOT NULL,
    ReferenceId INT,
    Notes NVARCHAR(500),
    CreatedBy INT,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ProductId) REFERENCES Products(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Employees(Id)
);

CREATE TABLE ProductSales (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT,
    AppointmentId INT,
    TotalAmount DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    PaymentMethod NVARCHAR(50),
    Notes NVARCHAR(500),
    SoldBy INT,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id),
    FOREIGN KEY (SoldBy) REFERENCES Employees(Id)
);

CREATE TABLE ProductSaleItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SaleId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    Subtotal DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (SaleId) REFERENCES ProductSales(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

GO

-- ================================
-- INSERTAR DATOS DE EJEMPLO
-- ================================

-- Insertar Categorías de Servicios
INSERT INTO ServiceCategories (Name, Description, Color, DisplayOrder, IsActive)
VALUES 
('Cejas', 'Tratamientos y diseño de cejas', '#FF6B9D', 1, 1),
('Pestañas', 'Extensiones y tratamientos de pestañas', '#C44569', 2, 1),
('Faciales', 'Tratamientos faciales y limpieza', '#A8E6CF', 3, 1),
('Depilación', 'Servicios de depilación', '#FFD93D', 4, 1);

-- Insertar Usuarios Base
INSERT INTO Users (FirstName, LastName, Email, Phone, Rol, CreatedAt)
VALUES 
('María', 'García López', 'maria.garcia@reservarte.com', '600111222', 'employee', GETDATE()),
('Laura', 'Martínez Ruiz', 'laura.martinez@reservarte.com', '600333444', 'employee', GETDATE()),
('Ana', 'López Fernández', 'ana.lopez@email.com', '600555666', 'client', GETDATE()),
('Carmen', 'Rodríguez Gil', 'carmen.rodriguez@email.com', '600777888', 'client', GETDATE()),
('Isabel', 'Sánchez Navarro', 'isabel.sanchez@email.com', '600999000', 'client', GETDATE()),
('Patricia', 'Torres Moreno', 'patricia.torres@email.com', '600111333', 'client', GETDATE()),
('Lucía', 'Ramírez Castro', 'lucia.ramirez@email.com', '600222444', 'client', GETDATE());

-- Insertar Empleados
INSERT INTO Employees (Id, HireDate, IsActive)
VALUES 
(1, '2023-01-15', 1),
(2, '2023-06-01', 1);

-- Insertar Clientes
INSERT INTO Customers (Id, BirthDate, Category, LoyaltyPoints, PreferredContactMethod, MarketingConsent)
VALUES 
(3, '1990-05-15', 'VIP', 150, 'email', 1),
(4, '1995-08-22', 'regular', 50, 'phone', 1),
(5, '1988-03-10', 'regular', 75, 'email', 0),
(6, '1992-11-30', 'premium', 120, 'email', 1),
(7, '1998-07-18', 'regular', 25, 'phone', 1);

-- Insertar Servicios
INSERT INTO Services (Name, Description, DurationMinutes, BasePrice, CategoryId, IsActive, RequiresAllergyTest)
VALUES 
('Diseño de Cejas', 'Diseño y depilación de cejas personalizadas', 30, 15.00, 1, 1, 0),
('Microblading', 'Micropigmentación de cejas técnica pelo a pelo', 120, 250.00, 1, 1, 1),
('Laminado de Cejas', 'Tratamiento para cejas más definidas y pobladas', 45, 35.00, 1, 1, 0),
('Extensiones de Pestañas Clásicas', 'Extensiones de pestañas pelo a pelo', 90, 45.00, 2, 1, 1),
('Extensiones de Pestañas Volumen', 'Extensiones con efecto volumen 3D-6D', 120, 65.00, 2, 1, 1),
('Lifting de Pestañas', 'Tratamiento para rizar y definir pestañas naturales', 60, 40.00, 2, 1, 0),
('Limpieza Facial Profunda', 'Limpieza facial completa con extracción', 60, 50.00, 3, 1, 0),
('Hidratación Facial', 'Tratamiento hidratante con mascarilla', 45, 40.00, 3, 1, 0),
('Depilación Cejas', 'Depilación y diseño básico de cejas', 15, 8.00, 4, 1, 0),
('Depilación Labio Superior', 'Depilación de labio superior', 10, 6.00, 4, 1, 0);

-- Insertar Variaciones de Servicios
INSERT INTO ServiceVariations (ServiceId, Name, PriceModifier, DurationModifier, IsActive)
VALUES 
(4, 'Relleno extensiones clásicas', 15.00, 30, 1),
(5, 'Relleno extensiones volumen', 25.00, 45, 1),
(2, 'Retoque microblading (6 meses)', -100.00, -30, 1);

-- Insertar Precios por Nivel
INSERT INTO ServicePricings (ServiceId, EmployeeLevel, Price)
VALUES 
(2, 'Junior', 200.00),
(2, 'Senior', 250.00),
(2, 'Expert', 300.00),
(4, 'Junior', 40.00),
(4, 'Senior', 45.00),
(4, 'Expert', 55.00);

-- Insertar Categorías de Productos
INSERT INTO ProductCategories (Name, Description, IsActive)
VALUES 
('Adhesivos', 'Adhesivos para extensiones de pestañas', 1),
('Tintes', 'Tintes para cejas y pestañas', 1),
('Cosméticos', 'Productos cosméticos varios', 1),
('Herramientas', 'Herramientas y accesorios', 1);

-- Insertar Productos
INSERT INTO Products (Name, Description, Brand, Sku, Price, Stock, MinStockAlert, CategoryId, IsActive)
VALUES 
('Adhesivo Premium Black', 'Adhesivo negro de secado rápido', 'LashPro', 'ADH-001', 25.00, 15, 5, 1, 1),
('Adhesivo Sensitive', 'Adhesivo para pieles sensibles', 'LashPro', 'ADH-002', 28.00, 10, 5, 1, 1),
('Tinte Cejas Marrón', 'Tinte para cejas tono marrón', 'BrowTint', 'TIN-001', 12.00, 20, 8, 2, 1),
('Tinte Cejas Negro', 'Tinte para cejas tono negro', 'BrowTint', 'TIN-002', 12.00, 18, 8, 2, 1),
('Sérum Crecimiento Pestañas', 'Sérum estimulante crecimiento', 'LashGrow', 'SER-001', 35.00, 12, 5, 3, 1),
('Pinzas Precisión', 'Pinzas de acero inoxidable', 'ProTools', 'TOOL-001', 15.00, 25, 10, 4, 1);

-- Insertar relación Servicios-Productos
INSERT INTO ServiceProducts (ServiceId, ProductId, QuantityUsed, Notes)
VALUES 
(4, 1, 0.5, 'Uso medio por servicio'),
(5, 1, 0.8, 'Uso alto por servicio'),
(1, 3, 1.0, 'Un uso por servicio'),
(2, 3, 1.0, 'Un uso por servicio');

-- Insertar Disponibilidad de Empleados
INSERT INTO EmployeeAvailabilities (EmployeeId, DayOfWeek, StartTime, EndTime, IsRecurring)
VALUES 
-- María: Lunes a Viernes 9:00-18:00
(1, 1, '09:00', '18:00', 1),
(1, 2, '09:00', '18:00', 1),
(1, 3, '09:00', '18:00', 1),
(1, 4, '09:00', '18:00', 1),
(1, 5, '09:00', '18:00', 1),
-- Laura: Martes a Sábado 10:00-19:00
(2, 2, '10:00', '19:00', 1),
(2, 3, '10:00', '19:00', 1),
(2, 4, '10:00', '19:00', 1),
(2, 5, '10:00', '19:00', 1),
(2, 6, '10:00', '14:00', 1);

-- Insertar Servicios por Empleado
INSERT INTO EmployeeServices (EmployeeId, ServiceId, ProficiencyLevel)
VALUES 
(1, 1, 3), (1, 2, 3), (1, 3, 2), (1, 4, 3), (1, 5, 2),
(2, 1, 2), (2, 3, 3), (2, 4, 2), (2, 6, 3), (2, 7, 3), (2, 8, 2);

-- Insertar Métodos de Pago de Clientes
INSERT INTO CustomerPaymentMethods (CustomerId, RedsysToken, CardLast4, CardBrand, CardExpiry, IsDefault)
VALUES 
(3, 'TOKEN_12345_VIP', '4567', 'Visa', '2712', 1),
(4, 'TOKEN_67890_REG', '8901', 'Mastercard', '2611', 1),
(6, 'TOKEN_11111_PREM', '2345', 'Visa', '2710', 1);

-- Insertar Citas
INSERT INTO Appointments (CustomerId, EmployeeId, AppointmentDate, StartTime, EndTime, Status, TotalPrice, DepositAmount, PaymentMethodId, CreatedAt)
VALUES 
-- Citas pasadas completadas
(3, 1, DATEADD(DAY, -30, GETDATE()), '10:00', '11:30', 'completed', 45.00, 0, 1, DATEADD(DAY, -35, GETDATE())),
(4, 2, DATEADD(DAY, -20, GETDATE()), '11:00', '12:00', 'completed', 40.00, 0, 2, DATEADD(DAY, -25, GETDATE())),
(3, 1, DATEADD(DAY, -15, GETDATE()), '15:00', '16:00', 'completed', 35.00, 0, 1, DATEADD(DAY, -20, GETDATE())),
-- Citas próximas confirmadas
(5, 1, DATEADD(DAY, 2, GETDATE()), '10:00', '11:30', 'confirmed', 45.00, 10.00, NULL, DATEADD(DAY, -2, GETDATE())),
(6, 2, DATEADD(DAY, 3, GETDATE()), '11:00', '12:00', 'confirmed', 50.00, 15.00, 3, DATEADD(DAY, -1, GETDATE())),
(7, 1, DATEADD(DAY, 5, GETDATE()), '16:00', '17:30', 'pending', 65.00, 0, NULL, GETDATE()),
-- Cita para hoy
(4, 2, CAST(GETDATE() AS DATE), '14:00', '15:00', 'confirmed', 40.00, 10.00, 2, DATEADD(DAY, -3, GETDATE()));

-- Insertar Items de Servicio para Citas
INSERT INTO AppointmentServiceItems (AppointmentId, ServiceId, ServiceVariationId, Price, DurationMinutes, [Order])
VALUES 
(1, 4, NULL, 45.00, 90, 1),
(2, 6, NULL, 40.00, 60, 1),
(3, 3, NULL, 35.00, 45, 1),
(4, 4, NULL, 45.00, 90, 1),
(5, 7, NULL, 50.00, 60, 1),
(6, 5, NULL, 65.00, 120, 1),
(7, 6, NULL, 40.00, 60, 1);

-- Insertar Pagos
INSERT INTO Payments (AppointmentId, CustomerId, Amount, Currency, PaymentMethodType, Status, CustomerPaymentMethodId, ProcessedAt, CreatedAt)
VALUES 
(1, 3, 45.00, 'EUR', 'card', 'captured', 1, DATEADD(DAY, -30, GETDATE()), DATEADD(DAY, -30, GETDATE())),
(2, 4, 40.00, 'EUR', 'card', 'captured', 2, DATEADD(DAY, -20, GETDATE()), DATEADD(DAY, -20, GETDATE())),
(3, 3, 35.00, 'EUR', 'card', 'captured', 1, DATEADD(DAY, -15, GETDATE()), DATEADD(DAY, -15, GETDATE())),
(4, 5, 10.00, 'EUR', 'card', 'captured', NULL, DATEADD(DAY, -2, GETDATE()), DATEADD(DAY, -2, GETDATE())),
(5, 6, 15.00, 'EUR', 'card', 'captured', 3, DATEADD(DAY, -1, GETDATE()), DATEADD(DAY, -1, GETDATE()));

-- Insertar Política de Cancelación
INSERT INTO CancellationPolicies (MinHoursBeforeCancel, PenaltyPercentage, MaxNoShowsBeforeBlock, VipMinHoursBeforeCancel, VipPenaltyPercentage, IsActive)
VALUES 
(24, 50, 3, 12, 25, 1);

-- Insertar Notas de Clientes
INSERT INTO CustomerNotes (CustomerId, EmployeeId, Note)
VALUES 
(3, 1, 'Cliente VIP. Prefiere estilo natural para cejas.'),
(4, 2, 'Piel sensible, usar productos hipoalergénicos.'),
(6, 1, 'Le gusta el estilo dramático en pestañas.');

-- Insertar Alergias
INSERT INTO CustomerAllergies (CustomerId, AllergyDescription, Severity)
VALUES 
(4, 'Alergia al níquel', 'moderate'),
(5, 'Sensibilidad a fragancias', 'mild');

-- Insertar Consentimientos
INSERT INTO CustomerConsents (CustomerId, ConsentType, IsGranted, GrantedAt)
VALUES 
(3, 'marketing', 1, GETDATE()),
(3, 'data_processing', 1, GETDATE()),
(4, 'marketing', 1, GETDATE()),
(5, 'data_processing', 1, GETDATE());

-- Insertar Paquetes de Servicios
INSERT INTO ServicePackages (Name, Description, TotalPrice, DiscountPercentage, IsActive)
VALUES 
('Paquete Cejas Perfect', 'Diseño + Laminado + Tinte', 55.00, 10, 1),
('Paquete Look Completo', 'Extensiones pestañas + Diseño cejas', 85.00, 15, 1);

-- Insertar Items de Paquetes
INSERT INTO ServicePackageItems (ServicePackageId, ServiceId, [Order])
VALUES 
(1, 1, 1), (1, 3, 2),
(2, 4, 1), (2, 1, 2);

-- Insertar Promociones
INSERT INTO ServicePromotions (ServiceId, Name, Description, DiscountPercentage, StartDate, EndDate, IsSeasonalService, IsActive)
VALUES 
(2, 'Promo Microblading Primavera', 'Descuento especial primavera', 15.00, DATEADD(MONTH, -1, GETDATE()), DATEADD(MONTH, 2, GETDATE()), 1, 1),
(7, 'Facial Verano', 'Prepara tu piel para el verano', 20.00, GETDATE(), DATEADD(MONTH, 3, GETDATE()), 1, 1);

-- Insertar Templates de Mensajes
INSERT INTO MessageTemplates (Name, Type, Subject, Body, Language)
VALUES 
('Recordatorio 24h', 'reminder', 'Recordatorio de tu cita en ReservArte', 
 'Hola {CustomerName}, te recordamos que tienes una cita mañana a las {StartTime} con {EmployeeName} para {ServiceName}. ¡Te esperamos!', 'es'),
('Confirmación Cita', 'confirmation', 'Cita confirmada en ReservArte',
 'Hola {CustomerName}, tu cita para {ServiceName} el {AppointmentDate} a las {StartTime} ha sido confirmada. ¡Nos vemos pronto!', 'es');

GO

-- ================================
-- ÍNDICES PARA OPTIMIZACIÓN
-- ================================

CREATE INDEX idx_Appointments_Customer ON Appointments(CustomerId);
CREATE INDEX idx_Appointments_Employee ON Appointments(EmployeeId);
CREATE INDEX idx_Appointments_Date ON Appointments(AppointmentDate);
CREATE INDEX idx_Appointments_Status ON Appointments(Status);
CREATE INDEX idx_Payments_Customer ON Payments(CustomerId);
CREATE INDEX idx_Payments_Status ON Payments(Status);
CREATE INDEX idx_Services_Category ON Services(CategoryId);
CREATE INDEX idx_Services_Active ON Services(IsActive);

GO

PRINT '========================================';
PRINT 'Base de datos ReservArteDB creada exitosamente';
PRINT 'Datos de ejemplo insertados';
PRINT '========================================';
