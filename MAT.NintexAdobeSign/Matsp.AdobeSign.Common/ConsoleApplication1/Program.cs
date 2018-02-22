using System;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.DirectoryServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Akki.AdobeSign.Common;

using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration.Claims;
using Microsoft.SharePoint.Utilities;
using System.IO;
using System.Xml;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {

             Utilities.GetUserEmails("test5@Akki.be,i:0#.w|testsharepoint\\test5;Akshay Owners;that is", "http://akshay/sites/test/");
            
            // UploadMultipleDocs();
            // UploadListItem();  



            //string s =  GetSerialisedString("3AAABLblqZhCMa3FBIxX6F_aQsyi-KcY_eVYAgYHJFEmzy-_Et8vPIEyLCgRJLiVzCUw_K8lpRxH51zfEHTmVYk2ZqR98ushf");
            // Console.WriteLine(s);


            //string claim = "i:0#.w|testsharepoint\testperf5";
            //string claim = "testsharepoint\testperf5";
            //SPClaimProviderManager mgr = SPClaimProviderManager.Local;
            //var userName = mgr.DecodeClaim(claim).Value;
            
            //UploadListItem();
            //GetUser();

            //using ( SPSite site = new SPSite("http://akshay/sites/HR/"))
            //{
            //    using (SPWeb spWb = site.OpenWeb())
            //    {
            //        SPList spList = spWb.Lists["testWf"];
            //        spList.Fields.Add("YesNo", SPFieldType.Boolean, true);
            //        spList.Update();
            //        int g = 23;
            //    }

            //}
            //DownLoadSignedCopy();
        }

        public static string GetSerialisedString(string agreementID)
        {
            AgreementInfo agreementInfo = AdobeOperations.GetAgreementStatus(agreementID);            
            string ss = SerializeXml<List<DocumentHistoryEvent>>(agreementInfo.events);
            return ss;
        }

        internal static string SerializeXml<T>(T objectToSerialize)
        {

            XmlSerializer xmlSerializer = new XmlSerializer(objectToSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, objectToSerialize);
                return textWriter.ToString();
            }

        }


        public static void UploadListItem()
        {
            using (SPSite spSite = new SPSite("http://leumtstspapp1/sites/staff/HR/"))
            {
                SPWeb spWeb = spSite.OpenWeb();
                SPList spList = spWeb.Lists["testWf"];
                SPListItem spItm = spList.Items[0];
                string[] arrApp = new string[]{"testrf5@Akki.be"};
                SPView listView = spList.Views["MAT_ADOBE_SIGN_VIEW"];
                if (listView == null)
                    throw new ApplicationException("The list does not contain the view by name  MAT_ADOBE_SIGN_VIEW");

                SPViewFieldCollection viewfieldCollection = listView.ViewFields;
                StringBuilder buff = new StringBuilder(16);
                buff.Append("Column Name : Column Value");
                foreach (string viewFieldName in viewfieldCollection)
                {
                    var val = spItm[viewFieldName];
                    buff.Append(string.Format("{0}:{1}", viewFieldName,val));
                    buff.Append(Environment.NewLine);
                    buff.Append(Environment.NewLine);
                }
                string miscInfo = "WEbUrl=" + spWeb.Url + "&listTitle=" + spList.Title + "&ItemId=" + spItm.ID;
                // now send it to signature 
                AdobeOperations.SendDocumentByBytesForSigninig(Encoding.ASCII.GetBytes(buff.ToString()),null, spItm["ID"].ToString(), arrApp, null, miscInfo);
            }
        }
        

        public static void DownLoadSignedCopy()
        {
            var agreementId ="3AAABLblqZhDMmc8hXR-PM8jUhTkq0C_muHzbsDdoylsPG8iCmPIHMZOhHMSTHEpB8mexkQWI2QUW1XmTVkODnnhCHMVSx20J";
            var bytes = AdobeOperations.GetSignedDocument(agreementId);            
            //File.WriteAllBytes(string path, byte[] bytes)
                   
        }
        

        public static void RunUploadTest()
        {
            //string nwaFile = "template\\features\\Akki.NintexAdobeSign_MatAdobeEsign\\UploadDoc";
            //string path = SPUtility.GetVersionedGenericSetupPath(nwaFile, 15);
            //// read all files in the 
            //foreach (var filePath in Directory.GetFiles(path))
            //{                
            //    XmlDocument nwaXml = new XmlDocument();
            //    nwaXml.Load(filePath);

            //    int s = 23;
            //}

           

            //AgreementInfo a = AdobeOperations.GetAgreementStatus("3AAABLblqZhANgtEysCEsNARcRBWQOlOyovy6-SSu7lb8-XnOLRJDyYYVuDAe78Mh2tDrjqVB5NSdXDkXA5-LOjbWFHbhkBwE");
            //var linqPendingApproval = a.nextParticipantSetInfos.SelectMany(npSInfo => npSInfo.nextParticipantSetMemberInfos).ToList().Select(e=>e.email).ToArray();

            //string appNames = string.Join(";", linqPendingApproval);
            //int f = 23;

            //using (SPSite spSite = new SPSite("http://akshay/sites/HR/"))
            //{
            //    SPWeb spWeb = spSite.OpenWeb();
            //    SPList spList = spWeb.Lists["adobeTestDoc"];
            //    SPListItem spItm = spList.Items[0];
            //    string[] arr = new string[]{"testperf5@Akki.be","testperf1@Akki.be"};
            //  //  string[] arr = new string[] { "testperf5@Akki.be"};
            //    // var v = AdobeOperations.SendDocumentByBytesForSigninig(spItm.File.OpenBinary(), "ConsoleApp", arr);

            //    AgreementInfo a = AdobeOperations.GetAgreementStatus("3AAABLblqZhCbjKCWp581rjZ2UaDOJB7WZLJaCzIoElADXeIaG3PIeJXCg4d3N9QLLT0Sz3dOMzKvcqqn-WQKj2TSE3kVYnll");
            //    string status = a.status;
            //    string pendingApproval = string.Empty;
                 
            //    foreach( var v in a.nextParticipantSetInfos)
            //    {
            //        foreach ( var b in v.nextParticipantSetMemberInfos)
            //        {
            //            pendingApproval += ";"+ b.email;
                        
            //        }
            //    }

            //    var linqPendingApproval = a.nextParticipantSetInfos;
            //    int f =23;
            //}
        }


        public static string RunV()
        {
            string intKey = "3AlqZhCJbhJb-AMG0scMN6gmef-dWM6gnGd3DQZcscF_YHPGIBOW9GaeJhaSHtoLqxASbGV1_Om8KXLC2acIZ1PwatLJ";
            var client = "https://api.eu1.echosign.com/api/rest/v5";

            string[] arr = new string[] { "tperf5@Akki.be", "tperf1@Akki.be" };
            var v = AdobeOperations.SendDocumentForAgreemntByFilePath(@"E:\backup\TestingAkkiDoc.docx",null, "agg", arr,null,null).Result;
            var ss = v.agreementId;
            return ss;

        }

        public static string UploadMultipleDocs()
        {
            string filename = @"E:\Backup\C-MED-17-55 Complaint Form.docx";
            string file1 = @"E:\Backup\Log.log";
            string file2 = @"E:\Backup\new.txt";
            
            byte[] Mainfile = System.IO.File.ReadAllBytes(filename);
            List<byte[]> arrByte = new List<byte[]>();
            arrByte.Add(System.IO.File.ReadAllBytes(file1));
            arrByte.Add(System.IO.File.ReadAllBytes(file2));

            string[] arr1 = new string[] { "tesrf5@Akki.be" };

            var agreementId = AdobeOperations.SendDocumentByBytesForSigninig(Mainfile, arrByte, "Test Multiple Docs", arr1, null, null).agreementId;
            return agreementId;

        }


       
    }
}
