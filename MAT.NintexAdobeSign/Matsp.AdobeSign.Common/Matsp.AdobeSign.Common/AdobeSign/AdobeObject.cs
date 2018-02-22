using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Akki.AdobeSign.Common
{
    public class AdobeObject
    {
        private AdobeRestAPI API;
       
        public AdobeObject()
        {
            API = new AdobeRestAPI();
        }

        public async Task<BaseUriInfo> GetBaseURI()
        {
            string json = await API.GetRestJson("/base_uris");
            return API.DeserializeJSon<BaseUriInfo>(json);
        }


        #region Agreements
        public async Task<UserAgreements> GetAgreements()
        {
            string json = await API.GetRestJson("/agreements");            
            return API.DeserializeJSon<UserAgreements>(json);
        }

        public async Task<AgreementInfo> GetAgreement(string agreementID)
        {
            //On null or empty agreement id, API is returning all agreements.
            if(string.IsNullOrWhiteSpace(agreementID))
            {
                //TODO: log4Net error 
                return null;
            }

            var endpoint = string.Format("/agreements/{0}", agreementID);
            string json = await API.GetRestJson(endpoint);
            //TODO: log4net debug 
            return API.DeserializeJSon<AgreementInfo>(json);
        }

        public async Task<byte[]> GetAgreementCombinedDocument(string agreementID)
        {
            var endpoint = string.Format("/agreements/{0}/combinedDocument", agreementID);
            return await API.GetRestBytes(endpoint);
        }


        public async Task<AgreementCreationResponse> CreateAgreement(AgreementCreationInfo agreementCreationInfo, string messageInitiatorEmail)
        {
            var jsonContent = API.SerializeJSon<AgreementCreationInfo>(agreementCreationInfo);
            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            string json = await API.PostRest("/agreements", byteContent, messageInitiatorEmail);
            return API.DeserializeJSon<AgreementCreationResponse>(json);
            
        }

        public async Task<TransientDocumentResponse> AddDocument(string fileName, byte[] fileData, string messageInitiatorEmail)
        {
            var content = new MultipartFormDataContent();
            HttpContent fileContent = new ByteArrayContent(fileData);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "File",
                FileName = fileName

            };
            content.Add(fileContent);

            content.Add(new StringContent(fileName), String.Format("\"{0}\"", "File-Name"));


            //if (!string.IsNullOrWhiteSpace(mimeType))
            //{
            //    content.Add(new StringContent(mimeType), String.Format("\"{0}\"", "Mime-Type"));
            //}

            string json = await API.PostRest("/transientDocuments", content, messageInitiatorEmail);
            return API.DeserializeJSon<TransientDocumentResponse>(json);

        }


        #endregion Agreements


        #region STATIC METHODS

        /// <summary>
        /// Get the access token using authorization code
        /// </summary>
        /// <param name="apiURL">API Uri</param>
        /// <param name="authorization_code">Authorization Code - the authorization code obtained in Authorization Request process</param>
        /// <param name="clientid">Application ID - obtained from OAuth Configuration page / Identifies the application</param>
        /// <param name="client_secret">Client secret key - obtained from OAuth Configuration page / Authenticates the application</param>
        /// <param name="redirectURL">Redirect URL - must match the value used during the Authorization Code step / This value must belong to the set of values specified on the OAuth Configuration page</param>        
        /// <returns>AccessToken object</returns>
        public static async Task<AccessToken> GetAccessToken(string apiURL, string authorization_code, string clientid, string client_secret, string redirectURL)
        {
            AdobeRestAPI API = new AdobeRestAPI(apiURL, "");

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("code", authorization_code);
            parameters.Add("client_id", clientid);
            parameters.Add("client_secret", client_secret);
            parameters.Add("redirect_uri", redirectURL);
            parameters.Add("grant_type", "authorization_code");

            FormUrlEncodedContent encodedContent = new FormUrlEncodedContent(parameters);           

            string json = await API.PostRest("/oauth/token", encodedContent, "application/x-www-form-urlencoded");
            return API.DeserializeJSon<AccessToken>(json);
        }


        /// <summary>
        /// Get Access Token using refresh token
        /// </summary>
        /// <param name="apiURL">API Uri</param>
        /// <param name="refresh_token">Refresh Token, which can be used to get a fresh Access Token</param>
        /// <param name="clientid">Application ID - obtained from OAuth Configuration page / Identifies the application</param>
        /// <param name="client_secret">Client secret key - obtained from OAuth Configuration page / Authenticates the application</param>
        /// <returns>AccessToken object - Refresh_token property would be null on this call.</returns>
        public static async Task<AccessToken> GetAccessTokenByRefreshToken(string apiURL, string refresh_token, string clientid, string client_secret)
        {
            AdobeRestAPI API = new AdobeRestAPI(apiURL, "");

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("refresh_token", refresh_token);
            parameters.Add("client_id", clientid);
            parameters.Add("client_secret", client_secret);
            parameters.Add("grant_type", "refresh_token");

            FormUrlEncodedContent encodedContent = new FormUrlEncodedContent(parameters);

            string json = await API.PostRest("/oauth/refresh", encodedContent, "application/x-www-form-urlencoded");
            return API.DeserializeJSon<AccessToken>(json);
        }


        #endregion STATIC METHODS

    }
}
