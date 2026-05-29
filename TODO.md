# UniConnect Implementation TODO

## Phase 1: Project Setup
- [ ] Update appsettings.json with database connection string
- [ ] Update Program.cs with all necessary services (MVC, EF, Identity)
- [ ] Create folder structure (Models, Data, Services, Controllers, Views)

## Phase 2: Database Models
- [ ] User model (Id, UniversityId, Email, PasswordHash, Role, Points, IsVerified, etc.)
- [ ] Category model (Faculty, Course, Topic)
- [ ] Post model (Question)
- [ ] Comment model (Answer)
- [ ] AcademicRequest model
- [ ] Proposal model
- [ ] PrivateSession model
- [ ] Message model
- [ ] Report model
- [ ] PointTransaction model

## Phase 3: Data Layer
- [ ] Create UniConnectDbContext
- [ ] Configure relationships

## Phase 4: Services
- [ ] UserService
- [ ] PostService
- [ ] CommentService
- [ ] RequestService
- [ ] PointsService
- [ ] ModerationService
- [ ] SearchService

## Phase 5: Controllers
- [ ] AccountController (Login, Register, Profile)
- [ ] HomeController
- [ ] PostsController
- [ ] RequestsController
- [ ] SessionsController
- [ ] AdminController
- [ ] ReportsController

## Phase 6: Views
- [ ] Shared Layout
- [ ] Home/Index
- [ ] Account Views
- [ ] Posts Views
- [ ] Requests Views
- [ ] Admin Views

## Phase 7: Testing
- [ ] Run the application
- [ ] Test basic functionality
