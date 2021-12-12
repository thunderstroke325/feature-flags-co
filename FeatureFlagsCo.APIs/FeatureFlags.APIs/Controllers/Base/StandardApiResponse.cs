using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FeatureFlags.APIs.Controllers.Base
{
    public class StandardApiResponse
    {
        /// <summary>
        /// indicate if this response is success or not
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// the response message from the server
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// the response data in JSON format from the server
        /// </summary>
        public object Data { get; set; }

        public StandardApiResponse(bool success, object data = null, string message = null)
        {
            Success = success;
            Data = data;
            Message = message;
        }

        public static StandardApiResponse Failed(string message)
        {
            return new StandardApiResponse(false, null, message);
        }

        public string SerializeToJson()
        {
            var setting = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            
            return JsonConvert.SerializeObject(this, setting);
        }
    }
}