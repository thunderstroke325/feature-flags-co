using System.ComponentModel.DataAnnotations;
using FeatureFlags.APIs.Models;

namespace FeatureFlags.APIs.ViewModels.Project
{
    public class UpsertEnvironmentSetting
    {
        [Required(AllowEmptyStrings = false)]
        public string Id { get; set; }
        
        [Required(AllowEmptyStrings = false)]
        [StringLength(256)]
        public string Type { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(256)]
        public string Key { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(2048)]
        public string Value { get; set; }

        public string Tag { get; set; }

        public string Remark { get; set; }

        public EnvironmentSettingV2 NewSetting()
        {
            var setting = new EnvironmentSettingV2(Id, Type, Key, Value, Tag, Remark);

            return setting;
        }
    }
}