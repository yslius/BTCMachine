using Newtonsoft.Json;
using System;

namespace BTCMachine
{
    public class ByBit_Position
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("user_id")]
        public int UserID { get; set; }

        [JsonProperty("risk_id")]
        public int RiskID { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("position_value")]
        public string PositionValue { get; set; }

        [JsonProperty("entry_price")]
        public string EntryPrice { get; set; }

        [JsonProperty("is_isolated")]
        public bool IsIsolated { get; set; }

        [JsonProperty("auto_add_margin")]
        public Decimal AutoAddMargin { get; set; }

        [JsonProperty("leverage")]
        public string Leverage { get; set; }

        [JsonProperty("effective_leverage")]
        public string EffectiveLeverage { get; set; }

        [JsonProperty("position_margin")]
        public string PositionMargin { get; set; }

        [JsonProperty("liq_price")]
        public string LiqPrice { get; set; }

        [JsonProperty("bust_price")]
        public string BustPrice { get; set; }

        [JsonProperty("occ_closing_fee")]
        public string OccClosingFee { get; set; }

        [JsonProperty("occ_funding_fee")]
        public string OccFundingFee { get; set; }

        [JsonProperty("take_profit")]
        public string TakeProfit { get; set; }

        [JsonProperty("stop_loss")]
        public string StopLoss { get; set; }

        [JsonProperty("trailing_stop")]
        public string TrailingStop { get; set; }

        [JsonProperty("position_status")]
        public string PositionStatus { get; set; }

        [JsonProperty("deleverage_indicator")]
        public Decimal DeleverageIndicator { get; set; }

        [JsonProperty("oc_calc_data")]
        public string OcCalcData { get; set; }

        [JsonProperty("order_margin")]
        public string OrderMargin { get; set; }

        [JsonProperty("wallet_balance")]
        public string WalletBalance { get; set; }

        [JsonProperty("realised_pnl")]
        public string RealisedPNL { get; set; }

        [JsonProperty("unrealised_pnl")]
        public string UnrealisedPNL { get; set; }

        [JsonProperty("cum_realised_pnl")]
        public string CumRealisedPNL { get; set; }

        [JsonProperty("cross_seq")]
        public Decimal CrossSeq { get; set; }

        [JsonProperty("position_seq")]
        public Decimal PositionSeq { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public override string ToString() => JsonConvert.SerializeObject((object)this);
    }
}
