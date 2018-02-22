using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Akki.AdobeSign.Common
{
    public class AdobeOperations
    {

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <param name="lstCompendiumFiles">array of the files which should be part of agreement as compendium</param>
        /// <param name="agreementName"></param>
        /// <param name="arrSignerEmail"></param>
        /// <param name="messageInitiatorEmail">Email address of the person who is making the adobe call. If left empty the call will be made on behalf os admin of adobe account</param>
        /// <returns></returns>
        public static async Task<AgreementCreationResponse> SendDocumentForAgreemntByFilePath(string fullFilePath, List<string> lstCompendiumFiles, string agreementName, string[] arrSignerEmail, string messageInitiatorEmail, string miscInfo)
        {
            
               //Create trasient document
                var fileData = System.IO.File.ReadAllBytes(fullFilePath);
                return SendDocumentByBytesForSigninig(fileData,null,agreementName, arrSignerEmail, messageInitiatorEmail, miscInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileDataInBytes"></param>
        /// <param name="agreementName"></param>
        /// <param name="arrSignerEmail"></param>
        /// <param name="messageInitiatorEmail">Email addres of the person who is making the adobe call. If left empty the call will be made on behalf os admin of adobe account</param>
        /// <param name="urlMiscInfo">anything you want to add as part of the request to adobe esign as query string parameter. The same will be sent to the App function</param>
        /// <returns></returns>
        public static AgreementCreationResponse SendDocumentByBytesForSigninig(byte[] fileDataInBytes, List<byte[]> lstCompendiumFiles, string agreementName, string[] arrSignerEmail, 
            string messageInitiatorEmail, string urlMiscInfo)
        {
            try
            {
                AdobeObject obj = new AdobeObject();

                var transientDocumentResponse = obj.AddDocument(agreementName, fileDataInBytes, messageInitiatorEmail).Result;
                var creationInfo = GetAgreementCreationInfoObject(transientDocumentResponse.transientDocumentId, lstCompendiumFiles, agreementName, arrSignerEmail, messageInitiatorEmail, urlMiscInfo);
               // creationInfo.options.authoringRequested = true;
                var agreementCreationResponse = obj.CreateAgreement(creationInfo, messageInitiatorEmail).Result;
                return agreementCreationResponse;
            }
            catch(Exception e)            
            {
                //log.Error("[SendDocumentByBytesForSigninig]", e);
                throw;
            }
        }

        public static AgreementCreationResponse SendDocumentByBytesForAuthoring(byte[] fileDataInBytes, List<byte[]> lstCompendiumFiles, string agreementName, string[] arrSignerEmail,
            string messageInitiatorEmail, string urlMiscInfo)
        {
            try
            {
                AdobeObject obj = new AdobeObject();

                var transientDocumentResponse = obj.AddDocument(agreementName, fileDataInBytes, messageInitiatorEmail).Result;
                var creationInfo = GetAgreementCreationInfoObject(transientDocumentResponse.transientDocumentId, lstCompendiumFiles, agreementName, arrSignerEmail, messageInitiatorEmail, urlMiscInfo);
                creationInfo.options = new Options();
                creationInfo.options.authoringRequested = true;
                var agreementCreationResponse = obj.CreateAgreement(creationInfo, messageInitiatorEmail).Result;
                return agreementCreationResponse;
            }
            catch (Exception e)
            {
                //log.Error("[SendDocumentByBytesForSigninig]", e);
                throw;
            }
        }

        public static AgreementInfo  GetAgreementStatus(string agreementID)
        {
            try
            {
                AdobeObject obj = new AdobeObject();
                return obj.GetAgreement(agreementID).Result;
            }
            catch(Exception e)
            {
                //log.Error($"[GetAgreementStatus], Agreemed id ={agreementID}", e);
                throw;
            }
        }

        public static byte[] GetSignedDocument ( string agreementID)
        {
            try
            {
                // make sure the agreement is signed. if not throw error 
                var agreement = GetAgreementStatus(agreementID);
                if (agreement.status != AgreementStatus.SIGNED)
                    throw new ApplicationException(string.Format(Constants.ErrAgreementNotSigned, agreementID));

                AdobeObject obj = new AdobeObject();
                return obj.GetAgreementCombinedDocument(agreementID).Result;
            }
            catch (Exception e)
            {
                //log.Error($"[GetSignedDocument], Agreemed id ={agreementID}", e);
                throw;
            }
        }

        public static AgreementCreationInfo GetAgreementCreationInfoObject(string transientDocumentId, List<byte[]> lstCompendiumFiles, string agreementName, string[] arrSignerEmail,
           string messageInitiatorEmail, string urlMiscInfo)
        {
            try
            {
                
                AgreementCreationInfo creationInfo = new AgreementCreationInfo();
                creationInfo.documentCreationInfo = new DocumentCreationInfo();

                //Document Creation Info
                var documentCreationInfo = creationInfo.documentCreationInfo;
                documentCreationInfo.name = agreementName;
                documentCreationInfo.signatureType = SignatureTypeEnum.ESIGN;

                string qryUrl = urlMiscInfo;
                if (!string.IsNullOrEmpty(qryUrl) && !urlMiscInfo.StartsWith("&"))
                    qryUrl = "&" + qryUrl;

                // TODO: read the url and code from a confir property
                //documentCreationInfo.callbackInfo = "https://akkiAdobeesignazurefx.azurewebsites.net/api/AkkiAdobeEsignAzureFx?code=5RQkWJkgnTOkOxBYo9PnKq8ZZLqr01dP3nRNuyLdGglD343Tj1kmHQ==" + qryUrl;
                documentCreationInfo.callbackInfo = AdobeRestAPI.AzureFxUrlWithCode + qryUrl;

                //FileInfo
                documentCreationInfo.fileInfos = new List<FileInfo>();
                var fileInfos = documentCreationInfo.fileInfos;
                FileInfo fileInfo = new FileInfo(transientDocumentId);
                fileInfos.Add(fileInfo);

                if (lstCompendiumFiles != null)
                {
                    foreach (var compFileByte in lstCompendiumFiles)
                    {
                        AdobeObject compFile = new AdobeObject();
                        var response = compFile.AddDocument(agreementName, compFileByte, messageInitiatorEmail).Result;
                        documentCreationInfo.fileInfos.Add(new FileInfo { transientDocumentId = response.transientDocumentId });
                    }
                }

                documentCreationInfo.signatureFlow = SignatureFlow.PARALLEL;
                documentCreationInfo.recipientSetInfos = new List<RecipientSetInfo>();

                // if there is only one signer and that signer  is the person who is running the worflow then use different flow and no recipient set into
                if (arrSignerEmail.Length == 1 && arrSignerEmail.Contains(messageInitiatorEmail))
                {
                    //  documentCreationInfo.signatureFlow = SignatureFlow.FILL_SIGN;
                    RecipientSetInfo recipientSetInfo = new RecipientSetInfo();               
                    recipientSetInfo.recipientSetRole = RecipientRoleEnum.SIGNER;

                    RecipientSetMemberInfo setMemberInfo = new RecipientSetMemberInfo();
                    setMemberInfo.email = messageInitiatorEmail;
                    RecipientSetMemberInfo setMemberInfoSys = new RecipientSetMemberInfo();
                    setMemberInfoSys.email = AdobeRestAPI.FarmSysEmailAcc;

                    recipientSetInfo.recipientSetMemberInfos.Add(setMemberInfo);
                    recipientSetInfo.recipientSetMemberInfos.Add(setMemberInfoSys);
                    documentCreationInfo.recipientSetInfos.Add(recipientSetInfo);

                }
                else
                {                  

                    foreach (var signerEmail in arrSignerEmail)
                    {
                        RecipientSetInfo recipientSetInfo = new RecipientSetInfo();
                        recipientSetInfo.recipientSetRole = RecipientRoleEnum.SIGNER;
                        RecipientSetMemberInfo setMemberInfo = new RecipientSetMemberInfo();
                        setMemberInfo.email = signerEmail;
                        recipientSetInfo.recipientSetMemberInfos.Add(setMemberInfo);

                        documentCreationInfo.recipientSetInfos.Add(recipientSetInfo);
                    }

                }
                return creationInfo;
                
            }
            catch (Exception e)
            {
                //log.Error("[GetAgreementCreationInfoObject]", e);
                throw;
            }
        }
    }
}
