<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="WizardGame.Register" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            $("#MainForm")[0].reset();
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container login well well-sm ">
        <h3 class="form-signin-heading" style="margin-top: 0px;">
            <span class="glyphicon glyphicon-user"></span>
            Create an Account
        </h3>
        <div id="MessageBox" class="alert alert-danger alert-dismissible fade in" role="alert" runat="server" visible="false">
            <button type="button" class="close" data-dismiss="alert"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>
            <p id="MessageBoxText" runat="server" />
        </div>
        <div class="form-group">


            <asp:Label AssociatedControlID="txtUsername" runat="server">Username</asp:Label>
            <input id="txtUsername" name="txtUsername" runat="server" type="text" class="form-control" placeholder="Username" required autofocus autocomplete="off" />
        </div>
        <div class="form-group">
            <asp:Label AssociatedControlID="txtEmailAddress" runat="server">Email Address</asp:Label>
            <input id="txtEmailAddress" name="txtEmailAddress" runat="server" type="text" class="form-control" placeholder="Email Address" required autocomplete="off" />
        </div>
        <div class="form-group">
            <asp:Label AssociatedControlID="txtPassword" runat="server">Password</asp:Label>
            <input id="txtPassword" name="txtPassword" runat="server" type="password" class="form-control" placeholder="Password" required />
        </div>
        <asp:Button ID="btnRegister" CssClass="btn btn-lg btn-primary btn-block" Text="Register" runat="server" OnClick="btnRegister_Click" />
        <a href="Default.aspx" class="btn btn-lg btn-warning btn-block">Back to Login page</a>
    </div>
    <style type="text/css">
        .login {
            max-width: 480px;
        }
    </style>
</asp:Content>
