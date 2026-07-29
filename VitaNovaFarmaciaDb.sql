CREATE DATABASE VitaNovaFarmaciaDb;
GO

USE VitaNovaFarmaciaDb;
GO

CREATE TABLE Roles (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    NombreRole VARCHAR(50) NOT NULL
);

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

CREATE TABLE Categorias (
    IdCategoria INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(300) NULL,
    Activa BIT NOT NULL DEFAULT 1
);

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

CREATE TABLE MetodosPago (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL
);

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

CREATE TABLE DevolucionDetalles (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    DevolucionID INT NOT NULL,
    DetalleVentaID INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioDevuelto DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_DevDet_Devoluciones FOREIGN KEY (DevolucionID) REFERENCES Devoluciones(ID) ON DELETE CASCADE,
    CONSTRAINT FK_DevDet_DetalleVenta FOREIGN KEY (DetalleVentaID) REFERENCES DetalleVentas(ID)
);

-- DATOS INICIALES

-- Roles
INSERT INTO Roles (NombreRole) VALUES ('Administrador'), ('Cajero'), ('Regente'), ('Auditor');

-- Metodos de pago
INSERT INTO MetodosPago (Nombre) VALUES ('Efectivo'), ('Tarjeta');

-- Categorias
INSERT INTO Categorias (Nombre, Descripcion) VALUES 
('Analgesicos', 'Medicamentos para el dolor'),
('Antibioticos', 'Medicamentos para infecciones'),
('Vitaminas', 'Suplementos vitaminicos'),
('Antihistaminicos', 'Medicamentos para alergias'),
('Digestivos', 'Medicamentos para el sistema digestivo');

-- Proveedores
INSERT INTO Proveedores (Nombre, Telefono, Correo, Direccion, NIT) VALUES 
('Laboratorios Vita', '2233-4455', 'ventas@vitalab.com', 'Calle 10 #5-30', '900123456-1'),
('Distribuidora Medica SA', '2299-8877', 'info@dmsa.com', 'Av. Principal #15-20', '900654321-2');

-- Empleado Admin (contrasena: Admin2026)
INSERT INTO Empleados (Nombre, Email, PasswordHash, RolID) VALUES 
('Admin Principal', 'admin@vitanova.com', '$2a$11$ZyBviw8F.7TqVrOZ1KxIuOVfF7HsPqkMkLj.XTqCpV3XLtBfZZ6u6', 1);

-- Empleado Regente (contrasena: Regente2026)
INSERT INTO Empleados (Nombre, Email, PasswordHash, RolID) VALUES 
('Regente Principal', 'regente@vitanova.com', '$2a$11$YvBviw8F.7TqVrOZ1KxIuOVfF7HsPqkMkLj.XTqCpV3XLtBfZZ6u7', 3);

-- Empleado Cajero (contrasena: Cajero2026)
INSERT INTO Empleados (Nombre, Email, PasswordHash, RolID) VALUES 
('Cajero Principal', 'cajero@vitanova.com', '$2a$11$XwBviw8F.7TqVrOZ1KxIuOVfF7HsPqkMkLj.XTqCpV3XLtBfZZ6u8', 2);

-- Cliente general
INSERT INTO Clientes (Documento, Nombre) VALUES ('00000000', 'Cliente General');

-- Productos
INSERT INTO Productos (Nombre, ComponenteActivo, Precio, StockMinimo, IdCategoria, CodigoBarras) VALUES 
('Paracetamol 500mg', 'Acetaminofen', 3.50, 10, 1, '750123456789'),
('Ibuprofeno 400mg', 'Ibuprofeno', 4.25, 10, 1, '750987654321'),
('Amoxicilina 500mg', 'Amoxicilina', 12.50, 5, 2, '750456789123'),
('Vitamina C 1000mg', 'Acido Ascorbico', 8.99, 8, 3, '750321654987'),
('Loratadina 10mg', 'Loratadina', 5.75, 5, 4, '750789123456');

-- Lotes
INSERT INTO Lotes (ProductoID, NumeroLote, Cantidad, CostoAdquisicion, ProveedorID, FechaVencimiento) VALUES 
(1, 'LOTE-001', 100, 2.50, 1, '2027-12-31'),
(1, 'LOTE-002', 50, 2.30, 1, '2027-08-15'),
(2, 'LOTE-003', 80, 3.00, 2, '2027-11-30'),
(3, 'LOTE-004', 60, 8.00, 1, '2027-10-15'),
(4, 'LOTE-005', 120, 6.00, 2, '2028-01-31'),
(5, 'LOTE-006', 40, 4.00, 1, '2027-09-20');

-- INDICES PARA RENDIMIENTO
CREATE INDEX IX_Lotes_ProductoFecha ON Lotes(ProductoID, FechaVencimiento);
CREATE INDEX IX_DetalleVentas_VentaID ON DetalleVentas(VentaID);
CREATE INDEX IX_BitacoraAuditoria_FechaHora ON BitacoraAuditoria(FechaHora);
CREATE INDEX IX_Ventas_Fecha ON Ventas(Fecha);
CREATE INDEX IX_Devoluciones_Estado ON Devoluciones(Estado);
CREATE INDEX IX_Productos_Nombre ON Productos(Nombre);
CREATE INDEX IX_Lotes_Activo ON Lotes(Activo);