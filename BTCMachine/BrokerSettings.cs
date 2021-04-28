using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BTCMachine
{
    public class BrokerSettings : Form
    {
        private DoTenTraderManager atb_manager_;
        private Dictionary<string, BrokerInfo> broker_infos_;
        private IContainer components;
        private Button btnSave;
        private Label labelAPIKey;
        private Label labelBroker;
        private ComboBox comboBroker;
        private TextBox textAPIKey;
        private Label labelAPISec;
        private TextBox textAPISec;
        private Button btnApply;
        private Button btnCancel;

        public BrokerSettings(DoTenTraderManager atb_manager)
        {
            this.atb_manager_ = atb_manager;
            this.InitializeComponent();
            this.ChangeLanguage();
            this.broker_infos_ = new Dictionary<string, BrokerInfo>();
            this.broker_infos_ = atb_manager.BrokerList;
            this.InitComboBroker();
            this.SetAPIValues();
        }

        private void InitComboBroker()
        {
            this.comboBroker.Items.Clear();
            foreach (KeyValuePair<string, BrokerInfo> brokerInfo in this.broker_infos_)
                this.comboBroker.Items.Add((object)brokerInfo.Key);
            this.comboBroker.SelectedIndex = 0;
        }

        private void ChangeLanguage()
        {
            if (this.atb_manager_.IsEnglish)
            {
                this.labelBroker.Text = "Broker";
                this.labelAPIKey.Text = "APIKey";
                this.labelAPISec.Text = "APISec";
                this.btnSave.Text = "Save";
                this.btnApply.Text = "Apply";
                this.btnCancel.Text = "Cancel";
            }
            else
            {
                this.labelBroker.Text = "取引所";
                this.labelAPIKey.Text = "APIKey";
                this.labelAPISec.Text = "APISec";
                this.btnSave.Text = "保存";
                this.btnApply.Text = "適用";
                this.btnCancel.Text = "キャンセル";
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string text = this.comboBroker.Text;
            if (!this.broker_infos_.ContainsKey(text))
                return;
            string symbols = this.broker_infos_[text].Symbols;
            this.broker_infos_.Remove(text);
            this.broker_infos_.Add(text, new BrokerInfo(text, symbols, this.textAPIKey.Text, this.textAPISec.Text));
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            this.atb_manager_.BrokerList = this.broker_infos_;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) => this.Close();

        private void comboBroker_SelectionChangeCommitted(object sender, EventArgs e) => this.SetAPIValues();

        private void SetAPIValues()
        {
            string key = this.comboBroker.SelectedItem.ToString();
            if (!this.broker_infos_.ContainsKey(key))
                return;
            this.textAPIKey.Text = this.broker_infos_[key].APIKey;
            this.textAPISec.Text = this.broker_infos_[key].APISec;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(BrokerSettings));
            this.btnSave = new Button();
            this.labelAPIKey = new Label();
            this.labelBroker = new Label();
            this.comboBroker = new ComboBox();
            this.textAPIKey = new TextBox();
            this.labelAPISec = new Label();
            this.textAPISec = new TextBox();
            this.btnApply = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();
            //this.btnSave.Image = (Image)componentResourceManager.GetObject("btnSave.Image");
            this.btnSave.ImageAlign = ContentAlignment.MiddleLeft;
            this.btnSave.Location = new Point(294, 8);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new Size(80, 32);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);
            this.labelAPIKey.Location = new Point(14, 55);
            this.labelAPIKey.Name = "labelAPIKey";
            this.labelAPIKey.Size = new Size(84, 15);
            this.labelAPIKey.TabIndex = 4;
            this.labelAPIKey.Text = "API Key";
            this.labelAPIKey.TextAlign = ContentAlignment.TopRight;
            this.labelBroker.Location = new Point(14, 18);
            this.labelBroker.Name = "labelBroker";
            this.labelBroker.Size = new Size(84, 15);
            this.labelBroker.TabIndex = 5;
            this.labelBroker.Text = "Broker";
            this.labelBroker.TextAlign = ContentAlignment.TopRight;
            this.comboBroker.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBroker.FormattingEnabled = true;
            this.comboBroker.Location = new Point(104, 12);
            this.comboBroker.Name = "comboBroker";
            this.comboBroker.Size = new Size(109, 21);
            this.comboBroker.TabIndex = 6;
            this.comboBroker.SelectionChangeCommitted += new EventHandler(this.comboBroker_SelectionChangeCommitted);
            this.textAPIKey.Location = new Point(104, 50);
            this.textAPIKey.Name = "textAPIKey";
            this.textAPIKey.Size = new Size(272, 20);
            this.textAPIKey.TabIndex = 7;
            this.labelAPISec.Location = new Point(14, 92);
            this.labelAPISec.Name = "labelAPISec";
            this.labelAPISec.Size = new Size(84, 15);
            this.labelAPISec.TabIndex = 8;
            this.labelAPISec.Text = "APISec";
            this.labelAPISec.TextAlign = ContentAlignment.TopRight;
            this.textAPISec.Location = new Point(104, 87);
            this.textAPISec.Name = "textAPISec";
            this.textAPISec.Size = new Size(272, 20);
            this.textAPISec.TabIndex = 9;
            //this.btnApply.Image = (Image)componentResourceManager.GetObject("btnApply.Image");
            this.btnApply.ImageAlign = ContentAlignment.MiddleLeft;
            this.btnApply.Location = new Point(152, 115);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new Size(109, 40);
            this.btnApply.TabIndex = 0;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new EventHandler(this.btnApply_Click);
            //this.btnCancel.Image = (Image)componentResourceManager.GetObject("btnCancel.Image");
            this.btnCancel.ImageAlign = ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new Point(267, 115);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(109, 40);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(389, 167);
            this.Controls.Add((Control)this.labelAPISec);
            this.Controls.Add((Control)this.textAPISec);
            this.Controls.Add((Control)this.labelAPIKey);
            this.Controls.Add((Control)this.labelBroker);
            this.Controls.Add((Control)this.comboBroker);
            this.Controls.Add((Control)this.textAPIKey);
            this.Controls.Add((Control)this.btnCancel);
            this.Controls.Add((Control)this.btnApply);
            this.Controls.Add((Control)this.btnSave);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            //this.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = nameof(BrokerSettings);
            this.Text = "Settings";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
