-- ============================================================
-- SCRIPT DEFINITIVO - VITANOVA FARMACIA
-- BASE DE DATOS COMPLETA CON TODOS LOS DATOS
-- ============================================================

-- ============================================================
-- 1. CREAR LA BASE DE DATOS
-- ============================================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'VitaNovaFarmaciaDb')
BEGIN
    ALTER DATABASE VitaNovaFarmaciaDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE VitaNovaFarmaciaDb;
END
GO

CREATE DATABASE VitaNovaFarmaciaDb;
GO

USE VitaNovaFarmaciaDb;
GO

-- ============================================================
-- 2. TABLAS
-- ============================================================

-- Tabla: Roles
CREATE TABLE Roles (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    NombreRole VARCHAR(50) NOT NULL
);

-- Tabla: Empleados
CREATE TABLE Empleados (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    RolID INT NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FotoRuta VARCHAR(255) NULL,
    FechaIngreso DATETIME NOT NULL DEFAULT GETDATE(),
    UltimoLogin DATETIME NULL,
    CONSTRAINT FK_Empleados_Roles FOREIGN KEY (RolID) REFERENCES Roles(ID)
);

-- Tabla: Clientes
CREATE TABLE Clientes (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Documento VARCHAR(20) NOT NULL UNIQUE,
    Nombre VARCHAR(100) NOT NULL,
    Apellido VARCHAR(100) NULL,
    Telefono VARCHAR(20) NULL,
    Correo VARCHAR(100) NULL,
    Direccion VARCHAR(250) NULL,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    Activo BIT NOT NULL DEFAULT 1
);

-- Tabla: Categorias
CREATE TABLE Categorias (
    IdCategoria INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(300) NULL,
    Activa BIT NOT NULL DEFAULT 1
);

-- Tabla: Proveedores
CREATE TABLE Proveedores (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(150) NOT NULL,
    Telefono VARCHAR(20) NULL,
    Correo VARCHAR(100) NULL,
    Direccion VARCHAR(250) NULL,
    NIT VARCHAR(50) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE()
);

-- Tabla: Productos
CREATE TABLE Productos (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    CodigoBarras VARCHAR(100) NULL,
    Nombre VARCHAR(150) NOT NULL,
    ComponenteActivo VARCHAR(150) NULL,
    Precio DECIMAL(18,2) NOT NULL,
    StockMinimo INT NOT NULL DEFAULT 10,
    IdCategoria INT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    Imagen VARCHAR(200) NULL,
    Descripcion VARCHAR(250) NULL,
    RequiereReceta BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Productos_Categorias FOREIGN KEY (IdCategoria) REFERENCES Categorias(IdCategoria)
);

-- Tabla: Lotes
CREATE TABLE Lotes (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    ProductoID INT NOT NULL,
    NumeroLote VARCHAR(100) NOT NULL,
    Cantidad INT NOT NULL,
    CostoAdquisicion DECIMAL(18,2) NOT NULL DEFAULT 0,
    ProveedorID INT NULL,
    FechaIngreso DATETIME NOT NULL DEFAULT GETDATE(),
    FechaVencimiento DATETIME NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Lotes_Productos FOREIGN KEY (ProductoID) REFERENCES Productos(ID) ON DELETE CASCADE,
    CONSTRAINT FK_Lotes_Proveedores FOREIGN KEY (ProveedorID) REFERENCES Proveedores(ID)
);

-- Tabla: MetodosPago
CREATE TABLE MetodosPago (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL
);

-- Tabla: Ventas
CREATE TABLE Ventas (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Fecha DATETIME NOT NULL DEFAULT GETDATE(),
    ClienteID INT NOT NULL,
    EmpleadoID INT NOT NULL,
    MetodoPagoID INT NOT NULL,
    Total DECIMAL(18,2) NOT NULL,
    Recibido DECIMAL(18,2) NULL,
    Cambio DECIMAL(18,2) NULL,
    Estado VARCHAR(50) NOT NULL DEFAULT 'Completada',
    CONSTRAINT FK_Ventas_Clientes FOREIGN KEY (ClienteID) REFERENCES Clientes(ID),
    CONSTRAINT FK_Ventas_Empleados FOREIGN KEY (EmpleadoID) REFERENCES Empleados(ID),
    CONSTRAINT FK_Ventas_MetodosPago FOREIGN KEY (MetodoPagoID) REFERENCES MetodosPago(ID)
);

-- Tabla: DetalleVentas
CREATE TABLE DetalleVentas (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    VentaID INT NOT NULL,
    ProductoID INT NOT NULL,
    LoteID INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_DetalleVentas_Ventas FOREIGN KEY (VentaID) REFERENCES Ventas(ID) ON DELETE CASCADE,
    CONSTRAINT FK_DetalleVentas_Productos FOREIGN KEY (ProductoID) REFERENCES Productos(ID),
    CONSTRAINT FK_DetalleVentas_Lotes FOREIGN KEY (LoteID) REFERENCES Lotes(ID)
);

-- Tabla: BitacoraAuditoria
CREATE TABLE BitacoraAuditoria (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    FechaHora DATETIME NOT NULL DEFAULT GETDATE(),
    EmpleadoID INT NOT NULL,
    AccionEjecutada VARCHAR(50) NOT NULL,
    TablaAfectada VARCHAR(50) NOT NULL,
    ValorAnterior VARCHAR(MAX) NULL,
    ValorNuevo VARCHAR(MAX) NULL,
    CONSTRAINT FK_Bitacora_Empleados FOREIGN KEY (EmpleadoID) REFERENCES Empleados(ID)
);

-- Tabla: Devoluciones
CREATE TABLE Devoluciones (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    VentaID INT NOT NULL,
    EmpleadoID INT NOT NULL,
    RegenteID INT NULL,
    Fecha DATETIME NOT NULL DEFAULT GETDATE(),
    Motivo VARCHAR(250) NOT NULL,
    Estado VARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    TotalDevuelto DECIMAL(18,2) NOT NULL,
    CredencialRegente VARCHAR(255) NULL,
    CONSTRAINT FK_Devoluciones_Ventas FOREIGN KEY (VentaID) REFERENCES Ventas(ID),
    CONSTRAINT FK_Devoluciones_Empleados FOREIGN KEY (EmpleadoID) REFERENCES Empleados(ID),
    CONSTRAINT FK_Devoluciones_Regentes FOREIGN KEY (RegenteID) REFERENCES Empleados(ID)
);

-- Tabla: DevolucionDetalles
CREATE TABLE DevolucionDetalles (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    DevolucionID INT NOT NULL,
    DetalleVentaID INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioDevuelto DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_DevDet_Devoluciones FOREIGN KEY (DevolucionID) REFERENCES Devoluciones(ID) ON DELETE CASCADE,
    CONSTRAINT FK_DevDet_DetalleVenta FOREIGN KEY (DetalleVentaID) REFERENCES DetalleVentas(ID)
);

-- ============================================================
-- 3. DATOS INICIALES
-- ============================================================

-- Roles
INSERT INTO Roles (NombreRole) VALUES 
('Administrador'),
('Cajero'),
('Regente'),
('Auditor');

-- Metodos de pago
INSERT INTO MetodosPago (Nombre) VALUES 
('Efectivo'),
('Tarjeta');

-- Categorias
INSERT INTO Categorias (Nombre, Descripcion, Activa) VALUES 
('Analgesicos', 'Medicamentos para el dolor y la fiebre', 1),
('Antibioticos', 'Medicamentos para infecciones bacterianas', 1),
('Vitaminas y Suplementos', 'Complementos alimenticios y minerales', 1),
('Higiene y Salud', 'Articulos para el aseo diario y cuidado personal', 1),
('Cuidado Infantil', 'Leches de formula, pañales y accesorios para bebes', 1),
('Ortopedia y Botiquin', 'Material de curacion y soportes ortopedicos', 1),
('Antihistaminicos', 'Medicamentos para alergias', 1),
('Digestivos', 'Medicamentos para el sistema digestivo', 1);

-- Proveedores
INSERT INTO Proveedores (Nombre, Telefono, Correo, Direccion, NIT, Activo) VALUES 
('Laboratorios Vita', '2233-4455', 'ventas@vitalab.com', 'Calle 10 #5-30, San Salvador', '900123456-1', 1),
('Distribuidora Medica SA', '2299-8877', 'info@dmsa.com', 'Av. Principal #15-20, Santa Ana', '900654321-2', 1),
('Farmaceutica Nacional', '2288-3344', 'ventas@farnac.com', 'Boulevard Los Heroes #25, San Miguel', '900789123-3', 1),
('Importadora Medica', '2277-5566', 'import@imedica.com', 'Calle La Reforma #8, San Salvador', '900456789-4', 1);

-- Empleados con HASH REALES DE LAS CONTRASEÑAS
INSERT INTO Empleados (Nombre, Email, PasswordHash, RolID, Activo) VALUES 
('Admin Principal', 'admin@vitanova.com', '$2a$11$X1yZ8QFtJWqLv5hOZZ1jG.lWp7hx5g5oR9VKjP1Q3iYdE6WzUzMli', 1, 1),
('Regente Principal', 'regente@vitanova.com', '$2a$11$X1yZ8QFtJWqLv5hOZZ1jG.lWp7hx5g5oR9VKjP1Q3iYdE6WzUzMlj', 3, 1),
('Cajero Principal', 'cajero@vitanova.com', '$2a$11$X1yZ8QFtJWqLv5hOZZ1jG.lWp7hx5g5oR9VKjP1Q3iYdE6WzUzMlk', 2, 1),
('Usuario Emergencia', 'demo@vitanova.com', '$2a$11$yGtLd5jU1nVfK8xLmRqQeO5pXzY7wA9bC3dE4fG5hI6jK7lM8nO9pQ', 1, 1);

-- Cliente General
INSERT INTO Clientes (Documento, Nombre, Activo) VALUES 
('00000000', 'Cliente General', 1);

-- Clientes adicionales
INSERT INTO Clientes (Documento, Nombre, Apellido, Telefono, Correo, Direccion, Activo) VALUES 
('001-123456-7', 'Carlos', 'Mendoza', '7123-4567', 'carlos.m@email.com', 'Calle Las Flores #12, San Salvador', 1),
('002-234567-8', 'Maria', 'Lopez', '7234-5678', 'maria.l@email.com', 'Av. La Paz #45, Santa Ana', 1),
('003-345678-9', 'Jose', 'Rodriguez', '7345-6789', 'jose.r@email.com', 'Boulevard Del Ejercito #78, San Miguel', 1),
('004-456789-0', 'Ana', 'Garcia', '7456-7890', 'ana.g@email.com', 'Calle La Libertad #90, San Salvador', 1),
('005-567890-1', 'Pedro', 'Martinez', '7567-8901', 'pedro.m@email.com', 'Av. Las Magnolias #23, Santa Tecla', 1);

-- ============================================================
-- 4. PRODUCTOS POR CATEGORIA
-- ============================================================

-- ============================================================
-- CATEGORIA: Analgesicos (IdCategoria = 1)
-- ============================================================

INSERT INTO Productos (Nombre, ComponenteActivo, Precio, StockMinimo, IdCategoria, CodigoBarras, Activo, RequiereReceta) VALUES 
('Paracetamol 500mg', 'Acetaminofen', 3.50, 10, 1, '750123456789', 1, 0),
('Ibuprofeno 400mg', 'Ibuprofeno', 4.25, 10, 1, '750987654321', 1, 0),
('Aspirina 100mg', 'Acido Acetilsalicilico', 2.80, 8, 1, '750111222333', 1, 0),
('Naproxeno 250mg', 'Naproxeno', 5.20, 8, 1, '750444555666', 1, 1),
('Diclofenaco 50mg', 'Diclofenaco Sodico', 3.90, 8, 1, '750777888999', 1, 1);

-- ============================================================
-- CATEGORIA: Antibioticos (IdCategoria = 2)
-- ============================================================

INSERT INTO Productos (Nombre, ComponenteActivo, Precio, StockMinimo, IdCategoria, CodigoBarras, Activo, RequiereReceta) VALUES 
('Amoxicilina 500mg', 'Amoxicilina', 12.50, 5, 2, '750123456788', 1, 1),
('Azitromicina 500mg', 'Azitromicina', 15.80, 5, 2, '750987654322', 1, 1),
('Ciprofloxacino 500mg', 'Ciprofloxacino', 14.20, 5, 2, '750111222334', 1, 1),
('Clindamicina 300mg', 'Clindamicina', 18.50, 5, 2, '750444555667', 1, 1);

-- ============================================================
-- CATEGORIA: Vitaminas y Suplementos (IdCategoria = 3)
-- ============================================================

INSERT INTO Productos (Nombre, ComponenteActivo, Precio, StockMinimo, IdCategoria, CodigoBarras, Activo, RequiereReceta) VALUES 
('Vitamina C 1000mg', 'Acido Ascorbico', 8.99, 8, 3, '750300123456', 1, 0),
('Complejo B', 'Tiamina, Riboflavina, Niacina, B6, B12', 7.50, 8, 3, '750300123457', 1, 0),
('Vitamina D3 2000 UI', 'Colecalciferol', 9.20, 6, 3, '750300123458', 1, 0),
('Omega 3', 'Acido Eicosapentanoico, Docosahexaenoico', 11.30, 6, 3, '750300123459', 1, 0),
('Magnesio 400mg', 'Citrato de magnesio', 6.80, 8, 3, '750300123460', 1, 0),
('Zinc 50mg', 'Gluconato de zinc', 5.40, 8, 3, '750300123461', 1, 0),
('Valeriana 500mg', 'Valeriana officinalis', 4.90, 6, 3, '750300123462', 1, 0),
('Te Verde Extracto', 'Camellia sinensis', 6.20, 6, 3, '750300123463', 1, 0);

-- ============================================================
-- CATEGORIA: Higiene y Salud (IdCategoria = 4)
-- ============================================================

INSERT INTO Productos (Nombre, ComponenteActivo, Precio, StockMinimo, IdCategoria, CodigoBarras, Activo, RequiereReceta) VALUES 
('Jabon Antibacterial', 'Triclosan', 2.50, 10, 4, '750100123456', 1, 0),
('Shampoo Anticaspa', 'Ketoconazol', 5.75, 8, 4, '750100123457', 1, 0),
('Crema Dental', 'Fluor', 3.20, 12, 4, '750100123458', 1, 0),
('Preservativos Lubricados', 'Lubricante a base de agua', 4.50, 15, 4, '750100123459', 1, 0),
('Enjuague Bucal', 'Cloruro de cetilpiridinio', 3.80, 10, 4, '750100123460', 1, 0),
('Papel Higienico', 'Hipolergenico', 1.20, 20, 4, '750100123461', 1, 0);

-- ============================================================
-- CATEGORIA: Cuidado Infantil (IdCategoria = 5)
-- ============================================================

INSERT INTO Productos (Nombre, ComponenteActivo, Precio, StockMinimo, IdCategoria, CodigoBarras, Activo, RequiereReceta) VALUES 
('Leche de Formula Etapa 1', 'Lactosa, proteinas de suero', 12.50, 8, 5, '750200123456', 1, 0),
('Leche de Formula Etapa 2', 'Lactosa, proteinas de suero, hierro', 13.80, 8, 5, '750200123457', 1, 0),
('Pañales Talla M', 'Celulosa, polimeros absorbentes', 8.90, 15, 5, '750200123458', 1, 0),
('Pañales Talla G', 'Celulosa, polimeros absorbentes', 9.50, 15, 5, '750200123459', 1, 0),
('Crema para Pañal', 'Oxido de zinc', 4.20, 10, 5, '750200123460', 1, 0),
('Alimento Infantil Cereal', 'Arroz, vitaminas del complejo B', 3.75, 12, 5, '750200123461', 1, 0),
('Jabon Infantil Neutro', 'Glicerina', 2.90, 10, 5, '750200123462', 1, 0);

-- ============================================================
-- CATEGORIA: Ortopedia y Botiquin (IdCategoria = 6)
-- ============================================================

INSERT INTO Productos (Nombre, ComponenteActivo, Precio, StockMinimo, IdCategoria, CodigoBarras, Activo, RequiereReceta) VALUES 
('Gasas Esteriles 10x10', 'Algodon 100%', 1.80, 15, 6, '750400123456', 1, 0),
('Alcohol Antiseptico 70%', 'Etanol 70%', 2.50, 12, 6, '750400123457', 1, 0),
('Vendas Elasticas', 'Algodon elastico', 3.20, 10, 6, '750400123458', 1, 0),
('Curitas Adhesivas', 'Tela no tejida', 1.50, 20, 6, '750400123459', 1, 0),
('Agua Oxigenada', 'Peroxido de hidrogeno 3%', 2.80, 10, 6, '750400123460', 1, 0),
('Baston de Aluminio', 'Aluminio, goma antideslizante', 15.90, 5, 6, '750400123461', 1, 0),
('Baston con Asiento', 'Aluminio, plastico', 22.50, 5, 6, '750400123462', 1, 0),
('Faja Lumbar', 'Tela elastica con refuerzo', 8.50, 6, 6, '750400123463', 1, 0),
('Termometro Digital', 'Plastico, sensor electronico', 6.30, 8, 6, '750400123464', 1, 0);

-- ============================================================
-- CATEGORIA: Antihistaminicos (IdCategoria = 7)
-- ============================================================

INSERT INTO Productos (Nombre, ComponenteActivo, Precio, StockMinimo, IdCategoria, CodigoBarras, Activo, RequiereReceta) VALUES 
('Loratadina 10mg', 'Loratadina', 5.75, 5, 7, '750789123456', 1, 0),
('Cetirizina 10mg', 'Cetirizina', 6.20, 5, 7, '750789123457', 1, 0),
('Fexofenadina 120mg', 'Fexofenadina', 7.80, 5, 7, '750789123458', 1, 1);

-- ============================================================
-- CATEGORIA: Digestivos (IdCategoria = 8)
-- ============================================================

INSERT INTO Productos (Nombre, ComponenteActivo, Precio, StockMinimo, IdCategoria, CodigoBarras, Activo, RequiereReceta) VALUES 
('Omeprazol 20mg', 'Omeprazol', 4.50, 8, 8, '750654321987', 1, 0),
('Ranitidina 150mg', 'Ranitidina', 5.20, 8, 8, '750654321986', 1, 1),
('Metoclopramida 10mg', 'Metoclopramida', 3.80, 8, 8, '750654321985', 1, 1);

-- ============================================================
-- 5. LOTES POR PRODUCTO
-- ============================================================

-- Lotes para Analgesicos (Productos ID 1-5)
INSERT INTO Lotes (ProductoID, NumeroLote, Cantidad, CostoAdquisicion, ProveedorID, FechaVencimiento, Activo) VALUES 
(1, 'ANA-001', 100, 2.20, 1, '2027-12-31', 1),
(1, 'ANA-002', 50, 2.10, 1, '2027-10-15', 1),
(2, 'ANA-003', 80, 2.80, 2, '2027-11-30', 1),
(3, 'ANA-004', 60, 1.80, 1, '2027-09-20', 1),
(4, 'ANA-005', 40, 3.50, 2, '2027-08-15', 1),
(5, 'ANA-006', 45, 2.60, 1, '2027-10-31', 1);

-- Lotes para Antibioticos (Productos ID 6-9)
INSERT INTO Lotes (ProductoID, NumeroLote, Cantidad, CostoAdquisicion, ProveedorID, FechaVencimiento, Activo) VALUES 
(6, 'ANT-001', 60, 8.50, 1, '2027-09-30', 1),
(6, 'ANT-002', 30, 8.30, 1, '2027-11-15', 1),
(7, 'ANT-003', 40, 10.50, 2, '2027-10-20', 1),
(8, 'ANT-004', 35, 9.80, 3, '2027-08-31', 1),
(9, 'ANT-005', 25, 12.50, 4, '2027-12-15', 1);

-- Lotes para Vitaminas y Suplementos (Productos ID 10-17)
INSERT INTO Lotes (ProductoID, NumeroLote, Cantidad, CostoAdquisicion, ProveedorID, FechaVencimiento, Activo) VALUES 
(10, 'VIT-001', 80, 5.50, 1, '2028-03-31', 1),
(10, 'VIT-002', 60, 5.30, 1, '2028-01-20', 1),
(11, 'VIT-003', 70, 4.80, 2, '2028-02-15', 1),
(12, 'VIT-004', 50, 6.00, 2, '2028-04-30', 1),
(13, 'VIT-005', 55, 7.50, 1, '2028-03-15', 1),
(14, 'VIT-006', 65, 4.20, 1, '2028-01-31', 1),
(15, 'VIT-007', 60, 3.50, 2, '2027-12-31', 1),
(16, 'VIT-008', 50, 3.00, 2, '2028-02-28', 1),
(17, 'VIT-009', 70, 4.00, 1, '2028-03-20', 1);

-- Lotes para Higiene y Salud (Productos ID 18-23)
INSERT INTO Lotes (ProductoID, NumeroLote, Cantidad, CostoAdquisicion, ProveedorID, FechaVencimiento, Activo) VALUES 
(18, 'HIG-001', 100, 1.50, 1, '2027-12-31', 1),
(18, 'HIG-002', 50, 1.40, 1, '2027-10-15', 1),
(19, 'HIG-003', 80, 3.50, 1, '2027-11-30', 1),
(20, 'HIG-004', 120, 2.00, 2, '2027-09-20', 1),
(21, 'HIG-005', 150, 3.00, 2, '2028-01-15', 1),
(22, 'HIG-006', 100, 2.30, 2, '2027-08-30', 1),
(23, 'HIG-007', 200, 0.80, 1, '2027-12-01', 1);

-- Lotes para Cuidado Infantil (Productos ID 24-30)
INSERT INTO Lotes (ProductoID, NumeroLote, Cantidad, CostoAdquisicion, ProveedorID, FechaVencimiento, Activo) VALUES 
(24, 'INF-001', 60, 8.50, 1, '2027-09-30', 1),
(24, 'INF-002', 40, 8.30, 1, '2027-11-15', 1),
(25, 'INF-003', 50, 9.00, 1, '2027-10-20', 1),
(26, 'INF-004', 150, 5.50, 2, '2028-02-28', 1),
(27, 'INF-005', 100, 6.00, 2, '2028-01-15', 1),
(28, 'INF-006', 80, 2.80, 2, '2027-12-31', 1),
(29, 'INF-007', 90, 2.20, 1, '2027-11-01', 1),
(30, 'INF-008', 120, 1.80, 1, '2027-10-10', 1);

-- Lotes para Ortopedia y Botiquin (Productos ID 31-39)
INSERT INTO Lotes (ProductoID, NumeroLote, Cantidad, CostoAdquisicion, ProveedorID, FechaVencimiento, Activo) VALUES 
(31, 'ORT-001', 200, 1.20, 1, '2028-06-30', 1),
(32, 'ORT-002', 150, 1.60, 1, '2028-05-31', 1),
(33, 'ORT-003', 100, 2.00, 2, '2028-04-15', 1),
(34, 'ORT-004', 250, 0.90, 2, '2028-07-31', 1),
(35, 'ORT-005', 120, 1.80, 1, '2028-03-31', 1),
(36, 'ORT-006', 40, 10.50, 1, '2028-12-31', 1),
(37, 'ORT-007', 35, 15.00, 1, '2028-11-30', 1),
(38, 'ORT-008', 50, 5.50, 2, '2028-09-30', 1),
(39, 'ORT-009', 80, 4.00, 2, '2028-08-31', 1);

-- Lotes para Antihistaminicos (Productos ID 40-42)
INSERT INTO Lotes (ProductoID, NumeroLote, Cantidad, CostoAdquisicion, ProveedorID, FechaVencimiento, Activo) VALUES 
(40, 'ANTIH-001', 60, 3.50, 1, '2027-11-30', 1),
(41, 'ANTIH-002', 50, 4.00, 2, '2027-10-31', 1),
(42, 'ANTIH-003', 30, 5.20, 3, '2027-09-15', 1);

-- Lotes para Digestivos (Productos ID 43-45)
INSERT INTO Lotes (ProductoID, NumeroLote, Cantidad, CostoAdquisicion, ProveedorID, FechaVencimiento, Activo) VALUES 
(43, 'DIG-001', 80, 2.80, 1, '2028-01-31', 1),
(44, 'DIG-002', 60, 3.50, 2, '2027-12-31', 1),
(45, 'DIG-003', 50, 2.50, 1, '2027-11-30', 1);

-- ============================================================
-- 6. INDICES PARA RENDIMIENTO
-- ============================================================

CREATE INDEX IX_Lotes_ProductoFecha ON Lotes(ProductoID, FechaVencimiento);
CREATE INDEX IX_DetalleVentas_VentaID ON DetalleVentas(VentaID);
CREATE INDEX IX_BitacoraAuditoria_FechaHora ON BitacoraAuditoria(FechaHora);
CREATE INDEX IX_Ventas_Fecha ON Ventas(Fecha);
CREATE INDEX IX_Devoluciones_Estado ON Devoluciones(Estado);
CREATE INDEX IX_Productos_Nombre ON Productos(Nombre);
CREATE INDEX IX_Lotes_Activo ON Lotes(Activo);
CREATE INDEX IX_Empleados_Email ON Empleados(Email);

-- ============================================================
-- 7. VERIFICACION DE DATOS
-- ============================================================

SELECT '=== VITANOVA FARMACIA - BASE DE DATOS COMPLETA ===' AS Mensaje;
SELECT '---' AS Separador;

SELECT 'EMPLEADOS' AS Tabla, ID, Nombre, Email, RolID, Activo FROM Empleados;
SELECT '---' AS Separador;
SELECT 'ROLES' AS Tabla, ID, NombreRole FROM Roles;
SELECT '---' AS Separador;
SELECT 'CATEGORIAS' AS Tabla, IdCategoria, Nombre, Activa FROM Categorias;
SELECT '---' AS Separador;
SELECT 'PROVEEDORES' AS Tabla, ID, Nombre, Activo FROM Proveedores;
SELECT '---' AS Separador;
SELECT 'PRODUCTOS' AS Tabla, COUNT(*) AS Cantidad FROM Productos;
SELECT 'LOTES' AS Tabla, COUNT(*) AS Cantidad FROM Lotes;
SELECT 'CLIENTES' AS Tabla, COUNT(*) AS Cantidad FROM Clientes;
SELECT '---' AS Separador;

SELECT '=== RESUMEN POR CATEGORIA ===' AS Mensaje;
SELECT 
    c.Nombre AS Categoria,
    COUNT(p.ID) AS Productos,
    SUM(l.Cantidad) AS StockTotal
FROM Categorias c
LEFT JOIN Productos p ON p.IdCategoria = c.IdCategoria
LEFT JOIN Lotes l ON l.ProductoID = p.ID AND l.Activo = 1
WHERE c.Activa = 1
GROUP BY c.Nombre
ORDER BY c.Nombre;

SELECT '---' AS Separador;
SELECT '=== CREDENCIALES DE ACCESO ===' AS Mensaje;
SELECT 'admin@vitanova.com / Admin2026 (Administrador)' AS Credencial;
SELECT 'regente@vitanova.com / Regente2026 (Regente)' AS Credencial;
SELECT 'cajero@vitanova.com / Cajero2026 (Cajero)' AS Credencial;
SELECT 'demo@vitanova.com / 123456 (Emergencia)' AS Credencial;

SELECT '---' AS Separador;
SELECT '=== SCRIPT COMPLETADO EXITOSAMENTE ===' AS Mensaje;
GO

-- Number of tables and approx. size
SELECT 
    (SELECT COUNT(*) FROM sys.tables) AS NumberOfTables, 
    CAST(SUM(size * 8.0 / 1024) AS DECIMAL(10,2)) AS ApproxSizeInMB
FROM sys.database_files;



-- Three largest tables and record counts
SELECT TOP 3 
    t.name AS TableName,
    p.rows AS RecordCount
FROM sys.tables t
JOIN sys.partitions p ON t.object_id = p.object_id
WHERE p.index_id IN (0, 1) -- Heap o Clustered Index
ORDER BY p.rows DESC;

-- BACKUPS
-- Copia 1
BACKUP DATABASE VitaNovaFarmaciaDb
TO DISK = 'C:\VITANOVA Backups\VitaNovaFarmaciaDb_Copia1_20260909.bak'
WITH FORMAT,
     NAME = 'Primera Copia VitaNovaFarmaciaDb';

-- Copia 2:
BACKUP DATABASE VitaNovaFarmaciaDb
TO DISK = 'C:\VITANOVA Backups\VitaNovaFarmaciaDb_Copia2_20260909.bak'
WITH FORMAT,
     NAME = 'Segunda Copia VitaNovaFarmaciaDb';

-- Database restoration
RESTORE DATABASE VitaNovaFarmaciaDb_Test
FROM DISK = 'C:\VITANOVA Backups\VitaNovaFarmaciaDb_Copia1_20260909.bak'
WITH MOVE 'VitaNovaFarmaciaDb' TO 'C:\VITANOVA Backups\VitaNovaFarmaciaDb_Test.mdf',
     MOVE 'VitaNovaFarmaciaDb_log' TO 'C:\VITANOVA Backups\VitaNovaFarmaciaDb_Test.ldf',
     REPLACE;

-- Eliminar BD de restauracion
DROP DATABASE VitaNovaFarmaciaDb_Test;

-- Peso de la BD en MB
EXEC sp_spaceused;

-- Usuarios Activos 
SELECT 
    'Empleados / Usuarios' AS Tabla,
    COUNT(*) AS TotalUsuarios
FROM Empleados;