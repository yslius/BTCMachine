using System;

namespace BTCMachine
{
    public struct Position
    {
        public DateTime OpenDateTime;
        public string Symbol;
        public string PositionID;
        public double Volume;
        public double OpenPrice;
        public int Type;
        public double PnL;

        public Position(
          string position_id,
          DateTime open_date,
          string symbol,
          double volume,
          double open_price,
          int type,
          double pnl)
        {
            this.OpenDateTime = open_date;
            this.PositionID = position_id;
            this.Symbol = symbol;
            this.Volume = volume;
            this.OpenPrice = open_price;
            this.PnL = pnl;
            this.Type = type;
        }
    }
}
