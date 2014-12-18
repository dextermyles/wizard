<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="ViewHandHistory.aspx.cs" Inherits="WizardGame.ViewHandHistory" %>
<asp:Content ID="ContentHead" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- viewport settings for mobile -->
    <meta name="viewport" content="height=device-height,width=device-width,initial-scale=0.45,maximum-scale=1" >
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="page-header hidden-xs" style="margin-top: 14px;">
            <h1>Hand History</h1>
        </div>
        <div class="row">
            <div class="panel panel-default">
                <div class="panel-heading">
                    <strong>
                        GameID: #<%=GameId %>
                        <span class="pull-right">
                            Completed on
                            <%
                                if (GameData.DateCompleted.HasValue)
                                    Response.Write(GameData.DateCompleted.Value.ToString("d"));
                                else
                                    Response.Write("?");
                            %>
                        </span>
                    </strong>
                    <div class="clearfix"></div>
                </div>
                <table class="table table-striped table-responsive table-hover hand-history">
                    <thead>
                        <tr>
                            <th class="text-center">Hand #</th>
                            <th class="text-center">Round</th>
                            <th class="text-center hidden-xs">Cards Played</th>
                            <th class="text-center">Trump</th>
                            <th class="text-center">Suit to follow</th>
                            <th class="text-center">Winner</th>
                        </tr>
                    </thead>
                    <tbody>
                        <%=HandHistoryHtml() %>
                    </tbody>
                </table>
                <style type="text/css">
                    .hand-history tr td {
                        vertical-align: middle !important;
                    }
                    .card-owner {
                        background-color: rgb(199, 199, 199);
                        border-radius: 5px;
                    }

                    .pagination {
                        display: inline-block;
                        padding-left: 0;
                        margin: 0;
                        border-radius: 4px;
                    }
                </style>
                <div class="panel-footer text-center">
                    <a href="Home.aspx">Back to dashboard</a>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
