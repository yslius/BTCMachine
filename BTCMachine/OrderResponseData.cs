namespace BTCMachine
{
    public struct OrderResponseData
    {
        public string account_id;
        public string symbol;
        public ORDER_TYPE trans_type;
        public double request_price;
        public double accept_price;
        public double request_quantity;
        public double accept_quantity;
        public bool success;
        public ErrorType error_type;
        public string error_message;
        public bool is_market;
        public double fee;
    }
}
