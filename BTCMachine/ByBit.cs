using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BTCMachine
{
    internal class ByBit : Broker
    {
        private Thread rate_thread_;
        private ByBit_PrivateApi api_client_;

        public ByBit(DoTenTraderManager atb_manager)
          : base(atb_manager)
        {
            this.rate_thread_ = new Thread(new ThreadStart(this.ProcessTickerThread));
            this.rate_thread_.IsBackground = true;
        }

        public override string APIInfo() => this.api_client_ != null ? this.api_client_.GetAPIInfo() : "";

        public override ErrorType Login(string api_key, string api_sec)
        {
            if (!this.LoginBasic(api_key, api_sec))
                return ErrorType.InvalidAccountInfo;
            this.api_client_ = new ByBit_PrivateApi(this.api_key_, this.api_sec_);
            return ErrorType.Success;
        }

        public override List<Position> GetPositions(string symbol)
        {
            if (this.api_client_ == null)
                return (List<Position>)null;
            List<Position> positionList = new List<Position>();
            ByBit_Position[] positions = this.api_client_.GetPositions(symbol);
            if (positions == null)
                return (List<Position>)null;
            if (((IEnumerable<ByBit_Position>)positions).Count<ByBit_Position>() > 0)
            {
                foreach (ByBit_Position byBitPosition in positions)
                {
                    Position position = new Position(byBitPosition.ID.ToString(), byBitPosition.UpdatedAt, byBitPosition.Symbol, double.Parse(byBitPosition.Size), double.Parse(byBitPosition.EntryPrice), byBitPosition.Side == "Buy" ? 1 : 2, double.Parse(byBitPosition.UnrealisedPNL));
                    positionList.Add(position);
                }
            }
            return positionList;
        }

        public override List<Order> GetOrders(string symbol)
        {
            if (this.api_client_ == null)
                return (List<Order>)null;
            List<Order> orderList = new List<Order>();
            ByBit_Order[] orders = this.api_client_.GetOrders(symbol);
            if (orders == null)
                return (List<Order>)null;
            if (((IEnumerable<ByBit_Order>)orders).Count<ByBit_Order>() > 0)
            {
                foreach (ByBit_Order byBitOrder in orders)
                {
                    Order order = new Order(byBitOrder.OrderID.ToString(), byBitOrder.CreatedAt, byBitOrder.Symbol, byBitOrder.Quantity, byBitOrder.Price, byBitOrder.Side == "Buy" ? 1 : 2, double.Parse(byBitOrder.LeavesQuantity));
                    orderList.Add(order);
                }
            }
            Thread.Sleep(1000);
            ByBit_Order[] conditionalOrders = this.api_client_.GetConditionalOrders(symbol);
            if (conditionalOrders != null && ((IEnumerable<ByBit_Order>)conditionalOrders).Count<ByBit_Order>() > 0)
            {
                foreach (ByBit_Order byBitOrder in conditionalOrders)
                {
                    Order order = new Order(byBitOrder.StopOrderID.ToString(), byBitOrder.CreatedAt, byBitOrder.Symbol, byBitOrder.Quantity, byBitOrder.StopPrice, byBitOrder.Side == "Buy" ? 1 : 2, byBitOrder.Quantity, true);
                    orderList.Add(order);
                }
            }
            return orderList;
        }

        public override void CancelAllOrders(string symbol)
        {
            this.api_client_.CancelAllOrders(symbol);
            Thread.Sleep(1000);
            this.api_client_.CancelAllConditionalOrders(symbol);
        }

        public override Margin GetMargin()
        {
            if (this.api_client_ == null)
                return (Margin)null;
            ByBitMarginResult margin = this.api_client_.GetMargin();
            if (margin == null)
                return (Margin)null;
            if (margin.BTCMarginData == null)
                return (Margin)null;
            return new Margin()
            {
                Exchange = nameof(ByBit),
                Balance = margin.BTCMarginData.AvailableBalance,
                Equity = margin.BTCMarginData.Equity
            };
        }

        private void ProcessTickerThread()
        {
            while (this.is_running_)
            {
                this.RequestTicker(Constants.TradeSymbol);
                Thread.Sleep(500);
            }
            this.rate_thread_.Abort();
        }

        private void RequestTicker(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                return;
            string str = this.RequestHttp("https://api.bybit.com/v2/public/orderBook/L2?symbol=" + symbol, "GET");
            if (str.Substring(0, 14) == "RESPONSE_ERROR")
            {
                this.atb_manager_.AppendLog("ByBit : " + str);
            }
            else
            {
                try
                {
                    ByBit_Response byBitResponse = JsonConvert.DeserializeObject<ByBit_Response>(str);
                    if (byBitResponse == null || byBitResponse.RetCode != 0)
                        return;
                    double ask = 0.0;
                    List<ByBit_OrderBookData> bitOrderBookDataList = JsonConvert.DeserializeObject<List<ByBit_OrderBookData>>(byBitResponse.Result.ToString());
                    if (bitOrderBookDataList == null || bitOrderBookDataList.Count <= 10)
                        return;
                    double price = (double)bitOrderBookDataList[0].Price;
                    for (int index = 1; index < bitOrderBookDataList.Count; ++index)
                    {
                        if (bitOrderBookDataList[index].Side == "Sell")
                        {
                            ask = (double)bitOrderBookDataList[index].Price;
                            break;
                        }
                    }
                    this.atb_manager_.UpdateRate(nameof(ByBit), new DateTime(1970, 1, 1).AddSeconds(double.Parse(byBitResponse.TimeNow)).ToLocalTime(), price, ask);
                }
                catch (Exception ex)
                {
                }
            }
        }

        public override List<Rate> GetOHLCData(string time_frame, int watches)
        {
            string str1 = "1";
            int num1 = 60;
            if (time_frame == "M1")
            {
                str1 = "1";
                num1 = 1;
            }
            else if (time_frame == "M5")
            {
                str1 = "5";
                num1 = 5;
            }
            else if (time_frame == "M15")
            {
                str1 = "15";
                num1 = 15;
            }
            else if (time_frame == "H1")
            {
                str1 = "60";
                num1 = 60;
            }
            else if (time_frame == "H4")
            {
                str1 = "240";
                num1 = 240;
            }
            else if (time_frame == "D1")
            {
                str1 = "D1";
                num1 = 1440;
            }
            long num2 = DateTimeOffset.UtcNow.ToUnixTime() - (long)(watches * num1 * 60);
            string str2 = this.RequestHttp("https://api.bybit.com/v2/public/mark-price-kline?symbol=" + Constants.TradeSymbol + "&interval=" + str1 + "&from=" + num2.ToString(), "GET");
            if (str2.Substring(0, 14) == "RESPONSE_ERROR")
            {
                this.atb_manager_.AppendLog("ByBit : " + str2);
                return (List<Rate>)null;
            }
            try
            {
                ByBit_Response byBitResponse = JsonConvert.DeserializeObject<ByBit_Response>(str2);
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                if (byBitResponse != null)
                {
                    if (byBitResponse.RetCode == 0)
                    {
                        List<ByBit_OHLCData> byBitOhlcDataList = JsonConvert.DeserializeObject<List<ByBit_OHLCData>>(byBitResponse.Result.ToString());
                        if (byBitOhlcDataList == null)
                            return (List<Rate>)null;
                        List<Rate> rateList = new List<Rate>();
                        foreach (ByBit_OHLCData byBitOhlcData in byBitOhlcDataList)
                            rateList.Add(new Rate(dateTime.AddSeconds((double)byBitOhlcData.StartAt).ToLocalTime(), byBitOhlcData.Open, byBitOhlcData.Open, byBitOhlcData.High, byBitOhlcData.High, byBitOhlcData.Low, byBitOhlcData.Low, byBitOhlcData.Close, byBitOhlcData.Close));
                        return rateList;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return (List<Rate>)null;
        }

        public override void Stop()
        {
            this.is_running_ = false;
            this.rate_thread_.Abort();
        }

        public override void Start()
        {
            this.rate_thread_ = new Thread(new ThreadStart(this.ProcessTickerThread));
            this.rate_thread_.IsBackground = true;
            this.rate_thread_.Start();
            this.is_running_ = true;
        }

        public int Logout() => 0;

        public string GetBalances(string currency) => "";

        public override ErrorType SendLimitOrder(OrderRequestData order_request)
        {
            OrderResponseData orderResponseData = new OrderResponseData();
            orderResponseData.account_id = "bybit";
            orderResponseData.symbol = order_request.symbol;
            orderResponseData.trans_type = order_request.signal;
            orderResponseData.request_quantity = order_request.amount;
            orderResponseData.request_price = order_request.request_price;
            orderResponseData.is_market = order_request.is_market;
            string symbol = order_request.symbol;
            string str = order_request.signal == ORDER_TYPE.Buy ? "Buy" : "Sell";
            ByBitOrderResult byBitOrderResult;
            if (order_request.is_stop)
                byBitOrderResult = this.api_client_.PlaceNewConditionalOrder(new Dictionary<string, object>()
        {
          {
            "api_key",
            (object) this.api_key_
          },
          {
            "base_price",
            (object) order_request.current_price.ToString()
          },
          {
            "order_type",
            (object) "Limit"
          },
          {
            "price",
            (object) order_request.request_price.ToString()
          },
          {
            "qty",
            (object) order_request.amount.ToString()
          },
          {
            "side",
            (object) str
          },
          {
            "stop_px",
            (object) order_request.request_price.ToString()
          },
          {
            "symbol",
            (object) symbol
          },
          {
            "time_in_force",
            (object) "GoodTillCancel"
          },
          {
            "timestamp",
            (object) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()
          }
        });
            else
                byBitOrderResult = this.api_client_.PlaceNewOrder(new Dictionary<string, object>()
        {
          {
            "api_key",
            (object) this.api_key_
          },
          {
            "order_type",
            (object) "Limit"
          },
          {
            "price",
            (object) order_request.request_price.ToString()
          },
          {
            "qty",
            (object) order_request.amount.ToString()
          },
          {
            "reduce_only",
            (object) false
          },
          {
            "side",
            (object) str
          },
          {
            "symbol",
            (object) symbol
          },
          {
            "time_in_force",
            (object) "GoodTillCancel"
          },
          {
            "timestamp",
            (object) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()
          }
        });
            if (byBitOrderResult == null)
            {
                this.atb_manager_.AppendLog(this.api_client_.LastMessage);
                return this.api_client_.LastError;
            }
            orderResponseData.accept_quantity = (double)byBitOrderResult.CumExecQuantity;
            orderResponseData.accept_price = order_request.request_price;
            return ErrorType.Success;
        }

        public override ErrorType SendOrder(OrderRequestData order_request)
        {
            OrderResponseData orderResponseData = new OrderResponseData();
            orderResponseData.account_id = "bybit";
            orderResponseData.symbol = order_request.symbol;
            orderResponseData.trans_type = order_request.signal;
            orderResponseData.request_quantity = order_request.amount;
            orderResponseData.request_price = order_request.request_price;
            orderResponseData.is_market = order_request.is_market;
            string symbol = order_request.symbol;
            string str = order_request.signal == ORDER_TYPE.Buy ? "Buy" : "Sell";
            Dictionary<string, object> order_query = new Dictionary<string, object>()
      {
        {
          "api_key",
          (object) this.api_key_
        },
        {
          "order_type",
          order_request.is_market ? (object) "Market" : (object) "Limit"
        },
        {
          "price",
          (object) order_request.request_price.ToString()
        },
        {
          "qty",
          (object) order_request.amount.ToString()
        },
        {
          "reduce_only",
          (object) false
        },
        {
          "side",
          (object) str
        },
        {
          "symbol",
          (object) symbol
        },
        {
          "time_in_force",
          (object) "GoodTillCancel"
        },
        {
          "timestamp",
          (object) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()
        }
      };
            if (order_request.is_market)
                order_query.Remove("price");
            ByBitOrderResult byBitOrderResult = this.api_client_.PlaceNewOrder(order_query);
            if (byBitOrderResult == null)
            {
                this.atb_manager_.AppendLog(this.api_client_.LastMessage);
                return this.api_client_.LastError;
            }
            orderResponseData.accept_quantity = (double)byBitOrderResult.CumExecQuantity;
            orderResponseData.accept_price = order_request.request_price;
            return ErrorType.Success;
        }
    }
}
