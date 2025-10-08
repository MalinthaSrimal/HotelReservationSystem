-- Create the database
CREATE DATABASE HotelResSysDb;
GO

USE HotelResSysDb;
GO

-- 1. Customers Table
CREATE TABLE Customers (
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    IdNumber NVARCHAR(50) NOT NULL, -- Passport/ID
    PhoneNumber NVARCHAR(20)
);
GO

-- 2. TravelCompanies Table
CREATE TABLE TravelCompanies (
    TravelCompanyId INT IDENTITY(1,1) PRIMARY KEY,
    CompanyName NVARCHAR(100) NOT NULL,
    ContactEmail NVARCHAR(100),
    ContactPhone NVARCHAR(20),
    DiscountRate DECIMAL(3,2) DEFAULT 0.15, -- 15%
    IsActive BIT DEFAULT 1
);
GO

-- 3. Rooms Table
CREATE TABLE Rooms (
    RoomId INT IDENTITY(1,1) PRIMARY KEY,
    RoomNumber NVARCHAR(10) NOT NULL UNIQUE,
    RoomType NVARCHAR(20) NOT NULL CHECK (RoomType IN ('Standard', 'Deluxe', 'Suite', 'ResidentialSuite')),
    NightlyRate DECIMAL(10,2) NOT NULL,
    WeeklyRate DECIMAL(10,2) DEFAULT 0,   -- For ResidentialSuite
    MonthlyRate DECIMAL(10,2) DEFAULT 0,  -- For ResidentialSuite
    IsAvailable BIT DEFAULT 1
);
GO

-- 4. Reservations Table
CREATE TABLE Reservations (
    ReservationId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    RoomId INT NOT NULL,
    TravelCompanyId INT NULL,
    ReservationNumber NVARCHAR(20) NOT NULL UNIQUE,
    ArrivalDate DATE NOT NULL,
    DepartureDate DATE NOT NULL,
    NumberOfOccupants INT DEFAULT 1,
    HasCreditCard BIT DEFAULT 0,
    CardNumber NVARCHAR(25) NULL, -- Store last 4 in real app; never CVV!
    Expiry NVARCHAR(10) NULL,
    IsConfirmed BIT DEFAULT 1,
    IsCheckedIn BIT DEFAULT 0,
    IsNoShow BIT DEFAULT 0,
    --CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CheckedInAt DATETIME2 NULL,
    CheckedOutAt DATETIME2 NULL,
    
    CONSTRAINT FK_Reservations_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT FK_Reservations_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId),
    CONSTRAINT FK_Reservations_TravelCompanies FOREIGN KEY (TravelCompanyId) REFERENCES TravelCompanies(TravelCompanyId),
    CONSTRAINT CHK_DepartureAfterArrival CHECK (DepartureDate > ArrivalDate)
);
GO

ALTER TABLE Reservations
ADD 
    CVV NVARCHAR(10) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE();
GO

ALTER TABLE Reservations
ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE();

-- Add the missing CreatedAt column
ALTER TABLE Reservations
ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE();
GO

-- 5. BillingRecords Table
CREATE TABLE BillingRecords (
    BillingRecordId INT IDENTITY(1,1) PRIMARY KEY, -- ✅ Primary key fixed!
    ReservationId INT NOT NULL,
    RoomCharge DECIMAL(10,2) NOT NULL,
    RestaurantCharge DECIMAL(10,2) DEFAULT 0,
    RoomServiceCharge DECIMAL(10,2) DEFAULT 0,
    LaundryCharge DECIMAL(10,2) DEFAULT 0,
    TelephoneCharge DECIMAL(10,2) DEFAULT 0,
    ClubFacilityCharge DECIMAL(10,2) DEFAULT 0,
    OverstayCharge DECIMAL(10,2) DEFAULT 0,
    PaymentMethod NVARCHAR(20) DEFAULT 'Cash' CHECK (PaymentMethod IN ('Cash', 'CreditCard', 'Auto (No-Show)')),
    IsPaid BIT DEFAULT 1,
    IsNoShowBill BIT DEFAULT 0,
    BilledAt DATETIME2 DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Billing_Reservations FOREIGN KEY (ReservationId) REFERENCES Reservations(ReservationId)
);
GO

-- ======================
-- Sample Data (Optional)
-- ======================

-- Rooms
INSERT INTO Rooms (RoomNumber, RoomType, NightlyRate, WeeklyRate, MonthlyRate, IsAvailable)
VALUES
('101', 'Standard', 120, 0, 0, 1),
('102', 'Standard', 120, 0, 0, 1),
('201', 'Deluxe', 180, 0, 0, 1),
('301', 'Suite', 300, 0, 0, 1),
('R101', 'ResidentialSuite', 0, 700, 2500, 1);

-- Customer
INSERT INTO Customers (FullName, Email, IdNumber, PhoneNumber)
VALUES ('John Doe', 'john@example.com', 'P1234567', '+1234567890');

-- Reservation (with CC)
INSERT INTO Reservations (CustomerId, RoomId, ReservationNumber, ArrivalDate, DepartureDate, NumberOfOccupants, HasCreditCard, CardNumber, Expiry, IsConfirmed)
VALUES (1, 1, 'RES-001', '2024-07-15', '2024-07-18', 2, 1, '4111111111111111', '12/27', 1);

-- No-show reservation (no CC)
INSERT INTO Reservations (CustomerId, RoomId, ReservationNumber, ArrivalDate, DepartureDate, HasCreditCard, IsConfirmed)
VALUES (1, 2, 'RES-002', '2024-07-10', '2024-07-12', 0, 0);

-- Billing (for no-show)
INSERT INTO BillingRecords (ReservationId, RoomCharge, PaymentMethod, IsPaid, IsNoShowBill)
VALUES (2, 120, 'Auto (No-Show)', 1, 1);

-- Travel Company
INSERT INTO TravelCompanies (CompanyName, ContactEmail, DiscountRate)
VALUES ('Global Tours Inc.', 'contact@globaltours.com', 0.20);
GO