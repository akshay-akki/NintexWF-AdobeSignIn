using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akki.NintexAdobeSign
{
  internal class Constants
    {
        internal const string PathToNWA = "template\\features\\Akki.NintexAdobeSign_AkkiAdobeEsign\\UploadDoc\\";
        internal const int    SharePointVersion = 15;
        internal const string AdapterType = "Akki.NintexAdobeSign.NintexAdobeSignAdapter";
        internal const string Assembly = "Akki.NintexAdobeSign, Version=1.0.0.0, Culture=neutral, PublicKeyToken=f46d01208e388925";
        internal const string PdfExtension = ".pdf";

        internal const string MsgAgreemntID = "The adobe sign in agreement id is {0}";
        internal const string MsgDocmuentUploadedSuccessfully = "Signed document downlaod from Adobe and adddd to the list successfully";
        internal const string MsgListItemAgreementName = "Document ID {0} list {1}";

        internal const string ErrNoAgreementID = "Agreement Id canot be blank";
        internal const string ErrNoAgreementFound = "No Agreement returned by Adobe for the agreemend ID {0}";
        internal const string ErrNoAgreememtID = "Agreemtn ID can not be blank.";
        internal const string ErrNoViewFound = "The list does not contain the view by name  "+ AkkiItemViewTypeAdobe;

        internal const char   Delimiter = ',';
        internal const string AkkiItemViewTypeAdobe = "AKKI_ADOBE_VIEW_SIGN";
        internal const string HeaderTextInfo = "Column Name : Column Value";

        internal const string ColAgreementID = "AkkiAdobeAgreementID";
        internal const string ColSignedStatus = "AkkiAdobeDocSignedStatus";


        //for the azure functions to read the list item etc 
        internal const string WEB_URL = " WEB_URL";
        internal const string LIST_TITLE = "LIST_TITLE";
        internal const string ITEM_ID = "ITEM_ID";
    }
}
