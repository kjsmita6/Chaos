using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Chaos.Helpers
{
    class SteamAPI
    {
        /// <summary>
        /// Calls a method on the steam api 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="i">Interface, starts with I</param>
        /// <param name="e">API Endpoint</param>
        /// <param name="v">Version</param>
        /// <param name="m">Method - default = GET</param>
        /// <param name="k">API Key - default = ""</param>
        /// <param name="o">Options - must be in form option1=value1&options2=value2 - defualt = ""</param>
        /// <returns>Json-formatted class type</returns>
        public static async Task<T> Request<T>(string i, string e, string v, string m = "", string k = "", string o = "") where T : class
        {
            HttpWebResponse response;
            try
            {
                string uri = string.Format("https://api.steampowered.com/{0}/{1}/{2}/?json=true", i, e, v);
                if(k != "")
                {
                    uri += "&key=" + k;
                }
                if(o != "")
                {
                    uri += "&" + o;
                }
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                response = (HttpWebResponse)await request.GetResponseAsync();
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    return sr.ReadToEnd().ParseJSON<T>();
                }
            }
            catch(WebException we)
            {
                Log.Instance.Error(we.Message + ": " + we.StackTrace);
                return default(T);
            }
            catch(Exception err)
            {
                Log.Instance.Error(err.Message + ": " + err.StackTrace);
                return default(T);
            }
        }
    }
}
