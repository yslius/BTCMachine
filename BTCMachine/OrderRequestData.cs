namespace BTCMachine
{
    public struct OrderRequestData
    {
        public string account;
        public string symbol;
        public double base_price;
        public double current_price;
        public double request_price;
        public ORDER_TYPE signal;
        public double amount;
        public bool is_market;
        public bool is_stop;
    }
}
