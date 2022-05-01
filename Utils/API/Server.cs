using Eternity.Captcha;
using Eternity.Configs.Logger;
using Eternity.Engine.Helpers;
using Eternity.Engine.Network;
using Newtonsoft.Json;
using System;

namespace Eternity.Utils.API
{
    public static class Server
    {
        public static string Version = "5.131";
		public static string APIRequest(string method, string param, string token, string captcha_data = "")
		{
			if (captcha_data == "")
			{
				param = Uri.EscapeDataString(param);
				param = param.Replace("%3D", "=").Replace("%26", "&");
			}

			var response = Network.GET(
				$"https://api.vk.com/method/{method}?{param}&access_token={token}{captcha_data}&v={Version}");


            if (response.Contains("\"error_code\":14"))
            {
                dynamic json = JsonConvert.DeserializeObject(response);
                Log.Push("Капча... Решаю.");
                var captchaSid = Convert.ToString(json["error"]["captcha_sid"]);
                var captchaKey = CaptchaSolver.Solve(Convert.ToString(json["error"]["captcha_img"]));
                Log.Push($"Капча решена, код с картинки — {captchaKey}");
                return Network.GET($"https://api.vk.com/method/{method}?{param}&access_token={token}&v=5.107&captcha_sid={captchaSid}&captcha_key={captchaKey}");
            }
            else
            {
                if (response.Contains("\"error_code\""))
                {
                    throw new Exception(StrWrk.GetBetween(response, "\"error_msg\":\"", "\""));
                }
                return response;
            }
        }
	}
}
