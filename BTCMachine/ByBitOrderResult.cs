using Newtonsoft.Json;
using System;

namespace BTCMachine
{
    public class ByBitOrderResult
    {
        [JsonProperty("user_id")]
        public int UserID;
        [JsonProperty("order_id")]
        public string OrderID;
        [JsonProperty("symbol")]
        public string Symbol;
        [JsonProperty("side")]
        public string Side;
        [JsonProperty("order_type")]
        public string OrderType;
        [JsonProperty("price")]
        public Decimal Price;
        [JsonProperty("qty")]
        public Decimal Quantity;
        [JsonProperty("time_in_force")]
        public string TimeInForce;
        [JsonProperty("order_status")]
        public string OrderStatus;
        [JsonProperty("last_exec_time")]
        public long LastExecTime;
        [JsonProperty("last_exec_price")]
        public Decimal LastExecPrice;
        [JsonProperty("leaves_qty")]
        public Decimal LeavesQuantity;
        [JsonProperty("cum_exec_qty")]
        public Decimal CumExecQuantity;
        [JsonProperty("cum_exec_value")]
        public Decimal CumExecValue;
        [JsonProperty("cum_exec_fee")]
        public Decimal CumExecFee;
        [JsonProperty("reject_reason")]
        public string RejectReason;
        [JsonProperty("order_link_id")]
        public string OrderLinkID;
        [JsonProperty("created_at")]
        public string CreatedAt;
        [JsonProperty("updated_at")]
        public string UpdatedAt;
    }
}
