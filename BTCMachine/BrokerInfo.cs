namespace BTCMachine
{
    public struct BrokerInfo
    {
        public string BrokerName;
        public string Symbols;
        public string APIKey;
        public string APISec;

        public BrokerInfo(string broker_name, string symbols = "", string key = "", string sec = "")
        {
            this.BrokerName = broker_name;
            this.APIKey = key;
            this.APISec = sec;
            this.Symbols = symbols;
        }
    }
}
