
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;
using System.Workflow.ComponentModel;
using Microsoft.SharePoint.WorkflowActions;
using Nintex.Workflow;
using Nintex.Workflow.Activities;
using Akki.AdobeSign.Common;



namespace Akki.NintexAdobeSign
{
   
    public class UploadActivity : ProgressTrackingActivity
    {
        

        public const string KeyAgreementName = "AgreementName";
        public const string KeyApproversPropertyName = "Approvers";
        public const string KeyUrlMiscInfoPropertyName = "UrlMiscInfo";
        public const string KeyCompendiumDocsID = "CompendiumDocsID";
        public const string KeyOutAdobeAgreementIDProperty = "OutAdobeAgreementID";


        #region Standard dependency properties
        public static DependencyProperty __ListItemProperty =    DependencyProperty.Register("__ListItem", typeof(SPItemKey), typeof(UploadActivity));
        public static DependencyProperty __ContextProperty =     DependencyProperty.Register("__Context", typeof(WorkflowContext), typeof(UploadActivity));
        public static DependencyProperty __ListIdProperty =      DependencyProperty.Register("__ListId", typeof(string), typeof(UploadActivity));

        //custom activities
        public static DependencyProperty ApproversProperty = DependencyProperty.Register(KeyApproversPropertyName, typeof(string), typeof(UploadActivity));
        public static DependencyProperty CompendiumDocsIDProperty = DependencyProperty.Register(KeyCompendiumDocsID, typeof(string), typeof(UploadActivity));
        public static DependencyProperty AgreementNameProperty = DependencyProperty.Register(KeyAgreementName, typeof(string), typeof(UploadActivity));
        public static DependencyProperty UrlMiscInfoProperty = DependencyProperty.Register(KeyUrlMiscInfoPropertyName, typeof(string), typeof(UploadActivity));


        // this the code should do automatically ????. but the signed propeorty should be yes or not
        public static DependencyProperty OutAdobeAgreementIDProperty = DependencyProperty.Register(KeyOutAdobeAgreementIDProperty, typeof(string), typeof(UploadActivity));

        public SPItemKey __ListItem
        {
            get { return (SPItemKey)base.GetValue(__ListItemProperty); }
            set { SetValue(__ListItemProperty, value); }
        }
        public WorkflowContext __Context
        {
            get { return (WorkflowContext)base.GetValue(__ContextProperty); }
            set { SetValue(__ContextProperty, value); }
        }
        public string __ListId
        {
            get { return (string)base.GetValue(__ListIdProperty); }
            set { SetValue(__ListIdProperty, value); }
        }

        public string Approvers
        {
            get { return (string)base.GetValue(UploadActivity.ApproversProperty); }
            set { base.SetValue(UploadActivity.ApproversProperty, value); }
        }
        public string AgreementName
        {
            get { return (string)base.GetValue(UploadActivity.AgreementNameProperty); }
            set { base.SetValue(UploadActivity.AgreementNameProperty, value); }
        }
        public string OutAdobeAgreementID
        {
            get { return (string)base.GetValue(UploadActivity.OutAdobeAgreementIDProperty); }
            set { base.SetValue(UploadActivity.OutAdobeAgreementIDProperty, value); }
        }
        public string UrlMiscInfo
        {
            get { return (string)base.GetValue(UploadActivity.UrlMiscInfoProperty); }
            set { base.SetValue(UploadActivity.UrlMiscInfoProperty, value); }
        }
        public string CompendiumDocsID
        {
            get { return (string)base.GetValue(UploadActivity.CompendiumDocsIDProperty); }
            set { base.SetValue(UploadActivity.CompendiumDocsIDProperty, value); }
        }
        
        #endregion

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
           
            ActivityActivationReference.IsAllowed(this, __Context.Web);            
            // Get the workflow context for the workflow activity.
            NWWorkflowContext ctx = NWWorkflowContext.GetContext( __Context,  new Guid(__ListId), __ListItem.Id,  WorkflowInstanceId, this);            
            base.LogProgressStart(ctx);
            
            
            //BLOG. this function 
            // get all the properties you need here
            string aggreementName = ctx.AddContextDataToString(this.AgreementName);            
            string approversProp = ctx.AddContextDataToString(this.Approvers);
            string qryUrl = ctx.AddContextDataToString(this.UrlMiscInfo);
            string compDocsIds = ctx.AddContextDataToString(this.CompendiumDocsID);

            try
            {
                
                SPList spList = this.__Context.Web.Lists[new Guid(this.__ListId)];                
                SPListItem spItm = spList.GetItemById(this.__ListItem.Id);                
                aggreementName = string.IsNullOrEmpty(aggreementName) ? spList.Title + "-" + spItm.ID + "-" + spItm.File.Name : aggreementName;

                List<string> lstApprovers = Akki.AdobeSign.Common.Utilities.GetUserEmails(approversProp, ctx.Web.Url);
                this.HistoryListMessage = "Initiator Email " + ctx.WorkflowInitiator.Email;
                // get the bytes for all the compendium documents
                List<byte[]> comDocBytes = null;
                if ( !string.IsNullOrEmpty(compDocsIds))
                {
                    comDocBytes = new List<byte[]>();
                    var docsID = compDocsIds.Split(Constants.Delimiter);                    
                    foreach(var doc in docsID)
                    {
                        int docId  =  int.Parse(doc);
                        var spItmCompDoc = spList.GetItemById(docId);
                        comDocBytes.Add(spItmCompDoc.File.OpenBinary());
                    }
                }
                var response = AdobeOperations.SendDocumentByBytesForSigninig(spItm.File.OpenBinary(), comDocBytes, aggreementName, lstApprovers.ToArray(), ctx.WorkflowInitiator.Email, qryUrl);               
                                                              
                this.HistoryListMessage = string.Format(Constants.MsgAgreemntID, response.agreementId);                
                this.OutAdobeAgreementID = response.agreementId;

            }
            catch (Exception e)
            {
                //log.Error("[Execute]:", e);
                throw;
            }
            base.LogProgressEnd(ctx, executionContext);            
            return ActivityExecutionStatus.Closed;
            
        }


        protected override ActivityExecutionStatus HandleFault(ActivityExecutionContext executionContext,  Exception exception)
        {
            // TODO: Provide activity-specific text introducing the exception.
            string activityErrorIntro = "Error in SendDocForAuthoring " + exception.ToString(); 
           
            Nintex.Workflow.Diagnostics.ActivityErrorHandler.HandleFault(
                executionContext,
                exception,
                this.WorkflowInstanceId,
                activityErrorIntro,
                __ListItem.Id,
                __ListId,
                __Context);

            return base.HandleFault(executionContext, exception);
        }
    }
}
