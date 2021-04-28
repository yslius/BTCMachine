using LineNotifySDK;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace BTCMachine
{
    public class DoTenTraderManager
    {
        private DoTenTraderMain interface_;
        private bool is_english_ = true;
        private bool is_logic_running_;
        private double auto_lot_ = 0.01;
        private bool is_allowed_;
        private double nanping_step_ = 10000.0;
        private double new_limit_range_ = 5000.0;
        private double settle_limit_range_ = 2000.0;
        private double tp_ = 10000.0;
        private double sl_ = 50000.0;
        private int max_pos_count_ = 1;
        private Dictionary<string, BrokerInfo> broker_infos_;
        private Dictionary<string, Tick> current_tick_;
        private Dictionary<string, Rate> current_bar_;
        private Dictionary<string, Broker> brokers_;
        private List<Rate> ohlc_data_;
        private string current_broker_ = "ByBit";
        private string current_time_frame_ = "H1";
        private Logging logging_;
        private int error_count_;
        private int watch_hours_ = Constants.WatchHours;
        private string line_token_ = "";
        private bool is_token_;

        public string CurrentBroker
        {
            get => this.current_broker_;
            set => this.current_broker_ = value;
        }

        public bool IsEnglish
        {
            get => this.is_english_;
            set => this.is_english_ = value;
        }

        public bool IsAllowed
        {
            get => this.is_allowed_;
            set => this.is_allowed_ = value;
        }

        public int ErrorCount
        {
            get => this.error_count_;
            set => this.error_count_ = value;
        }

        public bool IsLogicRunning
        {
            get => this.is_logic_running_;
            set => this.is_logic_running_ = value;
        }

        public bool IsToken
        {
            get => this.is_token_;
            set => this.is_token_ = value;
        }

        public int WatchHours
        {
            get => this.watch_hours_;
            set => this.watch_hours_ = value;
        }

        public string LineToken
        {
            get => this.line_token_;
            set => this.line_token_ = value;
        }

        public double AutoLotParam
        {
            get => this.auto_lot_;
            set => this.auto_lot_ = value;
        }

        public int MaxPositionCount
        {
            get => this.max_pos_count_;
            set => this.max_pos_count_ = value;
        }

        public double NanpingStep
        {
            get => this.nanping_step_;
            set => this.nanping_step_ = value;
        }

        public double NewLimitRange
        {
            get => this.new_limit_range_;
            set => this.new_limit_range_ = value;
        }

        public double SettleLimitRange
        {
            get => this.settle_limit_range_;
            set => this.settle_limit_range_ = value;
        }

        public double TP
        {
            get => this.tp_;
            set => this.tp_ = value;
        }

        public double SL
        {
            get => this.sl_;
            set => this.sl_ = value;
        }

        public Dictionary<string, BrokerInfo> BrokerList
        {
            get
            {
                Dictionary<string, BrokerInfo> dictionary = new Dictionary<string, BrokerInfo>();
                foreach (KeyValuePair<string, BrokerInfo> brokerInfo in this.broker_infos_)
                    dictionary.Add(brokerInfo.Key, brokerInfo.Value);
                return dictionary;
            }
            set
            {
                this.broker_infos_.Clear();
                foreach (KeyValuePair<string, BrokerInfo> keyValuePair in value)
                    this.broker_infos_.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public DoTenTraderManager(DoTenTraderMain main_form)
        {
            this.interface_ = main_form;
            this.broker_infos_ = new Dictionary<string, BrokerInfo>();
            this.current_tick_ = new Dictionary<string, Tick>();
            this.current_bar_ = new Dictionary<string, Rate>();
            this.ohlc_data_ = new List<Rate>();
            this.logging_ = new Logging();
            this.InitDefaultBrokerBuffers();
            this.LoadBrokerInformation();
            this.brokers_ = new Dictionary<string, Broker>();
            this.brokers_.Add("ByBit", (Broker)new ByBit(this));
        }

        public void LoginBroker(string broker)
        {
            foreach (KeyValuePair<string, Broker> broker1 in this.brokers_)
                broker1.Value.Stop();
            if (!this.brokers_.ContainsKey(broker) || !this.broker_infos_.ContainsKey(broker))
                return;
            int num = (int)this.brokers_[broker].Login(this.broker_infos_[broker].APIKey, this.broker_infos_[broker].APISec);
            this.brokers_[broker].Start();
            string id = this.brokers_[broker].APIInfo();
            if (!string.IsNullOrEmpty(id))
            {
                this.interface_.SetUserID("UID: " + id);
                if (!this.IsGoogleSpreadSheetExist(id))
                    return;
                this.is_allowed_ = true;
            }
            else
                this.interface_.SetUserID(this.is_english_ ? "NO API" : "API情報なし");
        }

        public void InitOHLCData()
        {
            this.ohlc_data_.Clear();
            List<Rate> ohlcData = this.brokers_[this.current_broker_].GetOHLCData(this.current_time_frame_, 200);
            if (ohlcData == null)
                return;
            foreach (Rate rate in ohlcData)
                this.ohlc_data_.Add(rate);
        }

        public void ChangeTimeFrame(string broker, string time_frame) => this.current_time_frame_ = time_frame;

        public void UpdateRate(string broker, DateTime date_time, double bid, double ask)
        {
            this.current_tick_[broker].PriceTime = date_time;
            this.current_tick_[broker].Bid = bid;
            this.current_tick_[broker].Ask = ask;
            this.interface_.UpdateTickLabel(bid, ask);
            this.UpdateBar(broker, this.current_time_frame_, date_time, bid, ask);
            string key = broker + "_" + this.current_time_frame_;
            double highest = this.Highest(this.watch_hours_);
            double lowest = this.Lowest(this.watch_hours_);
            this.interface_.UpdateQuoteList(this.current_bar_[key].Open, this.current_bar_[key].High, this.current_bar_[key].Low, this.current_bar_[key].Close, highest, lowest);
        }

        public void AppendLog(string message)
        {
            DateTime now = DateTime.Now;
            this.logging_.AppendSimpleLog(now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + message);
            this.interface_.AppendLog(now.ToString("yyyy/MM/dd HH:mm:ss"), message);
        }

        private bool CheckNewBar(string key, string time_frame, DateTime date_time)
        {
            if (time_frame == "M1")
                return this.current_bar_[key].PriceTime.Minute != date_time.Minute;
            if (time_frame == "M5")
                return this.current_bar_[key].PriceTime.Minute / 5 != date_time.Minute / 5;
            if (time_frame == "M15")
                return this.current_bar_[key].PriceTime.Minute / 15 != date_time.Minute / 15;
            if (time_frame == "H1")
                return this.current_bar_[key].PriceTime.Hour != date_time.Hour;
            if (time_frame == "H4")
                return this.current_bar_[key].PriceTime.Hour / 4 != date_time.Hour / 4;
            return time_frame == "D1" ? this.current_bar_[key].PriceTime.Day != date_time.Day : this.current_bar_[key].PriceTime.Minute != date_time.Minute;
        }

        private bool IsSameTimeHour(DateTime datetime1, DateTime datetime2, bool is_hour)
        {
            if (is_hour)
            {
                DateTime datetime3 = datetime1.AddMinutes((double)-datetime1.Minute).AddSeconds((double)-datetime1.Second).AddMilliseconds((double)-datetime1.Millisecond);
                DateTime datetime4 = datetime2.AddMinutes((double)-datetime2.Minute).AddSeconds((double)-datetime2.Second).AddMilliseconds((double)-datetime2.Millisecond);
                return datetime3.ToUnixTime() == datetime4.ToUnixTime();
            }
            DateTime datetime5 = datetime1.AddSeconds((double)-datetime1.Second).AddMilliseconds((double)-datetime1.Millisecond);
            DateTime datetime6 = datetime2.AddSeconds((double)-datetime2.Second).AddMilliseconds((double)-datetime2.Millisecond);
            return datetime5.ToUnixTime() == datetime6.ToUnixTime();
        }

        private void UpdateBar(
          string broker,
          string time_frame,
          DateTime date_time,
          double bid,
          double ask)
        {
            string key = broker + "_" + time_frame;
            if (this.CheckNewBar(key, time_frame, date_time) || this.current_bar_[key].Open <= 0.0)
            {
                if (!this.IsSameTimeHour(this.ohlc_data_[this.ohlc_data_.Count - 1].PriceTime, this.current_bar_[key].PriceTime, time_frame == "H1"))
                    this.ohlc_data_.Add(new Rate(this.current_bar_[key]));
                this.current_bar_[key].PriceTime = date_time;
                this.current_bar_[key].Open = bid;
                this.current_bar_[key].OpenAsk = ask;
                this.current_bar_[key].High = bid;
                this.current_bar_[key].HighAsk = ask;
                this.current_bar_[key].Low = bid;
                this.current_bar_[key].LowAsk = ask;
            }
            if (this.IsSameTimeHour(this.ohlc_data_[this.ohlc_data_.Count - 1].PriceTime, this.current_bar_[key].PriceTime, time_frame == "H1"))
            {
                Rate rate = this.ohlc_data_[this.ohlc_data_.Count - 1];
                if (rate.High > this.current_bar_[key].High)
                {
                    this.current_bar_[key].High = rate.High;
                    this.current_bar_[key].HighAsk = rate.High;
                }
                this.current_bar_[key].Open = rate.Open;
                this.current_bar_[key].OpenAsk = rate.Open;
                if (rate.Low < this.current_bar_[key].Low)
                {
                    this.current_bar_[key].Low = rate.Low;
                    this.current_bar_[key].LowAsk = rate.Low;
                }
            }
            if (bid > this.current_bar_[key].High)
                this.current_bar_[key].High = bid;
            if (ask > this.current_bar_[key].OpenAsk)
                this.current_bar_[key].HighAsk = ask;
            if (bid < this.current_bar_[key].Low)
                this.current_bar_[key].Low = bid;
            if (ask < this.current_bar_[key].LowAsk)
                this.current_bar_[key].LowAsk = ask;
            this.current_bar_[key].Close = bid;
            this.current_bar_[key].CloseAsk = ask;
        }

        private double Highest(int watch_hours)
        {
            double num = 0.0;
            DateTime now = DateTime.Now;
            DateTime dateTime = now.AddMinutes((double)-now.Minute);
            dateTime = dateTime.AddSeconds((double)-now.Second);
            DateTime datetime1 = dateTime.AddMilliseconds((double)-now.Millisecond);
            DateTime datetime2 = datetime1.AddHours((double)-watch_hours);
            if (this.ohlc_data_.Count > 0)
            {
                for (int index = this.ohlc_data_.Count - 1; index >= 0; --index)
                {
                    Rate rate = this.ohlc_data_[index];
                    if (rate.PriceTime.ToUnixTime() != datetime1.ToUnixTime())
                    {
                        if (rate.PriceTime.ToUnixTime() >= datetime2.ToUnixTime())
                        {
                            if (num < rate.High)
                                num = rate.High;
                        }
                        else
                            break;
                    }
                }
            }
            return num;
        }

        private double Lowest(int watch_hours)
        {
            double num = 999999999.0;
            DateTime now = DateTime.Now;
            DateTime dateTime = now.AddMinutes((double)-now.Minute);
            dateTime = dateTime.AddSeconds((double)-now.Second);
            DateTime datetime1 = dateTime.AddMilliseconds((double)-now.Millisecond);
            DateTime datetime2 = datetime1.AddHours((double)-watch_hours);
            if (this.ohlc_data_.Count <= 0)
                return 0.0;
            for (int index = this.ohlc_data_.Count - 1; index >= 0; --index)
            {
                Rate rate = this.ohlc_data_[index];
                if (rate.PriceTime.ToUnixTime() != datetime1.ToUnixTime())
                {
                    if (rate.PriceTime.ToUnixTime() >= datetime2.ToUnixTime())
                    {
                        if (num > rate.Low)
                            num = rate.Low;
                    }
                    else
                        break;
                }
            }
            if (num > 999999998.0)
                num = 0.0;
            return num;
        }

        private void SaveCurrentBar(string path, string time_frame)
        {
            if (this.current_bar_[path].Open <= 0.0)
                return;
            try
            {
                if (System.IO.File.Exists(path + ".csv"))
                    System.IO.File.AppendAllText(path + ".csv", this.current_bar_[path].ToOutputString(time_frame) + "\r\n");
                else
                    System.IO.File.WriteAllText(path + ".csv", this.current_bar_[path].ToOutputString(time_frame) + "\r\n");
            }
            catch (Exception ex)
            {
            }
        }

        public Tick CurrentTick(string broker) => this.current_tick_ != null ? this.current_tick_[broker] : (Tick)null;

        public void OnTick(string broker, string time_frame)
        {
            if (this.error_count_ >= 5 && this.is_logic_running_)
            {
                this.is_logic_running_ = false;
                this.interface_.StopAutoTrade();
                this.interface_.ShowMessageBox();
            }
            string key = broker + "_" + time_frame;
            if (!this.current_bar_.ContainsKey(key))
                return;
            Rate rate = this.current_bar_[key];
            double num1 = this.Highest(this.watch_hours_);
            double num2 = this.Lowest(this.watch_hours_);
            double bid = this.current_tick_[broker].Bid;
            double ask = this.current_tick_[broker].Ask;
            Margin margin = this.GetMargin(broker);
            if (margin == null)
            {
                Thread.Sleep(10000);
            }
            else
            {
                if (margin.Exchange != this.current_broker_)
                    return;
                this.interface_.UpdateEquity(margin.Equity, margin.Balance);
                Thread.Sleep(1200);
                List<Position> position_list = this.Positions(broker);
                this.interface_.UpdatePositionList(position_list);
                Thread.Sleep(1200);
                if (!this.is_logic_running_ || !this.is_allowed_)
                    return;
                if (bid > num1 && num1 > 0.0)
                {
                    if (position_list.Count > 0)
                    {
                        if (position_list[0].Type != 1)
                        {
                            this.CloseAllPositionImmediate(this.current_broker_);
                            int num3 = (int)this.SendOrder(this.current_broker_, this.auto_lot_, 0.0, true);
                            this.LineNotify(this.line_token_, Constants.TradeSymbol + "Close and Buy " + this.auto_lot_.ToString() + "$");
                        }
                    }
                    else
                    {
                        int num3 = (int)this.SendOrder(this.current_broker_, this.auto_lot_, 0.0, true);
                        this.LineNotify(this.line_token_, Constants.TradeSymbol + "Buy " + this.auto_lot_.ToString() + "$");
                        Thread.Sleep(1200);
                    }
                }
                if (bid >= num2 || num2 <= 0.0)
                    return;
                if (position_list.Count > 0)
                {
                    if (position_list[0].Type != 1)
                        return;
                    this.CloseAllPositionImmediate(this.current_broker_);
                    int num3 = (int)this.SendOrder(this.current_broker_, this.auto_lot_, 0.0, false);
                    this.LineNotify(this.line_token_, Constants.TradeSymbol + "Close and Sell" + this.auto_lot_.ToString() + "$");
                }
                else
                {
                    int num3 = (int)this.SendOrder(this.current_broker_, this.auto_lot_, 0.0, false);
                    this.LineNotify(this.line_token_, Constants.TradeSymbol + "Sell " + this.auto_lot_.ToString() + "$");
                    Thread.Sleep(1200);
                }
            }
        }

        public double GetPriceDouble(string broker, double value) => broker == "Bitflyer" ? Math.Truncate(value) : Math.Truncate(value * 100.0) / 100.0;

        public void ModifyTPSL(string broker, List<Position> positions, double tp, double sl)
        {
            double lot = 0.0;
            double num1 = 0.0;
            foreach (Position position in positions)
            {
                lot += position.Volume;
                num1 += position.OpenPrice * position.Volume;
            }
            if (lot <= 0.0)
                return;
            double num2 = num1 / lot;
            int type = positions[0].Type;
            this.CancelAll(broker);
            Thread.Sleep(1200);
            if (type == 1)
            {
                if (this.SendOrder(broker, lot, this.GetPriceDouble(broker, num2 + tp), false) != ErrorType.Success)
                    ++this.error_count_;
                Thread.Sleep(1200);
                if (this.SendLimitOrder(broker, lot, this.GetPriceDouble(broker, num2 - sl), false, true) != ErrorType.Success)
                    ++this.error_count_;
                Thread.Sleep(1200);
            }
            else
            {
                if (this.SendOrder(broker, lot, this.GetPriceDouble(broker, num2 - tp), true) != ErrorType.Success)
                    ++this.error_count_;
                Thread.Sleep(1200);
                if (this.SendLimitOrder(broker, lot, this.GetPriceDouble(broker, num2 + sl), true, true) != ErrorType.Success)
                    ++this.error_count_;
                Thread.Sleep(1200);
            }
        }

        public void CloseAllPositionImmediate(string broker)
        {
            List<Position> positionList = this.Positions(broker);
            if (positionList == null || positionList.Count == 0)
                return;
            double lot = 0.0;
            foreach (Position position in positionList)
                lot += position.Volume;
            if (lot <= 0.0)
                return;
            if (positionList[0].Type == 1)
            {
                if (this.SendOrder(broker, lot, 0.0, false, true) != ErrorType.Success)
                    ++this.error_count_;
                Thread.Sleep(1200);
                this.CancelAll(broker);
            }
            else
            {
                if (this.SendOrder(broker, lot, 0.0, true, true) != ErrorType.Success)
                    ++this.error_count_;
                Thread.Sleep(1200);
                this.CancelAll(broker);
            }
            Thread.Sleep(1200);
        }

        public ErrorType SendOrder(
          string broker,
          double lot,
          double offset,
          bool is_buy,
          bool is_market)
        {
            if (this.brokers_.ContainsKey(broker))
            {
                OrderRequestData order_request = new OrderRequestData();
                order_request.symbol = this.broker_infos_[broker].Symbols;
                order_request.account = broker;
                order_request.amount = lot;
                order_request.signal = is_buy ? ORDER_TYPE.Buy : ORDER_TYPE.Sell;
                order_request.is_market = is_market;
                order_request.request_price = is_buy ? this.current_tick_[broker].Ask - offset : this.current_tick_[broker].Bid + offset;
                if (order_request.request_price > 0.0)
                {
                    this.AppendLog("Order Sent " + broker + " size : " + lot.ToString() + " type : " + (is_buy ? (object)"buy" : (object)" sell") + " price : " + (object)order_request.request_price);
                    return this.brokers_[broker].SendOrder(order_request);
                }
            }
            return ErrorType.UnknownError;
        }

        public ErrorType SendOrder(
          string broker,
          double lot,
          double limit_price,
          bool is_buy)
        {
            if (this.brokers_.ContainsKey(broker))
            {
                OrderRequestData order_request = new OrderRequestData();
                order_request.symbol = this.broker_infos_[broker].Symbols;
                order_request.account = broker;
                order_request.amount = lot;
                order_request.signal = is_buy ? ORDER_TYPE.Buy : ORDER_TYPE.Sell;
                order_request.is_market = true;
                if (limit_price > 0.0)
                    order_request.is_market = false;
                order_request.request_price = limit_price;
                if (order_request.request_price > 0.0)
                {
                    ErrorType errorType = this.brokers_[broker].SendOrder(order_request);
                    this.AppendLog("Order Sent " + broker + " size : " + lot.ToString() + " type : " + (is_buy ? (object)"buy" : (object)" sell") + " price : " + (object)order_request.request_price);
                    return errorType;
                }
                if (order_request.is_market)
                {
                    ErrorType errorType = this.brokers_[broker].SendOrder(order_request);
                    this.AppendLog("Order Sent " + broker + " size : " + lot.ToString() + " type : " + (is_buy ? "buy" : " sell"));
                    return errorType;
                }
            }
            return ErrorType.UnknownError;
        }

        public ErrorType SendLimitOrder(
          string broker,
          double lot,
          double limit_price,
          bool is_buy,
          bool is_stop)
        {
            if (this.brokers_.ContainsKey(broker))
            {
                OrderRequestData order_request = new OrderRequestData();
                order_request.symbol = this.broker_infos_[broker].Symbols;
                order_request.account = broker;
                order_request.amount = lot;
                order_request.signal = is_buy ? ORDER_TYPE.Buy : ORDER_TYPE.Sell;
                order_request.is_market = false;
                order_request.is_stop = is_stop;
                order_request.request_price = limit_price;
                order_request.current_price = is_buy ? this.current_tick_[broker].Ask : this.current_tick_[broker].Bid;
                if (order_request.request_price > 0.0)
                {
                    ErrorType errorType = this.brokers_[broker].SendLimitOrder(order_request);
                    this.AppendLog("Limit Order Sent " + broker + " size : " + lot.ToString() + " type : " + (is_buy ? (object)"buy" : (object)" sell") + " limit price : " + (object)limit_price);
                    return errorType;
                }
            }
            return ErrorType.UnknownError;
        }

        public void CancelAll(string broker)
        {
            if (this.brokers_.ContainsKey(broker))
                this.brokers_[broker].CancelAllOrders(this.broker_infos_[broker].Symbols);
            this.AppendLog("Cancel requested.");
        }

        private Margin GetMargin(string broker) => this.brokers_.ContainsKey(broker) ? this.brokers_[broker].GetMargin() : (Margin)null;

        private List<Position> Positions(string broker) => this.brokers_.ContainsKey(broker) ? this.brokers_[broker].GetPositions(this.broker_infos_[broker].Symbols) : (List<Position>)null;

        private List<Order> Orders(string broker) => this.brokers_.ContainsKey(broker) ? this.brokers_[broker].GetOrders(this.broker_infos_[broker].Symbols) : (List<Order>)null;

        private void CreateDefaultAndSave(string path)
        {
            BrokerInfo brokerInfo = new BrokerInfo("ByBit", Constants.TradeSymbol);
            this.broker_infos_.Add(brokerInfo.BrokerName, brokerInfo);
        }

        private void InitDefaultBrokerBuffers()
        {
            this.current_tick_.Add("ByBit", new Tick());
            foreach (string timeFrame in Constants.TimeFrames)
                this.current_bar_.Add("ByBit_" + timeFrame, new Rate());
        }

        public void SaveInformations(string path)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, BrokerInfo> brokerInfo in this.broker_infos_)
                stringBuilder.AppendLine(JsonConvert.SerializeObject((object)brokerInfo.Value));
            System.IO.File.WriteAllText(path, stringBuilder.ToString());
        }

        private void LoadBrokerInformation()
        {
            try
            {
                string path = "Broker.cfg";
                if (System.IO.File.Exists(path))
                {
                    StreamReader streamReader = new StreamReader(path);
                    while (true)
                    {
                        string str = streamReader.ReadLine();
                        if (!string.IsNullOrEmpty(str))
                        {
                            BrokerInfo brokerInfo = JsonConvert.DeserializeObject<BrokerInfo>(str);
                            brokerInfo.Symbols = Constants.TradeSymbol;
                            this.broker_infos_.Add(brokerInfo.BrokerName, brokerInfo);
                        }
                        else
                            break;
                    }
                    streamReader?.Close();
                }
                else
                    this.CreateDefaultAndSave(path);
            }
            catch (Exception ex)
            {
                this.logging_.AppendSimpleLog("Load Broker Information Exception:" + ex.Message);
            }
        }

        public void OnResponseOrder(OrderResponseData response_data)
        {
            if (response_data.error_type == ErrorType.Success)
                this.AppendLog("Order Success accept size : " + response_data.accept_quantity.ToString() + "  price : " + (object)response_data.accept_price);
            else
                this.AppendLog("[Order Error] " + response_data.error_type.ToString() + "  " + response_data.error_message);
        }

        public void LineNotify(string Token, string Message)
        {
            if (string.IsNullOrEmpty(Token))
                return;
            if (!this.is_token_)
                return;
            try
            {
                Utility.SendNotification(Token, Message);
            }
            catch (Exception ex)
            {
            }
        }

        public bool IsGoogleSpreadSheetExist(string id)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://docs.google.com/spreadsheets/d/1cOKbFMH_0oKOwr6A1LBrfamYC_ZK6PoCOpDABfA31EE/edit?usp=drivesdk");
            httpWebRequest.Method = "GET";
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36";
            httpWebRequest.ContentType = "application/json";
            try
            {
                using (WebResponse response = httpWebRequest.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader streamReader = new StreamReader(responseStream))
                        {
                            try
                            {
                                string end = streamReader.ReadToEnd();
                                if (!end.Contains("dir=\"ltr\">" + id))
                                {
                                    if (!end.Contains(id + "</td>"))
                                        goto label_18;
                                }
                                return true;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        label_18:
            return false;
        }

        public void Stop()
        {
            foreach (KeyValuePair<string, Broker> broker in this.brokers_)
                broker.Value.Stop();
        }
    }
}
