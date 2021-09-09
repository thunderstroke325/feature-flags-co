using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FeatureFlags.APIs.Models
{
    public class FFPendingChanges: MongoModelBase
    {
        //[BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        //public string _Id { get; set; }

        //[JsonProperty("id")]
        //public string Id { get; set; }

        public string FeatureFlagId { get; set; }

        public int EnvId { get; set; }

        public int ProjectId { get; set; }

        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<FFInstruction> Instructions { get; set; }

        public FFApprovalRequest ApprovalRequest { get; set; }

        public FFSchedule Schedule { get; set; }

        public List<FFActivityHistory> Histories { get; set; }

        public override string GetCollectionName()
        {
            return "PendingChanges";
        }
    }

    public class FFInstruction 
    {
        public string Kind { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

    public class FFApprovalRequest 
    {
        public string RequestorId { get; set; }
        public List<string> ReviewerIds { get; set; }
        public FFReviewStatusEnum ReviewStatus { get; set; }
    }

    public enum FFReviewStatusEnum 
    {
        Pending = 1,
        Approved = 2,
        Applied = 3,
        Declined = 4
    }

    public enum FFStatusEnum 
    {
        Pending = 1,
        Completed = 2
    }

    public class FFActivityHistory 
    {
        public string UserId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Comment { get; set; }
        public FFActivityKindEnum Kind { get; set; } 
    }

    public enum FFActivityKindEnum 
    {
        Create,
        Comment, // review status change
        Approve, // review status change
        Decline, // review status change
        Apply,
        Schedule
    }

    public class FFSchedule 
    {
        
    }
}
