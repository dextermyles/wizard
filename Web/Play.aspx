<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Play.aspx.cs" Inherits="WizardGame.Play" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        // player object
        var currentPlayer = new function () {
            this.PlayerId = 0,
            this.Name = "",
            this.PictureURL = "",
            this.UserId = 0,
            this.connectionId = ""
        };

        var gameState = new function() {
            this.GameId = 0
        };

        // playerList array
        var playerList = Array();

        // server group id
        var groupNameId = '<%=Game.GroupNameId%>';

        // game lobby id
        var gameLobbyId = '<%=Game.GameLobbyId%>';

        // game id
        var gameId = '<%=Game.GameId%>';

        // is connected to server
        var isConnected = false;

        // max players
        var maxPlayers = <%=GameLobby.MaxPlayers%>;

        // connected players
        var totalPlayers = 0;

        var keepAliveInterval = 0;

        currentPlayer.PlayerId = '<%= PlayerData.PlayerId %>';
        currentPlayer.Name = '<%= PlayerData.Name %>';
        currentPlayer.PictureURL = '<%= PlayerData.PictureURL %>';
        currentPlayer.UserId = '<%= PlayerData.UserId %>';

        // initialize connection
        function onConnectionInit() {

            // update connection flag
            isConnected = true;

            // tell server we are joining the lobby
            joinGame(currentPlayer.PlayerId, groupNameId);

            // append chat message
            appendChatMessage("Server", "Connected to game lobby!");

            // setup keep-alive
            keepAliveInterval = setInterval(function () {
                keepAlive();
            }, 15000);
        };

        // Start the connection
        $.connection.hub.start().done(onConnectionInit);

        // get reference to hub
        var hub = $.connection.gameSessionHub;

        /*******************************************
         * functions that are called by the server *
         *******************************************/

        // playerJoinedLobby
        hub.client.playerJoinedGame = function (playerId, playerName, playerConnectionId) {
            // log message
            logMessage("-- " + playerName + " has joined the game lobby --");

            // chat message player joined
            appendChatMessage(playerName, "Joined the game lobby.")
        };

        /*******************************************
         * functions that are called by the client *
         *******************************************/

        function joinGame(playerId, groupNameId) {
            logMessage("-- calling joinGame(" + playerId + "," + groupNameId + ") on server --");

            // call joinGameLobby on server
            hub.server.joinGame(playerId, gameId, groupNameId)
                .done(function () {
                    logMessage("-- joinGame executed on server --");
                })
                .fail(function (msg) {
                    logMessage("-- " + msg + " --");
                });
        };

        function sendChatMessage() {
            logMessage("-- calling sendChatMessage on server --");

            // get chat message box
            var $chatbox = $("#txtChatMessage");

            // get msg
            var message = $chatbox.val();

            // clear chat box
            $chatbox.val('');

            // send to server
            hub.server.sendChatMessage(currentPlayer.Name, message, groupNameId)
                .done(function () {
                    logMessage("-- sendChatMessage executed on server --");
                })
                .fail(function (msg) {
                    logMessage("-- " + msg + " --");
                });
        };

        function clearChatWindow() {
            var $chatwindow = $("#txtChatWindow");

            // clear chat window
            $chatwindow.val('');

            logMessage("-- cleared chat window --");
        };

        function appendChatMessage(playerName, message) {
            var oldMessages = $("#txtChatWindow").val();
            var time = new Date();
            var timeStr = time.getHours() + ":" + time.getMinutes() + ":" + time.getSeconds();
            var chatStr = "[" + timeStr + "] " + playerName + ": " + message + "\n";

            // append new message
            $("#txtChatWindow").val(oldMessages + chatStr);
        };
    </script>
    <style type="text/css">
        .auto-style2 {
            height: 20px;
        }
    </style>
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="col-md-9">
            <div class="game-board">
                <table class="game-board-table">
                    <tr>
                        <td></td>
                        <td>
                            <span class="label label-info">Player 1</span>
                        </td>
                        <td>
                            <span class="label label-info">Player 2</span>
                        </td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>
                            <span class="label label-info">Player 6</span>
                        </td>
                        <td colspan="2">
                            <div class="cards-played">
                            </div>
                        </td>
                        <td>
                            <span class="label label-info">Player 3</span>
                        </td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td>
                            <span class="label label-info">Player 5</span>
                        </td>
                        <td>
                            <span class="label label-info">Player 4</span>
                        </td>
                        <td>&nbsp;</td>
                    </tr>
                </table>
            </div>
            <style type="text/css">
                .game-board {
                    background-image: url('/assets/table/table-default.png');
                    background-repeat: no-repeat;
                    background-size: 100% 100%;
                }

                .game-board-table {
                    width: 100%;
                    height: 360px;
                }

                    .game-board-table tr td {
                        vertical-align: middle;
                        text-align: center;
                    }
            </style>
        </div>
        <div class="col-md-3">
            <div class="panel panel-default">
                <div class="panel-heading">Chat</div>
                <div class="panel-body">
                    <div class="form-group">
                        <textarea id="txtChatWindow" name="txtChatWindow" class="form-control" rows="6" readonly></textarea>
                        <style type="text/css">
                            #txtChatWindow {
                                width: 100%;
                                border: none;
                                resize: none;
                                background-color: transparent;
                            }
                        </style>
                    </div>
                    <div class="form-group" style="margin-bottom: 0px;">
                        <div class="input-group">
                            <input type="text" id="txtChatMessage" name="txtChatMessage" class="form-control" placeholder="Chat message" />
                            <span class="input-group-btn">
                                <!--input type="button" id="btnClearChat" name="btnClearChat" class="btn btn-default" value="Clear" onclick="clearChatWindow(); return false;" /-->
                                <input type="button" id="btnSendChat" name="btnSendChat" class="btn btn-primary" value="Send" onclick="sendChatMessage(); return false;" />
                                <script>
                                    // bind enter event to chat text box
                                    $("#txtChatMessage").bind("keypress", function (event) {
                                        // enter key pressed
                                        if (event.keyCode == 13) {
                                            sendChatMessage();
                                        }
                                    });
                                </script>
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
