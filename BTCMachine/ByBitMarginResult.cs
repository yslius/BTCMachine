using Newtonsoft.Json;

namespace BTCMachine
{
    public class ByBitMarginResult
    {
        [JsonProperty("BTC")]
        public ByBitMarginData BTCMarginData { get; set; }

        [JsonProperty("ETH")]
        public ByBitMarginData ETHMarginData { get; set; }

        [JsonProperty("EOS")]
        public ByBitMarginData EOSMarginData { get; set; }

        [JsonProperty("USDT")]
        public ByBitMarginData USDTMarginData { get; set; }

        [JsonProperty("XRP")]
        public ByBitMarginData XRPMarginData { get; set; }
    }
}
