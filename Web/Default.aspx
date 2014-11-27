<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WizardGame.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script>
        
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container login well well-sm">
        <h1></h1>
        <h3 class="form-signin-heading" style="margin-top: 0px;">
            <span class="glyphicon glyphicon-user"></span>
            Please sign in
        </h3>
        <div id="MessageBox" class="alert alert-danger alert-dismissible fade in" role="alert" runat="server" visible="false">
            <button type="button" class="close" data-dismiss="alert"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>
            <p id="MessageBoxText" runat="server" />
        </div>
        <label for="inputEmail" class="sr-only">Username</label>
        <input type="text" id="txtUsername" class="form-control username" placeholder="Username" required autofocus runat="server" />
        <label for="inputPassword" class="sr-only">Password</label>
        <input type="password" id="txtPassword" class="form-control password" placeholder="Password" required runat="server" />
        <div class="checkbox">
            <label>
                <input id="cbRemember" type="checkbox" value="remember-me" runat="server" />
                Remember me
            </label>
            <label class="pull-right">
                <a href="Register.aspx">Create account</a>
            </label>
        </div>
        <br />
        <asp:Button ID="btnLogin" CssClass="btn btn-lg btn-success btn-block" Text="Sign in" runat="server" OnClick="btnLogin_Click" />
        <button id="btnFacebook" class="btn btn-lg btn-primary btn-block" onclick="facebookLogin(); return false;">Sign in with Facebook</button>
        <input type="hidden" id="txtFacebookEmail" name="txtFacebookEmail" runat="server" />
        <input type="hidden" id="txtFacebookUserId" name="txtFacebookUserId" runat="server" />
        <input type="hidden" id="txtIsFacebookLogin" name="txtIsFacebookLogin" runat="server" value="0" />
    </div>
    <style type="text/css">
        .login {
            max-width: 480px;
        }
    </style>
</asp:Content>
