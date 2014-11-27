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

            // get updated player list every 10 seconds
            keepAliveInterval = setInterval(function () {
                sendKeepAlive();
            }, 30000);
        };

        // Start the connection
        $.connection.hub.start().done(onConnectionInit);

        $.connection.hub.reconnecting(function () {
            appendChatMessage("Server", "Attempting to reconnect to game lobby.");

            isConnected = false;
        });

        $.connection.hub.reconnected(function () {
            appendChatMessage("Server", "Reconnected to game lobby.");

            // tell server we are joining the lobby
            joinGameLobby(currentPlayer.PlayerId, groupNameId);

            isConnected = true;
        });

        $.connection.hub.disconnected(function () {
            // has error
            if ($.connection.hub.lastError) { 
                appendChatMessage("Server", $.connection.hub.lastError.message);
            }
            else {
                appendChatMessage("Server", "You have been disconnected from game lobby.");
            }

            isConnected = false;
        });

        // get reference to hub
        var hub = $.connection.gameHub;

        /*******************************************
         * functions that are called by the server *
         *******************************************/

        // playerJoinedLobby
        hub.client.playerJoinedGame = function (playerId, playerName, playerConnectionId) {
            // chat message player joined
            appendChatMessage(playerName, "Joined the game lobby.")
        };

        // receiveChatMessage
        hub.client.receiveChatMessage = function receiveChatMessage(playerName, message) {
            // append to chat window
            appendChatMessage(playerName, message);
        };

        // receivePlayerList
        hub.client.receivePlayerList = function receivePlayerList(players) {
            // update player list
            updatePlayerList(players);
        };

        // receiveGameData
        hub.client.receiveGameData = function receiveGameData(gameData) {
            console.log(gameData);
        };

        /*******************************************
         * functions that are called by the client *
         *******************************************/

        function joinGame(playerId, groupNameId) {
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
        };

        function appendChatMessage(playerName, message) {
            var oldMessages = $("#txtChatWindow").val();
            var time = new Date();
            var timeStr = time.getHours() + ":" + time.getMinutes() + ":" + time.getSeconds();
            var chatStr = "[" + timeStr + "] " + playerName + ": " + message + "\n";

            // append new message
            $("#txtChatWindow").val(oldMessages + chatStr);

            // scroll to bottom
            var psconsole = $('#txtChatWindow');

            if(psconsole.length)
                psconsole.scrollTop(psconsole[0].scrollHeight - psconsole.height());
        };

        function updatePlayerList(players) {
            console.log(players);
        };

        function getListOfPlayersInGame() {
            if (isConnected) {
                hub.server.listPlayersInGame(gameId, groupNameId);
            }
        };

        function sendKeepAlive() {
            if(isConnected) {
                // send keep-alive
                hub.server.keepAlive(currentPlayer.PlayerId, gameId, groupNameId);
            }
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
                <div class="panel-heading">Chat window</div>
                <div class="panel-body">
                    <div class="form-group">
                        <textarea id="txtChatWindow" name="txtChatWindow" class="form-control" rows="8" readonly></textarea>
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
                                <script type="text/javascript">
                                    $(document).ready(function() {
                                        // bind enter event to chat text box
                                        $("#txtChatMessage").bind("keypress", function (event) {
                                            // enter key pressed
                                            if (event.keyCode == 13) {
                                                sendChatMessage();

                                                return false;
                                            }
                                        });
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
