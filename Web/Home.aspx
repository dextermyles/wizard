<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="WizardGame.Home" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="HeadContent" runat="server">
    <script>
        function selectUseFacebookPhoto() {
            // make sure we have the url
            if (facebookPhotoUrl != null && facebookPhotoUrl.length > 0) {
                $("#facebook-photo-preview").html("<img src='" + facebookPhotoUrl + "' alt='profile photo' />");
                $("#facebook-photo-preview").fadeIn("slow");
            }
        }
    </script>
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="page-header hidden-xs" style="margin-top: 14px;">
            <h1 id="WelcomeTitle" runat="server">Welcome, guest!</h1>
        </div>
        <div class="col-sm-6">
            <div class="panel panel-default">
                <div class="panel-heading">
                    <strong>Getting started</strong>
                </div>
                <ul class="list-group">
                    <% if (UserPlayers != null && UserPlayers.Length == 0)
                       { %>
                    <li class="list-group-item">
                        <button class="btn btn-lg btn-default btn-block" data-toggle="modal" data-target="#newPlayerModal" onclick="return false;">
                            Create your Player
                        </button>
                    </li>
                    <% } // UserPlayers %>
                    <li class="list-group-item">
                        <a class="btn btn-lg btn-default btn-block" href="HostGame.aspx">
                            Host Game
                        </a>
                    </li>
                    <li class="list-group-item">
                        <a class="btn btn-lg btn-default btn-block" href="AvailableGames.aspx">
                            Join Game
                        </a>
                    </li>
                </ul>
            </div>
        </div>
        <div class="col-sm-6">
            <!-- available games -->
            <div class="panel panel-default">
                <div class="panel-heading">
                    <strong>Available games to join</strong>
                </div>
                <table class="table table-responsive">
                    <thead>
                        <tr>
                            <th style="width:40%;">Lobby</th>
                            <th class="text-center" style="width:40%;">Host</th>
                            <th class="text-center" style="width: 20%;">Players</th>
                        </tr>
                    </thead>
                    <tbody>
                        <%= ListGameLobbiesHtml() %>
                    </tbody>
                </table>
                <div class="panel-footer">
                    <a href="#">Show all</a>
                </div>
            </div>
            <!-- match history -->
            <!--
            <div class="panel panel-default hidden-xs">
                <div class="panel-heading">
                    <strong>Match history</strong>
                </div>
                <table class="table table-responsive panel-body">
                    <thead>
                        <tr>
                            <th>Date</th>
                            <th class="text-center">Score</th>
                            <th class="text-center">Result</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td colspan="3">No recent matches</td>
                        </tr>
                    </tbody>
                </table>
                <div class="panel-footer">
                    <a href="#">Show all</a>
                </div>
            </div>
            -->
        </div>
    </div>
    <div class="modal fade" id="newPlayerModal" tabindex="-1" role="dialog" aria-labelledby="newPlayerModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal"><span aria-hidden="true">&times;</span><span class="sr-only">Close</span></button>
                    <h4 class="modal-title" id="newPlayerModalLabel">New player</h4>
                </div>
                <div class="modal-body">
                    <p class="alert alert-info"><span class="glyphicon glyphicon-info-sign"></span>Please enter some information about your player.</p>
                    <div class="form-group">
                        <label for="recipient-name" class="control-label">Name</label>
                        <asp:TextBox ID="PlayerName" CssClass="form-control" runat="server" />
                    </div>
                    <div class="form-group">
                        <label for="message-text" class="control-label">Voice pack</label>
                        <asp:DropDownList ID="VoicePackList" runat="server" CssClass="form-control">
                            <asp:ListItem Text="Rob" Value="rob"></asp:ListItem>
                            <asp:ListItem Text="Dexter" Value="dexter"></asp:ListItem>
                            <asp:ListItem Text="Scott" Value="scott"></asp:ListItem>
                            <asp:ListItem Text="Kevin" Value="kevin"></asp:ListItem>
                            <asp:ListItem Text="Tony" Value="tony"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="form-group">
                        <label for="message-text" class="control-label">Photo</label>
                        <asp:FileUpload ID="PlayerPhoto" CssClass="form-control" runat="server" />
                    </div>
                    <div class="checkbox" id="UseFacebookProfilePhoto" runat="server">
                        <label>
                            <asp:CheckBox ID="cbUseFacebookPhoto" runat="server" OnClick="selectUseFacebookPhoto();" />
                            Use profile photo from Facebook
                            <input type="hidden" id="txtFacebookPhotoUrl" name="txtFacebookPhotoUrl" runat="server" value="" />
                        </label>
                    </div>
                    <div class="form-group facebook-photo-preview hidden">
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-default" data-dismiss="modal" onclick="return false;">Close</button>
                    <asp:Button ID="btnNewPlayer" runat="server" CssClass="btn btn-primary" Text="Create player" OnClick="btnNewPlayer_Click" />
                </div>
            </div>
        </div>
    </div>
    <script type="text/javascript">
        $('#newPlayerModal').on('show.bs.modal', function (event) {
            // load profile picture url from facebook (if signed in)
            getFacebookPictureURL();

            var button = $(event.relatedTarget) // Button that triggered the modal
            var modal = $(this)
        })
    </script>
</asp:Content>
