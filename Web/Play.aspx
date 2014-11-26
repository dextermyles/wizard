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
            hub.server.joinGame(playerId, gameLobbyId, groupNameId)
                .done(function () {
                    logMessage("-- joinGame executed on server --");
                })
                .fail(function (msg) {
                    logMessage("-- " + msg + " --");
                });
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
        <div class="game-board">
            <table class="table">
                <tr>
                    <td></td>
                    <td>Player 1</td>
                    <td>Player 2</td>
                    <td></td>
                </tr>
                <tr>
                    <td>Player 6</td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                    <td>Player 3</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td>Player 5</td>
                    <td>Player 4</td>
                    <td>&nbsp;</td>
                </tr>
            </table>
        </div>
        <style type="text/css">
            .game-board {
                background-image: url('/assets/table/table-default.png');
                background-repeat:no-repeat;
                background-size:100% 100%;
            }
        </style>
    </div>
</asp:Content>
