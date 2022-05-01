using Eternity.Engine.Accounts;
using Eternity.Utils.Edit.Settings;
using System.Threading;
using VkNet;
using VkNet.Model;

namespace Eternity.Utils.Edit
{
    public class PrivateSettings
    {
        public void SetPrivateSettings(Account account)
        {
            try
            {
                var api = new VkApi();

                api.Authorize(new ApiAuthParams { AccessToken = account.Token });
                var value = string.Empty;
                var closedProfile = string.Empty;

                if (GetPrivate.EnabledClosedProfile)
                    closedProfile = "true";
                else
                    closedProfile = "false";

                api.Account.SetPrivacy(GetPrivate.ClosedProfile, closedProfile);

                if (GetPrivate.EnabledSearchByRegPhone)
                    api.Account.SetPrivacy(GetPrivate.SearchByRegPhone, GetPrivate.NoBody);
                else
                    api.Account.SetPrivacy(GetPrivate.SearchByRegPhone, GetPrivate.All);

                if (GetPrivate.Values == GetPrivate.Value.All)
                    value = GetPrivate.All;
                if (GetPrivate.Values == GetPrivate.Value.Friends)
                    value = GetPrivate.Friends;
                if (GetPrivate.Values == GetPrivate.Value.OnlyMe)
                    value = GetPrivate.OnlyMe;

                foreach (var key in GetPrivate.Key)
                    api.Account.SetPrivacy(key, value);
            }
            catch
            {

            }
        }

        public void AsyncWorker(Account account)
        {
            new Thread(() => SetPrivateSettings(account)).Start();
        }
    }
}
