<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Leaderboard.aspx.cs" Inherits="WizardGame.Leaderboard" %>
<asp:Content ID="ContentHead" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        // execute on dom ready
        $(document).ready(function () {
            $(".nav li").removeClass("active");
            $("#link-leaderboard").addClass("active");
        });
    </script>
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="page-header hidden-xs" style="margin-top: 14px;">
            <h1>Leaderboard</h1>
        </div>
        <div class="row">
            <div class="panel panel-default">
                <div class="panel-heading">
                    <strong>
                        Top 10 players with most wins
                    </strong>
                    <div class="clearfix"></div>
                </div>
                <table class="table table-striped table-responsive table-hover hand-history">
                    <thead>
                        <tr>
                            <th class="">Player</th>
                            <th class="text-center">Wins</th>
                        </tr>
                    </thead>
                    <tbody>
                        <%=LeaderboardHtml() %>
                    </tbody>
                </table>
                <div class="panel-footer text-center">
                    <a href="Home.aspx">Back to dashboard</a>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
