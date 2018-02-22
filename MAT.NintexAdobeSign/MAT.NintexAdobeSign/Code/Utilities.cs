using System;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Akki.AdobeSign.Common;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;


namespace Akki.NintexAdobeSign
{
    public class Utilities
    {
        internal static void SetDefaultColValues(SPListItem spItm, string agreementId, AgreementStatus status)
        {
            spItm[Constants.ColSignedStatus] = status.ToString();
            spItm.Update();           

            try
            {
                spItm[Constants.ColAgreementID] = agreementId;
                spItm.Update();
            }
            catch {
                // do nothign as it might not exist 
            }
        }

        internal static void AddESignStatusColumn(SPList  spList )
        {
            if (!spList.Fields.ContainsField(Constants.ColSignedStatus))
            {
                spList.Fields.Add(Constants.ColSignedStatus, SPFieldType.Choice, false);
                SPFieldChoice chFldPurpose = (SPFieldChoice)spList.Fields[Constants.ColSignedStatus];
                chFldPurpose.EditFormat = SPChoiceFormatType.Dropdown;
                var values = Enum.GetValues(typeof(AgreementStatus));
                foreach (var s in values)
                    chFldPurpose.Choices.Add(s.ToString());

                chFldPurpose.Update();                
            }  

            if(!spList.Fields.ContainsField(Constants.ColAgreementID))
            {
                spList.Fields.Add(Constants.ColAgreementID, SPFieldType.Text, false);
                
            }

            spList.Update();
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

    }
}

