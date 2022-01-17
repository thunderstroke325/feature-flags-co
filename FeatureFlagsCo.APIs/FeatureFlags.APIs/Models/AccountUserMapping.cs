using System;
using System.ComponentModel.DataAnnotations;
using FeatureFlags.APIs.Services.MongoDb;

namespace FeatureFlags.APIs.Models
{
    public class AccountUserMapping
    {
        [Key]
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
        public string InvitorUserId { get; set; }
    }

    public class AccountUserV2 : MongoDbIntIdEntity
    {
        public int AccountId { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
        public string InvitorUserId { get; set; }
        public string InitialPassword { get; set; }

        public AccountUserV2(
            int accountId,
            string userId,
            string role,
            string invitorUserId = null, 
            string initialPassword = null)
        {
            if (accountId == 0)
            {
                throw new ArgumentException("accountUser accountId cannot be 0");
            }
            AccountId = accountId;
            
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("accountUser userId cannot be empty");
            }
            UserId = userId;
            
            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException("accountUser role cannot be empty");
            }
            Role = role;

            InvitorUserId = invitorUserId;
            InitialPassword = initialPassword;
        }
    }
}