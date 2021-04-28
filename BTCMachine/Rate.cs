using System;
using System.Collections.Generic;
using System.Linq;

namespace BTCMachine
{
    public class Rate
    {
        public DateTime PriceTime;
        public double Open;
        public double OpenAsk;
        public double High;
        public double HighAsk;
        public double Low;
        public double LowAsk;
        public double Close;
        public double CloseAsk;

        public Rate()
        {
            this.PriceTime = DateTime.Now;
            this.Open = 0.0;
            this.High = 0.0;
            this.Low = 0.0;
            this.Close = 0.0;
            this.OpenAsk = 0.0;
            this.HighAsk = 0.0;
            this.LowAsk = 0.0;
            this.CloseAsk = 0.0;
        }

        public Rate(Rate rate)
        {
            this.PriceTime = rate.PriceTime;
            this.Open = rate.Open;
            this.High = rate.High;
            this.Low = rate.Low;
            this.Close = rate.Close;
            this.OpenAsk = rate.OpenAsk;
            this.HighAsk = rate.HighAsk;
            this.LowAsk = rate.LowAsk;
            this.CloseAsk = rate.CloseAsk;
        }

        public Rate(
          DateTime date_time,
          double open,
          double open_ask,
          double high,
          double high_ask,
          double low,
          double low_ask,
          double close,
          double close_ask)
        {
            this.PriceTime = date_time;
            this.Open = open;
            this.OpenAsk = open_ask;
            this.High = high;
            this.HighAsk = high_ask;
            this.Low = low;
            this.LowAsk = low_ask;
            this.Close = close;
            this.CloseAsk = close_ask;
        }

        public Rate(string line)
        {
            string[] strArray = line.Split(',');
            if (((IEnumerable<string>)strArray).Count<string>() < 5)
                return;
            this.PriceTime = DateTime.Parse(strArray[0]);
            this.Open = double.Parse(strArray[1]);
            this.High = double.Parse(strArray[2]);
            this.Low = double.Parse(strArray[3]);
            this.Close = double.Parse(strArray[4]);
            if (((IEnumerable<string>)strArray).Count<string>() < 9)
                return;
            this.OpenAsk = double.Parse(strArray[5]);
            this.HighAsk = double.Parse(strArray[6]);
            this.LowAsk = double.Parse(strArray[7]);
            this.CloseAsk = double.Parse(strArray[8]);
        }

        public string ToOutputString(string time_frame)
        {
            DateTime priceTime = this.PriceTime;
            return string.Format("{0},{1:F5},{2:F5},{3:F5},{4:F5},{5:F5},{6:F5},{7:F5},{8:F5}", (object)(!(time_frame == "M1") ? (!(time_frame == "M5") ? (!(time_frame == "M15") ? (!(time_frame == "H1") ? (!(time_frame == "H4") ? this.PriceTime.ToString("yyyy/MM/dd 00:00") : this.PriceTime.ToString("yyyy/MM/dd ") + (this.PriceTime.Hour / 4 * 4).ToString("D2") + ":00") : this.PriceTime.ToString("yyyy/MM/dd HH:00")) : this.PriceTime.ToString("yyyy/MM/dd HH:") + (this.PriceTime.Minute / 15 * 15).ToString("D2")) : this.PriceTime.ToString("yyyy/MM/dd HH:") + (this.PriceTime.Minute / 5 * 5).ToString("D2")) : this.PriceTime.ToString("yyyy/MM/dd HH:mm")), (object)this.Open, (object)this.High, (object)this.Low, (object)this.Close, (object)this.OpenAsk, (object)this.HighAsk, (object)this.LowAsk, (object)this.CloseAsk);
        }
    }
}
