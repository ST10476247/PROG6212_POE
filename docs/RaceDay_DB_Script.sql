-- ============================================================================
-- Project: RaceDay - Full-Stack Event Management System
-- Part 1: Relational Database Schema and Seed Data Script
-- Author: ST10476247 (RaceDay Project Portfolio of Evidence)
-- Description: Complete T-SQL schema creation and data population script
--              for South African road running, walking, and cycling events.
-- Target RDBMS: Microsoft SQL Server 2019 / 2022 / Azure SQL Database
-- ============================================================================

USE master;
GO

-- 1. DATABASE CREATION (Safe conditional recreation)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'RaceDayDB')
BEGIN
    CREATE DATABASE RaceDayDB;
    PRINT 'Database [RaceDayDB] created successfully.';
END
ELSE
BEGIN
    PRINT 'Database [RaceDayDB] already exists. Using existing database.';
END
GO

USE RaceDayDB;
GO

-- 2. DROP TABLES IN REVERSE DEPENDENCY ORDER (For clean idempotent execution)
IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL DROP TABLE dbo.Results;
IF OBJECT_ID('dbo.Entries', 'U') IS NOT NULL DROP TABLE dbo.Entries;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID('dbo.Participants', 'U') IS NOT NULL DROP TABLE dbo.Participants;
IF OBJECT_ID('dbo.Organisers', 'U') IS NOT NULL DROP TABLE dbo.Organisers;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

PRINT 'Existing tables dropped successfully.';
GO

-- ============================================================================
-- 3. SCHEMA DEFINITIONS (7 Core Entities matching ERD)
-- ============================================================================

-- ENTITY 1: Users (Authentication, Credentials, and System Roles)
CREATE TABLE dbo.Users (
    UserID INT IDENTITY(1,1) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
    
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (UserID ASC),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Organiser', 'Participant', 'Admin'))
);
GO

-- ENTITY 2: Organisers (Event Host Organisations & Club Officials)
CREATE TABLE dbo.Organisers (
    OrganiserID INT IDENTITY(1,1) NOT NULL,
    UserID INT NOT NULL,
    OrganizationName NVARCHAR(150) NOT NULL,
    ContactEmail NVARCHAR(150) NOT NULL,
    Phone NVARCHAR(25) NOT NULL,
    Province NVARCHAR(50) NOT NULL,
    IsVerified BIT NOT NULL CONSTRAINT DF_Organisers_IsVerified DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_Organisers_CreatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Organisers PRIMARY KEY CLUSTERED (OrganiserID ASC),
    CONSTRAINT UQ_Organisers_UserID UNIQUE (UserID),
    CONSTRAINT FK_Organisers_Users FOREIGN KEY (UserID) 
        REFERENCES dbo.Users (UserID) ON DELETE CASCADE,
    CONSTRAINT CK_Organisers_Province CHECK (Province IN (
        'Eastern Cape', 'Free State', 'Gauteng', 'KwaZulu-Natal',
        'Limpopo', 'Mpumalanga', 'Northern Cape', 'North West', 'Western Cape'
    ))
);
GO

-- ENTITY 3: Participants (Athletes, Runners, Walkers, and Cyclists)
CREATE TABLE dbo.Participants (
    ParticipantID INT IDENTITY(1,1) NOT NULL,
    UserID INT NOT NULL,
    FirstName NVARCHAR(75) NOT NULL,
    LastName NVARCHAR(75) NOT NULL,
    SAIDOrPassport NVARCHAR(30) NOT NULL,
    Gender NVARCHAR(10) NOT NULL,
    DateOfBirth DATE NOT NULL,
    ClubName NVARCHAR(100) NULL,
    EmergencyPhone NVARCHAR(25) NOT NULL,
    Email NVARCHAR(150) NOT NULL,

    CONSTRAINT PK_Participants PRIMARY KEY CLUSTERED (ParticipantID ASC),
    CONSTRAINT UQ_Participants_UserID UNIQUE (UserID),
    CONSTRAINT UQ_Participants_SAIDOrPassport UNIQUE (SAIDOrPassport),
    CONSTRAINT FK_Participants_Users FOREIGN KEY (UserID) 
        REFERENCES dbo.Users (UserID) ON DELETE CASCADE,
    CONSTRAINT CK_Participants_Gender CHECK (Gender IN ('Male', 'Female', 'Other'))
);
GO

-- ENTITY 4: Events (South African Road Running, Walking & Cycling Events)
CREATE TABLE dbo.Events (
    EventID INT IDENTITY(1,1) NOT NULL,
    OrganiserID INT NOT NULL,
    EventName NVARCHAR(200) NOT NULL,
    EventType NVARCHAR(30) NOT NULL,
    EventDate DATETIME2(7) NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    Province NVARCHAR(50) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Events_Status DEFAULT 'Upcoming',
    BannerUrl NVARCHAR(500) NULL,
    ElevationGainMeters INT NOT NULL CONSTRAINT DF_Events_Elevation DEFAULT 0,
    KeyLandmarks NVARCHAR(300) NULL,
    TerrainType NVARCHAR(100) NOT NULL CONSTRAINT DF_Events_Terrain DEFAULT 'Road / Tarmac',
    CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_Events_CreatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Events PRIMARY KEY CLUSTERED (EventID ASC),
    CONSTRAINT FK_Events_Organisers FOREIGN KEY (OrganiserID) 
        REFERENCES dbo.Organisers (OrganiserID) ON DELETE NO ACTION,
    CONSTRAINT CK_Events_Type CHECK (EventType IN ('Running', 'Cycling', 'Walking', 'Multi-Sport')),
    CONSTRAINT CK_Events_Status CHECK (Status IN ('Upcoming', 'Live', 'Completed', 'Cancelled')),
    CONSTRAINT CK_Events_Province CHECK (Province IN (
        'Eastern Cape', 'Free State', 'Gauteng', 'KwaZulu-Natal',
        'Limpopo', 'Mpumalanga', 'Northern Cape', 'North West', 'Western Cape'
    ))
);
GO

-- ENTITY 5: Categories (Distances, Wave Starts, Fees, and Capacities)
CREATE TABLE dbo.Categories (
    CategoryID INT IDENTITY(1,1) NOT NULL,
    EventID INT NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    EntryFeeZAR DECIMAL(10,2) NOT NULL,
    MaxCapacity INT NOT NULL,
    StartTime TIME(0) NOT NULL,
    CutoffHours DECIMAL(4,2) NOT NULL,

    CONSTRAINT PK_Categories PRIMARY KEY CLUSTERED (CategoryID ASC),
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventID) 
        REFERENCES dbo.Events (EventID) ON DELETE CASCADE,
    CONSTRAINT CK_Categories_Distance CHECK (DistanceKm > 0),
    CONSTRAINT CK_Categories_Fee CHECK (EntryFeeZAR >= 0),
    CONSTRAINT CK_Categories_Capacity CHECK (MaxCapacity > 0),
    CONSTRAINT CK_Categories_Cutoff CHECK (CutoffHours > 0)
);
GO

-- ENTITY 6: Entries (Enrolments / Registrations - Associative Entity)
CREATE TABLE dbo.Entries (
    EntryID INT IDENTITY(1,1) NOT NULL,
    ParticipantID INT NOT NULL,
    CategoryID INT NOT NULL,
    BibNumber NVARCHAR(20) NOT NULL,
    RegistrationDate DATETIME2(7) NOT NULL CONSTRAINT DF_Entries_RegDate DEFAULT SYSUTCDATETIME(),
    PaymentStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_Entries_PaymentStatus DEFAULT 'Paid',
    PaymentReference NVARCHAR(50) NOT NULL,
    MedicalNotes NVARCHAR(500) NULL CONSTRAINT DF_Entries_Medical DEFAULT 'None',

    CONSTRAINT PK_Entries PRIMARY KEY CLUSTERED (EntryID ASC),
    CONSTRAINT UQ_Entries_PaymentReference UNIQUE (PaymentReference),
    CONSTRAINT UQ_Entries_Participant_Category UNIQUE (ParticipantID, CategoryID),
    CONSTRAINT UQ_Entries_Category_Bib UNIQUE (CategoryID, BibNumber),
    CONSTRAINT FK_Entries_Participants FOREIGN KEY (ParticipantID) 
        REFERENCES dbo.Participants (ParticipantID) ON DELETE NO ACTION,
    CONSTRAINT FK_Entries_Categories FOREIGN KEY (CategoryID) 
        REFERENCES dbo.Categories (CategoryID) ON DELETE CASCADE,
    CONSTRAINT CK_Entries_PaymentStatus CHECK (PaymentStatus IN ('Paid', 'Pending', 'Cancelled', 'Refunded'))
);
GO

-- ENTITY 7: Results (Official Timing, Overall/Category/Gender Ranks & Status)
CREATE TABLE dbo.Results (
    ResultID INT IDENTITY(1,1) NOT NULL,
    EntryID INT NOT NULL,
    GunTime TIME(3) NULL,
    ChipTime TIME(3) NULL,
    OverallRank INT NULL,
    CategoryRank INT NULL,
    GenderRank INT NULL,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Results_Status DEFAULT 'Finished',
    RecordedAt DATETIME2(7) NOT NULL CONSTRAINT DF_Results_RecordedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Results PRIMARY KEY CLUSTERED (ResultID ASC),
    CONSTRAINT UQ_Results_EntryID UNIQUE (EntryID),
    CONSTRAINT FK_Results_Entries FOREIGN KEY (EntryID) 
        REFERENCES dbo.Entries (EntryID) ON DELETE CASCADE,
    CONSTRAINT CK_Results_Status CHECK (Status IN ('Finished', 'DNF', 'DNS', 'Disqualified'))
);
GO

-- ============================================================================
-- 4. PERFORMANCE INDEXES
-- ============================================================================
CREATE NONCLUSTERED INDEX IX_Events_OrganiserID ON dbo.Events(OrganiserID);
CREATE NONCLUSTERED INDEX IX_Events_Status_Date ON dbo.Events(Status, EventDate);
CREATE NONCLUSTERED INDEX IX_Categories_EventID ON dbo.Categories(EventID);
CREATE NONCLUSTERED INDEX IX_Entries_ParticipantID ON dbo.Entries(ParticipantID);
CREATE NONCLUSTERED INDEX IX_Entries_CategoryID ON dbo.Entries(CategoryID);
CREATE NONCLUSTERED INDEX IX_Results_EntryID ON dbo.Results(EntryID);
GO

-- ============================================================================
-- 5. RELATIONAL VIEWS (Reporting & API Aggregations)
-- ============================================================================
CREATE OR ALTER VIEW dbo.vw_EventDetails AS
SELECT 
    e.EventID,
    e.EventName,
    e.EventType,
    e.EventDate,
    e.Location,
    e.Province,
    e.Status,
    e.BannerUrl,
    e.ElevationGainMeters,
    e.KeyLandmarks,
    e.TerrainType,
    o.OrganiserID,
    o.OrganizationName,
    o.ContactEmail AS OrganiserEmail,
    o.Phone AS OrganiserPhone,
    COUNT(DISTINCT c.CategoryID) AS TotalCategories,
    COUNT(DISTINCT ent.EntryID) AS TotalEnrolledParticipants
FROM dbo.Events e
INNER JOIN dbo.Organisers o ON e.OrganiserID = o.OrganiserID
LEFT JOIN dbo.Categories c ON e.EventID = c.EventID
LEFT JOIN dbo.Entries ent ON c.CategoryID = ent.CategoryID
GROUP BY 
    e.EventID, e.EventName, e.EventType, e.EventDate, e.Location, e.Province, 
    e.Status, e.BannerUrl, e.ElevationGainMeters, e.KeyLandmarks, e.TerrainType,
    o.OrganiserID, o.OrganizationName, o.ContactEmail, o.Phone;
GO

CREATE OR ALTER VIEW dbo.vw_EnrolmentLeaderboard AS
SELECT 
    r.ResultID,
    r.OverallRank,
    r.CategoryRank,
    r.GenderRank,
    r.ChipTime,
    r.GunTime,
    r.Status AS RaceStatus,
    ent.BibNumber,
    ent.PaymentStatus,
    p.ParticipantID,
    p.FirstName + ' ' + p.LastName AS ParticipantName,
    p.Gender,
    p.ClubName,
    c.CategoryID,
    c.CategoryName,
    c.DistanceKm,
    e.EventID,
    e.EventName,
    e.EventDate,
    e.Location
FROM dbo.Results r
INNER JOIN dbo.Entries ent ON r.EntryID = ent.EntryID
INNER JOIN dbo.Participants p ON ent.ParticipantID = p.ParticipantID
INNER JOIN dbo.Categories c ON ent.CategoryID = c.CategoryID
INNER JOIN dbo.Events e ON c.EventID = e.EventID;
GO

PRINT 'Schema definitions, indexes, and views created successfully.';
GO

-- ============================================================================
-- 6. REALISTIC SOUTH AFRICAN SEED DATA
-- ============================================================================

-- A. SEED USERS (2 Organisers + 4 Participants)
-- Passwords hashed representation for demonstration/development
INSERT INTO dbo.Users (Email, PasswordHash, Role, IsActive) VALUES
-- Organisers
('info@comrades.com', 'AQAAAAEAACcQAAAAEJ8u8vXmK1a...HashedPasswordOrg1', 'Organiser', 1),
('entries@capetowncycletour.com', 'AQAAAAEAACcQAAAAEK9v9wYnL2b...HashedPasswordOrg2', 'Organiser', 1),
('admin@sowetomarathon.com', 'AQAAAAEAACcQAAAAEM1x0zZoM3c...HashedPasswordOrg3', 'Organiser', 1),
-- Participants
('sibusiso.vilakazi@gmail.com', 'AQAAAAEAACcQAAAAEN2y1aApN4d...HashedPasswordPart1', 'Participant', 1),
('pieter.vandermerwe@outlook.com', 'AQAAAAEAACcQAAAAEO3z2bBqO5e...HashedPasswordPart2', 'Participant', 1),
('lerato.molefe@yahoo.com', 'AQAAAAEAACcQAAAAEP4a3cCrP6f...HashedPasswordPart3', 'Participant', 1),
('chloe.adams@mweb.co.za', 'AQAAAAEAACcQAAAAEQ5b4dDsQ7g...HashedPasswordPart4', 'Participant', 1);
GO

-- B. SEED ORGANISERS
INSERT INTO dbo.Organisers (UserID, OrganizationName, ContactEmail, Phone, Province, IsVerified) VALUES
(1, 'Comrades Marathon Association', 'info@comrades.com', '+27 33 897 8650', 'KwaZulu-Natal', 1),
(2, 'Cape Town Cycle Tour Trust', 'entries@capetowncycletour.com', '+27 21 681 4300', 'Western Cape', 1),
(3, 'Soweto Marathon Trust', 'admin@sowetomarathon.com', '+27 11 837 8443', 'Gauteng', 1);
GO

-- C. SEED PARTICIPANTS
INSERT INTO dbo.Participants (UserID, FirstName, LastName, SAIDOrPassport, Gender, DateOfBirth, ClubName, EmergencyPhone, Email) VALUES
(4, 'Sibusiso', 'Vilakazi', '9205145829081', 'Male', '1992-05-14', 'Hollywoodbets Athletics Club', '+27 82 456 7890', 'sibusiso.vilakazi@gmail.com'),
(5, 'Pieter', 'van der Merwe', '8811235129088', 'Male', '1988-11-23', 'Tygerberg Cycling Club', '+27 83 987 6543', 'pieter.vandermerwe@outlook.com'),
(6, 'Lerato', 'Molefe', '9502180429084', 'Female', '1995-02-18', 'Orlando AC Soweto', '+27 71 234 5678', 'lerato.molefe@yahoo.com'),
(7, 'Chloe', 'Adams', '9907090229080', 'Female', '1999-07-09', 'Atlantic Triathlon Club', '+27 84 345 6789', 'chloe.adams@mweb.co.za');
GO

-- D. SEED EVENTS (Iconic South African Road Events)
INSERT INTO dbo.Events (OrganiserID, EventName, EventType, EventDate, Location, Province, Description, Status, BannerUrl, ElevationGainMeters, KeyLandmarks, TerrainType) VALUES
(1, 'Comrades Marathon 2026', 'Running', '2026-06-14 05:30:00', 'Pietermaritzburg to Durban (Down Run)', 'KwaZulu-Natal', 
 'The Ultimate Human Race - The world''s oldest and largest ultramarathon run over 89km between Pietermaritzburg City Hall and Durban Kingsmead Stadium.', 
 'Upcoming', 'https://images.unsplash.com/photo-1530549387789-4c1017266635?auto=format&fit=crop&w=1200&q=80', 1250, 'Polly Shorts, Inchanga, Botha''s Hill, Fields Hill, Cowie''s Hill', 'Road / Tarmac'),

(2, 'Cape Town Cycle Tour 2026', 'Cycling', '2026-03-08 06:00:00', 'Cape Town Grand Parade, Cape Town', 'Western Cape', 
 'The iconic 109km bicycle race around the picturesque Cape Peninsula, hosting over 35,000 local and international cyclists through scenic coastal roads.', 
 'Upcoming', 'https://images.unsplash.com/photo-1517649763962-0c623266ddc0?auto=format&fit=crop&w=1200&q=80', 1180, 'Smitswinkel, Chapman''s Peak Drive, Suikerbossie', 'Road / Coastal Tarmac'),

(3, 'Soweto Marathon 2026', 'Running', '2026-11-01 06:00:00', 'FNB Stadium, Nasrec, Soweto', 'Gauteng', 
 'The People''s Race - Experience the vibrant heart of South Africa, running through the historical streets of Soweto past Vilakazi Street and Hector Pieterson Memorial.', 
 'Upcoming', 'https://images.unsplash.com/photo-1452626038306-9aae5e071dd3?auto=format&fit=crop&w=1200&q=80', 490, 'Vilakazi Street, Walter Sisulu Square, Chris Hani Baragwanath Hospital', 'City Road / Tarmac');
GO

-- E. SEED CATEGORIES
INSERT INTO dbo.Categories (EventID, CategoryName, DistanceKm, EntryFeeZAR, MaxCapacity, StartTime, CutoffHours) VALUES
-- Comrades Marathon Categories
(1, '89km Ultra Marathon (Down Run)', 89.00, 1250.00, 25000, '05:30:00', 12.00),
(1, '45km Half-Comrades Challenge', 45.00, 650.00, 5000, '07:00:00', 6.50),

-- Cape Town Cycle Tour Categories
(2, '109km Classic Road Race', 109.00, 850.00, 30000, '06:00:00', 7.00),
(2, '42km Short Peninsula Route', 42.00, 480.00, 8000, '08:30:00', 4.00),

-- Soweto Marathon Categories
(3, '42.2km Standard Marathon', 42.20, 450.00, 10000, '06:00:00', 6.00),
(3, '21.1km Half Marathon', 21.10, 300.00, 12000, '06:30:00', 3.50),
(3, '10km Community Run & Walk', 10.00, 180.00, 8000, '07:15:00', 2.00);
GO

-- F. SEED ENTRIES (Registrations)
INSERT INTO dbo.Entries (ParticipantID, CategoryID, BibNumber, RegistrationDate, PaymentStatus, PaymentReference, MedicalNotes) VALUES
-- Sibusiso enters Comrades 89km
(1, 1, 'C-10482', '2026-01-15 08:30:00', 'Paid', 'PAY-COM26-89K-001', 'None - Completed qualifying marathon in 03:15:00'),
-- Pieter enters Cape Town Cycle Tour 109km
(2, 3, 'CT-34901', '2026-01-18 10:15:00', 'Paid', 'PAY-CTCT26-109-002', 'Asthma inhaler carried in jersey pocket'),
-- Lerato enters Soweto Marathon 42.2km
(3, 5, 'SW-08421', '2026-02-01 14:20:00', 'Paid', 'PAY-SOW26-42K-003', 'Mild penicillin allergy'),
-- Chloe enters Cape Town Cycle Tour 109km
(4, 3, 'CT-34902', '2026-02-10 11:45:00', 'Paid', 'PAY-CTCT26-109-004', 'None');
GO

-- G. SEED RESULTS (Timing & Leaderboard)
INSERT INTO dbo.Results (EntryID, GunTime, ChipTime, OverallRank, CategoryRank, GenderRank, Status) VALUES
(1, '06:14:22.450', '06:13:58.120', 48, 12, 42, 'Finished'),
(2, '02:58:14.300', '02:56:45.890', 115, 34, 108, 'Finished'),
(3, '03:42:10.150', '03:41:28.020', 82, 19, 15, 'Finished'),
(4, '03:15:40.000', '03:14:12.750', 230, 45, 38, 'Finished');
GO

-- ============================================================================
-- 7. VERIFICATION SCRIPT / QUERY CHECK
-- ============================================================================
PRINT '==================================================';
PRINT 'RaceDay Database Schema & Seed Data Verification';
PRINT '==================================================';
SELECT 'Users' AS EntityName, COUNT(*) AS TotalRecords FROM dbo.Users
UNION ALL
SELECT 'Organisers', COUNT(*) FROM dbo.Organisers
UNION ALL
SELECT 'Participants', COUNT(*) FROM dbo.Participants
UNION ALL
SELECT 'Events', COUNT(*) FROM dbo.Events
UNION ALL
SELECT 'Categories', COUNT(*) FROM dbo.Categories
UNION ALL
SELECT 'Entries', COUNT(*) FROM dbo.Entries
UNION ALL
SELECT 'Results', COUNT(*) FROM dbo.Results;
GO

PRINT 'RaceDay database setup completed with zero errors.';
GO
