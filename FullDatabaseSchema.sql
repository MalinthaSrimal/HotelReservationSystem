-- Hotel Reservation System - Complete Database Schema
-- Version: 1.0
-- Created: October 2025

-- Create the database
CREATE DATABASE HotelResSysDb;
GO

USE HotelResSysDb;
GO

-- ======================
-- TABLE DEFINITIONS
-- ======================

-- 1. Customers Table
CREATE TABLE Customers (
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    IdNumber NVARCHAR(50) NOT NULL, -- Passport/ID
    PhoneNumber NVARCHAR(20),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
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
    IsAvailable BIT DEFAULT 1,
    Description NVARCHAR(500) NULL
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
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CheckedInAt DATETIME2 NULL,
    CheckedOutAt DATETIME2 NULL,
    
    CONSTRAINT FK_Reservations_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT FK_Reservations_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId),
    CONSTRAINT FK_Reservations_TravelCompanies FOREIGN KEY (TravelCompanyId) REFERENCES TravelCompanies(TravelCompanyId),
    CONSTRAINT CHK_DepartureAfterArrival CHECK (DepartureDate > ArrivalDate)
);
GO

-- 5. BillingRecords Table
CREATE TABLE BillingRecords (
    BillingRecordId INT IDENTITY(1,1) PRIMARY KEY,
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
-- INDEXES FOR PERFORMANCE
-- ======================

-- Reservation indexes
CREATE INDEX IX_Reservations_ArrivalDate ON Reservations(ArrivalDate);
CREATE INDEX IX_Reservations_IsNoShow ON Reservations(IsNoShow);
CREATE INDEX IX_Reservations_IsCheckedIn ON Reservations(IsCheckedIn);
CREATE INDEX IX_Reservations_CheckedOutAt ON Reservations(CheckedOutAt);

-- Billing indexes
CREATE INDEX IX_BillingRecords_BilledAt ON BillingRecords(BilledAt);
CREATE INDEX IX_BillingRecords_IsNoShowBill ON BillingRecords(IsNoShowBill);

-- Customer indexes
CREATE INDEX IX_Customers_Email ON Customers(Email);
CREATE INDEX IX_Customers_IdNumber ON Customers(IdNumber);

-- Room indexes
CREATE INDEX IX_Rooms_RoomType ON Rooms(RoomType);
CREATE INDEX IX_Rooms_IsAvailable ON Rooms(IsAvailable);

-- ======================
-- SAMPLE DATA (OPTIONAL)
-- ======================

-- Rooms
INSERT INTO Rooms (RoomNumber, RoomType, NightlyRate, WeeklyRate, MonthlyRate, IsAvailable, Description)
VALUES
('101', 'Standard', 120, 0, 0, 1, 'Standard room with queen bed, city view'),
('102', 'Standard', 120, 0, 0, 1, 'Standard room with twin beds, garden view'),
('201', 'Deluxe', 180, 0, 0, 1, 'Deluxe room with king bed, ocean view, balcony'),
('301', 'Suite', 300, 0, 0, 1, 'Executive suite with separate living area, premium amenities'),
('R101', 'ResidentialSuite', 0, 700, 2500, 1, 'Residential suite for extended stays, kitchenette included');

-- Sample Customer
INSERT INTO Customers (FullName, Email, IdNumber, PhoneNumber)
VALUES ('John Doe', 'john@example.com', 'P1234567', '+1234567890');

-- Sample Travel Company
INSERT INTO TravelCompanies (CompanyName, ContactEmail, DiscountRate, ContactPhone)
VALUES ('Global Tours Inc.', 'contact@globaltours.com', 0.20, '+1-555-0123');

-- Sample Reservation (with Credit Card)
INSERT INTO Reservations (CustomerId, RoomId, ReservationNumber, ArrivalDate, DepartureDate, NumberOfOccupants, HasCreditCard, CardNumber, Expiry, IsConfirmed)
VALUES (1, 1, 'RES-001', '2024-07-15', '2024-07-18', 2, 1, '****1111', '12/27', 1);

-- Sample No-show reservation (no Credit Card)
INSERT INTO Reservations (CustomerId, RoomId, ReservationNumber, ArrivalDate, DepartureDate, HasCreditCard, IsConfirmed)
VALUES (1, 2, 'RES-002', '2024-07-10', '2024-07-12', 0, 0);

-- Sample Billing (for no-show)
INSERT INTO BillingRecords (ReservationId, RoomCharge, PaymentMethod, IsPaid, IsNoShowBill)
VALUES (2, 120, 'Auto (No-Show)', 1, 1);

GO

-- ======================
-- VIEWS FOR REPORTING
-- ======================

-- View for active reservations
CREATE VIEW vw_ActiveReservations AS
SELECT 
    r.ReservationId,
    r.ReservationNumber,
    c.FullName AS GuestName,
    rm.RoomNumber,
    rm.RoomType,
    r.ArrivalDate,
    r.DepartureDate,
    r.IsCheckedIn,
    r.CheckedOutAt,
    DATEDIFF(day, r.ArrivalDate, r.DepartureDate) AS Nights
FROM Reservations r
JOIN Customers c ON r.CustomerId = c.CustomerId
JOIN Rooms rm ON r.RoomId = rm.RoomId
WHERE r.CheckedOutAt IS NULL;
GO

-- View for daily revenue summary
CREATE VIEW vw_DailyRevenue AS
SELECT 
    CAST(b.BilledAt AS DATE) AS RevenueDate,
    COUNT(*) AS TotalBills,
    SUM(b.RoomCharge) AS RoomRevenue,
    SUM(b.RestaurantCharge + b.RoomServiceCharge + b.LaundryCharge + b.TelephoneCharge + b.ClubFacilityCharge) AS ServicesRevenue,
    SUM(b.OverstayCharge) AS OverstayRevenue,
    SUM(b.RoomCharge + b.RestaurantCharge + b.RoomServiceCharge + b.LaundryCharge + b.TelephoneCharge + b.ClubFacilityCharge + b.OverstayCharge) AS TotalRevenue
FROM BillingRecords b
WHERE b.IsPaid = 1
GROUP BY CAST(b.BilledAt AS DATE);
GO

-- ======================
-- STORED PROCEDURES
-- ======================

-- Procedure to get room availability for date range
CREATE PROCEDURE sp_GetAvailableRooms
    @CheckInDate DATE,
    @CheckOutDate DATE,
    @RoomType NVARCHAR(20) = NULL
AS
BEGIN
    SELECT 
        r.RoomId,
        r.RoomNumber,
        r.RoomType,
        r.NightlyRate,
        r.WeeklyRate,
        r.MonthlyRate,
        r.Description
    FROM Rooms r
    WHERE r.IsAvailable = 1
        AND (@RoomType IS NULL OR r.RoomType = @RoomType)
        AND r.RoomId NOT IN (
            SELECT res.RoomId 
            FROM Reservations res
            WHERE res.IsConfirmed = 1
                AND res.CheckedOutAt IS NULL
                AND NOT (res.DepartureDate <= @CheckInDate OR res.ArrivalDate >= @CheckOutDate)
        );
END
GO

-- Procedure to process no-show billing
CREATE PROCEDURE sp_ProcessNoShowBilling
    @ProcessDate DATE
AS
BEGIN
    DECLARE @NoShowCount INT = 0;
    
    -- Insert billing records for no-shows
    INSERT INTO BillingRecords (ReservationId, RoomCharge, PaymentMethod, IsPaid, IsNoShowBill, BilledAt)
    SELECT 
        r.ReservationId,
        rm.NightlyRate,
        'Auto (No-Show)',
        1,
        1,
        DATEADD(HOUR, 19, @ProcessDate) -- 7 PM
    FROM Reservations r
    JOIN Rooms rm ON r.RoomId = rm.RoomId
    WHERE r.ArrivalDate = @ProcessDate
        AND r.IsCheckedIn = 0
        AND r.IsNoShow = 0
        AND NOT EXISTS (
            SELECT 1 FROM BillingRecords b 
            WHERE b.ReservationId = r.ReservationId AND b.IsNoShowBill = 1
        );
    
    SET @NoShowCount = @@ROWCOUNT;
    
    -- Mark reservations as no-show
    UPDATE Reservations 
    SET IsNoShow = 1
    WHERE ArrivalDate = @ProcessDate
        AND IsCheckedIn = 0
        AND IsNoShow = 0;
    
    SELECT @NoShowCount AS ProcessedNoShows;
END
GO

-- ======================
-- FUNCTIONS
-- ======================

-- Function to calculate total amount for a billing record
CREATE FUNCTION fn_CalculateTotalBill(@BillingRecordId INT)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @Total DECIMAL(10,2);
    
    SELECT @Total = RoomCharge + RestaurantCharge + RoomServiceCharge + 
                   LaundryCharge + TelephoneCharge + ClubFacilityCharge + OverstayCharge
    FROM BillingRecords
    WHERE BillingRecordId = @BillingRecordId;
    
    RETURN ISNULL(@Total, 0);
END
GO

-- ======================
-- TRIGGERS
-- ======================

-- Trigger to automatically update room availability
CREATE TRIGGER tr_UpdateRoomAvailability
ON Reservations
AFTER INSERT, UPDATE
AS
BEGIN
    -- Mark rooms as unavailable when checked in
    UPDATE Rooms 
    SET IsAvailable = 0
    WHERE RoomId IN (
        SELECT RoomId FROM inserted 
        WHERE IsCheckedIn = 1 AND CheckedOutAt IS NULL
    );
    
    -- Mark rooms as available when checked out
    UPDATE Rooms 
    SET IsAvailable = 1
    WHERE RoomId IN (
        SELECT RoomId FROM inserted 
        WHERE CheckedOutAt IS NOT NULL
    );
END
GO

PRINT 'Hotel Reservation System database schema created successfully!';
PRINT 'Database includes:';
PRINT '- 5 main tables with proper relationships';
PRINT '- Performance indexes';
PRINT '- Sample data';
PRINT '- 2 reporting views';
PRINT '- 2 stored procedures';
PRINT '- 1 function';
PRINT '- 1 trigger for room availability';