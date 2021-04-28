using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace BTCMachine
{
    public class ByBit_PrivateApi
    {
        private const string WalletBalanceUrl = "/v2/private/wallet/balance";
        private const string PlaceOrderUrl = "/v2/private/order/create";
        private const string PlaceConditionalOrderUrl = "/v2/private/stop-order/create";
        private const string CancelActiveOrderUrl = "/v2/private/order/cancel";
        private const string CancelAllActiveOrdersUrl = "/v2/private/order/cancelAll";
        private const string CancelAllConditionalOrdersUrl = "/v2/private/stop-order/cancelAll";
        private const string PositionListUrl = "/v2/private/position/list";
        private const string ActiveOrderListUrl = "/v2/private/order/list";
        private const string ConditionalOrderListUrl = "/v2/private/stop-order/list";
        private const string APIInfoUrl = "/v2/private/account/api-key";
        private readonly string api_key_;
        private readonly string api_sec_;
        public ErrorType LastError;
        public string LastMessage = "";
        private Logging logging_ = new Logging();

        public ByBit_PrivateApi(string api_key, string api_sec)
        {
            this.api_key_ = api_key;
            this.api_sec_ = api_sec;
            this.logging_.SetLogPrefix("ByBit");
        }

        public ByBitMarginResult GetMargin()
        {
            ByBit_Response byBitResponse = this.Get<ByBit_Response>("/v2/private/wallet/balance", new Dictionary<string, object>()
      {
        {
          "api_key",
          (object) this.api_key_
        },
        {
          "timestamp",
          (object) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()
        }
      });
            if (byBitResponse != null && byBitResponse.Result != null)
            {
                ByBitMarginResult byBitMarginResult = JsonConvert.DeserializeObject<ByBitMarginResult>(byBitResponse.Result.ToString());
                if (byBitMarginResult != null)
                    return byBitMarginResult;
            }
            return (ByBitMarginResult)null;
        }

        public string GetAPIInfo()
        {
            ByBit_Response byBitResponse = this.Get<ByBit_Response>("/v2/private/account/api-key", new Dictionary<string, object>()
      {
        {
          "api_key",
          (object) this.api_key_
        },
        {
          "timestamp",
          (object) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()
        }
      });
            if (byBitResponse != null && byBitResponse.Result != null)
            {
                List<ByBit_APIInfo> byBitApiInfoList = JsonConvert.DeserializeObject<List<ByBit_APIInfo>>(byBitResponse.Result.ToString());
                if (byBitApiInfoList != null)
                    return byBitApiInfoList[0].UserID.ToString();
            }
            return "";
        }

        public ByBit_Position[] GetPositions(string symbol)
        {
            ByBit_Response byBitResponse = this.Get<ByBit_Response>("/v2/private/position/list", new Dictionary<string, object>()
      {
        {
          "api_key",
          (object) this.api_key_
        },
        {
          nameof (symbol),
          (object) symbol
        },
        {
          "timestamp",
          (object) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()
        }
      });
            if (byBitResponse != null && byBitResponse.Result != null)
            {
                ByBit_Position byBitPosition = JsonConvert.DeserializeObject<ByBit_Position>(byBitResponse.Result.ToString());
                if (byBitPosition != null)
                {
                    List<ByBit_Position> byBitPositionList = new List<ByBit_Position>();
                    if (double.Parse(byBitPosition.Size) > 0.0)
                        byBitPositionList.Add(byBitPosition);
                    return byBitPositionList.ToArray();
                }
            }
            return (ByBit_Position[])null;
        }

        public ByBit_Order[] GetOrders(string symbol)
        {
            Dictionary<string, object> query = new Dictionary<string, object>()
      {
        {
          "api_key",
          (object) this.api_key_
        },
        {
          "order_status",
          (object) "New,PartiallyFilled"
        },
        {
          nameof (symbol),
          (object) symbol
        },
        {
          "timestamp",
          (object) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()
        }
      };
            try
            {
                ByBit_Response byBitResponse = this.Get<ByBit_Response>("/v2/private/order/list", query);
                if (byBitResponse != null)
                {
                    ByBit_OrderResult byBitOrderResult = JsonConvert.DeserializeObject<ByBit_OrderResult>(byBitResponse.Result.ToString());
                    if (byBitOrderResult != null)
                        return byBitOrderResult.Data.ToArray();
                }
            }
            catch (Exception ex)
            {
            }
            return (ByBit_Order[])null;
        }

        public ByBit_Order[] GetConditionalOrders(string symbol)
        {
            Dictionary<string, object> query = new Dictionary<string, object>()
      {
        {
          "api_key",
          (object) this.api_key_
        },
        {
          "stop_order_status",
          (object) "Untriggered"
        },
        {
          nameof (symbol),
          (object) symbol
        },
        {
          "timestamp",
          (object) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()
        }
      };
            try
            {
                ByBit_Response byBitResponse = this.Get<ByBit_Response>("/v2/private/stop-order/list", query);
                if (byBitResponse != null)
                {
                    ByBit_OrderResult byBitOrderResult = JsonConvert.DeserializeObject<ByBit_OrderResult>(byBitResponse.Result.ToString());
                    if (byBitOrderResult != null)
                        return byBitOrderResult.Data.ToArray();
                }
            }
            catch (Exception ex)
            {
            }
            return (ByBit_Order[])null;
        }

        public void CancelAllOrders(string symbol)
        {
            Dictionary<string, object> source = new Dictionary<string, object>()
      {
        {
          "api_key",
          (object) this.api_key_
        },
        {
          nameof (symbol),
          (object) symbol
        },
        {
          "timestamp",
          (object) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()
        }
      };
            string signature = this.CreateSignature(this.api_sec_, source.ToQueryString().Substring(1));
            this.Post<ByBit_Response>("/v2/private/order/cancelAll", (Dictionary<string, object>)null, (object)new ByBitCancelAllRequest()
            {
                APIKey = (string)source["api_key"],
                Symbol = (string)source[nameof(symbol)],
                TimeStamp = (string)source["timestamp"],
                Sign = signature
            });
        }

        public void CancelAllConditionalOrders(string symbol)
        {
            Dictionary<string, object> source = new Dictionary<string, object>()
      {
        {
          "api_key",
          (object) this.api_key_
        },
        {
          nameof (symbol),
          (object) symbol
        },
        {
          "timestamp",
          (object) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()
        }
      };
            string signature = this.CreateSignature(this.api_sec_, source.ToQueryString().Substring(1));
            this.Post<ByBit_Response>("/v2/private/stop-order/cancelAll", (Dictionary<string, object>)null, (object)new ByBitCancelAllRequest()
            {
                APIKey = (string)source["api_key"],
                Symbol = (string)source[nameof(symbol)],
                TimeStamp = (string)source["timestamp"],
                Sign = signature
            });
        }

        public ByBitOrderResult PlaceNewOrder(Dictionary<string, object> order_query)
        {
            string signature = this.CreateSignature(this.api_sec_, order_query.ToQueryString().Substring(1).Replace("True", "true").Replace("False", "false"));
            ByBit_Response byBitResponse = this.Post<ByBit_Response>("/v2/private/order/create", (Dictionary<string, object>)null, (object)new ByBitOrderRequest()
            {
                APIKey = (string)order_query["api_key"],
                OrderType = (string)order_query["order_type"],
                Quantity = (string)order_query["qty"],
                ReduceOnly = false,
                Side = (string)order_query["side"],
                Symbol = (string)order_query["symbol"],
                TimeInForce = (string)order_query["time_in_force"],
                TimeStamp = (string)order_query["timestamp"],
                Sign = signature
            });
            if (byBitResponse == null)
                return (ByBitOrderResult)null;
            if (byBitResponse.Result != null)
                return JsonConvert.DeserializeObject<ByBitOrderResult>(byBitResponse.Result.ToString());
            this.LastMessage = byBitResponse.RetMsg;
            this.LastError = ErrorType.UnknownError;
            return (ByBitOrderResult)null;
        }

        public ByBitOrderResult PlaceNewConditionalOrder(
          Dictionary<string, object> order_query)
        {
            string signature = this.CreateSignature(this.api_sec_, order_query.ToQueryString().Substring(1).Replace("True", "true").Replace("False", "false"));
            ByBit_Response byBitResponse = this.Post<ByBit_Response>("/v2/private/stop-order/create", (Dictionary<string, object>)null, (object)new ByBitConditionalOrderRequest()
            {
                APIKey = (string)order_query["api_key"],
                BasePrice = (string)order_query["base_price"],
                OrderType = (string)order_query["order_type"],
                Price = (string)order_query["price"],
                Quantity = (string)order_query["qty"],
                Side = (string)order_query["side"],
                StopPrice = (string)order_query["stop_px"],
                Symbol = (string)order_query["symbol"],
                TimeInForce = (string)order_query["time_in_force"],
                TimeStamp = (string)order_query["timestamp"],
                Sign = signature
            });
            if (byBitResponse == null)
                return (ByBitOrderResult)null;
            if (byBitResponse.Result != null)
                return JsonConvert.DeserializeObject<ByBitOrderResult>(byBitResponse.Result.ToString());
            this.LastMessage = byBitResponse.RetMsg;
            this.LastError = ErrorType.UnknownError;
            return (ByBitOrderResult)null;
        }

        internal T Get<T>(string path, Dictionary<string, object> query = null) => this.SendRequest<T>(HttpMethod.Get, path, query);

        internal T Post<T>(string path, object body) => this.SendRequest<T>(HttpMethod.Post, path, body: body);

        internal T Post<T>(string path, Dictionary<string, object> query, object body) => this.SendRequest<T>(HttpMethod.Post, path, query, body);

        private T SendRequest<T>(
          HttpMethod method,
          string path,
          Dictionary<string, object> query = null,
          object body = null)
        {
            this.LastError = ErrorType.Success;
            string str = this.SendRequest(method, path, query, body);
            try
            {
                return JsonConvert.DeserializeObject<T>(str);
            }
            catch (Exception ex)
            {
            }
            return default(T);
        }

        private string SendRequest(
          HttpMethod method,
          string path,
          Dictionary<string, object> query = null,
          object body = null)
        {
            string str1 = string.Empty;
            if (query != null)
                str1 = query.ToQueryString();
            string str2 = "";
            if (body != null)
                str2 = JsonConvert.SerializeObject(body);
            string requestUriString = ByBitConstants.BaseUrl + path;
            if (!string.IsNullOrEmpty(str1))
                requestUriString += str1;
            if (method == HttpMethod.Get)
            {
                string signature = this.CreateSignature(this.api_sec_, str1.Substring(1));
                requestUriString = requestUriString + "&sign=" + signature;
            }
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = method.Method;
            if (!string.IsNullOrEmpty(str2))
            {
                using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    streamWriter.Write(str2);
            }
            try
            {
                using (WebResponse response = httpWebRequest.GetResponse())
                {
                    Stream responseStream = response.GetResponseStream();
                    if (responseStream == null)
                        return "";
                    using (StreamReader streamReader = new StreamReader(responseStream))
                        return streamReader.ReadToEnd();
                }
            }
            catch (WebException ex1)
            {
                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)ex1.Response)
                    {
                        Stream responseStream = response.GetResponseStream();
                        if (responseStream == null)
                            return "";
                        using (StreamReader streamReader = new StreamReader(responseStream))
                            return streamReader.ReadToEnd();
                    }
                }
                catch (Exception ex2)
                {
                    return "";
                }
            }
        }

        private string CreateSignature(string secret, string message) => this.ByteArrayToString(this.Hmacsha256(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(message)));

        private byte[] Hmacsha256(byte[] keyByte, byte[] messageBytes)
        {
            using (HMACSHA256 hmacshA256 = new HMACSHA256(keyByte))
                return hmacshA256.ComputeHash(messageBytes);
        }

        private string ByteArrayToString(byte[] ba)
        {
            StringBuilder stringBuilder = new StringBuilder(ba.Length * 2);
            foreach (byte num in ba)
                stringBuilder.AppendFormat("{0:x2}", (object)num);
            return stringBuilder.ToString();
        }
    }
}
