using Newtonsoft.Json;

namespace BTCMachine
{
    public class ByBitCancelAllRequest
    {
        [JsonProperty("api_key")]
        public string APIKey { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("timestamp")]
        public string TimeStamp { get; set; }

        [JsonProperty("sign")]
        public string Sign { get; set; }
    }
}
