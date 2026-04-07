# Chapter 5 – Database Design

## 5.1 Overview

The Uni-Connect system uses a relational database (SQL Server) managed through Entity Framework Core.
The database contains **11 tables** that together support user management, community posts, answers,
private tutoring sessions, notifications, and moderation.

Soft-delete is applied across all major tables via an `IsDeleted` flag; a global query filter
automatically excludes deleted rows from every query.

---

## 5.2 Database Tables

### 5.2.1 Users

Stores every registered account (students, helpers, and admins).

| Column | Data Type | Constraints | Description |
|---|---|---|---|
| UserID | int | PK, Identity | Unique user identifier |
| UniversityID | nvarchar(max) | NOT NULL | University student/staff ID |
| Name | nvarchar(max) | NOT NULL | Full display name |
| Username | nvarchar(max) | NOT NULL | Unique login handle |
| Email | nvarchar(max) | NOT NULL | Email address |
| PasswordHash | nvarchar(max) | NOT NULL | Bcrypt-hashed password |
| Role | nvarchar(max) | NOT NULL | User role: Student / Helper / Admin |
| Points | int | NOT NULL | Reputation points earned |
| IsDeleted | bit | NOT NULL, Default 0 | Soft-delete flag |
| ProfileImageUrl | nvarchar(max) | NULL | URL to profile picture |
| CreatedAt | datetime2 | NOT NULL | Account creation timestamp |
| LastLoginAt | datetime2 | NULL | Last successful login timestamp |
| PasswordResetToken | nvarchar(max) | NULL | Token for password-reset flow |
| PasswordResetTokenExpiry | datetime2 | NULL | Expiry time of the reset token |
| FailedLoginAttempts | int | NOT NULL, Default 0 | Counter for consecutive failed logins |
| AccountLockedUntil | datetime2 | NULL | Timestamp until account is locked |

---

### 5.2.2 Categories

Defines the academic categories that posts can be filed under.

| Column | Data Type | Constraints | Description |
|---|---|---|---|
| CategoryID | int | PK, Identity | Unique category identifier |
| Name | nvarchar(max) | NOT NULL | Category name (e.g., Mathematics) |
| Faculty | nvarchar(max) | NOT NULL | Faculty this category belongs to |

---

### 5.2.3 Tags

Keyword tags that can be attached to posts for better discoverability.

| Column | Data Type | Constraints | Description |
|---|---|---|---|
| TagID | int | PK, Identity | Unique tag identifier |
| Name | nvarchar(max) | NOT NULL | Tag label (e.g., Calculus, OOP) |

---

### 5.2.4 Posts

Community questions or discussion threads created by users.

| Column | Data Type | Constraints | Description |
|---|---|---|---|
| PostID | int | PK, Identity | Unique post identifier |
| UserID | int | FK → Users(UserID), NOT NULL | Author of the post |
| CategoryID | int | FK → Categories(CategoryID), NOT NULL | Category the post belongs to |
| Title | nvarchar(max) | NOT NULL | Post headline |
| Content | nvarchar(max) | NOT NULL | Full post body |
| ViewsCount | int | NOT NULL | Number of times the post was viewed |
| Upvotes | int | NOT NULL | Total upvotes received |
| CreatedAt | datetime2 | NOT NULL | Timestamp when post was created |
| IsDeleted | bit | NOT NULL, Default 0 | Soft-delete flag |

**Foreign Key Constraints:**
- `FK_Posts_Users_UserID` → Users(UserID) — NO ACTION on delete
- `FK_Posts_Categories_CategoryID` → Categories(CategoryID) — CASCADE on delete

---

### 5.2.5 Answers

Replies submitted by users in response to a post.

| Column | Data Type | Constraints | Description |
|---|---|---|---|
| AnswerID | int | PK, Identity | Unique answer identifier |
| PostID | int | FK → Posts(PostID), NOT NULL | The post being answered |
| UserID | int | FK → Users(UserID), NOT NULL | Author of the answer |
| Content | nvarchar(max) | NOT NULL | Answer body text |
| IsAccepted | bit | NOT NULL | Whether the post owner accepted this answer |
| Upvotes | int | NOT NULL | Total upvotes received |
| CreatedAt | datetime2 | NOT NULL | Timestamp when answer was created |
| IsDeleted | bit | NOT NULL, Default 0 | Soft-delete flag |

**Foreign Key Constraints:**
- `FK_Answers_Posts_PostID` → Posts(PostID) — CASCADE on delete
- `FK_Answers_Users_UserID` → Users(UserID) — NO ACTION on delete

---

### 5.2.6 PostTags

Junction table that links posts to their tags (many-to-many).

| Column | Data Type | Constraints | Description |
|---|---|---|---|
| PostID | int | PK (composite), FK → Posts(PostID) | Reference to the post |
| TagID | int | PK (composite), FK → Tags(TagID) | Reference to the tag |

**Primary Key:** Composite `(PostID, TagID)`

**Foreign Key Constraints:**
- `FK_PostTags_Posts_PostID` → Posts(PostID) — NO ACTION on delete
- `FK_PostTags_Tags_TagID` → Tags(TagID) — CASCADE on delete

---

### 5.2.7 Requests

A user's formal request to have a post answered in a private tutoring session.

| Column | Data Type | Constraints | Description |
|---|---|---|---|
| RequestID | int | PK, Identity | Unique request identifier |
| OwnerID | int | FK → Users(UserID), NOT NULL | User who created the request |
| PostID | int | FK → Posts(PostID), NOT NULL | Post the request is linked to |
| Description | nvarchar(max) | NOT NULL | Additional context or requirements |
| Status | nvarchar(max) | NOT NULL | Request state: Open / Accepted / Closed |
| CreatedAt | datetime2 | NOT NULL | Timestamp when request was created |
| IsDeleted | bit | NOT NULL, Default 0 | Soft-delete flag |

**Foreign Key Constraints:**
- `FK_Requests_Users_OwnerID` → Users(UserID) — NO ACTION on delete
- `FK_Requests_Posts_PostID` → Posts(PostID) — CASCADE on delete

---

### 5.2.8 PrivateSessions

A real-time private messaging session between a student and a helper, created from an accepted request.

| Column | Data Type | Constraints | Description |
|---|---|---|---|
| PrivateSessionID | int | PK, Identity | Unique session identifier |
| RequestID | int | FK → Requests(RequestID), NOT NULL, Unique | The originating request |
| StudentID | int | FK → Users(UserID), NOT NULL | The student in the session |
| HelperID | int | FK → Users(UserID), NOT NULL | The helper in the session |
| CreatedAt | datetime2 | NOT NULL | Timestamp when session started |
| IsActive | bit | NOT NULL | Whether the session is currently active |
| IsDeleted | bit | NOT NULL, Default 0 | Soft-delete flag |

**Foreign Key Constraints:**
- `FK_PrivateSessions_Requests_RequestID` → Requests(RequestID) — CASCADE on delete
- `FK_PrivateSessions_Users_StudentID` → Users(UserID) — NO ACTION on delete
- `FK_PrivateSessions_Users_HelperID` → Users(UserID) — NO ACTION on delete

**Index:** `IX_PrivateSessions_RequestID` is **unique** (one session per request).

---

### 5.2.9 Messages

Individual chat messages exchanged inside a private session.

| Column | Data Type | Constraints | Description |
|---|---|---|---|
| MessageID | int | PK, Identity | Unique message identifier |
| SessionID | int | FK → PrivateSessions(PrivateSessionID), NOT NULL | Session this message belongs to |
| SenderID | int | FK → Users(UserID), NOT NULL | User who sent the message |
| MessageText | nvarchar(max) | NOT NULL | Text content of the message |
| SentAt | datetime2 | NOT NULL | Timestamp when message was sent |
| IsRead | bit | NOT NULL | Whether the recipient has read the message |
| IsDeleted | bit | NOT NULL, Default 0 | Soft-delete flag |

**Foreign Key Constraints:**
- `FK_Messages_PrivateSessions_SessionID` → PrivateSessions(PrivateSessionID) — CASCADE on delete
- `FK_Messages_Users_SenderID` → Users(UserID) — NO ACTION on delete

---

### 5.2.10 Reports

Abuse/violation reports filed by users against posts.

| Column | Data Type | Constraints | Description |
|---|---|---|---|
| ReportID | int | PK, Identity | Unique report identifier |
| ReporterID | int | FK → Users(UserID), NOT NULL | User who submitted the report |
| PostID | int | FK → Posts(PostID), NOT NULL | Post being reported |
| Reason | nvarchar(max) | NOT NULL | Description of the violation |
| Status | nvarchar(max) | NOT NULL | Review state: Pending / Reviewed |
| IsDeleted | bit | NOT NULL, Default 0 | Soft-delete flag |

**Foreign Key Constraints:**
- `FK_Reports_Users_ReporterID` → Users(UserID) — NO ACTION on delete
- `FK_Reports_Posts_PostID` → Posts(PostID) — CASCADE on delete

---

### 5.2.11 Notifications

In-app notifications delivered to users for various system events.

| Column | Data Type | Constraints | Description |
|---|---|---|---|
| NotificationID | int | PK, Identity | Unique notification identifier |
| UserID | int | FK → Users(UserID), NOT NULL | Recipient of the notification |
| Type | nvarchar(max) | NOT NULL | Event type: Message / Answer / Like / etc. |
| ReferenceID | int | NOT NULL | ID of the related entity (post, message, etc.) |
| IsRead | bit | NOT NULL | Whether the user has read the notification |
| CreatedAt | datetime2 | NOT NULL | Timestamp when notification was created |
| IsDeleted | bit | NOT NULL, Default 0 | Soft-delete flag |

**Foreign Key Constraints:**
- `FK_Notifications_Users_UserID` → Users(UserID) — NO ACTION on delete

---

## 5.3 Relationships Summary

| Relationship | Type | Description |
|---|---|---|
| Users → Posts | One-to-Many | A user can create many posts |
| Users → Answers | One-to-Many | A user can submit many answers |
| Users → Requests | One-to-Many | A user can open many requests |
| Users → Reports | One-to-Many | A user can file many reports |
| Users → Notifications | One-to-Many | A user can receive many notifications |
| Users → PrivateSessions (as Student) | One-to-Many | A user can be a student in many sessions |
| Users → PrivateSessions (as Helper) | One-to-Many | A user can be a helper in many sessions |
| Categories → Posts | One-to-Many | A category groups many posts |
| Posts → Answers | One-to-Many | A post can have many answers |
| Posts → PostTags | One-to-Many | A post can have many tags |
| Posts → Reports | One-to-Many | A post can have many reports |
| Posts → Requests | One-to-Many | A post can have many requests |
| Tags → PostTags | One-to-Many | A tag can be linked to many posts |
| Posts ↔ Tags (via PostTags) | Many-to-Many | A post has multiple tags; a tag applies to multiple posts |
| Requests → PrivateSessions | One-to-One | Each accepted request spawns exactly one session |
| PrivateSessions → Messages | One-to-Many | A session contains many messages |

---

## 5.4 Entity-Relationship Diagram (Textual)

```
Users ──< Posts >── Categories
  │          │
  │          ├──< Answers (by Users)
  │          ├──< PostTags >── Tags
  │          ├──< Reports (by Users)
  │          └──< Requests (by Users)
  │                   │
  │                   └──── PrivateSessions ──< Messages
  │                            (Student: Users)
  │                            (Helper:  Users)
  │
  └──< Notifications
```

---

## 5.5 Database Indexes

| Index Name | Table | Column(s) | Unique |
|---|---|---|---|
| IX_Answers_PostID | Answers | PostID | No |
| IX_Answers_UserID | Answers | UserID | No |
| IX_Messages_SenderID | Messages | SenderID | No |
| IX_Messages_SessionID | Messages | SessionID | No |
| IX_Notifications_UserID | Notifications | UserID | No |
| IX_Posts_CategoryID | Posts | CategoryID | No |
| IX_Posts_UserID | Posts | UserID | No |
| IX_PostTags_TagID | PostTags | TagID | No |
| IX_PrivateSessions_RequestID | PrivateSessions | RequestID | **Yes** |
| IX_PrivateSessions_StudentID | PrivateSessions | StudentID | No |
| IX_PrivateSessions_HelperID | PrivateSessions | HelperID | No |
| IX_Reports_PostID | Reports | PostID | No |
| IX_Reports_ReporterID | Reports | ReporterID | No |
| IX_Requests_OwnerID | Requests | OwnerID | No |
| IX_Requests_PostID | Requests | PostID | No |
