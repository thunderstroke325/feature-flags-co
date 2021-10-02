using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Models
{
    public class FeatureFlagCommit : FeatureFlag
    {
        public string FeatureFlagId { get; set; }
        public string PreviousVersion { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RequestUserId { get; set; }
        public ApprovalRequest ApprovalRequest { get; set; }
        public string Comment { get; set; }
        public List<ActivityLog> ActivityLogs { get; set; }
        public override string GetCollectionName()
        {
            return "FeatureFlagCommits";
        }
        public void CreatedFromFeatureFlag(FeatureFlag ff)
        {
            this.FeatureFlagId = ff.Id;
            this.Id = Guid.NewGuid().ToString();
            this.PreviousVersion = "";
            this.TargetIndividuals = ff.TargetIndividuals;
            this.VariationOptions = ff.VariationOptions;
            this.Version = ff.Version;
            this._Id = ff._Id;
            this.CreatedAt = DateTime.UtcNow;
            this.EffeciveDate = this.CreatedAt;
            this.EnvironmentId = ff.EnvironmentId;
            this.FF = ff.FF;
            this.FFP = ff.FFP;
            this.FFTUWMTR = ff.FFTUWMTR;
            this.IsArchived = ff.IsArchived;
            this.ApprovalRequest = new ApprovalRequest();
            this.ActivityLogs = new List<ActivityLog>();
        }
    }
    public class ActivityLog
    {
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Comment { get; set; }
        public ActivityTypeEnum ActivityType { get; set; }
    }

    public enum ActivityTypeEnum
    {
        Create,
        Comment, // review status change
        Approve, // review status change
        ChangeRequest,
        Decline, // review status change
        Apply,
        Schedule
    }
    public enum ReviewStatusEnum
    {
        Pending = 1,
        Approved = 2,
        Applied = 3,
        Declined = 4
    }

    public class ApprovalRequest
    {
        public string RequestorId { get; set; }
        public List<string> ReviewerIds { get; set; }
        public ApproveStrategyEnum Strategy { get; set; }
        public int MinimumApprovalUserNumber { get; set; }
        public List<string> ApprovedByUserIds { get; set; }
        public ReviewStatusEnum ReviewStatus { get; set; }
    }

    public enum ApproveStrategyEnum
    {
        Any = 1,
        All = 2
    }

}
