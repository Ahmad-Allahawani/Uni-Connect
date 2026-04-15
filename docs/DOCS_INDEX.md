# Uni-Connect Documentation Index

Repository: ala6rash/uni-staging  
Last updated: 2026-04-15

## Purpose
This index provides a map of all documentation, reference materials, research, and design assets for the Uni-Connect graduation project.

---

## 📁 Documentation Structure

### Root-Level Docs
| File | Purpose |
|------|---------|
| `README.md` | Project overview and setup instructions |
| `PROJECT_OVERVIEW.md` | Complete project scope, goals, and architecture |
| `DEVELOPMENT_PLAN.md` | Development roadmap and feature priorities |
| `DEBUGGING_AND_FIX_DETAILS.md` | Historical debugging session and fixes applied |
| `PROJECT_OVERVIEW.md` | Full system architecture and feature breakdown |

### docs/ Folder
| File/Folder | Purpose |
|-------------|---------|
| `COPILOT_RULES.md` | **[MANDATORY]** Copilot working rules and boundaries |
| `DEV_LOG.md` | **[MANDATORY]** Running log of every meaningful code change |
| `DOCS_INDEX.md` | This file — documentation map |
| `reference/` | Research materials, specifications, documentation chapter excerpts |
| `design/` | UI/UX design images and mockups |

### reference/ Folder
| File | Purpose |
|------|---------|
| `Doc1_Chapter5_Content.txt` | Software Design Document (Chapter 5) excerpts and content |
| `Doc2_Research_Content.txt` | Research findings, specifications, and technical notes |

### design/ Folder
- **Currently empty** — UI/UX mockups and design images to be added
- When images are uploaded, they will serve as design reference
- Code implementation may differ from designs due to constraints

---

## 🔄 Mandatory Workflows

### Before Any Code Change
1. **Ask for confirmation** + clarify scope
2. **Provide a plan** (what, files, risks, testing)
3. **Wait for approval**
4. **Implement** with minimal changes

### After Any Code Change
1. **Append to `docs/DEV_LOG.md`** with:
   - Date, title, area, owner, type
   - What changed, why, how
   - Files changed
   - Testing steps

### Always
- Follow `docs/COPILOT_RULES.md`
- Code is source of truth; docs/design are reference
- Never modify without approval:
  - Auth system (ala6rash's domain)
  - Chat/Sessions/SignalR (Ahmad's domain)
  - Program.cs auth config
  - Database schema (without migration)

---

## 📊 Project Status

| Area | Status | Notes |
|------|--------|-------|
| **Repo Setup** | ✅ Complete | uni-staging created, synced with Ahmad's repo |
| **Documentation** | ✅ In Progress | COPILOT_RULES.md, DEV_LOG.md created |
| **Design Assets** | ⏳ Pending | Awaiting UI/UX design images |
| **Auth System** | 🟡 Needs Review | Ownership: ala6rash |
| **Chat/Sessions** | 🟡 Needs Review | Ownership: Ahmad |

---

## 🚀 How to Use This Index

1. **Need auth help?** → See `COPILOT_RULES.md` (ala6rash owns auth)
2. **Need chat/session help?** → See `COPILOT_RULES.md` (Ahmad owns chat)
3. **Want project scope?** → See `PROJECT_OVERVIEW.md`
4. **Need design reference?** → Check `docs/design/`
5. **Need to understand a change?** → See `docs/DEV_LOG.md`
6. **Need technical specs?** → See `docs/reference/`

---

## 📝 Notes

- This is a **4-member team project** with ownership boundaries
- Staging repo (`uni-staging`) is for isolated development
- All changes must be logged in `docs/DEV_LOG.md`
- Design images serve as reference; code constraints take precedence
- Security-first: no changes to auth/security without team discussion

---

**Next steps:** Upload design images to `docs/design/` when ready.
