using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace BTCMachine
{
    public class Broker
    {
        protected DoTenTraderManager atb_manager_;
        protected string api_key_;
        protected string api_sec_;
        public bool is_running_;

        public Broker(DoTenTraderManager atb_manager)
        {
            this.atb_manager_ = atb_manager;
            this.is_running_ = true;
        }

        public bool LoginBasic(string api_key, string api_sec)
        {
            if (string.IsNullOrEmpty(api_key) || string.IsNullOrEmpty(api_sec))
                return false;
            this.api_key_ = api_key;
            this.api_sec_ = api_sec;
            return true;
        }

        public virtual ErrorType Login(string api_key, string api_sec) => ErrorType.UnknownError;

        public virtual string APIInfo() => "";

        public virtual Margin GetMargin() => (Margin)null;

        public virtual List<Position> GetPositions(string symbol) => (List<Position>)null;

        public virtual List<Order> GetOrders(string symbol) => (List<Order>)null;

        public virtual ErrorType SendOrder(OrderRequestData order_request) => ErrorType.UnknownError;

        public virtual ErrorType SendLimitOrder(OrderRequestData order_request) => ErrorType.UnknownError;

        public virtual void CancelAllOrders(string symbol)
        {
        }

        public virtual List<Rate> GetOHLCData(string time_frame, int hours) => (List<Rate>)null;

        public virtual void Stop()
        {
        }

        public virtual void Start()
        {
        }

        protected string RequestHttp(string url, string method)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = method;
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
                                return streamReader.ReadToEnd();
                            }
                            catch (Exception ex)
                            {
                                return "RESPONSE_ERROR Exception while read stream " + ex.Message;
                            }
                        }
                    }
                }
            }
            catch (WebException ex1)
            {
                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)ex1.Response)
                    {
                        if (response == null)
                            return "RESPONSE_ERROR " + ex1.Message;
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(responseStream))
                                return "RESPONSE_ERROR " + ex1.Message + "|" + streamReader.ReadToEnd();
                        }
                    }
                }
                catch (Exception ex2)
                {
                    return "RESPONSE_ERROR Exception" + ex2.Message;
                }
            }
        }
    }
}
