using Eternity.Engine.Helpers;
using Eternity.Utils.API;
using FlatUI;
using MaterialSkin.Controls;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Eternity.Forms.Dialogs
{
    public partial class TokenForm : MaterialForm
    {
        public MainForm _form;
        public FlatComboBox _accounts;
        public TokenForm()
        {
            InitializeComponent();
        }

        private void TokenForm_Shown(object sender, System.EventArgs e)
        {
            try
            {
                var account = _form.Accounts[_accounts.SelectedIndex];
                textBox_Token.Text = account.Token;
            }
            catch
            {

            }
        }

        private void link_GetToken_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://vkhost.github.io");
        }

        private void button_Apply_Click(object sender, EventArgs e)
        {
            try
            {
                var account = _form.Accounts[_accounts.SelectedIndex];
                if (textBox_Token.Text.Contains("access_token="))
                    textBox_Token.Text = StrWrk.GetBetween(textBox_Token.Text, "access_token=", "&");
                var response
                    = Server.APIRequest("users.get", "", textBox_Token.Text);
                dynamic json = JsonConvert.DeserializeObject(response);
                if (response.Contains("response"))
                {
                    account.Token = textBox_Token.Text;
                    account.Info = $"{json["response"][0]["first_name"]} {json["response"][0]["last_name"]}";
                    account.Applications = "Kate Mobile";
                    account.Save();
                    _accounts.Items[_accounts.SelectedIndex] = $"{account.Login} ({account.Info})";
                    flatLabel18.Text = "Токен изменен";
                    flatLabel18.ForeColor = Color.Green;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("invalid"))
                {
                    flatLabel18.Text = "Токен неверный";
                    flatLabel18.ForeColor = Color.Red;
                }
            }
        }
    }
}
