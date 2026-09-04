# RaceDay RESTful API Endpoint Plan

## Project Overview
**RaceDay** is a web-based event management platform built specifically for the South African road running, walking, and cycling community. This document outlines the complete RESTful API specification to be implemented in **Part 2** and consumed by the MVC frontend in **Part 3**.

---

## System Roles and Security Principles
The API enforces role-based access control (RBAC) via JSON Web Tokens (JWT) with two primary authenticated roles and public access:
1. **Public / None**: Open to unregistered guests to browse upcoming races, check route landmarks, and view public timing leaderboards.
2. **Participant**: Athletes registered with a valid South African ID or Passport who can browse events, enrol into specific event categories, retrieve entry tickets/bibs, and track their personal race results.
3. **Organiser**: Race Directors and timing officials authorized to create/edit/delete events, configure race categories, view participant rosters, and upload/manage race timing results.
4. **Any (Authenticated)**: Endpoints accessible by any logged-in user with a valid JWT token (e.g., viewing/updating profile details).

---

## Master API Endpoint Specification Table

| HTTP Method | Route | Description | Role Required | Request Body (if any) | Expected Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **AUTHENTICATION & USER MANAGEMENT** | | | | | |
| `POST` | `/api/auth/register` | Registers a new user account (Organiser or Participant) and generates their associated profile. | None (Public) | `{"email": "string", "password": "string", "role": "Participant\|Organiser", "firstName": "string", "lastName": "string", "saIdOrPassport": "string", "gender": "string", "dateOfBirth": "yyyy-MM-dd", "emergencyPhone": "string", "clubName": "string", "organizationName": "string", "phone": "string", "province": "string"}` | **201 Created**: `{"userId": 1, "email": "...", "role": "...", "token": "jwt_token"}`<br>**400 Bad Request**: Validation failure (invalid email/SA ID)<br>**409 Conflict**: Email or SA ID already registered |
| `POST` | `/api/auth/login` | Authenticates user credentials and issues a signed JWT bearer token containing role claims. | None (Public) | `{"email": "string", "password": "string"}` | **200 OK**: `{"token": "jwt_token", "expiresIn": 86400, "role": "Organiser", "userId": 1, "email": "..."}`<br>**400 Bad Request**: Missing email/password<br>**401 Unauthorized**: Invalid credentials |
| **USER PROFILE MANAGEMENT** | | | | | |
| `GET` | `/api/profile/me` | Retrieves the authenticated user's profile information (Organiser or Participant profile). | Any (Logged in) | None | **200 OK**: Complete profile object with demographic/club/organization details<br>**401 Unauthorized**: Token missing or expired |
| `PUT` | `/api/profile/me` | Updates contact numbers, running club affiliation, or organization details for the current user. | Any (Logged in) | `{"phone": "string", "emergencyPhone": "string", "clubName": "string", "organizationName": "string"}` | **200 OK**: Updated profile object<br>**400 Bad Request**: Invalid payload<br>**401 Unauthorized**: Token missing or expired |
| **EVENTS MANAGEMENT** | | | | | |
| `GET` | `/api/events` | Retrieves a paginated list of all upcoming, live, and completed road events across South Africa. | None (Public) | None | **200 OK**: Array of event summary objects with categories, dates, and locations |
| `GET` | `/api/events/{id}` | Retrieves full event details, including route elevation, key landmarks, categories, and weather summary. | None (Public) | None | **200 OK**: Detailed event object with embedded categories and route info<br>**404 Not Found**: Event ID does not exist |
| `POST` | `/api/events` | Creates a new road running, cycling, or walking event in the system. | Organiser | `{"eventName": "string", "eventType": "Running\|Cycling\|Walking", "eventDate": "yyyy-MM-ddTHH:mm:ss", "location": "string", "province": "string", "description": "string", "bannerUrl": "string", "elevationGainMeters": 1250, "keyLandmarks": "string", "terrainType": "string"}` | **201 Created**: Created event entity with assigned `eventId`<br>**400 Bad Request**: Validation error<br>**401 Unauthorized**: Unauthenticated<br>**403 Forbidden**: User is not an Organiser |
| `PUT` | `/api/events/{id}` | Updates existing event information, dates, status, or route details. | Organiser | `{"eventName": "string", "eventType": "Running\|Cycling\|Walking", "eventDate": "yyyy-MM-ddTHH:mm:ss", "location": "string", "province": "string", "description": "string", "status": "Upcoming\|Live\|Completed", "bannerUrl": "string", "elevationGainMeters": 1250, "keyLandmarks": "string", "terrainType": "string"}` | **200 OK**: Updated event object<br>**400 Bad Request**: Validation failure<br>**401 Unauthorized**: Unauthenticated<br>**403 Forbidden**: Organiser does not own this event<br>**404 Not Found**: Event ID not found |
| `DELETE` | `/api/events/{id}` | Removes an event and cascades deletion to categories and unconfirmed entries. | Organiser | None | **204 No Content**: Event deleted successfully<br>**401 Unauthorized**: Unauthenticated<br>**403 Forbidden**: Organiser does not own this event<br>**404 Not Found**: Event not found<br>**409 Conflict**: Cannot delete event with active paid entries |
| **EVENT CATEGORIES MANAGEMENT** | | | | | |
| `GET` | `/api/events/{eventId}/categories` | Retrieves all race distance categories, start times, entry fees, and remaining capacities for an event. | None (Public) | None | **200 OK**: Array of category objects for the event<br>**404 Not Found**: Event ID does not exist |
| `POST` | `/api/events/{eventId}/categories` | Adds a new distance category (e.g., 89km Ultra, 42.2km, 21.1km, 10km) to an event. | Organiser | `{"categoryName": "string", "distanceKm": 42.2, "entryFeeZAR": 450.00, "maxCapacity": 10000, "startTime": "06:00:00", "cutoffHours": 6.0}` | **201 Created**: Created category object with `categoryId`<br>**400 Bad Request**: Invalid distance or fee<br>**401 Unauthorized**: Unauthenticated<br>**403 Forbidden**: Forbidden<br>**404 Not Found**: Event not found |
| `PUT` | `/api/categories/{id}` | Updates distance, entry fee, start time, or cutoff duration for a specific race category. | Organiser | `{"categoryName": "string", "distanceKm": 42.2, "entryFeeZAR": 450.00, "maxCapacity": 10000, "startTime": "06:00:00", "cutoffHours": 6.0}` | **200 OK**: Updated category object<br>**400 Bad Request**: Validation error<br>**404 Not Found**: Category ID not found |
| `DELETE` | `/api/categories/{id}` | Deletes a race category if no participant entries are linked to it. | Organiser | None | **204 No Content**: Category deleted successfully<br>**404 Not Found**: Category not found<br>**409 Conflict**: Category contains registered participants |
| **EVENT ENROLMENTS / ENTRIES** | | | | | |
| `POST` | `/api/enrolments` | Enrols the logged-in participant into a chosen race category, generates a bib number, and issues a payment reference. | Participant | `{"categoryId": 1, "medicalNotes": "string"}` | **201 Created**: Entry receipt object with `entryId`, `bibNumber`, and `paymentReference`<br>**400 Bad Request**: Capacity full or category closed<br>**401 Unauthorized**: Unauthenticated<br>**403 Forbidden**: User is not a Participant<br>**409 Conflict**: Participant already registered for this category |
| `GET` | `/api/enrolments/my` | Retrieves all event enrolments and entry tickets for the logged-in participant. | Participant | None | **200 OK**: Array of participant enrolment history with category and event details<br>**401 Unauthorized**: Unauthenticated |
| `GET` | `/api/enrolments/{id}` | Retrieves a single enrolment receipt, bib number details, and payment verification status. | Any (Logged in) | None | **200 OK**: Enrolment detail object<br>**401 Unauthorized**: Unauthenticated<br>**403 Forbidden**: User cannot view another participant's enrolment<br>**404 Not Found**: Enrolment ID not found |
| `GET` | `/api/events/{eventId}/enrolments` | Retrieves all participant enrolments, bib allocations, and medical alerts for an event (roster view). | Organiser | None | **200 OK**: Array of participant entries registered for the event<br>**401 Unauthorized**: Unauthenticated<br>**403 Forbidden**: Organiser does not own this event<br>**404 Not Found**: Event ID not found |
| **RACE RESULTS & TIMING** | | | | | |
| `GET` | `/api/events/{eventId}/results` | Retrieves public race results, gun times, chip times, and leaderboard ranks for an event. | None (Public) | None | **200 OK**: Array of result records sorted by category rank and chip time<br>**404 Not Found**: Event ID not found |
| `GET` | `/api/results/my` | Retrieves personal performance history, chip times, and rank progression across all events for the logged-in participant. | Participant | None | **200 OK**: Array of personal race results with comparative statistics<br>**401 Unauthorized**: Unauthenticated |
| `POST` | `/api/results` | Captures or imports an official timing result for a registered participant entry. | Organiser | `{"entryId": 1, "gunTime": "06:14:22.450", "chipTime": "06:13:58.120", "overallRank": 48, "categoryRank": 12, "genderRank": 42, "status": "Finished"}` | **201 Created**: Created result record with `resultId`<br>**400 Bad Request**: Invalid timing or status<br>**401 Unauthorized**: Unauthenticated<br>**403 Forbidden**: Forbidden<br>**409 Conflict**: Result already recorded for this entry |
| `PUT` | `/api/results/{id}` | Updates a timing record or adjusts disqualification/DNF status. | Organiser | `{"gunTime": "06:14:22.450", "chipTime": "06:13:58.120", "overallRank": 48, "categoryRank": 12, "genderRank": 42, "status": "Finished\|DNF\|DNS\|Disqualified"}` | **200 OK**: Updated result record<br>**400 Bad Request**: Validation error<br>**404 Not Found**: Result ID not found |
| **LIVE WEATHER & ROUTE INFORMATION** | | | | | |
| `GET` | `/api/events/{id}/weather` | Retrieves real-time simulated weather forecast and race-day advice based on event geographic location. | None (Public) | None | **200 OK**: `{"location": "Pietermaritzburg", "temperatureC": 18, "condition": "Sunny", "humidity": 65, "windSpeedKmH": 12, "advice": "Hydrate well at Inchanga water stations."}`<br>**404 Not Found**: Event ID not found |
| `GET` | `/api/events/{id}/route` | Retrieves route elevation profile, terrain details, water point distances, and key South African landmarks. | None (Public) | None | **200 OK**: `{"elevationGainM": 1250, "terrain": "Road / Tarmac", "keyLandmarks": ["Polly Shorts", "Botha's Hill", "Inchanga"]}`<br>**404 Not Found**: Event ID not found |

---

## Standard Error Response Format
All non-2xx HTTP responses follow the RFC 7807 Problem Details specification:
```json
{
  "type": "https://raceday.co.za/errors/validation-error",
  "title": "Bad Request",
  "status": 400,
  "detail": "Participant is already registered for this category.",
  "instance": "/api/enrolments",
  "timestamp": "2026-09-04T20:40:00Z"
}
```
