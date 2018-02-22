
using System;
using System.IO;
using System.Workflow.ComponentModel;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;
using Microsoft.SharePoint.WorkflowActions;
using Nintex.Workflow;
using Nintex.Workflow.Activities;
using Akki.AdobeSign.Common;


namespace Akki.NintexAdobeSign
{

    public class DownloadDocumentActivity : ProgressTrackingActivity
    {

       

        public const string KeyAgreementID = "AgreementID";
       // public const string KeyDownloadDocLocation = "DownloadDocLocation";
        public const string KeyDestWebUrl = "DestWebUrl";
        public const string KeyDestDocLib = "DestDocLib";
        public const string KeyDestDocName = "DestDocName";

        public const string KeyOutDocumentID = "OutDocumentID";
        


        #region Standard dependency properties
        public static DependencyProperty __ListItemProperty = DependencyProperty.Register("__ListItem", typeof(SPItemKey), typeof(DownloadDocumentActivity));
        public static DependencyProperty __ContextProperty = DependencyProperty.Register("__Context", typeof(WorkflowContext), typeof(DownloadDocumentActivity));
        public static DependencyProperty __ListIdProperty =      DependencyProperty.Register("__ListId", typeof(string), typeof(DownloadDocumentActivity));

        //custom activities
        public static DependencyProperty AgreementIDProperty = DependencyProperty.Register(KeyAgreementID, typeof(string), typeof(DownloadDocumentActivity));


        public static DependencyProperty DestWebUrlProperty = DependencyProperty.Register(KeyDestWebUrl, typeof(string), typeof(DownloadDocumentActivity));
        public static DependencyProperty DestDocLibProperty = DependencyProperty.Register(KeyDestDocLib, typeof(string), typeof(DownloadDocumentActivity));
        public static DependencyProperty DestDocNameProperty = DependencyProperty.Register(KeyDestDocName, typeof(string), typeof(DownloadDocumentActivity));
        public static DependencyProperty OutDocumentIDProperty = DependencyProperty.Register(KeyOutDocumentID, typeof(int), typeof(DownloadDocumentActivity));


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

        public string AgreementID
        {
            get { return (string)base.GetValue(DownloadDocumentActivity.AgreementIDProperty); }
            set { base.SetValue(DownloadDocumentActivity.AgreementIDProperty, value); }
        }
        public string DestSiteUrl
        {
            get { return (string)base.GetValue(DownloadDocumentActivity.DestWebUrlProperty); }
            set { base.SetValue(DownloadDocumentActivity.DestWebUrlProperty, value); }
        }
        public string DestDocName
        {
            get { return (string)base.GetValue(DownloadDocumentActivity.DestDocNameProperty); }
            set { base.SetValue(DownloadDocumentActivity.DestDocNameProperty, value); }
        }
        public string DestDocLib
        {
            get { return (string)base.GetValue(DownloadDocumentActivity.DestDocLibProperty); }
            set { base.SetValue(DownloadDocumentActivity.DestDocLibProperty, value); }
        }
        public int OutDocumentID
        {
            get { return (int)base.GetValue(DownloadDocumentActivity.OutDocumentIDProperty); }
            set { base.SetValue(DownloadDocumentActivity.OutDocumentIDProperty, value); }
        }

        #endregion

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {

            try
            {
                ActivityActivationReference.IsAllowed(this, __Context.Web);
                // Get the workflow context for the workflow activity.
                NWWorkflowContext ctx = NWWorkflowContext.GetContext(__Context, new Guid(__ListId), __ListItem.Id, WorkflowInstanceId, this);
                base.LogProgressStart(ctx);

                //BLOG. this function ttps://community.nintex.com/docs/DOC-1673-list-of-nintex-workflowonpremise-custom-actions
                // get all the properties you need here
                string agreementID = ctx.AddContextDataToString(this.AgreementID);

                var destsiteUrl = ctx.AddContextDataToString(this.DestSiteUrl);
                var destDocLib = ctx.AddContextDataToString(this.DestDocLib);
                var destDocName = ctx.AddContextDataToString(this.DestDocName);

                var spWeb = string.IsNullOrEmpty(destsiteUrl) ? this.__Context.Web : new SPSite(destsiteUrl).OpenWeb();
                var docLib = string.IsNullOrEmpty(destDocLib) ? spWeb.Lists[new Guid(this.__ListId)] : spWeb.Lists[destDocLib];
                destDocName = (string.IsNullOrEmpty(destDocName) ? this.__Context.ItemName : Path.GetFileNameWithoutExtension(destDocName)) + Constants.PdfExtension;
                string destFileUrl = (docLib.RootFolder.ServerRelativeUrl.EndsWith("/") ? docLib.RootFolder.ServerRelativeUrl : docLib.RootFolder.ServerRelativeUrl + "/") + destDocName;


                if (string.IsNullOrEmpty(agreementID))
                {
                    this.HistoryListMessage = Constants.ErrNoAgreememtID;

                }
                else
                {
                    var fileBytes = AdobeOperations.GetSignedDocument(agreementID);
                    //TODO...use the input parameter for overriding 
                    SPFile spFile = spWeb.Files.Add(destFileUrl, fileBytes, true);
                    spFile.Update();
                    //  Utilities.SetDefaultColValues(spFile.Item, agreementID, AgreementStatus.SIGNED);
                    this.OutDocumentID = spFile.Item.ID;

                    this.HistoryListMessage = Constants.MsgDocmuentUploadedSuccessfully;
                }

                base.LogProgressEnd(ctx, executionContext);
                return ActivityExecutionStatus.Closed;
            }
            catch(Exception e)
            {
                ///log.Error("[Execute]", e);
                throw;
            }
        }


        protected override ActivityExecutionStatus HandleFault(ActivityExecutionContext executionContext,  Exception exception)
        {
            // TODO: Provide activity-specific text introducing the exception.
            string activityErrorIntro = "Error in DownloadDocumentActivity " + exception.ToString(); 
           
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
