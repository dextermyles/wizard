<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Account.aspx.cs" Inherits="WizardGame.Account" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        // execute on dom ready
        $(document).ready(function () {
            $(".nav li").removeClass("active");
            $("#link-account").addClass("active");
        });
    </script>
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="page-header hidden-xs" style="margin-top: 14px;">
            <h1>Leaderboard</h1>
        </div>
        <div class="row">
        </div>
    </div>
</asp:Content>
