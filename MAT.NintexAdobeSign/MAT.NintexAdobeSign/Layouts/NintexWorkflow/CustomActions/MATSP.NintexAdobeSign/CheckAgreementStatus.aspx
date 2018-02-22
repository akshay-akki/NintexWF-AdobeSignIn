<%-- 
    Akki.NintexAdobeSignDialog.aspx
    
    Nintex Workflow 2013 configuration page for custom workflow actions

    Copyright (c) 2015 – Nintex UK Ltd. All Rights Reserved.  
    This code released under the terms of the 
    Microsoft Reciprocal License (MS-RL, http://opensource.org/licenses/MS-RL.html.)
--%>

<%-- Page directive required by Nintex Workflow 2013 --%>
<%@ Page Language="C#" DynamicMasterPageFile="~masterurl/default.master" AutoEventWireup="true" CodeBehind="CheckAgreementStatus.aspx.cs" EnableEventValidation="false"
    Inherits="Akki.NintexAdobeSign.NintexAdobeSignDialog, $SharePoint.Project.AssemblyFullName$"  %>

<%-- Register directives required by Nintex Workflow 2013 --%>
<%@ Register TagPrefix="Nintex" Namespace="Nintex.Workflow.ServerControls" 
    Assembly="Nintex.Workflow.ServerControls, Version=1.0.0.0, Culture=neutral, PublicKeyToken=913f6bae0ca5ae12" %>
<%@ Register TagPrefix="Nintex" Namespace="Nintex.Workflow.ApplicationPages" 
    Assembly="Nintex.Workflow.ApplicationPages, Version=1.0.0.0, Culture=neutral, PublicKeyToken=913f6bae0ca5ae12" %>
<%@ Register TagPrefix="Nintex" TagName="ConfigurationPropertySection" src="~/_layouts/15/NintexWorkflow/ConfigurationPropertySection.ascx" %>
<%@ Register TagPrefix="Nintex" TagName="ConfigurationProperty" src="~/_layouts/15/NintexWorkflow/ConfigurationProperty.ascx" %>
<%@ Register TagPrefix="Nintex" TagName="DialogLoad" Src="~/_layouts/15/NintexWorkflow/DialogLoad.ascx" %>
<%@ Register TagPrefix="Nintex" TagName="DialogBody" Src="~/_layouts/15/NintexWorkflow/DialogBody.ascx" %>

<%@ Register TagPrefix="Nintex" TagName="SingleLineInput" Src="~/_layouts/15/NintexWorkflow/SingleLineInput.ascx" %>

<%-- Place additional Register directives after this comment. --%>

<asp:Content ID="ContentHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <%-- The DialogLoad control must be the first child of this Content control. --%>
    <Nintex:DialogLoad runat="server" />

    <%-- JavaScript functions for reading and writing configuration data. --%>
    <script type="text/javascript" language="javascript">
        function TPARetrieveConfig() {

            setRTEValue('<%=AgreementID.ClientID%>', configXml.selectSingleNode("/NWActionConfig/Parameters/Parameter[@Name='AgreementID']/PrimitiveValue/@Value").text);

             var drpStatus = document.getElementById("<%= Status.ClientID %>");
             drpStatus.value = configXml.selectSingleNode("/NWActionConfig/Parameters/Parameter[@Name='Status']/Variable/@Name").text;

             var drpPendingApprovers = document.getElementById("<%= PendingApprovers.ClientID %>");
            drpPendingApprovers.value = configXml.selectSingleNode("/NWActionConfig/Parameters/Parameter[@Name='PendingApprovers']/Variable/@Name").text;

            var drpEvents = document.getElementById("<%= Events.ClientID %>");
            drpEvents.value = configXml.selectSingleNode("/NWActionConfig/Parameters/Parameter[@Name='Events']/Variable/@Name").text;


        }

        function TPAWriteConfig() {
            // Use this JavaScript function to retrieve configuration settings from
            // controls on the configuration page and set the values of the 
            // corresponding elements in the configuration XML.
            configXml.selectSingleNode("/NWActionConfig/Parameters/Parameter[@Name='AgreementID']/PrimitiveValue/@Value").text = getRTEValue('<%=AgreementID.ClientID%>');

            var drpStatus = document.getElementById("<%= Status.ClientID %>");
            configXml.selectSingleNode("/NWActionConfig/Parameters/Parameter[@Name='Status']/Variable/@Name").text = drpStatus.value;


            var drpPendingApprovers = document.getElementById("<%= PendingApprovers.ClientID %>");
            configXml.selectSingleNode("/NWActionConfig/Parameters/Parameter[@Name='PendingApprovers']/Variable/@Name").text = drpPendingApprovers.value;

            var drpEvents = document.getElementById("<%= Events.ClientID %>");
            configXml.selectSingleNode("/NWActionConfig/Parameters/Parameter[@Name='Events']/Variable/@Name").text = drpEvents.value;
            
            return true;
        }

        // Register the ConfigurationPropertySection on the page.
        // The dialogSectionsArray determines what sections are displayed in the
        // Configure Action configuration settings dialog. 
        // Set the value to true to initially show the section when the Action button
        // is toggled on; otherwise, set it to false to initially hide the section.
        onLoadFunctions[onLoadFunctions.length] = function () {
            dialogSectionsArray["<%= MainControls1.ClientID %>"] = true;
        };
    </script>
</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="PlaceHolderMain" runat="Server">     
    <Nintex:ConfigurationPropertySection runat="server" Id="MainControls1">
        <TemplateRowsArea>
            <%-- Place ConfigurationProperty controls here --%>    
            
              <Nintex:ConfigurationProperty runat="server" FieldTitle="Adobe Agreement Id" RequiredField="True">
                   <TemplateControlArea>                       
                        <Nintex:SingleLineInput clearFieldOnInsert="true"  runat="server" id="AgreementID"></Nintex:SingleLineInput>
                  </TemplateControlArea>
                </Nintex:ConfigurationProperty>


            <Nintex:ConfigurationProperty runat="server" FieldTitle="Status" >
                <TemplateControlArea>
                     <Nintex:VariableSelector ID="Status" runat="server" IncludeTextVars="True"></Nintex:VariableSelector>
                </TemplateControlArea>
            </Nintex:ConfigurationProperty>
            
            <Nintex:ConfigurationProperty runat="server" FieldTitle="PendingApprovers">
                <TemplateControlArea>
                     <Nintex:VariableSelector ID="PendingApprovers" runat="server" IncludeTextVars="True"></Nintex:VariableSelector>
                </TemplateControlArea>
            </Nintex:ConfigurationProperty>
            
            
            <Nintex:ConfigurationProperty runat="server" FieldTitle="Adobe Events">
                <TemplateControlArea>
                     <Nintex:VariableSelector ID="Events" runat="server" IncludeTextVars="True"></Nintex:VariableSelector>
                </TemplateControlArea>
            </Nintex:ConfigurationProperty>

            

                   
        </TemplateRowsArea>
    </Nintex:ConfigurationPropertySection>

    <%-- The DialogBody control must be the last child of this Content control. --%>    
    <Nintex:DialogBody runat="server" id="DialogBody" />
</asp:Content>
