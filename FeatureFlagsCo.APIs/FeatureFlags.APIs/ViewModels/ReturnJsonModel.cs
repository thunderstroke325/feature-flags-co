using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels
{
    public class ReturnJsonModel<T>
    {
        public Exception Error { get; set; }
        public int StatusCode { get; set; }
        public T Data { get; set; }
    }
}
