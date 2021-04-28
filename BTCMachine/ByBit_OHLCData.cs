using Newtonsoft.Json;
using System;

namespace BTCMachine
{
    public class ByBit_OHLCData
    {
        [JsonProperty("id")]
        public Decimal ID;
        [JsonProperty("symbol")]
        public string Symbol;
        [JsonProperty("period")]
        public string Period;
        [JsonProperty("start_at")]
        public Decimal StartAt;
        [JsonProperty("open")]
        public double Open;
        [JsonProperty("high")]
        public double High;
        [JsonProperty("low")]
        public double Low;
        [JsonProperty("close")]
        public double Close;
    }
}
