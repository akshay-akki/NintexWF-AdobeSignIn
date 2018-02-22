
using System;
using System.Collections.Generic;
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
   
    public class CheckAgreementStatusActivity : ProgressTrackingActivity
    {
       

        public const string KeyAgreementID = "AgreementID";        
        public const string KeyOutPendingApprovers = "PendingApprovers";
        public const string KeyOutStaus = "Status";
        public const string KeyOutEvents = "Events";


        #region Standard dependency properties
        public static DependencyProperty __ListItemProperty =    DependencyProperty.Register("__ListItem", typeof(SPItemKey), typeof(CheckAgreementStatusActivity));
        public static DependencyProperty __ContextProperty =     DependencyProperty.Register("__Context", typeof(WorkflowContext), typeof(CheckAgreementStatusActivity));
        public static DependencyProperty __ListIdProperty =      DependencyProperty.Register("__ListId", typeof(string), typeof(CheckAgreementStatusActivity));

        //custom activities
        public static DependencyProperty AgreementIDProperty =    DependencyProperty.Register(KeyAgreementID, typeof(string), typeof(CheckAgreementStatusActivity));
        public static DependencyProperty PendingApproversProperty = DependencyProperty.Register(KeyOutPendingApprovers, typeof(string), typeof(CheckAgreementStatusActivity));
        public static DependencyProperty StatusProperty = DependencyProperty.Register(KeyOutStaus, typeof(string), typeof(CheckAgreementStatusActivity));
        public static DependencyProperty EventsProperty = DependencyProperty.Register(KeyOutEvents, typeof(string), typeof(CheckAgreementStatusActivity));
        
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
            get { return (string)base.GetValue(CheckAgreementStatusActivity.AgreementIDProperty); }
            set { base.SetValue(CheckAgreementStatusActivity.AgreementIDProperty, value); }
        }

        public string Status
        {
            get { return (string)base.GetValue(CheckAgreementStatusActivity.StatusProperty); }
            set { base.SetValue(CheckAgreementStatusActivity.StatusProperty, value); }
        }

        public string PendingApprovers
        {
            get { return (string)base.GetValue(CheckAgreementStatusActivity.PendingApproversProperty); }
            set { base.SetValue(CheckAgreementStatusActivity.PendingApproversProperty, value); }
        }
        public string Events
        {
            get { return (string)base.GetValue(CheckAgreementStatusActivity.EventsProperty); }
            set { base.SetValue(CheckAgreementStatusActivity.EventsProperty, value); }
        }

       
        #endregion

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {           
            
            ActivityActivationReference.IsAllowed(this, __Context.Web);
            // Get the workflow context for the workflow activity.
            NWWorkflowContext ctx = NWWorkflowContext.GetContext( __Context,  new Guid(__ListId), __ListItem.Id,  WorkflowInstanceId, this);            
            base.LogProgressStart(ctx);
            string agreementId = ctx.AddContextDataToString(this.AgreementID);
            if( string.IsNullOrEmpty(agreementId))
            {
                //log.Error("[Execute]:" + Constants.ErrNoAgreementID); 
                throw new ApplicationException(Constants.ErrNoAgreementID);
            }

            try
            {
                AgreementInfo agreementInfo  = AdobeOperations.GetAgreementStatus(agreementId);
                
                if (agreementInfo == null)
                {
                    // TODO: log4Net, debug
                    // log in history 
                    this.LogHistoryListMessage = true;
                    this.HistoryListMessage = string.Format(Constants.ErrNoAgreementFound, agreementId);
                }
                else
                {
                    // LOG events and status 
                    this.Status = agreementInfo.status.ToString();
                    this.Events = Utilities.SerializeXml<List<DocumentHistoryEvent>>(agreementInfo.events);

                    if (agreementInfo.nextParticipantSetInfos != null)
                             this.PendingApprovers = string.Join(";" , agreementInfo.nextParticipantSetInfos.SelectMany(npSInfo => npSInfo.nextParticipantSetMemberInfos).ToList().Select(e => e.email).ToArray());
                }
            }
            catch (Exception e)
            {
                //log.Error("[Execute]", e);
                throw;
            }
            base.LogProgressEnd(ctx, executionContext);           
            return ActivityExecutionStatus.Closed;
        }
        
        protected override ActivityExecutionStatus HandleFault(ActivityExecutionContext executionContext,  Exception exception)
        {
            // TODO: Provide activity-specific text introducing the exception.
            // log4nET ERROR 
            string activityErrorIntro = "Error in CheckAgreementStatusActivity " + exception.ToString(); 

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
