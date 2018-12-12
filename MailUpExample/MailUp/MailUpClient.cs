using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace MailUp {
    public enum ContentType {
        Json,
        Xml
    }

    public class MailUpException : Exception {
        public int StatusCode { set; get; }

        public MailUpException(int statusCode, string message) : base(message) {
            MailUpException mailUpException = this;
            mailUpException.StatusCode = statusCode;
        }
    }
    public partial class MailUpClient {
        public MailUpClient(string clientId, string clientSecret, string callbackUri) {
            MailUpClient mailUpClient = this;
            mailUpClient.ClientId = clientId;
            mailUpClient.ClientSecret = clientSecret;
            mailUpClient.CallbackUri = callbackUri;
            LoadToken();
        }
        public void LogOn() {
            string url = GetLogOnUri();
            HttpContext.Current.Response.Redirect(url);
        }
        public void LogOnWithUsernamePassword(string username, string password) {
            MailUpClient mailUpClient = this;
            mailUpClient.RetrieveAccessToken(username, password);
        }
        public string LogonEndpoint { get; set; } = "https://services.mailup.com/Authorization/OAuth/LogOn";
        public string AuthorizationEndpoint { get; set; } = "https://services.mailup.com/Authorization/OAuth/Authorization";
        public string TokenEndpoint { get; set; } = "https://services.mailup.com/Authorization/OAuth/Token";
        public string ConsoleEndpoint { get; set; } = "https://services.mailup.com/API/v1.1/Rest/ConsoleService.svc";
        public string MailstatisticsEndpoint { get; set; } = "https://services.mailup.com/API/v1.1/Rest/MailStatisticsService.svc";
        public string ClientId { set; get; }
        public string ClientSecret { set; get; }
        public string CallbackUri { set; get; }
        public string AccessToken { set; get; }
        public string RefreshToken { set; get; }       
        public string GetLogOnUri() {
            return $"{LogonEndpoint}?client_id={ClientId}&client_secret={ClientSecret}&response_type=code&redirect_uri={CallbackUri}" ;
        }    
        public string RetrieveAccessToken(string code) {
            int statusCode = 0;
            try {
                HttpWebRequest wrLogon = (HttpWebRequest)WebRequest.Create($"{TokenEndpoint}?code={code}&grant_type=authorization_code");
                wrLogon.AllowAutoRedirect = false;
                wrLogon.KeepAlive = true;
                HttpWebResponse retrieveResponse = (HttpWebResponse)wrLogon.GetResponse();
                statusCode = (int)retrieveResponse.StatusCode;
                Stream objStream = retrieveResponse.GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);
                string json = objReader.ReadToEnd();
                retrieveResponse.Close();
                AccessToken = ExtractJsonValue(json, "access_token");
                RefreshToken = ExtractJsonValue(json, "refresh_token");
                SaveToken();
            }
            catch (WebException wex) {
                HttpWebResponse wrs = (HttpWebResponse)wex.Response;
                throw new MailUpException((int)wrs.StatusCode, wex.Message);
            }
            catch (Exception ex) {
                throw new MailUpException(statusCode, ex.Message);
            }
            return AccessToken;
        }
        public string RetrieveAccessToken(string login, string password) {
            int statusCode = 0;
            try {
                CookieContainer cookies = new CookieContainer();
                string body = $"client_id={ClientId}&client_secret={ClientSecret}&grant_type=password&username={login}&password={password}" ;
                HttpWebRequest wrLogon = (HttpWebRequest)WebRequest.Create(TokenEndpoint);
                wrLogon.CookieContainer = cookies;
                wrLogon.AllowAutoRedirect = false;
                wrLogon.KeepAlive = true;
                wrLogon.Method = "POST";
                wrLogon.ContentType = "application/x-www-form-urlencoded";
                string auth = $"{ClientId}:{ClientSecret}";
                wrLogon.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));
                byte[] byteArray = Encoding.UTF8.GetBytes(body);
                wrLogon.ContentLength = byteArray.Length;
                Stream dataStream = wrLogon.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                HttpWebResponse tokenResponse = (HttpWebResponse)wrLogon.GetResponse();
                statusCode = (int)tokenResponse.StatusCode;
                Stream objStream = tokenResponse.GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);
                string json = objReader.ReadToEnd();
                tokenResponse.Close();
                AccessToken = ExtractJsonValue(json, "access_token");
                RefreshToken = ExtractJsonValue(json, "refresh_token");
                SaveToken();
            }
            catch (WebException wex) {
                HttpWebResponse wrs = (HttpWebResponse)wex.Response;
                throw new MailUpException((int)wrs.StatusCode, wex.Message);
            }
            catch (Exception ex) {
                throw new MailUpException(statusCode, ex.Message);
            }
            return AccessToken;
        }
        public string RefreshAccessToken() {
            int statusCode = 0;
            try {
                HttpWebRequest wrLogon = (HttpWebRequest)WebRequest.Create(TokenEndpoint);
                wrLogon.AllowAutoRedirect = false;
                wrLogon.KeepAlive = true;
                wrLogon.Method = "POST";
                wrLogon.ContentType = "application/x-www-form-urlencoded";

                string body = "client_id=" + ClientId + "&client_secret=" + ClientSecret + "&refresh_token=" + RefreshToken + "&grant_type=refresh_token";
                byte[] byteArray = Encoding.UTF8.GetBytes(body);
                wrLogon.ContentLength = byteArray.Length;
                Stream dataStream = wrLogon.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                HttpWebResponse refreshResponse = (HttpWebResponse)wrLogon.GetResponse();
                statusCode = (int)refreshResponse.StatusCode;
                Stream objStream = refreshResponse.GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);
                string json = objReader.ReadToEnd();
                refreshResponse.Close();
                AccessToken = ExtractJsonValue(json, "access_token");
                RefreshToken = ExtractJsonValue(json, "refresh_token");
                SaveToken();
            }
            catch (WebException wex) {
                HttpWebResponse wrs = (HttpWebResponse)wex.Response;
                throw new MailUpException((int)wrs.StatusCode, wex.Message);
            }
            catch (Exception ex) {
                throw new MailUpException(statusCode, ex.Message);
            }
            return AccessToken;
        }
        public string CallMethod(string url, string verb, string body, ContentType contentType = ContentType.Json) {
            return CallMethod(url, verb, body, contentType, true);
        }
        private string CallMethod(string url, string verb, string body, ContentType contentType = ContentType.Json, bool refresh = true) {
            string result = "";
            HttpWebResponse callResponse = null;
            int statusCode = 0;
            try {
                HttpWebRequest wrLogon = (HttpWebRequest)WebRequest.Create(url);
                wrLogon.AllowAutoRedirect = false;
                wrLogon.KeepAlive = true;
                wrLogon.Method = verb;
                wrLogon.ContentType = GetContentTypeString(contentType);
                wrLogon.ContentLength = 0;
                wrLogon.Accept = GetContentTypeString(contentType);
                wrLogon.Headers.Add("Authorization", $"Bearer {AccessToken}");

                if (body != null && body != "") {
                    byte[] byteArray = Encoding.UTF8.GetBytes(body);
                    wrLogon.ContentLength = byteArray.Length;
                    Stream dataStream = wrLogon.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                }

                callResponse = (HttpWebResponse)wrLogon.GetResponse();
                statusCode = (int)callResponse.StatusCode;
                Stream objStream = callResponse.GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);
                result = objReader.ReadToEnd();
                callResponse.Close();
            }
            catch (WebException wex) {
                try {
                    HttpWebResponse wrs = (HttpWebResponse)wex.Response;
                    if ((int)wrs.StatusCode == 401 && refresh) {
                        RefreshAccessToken();
                        return CallMethod(url, verb, body, contentType, false);
                    }
                    else throw new MailUpException((int)wrs.StatusCode, wex.Message);
                }
                catch (Exception ex) {
                    throw new MailUpException(statusCode, ex.Message);
                }
            }
            catch (Exception ex) {
                throw new MailUpException(statusCode, ex.Message);
            }
            return result;
        }
        private string ExtractJsonValue(string json, string name) {
            string delim = "\"" + name + "\":\"";
            int start = json.IndexOf(delim) + delim.Length;
            int end = json.IndexOf("\"", start + 1);
            if (end > start && start > -1 && end > -1) return json.Substring(start, end - start);
            else return "";
        }
        private string GetContentTypeString(ContentType cType) {
            if (cType == ContentType.Json) return "application/json";
            else return "application/xml";
        }
        public virtual void LoadToken() {
            HttpCookie cookie = HttpContext.Current.Request.Cookies["MailUpCookie"];
            if (cookie != null) {
                if (!string.IsNullOrEmpty(cookie.Values["access_token"])) {
                    AccessToken = cookie.Values["access_token"].ToString();
                }
                if (!string.IsNullOrEmpty(cookie.Values["refresh_token"])) {
                    RefreshToken = cookie.Values["refresh_token"].ToString();
                }
            }
        }
        public virtual void SaveToken() {
            HttpCookie cookie = new HttpCookie("MailUpCookie");
            cookie.Values.Add("access_token", AccessToken);
            cookie.Values.Add("refresh_token", RefreshToken);
            cookie.Expires = DateTime.Now.AddDays(30);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
    }
}