using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace BTCMachine
{
    public class DoTenTraderMain : Form
    {
        private DoTenTraderManager atb_manager_;
        private Thread rate_thread_;
        private string current_broker_;
        private string current_timeframe_;
        private IContainer components;
        private ComboBox comboBroker;
        private ComboBox comboTimeFrame;
        private Button btnSettings;
        private TextBox textAPIKey;
        private Label labelAutoLot;
        private Label labelWatchHour;
        private Label labelAPIKey;
        private Label labelBroker;
        private TextBox textWatchHours;
        private TextBox textAutoLot;
        private Button buttonAutoTrade;
        private Button buttonBuy;
        private Button buttonSell;
        private Label labelTradeLot;
        private TextBox textLot;
        private Label labelAsk;
        private Label labelBid;
        private Label labelQuote;
        private GroupBox groupAPIInformation;
        private GroupBox groupAutoTrade;
        private GroupBox groupManualTrade;
        private ListView listQuote;
        private ColumnHeader colOpen;
        private ColumnHeader colHigh;
        private ColumnHeader colLow;
        private ColumnHeader colClose;
        private ColumnHeader colHighest;
        private ListView listPositions;
        private ColumnHeader colOpenDateTime;
        private ColumnHeader colOpenPrice;
        private ColumnHeader colCurrentPrice;
        private ColumnHeader colSize;
        private Label labelBalance;
        private Label labelEquity;
        private Label labelBalanceValue;
        private Label labelEquityValue;
        private ListView listLogs;
        private ColumnHeader colLogDateTime;
        private ColumnHeader colDescription;
        private ComboBox comboSymbol;
        private ColumnHeader colPNL;
        private Label labelSymbol;
        private Label labelTimeFrame;
        private Button btnLanguage;
        private Button buttonStop;
        private Label labelAutoCurrency;
        private Label labelManualCurrency;
        private ColumnHeader colType;
        private TextBox textLimitPrice;
        private Label labelLimitPrice;
        private TabControl tabWindow;
        private TabPage tabPositionPage;
        private TabPage tabOrderPage;
        private ListView listOrders;
        private ColumnHeader colOrderDateTime;
        private ColumnHeader colOrderSize;
        private ColumnHeader colOrderType;
        private ColumnHeader colOrderPrice;
        private ColumnHeader colOrderCurrentPrice;
        private ColumnHeader colOrderOutStandingSize;
        private ContextMenuStrip contextMenuOrders;
        private ToolStripMenuItem menuCancelAll;
        private Button buttonCloseAll;
        private TabPage tabLogs;
        private ContextMenuStrip contextMenuLogs;
        private ToolStripMenuItem menuClearLogs;
        private ColumnHeader colLowest;
        private TextBox textLineNotify;
        private CheckBox checkToken;
        private Label labelUserID;

        public DoTenTraderMain()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Directory.CreateDirectory("log");
            this.atb_manager_ = new DoTenTraderManager(this);
            this.atb_manager_.IsEnglish = true;
            this.InitializeComponent();
            if (Constants.Mode == "Free")
                this.Text = "BTC Machine Ex";
            this.ChangeLanguage(false);
            this.InitTimeframes();
            this.InitQuoteList();
            this.RefereshBrokers();
            this.rate_thread_ = new Thread(new ThreadStart(this.MarketDataProcess));
            this.rate_thread_.IsBackground = true;
            this.rate_thread_.Start();
        }

        private void InitQuoteList()
        {
            string[] items = new string[6];
            for (int index = 0; index < 6; ++index)
                items[index] = "0";
            this.listQuote.Items.Add(new ListViewItem(items)
            {
                Name = "Quote"
            });
        }

        private void InitTimeframes()
        {
            foreach (object timeFrame in Constants.TimeFrames)
                this.comboTimeFrame.Items.Add(timeFrame);
            this.comboTimeFrame.SelectedIndex = 3;
            this.current_timeframe_ = this.comboTimeFrame.Text;
        }

        private void RefereshBrokers()
        {
            this.comboBroker.Items.Clear();
            foreach (KeyValuePair<string, BrokerInfo> broker in this.atb_manager_.BrokerList)
                this.comboBroker.Items.Add((object)broker.Key);
            this.comboBroker.SelectedIndex = 0;
            this.RefreshBrokerInfo();
        }

        private void RefreshBrokerInfo()
        {
            string str1 = this.comboBroker.SelectedItem.ToString();
            string str2 = "";
            if (this.atb_manager_.BrokerList.ContainsKey(str1))
            {
                this.textAPIKey.Text = this.atb_manager_.BrokerList[str1].APIKey;
                str2 = this.atb_manager_.BrokerList[str1].Symbols;
            }
            this.current_broker_ = str1;
            this.comboSymbol.Items.Clear();
            this.comboSymbol.Items.Add((object)str2);
            this.comboSymbol.SelectedIndex = 0;
            if (str1 == "Bitflyer")
            {
                this.labelAutoCurrency.Text = "BTC";
                this.labelManualCurrency.Text = "BTC";
                this.textAutoLot.Text = "0.01";
                this.textLot.Text = "0.01";
            }
            else if (str1 == "ByBit")
            {
                this.labelAutoCurrency.Text = "USD";
                this.labelManualCurrency.Text = "USD";
                this.textAutoLot.Text = "100";
                this.textLot.Text = "100";
            }
            this.atb_manager_.LoginBroker(str1);
            this.atb_manager_.CurrentBroker = str1;
            this.atb_manager_.InitOHLCData();
            this.UpdateEquity(0.0, 0.0);
            this.UpdateTickLabel(0.0, 0.0);
            this.UpdateQuoteList(0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            this.UpdatePositionList((List<Position>)null);
        }

        private void RefreshTimeFrame()
        {
            this.current_timeframe_ = this.comboTimeFrame.Text;
            this.atb_manager_.ChangeTimeFrame(this.current_broker_, this.comboTimeFrame.Text);
        }

        private void btnLanguage_Click(object sender, EventArgs e) => this.ChangeLanguage(this.btnLanguage.Text == "EN");

        private void ChangeLanguage(bool isEnglish)
        {
            this.atb_manager_.IsEnglish = isEnglish;
            if (isEnglish)
            {
                this.ChangeToEnglish();
                this.btnLanguage.Text = "JP";
            }
            else
            {
                this.ChangeToJapanese();
                this.btnLanguage.Text = "EN";
            }
        }

        private void ChangeToEnglish()
        {
            this.labelBroker.Text = "Broker";
            this.labelAPIKey.Text = "APIKey";
            this.labelBalance.Text = "Balance";
            this.labelEquity.Text = "Equity";
            this.labelSymbol.Text = "Symbol";
            this.labelTimeFrame.Text = "Timeframe";
            this.labelWatchHour.Text = "Watch Hours";
            this.labelAutoLot.Text = "Size";
            this.labelLimitPrice.Text = "Limit Price";
            this.labelTradeLot.Text = "Size";
            this.checkToken.Text = "Line Token";
            this.labelQuote.Text = "Quote Data";
            this.tabWindow.Text = "Logs";
            this.groupAPIInformation.Text = "API Information";
            this.groupAutoTrade.Text = "Auto Trade";
            this.groupManualTrade.Text = "Manual Trade";
            this.colOpen.Text = "Open";
            this.colHigh.Text = "High";
            this.colLow.Text = "Low";
            this.colClose.Text = "Close";
            this.colHighest.Text = "Highest";
            this.colLowest.Text = "Lowest";
            this.colOpenDateTime.Text = "DateTime";
            this.colSize.Text = "Size";
            this.colType.Text = "Type";
            this.colOpenPrice.Text = "Open Price";
            this.colCurrentPrice.Text = "Current Price";
            this.colPNL.Text = "PNL";
            this.colOrderDateTime.Text = "DateTime";
            this.colOrderSize.Text = "Size";
            this.colOrderType.Text = "Type";
            this.colOrderPrice.Text = "Request Price";
            this.colOrderCurrentPrice.Text = "Current Price";
            this.colOrderOutStandingSize.Text = "OutStanding";
            this.colLogDateTime.Text = "DateTime";
            this.colDescription.Text = "Description";
            this.buttonAutoTrade.Text = "Start";
            this.buttonStop.Text = "Stop";
            this.buttonBuy.Text = "Buy";
            this.buttonSell.Text = "Sell";
            this.buttonCloseAll.Text = "Close All";
            this.menuCancelAll.Text = "Cancel All";
            this.menuClearLogs.Text = "Clear All";
            this.tabOrderPage.Text = "Orders";
            this.tabPositionPage.Text = "Positions";
            this.tabLogs.Text = "Logs";
        }

        private void ChangeToJapanese()
        {
            this.labelBroker.Text = "取引所";
            this.labelAPIKey.Text = "APIKey";
            this.labelBalance.Text = "残高";
            this.labelEquity.Text = "イクィティ";
            this.labelSymbol.Text = "通貨ペア";
            this.labelTimeFrame.Text = "分足";
            this.labelWatchHour.Text = "注目時間";
            this.labelAutoLot.Text = "数量";
            this.labelLimitPrice.Text = "価格";
            this.labelTradeLot.Text = "数量";
            this.checkToken.Text = "ライントークン";
            this.labelQuote.Text = "価格データ";
            this.tabWindow.Text = "ログ";
            this.groupAPIInformation.Text = "API情報";
            this.groupAutoTrade.Text = "自動取引";
            this.groupManualTrade.Text = "手動取引";
            this.colOpen.Text = "始値";
            this.colHigh.Text = "高値";
            this.colLow.Text = "安値";
            this.colClose.Text = "終値";
            this.colHighest.Text = "最高価";
            this.colLowest.Text = "最低価";
            this.colOpenDateTime.Text = "日付時間";
            this.colSize.Text = "取引数量";
            this.colType.Text = "買/売";
            this.colOpenPrice.Text = "参入価格";
            this.colCurrentPrice.Text = "価格";
            this.colPNL.Text = "利益";
            this.colOrderDateTime.Text = "日付時間";
            this.colOrderSize.Text = "取引数量";
            this.colOrderType.Text = "買/売";
            this.colOrderPrice.Text = "チャレンジ価格";
            this.colOrderCurrentPrice.Text = "価格";
            this.colOrderOutStandingSize.Text = "未締結数量";
            this.colLogDateTime.Text = "日付時間";
            this.colDescription.Text = "説明";
            this.buttonAutoTrade.Text = "スタート";
            this.buttonStop.Text = "静止";
            this.buttonBuy.Text = "買い";
            this.buttonSell.Text = "売り";
            this.buttonCloseAll.Text = "一斉決済";
            this.menuCancelAll.Text = "一斉取消";
            this.menuClearLogs.Text = "すべてクリアする";
            this.tabOrderPage.Text = "ペンディング";
            this.tabPositionPage.Text = "ポジション";
            this.tabLogs.Text = "ログ";
        }

        private void MarketDataProcess()
        {
            while (true)
            {
                this.atb_manager_.OnTick(this.current_broker_, this.current_timeframe_);
                Thread.Sleep(1200);
            }
        }

        public void SetControlVisible(Control control, bool value)
        {
            if (control.InvokeRequired)
                this.Invoke((Delegate)new DoTenTraderMain.SetConrolVisibleCallBack(this.SetControlVisible), (object)control, (object)value);
            else
                control.Visible = value;
        }

        public void SetControlEnabled(Control control, bool value)
        {
            if (control.InvokeRequired)
                this.Invoke((Delegate)new DoTenTraderMain.SetConrolEnabledCallBack(this.SetControlEnabled), (object)control, (object)value);
            else
                control.Enabled = value;
        }

        public void SetLabelText(Label label, string text)
        {
            if (label.InvokeRequired)
                this.Invoke((Delegate)new DoTenTraderMain.SetLabelTextCallBack(this.SetLabelText), (object)label, (object)text);
            else
                label.Text = text;
        }

        public void AppendLog(string time, string message)
        {
            if (this.listLogs.InvokeRequired)
                this.Invoke((Delegate)new DoTenTraderMain.AppendLogListCallBack(this.AppendLog), (object)time, (object)message);
            else
                this.listLogs.Items.Insert(0, new ListViewItem(new string[2]
                {
          time,
          message
                }));
        }

        public void UpdateQuoteList(
          double open,
          double high,
          double low,
          double close,
          double highest,
          double lowest)
        {
            if (this.listQuote.InvokeRequired)
            {
                this.Invoke((Delegate)new DoTenTraderMain.SetQuoteListCallBack(this.UpdateQuoteList), (object)open, (object)high, (object)low, (object)close, (object)highest, (object)lowest);
            }
            else
            {
                this.listQuote.Items[0].SubItems[0].Text = open.ToString();
                this.listQuote.Items[0].SubItems[1].Text = high.ToString();
                this.listQuote.Items[0].SubItems[2].Text = low.ToString();
                this.listQuote.Items[0].SubItems[3].Text = close.ToString();
                this.listQuote.Items[0].SubItems[4].Text = highest.ToString();
                this.listQuote.Items[0].SubItems[5].Text = lowest.ToString();
            }
        }

        public void UpdateTickLabel(double bid, double ask)
        {
            this.SetLabelText(this.labelBid, bid.ToString());
            this.SetLabelText(this.labelAsk, ask.ToString());
        }

        public void UpdateEquity(double equity, double balance)
        {
            this.SetLabelText(this.labelEquityValue, equity.ToString());
            this.SetLabelText(this.labelBalanceValue, balance.ToString());
        }

        public void UpdatePositionList(List<Position> position_list)
        {
            if (this.listPositions.InvokeRequired)
            {
                this.Invoke((Delegate)new DoTenTraderMain.SetPositionListCallBack(this.UpdatePositionList), (object)position_list);
            }
            else
            {
                if (position_list == null)
                    return;
                if (position_list.Count == 0)
                {
                    this.listPositions.Items.Clear();
                }
                else
                {
                    bool flag = false;
                    if (this.listPositions.Items.Count == position_list.Count)
                    {
                        for (int index = 0; index < position_list.Count; ++index)
                        {
                            string str1 = position_list[index].OpenDateTime.ToString("yyyy/MM/dd HH:mm:ss");
                            string text = this.listPositions.Items[index].SubItems[0].Text;
                            double num = 0.0;
                            string str2;
                            if (this.listPositions.Items[index].SubItems[2].Name == "Buy")
                            {
                                str2 = this.atb_manager_.IsEnglish ? "Buy" : "買";
                                num = this.atb_manager_.CurrentTick(this.current_broker_).Bid;
                            }
                            else
                            {
                                str2 = this.atb_manager_.IsEnglish ? "Sell" : "売";
                                num = this.atb_manager_.CurrentTick(this.current_broker_).Ask;
                            }
                            this.listPositions.Items[index].SubItems[2].Text = str2;
                            this.listPositions.Items[index].SubItems[4].Text = num.ToString();
                            string str3 = text;
                            if (str1 != str3)
                                flag = true;
                        }
                    }
                    else
                        flag = true;
                    if (!flag)
                        return;
                    this.listPositions.Items.Clear();
                    foreach (Position position in position_list)
                        this.listPositions.Items.Add(new ListViewItem(new string[6]
                        {
              position.OpenDateTime.ToString("yyyy/MM/dd HH:mm:ss"),
              position.Volume.ToString(),
              position.Type != 1 ? (this.atb_manager_.IsEnglish ? "Sell" : "売") : (this.atb_manager_.IsEnglish ? "Buy" : "買"),
              position.OpenPrice.ToString(),
              "XXX",
              position.PnL.ToString()
                        })
                        {
                            SubItems = {
                [2] = {
                  Name = position.Type == 1 ? "Buy" : "Sell"
                }
              },
                            Name = position.PositionID
                        });
                }
            }
        }

        public void UpdateOrderList(List<Order> order_list)
        {
            try
            {
                if (this.listOrders.InvokeRequired)
                {
                    this.Invoke((Delegate)new DoTenTraderMain.SetOrderListCallBack(this.UpdateOrderList), (object)order_list);
                }
                else
                {
                    if (order_list == null)
                        return;
                    if (order_list.Count == 0)
                    {
                        this.listOrders.Items.Clear();
                    }
                    else
                    {
                        bool flag = false;
                        if (this.listOrders.Items.Count == order_list.Count)
                        {
                            for (int index = 0; index < order_list.Count; ++index)
                            {
                                string str1 = order_list[index].OpenDateTime.ToString("yyyy/MM/dd HH:mm:ss");
                                string text = this.listOrders.Items[index].SubItems[0].Text;
                                double num = 0.0;
                                string str2;
                                if (this.listOrders.Items[index].SubItems[2].Name == "Buy")
                                {
                                    str2 = this.atb_manager_.IsEnglish ? "Buy" : "買";
                                    num = this.atb_manager_.CurrentTick(this.current_broker_).Bid;
                                }
                                else
                                {
                                    str2 = this.atb_manager_.IsEnglish ? "Sell" : "売";
                                    num = this.atb_manager_.CurrentTick(this.current_broker_).Ask;
                                }
                                this.listOrders.Items[index].SubItems[2].Text = str2;
                                this.listOrders.Items[index].SubItems[5].Text = num.ToString();
                                string str3 = text;
                                if (str1 != str3)
                                    flag = true;
                            }
                        }
                        else
                            flag = true;
                        if (!flag)
                            return;
                        this.listOrders.Items.Clear();
                        foreach (Order order in order_list)
                        {
                            string[] items = new string[6];
                            items[0] = order.OpenDateTime.ToString("yyyy/MM/dd HH:mm:ss");
                            double num = order.Volume;
                            items[1] = num.ToString();
                            items[2] = order.Type != 1 ? (this.atb_manager_.IsEnglish ? "Sell" : "売") : (this.atb_manager_.IsEnglish ? "Buy" : "買");
                            num = order.OutStandingSize;
                            items[3] = num.ToString();
                            num = order.OpenPrice;
                            items[4] = num.ToString();
                            items[5] = "XXX";
                            this.listOrders.Items.Add(new ListViewItem(items)
                            {
                                SubItems = {
                  [2] = {
                    Name = order.Type == 1 ? "Buy" : "Sell"
                  }
                },
                                Name = order.OrderID
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private bool CheckParameters()
        {
            double result1;
            if (!double.TryParse(this.textAutoLot.Text, out result1))
            {
                this.textAutoLot.Text = "100";
                return false;
            }
            if (result1 < 100.0)
            {
                this.textAutoLot.Text = "100";
                return false;
            }
            if (Constants.Mode != "Free" && result1 > 500.0)
            {
                this.textAutoLot.Text = "500";
                return false;
            }
            int result2;
            if (!int.TryParse(this.textWatchHours.Text, out result2) || result2 < 0)
                return false;
            if (result2 > 200)
            {
                result2 = 200;
                this.textWatchHours.Text = "200";
            }
            this.atb_manager_.AutoLotParam = result1;
            this.atb_manager_.WatchHours = result2;
            this.atb_manager_.LineToken = this.textLineNotify.Text;
            this.atb_manager_.IsToken = this.checkToken.Checked;
            return true;
        }

        private void EnableButtons(bool is_enable)
        {
            this.SetControlEnabled((Control)this.comboBroker, is_enable);
            this.SetControlEnabled((Control)this.btnSettings, is_enable);
            this.SetControlEnabled((Control)this.comboSymbol, is_enable);
            this.SetControlEnabled((Control)this.comboTimeFrame, is_enable);
            this.SetControlEnabled((Control)this.textWatchHours, is_enable);
            this.SetControlEnabled((Control)this.textAutoLot, is_enable);
        }

        public void ShowMessageBox()
        {
            if (this.atb_manager_.IsEnglish)
            {
                int num1 = (int)MessageBox.Show("Auto trade has stopped because of many errors.");
            }
            else
            {
                int num2 = (int)MessageBox.Show("多くのエラーが発生して自動取引が中断されました。");
            }
        }

        public void SetUserID(string user_id) => this.labelUserID.Text = user_id;

        public void StopAutoTrade()
        {
            this.SetControlVisible((Control)this.buttonStop, false);
            this.SetControlVisible((Control)this.buttonAutoTrade, true);
            this.EnableButtons(true);
            this.atb_manager_.IsLogicRunning = false;
        }

        private void btnStop_Click(object sender, EventArgs e) => this.StopAutoTrade();

        private void btnAutoTrade_Click(object sender, EventArgs e)
        {
            if (!this.atb_manager_.IsAllowed)
            {
                int num = (int)MessageBox.Show(this.atb_manager_.IsEnglish ? "Not registerd" : "登録されていないユーザーです。");
            }
            else
            {
                if (!this.CheckParameters())
                    return;
                this.SetControlVisible((Control)this.buttonStop, true);
                this.SetControlVisible((Control)this.buttonAutoTrade, false);
                this.EnableButtons(false);
                this.atb_manager_.IsLogicRunning = true;
                this.atb_manager_.ErrorCount = 0;
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            if (new BrokerSettings(this.atb_manager_).ShowDialog() != DialogResult.OK)
                return;
            this.atb_manager_.SaveInformations("Broker.cfg");
            this.RefreshBrokerInfo();
        }

        private void comboBroker_SelectionChangeCommitted(object sender, EventArgs e) => this.RefreshBrokerInfo();

        private void comboTimeFrame_SelectionChangeCommitted(object sender, EventArgs e)
        {
        }

        private void DoTenTraderMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.atb_manager_.Stop();
            this.rate_thread_.Abort();
            Environment.Exit(0);
        }

        private void btnClearLog_Click(object sender, EventArgs e) => this.listLogs.Items.Clear();

        private void btnBuy_Click(object sender, EventArgs e)
        {
            if (!this.atb_manager_.IsAllowed)
            {
                int num1 = (int)MessageBox.Show(this.atb_manager_.IsEnglish ? "Not registerd" : "登録されていないユーザーです。");
            }
            else
            {
                double result = 0.0;
                if (!double.TryParse(this.textLot.Text, out result))
                    return;
                int num2 = (int)this.atb_manager_.SendOrder(this.current_broker_, result, 0.0, true);
            }
        }

        private void btnSell_Click(object sender, EventArgs e)
        {
            if (!this.atb_manager_.IsAllowed)
            {
                int num1 = (int)MessageBox.Show(this.atb_manager_.IsEnglish ? "Not registerd" : "登録されていないユーザーです。");
            }
            else
            {
                double result = 0.0;
                if (!double.TryParse(this.textLot.Text, out result))
                    return;
                int num2 = (int)this.atb_manager_.SendOrder(this.current_broker_, result, 0.0, false);
            }
        }

        private void menuCancelAll_Click(object sender, EventArgs e) => this.atb_manager_.CancelAll(this.current_broker_);

        private void listOrders_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            this.listOrders.HitTest(e.X, e.Y);
            this.contextMenuOrders.Show((Control)sender, e.X, e.Y);
        }

        private void buttonCloseAll_Click(object sender, EventArgs e)
        {
            if (!this.atb_manager_.IsAllowed)
            {
                int num = (int)MessageBox.Show(this.atb_manager_.IsEnglish ? "Not registerd" : "登録されていないユーザーです。");
            }
            else
                this.atb_manager_.CloseAllPositionImmediate(this.current_broker_);
        }

        private void menuClearLogs_Click(object sender, EventArgs e) => this.listLogs.Items.Clear();

        private void listLogs_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            this.listLogs.HitTest(e.X, e.Y);
            this.contextMenuLogs.Show((Control)sender, e.X, e.Y);
        }

        private void labelAsk_MouseClick(object sender, MouseEventArgs e) => this.textLimitPrice.Text = this.labelAsk.Text;

        private void labelBid_MouseClick(object sender, MouseEventArgs e) => this.textLimitPrice.Text = this.labelBid.Text;

        private void textWatchHours_TextChanged(object sender, EventArgs e)
        {
            int result;
            if (!int.TryParse(this.textWatchHours.Text, out result))
                result = 24;
            if (result < 0)
                result = 24;
            if (result > 200)
            {
                result = 200;
                this.textWatchHours.Text = "200";
            }
            this.atb_manager_.WatchHours = result;
        }

        private void textLineNotify_TextChanged(object sender, EventArgs e) => this.atb_manager_.LineToken = this.textLineNotify.Text;

        private void checkToken_CheckedChanged(object sender, EventArgs e)
        {
            this.textLineNotify.Enabled = this.checkToken.Checked;
            this.atb_manager_.IsToken = this.checkToken.Checked;
        }

        private void textAutoLot_TextChanged(object sender, EventArgs e)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = (IContainer)new Container();
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(DoTenTraderMain));
            this.comboBroker = new ComboBox();
            this.comboTimeFrame = new ComboBox();
            this.btnSettings = new Button();
            this.textAPIKey = new TextBox();
            this.labelAutoLot = new Label();
            this.labelWatchHour = new Label();
            this.labelAPIKey = new Label();
            this.labelBroker = new Label();
            this.textWatchHours = new TextBox();
            this.textAutoLot = new TextBox();
            this.buttonAutoTrade = new Button();
            this.buttonBuy = new Button();
            this.buttonSell = new Button();
            this.labelTradeLot = new Label();
            this.textLot = new TextBox();
            this.labelAsk = new Label();
            this.labelBid = new Label();
            this.labelQuote = new Label();
            this.groupAPIInformation = new GroupBox();
            this.groupAutoTrade = new GroupBox();
            this.checkToken = new CheckBox();
            this.buttonCloseAll = new Button();
            this.textLineNotify = new TextBox();
            this.labelAutoCurrency = new Label();
            this.buttonStop = new Button();
            this.textLimitPrice = new TextBox();
            this.labelLimitPrice = new Label();
            this.groupManualTrade = new GroupBox();
            this.labelManualCurrency = new Label();
            this.listQuote = new ListView();
            this.colOpen = new ColumnHeader();
            this.colHigh = new ColumnHeader();
            this.colLow = new ColumnHeader();
            this.colClose = new ColumnHeader();
            this.colHighest = new ColumnHeader();
            this.colLowest = new ColumnHeader();
            this.listPositions = new ListView();
            this.colOpenDateTime = new ColumnHeader();
            this.colSize = new ColumnHeader();
            this.colType = new ColumnHeader();
            this.colOpenPrice = new ColumnHeader();
            this.colCurrentPrice = new ColumnHeader();
            this.colPNL = new ColumnHeader();
            this.labelBalance = new Label();
            this.labelEquity = new Label();
            this.labelBalanceValue = new Label();
            this.labelEquityValue = new Label();
            this.listLogs = new ListView();
            this.colLogDateTime = new ColumnHeader();
            this.colDescription = new ColumnHeader();
            this.comboSymbol = new ComboBox();
            this.labelSymbol = new Label();
            this.labelTimeFrame = new Label();
            this.btnLanguage = new Button();
            this.tabWindow = new TabControl();
            this.tabPositionPage = new TabPage();
            this.tabOrderPage = new TabPage();
            this.listOrders = new ListView();
            this.colOrderDateTime = new ColumnHeader();
            this.colOrderSize = new ColumnHeader();
            this.colOrderType = new ColumnHeader();
            this.colOrderOutStandingSize = new ColumnHeader();
            this.colOrderPrice = new ColumnHeader();
            this.colOrderCurrentPrice = new ColumnHeader();
            this.tabLogs = new TabPage();
            this.contextMenuOrders = new ContextMenuStrip(this.components);
            this.menuCancelAll = new ToolStripMenuItem();
            this.contextMenuLogs = new ContextMenuStrip(this.components);
            this.menuClearLogs = new ToolStripMenuItem();
            this.labelUserID = new Label();
            this.groupAPIInformation.SuspendLayout();
            this.groupAutoTrade.SuspendLayout();
            this.groupManualTrade.SuspendLayout();
            this.tabWindow.SuspendLayout();
            this.tabPositionPage.SuspendLayout();
            this.tabOrderPage.SuspendLayout();
            this.tabLogs.SuspendLayout();
            this.contextMenuOrders.SuspendLayout();
            this.contextMenuLogs.SuspendLayout();
            this.SuspendLayout();
            this.comboBroker.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBroker.FormattingEnabled = true;
            this.comboBroker.Location = new Point(94, 16);
            this.comboBroker.Name = "comboBroker";
            this.comboBroker.Size = new Size(109, 21);
            this.comboBroker.TabIndex = 1;
            this.comboBroker.SelectionChangeCommitted += new EventHandler(this.comboBroker_SelectionChangeCommitted);
            this.comboTimeFrame.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboTimeFrame.FormattingEnabled = true;
            this.comboTimeFrame.Location = new Point(526, 71);
            this.comboTimeFrame.Name = "comboTimeFrame";
            this.comboTimeFrame.Size = new Size(89, 21);
            this.comboTimeFrame.TabIndex = 1;
            this.comboTimeFrame.Visible = false;
            this.comboTimeFrame.SelectionChangeCommitted += new EventHandler(this.comboTimeFrame_SelectionChangeCommitted);
            //this.btnSettings.Image = (Image)componentResourceManager.GetObject("btnSettings.Image");
            this.btnSettings.Location = new Point(329, 12);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new Size(36, 35);
            this.btnSettings.TabIndex = 2;
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new EventHandler(this.btnSettings_Click);
            this.textAPIKey.Location = new Point(94, 51);
            this.textAPIKey.Name = "textAPIKey";
            this.textAPIKey.ReadOnly = true;
            this.textAPIKey.Size = new Size(271, 20);
            this.textAPIKey.TabIndex = 3;
            this.labelAutoLot.Location = new Point(41, 47);
            this.labelAutoLot.Name = "labelAutoLot";
            this.labelAutoLot.Size = new Size(54, 15);
            this.labelAutoLot.TabIndex = 0;
            this.labelAutoLot.Text = "Size";
            this.labelAutoLot.TextAlign = ContentAlignment.MiddleRight;
            this.labelWatchHour.Location = new Point(11, 20);
            this.labelWatchHour.Name = "labelWatchHour";
            this.labelWatchHour.Size = new Size(84, 15);
            this.labelWatchHour.TabIndex = 0;
            this.labelWatchHour.Text = "Watch Hours";
            this.labelWatchHour.TextAlign = ContentAlignment.TopRight;
            this.labelWatchHour.Visible = false;
            this.labelAPIKey.Location = new Point(4, 56);
            this.labelAPIKey.Name = "labelAPIKey";
            this.labelAPIKey.Size = new Size(84, 15);
            this.labelAPIKey.TabIndex = 0;
            this.labelAPIKey.Text = "API Key";
            this.labelAPIKey.TextAlign = ContentAlignment.TopRight;
            this.labelBroker.Location = new Point(4, 22);
            this.labelBroker.Name = "labelBroker";
            this.labelBroker.Size = new Size(84, 15);
            this.labelBroker.TabIndex = 0;
            this.labelBroker.Text = "Broker";
            this.labelBroker.TextAlign = ContentAlignment.TopRight;
            this.textWatchHours.Location = new Point(112, 18);
            this.textWatchHours.Name = "textWatchHours";
            this.textWatchHours.Size = new Size(75, 20);
            this.textWatchHours.TabIndex = 3;
            this.textWatchHours.Text = "38";
            this.textWatchHours.TextAlign = HorizontalAlignment.Right;
            this.textWatchHours.Visible = false;
            this.textWatchHours.TextChanged += new EventHandler(this.textWatchHours_TextChanged);
            this.textAutoLot.Location = new Point(113, 45);
            this.textAutoLot.Name = "textAutoLot";
            this.textAutoLot.Size = new Size(74, 20);
            this.textAutoLot.TabIndex = 3;
            this.textAutoLot.Text = "0.01";
            this.textAutoLot.TextAlign = HorizontalAlignment.Right;
            this.textAutoLot.TextChanged += new EventHandler(this.textAutoLot_TextChanged);
            this.buttonAutoTrade.BackColor = Color.Orange;
            this.buttonAutoTrade.ForeColor = Color.White;
            this.buttonAutoTrade.Location = new Point(235, 107);
            this.buttonAutoTrade.Name = "buttonAutoTrade";
            this.buttonAutoTrade.Size = new Size(72, 33);
            this.buttonAutoTrade.TabIndex = 2;
            this.buttonAutoTrade.Text = "Start";
            this.buttonAutoTrade.UseVisualStyleBackColor = false;
            this.buttonAutoTrade.Click += new EventHandler(this.btnAutoTrade_Click);
            this.buttonBuy.BackColor = Color.LimeGreen;
            this.buttonBuy.ForeColor = Color.White;
            this.buttonBuy.Location = new Point(22, 96);
            this.buttonBuy.Name = "buttonBuy";
            this.buttonBuy.Size = new Size(81, 32);
            this.buttonBuy.TabIndex = 2;
            this.buttonBuy.Text = "Buy";
            this.buttonBuy.UseVisualStyleBackColor = false;
            this.buttonBuy.Click += new EventHandler(this.btnBuy_Click);
            this.buttonSell.BackColor = Color.Tomato;
            this.buttonSell.ForeColor = Color.White;
            this.buttonSell.Location = new Point(207, 96);
            this.buttonSell.Name = "buttonSell";
            this.buttonSell.Size = new Size(82, 32);
            this.buttonSell.TabIndex = 2;
            this.buttonSell.Text = "Sell";
            this.buttonSell.UseVisualStyleBackColor = false;
            this.buttonSell.Click += new EventHandler(this.btnSell_Click);
            this.labelTradeLot.Location = new Point(31, 31);
            this.labelTradeLot.Name = "labelTradeLot";
            this.labelTradeLot.Size = new Size(84, 15);
            this.labelTradeLot.TabIndex = 0;
            this.labelTradeLot.Text = "Size";
            this.labelTradeLot.TextAlign = ContentAlignment.TopRight;
            this.textLot.Location = new Point(166, 31);
            this.textLot.Name = "textLot";
            this.textLot.Size = new Size(62, 20);
            this.textLot.TabIndex = 3;
            this.textLot.Text = "0.0001";
            this.textLot.TextAlign = HorizontalAlignment.Right;
            this.labelAsk.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            this.labelAsk.ForeColor = Color.LimeGreen;
            this.labelAsk.Location = new Point(19, 64);
            this.labelAsk.Name = "labelAsk";
            this.labelAsk.Size = new Size(84, 15);
            this.labelAsk.TabIndex = 0;
            this.labelAsk.Text = "35123542";
            this.labelAsk.TextAlign = ContentAlignment.TopRight;
            this.labelAsk.MouseClick += new MouseEventHandler(this.labelAsk_MouseClick);
            this.labelBid.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            this.labelBid.ForeColor = Color.Tomato;
            this.labelBid.Location = new Point(204, 64);
            this.labelBid.Name = "labelBid";
            this.labelBid.Size = new Size(84, 15);
            this.labelBid.TabIndex = 0;
            this.labelBid.Text = "35123542";
            this.labelBid.TextAlign = ContentAlignment.TopRight;
            this.labelBid.MouseClick += new MouseEventHandler(this.labelBid_MouseClick);
            this.labelQuote.Location = new Point(5, 273);
            this.labelQuote.Name = "labelQuote";
            this.labelQuote.Size = new Size(84, 15);
            this.labelQuote.TabIndex = 0;
            this.labelQuote.Text = "Quote Data";
            this.groupAPIInformation.Controls.Add((Control)this.btnSettings);
            this.groupAPIInformation.Controls.Add((Control)this.labelAPIKey);
            this.groupAPIInformation.Controls.Add((Control)this.labelBroker);
            this.groupAPIInformation.Controls.Add((Control)this.comboBroker);
            this.groupAPIInformation.Controls.Add((Control)this.textAPIKey);
            this.groupAPIInformation.Location = new Point(8, 10);
            this.groupAPIInformation.Name = "groupAPIInformation";
            this.groupAPIInformation.Size = new Size(371, 86);
            this.groupAPIInformation.TabIndex = 4;
            this.groupAPIInformation.TabStop = false;
            this.groupAPIInformation.Text = "API Information";
            this.groupAutoTrade.Controls.Add((Control)this.checkToken);
            this.groupAutoTrade.Controls.Add((Control)this.buttonCloseAll);
            this.groupAutoTrade.Controls.Add((Control)this.textLineNotify);
            this.groupAutoTrade.Controls.Add((Control)this.labelAutoCurrency);
            this.groupAutoTrade.Controls.Add((Control)this.buttonStop);
            this.groupAutoTrade.Controls.Add((Control)this.buttonAutoTrade);
            this.groupAutoTrade.Controls.Add((Control)this.labelAutoLot);
            this.groupAutoTrade.Controls.Add((Control)this.textAutoLot);
            this.groupAutoTrade.Controls.Add((Control)this.labelWatchHour);
            this.groupAutoTrade.Controls.Add((Control)this.textWatchHours);
            this.groupAutoTrade.Location = new Point(8, 102);
            this.groupAutoTrade.Name = "groupAutoTrade";
            this.groupAutoTrade.Size = new Size(325, 160);
            this.groupAutoTrade.TabIndex = 5;
            this.groupAutoTrade.TabStop = false;
            this.groupAutoTrade.Text = "Auto Trade";
            this.checkToken.AutoSize = true;
            this.checkToken.Checked = true;
            this.checkToken.CheckState = CheckState.Checked;
            this.checkToken.Location = new Point(17, 82);
            this.checkToken.Name = "checkToken";
            this.checkToken.Size = new Size(80, 17);
            this.checkToken.TabIndex = 13;
            this.checkToken.Text = "Line Token";
            this.checkToken.UseVisualStyleBackColor = true;
            this.checkToken.CheckedChanged += new EventHandler(this.checkToken_CheckedChanged);
            this.buttonCloseAll.BackColor = Color.Orange;
            this.buttonCloseAll.ForeColor = Color.White;
            this.buttonCloseAll.Location = new Point(139, 107);
            this.buttonCloseAll.Name = "buttonCloseAll";
            this.buttonCloseAll.Size = new Size(73, 33);
            this.buttonCloseAll.TabIndex = 11;
            this.buttonCloseAll.Text = "Close All";
            this.buttonCloseAll.UseVisualStyleBackColor = false;
            this.buttonCloseAll.Click += new EventHandler(this.buttonCloseAll_Click);
            this.textLineNotify.Location = new Point(112, 79);
            this.textLineNotify.Name = "textLineNotify";
            this.textLineNotify.Size = new Size(196, 20);
            this.textLineNotify.TabIndex = 11;
            this.textLineNotify.TextChanged += new EventHandler(this.textLineNotify_TextChanged);
            this.labelAutoCurrency.Location = new Point(189, 48);
            this.labelAutoCurrency.Name = "labelAutoCurrency";
            this.labelAutoCurrency.Size = new Size(30, 18);
            this.labelAutoCurrency.TabIndex = 9;
            this.labelAutoCurrency.Text = "BTC";
            this.buttonStop.BackColor = Color.Orange;
            this.buttonStop.ForeColor = Color.White;
            this.buttonStop.Location = new Point(236, 107);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new Size(72, 33);
            this.buttonStop.TabIndex = 2;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = false;
            this.buttonStop.Visible = false;
            this.buttonStop.Click += new EventHandler(this.btnStop_Click);
            this.textLimitPrice.Location = new Point(143, 59);
            this.textLimitPrice.Name = "textLimitPrice";
            this.textLimitPrice.Size = new Size(85, 20);
            this.textLimitPrice.TabIndex = 12;
            this.textLimitPrice.Text = "5000";
            this.textLimitPrice.TextAlign = HorizontalAlignment.Right;
            this.textLimitPrice.Visible = false;
            this.labelLimitPrice.Location = new Point(31, 59);
            this.labelLimitPrice.Name = "labelLimitPrice";
            this.labelLimitPrice.Size = new Size(84, 15);
            this.labelLimitPrice.TabIndex = 11;
            this.labelLimitPrice.Text = "Limit Price";
            this.labelLimitPrice.TextAlign = ContentAlignment.TopRight;
            this.labelLimitPrice.Visible = false;
            this.groupManualTrade.Controls.Add((Control)this.textLimitPrice);
            this.groupManualTrade.Controls.Add((Control)this.labelLimitPrice);
            this.groupManualTrade.Controls.Add((Control)this.labelManualCurrency);
            this.groupManualTrade.Controls.Add((Control)this.buttonSell);
            this.groupManualTrade.Controls.Add((Control)this.labelTradeLot);
            this.groupManualTrade.Controls.Add((Control)this.labelAsk);
            this.groupManualTrade.Controls.Add((Control)this.textLot);
            this.groupManualTrade.Controls.Add((Control)this.labelBid);
            this.groupManualTrade.Controls.Add((Control)this.buttonBuy);
            this.groupManualTrade.Location = new Point(364, 114);
            this.groupManualTrade.Name = "groupManualTrade";
            this.groupManualTrade.Size = new Size(315, 148);
            this.groupManualTrade.TabIndex = 6;
            this.groupManualTrade.TabStop = false;
            this.groupManualTrade.Text = "Manual Trade";
            this.labelManualCurrency.Location = new Point(231, 34);
            this.labelManualCurrency.Name = "labelManualCurrency";
            this.labelManualCurrency.Size = new Size(31, 18);
            this.labelManualCurrency.TabIndex = 9;
            this.labelManualCurrency.Text = "BTC";
            this.listQuote.Columns.AddRange(new ColumnHeader[6]
            {
        this.colOpen,
        this.colHigh,
        this.colLow,
        this.colClose,
        this.colHighest,
        this.colLowest
            });
            this.listQuote.Location = new Point(8, 301);
            this.listQuote.Name = "listQuote";
            this.listQuote.Size = new Size(674, 55);
            this.listQuote.TabIndex = 7;
            this.listQuote.UseCompatibleStateImageBehavior = false;
            this.listQuote.View = View.Details;
            this.colOpen.Text = "Open";
            this.colOpen.Width = 110;
            this.colHigh.Text = "High";
            this.colHigh.Width = 110;
            this.colLow.Text = "Low";
            this.colLow.Width = 110;
            this.colClose.Text = "Close";
            this.colClose.Width = 110;
            this.colHighest.Text = "Highest";
            this.colHighest.Width = 110;
            this.colLowest.Text = "Lowest";
            this.colLowest.Width = 110;
            this.listPositions.Columns.AddRange(new ColumnHeader[6]
            {
        this.colOpenDateTime,
        this.colSize,
        this.colType,
        this.colOpenPrice,
        this.colCurrentPrice,
        this.colPNL
            });
            this.listPositions.Location = new Point(5, 8);
            this.listPositions.Name = "listPositions";
            this.listPositions.Size = new Size(655, 150);
            this.listPositions.TabIndex = 7;
            this.listPositions.UseCompatibleStateImageBehavior = false;
            this.listPositions.View = View.Details;
            this.colOpenDateTime.Text = "DateTime";
            this.colOpenDateTime.Width = 120;
            this.colSize.Text = "Size";
            this.colSize.Width = 90;
            this.colType.Text = "Type";
            this.colOpenPrice.Text = "Open Price";
            this.colOpenPrice.Width = 110;
            this.colCurrentPrice.Text = "Current Price";
            this.colCurrentPrice.Width = 110;
            this.colPNL.Text = "PNL";
            this.colPNL.Width = 110;
            this.labelBalance.Location = new Point(395, 15);
            this.labelBalance.Name = "labelBalance";
            this.labelBalance.Size = new Size(63, 15);
            this.labelBalance.TabIndex = 0;
            this.labelBalance.Text = "Balance";
            this.labelBalance.TextAlign = ContentAlignment.TopRight;
            this.labelEquity.Location = new Point(395, 36);
            this.labelEquity.Name = "labelEquity";
            this.labelEquity.Size = new Size(63, 15);
            this.labelEquity.TabIndex = 0;
            this.labelEquity.Text = "Equity";
            this.labelEquity.TextAlign = ContentAlignment.TopRight;
            this.labelBalanceValue.Location = new Point(491, 15);
            this.labelBalanceValue.Name = "labelBalanceValue";
            this.labelBalanceValue.Size = new Size(68, 15);
            this.labelBalanceValue.TabIndex = 0;
            this.labelBalanceValue.Text = "0.00";
            this.labelBalanceValue.TextAlign = ContentAlignment.TopRight;
            this.labelEquityValue.Location = new Point(488, 36);
            this.labelEquityValue.Name = "labelEquityValue";
            this.labelEquityValue.Size = new Size(71, 15);
            this.labelEquityValue.TabIndex = 0;
            this.labelEquityValue.Text = "0.00";
            this.labelEquityValue.TextAlign = ContentAlignment.TopRight;
            this.listLogs.Columns.AddRange(new ColumnHeader[2]
            {
        this.colLogDateTime,
        this.colDescription
            });
            this.listLogs.Location = new Point(5, 6);
            this.listLogs.Name = "listLogs";
            this.listLogs.Size = new Size(655, 152);
            this.listLogs.TabIndex = 7;
            this.listLogs.UseCompatibleStateImageBehavior = false;
            this.listLogs.View = View.Details;
            this.listLogs.MouseDown += new MouseEventHandler(this.listLogs_MouseDown);
            this.colLogDateTime.Text = "DateTime";
            this.colLogDateTime.Width = 120;
            this.colDescription.Text = "Description";
            this.colDescription.Width = 500;
            this.comboSymbol.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboSymbol.FormattingEnabled = true;
            this.comboSymbol.Location = new Point(401, 71);
            this.comboSymbol.Name = "comboSymbol";
            this.comboSymbol.Size = new Size(99, 21);
            this.comboSymbol.TabIndex = 1;
            this.labelSymbol.Location = new Point(398, 53);
            this.labelSymbol.Name = "labelSymbol";
            this.labelSymbol.Size = new Size(84, 15);
            this.labelSymbol.TabIndex = 0;
            this.labelSymbol.Text = "Symbol";
            this.labelTimeFrame.Location = new Point(525, 53);
            this.labelTimeFrame.Name = "labelTimeFrame";
            this.labelTimeFrame.Size = new Size(84, 15);
            this.labelTimeFrame.TabIndex = 0;
            this.labelTimeFrame.Text = "Timeframe";
            this.labelTimeFrame.Visible = false;
            this.btnLanguage.BackColor = Color.DarkTurquoise;
            this.btnLanguage.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            this.btnLanguage.ForeColor = Color.White;
            this.btnLanguage.Location = new Point(636, 12);
            this.btnLanguage.Name = "btnLanguage";
            this.btnLanguage.Size = new Size(39, 38);
            this.btnLanguage.TabIndex = 8;
            this.btnLanguage.Text = "JP";
            this.btnLanguage.UseVisualStyleBackColor = false;
            this.btnLanguage.Click += new EventHandler(this.btnLanguage_Click);
            this.tabWindow.Controls.Add((Control)this.tabPositionPage);
            this.tabWindow.Controls.Add((Control)this.tabOrderPage);
            this.tabWindow.Controls.Add((Control)this.tabLogs);
            this.tabWindow.Location = new Point(8, 362);
            this.tabWindow.Name = "tabWindow";
            this.tabWindow.SelectedIndex = 0;
            this.tabWindow.Size = new Size(674, 194);
            this.tabWindow.TabIndex = 10;
            this.tabPositionPage.Controls.Add((Control)this.listPositions);
            this.tabPositionPage.Location = new Point(4, 22);
            this.tabPositionPage.Name = "tabPositionPage";
            this.tabPositionPage.Padding = new Padding(3);
            this.tabPositionPage.Size = new Size(666, 168);
            this.tabPositionPage.TabIndex = 0;
            this.tabPositionPage.Text = "Positions";
            this.tabPositionPage.UseVisualStyleBackColor = true;
            this.tabOrderPage.Controls.Add((Control)this.listOrders);
            this.tabOrderPage.Location = new Point(4, 22);
            this.tabOrderPage.Name = "tabOrderPage";
            this.tabOrderPage.Padding = new Padding(3);
            this.tabOrderPage.Size = new Size(666, 168);
            this.tabOrderPage.TabIndex = 1;
            this.tabOrderPage.Text = "Orders";
            this.tabOrderPage.UseVisualStyleBackColor = true;
            this.listOrders.Columns.AddRange(new ColumnHeader[6]
            {
        this.colOrderDateTime,
        this.colOrderSize,
        this.colOrderType,
        this.colOrderOutStandingSize,
        this.colOrderPrice,
        this.colOrderCurrentPrice
            });
            this.listOrders.Location = new Point(5, 8);
            this.listOrders.Name = "listOrders";
            this.listOrders.Size = new Size(655, 150);
            this.listOrders.TabIndex = 8;
            this.listOrders.UseCompatibleStateImageBehavior = false;
            this.listOrders.View = View.Details;
            this.listOrders.MouseDown += new MouseEventHandler(this.listOrders_MouseDown);
            this.colOrderDateTime.Text = "DateTime";
            this.colOrderDateTime.Width = 120;
            this.colOrderSize.Text = "Size";
            this.colOrderSize.Width = 100;
            this.colOrderType.Text = "Type";
            this.colOrderType.Width = 80;
            this.colOrderOutStandingSize.Text = "OutStanding";
            this.colOrderOutStandingSize.Width = 100;
            this.colOrderPrice.Text = "Request Price";
            this.colOrderPrice.Width = 110;
            this.colOrderCurrentPrice.Text = "Current Price";
            this.colOrderCurrentPrice.Width = 110;
            this.tabLogs.Controls.Add((Control)this.listLogs);
            this.tabLogs.Location = new Point(4, 22);
            this.tabLogs.Name = "tabLogs";
            this.tabLogs.Padding = new Padding(3);
            this.tabLogs.Size = new Size(666, 168);
            this.tabLogs.TabIndex = 2;
            this.tabLogs.Text = "Logs";
            this.tabLogs.UseVisualStyleBackColor = true;
            this.contextMenuOrders.Items.AddRange(new ToolStripItem[1]
            {
        (ToolStripItem) this.menuCancelAll
            });
            this.contextMenuOrders.Name = "contextMenuOrders";
            this.contextMenuOrders.Size = new Size(128, 26);
            this.menuCancelAll.Name = "menuCancelAll";
            this.menuCancelAll.Size = new Size((int)sbyte.MaxValue, 22);
            this.menuCancelAll.Text = "Cancel All";
            this.menuCancelAll.Click += new EventHandler(this.menuCancelAll_Click);
            this.contextMenuLogs.Items.AddRange(new ToolStripItem[1]
            {
        (ToolStripItem) this.menuClearLogs
            });
            this.contextMenuLogs.Name = "contextMenuOrders";
            this.contextMenuLogs.Size = new Size(119, 26);
            this.menuClearLogs.Name = "menuClearLogs";
            this.menuClearLogs.Size = new Size(118, 22);
            this.menuClearLogs.Text = "Clear All";
            this.menuClearLogs.Click += new EventHandler(this.menuClearLogs_Click);
            this.labelUserID.Location = new Point(504, 273);
            this.labelUserID.Name = "labelUserID";
            this.labelUserID.Size = new Size(114, 15);
            this.labelUserID.TabIndex = 11;
            this.labelUserID.Text = "X";
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(687, 549);
            this.Controls.Add((Control)this.labelUserID);
            this.Controls.Add((Control)this.tabWindow);
            this.Controls.Add((Control)this.btnLanguage);
            this.Controls.Add((Control)this.listQuote);
            this.Controls.Add((Control)this.groupManualTrade);
            this.Controls.Add((Control)this.groupAutoTrade);
            this.Controls.Add((Control)this.labelEquityValue);
            this.Controls.Add((Control)this.labelEquity);
            this.Controls.Add((Control)this.labelBalanceValue);
            this.Controls.Add((Control)this.labelBalance);
            this.Controls.Add((Control)this.groupAPIInformation);
            this.Controls.Add((Control)this.comboSymbol);
            this.Controls.Add((Control)this.comboTimeFrame);
            this.Controls.Add((Control)this.labelTimeFrame);
            this.Controls.Add((Control)this.labelSymbol);
            this.Controls.Add((Control)this.labelQuote);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            //this.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = nameof(DoTenTraderMain);
            this.Text = "BTC Machine";
            this.FormClosing += new FormClosingEventHandler(this.DoTenTraderMain_FormClosing);
            this.groupAPIInformation.ResumeLayout(false);
            this.groupAPIInformation.PerformLayout();
            this.groupAutoTrade.ResumeLayout(false);
            this.groupAutoTrade.PerformLayout();
            this.groupManualTrade.ResumeLayout(false);
            this.groupManualTrade.PerformLayout();
            this.tabWindow.ResumeLayout(false);
            this.tabPositionPage.ResumeLayout(false);
            this.tabOrderPage.ResumeLayout(false);
            this.tabLogs.ResumeLayout(false);
            this.contextMenuOrders.ResumeLayout(false);
            this.contextMenuLogs.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private delegate void SetQuoteListCallBack(
          double open,
          double high,
          double low,
          double close,
          double highest,
          double lowest);

        private delegate void SetPositionListCallBack(List<Position> positions);

        private delegate void SetOrderListCallBack(List<Order> orders);

        private delegate void AppendLogListCallBack(string time, string message);

        private delegate void SetLabelTextCallBack(Label label, string text);

        private delegate void SetConrolEnabledCallBack(Control control, bool is_enabled);

        private delegate void SetConrolVisibleCallBack(Control control, bool is_visible);
    }
}
