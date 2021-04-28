using Newtonsoft.Json;

namespace BTCMachine
{
    public class ByBitConditionalOrderRequest
    {
        [JsonProperty("api_key")]
        public string APIKey { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("order_type")]
        public string OrderType { get; set; }

        [JsonProperty("qty")]
        public string Quantity { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("base_price")]
        public string BasePrice { get; set; }

        [JsonProperty("stop_px")]
        public string StopPrice { get; set; }

        [JsonProperty("time_in_force")]
        public string TimeInForce { get; set; }

        [JsonProperty("timestamp")]
        public string TimeStamp { get; set; }

        [JsonProperty("sign")]
        public string Sign { get; set; }
    }
}
