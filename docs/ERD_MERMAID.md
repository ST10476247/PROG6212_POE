```mermaid
erDiagram
    USERS {
        int UserId PK "IDENTITY(1,1)"
        nvarchar(255) Email UK "NOT NULL"
        nvarchar(255) PasswordHash "NOT NULL"
        nvarchar(20) Role "CK ∈ {Organiser, Participant}"
        nvarchar(150) FullName "NOT NULL"
        nvarchar(20) PhoneNumber
        nvarchar(50) Province
        datetime2(3) CreatedAt "DF = SYSDATETIME()"
    }

    ORGANISERS {
        int OrganiserId PK "IDENTITY(1,1)"
        int UserId PK,FK "1:1 → Users.UserId  (CASCADE DELETE)"
        nvarchar(200) CompanyName "NOT NULL"
        nvarchar(30) CompanyRegNo
        nvarchar(255) ContactEmail "NOT NULL"
        bit IsVerified "DF = 0"
    }

    PARTICIPANTS {
        int ParticipantId PK "IDENTITY(1,1)"
        int UserId PK,FK "1:1 → Users.UserId  (CASCADE DELETE)"
        date DateOfBirth "NOT NULL"
        nvarchar(10) Gender
        nvarchar(150) EmergencyContactName
        nvarchar(20) EmergencyContactNo
        nvarchar(5) TshirtSize
    }

    EVENTS {
        int EventId PK "IDENTITY(1,1)"
        int OrganiserId FK "N:1 → Organisers.OrganiserId"
        nvarchar(200) Title "NOT NULL"
        nvarchar(30) EventType "CK ∈ Marathon, HalfMarathon, 10km, 5km, Cycle, Trail, Walk, ParkRun, Other"
        nvarchar(2000) Description
        datetime2(0) StartDate "NOT NULL"
        datetime2(0) EndDate
        nvarchar(100) City "NOT NULL"
        nvarchar(50) Province "NOT NULL"
        nvarchar(500) RouteUrl
        decimal(9,2) EntryFeeZar "DF = 0"
        int MaxEntries
        datetime2(0) RegistrationOpen
        datetime2(0) RegistrationClose
        nvarchar(20) Status "CK ∈ Draft, Open, Closed, Completed, Cancelled; DF=Draft"
        datetime2(3) CreatedAt "DF = SYSDATETIME()"
    }

    EVENT_CATEGORIES {
        int CategoryId PK "IDENTITY(1,1)"
        int EventId FK "N:1 → Events.EventId (CASCADE DELETE)"
        nvarchar(100) CategoryName "UK(EventId,CategoryName)"
        decimal(6,2) DistanceKm "NOT NULL"
        nvarchar(10) Gender "CK ∈ Male, Female, Open, Mixed; NULLABLE"
        int MinAgeYears
        int MaxAgeYears
        time(0) StartTime
        decimal(4,2) CutoffHours
        decimal(9,2) EntryFeeZar "DF = 0"
    }

    EVENT_ENROLMENTS {
        int EnrolmentId PK "IDENTITY(1,1)"
        int ParticipantId FK "N:1 → Participants.ParticipantId  (UK: ParticipantId + CategoryId)"
        int EventId FK "N:1 → Events.EventId"
        int CategoryId FK "N:1 → EventCategories.CategoryId"
        datetime2(3) EnrolmentDate "DF = SYSDATETIME()"
        nvarchar(15) BibNumber
        nvarchar(20) PaymentStatus "CK ∈ Pending, Paid, Refunded, Cancelled; DF=Pending"
        nvarchar(20) Status "CK ∈ Registered, Waitlisted, Withdrawn, DidNotStart, Started; DF=Registered"
        nvarchar(500) EmergencyNotes
    }

    RESULTS {
        int ResultId PK "IDENTITY(1,1)"
        int EnrolmentId UK,FK "1:1 → EventEnrolments.EnrolmentId (CASCADE DELETE)"
        int FinishTimeSec "NULLABLE (DNF/DNS/DQ)"
        int ChipTimeSec
        int GunPosition
        int CategoryPosition
        int GenderPosition
        nvarchar(20) Status "CK ∈ Finished, DNF, DNS, DQ; DF=Finished"
        nvarchar(1000) Splits "semicolon-separated split times"
        datetime2(3) CapturedAt "DF = SYSDATETIME()"
    }

    WEATHER_SNAPSHOTS {
        int SnapshotId PK "IDENTITY(1,1)"
        int EventId FK "N:1 → Events.EventId (CASCADE DELETE)"
        datetime2(3) SnapshotTime "NOT NULL"
        decimal(4,1) TemperatureC
        int HumidityPct
        decimal(5,1) WindKmh
        nvarchar(50) Condition
        nvarchar(50) Source
    }

    USERS ||--o| ORGANISERS : "is extended to (1:1)"
    USERS ||--o| PARTICIPANTS : "is extended to (1:1)"

    ORGANISERS ||--o{ EVENTS : "owns (1:N)"
    EVENTS ||--o{ EVENT_CATEGORIES : "has categories (1:N)"
    EVENTS ||--o{ WEATHER_SNAPSHOTS : "has weather (1:N)"

    PARTICIPANTS ||--o{ EVENT_ENROLMENTS : "enrols into (1:N)"
    EVENTS ||--o{ EVENT_ENROLMENTS : "attracts enrolments (1:N)"
    EVENT_CATEGORIES ||--o{ EVENT_ENROLMENTS : "categorises enrolments (1:N)"

    EVENT_ENROLMENTS ||--o| RESULTS : "produces a result (1:0..1)"
```
