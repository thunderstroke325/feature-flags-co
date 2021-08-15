using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Models
{
    public class EnvironmentUserQueryResultViewModel
    {
        public int Count { get; set; }
        public List<EnvironmentUser> Users { get; set; }
    }
}
