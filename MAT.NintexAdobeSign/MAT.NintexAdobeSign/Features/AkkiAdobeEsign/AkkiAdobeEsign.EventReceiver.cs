using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.Xml;
using Nintex.Workflow;
using Nintex.Workflow.Common;
using Nintex.Workflow.Administration;
using Microsoft.SharePoint.Utilities;
using System.Collections.ObjectModel;
using System.IO;


namespace Akki.NintexAdobeSign.Features.MatAdobeEsign
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("b53846b1-5a6c-4a77-b4f2-27b96b1ab662")]
    public class MatAdobeEsignEventReceiver : SPFeatureReceiver
    {
        
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            // Retrieve a reference to the parent web application for the feature.
            SPWebApplication parent = (SPWebApplication)properties.Feature.Parent;

            foreach (var file in GetAllNwaFilesNWADefinition())
            {

                // Retrieve the contents of the action definition file.
                XmlDocument nwaXml = GetNWADefinition(file);

                // Instantiate an ActivityReference object from the action definition file.
                ActivityReference newActivityReference = ActivityReference.ReadFromNWA(nwaXml);

                // Attempt to instantiate an ActivityReference object from the the workflow action adapter 
                // identified by the AdapterType and AdapterAssembly elements from the action definition file.
                // For new deployments, action is set to null; otherwise, the existing ActivityReference 
                // for the custom workflow action is retrieved.
                ActivityReference action = ActivityReferenceCollection.FindByAdapter(
                    newActivityReference.AdapterType, newActivityReference.AdapterAssembly);

                // If the custom workflow action has been previously deployed, 
                // update the ActivityReference for the custom action; otherwise, 
                // add a new Activityreference for the custom action and then
                // instantiate it. 
                if (action != null)
                {
                    // Update the ActivityReference for the custom workflow action.
                    ActivityReferenceCollection.UpdateActivity(
                        action.ActivityId,
                        newActivityReference.Name,
                        newActivityReference.Description,
                        newActivityReference.Category,
                        newActivityReference.ActivityAssembly,
                        newActivityReference.ActivityType,
                        newActivityReference.AdapterAssembly,
                        newActivityReference.AdapterType,
                        newActivityReference.HandlerUrl,
                        newActivityReference.ConfigPage,
                        newActivityReference.RenderBehaviour,
                        newActivityReference.Icon,
                        newActivityReference.ToolboxIcon,
                        newActivityReference.WarningIcon,
                        newActivityReference.QuickAccess,
                        newActivityReference.ListTypeFilter);
                }
                else
                {
                    // Add a new ActivityReference for the custom workflow action.
                    ActivityReferenceCollection.AddActivity(
                        newActivityReference.Name,
                        newActivityReference.Description,
                        newActivityReference.Category,
                        newActivityReference.ActivityAssembly,
                        newActivityReference.ActivityType,
                        newActivityReference.AdapterAssembly,
                        newActivityReference.AdapterType,
                        newActivityReference.HandlerUrl,
                        newActivityReference.ConfigPage,
                        newActivityReference.RenderBehaviour,
                        newActivityReference.Icon,
                        newActivityReference.ToolboxIcon,
                        newActivityReference.WarningIcon,
                        newActivityReference.QuickAccess,
                        newActivityReference.ListTypeFilter);

                    // Instantiate the newly-added ActivityReference.
                    action = ActivityReferenceCollection.FindByAdapter(
                        newActivityReference.AdapterType, newActivityReference.AdapterAssembly);
                }


                // Add a modification to the web.config file for the web application, to install the 
                // custom workflow activity to the collection of authorized activity types for the 
                // web application.
                string activityTypeName = string.Empty;
                string activityNamespace = string.Empty;

                // Extract the type name and namespace name from the value of the ActivityType property.
                Utility.ExtractNamespaceAndClassName(action.ActivityType, out activityTypeName, out activityNamespace);

                // Add the assembly, namespace, and type of the workflow activity to the collection of 
                // authorized activity types for the web application.
               
                // Activate the custom workflow action. 
                ActivityActivationReference reference = new ActivityActivationReference(
                    action.ActivityId, Guid.Empty, Guid.Empty);
                reference.AddOrUpdateActivationReference();
            }
            
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            // Retrieve a reference to the parent web application for the feature.
            SPWebApplication parent = (SPWebApplication)properties.Feature.Parent;

            foreach (var file in GetAllNwaFilesNWADefinition())
            {

                XmlDocument nwaXml = GetNWADefinition(file);
                ActivityReference action = ActivityReference.ReadFromNWA(nwaXml);
                if (action != null)
                {
                    // If the feature is not activated in any other web application, remove the action 
                    // definition from the configuration database.
                    if (!IsFeatureActivatedInAnyWebApp(parent, properties.Definition.Id))
                        ActivityReferenceCollection.RemoveAction(action.ActivityId);

                    // Remove the modification from the web.config file for the web application, to uninstall the 
                    // custom workflow activity from the collection of authorized activity types for the 
                    // web application.
                    string activityTypeName = string.Empty;
                    string activityNamespace = string.Empty;

                    // Extract the type name and namespace name from the value of the ActivityType property.
                    Utility.ExtractNamespaceAndClassName(action.ActivityType, out activityTypeName, out activityNamespace);

                    // Identify and remove the modification from the web.config file.
                    Collection<SPWebConfigModification> modifications = parent.WebConfigModifications;
                    foreach (SPWebConfigModification modification in modifications)
                    {
                        // If the modification was added by Nintex Workflow, compare the assembly, namespace, and type of the workflow activity to the collection of 
                        // authorized activity types in the modification. If they match, remove the modification.
                        // NOTE: AuthorizedTypes.OWNER_TOKEN is the owner token for any modification added by 
                        // Nintex Workflow.
                        if (modification.Owner == AuthorisedTypes.OWNER_TOKEN)
                        {
                            if (IsAuthorizedTypeMatch(modification.Value, action.ActivityAssembly, activityTypeName, activityNamespace))
                            {
                                // Remove the modification.
                                modifications.Remove(modification);
                                // Apply the updated modifications to the SharePoint farm containing the web application.
                                parent.Farm.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
                                break;
                            }
                        }
                    }
                }
            }
        }

        // Uncomment the method below to handle the event raised after a feature has been installed.

        public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        {
        }


        // Uncomment the method below to handle the event raised before a feature is uninstalled.

        //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        //{
        //}

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}

        private XmlDocument GetNWADefinition(string filePath)
        {
                XmlDocument nwaXml = new XmlDocument();
                nwaXml.Load(filePath);            
                return nwaXml;            
        }

        private string[] GetAllNwaFilesNWADefinition()
        {
            
            string path = SPUtility.GetVersionedGenericSetupPath(Constants.PathToNWA, Constants.SharePointVersion);
            return Directory.GetFiles(path);
        }

        private bool IsAuthorizedTypeMatch(string modification, string activityAssembly, string activityType, string activityNamespace)
        {
            // Indicates whether the specified type, namespace, and assembly match for the authorized type
            // in the specified web config modification. 

            // Load the web.config modification as an XML document.
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(modification);

            // If the XML document contains an authorizedType element, compare the values of the 
            // TypeName, Namespace, and Assembly attributes against the specified 
            // type, namespace, and assembly for the workflow activity. If authorizedType does
            // not exist, or if the attribute values do not match the specified values, return
            // false; otherwise, return true.
            if (doc.FirstChild.Name == "authorizedType")
            {
                return (doc.SelectSingleNode("//@TypeName").Value == activityType
                        && doc.SelectSingleNode("//@Namespace").Value == activityNamespace
                        && doc.SelectSingleNode("//@Assembly").Value == activityAssembly);

            }

            return false;
        }

        private bool IsFeatureActivatedInAnyWebApp(SPWebApplication thisWebApplication, Guid thisFeatureId)
        {

            // Indicates whether the feature is activated for any other web application in the 
            // SharePoint farm.

            // Attempt to access the Web service associated with the content application.
            SPWebService webService = SPWebService.ContentService;
            if (webService == null)
                throw new ApplicationException("Cannot access the Web service associated with the content application.");

            // Iterate through the collection of web applications. If this feature is 
            // activated for any web application other than the current web application,
            // return true; otherwise, return false.
            SPWebApplicationCollection webApps = webService.WebApplications;
            foreach (SPWebApplication webApp in webApps)
            {
                if (webApp != thisWebApplication)
                    if (webApp.Features[thisFeatureId] != null)
                        return true;
            }

            return false;
        }
    }
}
