<%@ Page Title="Homepage" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="MailUpExample._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">

    <%--    <div>
        <asp:Button OnClick="LogOn_ServerClick" ID="LogOn" runat="server" Text="Sign in to MailUp" />
        <br />
    </div>--%>
    <div>
        <div>
            Username: 
            <div>
                <asp:TextBox ID="txtUsr" Columns="40" runat="server" Text="m126193" Enabled="false" />
            </div>
        </div>
        <div>
            Password: 
             <div>
                 <asp:TextBox ID="txtPwd" Columns="40" runat="server" Text="m@1lUp2K18!" Enabled="false" />
             </div>
        </div>
        <br />
        <div>
            <asp:Button OnClick="LogOnWithUsernamePassword_ServerClick" Columns="40" ID="Button1" runat="server" Text="Sign in" />
        </div>
    </div>

    <p id="pAuthorization" runat="server"></p>
    <br />
    <%--    <p><b>Custom method call</b></p>--%>
    <table>
        <thead>
            <td>Verb</td>
            <td>Content-Type</td>
            <td>Endpoint</td>
            <td>Path</td>
        </thead>
        <tr>
            <td>
                <asp:DropDownList ID="lstVerb" runat="server" AutoPostBack="true" /></td>
            <td style="width: 100px;">
                <asp:DropDownList ID="lstContentType" runat="server" AutoPostBack="true" Width="100%" /></td>
            <td>
                <asp:DropDownList ID="lstEndpoint" runat="server" AutoPostBack="true" /></td>
            <td>
                <asp:TextBox ID="txtPath" Columns="80" Value="/Console/Authentication/Info" runat="server" /></td>
        </tr>
    </table>
    <p>Body</p>
    <p>
        <asp:TextBox ID="txtBody" TextMode="MultiLine" Rows="5" Columns="80" runat="server" />
    </p>
    <p>
        <asp:Button OnClick="CallMethod_ServerClick" ID="CallMethod" runat="server" Text="Call Method" />
    </p>

    <p id="pResultString" runat="server"></p>
    <p id="pExampleResultString" runat="server"></p>

    <p>
        <asp:Button Style="text-align: left;" Width="400px" OnClick="RunExample1_ServerClick" ID="RunExample1" runat="server" Text="1 - Import recipients" />
    </p>

    <p>
        <asp:Button Style="text-align: left;" Width="400px" OnClick="RunExample2_ServerClick" ID="RunExample2" runat="server" Text="2 - Unsubscripe a recipient" />
    </p>
    <p>
        <asp:Button Style="text-align: left;" Width="400px" OnClick="RunExample3_ServerClick" ID="RunExample3" runat="server" Text="3 - Update a recipient" />
    </p>
    <p>
        <asp:Button Style="text-align: left;" Width="400px" OnClick="RunExample4_ServerClick" ID="RunExample4" runat="server" Text="4 - Create a message from template" />
    </p>
    <p>
        <asp:Button Style="text-align: left;" Width="400px" OnClick="RunExample5_ServerClick" ID="RunExample5" runat="server" Text="5 - Create a message from scratch" />
    </p>
    <p>
        <asp:Button Style="text-align: left;" Width="400px" OnClick="RunExample6_ServerClick" ID="RunExample6" runat="server" Text="6 - Tag a message" />
    </p>
    <p>
        <asp:Button Style="text-align: left;" Width="400px" OnClick="RunExample7_ServerClick" ID="RunExample7" runat="server" Text="7 - Send a message" />
    </p>
    <p>
        <asp:Button Style="text-align: left;" Width="400px" OnClick="RunExample8_ServerClick" ID="RunExample8" runat="server" Text="8 - Retrieve statistics" />
    </p>
</asp:Content>
