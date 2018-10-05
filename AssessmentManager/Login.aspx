<%@ Page Language="C#" MasterPageFile="~/Web.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs"
    Inherits="AssessmentManager.Login" Title="User Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:Login ID="Login1" runat="server" CreateUserUrl="~/Register.aspx" CreateUserText="Register">
    </asp:Login>
</asp:Content>
