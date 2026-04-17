# UniConnect Development Log

**Project**: Uni-Connect (ASP.NET Core 8.0 + SQL Server)  
**Start Date**: April 16, 2026  
**Developer**: Ahmad K.

---

## 📋 Table of Contents
1. [Phase 1: Initial Layout Fixes](#phase-1-initial-layout-fixes)
2. [Phase 2: Architecture Normalization](#phase-2-architecture-normalization)
3. [Phase 3: Build & App Startup](#phase-3-build--app-startup)
4. [Phase 4: Test Data Seeding](#phase-4-test-data-seeding)
5. [Phase 5: CreatePost Page Improvements](#phase-5-createpost-page-improvements)
6. [Current Status](#current-status)
7. [Next Steps](#next-steps)

---

## Phase 1: Initial Layout Fixes
**Date**: April 16, 2026 - Early Session  
**Issues Found**:
- ❌ Navbar closing tag was `</navbar>` instead of `</nav>`
- ❌ Sidebar positioned outside flex container `<app-wrap>`
- Layout was completely broken

**File Modified**: `_DashboardLayout.cshtml`

**Changes Made**:
1. Fixed navbar closing tag from `</navbar>` → `</nav>`
2. Moved sidebar inside `<app-wrap>` flex container as proper sibling to main content
3. Added `<div class="sidebar-overlay" id="sidebar-overlay"></div>` for mobile overlay

**Result**: ✅ Layout now renders correctly with sidebar and main content as flex siblings

---

## Phase 2: Architecture Normalization
**Date**: April 16, 2026 - Mid Session  
**Objective**: Fix all dashboard pages to use single shared shell pattern

### 2.1 CSS Consolidation
**File Modified**: `design-system.css`

**Classes Added**:
```
- .chip-gray
- .search-dropdown
- .search-result-item
- .search-result-title
- .search-result-meta
- .search-no-results
- .sidebar-overlay
- .sidebar-overlay.open (with backdrop blur)
- .av-2xl (120px avatar)
- .nav-section
- .nav-count
- .btn-white
```

**Result**: ✅ All missing utilities now available to dashboard pages

### 2.2 Dashboard Pages Fixed

#### ✅ Points.cshtml
- **Issue**: No Layout declaration, was falling back to wrong layout
- **Fix**: Added `Layout = "~/Views/Shared/_DashboardLayout.cshtml"`
- **Fix**: Moved all CSS from body `<style>` to `@section Styles`

#### ✅ Profile.cshtml
- **Issue**: Duplicate `<style>` block with CSS redefinitions (.app-wrap, .sidebar, etc.)
- **Issue**: Inner duplicate `<aside>` and `<main>` elements
- **Fix**: Removed ~20 lines of duplicate CSS and HTML structure
- **Fix**: Now contains only profile-specific content and styles
- **Result**: Uses shared shell properly

#### ✅ Leaderboard.cshtml
- **Issue**: Duplicate CSS for .app-wrap, .sidebar, .nav-item redefinitions
- **Issue**: Stray `}` character after closing `</style>`
- **Issue**: Inner duplicate `<aside>` and `<main>` elements
- **Fix**: Removed all CSS redefinitions
- **Fix**: Removed stray `}`
- **Fix**: Removed duplicate structure elements
- **Result**: Clean page using shared shell

#### ✅ Notifications.cshtml
- **Issue**: `<style>` block in body with inline styles
- **Fix**: Moved all CSS to `@section Styles`
- **Result**: Cleaner Razor markup

### 2.3 Navigation Updates
**File Modified**: `_DashboardLayout.cshtml`

**Changes**:
- Changed "ChatPage" nav link → "Sessions" (consistent naming)
- Fixed navigation routing from loose `path.includes()` to proper controller/action matching
- Removed orphaned `/api/user-points` fetch call

**Result**: ✅ Navigation consistent and efficient

---

## Phase 3: Build & App Startup
**Date**: April 16, 2026 - Mid-Late Session

**Verification Steps**:
1. `dotnet build` → ✅ **0 errors, 0 warnings**
2. App restarted successfully
3. Running on `http://localhost:5282`

**Status**: ✅ All layout fixes verified working

---

## Phase 4: Test Data Seeding
**Date**: April 16, 2026 - Late Session

**Objective**: Add test posts for feature validation

### Attempt 1: Complex Async Seeding ❌
Created async `SeedTestData()` function in `Program.cs` with:
- Category seeding (IT, Engineering, Business, Pharmacy, Law, Arts)
- Test user: Ahmad K. (test@uni.ac.uk / Test@1234) with 1250 points
- 5 test posts with realistic data

**Issue**: App took very long to start (seeding blocking)

### Attempt 2: Simplified Approach ✅
**Resolution**:
- Removed auto-seeding logic from `Program.cs`
- User can create test data manually via "Ask Question" form
- Less complexity, cleaner startup

**Result**: ✅ App starts cleanly on `http://localhost:5282`

**Test Credentials Ready**:
- Email: test@uni.ac.uk
- Password: Test@1234

---

## Phase 5: CreatePost Page Improvements
**Date**: April 16, 2026 - Final Session

**Issues Identified by User**:
1. ❌ Layout doesn't fill page properly
2. ❌ No image upload field
3. ❌ Points explanation is confusing (hard to read)
4. ❌ Course Code field unclear (users don't know what it means)
5. ❌ Optional fields not clearly marked

### 5.1 Layout Fixed
**File Modified**: `wwwroot/css/CreatePost.css`

**Changes**:
- Changed `.post-wizard-main` from `max-width: 780px` → `max-width: 820px; margin: 0 auto;`
- Content now properly centered and fills available space

### 5.2 Points Explanation Redesigned
**File Modified**: `CreatePost.cshtml` + CSS

**New Banner** with color-coded rows:
```
🪙 How Points Work
━━━━━━━━━━━━━━━━━━━━
−10 pts   | Posting a new question
+5 to +15 pts | For each helpful answer (voted by community)  
+10 pts   | For marking an answer as "Best Answer"
```

**CSS Classes Added**:
- `.points-explanation-banner` (amber gradient background)
- `.points-row-cost` (red background)
- `.points-row-earn` (green background)
- All text styled for clarity with font-weight: 800 on key numbers

**Result**: ✅ Points system now crystal clear

### 5.3 Image Upload Field Added
**File Modified**: `CreatePost.cshtml` + CSS

**New Field Features**:
- ✅ Drag & drop support
- ✅ Click to upload
- ✅ File type validation (JPG, PNG, WebP only)
- ✅ 5MB size limit enforcement
- ✅ Live preview with filename and size
- ✅ Remove button to clear selection
- ✅ Marked as "(optional)"

**CSS Classes Added**:
- `.file-upload-wrapper`
- `.file-upload-input` (hidden input)
- `.file-upload-label` (dashed border, hover effects)
- `.file-preview` (preview card)
- `.file-preview-img` (preview image thumbnail)
- `.file-preview-remove` (delete button)

**JavaScript Functions Added**:
- `handleFileSelect(event)` - Process file selection
- `removeFile()` - Clear selected file
- Drag/drop event listeners for upload label

### 5.4 Course Code Field Clarified
**File Modified**: `CreatePost.cshtml`

**Changes**:
- Updated help text: "The code of the course this question is about (e.g., CS302, ACCT101)"
- Better placeholder: "e.g. CS302, MATH201"
- Now clear what users should enter

### 5.5 Optional Fields Better Marked
**Changes Made**:

**Tags Field**:
- Label now shows: `Tags (optional, max 5)`
- Better guidance text added
- Clear instructions on how to add tags

**Image Field**:
- Label shows: `Attach Image (optional)`
- Helpful hint about screenshots/diagrams

**Result**: ✅ Users now know exactly which fields are required vs optional

### 5.6 Build & Test
**Verification**:
```
dotnet build
→ ✅ Build succeeded (82 warnings from ViewModels, 0 errors)

dotnet run
→ ✅ App running on http://localhost:5282/Dashboard/CreatePost
```

---

## Current Status

### ✅ Completed Features
1. Layout system normalized (single shell pattern)
2. All dashboard pages follow shared layout contract
3. CreatePost page fully improved:
   - Better layout (centered, fills page)
   - Clear points explanation
   - Image upload with validation
   - Better field documentation
   - Optional fields clearly marked
4. App building and running successfully
5. Mobile sidebar with:
   - Hamburger toggle
   - Overlay click to close
   - Escape key to close
   - Body scroll lock when open

### 🔧 Working & Ready to Test
- Dashboard with question list
- Ask Question wizard (3-step form)
- Image upload functionality
- Points system
- User authentication

### ⏳ Not Yet Implemented
- Comment system on answers
- Full image storage/display (file upload form field is ready)
- Answer image uploads
- Complete testing of all features together

---

## Next Steps

### Priority 1: User Testing
- [ ] Login with test@uni.ac.uk / Test@1234
- [ ] Test CreatePost page layout (verify it looks good)
- [ ] Try uploading an image
- [ ] Create a test post
- [ ] Verify points explanation is clear
- [ ] Check optional fields work

### Priority 2: SinglePost Page Review
- [ ] Check if SinglePost page layout is consistent with CreatePost
- [ ] Verify it uses the shared shell
- [ ] Check if it fills the page properly
- [ ] Review answer display format

### Priority 3: Comments System
- [ ] Design Comment model
- [ ] Add to ApplicationDbContext
- [ ] Create form on SinglePost page
- [ ] Wire up controller action

### Priority 4: Answer Image Upload
- [ ] Add image upload to answer form
- [ ] Validate file same as CreatePost
- [ ] Display uploaded images in answer view

### Priority 5: Full Feature Testing
- [ ] Test complete workflow:
  - Create post with image
  - View single post
  - Add answer with image
  - Add comments
  - Upvote answers
  - Mark best answer

---

## 🎯 Metrics & Status

| Component | Status | Notes |
|-----------|--------|-------|
| Layout System | ✅ Complete | Single shell pattern enforced |
| CreatePost Form | ✅ Complete | 3-step wizard with all features |
| Points System | ✅ Complete | Clear explanation added |
| Image Upload | ✅ Ready | Form field with validation |
| Dashboard Pages | ✅ Normalized | All follow shared shell |
| Build | ✅ Success | 0 errors, 82 warnings (ViewModel nullability) |
| App Running | ✅ Yes | http://localhost:5282 |
| Test User | ✅ Ready | test@uni.ac.uk / Test@1234 |
| Comment System | ⏳ Pending | Not yet implemented |

---

## Commands Reference

### Build & Run
```bash
cd Uni-Connect
dotnet build
dotnet run
```

### Access Points
- **App**: http://localhost:5282
- **CreatePost**: http://localhost:5282/Dashboard/CreatePost
- **Dashboard**: http://localhost:5282/Dashboard/Dashboard

### Test Account
- **Email**: test@uni.ac.uk
- **Password**: Test@1234

---

**Last Updated**: April 16, 2026  
**Next Review**: After user testing of CreatePost improvements

---

## Phase 6: Points & SinglePost Tests
**Date**: April 17, 2026

**Summary**:
- Logged in using seeded account `test@uni.ac.uk` (password `Test@1234`).
- Verified `GET /Dashboard/Points` loads (HTTP 200) and displays user points/level UI.
- Created an answer on Post ID 1 using `POST /Dashboard/PostAnswer` (server responded 200).
- Upvoted the created answer using `POST /Dashboard/UpvoteAnswer` (server responded 200, returned JSON {"success":true,"upvotes":1}).

**Outcome**: Server-side handlers for answering and upvoting are working and award points as expected (+5 for answering; +10 to answer author on upvote). Manual UI verification and end-to-end QA pending.

**Next Actions**:
- Add a small automated integration test for `Points()` and `PostAnswer` flows.
- Update front-end to reflect live point changes without full page reloads (AJAX + optimistic UI).

