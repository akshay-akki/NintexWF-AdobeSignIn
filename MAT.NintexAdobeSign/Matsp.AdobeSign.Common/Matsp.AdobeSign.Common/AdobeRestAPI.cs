
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Administration;

namespace Akki.AdobeSign.Common
{

    
    public class AdobeRestAPI
    {
        private string ClientID = string.Empty;
        private string ClientSecret = string.Empty;       


        public AdobeRestAPI(string apiURL, string accessToken)
        {
            //APIURL = apiURL.TrimEnd('/') + "/";            
            //AccessToken = accessToken;            
        }
        public AdobeRestAPI()
        {
            //APIURL = apiURL.TrimEnd('/') + "/";            
            //AccessToken = accessToken;            
        }

        public static string AccessToken
        {
            get
            {
                if (!SPFarm.Local.Properties.ContainsKey(Constants.KeyAccessToken))
                {
                    //log.Error($"[AccessToken]Farm is missing Access Token property [{Constants.KeyAccessToken}]");
                    throw new ApplicationException($"[AccessToken]Farm is missing Access Token property [{Constants.KeyAccessToken}]");
                }

                return SPFarm.Local.Properties[Constants.KeyAccessToken] as string;
            }
        }

        public static string AdobeApiUrl
        {
            get
            {
                if (!SPFarm.Local.Properties.ContainsKey(Constants.KeyAdobeApiUrl))
                {
                    //log.Error($"[AdobeApiUrl]Farm is missing  Adobe Api Url property [{Constants.KeyAdobeApiUrl}]");
                    throw new ApplicationException($"[AdobeApiUrl]Farm is missing  Adobe Api Url property [{Constants.KeyAdobeApiUrl}]");
                }

                string url = SPFarm.Local.Properties[Constants.KeyAdobeApiUrl] as string;
                url = url.EndsWith("/")?url: url+"/";
                return url;
            }
        }

        public static string AzureFxUrlWithCode
        {
           //somethign like this https://akkiadobeesignazurefx.azurewebsites.net/api/akkiAdobeEsignAzureFx?code=5RQgWJkgnTOkOxBYo9PnKq8ZZLqr01dP3nRNuyLdGglDKXKTj1kmHQ==
            get
            {
                if (!SPFarm.Local.Properties.ContainsKey(Constants.AzureFxUrlWithCode))
                {
                    //log.Error($"[AzureFxUrlWithCode]Farm is missing  Adobe Api Url property [{Constants.AzureFxUrlWithCode}]");
                    throw new ApplicationException($"[AzureFxUrlWithCode]Farm is missing  Adobe Api Url property [{Constants.AzureFxUrlWithCode}]");
                }

                return SPFarm.Local.Properties[Constants.AzureFxUrlWithCode] as string;               
                
            }

        }
        public static string FarmSysEmailAcc
        {
            get
            {
                if (!SPFarm.Local.Properties.ContainsKey(Constants.KeyFarmSysEmailAcc))
                {
                    //log.Error($"[FarmSysEmailAcc]Farm is missing Access Token property [{Constants.KeyFarmSysEmailAcc}]");
                    throw new ApplicationException($"[FarmSysEmailAcc]Farm is missing Access Token property [{Constants.KeyFarmSysEmailAcc}]");
                }

                return SPFarm.Local.Properties[Constants.KeyFarmSysEmailAcc] as string;
            }
        }

        public async Task<string> GetRestJson(string endpoint, string contentType = "application/json")
        {   
            HttpResponseMessage response = await GetResponseMessage(endpoint, contentType);

            if (response.IsSuccessStatusCode)
            {
                string responseMsg = await response.Content.ReadAsStringAsync();
                //log.Debug($"[GetRestJson]{responseMsg}");
                return responseMsg;
            }
            else
            {
                string err = await GetError(response);
                //log.Error($"[GetRestJson]{err}");
                throw new Exception(await GetError(response));
            }
        }


        public async Task<byte[]> GetRestBytes(string endpoint, string contentType = "*/*")
        {            
            HttpResponseMessage response = await GetResponseMessage(endpoint, contentType);           

            if (response.IsSuccessStatusCode)
            {

                return await response.Content.ReadAsByteArrayAsync();                
            }
            else
            {
                throw new Exception(await GetError(response));
            }
        }

        public async Task<string> PostRest(string endpoint, HttpContent data, string messageInitiatorEmail)
        {
            HttpResponseMessage response = PostResponseMessage(endpoint, data, messageInitiatorEmail);            
            if (response.IsSuccessStatusCode)
            {
                string responseMsg = await response.Content.ReadAsStringAsync();
                //log.Debug($"[PostRest]{responseMsg}");
                return responseMsg;
            }
            else
            {
                string err = await GetError(response);
                //log.Error($"[PostRest]{err}");
                throw new Exception(err);
            }
        }


        #region Private Methods

        private async Task<string> GetError(HttpResponseMessage response)
        {
            var errorString = await response.Content.ReadAsStringAsync();
            var errorCode = DeserializeJSon<ErrorCode>(errorString);
            return errorCode.code + errorCode.error + System.Environment.NewLine + errorCode.message + errorCode.error_description;
        }


        private async Task<HttpResponseMessage> GetResponseMessage(string endpoint, string contentType)
        {
            endpoint = endpoint.TrimStart('/');
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(AdobeRestAPI.AdobeApiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            client.DefaultRequestHeaders.Add(Constants.HttpHeaderAccessKey, AdobeRestAPI.AccessToken);

            return await client.GetAsync(endpoint);
        }


        private async Task<HttpResponseMessage> PostResponseMessageAsync(string endpoint, HttpContent contents, string contentType)
        {
            
            endpoint = endpoint.TrimStart('/');
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(AdobeApiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            client.DefaultRequestHeaders.Add(Constants.HttpHeaderAccessKey, AccessToken);           

            return await client.PostAsync(endpoint, contents);
        }

        private HttpResponseMessage PostResponseMessage(string endpoint, HttpContent contents, string messageInitiatorEmail)
        {

            endpoint = endpoint.TrimStart('/');
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(AdobeApiUrl);
            client.DefaultRequestHeaders.Accept.Clear();          
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.JsonContentType));
            client.DefaultRequestHeaders.Add(Constants.HttpHeaderAccessKey, AccessToken);
            if (!string.IsNullOrEmpty(messageInitiatorEmail))
                client.DefaultRequestHeaders.Add(Constants.HttpHeaderXApiUser, "email:" + messageInitiatorEmail);  
            else
                client.DefaultRequestHeaders.Add(Constants.HttpHeaderXApiUser, "email:" + AdobeRestAPI.FarmSysEmailAcc);

            return  client.PostAsync(endpoint, contents).Result;            
        }


        #endregion Private Methods


        #region JSON Methods

        internal string SerializeJSon<T>(T t)
        {
            string jsonString = string.Empty;

            DataContractJsonSerializerSettings a = new DataContractJsonSerializerSettings();
           

            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings()
            {
                
                DateTimeFormat = new DateTimeFormat("yyyy-MM-ddTHH:mm:ss-f:ff"),
                UseSimpleDictionaryFormat = true,
                
            };

            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializer ds = new DataContractJsonSerializer(typeof(T), settings);                                
                ds.WriteObject(stream, t);
                byte[] data = stream.ToArray();
                jsonString = Encoding.UTF8.GetString(data, 0, data.Length);
            }
            
            return jsonString;
        }

        internal T DeserializeJSon<T>(string jsonString)
        {
            T obj;
            dynamic dT = typeof(T);

            if (dT.Name.EndsWith("List"))
                dT = dT.DeclaredProperties[0].PropertyType.GenericTypeArguments[0];

            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings()
            {
                DateTimeFormat = new DateTimeFormat("yyyy-MM-ddTHH:mm:ss-f:ff"),
                UseSimpleDictionaryFormat = true
            };

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T), settings);           
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                obj = (T)ser.ReadObject(stream);
            }
            
            return obj;
        }

        #endregion JSON Methods


       

    }


}
