/*  Akki.NintexAdobeSignAdapter.cs

    Copyright (c) 2015 – Nintex UK Ltd. All Rights Reserved.  
    This code released under the terms of the 
    Microsoft Reciprocal License (MS-RL, http://opensource.org/licenses/MS-RL.html.)

*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
using System.Workflow.ComponentModel;
using System.Collections;

using Microsoft.SharePoint;
using Microsoft.SharePoint.WorkflowActions;

using Nintex.Workflow;
using Nintex.Workflow.Activities.Adapters;

using Akki.AdobeSign.Common;

namespace Akki.NintexAdobeSign
{
    /// <summary>
    /// The workflow action adapter for the NW2013Template workflow action.
    /// </summary>
public class NintexAdobeSignAdapter : GenericRenderingAction
{
        


        /// <summary>
        /// Get the default configuration for the workflow action.
        /// </summary>
        /// <param name="context">The context in which the method was invoked.</param>
        /// <returns>The default configuration for the workflow action.</returns>
        /// <remarks></remarks>
        public override NWActionConfig GetDefaultConfig(GetDefaultConfigContext context)
        {

            NWActionConfig config = new NWActionConfig(this);
            config.Parameters = new ActivityParameter[5];
                        
            config.Parameters[0] = new ActivityParameter();
            config.Parameters[0].Name = UploadActivity.KeyApproversPropertyName;
            config.Parameters[0].PrimitiveValue = new PrimitiveValue();
            config.Parameters[0].PrimitiveValue.Value = string.Empty;
            config.Parameters[0].PrimitiveValue.ValueType = SPFieldType.Text.ToString();

            config.Parameters[1] = new ActivityParameter();
            config.Parameters[1].Name = UploadActivity.KeyAgreementName;
            config.Parameters[1].PrimitiveValue = new PrimitiveValue();
            config.Parameters[1].PrimitiveValue.Value = string.Empty;
            config.Parameters[1].PrimitiveValue.ValueType = SPFieldType.Text.ToString();


            config.Parameters[2] = new ActivityParameter();
            config.Parameters[2].Name = UploadActivity.KeyUrlMiscInfoPropertyName;
            config.Parameters[2].PrimitiveValue = new PrimitiveValue();
            config.Parameters[2].PrimitiveValue.Value = string.Empty;
            config.Parameters[2].PrimitiveValue.ValueType = SPFieldType.Text.ToString();


            config.Parameters[3] = new ActivityParameter();
            config.Parameters[3].Name = UploadActivity.KeyCompendiumDocsID;
            config.Parameters[3].PrimitiveValue = new PrimitiveValue();
            config.Parameters[3].PrimitiveValue.Value = string.Empty;
            config.Parameters[3].PrimitiveValue.ValueType = SPFieldType.Text.ToString();

            config.Parameters[4] = new ActivityParameter();
            config.Parameters[4].Name = UploadActivity.KeyOutAdobeAgreementIDProperty;
            config.Parameters[4].Variable = new NWWorkflowVariable();

            config.TLabel = ActivityReferenceCollection.FindByAdapter(this).Name;
            return config;
        }

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        /// <param name="context">The context in which the method was invoked.</param>
        /// <returns><b>true</b> if the configuration is valid; otherwise, <b>false</b>.</returns>
        /// <remarks></remarks>
        public override bool ValidateConfig(ActivityContext context)
        {
            bool isValid = true;            
            Dictionary<string, ActivityParameterHelper> parameters = context.Configuration.GetParameterHelpers();
            var approver = parameters[UploadActivity.KeyApproversPropertyName];
            if (string.IsNullOrEmpty(approver.Value))
            {
                validationSummary.AddError(UploadActivity.KeyApproversPropertyName, ValidationSummaryErrorType.CannotBeBlank);
                isValid &= false;
            }
            return isValid;
        }

        //TODO: Create  and Check the Column exists to store the transition key from adbe
        public override CompositeActivity AddActivityToWorkflow(PublishContext context)
        {
            // Prepare a keyed collection of ActivityParameterHelper objects.
            Dictionary<string, ActivityParameterHelper> parameters = 
                context.Config.GetParameterHelpers();

            // TODO: Instantiate the workflow activity.
            UploadActivity activity = new UploadActivity();

            parameters[UploadActivity.KeyApproversPropertyName].AssignTo(activity, UploadActivity.ApproversProperty, context);
            parameters[UploadActivity.KeyAgreementName].AssignTo(activity, UploadActivity.AgreementNameProperty, context);
            parameters[UploadActivity.KeyUrlMiscInfoPropertyName].AssignTo(activity, UploadActivity.UrlMiscInfoProperty, context);
            parameters[UploadActivity.KeyCompendiumDocsID].AssignTo(activity, UploadActivity.CompendiumDocsIDProperty, context);
            parameters[UploadActivity.KeyOutAdobeAgreementIDProperty].AssignTo(activity, UploadActivity.OutAdobeAgreementIDProperty, context);

                      
            // TODO: Set standard context items for the workflow activity.
            activity.SetBinding(UploadActivity.__ContextProperty, new ActivityBind(context.ParentWorkflow.Name, StandardWorkflowDataItems.__context));
            activity.SetBinding(UploadActivity.__ListItemProperty, new ActivityBind(context.ParentWorkflow.Name, StandardWorkflowDataItems.__item));
            activity.SetBinding(UploadActivity.__ListIdProperty, new ActivityBind(context.ParentWorkflow.Name, StandardWorkflowDataItems.__list));

            // If this custom workflow action supports error handling,
            // add any necessary code here.

            // TODO: Add labels from the configuration to the workflow activity.
            ActivityFlags f = new ActivityFlags();
            f.AddLabelsFromConfig(context.Config);
            f.AssignTo(activity);

            // TODO: Add the workflow activity to the parent workflow activity for the context.
            context.ParentActivity.Activities.Add(activity);

            //Utilities.AddESignStatusColumn(context.List);
            return null;
        }

        /// <summary>
        /// Gets the current configuration from the workflow action.
        /// </summary>
        /// <param name="context">The context in which the method was invoked.</param>
        /// <returns>The current configuration.</returns>
        public override NWActionConfig GetConfig(RetrieveConfigContext context)
        {
            
            NWActionConfig config = this.GetDefaultConfig(context);            
            Dictionary<string, ActivityParameterHelper> parameters = config.GetParameterHelpers();

            parameters[UploadActivity.KeyApproversPropertyName].RetrieveValue(context.Activity, UploadActivity.ApproversProperty, context);
            parameters[UploadActivity.KeyAgreementName].RetrieveValue(context.Activity, UploadActivity.AgreementNameProperty, context);
            parameters[UploadActivity.KeyUrlMiscInfoPropertyName].RetrieveValue(context.Activity, UploadActivity.UrlMiscInfoProperty, context);
            parameters[UploadActivity.KeyCompendiumDocsID].RetrieveValue(context.Activity, UploadActivity.CompendiumDocsIDProperty, context);
            parameters[UploadActivity.KeyOutAdobeAgreementIDProperty].RetrieveValue(context.Activity, UploadActivity.OutAdobeAgreementIDProperty, context);

            return config;
        }

        /// <summary>
        /// Gets the action summary from the workflow action, if the workflow action is 
        /// successfully configured.
        /// </summary>
        /// <param name="context">The context in which the method was invoked.</param>
        /// <returns>The action summary.</returns>
        /// <remarks>This method is invoked after the ValidateConfig method is 
        /// invoked to confirm that the workflow action is successfully configured.</remarks>
        public override ActionSummary BuildSummary(ActivityContext context)
        {
            string displayMessage = "";

            // Prepare a keyed collection of ActivityParameterHelper objects.
            Dictionary<string, ActivityParameterHelper> parameters = 
                context.Configuration.GetParameterHelpers();

            // TODO: Construct a display message for the action summary.
            displayMessage = string.Format("Send the document to Adobe Esign and assigns approvers.");

            // Return the action summary.
            return new ActionSummary(displayMessage);
        }
    }
}