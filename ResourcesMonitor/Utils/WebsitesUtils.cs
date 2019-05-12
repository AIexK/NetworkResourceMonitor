using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace ResourcesMonitor.Utils
{
    public class WebsitesUtils
    {
        public static bool CheckSite(string url, string htmlToControl, out string message)
        {
            var result = true;
            message = string.Empty;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);

            // Set the credentials to the current user account
            request.Credentials = System.Net.CredentialCache.DefaultCredentials;
            request.Method = "GET";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    WebHeaderCollection header = response.Headers;

                    var encoding = ASCIIEncoding.UTF8;
                    using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
                    {
                        var responseText = reader.ReadToEnd();
                        result = responseText.Contains(htmlToControl);
                        if  (!result)
                        {
                            message = $"Site '{url}' is not available. The response doesn't contain test HTML markup";
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                message = $"Site '{url}' is not available: '{ex.Message}'";
                Logger.LogError(message);

                result = false;
            }
            return result;
        }
    }
}