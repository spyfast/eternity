using Eternity.Configs.Logger;
using Eternity.Engine.Helpers;
using Eternity.Forms;
using Eternity.Targets;
using Eternity.Targets.Chat;
using Eternity.Utils;
using Eternity.Utils.API;
using Eternity.Utils.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using static Eternity.Utils.LinksParser.TargetData;

namespace Eternity.Engine.Accounts
{
    public sealed class Account
    {
        public static bool IsSendFalse = true;
        public static bool _workings;
        private Thread _thread;
        private readonly Random _rnd = new Random();
        public string Login { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string Info { get; set; }

        public string Applications;

        public List<FlooderTarget> FlooderTargets;
        public List<ConversationsTarget> ConversationsTargets;

        // Flooder Settings
        public string SelectDelay, DelayMin, DelayMax, LocationName, PhrasesFile;
        public string TimeMinutes, CountTarget;
        public string DelayConversations;
        public bool EnabledFlooder, EnabledConversations;
        public static bool Filter = false;
        public string[] Phrases;
        public Account()
        {
            FlooderTargets = new List<FlooderTarget>();
            ConversationsTargets = new List<ConversationsTarget>();
        }
        public AuthStatus? Authorize()
        {
            try
            {
                var api = new VkUtils(Login, Password);
                Token = api.Authorize();
                Info = api.Info;
              
                if (!Token.Contains("error"))
                {
                    Save();
                    return AuthStatus.Ok;
                }
                else
                    return AuthStatus.Invalid;

            }
            catch (Exception ex)
            {
                Log.Push($"Ошибка при авторизации аккаунта — {Login}: {ex.Message}", "", false, Log.TypeLogger.File);
                return AuthStatus.Other;
            }
        }
        public void Save()
            => File.WriteAllText(
                Path.Combine("AccsData", $"{Login}.json"),
                     JsonConvert.SerializeObject(this, Formatting.Indented));
        public dynamic GetConversations()
        {
            dynamic conversations = JsonConvert.DeserializeObject(Server.APIRequest("messages.getConversations", "count=100&filter=all", Token));
            return conversations;
        }

        #region ParseDataGrid
        public void ParseDataGridFlooder(DataGridView view)
        {
            FlooderTargets.Clear();

            foreach (DataGridViewRow row in view.Rows)
            {
                FlooderTargets.Add(new FlooderTarget
                {
                    Name = (row.Cells[0].Value ?? "").ToString(),
                    Link =  (row.Cells[1].Value ?? "").ToString(),
                    Contains = (row.Cells[2].Value ?? "").ToString()
                });
            }

            FlooderTargets.RemoveAt(FlooderTargets.Count - 1);
        }

        public void ParseDataGridConversations(DataGridView view)
        {
            ConversationsTargets.Clear();

            foreach (DataGridViewRow row in view.Rows)
            {
                ConversationsTargets.Add(new ConversationsTarget
                {
                    ChatId = (row.Cells[0].Value ?? "").ToString(),
                    Title = (row.Cells[1].Value ?? "").ToString(),
                    Method = (row.Cells[2].Value ?? "").ToString()
                });
            }

            ConversationsTargets.RemoveAt(ConversationsTargets.Count - 1);
        }
        #endregion
        private string RandomStickers()
        {
            var sticker = File.ReadAllLines("Files\\stickers.txt", Encoding.UTF8).ToList();
            if (sticker.Count == 0)
                Log.Push("Отсутствуют стикеры");
            return sticker[_rnd.Next(sticker.Count)];
        }
        public string RandomContent()
        {
            var content = File.ReadAllLines("Files\\content.txt", Encoding.UTF8).ToList();

            if (content.Count == 0)
                Log.Push("Отсутствует контент");
            return content[_rnd.Next(content.Count)];
        }
        public void SetActivity(int index)
        {
            if (MainForm.SetActivity)
            {
                var link = FlooderTargets[index].Link;
                var sendLink = LinksParser.Parse(link);
                if (sendLink.Type == TypeTarget.User)
                    Server.APIRequest("messages.setActivity", $"user_id={sendLink.Id1}&type=typing", Token);
                if (sendLink.Type == TypeTarget.Chat)
                    Server.APIRequest("messages.setActivity", $"chat_id={sendLink.Id1}&type=typing", Token);
            }
        }
        private void SendMessage(int index, int indexPh)
        {
            var message = Phrases[indexPh];
            var name = FlooderTargets[index].Name;
            var link = FlooderTargets[index].Link;
            var contains = FlooderTargets[index].Contains;
            var sendLink = LinksParser.Parse(link);
            var reply = string.Empty;

            if (Filter)
                message = ProfanityFilter.Replace(message);

            switch (LocationName)
            {
                case "Начало":
                    message = name + message;
                    break;
                case "Конец":
                    message = message + name;
                    break;
                default:
                    message = name + message;
                    break;
            }

            if (link.Contains(":"))
                reply = link.Split(':')[1];

            switch (contains)
            {
                case "Текст":
                    if (sendLink.Type == TypeTarget.Chat)
                        Server.APIRequest("messages.send", $"message={message}&chat_id={sendLink.Id1}&random_id={_rnd.Next()}", Token);
                    else if (sendLink.Type == TypeTarget.User)
                        Server.APIRequest("messages.send", $"message={message}&user_id={sendLink.Id1}&random_id={_rnd.Next()}", Token);
                    else if (sendLink.Type == TypeTarget.Wall)
                        Server.APIRequest("wall.createComment", $"owner_id={sendLink.Id1}&post_id={sendLink.Id2}&message={message}&reply_to_comment={reply}", Token);
                    else
                        Log.Push($"\"{link}\" — неверный формат ссылки");
                    if (IsSendFalse)
                        Log.Push($"постинг {message} в {link}", Login, false, Log.TypeLogger.File);
                    break;
                case "Текст+стикер":
                    if (sendLink.Type == TypeTarget.Chat)
                    {
                        Server.APIRequest("messages.send", $"message={message}&chat_id={sendLink.Id1}&random_id={_rnd.Next()}", Token);
                        Thread.Sleep(700);
                        Server.APIRequest("messages.send", $"sticker_id={RandomStickers()}&chat_id={sendLink.Id1}&random_id={_rnd.Next()}", Token);
                    }
                    else if (sendLink.Type == TypeTarget.User)
                    {
                        Server.APIRequest("messages.send", $"message={message}&user_id={sendLink.Id1}&random_id={_rnd.Next()}", Token);
                        Thread.Sleep(700);
                        Server.APIRequest("messages.send", $"sticker_id={RandomStickers()}&user_id={sendLink.Id1}&random_id={_rnd.Next()}", Token);
                    }
                    else if (sendLink.Type == TypeTarget.Wall)
                    {
                        Server.APIRequest("wall.createComment", $"owner_id={sendLink.Id1}&post_id={sendLink.Id2}&message={message}&reply_to_comment={reply}", Token);
                        Thread.Sleep(700);
                        Server.APIRequest("wall.createComment", $"owner_id={sendLink.Id1}&post_id={sendLink.Id2}&sticker_id={RandomStickers()}&reply_to_comment={reply}", Token);
                    }
                    else
                        Log.Push($"\"{link}\" — неверный формат ссылки", "", false, Log.TypeLogger.File);
                    if (IsSendFalse)
                        Log.Push($"постинг {message} + стикер в {link}", Login, false, Log.TypeLogger.File);
                    break;
                case "Текст+контент":
                    if (sendLink.Type == TypeTarget.Chat)
                    {
                        var resp =
                            Server.APIRequest("messages.send", $"message={message}&attachment={RandomContent()}&chat_id={sendLink.Id1}&random_id={new Random().Next()}", Token);
                        Log.Push(resp);
                        if (resp.Contains("error"))
                        {
                            Log.Push("[VK API]: Не удалось отправить сообщение... Пробую ещё раз.");
                            Server.APIRequest("messages.send", $"message={message}&attachment={RandomContent()}&chat_id={sendLink.Id1}&random_id={new Random().Next()}", Token);
                        }
                    }
                    else if (sendLink.Type == TypeTarget.User)
                        Server.APIRequest("messages.send", $"message={message}&attachment={RandomContent()}&user_id={sendLink.Id1}&random_id={new Random().Next()}", Token);
                    else if (sendLink.Type == TypeTarget.Wall)
                        Server.APIRequest("wall.createComment", $"attachments={RandomContent()}&owner_id={sendLink.Id1}&post_id={sendLink.Id2}&message={message}&reply_to_comment={reply}", Token);
                    else
                        Log.Push($"\"{link}\" — неверный формат ссылки");
                    if (IsSendFalse)
                        Log.Push($"постинг {message} + контент в {link}", Login, false, Log.TypeLogger.File);
                    break;
            }
        }
        private void StartFlooder()
        {
            int index = -1;
            int indexPh = -1;

            while (_workings && EnabledFlooder)
            {
                try
                {
                    index = (index + 1) % FlooderTargets.Count;
                    indexPh = (indexPh + 1) % Phrases.Length;

                    SendMessage(index, indexPh);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.ToLower().Contains("this chat") || !ex.Message.ToLower().Contains("kicked"))
                        Log.Push(ex.Message, "", false, Log.TypeLogger.File);
                    if (ex.Message.Contains("blocked"))
                        Log.Push(ex.Message, "", true, Log.TypeLogger.File);
                    if (ex.Message.ToLower().Contains("browser"))
                        Log.Push("Возможно, был отвязан номер", "", true);
                }


                switch (SelectDelay)
                {
                    case "Обычная":
                        Thread.Sleep(StrWrk.IsInteger(DelayMin, 333));
                        break;
                    case "Рандомная":
                        Thread.Sleep(_rnd.Next(StrWrk.IsInteger(DelayMin, 333),
                                StrWrk.IsInteger(DelayMax, 500)));
                        break;
                }
            }
        }

        private void ProcConversations(int index)
        {
            if (ConversationsTargets.Count != 0)
            {
                var js = new JavaScriptSerializer();
                var chatId = ConversationsTargets[index].ChatId;
                var title = ConversationsTargets[index].Title;
                var method = ConversationsTargets[index].Method;

                var titles = File.ReadAllLines("Files\\titles.txt").ToList();
                chatId.Substring(0, chatId.Length - 1);
                var response = StrWrk.qSubstr(
                    Server.APIRequest("messages.getChat", $"chat_ids={chatId}", Token), "\"response\":", false);
                response = response.Substring(0, response.Length - 1);
                var chatInfo = js.Deserialize<List<ChatInfo>>(response).GetEnumerator();
                var execute = new ExecuteManager(Token);

                while (chatInfo.MoveNext())
                {
                    if (method.Contains("Удалять") && chatInfo.Current.photo_50 != null)
                        execute.Add("API.messages.deleteChatPhoto({\"chat_id\":" + chatId + "});");
                    if (method.Contains("Менять") && title != "" && title != chatInfo.Current.title)
                    {
                        execute.Add("API.messages.editChat({\"chat_id\":" + chatId +
                            ", \"title\":\"" + title.Replace("\"", "\\\"") + "\"});");
                        Log.Push($"Название чата №{chatId} изменено на \"{title}\"");
                    }

                    if (method.Contains("Флуд"))
                    {
                        if (titles.Count == 0)
                            Log.Push("Отсутствуют названия для флуда");
                        else
                        {
                            var updateTitle = titles[_rnd.Next(titles.Count)];
                            execute.Add("API.messages.editChat({\"chat_id\":" + chatId +
                               ", \"title\":\"" + updateTitle.Replace("\"", "\\\"") + "\"});");
                            Log.Push($"Название чата №{chatId} изменено на \"{updateTitle}\"");
                        }
                    }
                }
                execute.Execute();
            }
        }
        private void StartConversations()
        {
            int index = -1;

            while (_workings && EnabledConversations)
            {
                try
                {
                    index = (index + 1) % ConversationsTargets.Count;
                    ProcConversations(index);
                }
                catch (Exception ex)
                {
                    Log.Push(ex.Message);
                }

                Thread.Sleep(StrWrk.IsInteger(DelayConversations, 333));
            }
        }
        public void AsyncWorker()
        {
            if (_thread != null)
                _thread.Abort();

            if (EnabledFlooder)
            {
                _thread = new Thread(StartFlooder)
                {
                    IsBackground = true
                };
                _thread.Start();
            }

            if (EnabledConversations)
            {
                _thread = new Thread(StartConversations)
                {
                    IsBackground = true
                };
                _thread.Start();
            }
        }
    }
}
