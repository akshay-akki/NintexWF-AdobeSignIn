/*  Akki.NintexAdobeSignDialog.aspx.cs

    Codebehind file for Nintex Workflow 2013 workflow action configuration page.

    Copyright (c) 2015 – Nintex UK Ltd. All Rights Reserved.  
    This code released under the terms of the 
    Microsoft Reciprocal License (MS-RL, http://opensource.org/licenses/MS-RL.html.)

*/
using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.UI;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Nintex.Workflow.ApplicationPages;

namespace Akki.NintexAdobeSign
{
    public partial class UploadItemToSign : Nintex.Workflow.ServerControls.NintexLayoutsBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Register user controls and server controls for JavaScript.
            // TODO: Add your user controls and server controls to the array.
            // ScriptFiles.RegisterControlsForJS(this, new Control[] { 
            //     yourUserControl 
            // });
        }
    }
}