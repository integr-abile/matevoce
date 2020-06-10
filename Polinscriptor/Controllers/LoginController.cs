using Polinscriptor.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Polinscriptor.Controllers
{
    class LoginController
    {
        public async Task<bool> Login(string username, string md5Password)
        {
            string loginUrl = App.Data.GetAppConfigValue("loginUrl");
            var postParams = new Dictionary<string, string>
            {
                {"username",username },
                {"password",md5Password }
            };
            var answer = await new RestService().PostRequestFormUrlEncodedFields(loginUrl, postParams);
            if(answer.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var res = (string)answer.JsonAnswer["g_api_key"];
                App.Data.G_API_KEY = res;
                return true;
            }
            return false;
        }
    }
}
