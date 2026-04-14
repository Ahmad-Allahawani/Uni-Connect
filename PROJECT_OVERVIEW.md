# 🎓 UniConnect — Project Overview

**UniConnect** is a professional, high-end community platform designed exclusively for students at **Philadelphia University**. It facilitates peer-to-peer learning through a point-based Q&A system, private study sessions, and a gamified experience.

---

## 🚀 Current Project State
The project has been fully restored and refactored to achieve **100% UI and functional parity** with the reference implementation. The codebase is now clean, dynamic, and follows ASP.NET Core MVC best practices.

### 🛠 Technical Stack
- **Framework**: ASP.NET Core 8.0 (MVC Architectural Pattern)
- **Database**: SQL Server (LocalDB) via Entity Framework Core 9.0
- **Frontend**: Razor Pages, Vanilla CSS (Premium Design System), JavaScript (ES6+)
- **Authentication**: Custom Claims-based Authentication with automatic Dashboard routing.

---

## ✨ Core Features

### 1. High-End Marketing Landing Page
- Professional "Hero" section with vibrant gradients.
- "How it Works" and "Faculties" sections for high-conversion onboarding.
- Automated routing: Guests see the Landing Page; Authenticated members are redirected to the Dashboard.

### 2. Personalized Student Dashboard
- **Dynamic Feed**: Real-time access to the latest questions from the university.
- **Student Profile Integration**: Shows real name, faculty, and points balance fetched from the database.
- **Gamification**: Visual progress bars showing current standing and "Verified Tutor" status.

### 3. Community & Points System
- **Leaderboard**: Global rankings of top student contributors across all faculties.
- **Points Mechanism**:
  - **-10 pts**: For asking a question.
  - **+5 to +15 pts**: For giving a helpful answer.
  - **Rewards**: Progression toward "Verified Tutor" badge and community recognition.

### 4. Robust Student Profiles
- Captures critical data: **Full Name**, **University Email**, **Faculty**, and **Year of Study**.
- Activity tracking: Total points, answers given, and questions asked.

---

## 🗄 Database Schema (Latest Update)
We have unified the database schema to ensure sync between the application and the physical database.
- **Unified_Initial Migration**: Replaced fragmented legacy migrations with a single, stable entry point.
- **Added Fields**: Successfully integrated `Faculty` and `YearOfStudy` into the `User` table to support faculty-specific filtering.

---

## 🚦 How to Run & Maintain
- **Run the App**: `dotnet run --project Uni-Connect\Uni-Connect.csproj`
- **Database Access**: Uses `(localdb)\MSSQLLocalDB` with `Uni-Connect-DB`.
- **Primary Design Reference**: `http://allahawani-001-site1.rtempurl.com/`

---

## 🎯 Graduation Presentation Note
The platform is optimized for visual impact. The design uses the **Plus Jakarta Sans** typography and an **Indigo-Amber** aesthetic to ensure a premium first impression for examiners and students alike.
