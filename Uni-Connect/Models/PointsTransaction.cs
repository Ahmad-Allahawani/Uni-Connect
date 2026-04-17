using System;
using System.ComponentModel.DataAnnotations;

namespace Uni_Connect.Models
{
    public class PointsTransaction
    {
        [Key]
        public int TransactionID { get; set; }
        
        public int UserID { get; set; }
        public User User { get; set; }

        public string Title { get; set; }
        public string Detail { get; set; }
        public int Amount { get; set; }
        public string Icon { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsDeleted { get; set; } = false;
    }
}
