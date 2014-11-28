<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Play.aspx.cs" Inherits="WizardGame.Play" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        // current player object
        var currentPlayer = new function () {
            this.PlayerId = 0,
            this.Name = "",
            this.PictureURL = "",
            this.UserId = 0,
            this.ConnectionId = ""
            this.IsTurn = false,
            this.isDealer = false
        };

        // game state status enum
        var gameStateStatus = {
            DealInProgress: 0,
            BiddingInProgress: 1,
            RoundInProgress: 2,
            Setup: 3,
            Finished:4,
            TurnEnded: 5
        };

        // card suits
        var suit = {
            Spades: 0,
            Hearts: 1,
            Clubs: 2,
            Diamonds: 3,
            Fluff: 4,
            Wizard: 5
        };

        // get suit name by value/id
        function getSuitName(suitId) {
            switch(suitId) {
                case suit.Spades:
                    return "Spades";
                case suit.Hearts:
                    return "Hearts";
                case suit.Clubs:
                    return "Clubs";
                case suit.Diamonds:
                    return "Diamonds";
                case suit.Fluff:
                    return "Fluff";
                case suit.Wizard:
                    return "Wizard";
                default:
                    return "Unknown";
            }
        };

        // get card image path
        function getCardImagePath(card) {
            if(card == null)
                return "";

            var suitName = getSuitName(card.Suit);
            var value = card.Value;
            var imageFileName = suitName.toLowerCase() + "_" + value + ".png";

            if(card.Suit == suit.Wizard)
                imageFileName = "wizard.png";

            if(card.Suit == suit.Fluff)
                imageFileName = "fluff.png";

            return "/Assets/Cards/" + imageFileName;
        };

        // keep track of last game state
        var lastGameState = null;

        // playerList array
        var playerList = Array();

        // playerCards array
        var playerCards = Array();

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
            joinGame(currentPlayer.PlayerId, groupNameId);

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

            processGameData(gameData);
        };

        // receiveBid
        hub.client.receiveBid = function receiveBid(playerId, playerName, bid) {
            var $playerDiv = $("#position-" + playerId);

            showToolTip($playerDiv, "I bid " + bid);

            appendChatMessage("Server", playerName + " bid " + bid);
        };

        // playerWonTrick
        hub.client.playerWonTrick = function playerWonTrick(playerId, playerName, card) {
            appendChatMessage("Server", playerName + " won the trick with a " + card);
        };

        // roundEnded
        hub.client.roundEnded = function() {
            logMessage("-- round ended --");

            var $cardsPlayed = $(".cards-played");

            // clear cards played
            $cardsPlayed.html('');

            appendChatMessage("Server", "Round has ended");
        };

        // gameEnded
        hub.client.gameEnded = function() {
            appendChatMessage("Server", "Game has ended");
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

        function showToolTip(target, message) {
            // configure tooltip
            target.tooltip({
                title: message
            });

            // show tool tip
            target.tooltip('show');

            // destroy it after delay
            setTimeout(function() {
                target.tooltip('destroy');
            }, 3000);
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

        function processGameData(gameData) {
            // update game data
            lastGameState = gameData.GameStateData;

            console.log("Last game state:");
            console.log(lastGameState);

            // get vars
            var players = lastGameState.Players;
            var deck = lastGameState.Deck;
            var round = lastGameState.Round;
            var status = lastGameState.Status;
            var cardsPlayed = lastGameState.CardsPlayed;
            var dealerPositionIndex = lastGameState.DealerPositionIndex;
            var playerTurnIndex = lastGameState.PlayerTurnIndex;
            var i = 0;

            // log data
            console.log(players);
            console.log(deck);
            console.log("Round: " + round);
            console.log("Status: " + status);
            console.log(cardsPlayed);
            console.log("Dealer: " + players[dealerPositionIndex].Name);
            console.log("Player turn: " + players[playerTurnIndex].Name);

            // update local variables
            playerList = players;

            // update UI
            for(i = 0; i < players.length; i++) {
                // update names
                var $playerDiv = $("#position-" + (i+1));
                var player = players[i];

                // update player name
                $playerDiv.children("span").html(player.Name);

                // remove labels
                $playerDiv.children("span").removeClass("label-danger");
                $playerDiv.children("span").removeClass("label-info");

                // remove borders
                $(".profile-pic").css("border", "1px solid #000");

                // add special label for dealer
                if(player.IsDealer) {
                    $playerDiv.children("span").addClass("label-danger");
                }
                else {
                    $playerDiv.children("span").addClass("label-info");
                }

                // add special label for players turn
                if(player.IsTurn) {
                    // apply border to profile pic
                    $playerDiv.children(".profile-pic").css("border", "2px solid #ff0000");

                    // server msg
                    appendChatMessage("Server", player.Name + "'s turn to bid");

                    // only show tool tip for other players
                    if(currentPlayer.PlayerId != player.PlayerId) {
                        // show tool tip
                        showToolTip($playerDiv, "My turn to bid!");
                    }
                }

                // get player data for current user
                if(player.PlayerId == currentPlayer.PlayerId) {
                    // update current player object
                    currentPlayer = player;

                    console.log("updated player object");
                    console.log(currentPlayer);
                }
            } 

            // update trump
            if(lastGameState.TrumpCard != null) {
                // announce trump
                appendChatMessage("Server", "Trump is " + getSuitName(lastGameState.TrumpCard.Suit));
            }
            
            // draw cards played in middle
            var $cardsPlayed = $(".cards-played");

            // clear cards on table
            $cardsPlayed.html('');

            if(lastGameState.CardsPlayed != null && lastGameState.CardsPlayed.length > 0) {
                for(i = 0; i < lastGameState.CardsPlayed.length; i++) {
                    var card = lastGameState.CardsPlayed[i];

                    var suitName = getSuitName(card.Suit);
                    var value = card.Value;
                    var ownerPlayerId = card.OwnerPlayerId;
                    var imageFileName = getCardImagePath(card);

                    $cardsPlayed.append("<a class='img-rounded card'><img src=\"" + imageFileName + "\" /></a>");
                }
            }

            // draw player cards
            var $playerCards = $(".player-cards");

            // clear existing cards
            $playerCards.html('');

            for(i = 0; i < currentPlayer.Cards.length; i++) {
                var card = currentPlayer.Cards[i];

                var suitName = getSuitName(card.Suit);
                var value = card.Value;
                var ownerPlayerId = card.OwnerPlayerId;
                var imageFileName = getCardImagePath(card);

                $playerCards.append("<a class='card' onclick='verifySelectedCard(this);' suit='" + card.Suit + "' value='" + card.Value + "'><img src=\"" + imageFileName + "\" class=\"img-rounded\" /></a>");
            }

            // check if current player turn
            if(currentPlayer.IsTurn) {
                if(status == gameStateStatus.BiddingInProgress) {
                    // select bid
                    selectBid(round);
                }

                if(status == gameStateStatus.RoundInProgress) {
                    // select card to play
                    selectCard();
                }
            }


        };

        // select bid
        function selectBid(round) {
            // reset bid value
            $("#txtPlayerBid").val('0');

            // generate bid # buttons
            $playerBid = $(".player-bid");

            // erase buttons
            $playerBid.html('');

            // add new button based on round #
            for(var i = 0; i <= round; i++) {
                $playerBid.append("<a onclick=\"updateBidField(this);\" class=\"btn btn-lg btn-default\">" + i + "</a>");
            }

            // show bid box
            $('#selectBidModal').modal('show');
        };

        // select card to play
        function selectCard() {
            logMessage("-- select a card to play --");

            // first to act
            if(lastGameState.CardsPlayed == null) {
                $(".first-bid-info").show();
            }
            else {
                $(".first-bid-info").hide();
            }

            // show select card box
            $('#selectCardModal').modal('show');
        };

        function updateBidField(buttonPressed) {
            // get bid value
            var bidValue = $(buttonPressed).html();

            // update player bid value
            $("#txtPlayerBid").val(bidValue);
        };

        function verifyBid() {
            var bidValue = parseInt($("#txtPlayerBid").val());

            if(bidValue != NaN) {
                $('#selectBidModal').modal('hide');

                if(isConnected) {
                    logMessage("-- enter bid --");

                    hub.server.enterBid(gameId, currentPlayer.PlayerId, bidValue, groupNameId)
                        .done(function() {
                            logMessage("-- enter bid executed on server --");
                        });
                }
            } 
        };

        function verifySelectedCard(selectedCard) {
            var $card = $(selectedCard);
            var cardSuit = parseInt($card.attr("suit"));
            var cardValue = parseInt($card.attr("value"));

            $('#selectCardModal').modal('hide');

            logMessage("-- selected card: " + cardValue + " of " + getSuitName(cardSuit) + " (" + cardSuit + ")");

            var cardObject = {
                OwnerPlayerId: currentPlayer.PlayerId,
                Suit: cardSuit,
                Value: cardValue
            };

            if(isConnected) {
                hub.server.playCard(gameId, currentPlayer.PlayerId, cardObject, groupNameId);
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
                            <div id="position-1">
                                <img data-src="holder.js/64x64" class="img-circle profile-pic" />
                                <span class="label label-info">Player 1</span>
                            </div>
                        </td>
                        <td>
                            <div id="position-2">
                                <span class="label label-info">Player 2</span>
                                <img data-src="holder.js/64x64" class="img-circle profile-pic" />
                            </div>
                        </td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>
                            <div id="position-6">
                                <img data-src="holder.js/64x64" class="img-circle profile-pic" />
                                <span class="label label-info">Player 6</span>
                            </div>
                        </td>
                        <td colspan="2">
                            <div class="cards-played">
                                <!-- place holder for cards played -->
                            </div>
                        </td>
                        <td>
                            <div id="position-3">
                                <span class="label label-info">Player 3</span>
                                <img data-src="holder.js/64x64" class="img-circle profile-pic" />
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td>
                            <div id="position-4">
                                <img data-src="holder.js/64x64" class="img-circle profile-pic" />
                                <span class="label label-info">Player 4</span>
                            </div>
                        </td>
                        <td>
                            <div id="position-5">
                                <span class="label label-info">Player 5</span>
                                <img data-src="holder.js/64x64" class="img-circle profile-pic" />
                            </div>
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
            <div class="player-cards">

            </div>
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
        <div class="modal" id="selectBidModal" tabindex="-1" role="dialog" aria-labelledby="selectBidModalLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title" id="selectBidModalLabel">Enter your bid</h4>
                    </div>
                    <div class="modal-body">
                        <div class="form-group">
                            <label>Your cards:</label>
                            <div class="player-cards"></div>
                        </div>
                        
                        <div class="form-group">
                            <label>Your bid:</label>
                            <input type="text" id="txtPlayerBid" class="form-control"  readonly />
                        </div>
                        <div class="player-bid">
                            <!-- placeholder for buttons -->
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" onclick="verifyBid(); return false;">Enter my bid</button>
                    </div>
                </div>
            </div>
        </div>
        <div class="modal" id="selectCardModal" tabindex="-1" role="dialog" aria-labelledby="selectCardModalLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title" id="selectCardModalLabel">Its your turn! Select a card to play</h4>
                    </div>
                    <div class="modal-body">
                        <div class="alert alert-info first-bid-info">
                            <span class="glyphicon glyphicon-info-sign"></span>
                            <strong>Remember!</strong>
                            You are first to act. Other players will have to follow suit!
                        </div>
                        <div class="form-group">
                            <label>Cards played:</label>
                            <div class="cards-played"></div>
                        </div>
                        <div class="form-group">
                            <label>Select a card to play:</label>
                            <div class="player-cards"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <script>
            $(document).ready(function() {
                $(".modal").modal({
                    backdrop: 'static',
                    keyboard: false,
                    show: false
                });
            });
        </script>
    </div>
</asp:Content>
