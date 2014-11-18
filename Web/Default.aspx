<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WizardGame.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script>
        

        function fb_login() {
            FB.login(function (response) {
                if (response.authResponse) {
                    // log
                    logMessage('-- facebook login complete --');

                    // is facebook login
                    $("#MainContent_txtIsFacebookLogin").val(1);

                    // query api
                    getFacebookLoginInfo();
                } else {
                    //user hit cancel button
                    logMessage('User cancelled login or did not fully authorize.');
                }
            }, {
                scope: 'email'
            });
        }

        

        
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container login well well-sm" style="">
        <h2 class="form-signin-heading" style="margin-top: 0px;">Please sign in</h2>
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
        <asp:Button ID="btnLogin" CssClass="btn btn-lg btn-success btn-block" Text="Sign in" runat="server" OnClick="btnLogin_Click" />
        <button id="btnFacebook" class="btn btn-lg btn-primary btn-block" onclick="fb_login(); return false;">Sign in with Facebook</button>
        <input type="hidden" id="txtFacebookEmail" name="txtFacebookEmail" runat="server" />
        <input type="hidden" id="txtFacebookUserId" name="txtFacebookUserId" runat="server" />
        <input type="hidden" id="txtIsFacebookLogin" name="isFacebookLogin" runat="server" value="0" />
    </div>
    <style type="text/css">
        .login {
            max-width: 480px;
            margin-top: 70px;
        }
    </style>
</asp:Content>
