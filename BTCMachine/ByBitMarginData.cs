using Newtonsoft.Json;

namespace BTCMachine
{
    public class ByBitMarginData
    {
        [JsonProperty("equity")]
        public double Equity { get; set; }

        [JsonProperty("available_balance")]
        public double AvailableBalance { get; set; }

        [JsonProperty("used_margin")]
        public double UsedMargin { get; set; }

        [JsonProperty("order_margin")]
        public double OrderMargin { get; set; }

        [JsonProperty("position_margin")]
        public double PositionMargin { get; set; }

        [JsonProperty("occ_closing_fee")]
        public double OCCClosingFee { get; set; }

        [JsonProperty("occ_funding_fee")]
        public double OCCFundingFee { get; set; }

        [JsonProperty("wallet_balance")]
        public double WalletBalance { get; set; }

        [JsonProperty("realised_pnl")]
        public double RealisedPNL { get; set; }

        [JsonProperty("unrealised_pnl")]
        public double UnrealisedPNL { get; set; }

        [JsonProperty("cum_realised_pnl")]
        public double CumRealisedPNL { get; set; }

        [JsonProperty("given_cash")]
        public double GivenCash { get; set; }

        [JsonProperty("service_cash")]
        public double ServiceCash { get; set; }
    }
}
