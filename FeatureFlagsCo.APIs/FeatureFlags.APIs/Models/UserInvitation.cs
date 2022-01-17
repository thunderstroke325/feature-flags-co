using System.ComponentModel.DataAnnotations;

namespace FeatureFlags.APIs.Models
{
    public class UserInvitation
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public int AccountId { get; set; }
        public string InitialPassword { get; set; }
    }
}