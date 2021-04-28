using Newtonsoft.Json;

namespace BTCMachine
{
    public class ByBit_APIInfo
    {
        [JsonProperty("api_key")]
        public string ApiKey;
        [JsonProperty("type")]
        public string Type;
        [JsonProperty("user_id")]
        public long UserID;
    }
}
