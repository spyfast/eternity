using Eternity.Engine.Network;
using Eternity.Utils.API;
using Newtonsoft.Json;
using System;
namespace Eternity.Utils
{
    internal sealed class VkUtils
    {
        public static string Apps;
        public string Info { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public VkUtils(string login, string password)
        {
            Login = login;
            Password = password;
        }
        public string Authorize()
        {
            try
            {
                var token = string.Empty;
                var clientSecret = string.Empty;
                var clientKey = 0;

                switch (Apps)
                {
                    case "Android":
                        clientKey = 2274003;
                        clientSecret = "hHbZxrka2uZ6jB1inYsH";
                        break;
                    case "iPhone":
                        clientKey = 3140623;
                        clientSecret = "VeWdmVclDCtn6ihuP1nt";
                        break;
                }

                var response = Request(clientSecret, clientKey.ToString(), Server.Version);
                dynamic json = JsonConvert.DeserializeObject(response);
                token = Convert.ToString(json["access_token"]);

                GetUser(token);
                return token;
            }
            catch (Exception ex)
            {
                return "error " + ex.Message;
            }
        }
        private string Request(string clientSecret, string clientKey, string version)
        {
            return 
                Network.GET(
                    $"https://oauth.vk.com/token?grant_type=password&client_id={clientKey}&client_secret={clientSecret}&username={Login}&password={Password}&v={version}&2fa_supported=1");
        }
        private void GetUser(string token)
        {
            var response = Server.APIRequest("users.get", "", token);
            dynamic json = JsonConvert.DeserializeObject(response);
            Info = $"{json["response"][0]["first_name"]} {json["response"][0]["last_name"]}";
        }
    }
}
