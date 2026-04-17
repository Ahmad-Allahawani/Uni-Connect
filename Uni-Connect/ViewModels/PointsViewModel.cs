namespace Uni_Connect.ViewModels
{
    public class PointsViewModel
    {
        // User Info
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Faculty { get; set; }
        public string YearOfStudy { get; set; }

        // Points Info
        public int CurrentPoints { get; set; }
        public int CurrentLevel { get; set; }  // 1-10
        public int NextLevelPoints { get; set; }  // Points needed for next level
        public int ProgressPercentage { get; set; }  // 0-100% to next level

        // Stats
        public int QuestionsAsked { get; set; }
        public int AnswersGiven { get; set; }
        public int HelpfulAnswers { get; set; }

        // Achievements
        public List<Achievement> Achievements { get; set; } = new List<Achievement>();

        // Transaction History
        public List<PointTransaction> Transactions { get; set; } = new List<PointTransaction>();
    }

    public class PointTransaction
    {
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Time { get; set; }
        public int Amount { get; set; } // Positive or negative
        public string Detail { get; set; }
    }

    public class Achievement
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }  // emoji or icon name
        public bool Unlocked { get; set; }
        public DateTime? UnlockedDate { get; set; }
    }
}
