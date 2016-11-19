using System;
using System.Linq;
using Android.App;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Android.Content;
using Java.Net;

namespace AndroidAPI22ADCLibrary.Helpers
{
    class mAuthenticator : Authenticator
    {
        public async Task<AuthenticationResult> Authenticate(string authority, string resource, string clientId, string returnUri,Activity activity)
        {
            var authContext = new AuthenticationContext(authority);
            if (authContext.TokenCache.ReadItems().Any())
                authContext = new AuthenticationContext(authContext.TokenCache.ReadItems().First().Authority);
            var uri = new Uri(returnUri);
            var platformParams = new PlatformParameters(activity);
            var authResult = await authContext.AcquireTokenAsync(resource, clientId, uri, platformParams);
            return authResult;
        }

        public async Task<bool> DeAuthenticate(string authority)
        {
            try
            {
                var authContext = new AuthenticationContext(authority);
                await Task.Factory.StartNew(() => {
                    authContext.TokenCache.Clear();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR EN LOGUT"+ex.ToString());
                return false;
            }
            return true;
        }

        //public async Task<string> FetchToken(string authority)
        //{
        //    try
        //    {
        //        return
        //            (new AuthenticationContext(authority))
        //                .TokenCache
        //                .ReadItems()
        //                .FirstOrDefault(x => x.Authority == authority).AccessToken;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("ERROR EN FetchToken" + ex.ToString()); ;
        //    }
        //    return null;
        //}
    }
}