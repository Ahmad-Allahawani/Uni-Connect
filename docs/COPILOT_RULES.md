# Uni-Connect — Copilot Working Rules (uni-staging)

## Source of truth
- The repository code is the source of truth.
- Documentation can lag behind; do not implement from docs unless explicitly requested.

## Team ownership boundaries
- **Auth owner (ala6rash):** login / register / logout / forgot & reset password, user security logic.
- **Chat/sessions owner (Ahmad):** SignalR hub, chat UI, sessions lifecycle, messaging.

## Do-not-touch areas (unless explicitly requested)
- `Uni-Connect/Hubs/*`
- `Uni-Connect/Views/Dashboard/ChatPage.cshtml`
- Anything primarily implementing chat/sessions/SignalR behavior.

## Program.cs rules
- Keep `app.UseAuthentication()` before `app.UseAuthorization()`.
- Do not remove `app.MapHub<ChatHub>("/chatHub")`.
- Ask before changing auth configuration.

## Security rules
- Use **Cookie Authentication** (no JWT migration).
- Use **BCrypt** for passwords.
- Keep `[ValidateAntiForgeryToken]` on POST actions.

## Workflow requirements for changes
1. Explain the problem and approach.
2. List files to be changed.
3. Implement minimal change.
4. Provide a manual test checklist.
5. Append a DEV_LOG entry.

## Current priority note
- Forgot-password email sending is not in scope right now.
- Ask before changing forgot/reset password behavior.

## Dev log requirement (mandatory)
- Every time code is changed, append a new entry to `docs/DEV_LOG.md`:
  - What changed
  - Why
  - How
  - Files changed
  - How to test
