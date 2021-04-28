using Newtonsoft.Json;
using System;

namespace BTCMachine
{
    public class ByBit_Order
    {
        [JsonProperty("user_id")]
        public int UserID { get; set; }

        [JsonProperty("order_status")]
        public string OrderStatus { get; set; }

        [JsonProperty("stop_order_status")]
        public string StopOrderStatus { get; set; }

        [JsonProperty("stop_px")]
        public double StopPrice { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("order_type")]
        public string OrderType { get; set; }

        [JsonProperty("stop_order_type")]
        public string StopOrderType { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("qty")]
        public double Quantity { get; set; }

        [JsonProperty("time_in_force")]
        public string TimeInForce { get; set; }

        [JsonProperty("order_link_id")]
        public string OrderLinkID { get; set; }

        [JsonProperty("order_id")]
        public string OrderID { get; set; }

        [JsonProperty("stop_order_id")]
        public string StopOrderID { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("leaves_qty")]
        public string LeavesQuantity { get; set; }

        [JsonProperty("leaves_value")]
        public string LeavesValue { get; set; }

        [JsonProperty("cum_exec_qty")]
        public string CumExecQuantity { get; set; }

        [JsonProperty("cum_exec_value")]
        public string CumExecValue { get; set; }

        [JsonProperty("cum_exec_fee")]
        public string CumExecFee { get; set; }

        [JsonProperty("reject_reason")]
        public string RejectReason { get; set; }

        public override string ToString() => JsonConvert.SerializeObject((object)this);
    }
}
