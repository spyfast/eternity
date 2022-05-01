using Eternity.Captcha;
using Eternity.Configs.Logger;
using Eternity.Engine.Accounts;
using Eternity.Engine.Helpers;
using Eternity.Forms.Dialogs;
using Eternity.Utils;
using Eternity.Utils.API;
using Eternity.Utils.Edit;
using Eternity.Utils.Edit.Settings;
using Eternity.Utils.Filters;
using MaterialSkin;
using MaterialSkin.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace Eternity.Forms
{
    public partial class MainForm : MaterialForm
    {
        public List<Account> Accounts;

        private bool IsWorkBot { get; set; } = true;
        private int Hours, Minutes, Second;
        private System.Timers.Timer Timer { get; set; }
        public MainForm()
        {
            InitializeComponent();
            var instance = MaterialSkinManager.Instance;
            instance.ColorScheme =
                new ColorScheme(Primary.BlueGrey900, Primary.BlueGrey900, Primary.Red900, Accent.Red400, TextShade.WHITE);

            if (!File.Exists("FlatUI.dll") || !File.Exists("MaterialSkin.dll") || !File.Exists("Newtonsoft.Json.dll"))
            {
                MessageBox.Show("Отсутствуют .dll для работы приложения", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            Accounts = new List<Account>();
            Directory.CreateDirectory("AccsData");
            Directory.CreateDirectory("Files");
            Directory.CreateDirectory("Tmp");
            Directory.CreateDirectory("Files\\Phrases");
            CreateFileIfExists("Files\\accounts.txt");
            CreateFileIfExists("Files\\stickers.txt");
            CreateFileIfExists("Files\\content.txt");
            CreateFileIfExists("Files\\titles.txt");
            CreateFileIfExists("Files\\antiCaptcha.txt");

            foreach (var file in Directory.GetFiles("Tmp"))
                File.Delete(file);
            ReloadFromFile();

            versionApp.Text += Application.ProductVersion;
            comboBox_locationName.SelectedIndex = 0;
            textBox_antiCaptcha.Text = File.ReadAllText("Files\\antiCaptcha.txt");
            comboBox_Value.SelectedIndex = 0;

            Timer = new System.Timers.Timer();
            Timer.Interval = 1000;
            Timer.Elapsed += OnTimeEvent;

            if (!string.IsNullOrEmpty(textBox_antiCaptcha.Text))
                CaptchaSolver.SetKey(textBox_antiCaptcha.Text.Trim());
        }
        private void CreateFileIfExists(string fileName)
        {
            if (!File.Exists(fileName))
                File.Create(fileName).Close();
        }

        private void ReloadFromFile()
        {
            comboBox_PhrasesSource.Items.Clear();

            foreach (var item in Directory.GetFiles("Files\\Phrases"))
                comboBox_PhrasesSource.Items.Add(Path.GetFileName(item));
        }

        private void TimerUpdate_Tick(object sender, EventArgs e)
        {
            while (Log.Logs.Count > 0)
            {
                richTextBox1.Text = $"{Log.Logs.Dequeue()}\r\n{richTextBox1.Text}";
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            try
            {
                var accs = File.ReadAllLines("Files\\accounts.txt", Encoding.UTF8).ToList();
                for (int i = 0; i < accs.Count; i++)
                {
                    var data = accs[i].Split(':');

                    Account account = null;

                    if (!File.Exists($"AccsData\\{data[0]}.json"))
                        account = new Account { Login = data[0], Password = data[1] };
                    else
                        account = JsonConvert.DeserializeObject<Account>
                            (File.ReadAllText($"AccsData\\{data[0]}.json"));
                    Accounts.Add(account);
                    comboBox_accountsList.Items.Add($"{account.Login} ({account.Info})");
                }

                if (comboBox_accountsList.Items.Count > 0)
                    comboBox_accountsList_SelectedIndexChanged(null, null);
                else
                    comboBox_accountsList.Items.Add("Не было загружено ни одного аккаунта");
                comboBox_accountsList.SelectedIndex = 0;
            }
            catch
            {

            }
        }
        private bool OpenForm { get; set; } = false;
        private void comboBox_accountsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_accountsList.SelectedIndex != -1)
            {
                OpenForm = false;

                var account = Accounts[comboBox_accountsList.SelectedIndex];
                var flooder = account.FlooderTargets;
                var conversations = account.ConversationsTargets;

                dataGridView_flooder.Rows.Clear();
                dataGridView_conversations.Rows.Clear();

                foreach (var item in flooder)
                    dataGridView_flooder.Rows.Add(item.Name, item.Link, item.Contains);
                foreach (var item in conversations)
                    dataGridView_conversations.Rows.Add(item.ChatId, item.Title, item.Method);

                checkBox_EnabledFlooder.Checked = account.EnabledFlooder;
                numeric_DelayMaxFlooder.Text = account.DelayMax;
                numeric_DelayMinFlooder.Text = account.DelayMin;
                comboBox_PhrasesSource.Text = account.PhrasesFile;
                if (string.IsNullOrEmpty(account.LocationName))
                    comboBox_locationName.SelectedIndex = 0;
                else
                    comboBox_locationName.Text = account.LocationName;
                comboBox_SelectDelay.Text = account.SelectDelay;

                checkBox_conversationsEnabled.Checked = account.EnabledConversations;
                numeric_DelayConversationsMin.Text = account.DelayConversations;

                countTarget.Text = account.CountTarget;
                timeMinutesFlooder.Text = account.TimeMinutes;
                comboBox_appId.Text = account.Applications;

                OpenForm = true;
            }
        }

        private void comboBox_SelectDelay_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_SelectDelay.SelectedIndex == 0)
                numeric_DelayMaxFlooder.Enabled = false;
            if (comboBox_SelectDelay.SelectedIndex == 1)
                numeric_DelayMaxFlooder.Enabled = true;
        }

        private void comboBox_appId_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (OpenForm)
            {
                if (comboBox_appId.SelectedIndex == 2)
                {
                    var tokenForm = new TokenForm();
                    tokenForm._form = this;
                    tokenForm._accounts = comboBox_accountsList;
                    tokenForm.Show(this);
                }
            }
        }

        private void checkBox_LogOff_CheckedChanged(object sender)
        {
            if (checkBox_LogOff.Checked)
            {
                Log.Enabled = false;
                Log.Logs.Clear();
            }
            else
            {
                Log.Enabled = true;
                Log.Logs.Clear();
            }
        }

        private void flatCheckBox1_CheckedChanged(object sender)
        {
            if (!checkBox_isWorkBot.Checked)
            {
                IsWorkBot = true;
                Timer.Start();
            }
            else
            {
                IsWorkBot = false;
                Timer.Stop();
            }
        }

        private void checkBox_isWorkBot_CheckedChanged(object sender)
        {
            if (!checkBox_isWorkBot.Checked)
            {
                IsWorkBot = true;
                Timer.Start();
            }
            else
            {
                IsWorkBot = false;
                Timer.Stop();
            }
        }

        private void checkBox_SendMessageFalse_CheckedChanged(object sender)
        {
            if (checkBox_SendMessageFalse.Checked)
                Account.IsSendFalse = false;
            else
                Account.IsSendFalse = true;
        }

        private void button_Authorize_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                Invoke(new Action(() =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(comboBox_appId.Text))
                            Log.Push("Не выбрано приложение авторизации или токен");
                        else
                        {
                            if (!checkBox_AuthorizeAllAccount.Checked)
                            {
                                var account = Accounts[comboBox_accountsList.SelectedIndex];
                                account.Applications = comboBox_appId.Text;
                                account.Save();
                                comboBox_appId.Items[comboBox_appId.SelectedIndex] = account.Applications;
                                VkUtils.Apps = account.Applications;
                                var auth = account.Authorize();
                                if (auth == AuthStatus.Ok)
                                {
                                    comboBox_accountsList.Items[comboBox_accountsList.SelectedIndex] = $"{account.Login} ({account.Info})";
                                    Log.Push($"Авторизация завершена: {account.Login}", "", false, Log.TypeLogger.File);
                                }
                                else if (auth == AuthStatus.Invalid)
                                    Log.Push($"Неудачная авторизация: {account.Login}", "", false, Log.TypeLogger.File);
                            }
                            else
                            {
                                comboBox_accountsList.Items.Clear();
                                VkUtils.Apps = comboBox_appId.Text;

                                foreach (var acc in Accounts)
                                {
                                    var token = acc.Authorize();
                                    if (token == AuthStatus.Ok)
                                    {
                                        comboBox_accountsList.Items.Add($"{acc.Login} ({acc.Info})");
                                        Log.Push($"Авторизация завершена: {acc.Login}", "", false, Log.TypeLogger.File);
                                        comboBox_accountsList.SelectedIndex = 0;
                                    }
                                    else if (token == AuthStatus.Invalid)
                                        Log.Push($"Неудачная авторизация: {acc.Login}", "", false, Log.TypeLogger.File);
                                }
                            }

                        }
                    }
                    catch
                    {

                    }
                }));
            })
            { IsBackground = true }.Start();
        }

        private void button_ReloadTxt_Click(object sender, EventArgs e)
        {
            Log.Push("Перезагрузка .txt");
            ReloadFromFile();
        }

        private void button_StartStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (Account._workings)
                {
                    Account._workings = false;
                    button_StartStop.Text = "Старт";
                    Timer.Stop();
                    Hours = Minutes = Second = 0;
                }
                else
                {
                    Account._workings = true;
                    if (IsWorkBot)
                        Timer.Start();
                    button_StartStop.Text = "Стоп";
                    foreach (var account in Accounts)
                        account.AsyncWorker();
                }
            }
            catch (Exception ex)
            {
                Log.Push($"Ошибка запуска: {ex.Message}", "", false, Log.TypeLogger.File);
            }
        }

        private void button_clearLog_Click
            (object sender, EventArgs e)
                => richTextBox1.Text = null;

        private void button_Apply_Click(object sender, EventArgs e)
        {
            try
            {
                var from = StrWrk.IsInteger(textBox_FromTarget.Text, 1);
                var before = StrWrk.IsInteger(textBox_BeforeTarget.Text, 2);
                dataGridView_flooder.CurrentRow.Cells[0].Value += "";
                var text = dataGridView_flooder.CurrentRow.Cells[0].Value.ToString();
                var contains = (dataGridView_flooder.CurrentRow.Cells[2].Value ?? "").ToString();
                dataGridView_flooder.Rows.Clear();
                for (int i = from; i <= before; i++)
                    dataGridView_flooder.Rows.Add(text, $"im?sel=c{i}", contains);
                textBox_BeforeTarget.Text = textBox_FromTarget.Text = "";
            }
            catch
            {

            }
        }

        private void button_SaveFlood_Click(object sender, EventArgs e)
        {
            if (dataGridView_flooder.Rows.Count == 1)
            {
                MessageBox.Show("Заполните цели", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(comboBox_SelectDelay.Text))
            {
                MessageBox.Show("Заполните тип интервала", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(numeric_DelayMinFlooder.Text))
            {
                MessageBox.Show("Заполните минимальный интервал", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(numeric_DelayMaxFlooder.Text) && comboBox_SelectDelay.Text.Contains("Рандом"))
            {
                MessageBox.Show("Заполните максимальный интервал", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(comboBox_PhrasesSource.Text))
            {
                MessageBox.Show("Заполните источник фраз", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var account = Accounts[comboBox_accountsList.SelectedIndex];
            account.DelayMin = numeric_DelayMinFlooder.Text;
            account.DelayMax = numeric_DelayMaxFlooder.Text;
            account.EnabledFlooder = checkBox_EnabledFlooder.Checked;
            account.PhrasesFile = comboBox_PhrasesSource.Text;
            account.LocationName = comboBox_locationName.Text;
            account.SelectDelay = comboBox_SelectDelay.Text;
            account.ParseDataGridFlooder(dataGridView_flooder);
            if (checkBox_EnabledFlooder.Checked)
            {
                account.Phrases = File.ReadAllLines($"Files\\Phrases\\{comboBox_PhrasesSource.Text}", Encoding.UTF8);
                Log.Push($"Фраз доступно: {account.Phrases.Length}", $"{account.Login}", false, Log.TypeLogger.File);
            }
            account.Save();

            countTarget.Text = "Кол-во целей: " + account.FlooderTargets.Count;
            LeaveNumeric(null, null);
        }

        private void button_SaveConversations_Click(object sender, EventArgs e)
        {
            var account = Accounts[comboBox_accountsList.SelectedIndex];
            account.DelayConversations = numeric_DelayConversationsMin.Text;
            account.EnabledConversations = checkBox_conversationsEnabled.Checked;
            account.ParseDataGridConversations(dataGridView_conversations);
            account.Save();
        }

        private void button_SaveCaptcha_Click(object sender, EventArgs e)
        {
            File.WriteAllText("Files\\antiCaptcha.txt", textBox_antiCaptcha.Text);
            if (textBox_antiCaptcha.Text.Trim() != "")
                CaptchaSolver.SetKey(textBox_antiCaptcha.Text.Trim());
        }

        private void button_GetBalance_Click(object sender, EventArgs e)
        {
            try
            {
                label_BalanceCaptcha.Text = "Баланс: " + CaptchaSolver.GetBalance() + " ₽";
            }
            catch (Exception ex)
            {
                Log.Push(ex.Message, "", false, Log.TypeLogger.File);
            }
        }

        private void button_GetConversations_Click(object sender, EventArgs e)
        {
            foreach (var acc in Accounts)
            {
                Log.Push($"[{acc.Login}]: поиск активных чатов...", "", false, Log.TypeLogger.File);

                comboBox_ActiveChat.Items.Clear();
                var account = Accounts[comboBox_accountsList.SelectedIndex];
                dynamic convertsations = account.GetConversations();
                try
                {
                    for (int i = 0; i < 51; i++)
                    {
                        if (Convert.ToString(convertsations["response"]["items"][i]["conversation"]["peer"]["type"]) == "user" && checkBox_ConversationUser.Checked)
                        {
                            var id = Convert.ToString(convertsations["response"]["items"][i]["conversation"]["peer"]["id"]);
                            var title = Convert.ToString(convertsations["response"]["items"][i]["conversation"]["peer"]["local_id"]);
                            dynamic json = JsonConvert.DeserializeObject(Server.APIRequest("users.get", $"user_ids={id}", acc.Token));
                            comboBox_ActiveChat.Items.Add($"im?sel={id} ({Convert.ToString(json["response"][0]["first_name"])} {Convert.ToString(json["response"][0]["last_name"])})");
                            comboBox_ActiveChat.SelectedIndex = 0;
                        }

                        if (Convert.ToString(convertsations["response"]["items"][i]["conversation"]["peer"]["type"]) == "chat" && checkBox_ConversationsChats.Checked)
                        {
                            var titleConversations = Convert.ToString(convertsations["response"]["items"][i]["conversation"]["chat_settings"]["title"]);
                            var localID = Convert.ToString(convertsations["response"]["items"][i]["conversation"]["peer"]["local_id"]);
                            comboBox_ActiveChat.Items.Add($"im?sel=c{localID} ({titleConversations})");
                            comboBox_ActiveChat.SelectedIndex = 0;
                        }
                    }
                }
                catch { }

                Log.Push($"[{acc.Login}]: доступных чатов: {comboBox_ActiveChat.Items.Count}", "", false, Log.TypeLogger.File);
            }
        }

        private void comboBox_ActiveChat_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(comboBox_ActiveChat.Text.Replace(StrWrk.GetBetween(comboBox_ActiveChat.Text, "(", ")"), "").Replace("()", ""));
        }

        private void comboBox_ActiveChat_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите закрыть приложение?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                e.Cancel = false;
            else
                e.Cancel = true;
        }

        private void checkBox_loggerOfFile_CheckedChanged(object sender)
        {
            if (checkBox_loggerOfFile.Checked)
                Log.EnabledLogFile = true;
            else
                Log.EnabledLogFile = false;
        }

        private string GetToken()
        {
            return Accounts[comboBox_accountsList.SelectedIndex].Token;
        }
        private void button_applySettingsAccount_Click(object sender, EventArgs e)
        {
            try
            {
                var account = new AccountEdit();
                if (!string.IsNullOrEmpty(textBox_firstName.Text) || !string.IsNullOrEmpty(textBox_lastName.Text))
                    account.EditName(textBox_firstName.Text, textBox_lastName.Text, GetToken());
                if (comboBox_relation.Text != null)
                    account.EditRelation(comboBox_relation.Text, GetToken());
                if (!string.IsNullOrEmpty(textBox_city.Text))
                    account.EditCity(textBox_city.Text, GetToken());
                if (!string.IsNullOrEmpty(textBox_status.Text))
                    account.EditStatus(textBox_status.Text, GetToken());
                if (!string.IsNullOrEmpty(textBox_bdate.Text))
                    account.EditBDate(textBox_bdate.Text, GetToken());
            }
            catch
            {

            }
        }

        private void button_SetPrivateSettings_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox_Value.SelectedIndex == 0)
                    GetPrivate.Values = GetPrivate.Value.All;
                if (comboBox_Value.SelectedIndex == 1)
                    GetPrivate.Values = GetPrivate.Value.Friends;
                if (comboBox_Value.SelectedIndex == 2)
                    GetPrivate.Values = GetPrivate.Value.OnlyMe;

                var ps = new PrivateSettings();
                GetPrivate.EnabledSearchByRegPhone = checkBox_searchPhone.Checked;
                GetPrivate.EnabledClosedProfile = checkBox_ClosedProfile.Checked;

                if (checkBox_CurrentAccount.Checked)
                    ps.AsyncWorker(Accounts[comboBox_accountsList.SelectedIndex]);
                else
                {
                    foreach (var item in Accounts)
                        ps.AsyncWorker(item);
                }
            }
            catch
            {

            }
        }

        private void comboBox_ProfanityFilter_CheckedChanged(object sender)
        {
            if (comboBox_ProfanityFilter.Checked)
                Account.Filter = true;
            else
                Account.Filter = false;
        }
        public static bool SetActivity = false;
        private void checkBox_setActivity_CheckedChanged(object sender)
        {
            if (checkBox_setActivity.Checked)
                SetActivity = true;
            else
                SetActivity = false;
        }

        private void OnTimeEvent(object sender, ElapsedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                Second += 1;
                if (Second == 60)
                {
                    Second = 0;
                    Minutes += 1;
                }
                if (Minutes == 60)
                {
                    Minutes = 0;
                    Hours += 1;
                }
                WorksTime.Text = $"Время работы: {Hours.ToString().PadLeft(2, '0')}:{Minutes.ToString().PadLeft(2, '0')}:{Second.ToString().PadLeft(2, '0')}";
            }));
        }

        private void LeaveNumeric(object o, EventArgs e)
        {
            var account = Accounts[comboBox_accountsList.SelectedIndex];
            timeMinutesFlooder.Text = "Время в минутах (флудер): " + (StrWrk.IsInteger(numeric_DelayMinFlooder.Text, 333) / 60000) * account.FlooderTargets.Count + " мин.";
            account.TimeMinutes = timeMinutesFlooder.Text;
            account.CountTarget = countTarget.Text;
        }
    }
}
