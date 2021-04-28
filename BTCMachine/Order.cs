using System;

namespace BTCMachine
{
    public struct Order
    {
        public DateTime OpenDateTime;
        public string Symbol;
        public string OrderID;
        public double Volume;
        public double OpenPrice;
        public int Type;
        public double OutStandingSize;
        public bool IsParent;

        public Order(
          string position_id,
          DateTime open_date,
          string symbol,
          double volume,
          double open_price,
          int type,
          double outstanding_size,
          bool is_parent = false)
        {
            this.OpenDateTime = open_date;
            this.OrderID = position_id;
            this.Symbol = symbol;
            this.Volume = volume;
            this.OpenPrice = open_price;
            this.OutStandingSize = outstanding_size;
            this.Type = type;
            this.IsParent = is_parent;
        }
    }
}
