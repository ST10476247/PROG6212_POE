/* =========================================================
   RaceDay — Part 1 Database Script
   Target:  SQL Server (SSMS / MS SQL)
   Author:  PROG6212 PoE
   Entities: 8  (Users, Organisers, Participants, Events,
                  EventCategories, EventEnrolments, Results,
                  WeatherSnapshots)
   Usage:  Open in SSMS, connect to your SQL instance,
           make sure no RaceDay DB exists, then execute.
   ========================================================= */

PRINT '==================================================';
PRINT '  RaceDay PoE Part 1 - Database Script';
PRINT '==================================================';
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

USE master;
GO

IF DB_ID('RaceDay') IS NOT NULL
BEGIN
    PRINT 'Dropping existing RaceDay database...';
    ALTER DATABASE [RaceDay] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [RaceDay];
END
GO

CREATE DATABASE [RaceDay]
 ON  PRIMARY
(
    NAME      = N'RaceDay_Data',
    FILENAME  = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\RaceDay.mdf',
    SIZE      = 8 MB,
    FILEGROWTH = 64 MB
)
 LOG ON
(
    NAME      = N'RaceDay_Log',
    FILENAME  = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\RaceDay_log.ldf',
    SIZE      = 8 MB,
    FILEGROWTH = 64 MB
);
GO

PRINT 'Database [RaceDay] created.';
GO

USE [RaceDay];
GO

/* =========================================================
   1. Users   — shared authentication for Organisers & Participants
   ========================================================= */
CREATE TABLE [dbo].[Users] (
    [UserId]        INT IDENTITY(1,1) NOT NULL,
    [Email]         NVARCHAR(255) NOT NULL,
    [PasswordHash]  NVARCHAR(255) NOT NULL,
    [Role]          NVARCHAR(20)  NOT NULL,
    [FullName]      NVARCHAR(150) NOT NULL,
    [PhoneNumber]   NVARCHAR(20)  NULL,
    [Province]      NVARCHAR(50)  NULL,
    [CreatedAt]     DATETIME2(3)  NOT NULL
        CONSTRAINT [DF_Users_CreatedAt] DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT [UK_Users_Email] UNIQUE NONCLUSTERED ([Email] ASC),
    CONSTRAINT [CK_Users_Role] CHECK ([Role] IN ('Organiser','Participant'))
);
GO
PRINT 'Table [Users] created.';

/* =========================================================
   2. Organisers   — extension table for Role = Organiser
   ========================================================= */
CREATE TABLE [dbo].[Organisers] (
    [OrganiserId]   INT IDENTITY(1,1) NOT NULL,
    [UserId]        INT NOT NULL,
    [CompanyName]   NVARCHAR(200) NOT NULL,
    [CompanyRegNo]  NVARCHAR(30)  NULL,
    [ContactEmail]  NVARCHAR(255) NOT NULL,
    [IsVerified]    BIT NOT NULL
        CONSTRAINT [DF_Organisers_IsVerified] DEFAULT (0),
    CONSTRAINT [PK_Organisers] PRIMARY KEY CLUSTERED ([OrganiserId] ASC),
    CONSTRAINT [UK_Organisers_UserId] UNIQUE NONCLUSTERED ([UserId] ASC),
    CONSTRAINT [FK_Organisers_Users]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId])
        ON DELETE CASCADE
);
GO
PRINT 'Table [Organisers] created.';

/* =========================================================
   3. Participants  — extension table for Role = Participant
   ========================================================= */
CREATE TABLE [dbo].[Participants] (
    [ParticipantId]   INT IDENTITY(1,1) NOT NULL,
    [UserId]          INT NOT NULL,
    [DateOfBirth]     DATE NOT NULL,
    [Gender]          NVARCHAR(10) NULL,
    [EmergencyContactName] NVARCHAR(150) NULL,
    [EmergencyContactNo]   NVARCHAR(20)  NULL,
    [TshirtSize]      NVARCHAR(5)   NULL,
    CONSTRAINT [PK_Participants] PRIMARY KEY CLUSTERED ([ParticipantId] ASC),
    CONSTRAINT [UK_Participants_UserId] UNIQUE NONCLUSTERED ([UserId] ASC),
    CONSTRAINT [FK_Participants_Users]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId])
        ON DELETE CASCADE
);
GO
PRINT 'Table [Participants] created.';

/* =========================================================
   4. Events
   ========================================================= */
CREATE TABLE [dbo].[Events] (
    [EventId]          INT IDENTITY(1,1) NOT NULL,
    [OrganiserId]      INT NOT NULL,
    [Title]            NVARCHAR(200) NOT NULL,
    [EventType]        NVARCHAR(30)  NOT NULL,
    [Description]      NVARCHAR(2000) NULL,
    [StartDate]        DATETIME2(0) NOT NULL,
    [EndDate]          DATETIME2(0) NULL,
    [City]             NVARCHAR(100) NOT NULL,
    [Province]         NVARCHAR(50)  NOT NULL,
    [RouteUrl]         NVARCHAR(500) NULL,
    [EntryFeeZar]      DECIMAL(9,2)  NOT NULL
        CONSTRAINT [DF_Events_EntryFeeZar] DEFAULT (0),
    [MaxEntries]       INT NULL,
    [RegistrationOpen] DATETIME2(0) NULL,
    [RegistrationClose]DATETIME2(0) NULL,
    [Status]           NVARCHAR(20) NOT NULL
        CONSTRAINT [DF_Events_Status] DEFAULT ('Draft'),
    [CreatedAt]        DATETIME2(3) NOT NULL
        CONSTRAINT [DF_Events_CreatedAt] DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED ([EventId] ASC),
    CONSTRAINT [CK_Events_Type] CHECK ([EventType] IN
        ('Marathon','HalfMarathon','10km','5km','Cycle','Trail','Walk','ParkRun','Other')),
    CONSTRAINT [CK_Events_Status] CHECK ([Status] IN
        ('Draft','Open','Closed','Completed','Cancelled')),
    CONSTRAINT [CK_Events_Dates] CHECK ([EndDate] IS NULL OR [EndDate] >= [StartDate]),
    CONSTRAINT [FK_Events_Organisers]
        FOREIGN KEY ([OrganiserId]) REFERENCES [dbo].[Organisers]([OrganiserId])
        ON DELETE NO ACTION
);
GO
PRINT 'Table [Events] created.';

/* =========================================================
   5. EventCategories  (e.g., Comrades Men Open, Ladies 42.2km, etc.)
   ========================================================= */
CREATE TABLE [dbo].[EventCategories] (
    [CategoryId]     INT IDENTITY(1,1) NOT NULL,
    [EventId]        INT NOT NULL,
    [CategoryName]   NVARCHAR(100) NOT NULL,
    [DistanceKm]     DECIMAL(6,2) NOT NULL,
    [Gender]         NVARCHAR(10) NULL,
    [MinAgeYears]    INT NULL,
    [MaxAgeYears]    INT NULL,
    [StartTime]      TIME(0) NULL,
    [CutoffHours]    DECIMAL(4,2) NULL,
    [EntryFeeZar]    DECIMAL(9,2) NOT NULL
        CONSTRAINT [DF_Categories_EntryFeeZar] DEFAULT (0),
    CONSTRAINT [PK_EventCategories] PRIMARY KEY CLUSTERED ([CategoryId] ASC),
    CONSTRAINT [UK_Category_Per_Event] UNIQUE NONCLUSTERED ([EventId] ASC, [CategoryName] ASC),
    CONSTRAINT [CK_Categories_Gender] CHECK ([Gender] IS NULL OR [Gender] IN ('Male','Female','Open','Mixed')),
    CONSTRAINT [FK_Categories_Events]
        FOREIGN KEY ([EventId]) REFERENCES [dbo].[Events]([EventId])
        ON DELETE CASCADE
);
GO
PRINT 'Table [EventCategories] created.';

/* =========================================================
   6. EventEnrolments  — participant enters an event + category
   ========================================================= */
CREATE TABLE [dbo].[EventEnrolments] (
    [EnrolmentId]   INT IDENTITY(1,1) NOT NULL,
    [ParticipantId] INT NOT NULL,
    [EventId]       INT NOT NULL,
    [CategoryId]    INT NOT NULL,
    [EnrolmentDate] DATETIME2(3) NOT NULL
        CONSTRAINT [DF_Enrolments_Date] DEFAULT (SYSDATETIME()),
    [BibNumber]     NVARCHAR(15) NULL,
    [PaymentStatus] NVARCHAR(20) NOT NULL
        CONSTRAINT [DF_Enrolments_Payment] DEFAULT ('Pending'),
    [Status]        NVARCHAR(20) NOT NULL
        CONSTRAINT [DF_Enrolments_Status] DEFAULT ('Registered'),
    [EmergencyNotes] NVARCHAR(500) NULL,
    CONSTRAINT [PK_EventEnrolments] PRIMARY KEY CLUSTERED ([EnrolmentId] ASC),
    CONSTRAINT [UK_Enrolment_UniquePerCat] UNIQUE NONCLUSTERED ([ParticipantId] ASC, [CategoryId] ASC),
    CONSTRAINT [CK_Enrolments_Payment] CHECK ([PaymentStatus] IN
        ('Pending','Paid','Refunded','Cancelled')),
    CONSTRAINT [CK_Enrolments_Status] CHECK ([Status] IN
        ('Registered','Waitlisted','Withdrawn','DidNotStart','Started')),
    CONSTRAINT [FK_Enrolments_Participants]
        FOREIGN KEY ([ParticipantId]) REFERENCES [dbo].[Participants]([ParticipantId])
        ON DELETE NO ACTION,
    CONSTRAINT [FK_Enrolments_Events]
        FOREIGN KEY ([EventId]) REFERENCES [dbo].[Events]([EventId])
        ON DELETE NO ACTION,
    CONSTRAINT [FK_Enrolments_Categories]
        FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[EventCategories]([CategoryId])
        ON DELETE NO ACTION
);
GO
PRINT 'Table [EventEnrolments] created.';

/* =========================================================
   7. Results  — captured by Organiser after event
   ========================================================= */
CREATE TABLE [dbo].[Results] (
    [ResultId]      INT IDENTITY(1,1) NOT NULL,
    [EnrolmentId]   INT NOT NULL,
    [FinishTimeSec] INT NULL,
    [ChipTimeSec]   INT NULL,
    [GunPosition]   INT NULL,
    [CategoryPosition] INT NULL,
    [GenderPosition]   INT NULL,
    [Status]        NVARCHAR(20) NOT NULL
        CONSTRAINT [DF_Results_Status] DEFAULT ('Finished'),
    [Splits]        NVARCHAR(1000) NULL,
    [CapturedAt]    DATETIME2(3) NOT NULL
        CONSTRAINT [DF_Results_CapturedAt] DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_Results] PRIMARY KEY CLUSTERED ([ResultId] ASC),
    CONSTRAINT [UK_Results_PerEnrolment] UNIQUE NONCLUSTERED ([EnrolmentId] ASC),
    CONSTRAINT [CK_Results_Status] CHECK ([Status] IN
        ('Finished','DNF','DNS','DQ')),
    CONSTRAINT [FK_Results_Enrolments]
        FOREIGN KEY ([EnrolmentId]) REFERENCES [dbo].[EventEnrolments]([EnrolmentId])
        ON DELETE CASCADE
);
GO
PRINT 'Table [Results] created.';

/* =========================================================
   8. WeatherSnapshots  — support "live weather" feature
   ========================================================= */
CREATE TABLE [dbo].[WeatherSnapshots] (
    [SnapshotId]    INT IDENTITY(1,1) NOT NULL,
    [EventId]       INT NOT NULL,
    [SnapshotTime]  DATETIME2(3) NOT NULL,
    [TemperatureC]  DECIMAL(4,1) NULL,
    [HumidityPct]   INT NULL,
    [WindKmh]       DECIMAL(5,1) NULL,
    [Condition]     NVARCHAR(50) NULL,
    [Source]        NVARCHAR(50) NULL,
    CONSTRAINT [PK_WeatherSnapshots] PRIMARY KEY CLUSTERED ([SnapshotId] ASC),
    CONSTRAINT [FK_Weather_Events]
        FOREIGN KEY ([EventId]) REFERENCES [dbo].[Events]([EventId])
        ON DELETE CASCADE
);
GO
PRINT 'Table [WeatherSnapshots] created.';

/* =========================================================
   INDEXES for performance
   ========================================================= */
CREATE NONCLUSTERED INDEX [IX_Events_OrganiserId] ON [dbo].[Events]([OrganiserId]);
CREATE NONCLUSTERED INDEX [IX_Events_StartDate]   ON [dbo].[Events]([StartDate]);
CREATE NONCLUSTERED INDEX [IX_Categories_EventId]  ON [dbo].[EventCategories]([EventId]);
CREATE NONCLUSTERED INDEX [IX_Enrolments_Participant] ON [dbo].[EventEnrolments]([ParticipantId]);
CREATE NONCLUSTERED INDEX [IX_Enrolments_Event]     ON [dbo].[EventEnrolments]([EventId]);
CREATE NONCLUSTERED INDEX [IX_Enrolments_Category]  ON [dbo].[EventEnrolments]([CategoryId]);
CREATE NONCLUSTERED INDEX [IX_Results_Enrolment]    ON [dbo].[Results]([EnrolmentId]);
CREATE NONCLUSTERED INDEX [IX_Weather_Event]        ON [dbo].[WeatherSnapshots]([EventId]);
GO
PRINT 'Non-clustered indexes created.';

/* =========================================================
   VIEWS  (convenient for Part 2 / Part 3 reporting)
   ========================================================= */
EXEC ('CREATE VIEW [dbo].[vEventSummary] AS
SELECT
    e.EventId,
    e.Title,
    e.EventType,
    e.City + '', '' + e.Province AS [Location],
    e.StartDate,
    e.Status,
    o.CompanyName AS Organiser,
    COUNT(DISTINCT en.EnrolmentId) AS Entrants
FROM dbo.Events e
JOIN dbo.Organisers o ON o.OrganiserId = e.OrganiserId
LEFT JOIN dbo.EventEnrolments en ON en.EventId = e.EventId
GROUP BY e.EventId, e.Title, e.EventType, e.City, e.Province, e.StartDate, e.Status, o.CompanyName;');
GO

EXEC ('CREATE VIEW [dbo].[vParticipantHistory] AS
SELECT
    p.ParticipantId,
    u.FullName,
    e.EventId,
    e.Title AS EventName,
    ec.CategoryName,
    en.EnrolmentDate,
    en.Status AS EnrolmentStatus,
    r.Status   AS RaceStatus,
    r.FinishTimeSec,
    r.GunPosition
FROM dbo.Participants p
JOIN dbo.Users u ON u.UserId = p.UserId
JOIN dbo.EventEnrolments en ON en.ParticipantId = p.ParticipantId
JOIN dbo.Events e        ON e.EventId   = en.EventId
JOIN dbo.EventCategories ec ON ec.CategoryId = en.CategoryId
LEFT JOIN dbo.Results r  ON r.EnrolmentId = en.EnrolmentId;');
GO
PRINT 'Views created.';

/* =========================================================
   SEED DATA
   ========================================================= */
PRINT '=========================================';
PRINT '  Inserting seed data...';
PRINT '=========================================';

SET IDENTITY_INSERT [dbo].[Users] ON;
INSERT [dbo].[Users] ([UserId],[Email],[PasswordHash],[Role],[FullName],[PhoneNumber],[Province],[CreatedAt]) VALUES
 (1, N'tebogo.moagi@soulcityevents.co.za', N'hashed|AEn8Pq...Part1Demo', N'Organiser',   N'Tebogo Moagi',       N'082 345 6710', N'Gauteng',    '2026-06-01 09:00:00')
,(2, N'anika.vanheerden@ctcyclingtours.co.za', N'hashed|ALr7Xm...Part1Demo', N'Organiser',   N'Anika van Heerden',  N'074 211 3300', N'Western Cape', '2026-06-01 09:05:00')
,(3, N'sbusiso.ndlovu@mweb.co.za',      N'hashed|Am9Kp...Part1Demo',  N'Participant', N'Sibusiso Ndlovu',    N'079 123 4567', N'KwaZulu-Natal', '2026-06-03 10:12:00')
,(4, N'leigh-anne.botes@gmail.com',     N'hashed|AZx41...Part1Demo',  N'Participant', N'Leigh-Anne Botes',   N'083 991 0227', N'Eastern Cape', '2026-06-04 14:40:00')
;
SET IDENTITY_INSERT [dbo].[Users] OFF;
GO

SET IDENTITY_INSERT [dbo].[Organisers] ON;
INSERT [dbo].[Organisers] ([OrganiserId],[UserId],[CompanyName],[CompanyRegNo],[ContactEmail],[IsVerified]) VALUES
 (1, 1, N'Soul City Events (Pty) Ltd',     N'2018/012345/07', N'reg@soulcityevents.co.za',      1)
,(2, 2, N'Cape Town Cycling Tours CC',     N'2021/998877/23', N'entries@ctcyclingtours.co.za',  1)
;
SET IDENTITY_INSERT [dbo].[Organisers] OFF;
GO

SET IDENTITY_INSERT [dbo].[Participants] ON;
INSERT [dbo].[Participants] ([ParticipantId],[UserId],[DateOfBirth],[Gender],[EmergencyContactName],[EmergencyContactNo],[TshirtSize]) VALUES
 (1, 3, '1992-05-14', N'Male',   N'Thando Ndlovu', N'071 303 0908', N'L')
,(2, 4, '1997-11-02', N'Female', N'Michael Botes', N'082 711 4560', N'M')
;
SET IDENTITY_INSERT [dbo].[Participants] OFF;
GO

SET IDENTITY_INSERT [dbo].[Events] ON;
INSERT [dbo].[Events] ([EventId],[OrganiserId],[Title],[EventType],[Description],[StartDate],[EndDate],[City],[Province],[RouteUrl],[EntryFeeZar],[MaxEntries],[RegistrationOpen],[RegistrationClose],[Status],[CreatedAt]) VALUES
 (1, 1, N'94th Comrades Marathon 2026',   N'Marathon',
    N'The Ultimate Human Race — Down Run from Pietermaritzburg to Durban (89 km).',
    '2026-06-14 05:30:00','2026-06-14 18:00:00',
    N'Durban',  N'KwaZulu-Natal',
    N'https://routelink.comrades.co.za/2026-down-run', 600.00, 25000,
    '2026-01-15 00:00:00','2026-05-10 23:59:00',
    N'Completed', '2026-06-02 11:00:00')
,(2, 2, N'Cape Town Cycle Tour 2026',      N'Cycle',
    N'109 km around the Cape Peninsula — the largest individually timed cycling event in the world.',
    '2026-03-08 06:15:00','2026-03-08 16:00:00',
    N'Cape Town', N'Western Cape',
    N'https://routelink.cycletour.co.za/2026', 520.00, 35000,
    '2025-11-01 00:00:00','2026-02-20 23:59:00',
    N'Completed', '2026-06-02 11:05:00')
,(3, 1, N'Soweto Marathon 2026',           N'Marathon',
    N'42.2 km through Soweto — the People''s Race, including 10 km fun run.',
    '2026-11-01 06:00:00','2026-11-01 17:00:00',
    N'Soweto', N'Gauteng',
    N'https://routelink.sowetomarathon.co.za/2026', 450.00, 20000,
    '2026-07-01 00:00:00','2026-10-15 23:59:00',
    N'Open',      '2026-06-02 11:10:00')
;
SET IDENTITY_INSERT [dbo].[Events] OFF;
GO

SET IDENTITY_INSERT [dbo].[EventCategories] ON;
INSERT [dbo].[EventCategories] ([CategoryId],[EventId],[CategoryName],[DistanceKm],[Gender],[MinAgeYears],[MaxAgeYears],[StartTime],[CutoffHours],[EntryFeeZar]) VALUES
-- Comrades
 (1, 1, N'Comrades Open Men',        89.00, N'Male',   20, NULL, '05:30:00', 12.00, 600.00)
,(2, 1, N'Comrades Open Women',      89.00, N'Female', 20, NULL, '05:30:00', 12.00, 600.00)
,(3, 1, N'Comrades Veterans Men',   89.00, N'Male',   40, NULL, '05:30:00', 12.00, 600.00)
-- CT Cycle Tour
,(4, 2, N'CTCT Open',               109.00, N'Open',   16, NULL, '06:15:00',  7.00, 520.00)
,(5, 2, N'CTCT Ladies',             109.00, N'Female', 16, NULL, '06:15:00',  7.00, 520.00)
-- Soweto
,(6, 3, N'Soweto Marathon Men',     42.20, N'Male',   18, NULL, '06:00:00',  6.00, 450.00)
,(7, 3, N'Soweto Marathon Women',   42.20, N'Female', 18, NULL, '06:00:00',  6.00, 450.00)
,(8, 3, N'Soweto 10km Fun Run',     10.00, N'Open',   12, NULL, '07:00:00',  2.50, 120.00)
;
SET IDENTITY_INSERT [dbo].[EventCategories] OFF;
GO

SET IDENTITY_INSERT [dbo].[EventEnrolments] ON;
INSERT [dbo].[EventEnrolments] ([EnrolmentId],[ParticipantId],[EventId],[CategoryId],[EnrolmentDate],[BibNumber],[PaymentStatus],[Status],[EmergencyNotes]) VALUES
-- Comrades — seed enrolments (2026 completed)
 (1, 1, 1, 1, '2026-02-04 07:45:00', N'12742', N'Paid',      N'Withdrawn',     N'Asthmatic')
,(2, 2, 1, 2, '2026-02-05 10:02:00', N'08123', N'Paid',      N'Started',       NULL)
-- CT Cycle Tour — seed enrolments
,(3, 1, 2, 4, '2025-12-10 09:20:00', N'C2211', N'Paid',      N'DidNotStart',   NULL)
,(4, 2, 2, 5, '2026-01-11 18:31:00', N'C0458', N'Paid',      N'Registered',    NULL)
-- Soweto — seed enrolments (Open for entry)
,(5, 1, 3, 6, '2026-07-12 11:00:00', NULL,     N'Pending',   N'Registered',    N'Vegetarian meal')
,(6, 2, 3, 8, '2026-07-15 09:00:00', NULL,     N'Paid',      N'Registered',    NULL)
;
SET IDENTITY_INSERT [dbo].[EventEnrolments] OFF;
GO

SET IDENTITY_INSERT [dbo].[Results] ON;
INSERT [dbo].[Results] ([ResultId],[EnrolmentId],[FinishTimeSec],[ChipTimeSec],[GunPosition],[CategoryPosition],[GenderPosition],[Status],[Splits],[CapturedAt]) VALUES
 (1, 1, NULL,      NULL,      NULL,  NULL, NULL, N'DNF',     N'42km reached in 04:50:00 — stopped at Drummond', '2026-06-14 14:10:00')
,(2, 2, 40680,     40608,     876,   112,  876,  N'Finished',N'21km:03:05:20; 42km:06:21:10; 60km:08:45:50; End:11:18:00', '2026-06-14 18:02:00')
;
SET IDENTITY_INSERT [dbo].[Results] OFF;
GO

SET IDENTITY_INSERT [dbo].[WeatherSnapshots] ON;
INSERT [dbo].[WeatherSnapshots] ([SnapshotId],[EventId],[SnapshotTime],[TemperatureC],[HumidityPct],[WindKmh],[Condition],[Source]) VALUES
 (1, 1, '2026-06-14 05:00:00',  11.2, 88,  9.0,  N'Light mist',   N'SAWS Pietermaritzburg')
,(2, 1, '2026-06-14 10:00:00',  18.0, 65, 14.0,  N'Sunny',        N'SAWS Hillcrest')
,(3, 1, '2026-06-14 14:00:00',  22.5, 52, 21.0,  N'Hot & sunny',  N'SAWS Durban South')
,(4, 3, '2026-11-01 06:00:00',  17.0, NULL, 12.0, N'Clear',        N'[Live forecast — race day]')
;
SET IDENTITY_INSERT [dbo].[WeatherSnapshots] OFF;
GO

/* =========================================================
   REPORT — counts and sample data
   ========================================================= */
PRINT '';
PRINT '----- Seed data summary ------';
SELECT 'Users'         AS Tbl, COUNT(*) AS Rows FROM [dbo].[Users]         UNION ALL
SELECT 'Organisers'    AS Tbl, COUNT(*) AS Rows FROM [dbo].[Organisers]    UNION ALL
SELECT 'Participants'  AS Tbl, COUNT(*) AS Rows FROM [dbo].[Participants]  UNION ALL
SELECT 'Events'        AS Tbl, COUNT(*) AS Rows FROM [dbo].[Events]        UNION ALL
SELECT 'Categories'    AS Tbl, COUNT(*) AS Rows FROM [dbo].[EventCategories] UNION ALL
SELECT 'Enrolments'    AS Tbl, COUNT(*) AS Rows FROM [dbo].[EventEnrolments] UNION ALL
SELECT 'Results'       AS Tbl, COUNT(*) AS Rows FROM [dbo].[Results]       UNION ALL
SELECT 'Weather'       AS Tbl, COUNT(*) AS Rows FROM [dbo].[WeatherSnapshots];
GO

PRINT '';
PRINT '==================================================';
PRINT '  RaceDay Part 1 script completed successfully.';
PRINT '==================================================';
GO
