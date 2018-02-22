
using System;
using System.Text;
using System.Linq;
using System.Workflow.ComponentModel;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;
using Microsoft.SharePoint.WorkflowActions;
using Nintex.Workflow;
using Nintex.Workflow.Activities;
using Akki.AdobeSign.Common;


namespace Akki.NintexAdobeSign
{

    public class UploadItemToSignActivity : ProgressTrackingActivity
    {
       


        public const string KeyApproversPropertyName = "Approvers";
        public const string KeyUrlMiscInfoPropertyName = "UrlMiscInfo";
        public const string KeyOutAdobeAgreementIDProperty = "OutAdobeAgreementID";

        #region Standard dependency properties
        public static DependencyProperty __ListItemProperty = DependencyProperty.Register("__ListItem", typeof(SPItemKey), typeof(UploadItemToSignActivity));
        public static DependencyProperty __ContextProperty = DependencyProperty.Register("__Context", typeof(WorkflowContext), typeof(UploadItemToSignActivity));
        public static DependencyProperty __ListIdProperty = DependencyProperty.Register("__ListId", typeof(string), typeof(UploadItemToSignActivity));

        //custom activities
        public static DependencyProperty ApproversProperty = DependencyProperty.Register(KeyApproversPropertyName, typeof(string), typeof(UploadItemToSignActivity));
        public static DependencyProperty UrlMiscInfoProperty = DependencyProperty.Register(KeyUrlMiscInfoPropertyName, typeof(string), typeof(UploadItemToSignActivity));
        public static DependencyProperty OutAdobeAgreementIDProperty = DependencyProperty.Register(KeyOutAdobeAgreementIDProperty, typeof(string), typeof(UploadItemToSignActivity));

               

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
            get { return (string)base.GetValue(UploadItemToSignActivity.ApproversProperty); }
            set { base.SetValue(UploadItemToSignActivity.ApproversProperty, value); }
        }
        public string UrlMiscInfo
        {
            get { return (string)base.GetValue(UploadItemToSignActivity.UrlMiscInfoProperty); }
            set { base.SetValue(UploadItemToSignActivity.UrlMiscInfoProperty, value); }
        }

        public string OutAdobeAgreementID
        {
            get { return (string)base.GetValue(UploadItemToSignActivity.OutAdobeAgreementIDProperty); }
            set { base.SetValue(UploadItemToSignActivity.OutAdobeAgreementIDProperty, value); }
        }

        #endregion

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {           
            
            ActivityActivationReference.IsAllowed(this, __Context.Web);
            // Get the workflow context for the workflow activity.
            NWWorkflowContext ctx = NWWorkflowContext.GetContext( __Context,  new Guid(__ListId), __ListItem.Id,  WorkflowInstanceId, this);            
            base.LogProgressStart(ctx);

            string approverNames = ctx.AddContextDataToString(this.Approvers);
            string qryUrl = ctx.AddContextDataToString(this.UrlMiscInfo);
            string[] approvers = Akki.AdobeSign.Common.Utilities.GetUserEmails(approverNames, ctx.Web.Url).ToArray();

            try
            {
                SPList spList = this.__Context.Web.Lists[new Guid(this.__ListId)];
                SPListItem spItm = spList.GetItemById(this.__ListItem.Id);
                var agreeementName = string.Format(Constants.MsgListItemAgreementName, spItm.ID, spList.Title);
                SPView listView = spList.Views[Constants.AkkiItemViewTypeAdobe];
                if (listView == null)
                    throw new ApplicationException(Constants.ErrNoViewFound);

                SPViewFieldCollection viewfieldCollection = listView.ViewFields;
                StringBuilder buff = new StringBuilder(16);
                buff.Append(Constants.HeaderTextInfo);
                foreach (string viewFieldName in viewfieldCollection)
                {
                    buff.Append(Environment.NewLine);
                    buff.Append(Environment.NewLine);
                    var val = spItm[viewFieldName];
                    buff.Append(string.Format("{0}:{1}", viewFieldName, val));
                    buff.Append(Environment.NewLine);
                    buff.Append(Environment.NewLine);
                }
                
                var agreementId = AdobeOperations.SendDocumentByBytesForSigninig(Encoding.ASCII.GetBytes(buff.ToString()),null, agreeementName, approvers, null, qryUrl).agreementId;
                this.OutAdobeAgreementID = agreementId;

                // if no problem 
                try
                {
                    Utilities.SetDefaultColValues(spItm, agreementId, AgreementStatus.OUT_FOR_SIGNATURE);
                }
                catch { }
            }
            catch (Exception ex)
            {
                //log.Error("[Execute]", ex);
                throw;
            }

            base.LogProgressEnd(ctx, executionContext);           
            return ActivityExecutionStatus.Closed;
        }
        
        protected override ActivityExecutionStatus HandleFault(ActivityExecutionContext executionContext,  Exception exception)
        {
            // TODO: Provide activity-specific text introducing the exception.
            // log4nET ERROR 
            string activityErrorIntro = "Error in UploadItemToSignActivity " + exception.ToString(); 

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
