using Eternity.Configs.Logger;
using Eternity.Utils.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eternity.Utils.Edit
{
    public class AccountEdit
    {
        public void EditName(string firstName, string lastName, string token)
        {
            try
            {
                var response =
                    Server.APIRequest("account.saveProfileInfo", $"first_name={firstName}&last_name={lastName}", token);

                if (response.Contains("success"))
                    Log.Show("Имя успешно изменено");
                else if (response.Contains("processing"))
                    Log.Show("Заявка рассматривается");
                else if (response.Contains("declined"))
                    Log.Show("Заявка отклонена");
                else if (response.Contains("was_accepted"))
                    Log.Show("Недавно уже была одобрена заявка");
                else if (response.Contains("was_declined"))
                    Log.Show("Недавно заявка была отклонена, повторно подать нельзя");
                else
                    Log.Show("Неизвестный ответ от сервера ВК");
            }
            catch
            {

            }
        }

        public void EditRelation(string text, string token)
        {
            try
            {
                switch (text)
                {
                    case "не женат/не замужем":
                        Server.APIRequest("account.saveProfileInfo", $"relation=1", token);
                        break;
                    case "есть друг/есть подруга":
                        Server.APIRequest("account.saveProfileInfo", $"relation=2", token);
                        break;
                    case "помолвлен/помолвлена":
                        Server.APIRequest("account.saveProfileInfo", $"relation=3", token);
                        break;
                    case "женат/замужем":
                        Server.APIRequest("account.saveProfileInfo", $"relation=4", token);
                        break;
                    case "всё сложно":
                        Server.APIRequest("account.saveProfileInfo", $"relation=5", token);
                        break;
                    case "в активном поиске":
                        Server.APIRequest("account.saveProfileInfo", $"relation=6", token);
                        break;
                    case "влюблён/влюблена":
                        Server.APIRequest("account.saveProfileInfo", $"relation=7", token);
                        break;
                    case "в гражданском браке":
                        Server.APIRequest("account.saveProfileInfo", $"relation=8", token);
                        break;
                    case "не указано":
                        Server.APIRequest("account.saveProfileInfo", $"relation=0", token);
                        break;
                }
            }
            catch
            {

            }
        }

        public void EditCity(string city, string token)
        {
            try
            {
                Server.APIRequest("account.saveProfileInfo", $"home_town={city}", token);
            }
            catch
            {

            }
        }

        public void EditStatus(string text, string token)
        {
            try
            {
                Server.APIRequest("account.saveProfileInfo", $"status={text}", token);
            }
            catch
            {

            }
        }

        public void EditBDate(string text, string token)
        {
            try
            {
                Server.APIRequest("account.saveProfileInfo", $"bdate={text}", token);
            }
            catch
            {

            }
        }
    }
}
