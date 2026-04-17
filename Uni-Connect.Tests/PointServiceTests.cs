using Microsoft.EntityFrameworkCore;
using Uni_Connect.Models;
using Uni_Connect.Services;
using Xunit;

namespace Uni_Connect.Tests
{
    public class PointServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task DeductPoints_Succeeds_WhenUserHasEnoughPoints()
        {
            // Arrange
            var context = GetDbContext();
            var user = new User { Name = "Test", Email = "test@uni.ac.uk", Points = 100, UniversityID = "U1", Username = "u1", PasswordHash = "...", Role = "Student", Faculty = "IT", YearOfStudy = "1" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new PointService(context);

            // Act
            var result = await service.DeductPoints(user.UserID, 10, "Test Deduction");

            // Assert
            Assert.True(result);
            Assert.Equal(90, user.Points);
            var transaction = await context.PointsTransactions.FirstOrDefaultAsync(t => t.UserID == user.UserID);
            Assert.NotNull(transaction);
            Assert.Equal(-10, transaction.Amount);
        }

        [Fact]
        public async Task DeductPoints_Fails_WhenUserHasInsufficientPoints()
        {
            // Arrange
            var context = GetDbContext();
            var user = new User { Name = "Test", Email = "test@uni.ac.uk", Points = 5, UniversityID = "U1", Username = "u1", PasswordHash = "...", Role = "Student", Faculty = "IT", YearOfStudy = "1" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new PointService(context);

            // Act
            var result = await service.DeductPoints(user.UserID, 10, "Test Deduction");

            // Assert
            Assert.False(result);
            Assert.Equal(5, user.Points);
        }

        [Fact]
        public async Task AwardPoints_IncrementsBalance()
        {
            // Arrange
            var context = GetDbContext();
            var user = new User { Name = "Test", Email = "test2@uni.ac.uk", Points = 100, UniversityID = "U2", Username = "u2", PasswordHash = "...", Role = "Student", Faculty = "IT", YearOfStudy = "1" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new PointService(context);

            // Act
            await service.AwardPoints(user.UserID, 50, "Test Award");

            // Assert
            Assert.Equal(150, user.Points);
        }
    }
}
