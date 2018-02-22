<%-- 
    Akki.NintexAdobeSignDialog.aspx
    
    Nintex Workflow 2013 configuration page for custom workflow actions

    Copyright (c) 2015 – Nintex UK Ltd. All Rights Reserved.  
    This code released under the terms of the 
    Microsoft Reciprocal License (MS-RL, http://opensource.org/licenses/MS-RL.html.)
--%>

<%-- Page directive required by Nintex Workflow 2013 --%>
<%@ Page Language="C#" DynamicMasterPageFile="~masterurl/default.master" AutoEventWireup="true" CodeBehind="DownloadDocument.aspx.cs" EnableEventValidation="false"
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
            setRTEValue('<%=DestWebUrl.ClientID%>', configXml.selectSingleNode("/NWActionConfig/Parameters/Parameter[@Name='DestWebUrl']/PrimitiveValue/@Value").text);
            setRTEValue('<%=DestDocLib.ClientID%>', configXml.selectSingleNode("/NWActionConfig/Parameters/Parameter[@Name='DestDocLib']/PrimitiveValue/@Value").text);
            setRTEValue('<%=DestDocName.ClientID%>', configXml.selectSingleNode("/NWActionConfig/Parameters/Parameter[@Name='DestDocName']/PrimitiveValue/@Value").text);

            var drpOutDef = document.getElementById("<%= OutDocumentID.ClientID %>");
            drpOutDef.value = configXml.selectSingleNode("/NWActionConfig/Parameters/Parameter[@Name='OutDocumentID']/Variable/@Name").text;

        }

        function TPAWriteConfig() {           
            configXml.selectSingleNode("/NWActionConfig/Parameters/Parameter[@Name='AgreementID']/PrimitiveValue/@Value").text = getRTEValue('<%=AgreementID.ClientID%>');
            configXml.selectSingleNode("/NWActionConfig/Parameters/Parameter[@Name='DestWebUrl']/PrimitiveValue/@Value").text = getRTEValue('<%=DestWebUrl.ClientID%>');
            configXml.selectSingleNode("/NWActionConfig/Parameters/Parameter[@Name='DestDocLib']/PrimitiveValue/@Value").text = getRTEValue('<%=DestDocLib.ClientID%>');
            configXml.selectSingleNode("/NWActionConfig/Parameters/Parameter[@Name='DestDocName']/PrimitiveValue/@Value").text = getRTEValue('<%=DestDocName.ClientID%>');

            var drpOutDef = document.getElementById("<%= OutDocumentID.ClientID %>");
            configXml.selectSingleNode("/NWActionConfig/Parameters/Parameter[@Name='OutDocumentID']/Variable/@Name").text = drpOutDef.value;

            return true;
        }
        
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


            <Nintex:ConfigurationProperty runat="server" FieldTitle="Full URL path of the destination web site " >
                <TemplateControlArea>
                    <Nintex:SingleLineInput clearFieldOnInsert="true"  runat="server" id="DestWebUrl"></Nintex:SingleLineInput>
                </TemplateControlArea>
            </Nintex:ConfigurationProperty>

            
            <Nintex:ConfigurationProperty runat="server" FieldTitle="Doc Lib to store downloaded doc (in destination web site)" >
                <TemplateControlArea>
                    <Nintex:SingleLineInput clearFieldOnInsert="true"  runat="server" id="DestDocLib"></Nintex:SingleLineInput>
                </TemplateControlArea>
            </Nintex:ConfigurationProperty>

            <Nintex:ConfigurationProperty runat="server" FieldTitle="Name to store the downloaded pdf with" >
                <TemplateControlArea>
                    <Nintex:SingleLineInput clearFieldOnInsert="true"  runat="server" id="DestDocName"></Nintex:SingleLineInput>
                </TemplateControlArea>
            </Nintex:ConfigurationProperty>

             <Nintex:ConfigurationProperty runat="server" FieldTitle="ID of downloaded document" >
                <TemplateControlArea>
                     <Nintex:VariableSelector ID="OutDocumentID" runat="server"  IncludeIntegerVars ="True"></Nintex:VariableSelector>
                </TemplateControlArea>
            </Nintex:ConfigurationProperty>

        </TemplateRowsArea>
    </Nintex:ConfigurationPropertySection>

    <%-- The DialogBody control must be the last child of this Content control. --%>    
    <Nintex:DialogBody runat="server" id="DialogBody" />
</asp:Content>
