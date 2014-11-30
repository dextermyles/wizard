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
            this.IsDealer = false,
            this.IsLastToAct = false
            this.Score = 0;
        };

        // game state status enum
        var gameStateStatus = {
            DealInProgress: 0,
            BiddingInProgress: 1,
            RoundInProgress: 2,
            Setup: 3,
            Finished:4,
            TurnEnded: 5,
            SelectTrump: 6
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

        // keep alive interval
        var keepAliveInterval = 0;

        // is first hand
        var isFirstHand = true;

        // is deal animation in progress
        var isDealing = false;

        // player just connected
        var pageJustLoaded = true;

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
            // update game data
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

        // trumpUpdated
        hub.client.trumpUpdated = function trumpUpdated(playerId, playerName, newSuite) {
            appendChatMessage("Server", playerName + " has made " + getSuitName(newSuite) + " trump!");
        };

        // roundEnded
        hub.client.roundEnded = function(dealerPlayerName, firstPlayerName, roundNumber, trumpCard) {
            // update first hand flag
            isFirstHand = false;

            logMessage("-- round ended --");

            var $cardsPlayed = $(".cards-played");

            // clear cards played
            $cardsPlayed.html('');

            appendChatMessage("Server", "Round #" + roundNumber + " has begin. " + dealerPlayerName + " is now dealing!");

            // announce trump
            if(trumpCard != null) {
                // annouce dealer is choosing trump
                if(trumpCard.Suit == suit.Wizard) {
                    appendChatMessage("Server", dealerPlayerName + " turned a Wizard and is choosing trump!");
                }
                else if(trumpCard.Suit == suit.Fluff) {
                    appendChatMessage("Server", dealerPlayerName + " turned a Fluff. There is no Trump this round!");
                }
                else {
                    appendChatMessage("Server", dealerPlayerName + " turned a " + getSuitName(trumpCard.Suit) + "!");
                }
            }
            else {
                appendChatMessage("Server", "Last round! There is no trump.");
            }
            
            appendChatMessage("Server", firstPlayerName + " is first to act!");

            // animate card deal
            dealCards(roundNumber);

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
            // delay tool tip by half second
            setTimeout(function() {
                // configure tooltip
                target.tooltip({
                    title: message,
                    placement: 'top'
                });

                // show tool tip
                target.tooltip('show');

                // destroy it after delay
                setTimeout(function() {
                    target.tooltip('destroy');
                }, 3000);
            }, 500);
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
            var numPlayers = 0;
            var $playerDiv;
            var player;


            // update local variables
            playerList = players;

            // upodate round #
            $(".round-number").html(round);

            // update UI
            for(i = 0; i < players.length; i++) {
                // num players
                numPlayers++;

                // player container
                $playerDiv = $("#position-" + (i+1));

                // player object
                player = players[i];

                // update player name
                $playerDiv.children(".player-name").html(player.Name);
                $playerDiv.children(".player-score").html(player.Score + " points");

                // remove labels
                $playerDiv.children(".player-name").removeClass("label-danger");
                $playerDiv.children(".player-name").removeClass("label-info");

                // default border
                $(".profile-pic").css("border", "1px solid #000");

                // add special label for players turn
                if(player.IsTurn) {
                    // change background of player name when its their turn
                    $playerDiv.children(".player-name").addClass("label-danger");

                    // only show tool tip for other players
                    if(currentPlayer.PlayerId != player.PlayerId) {
                        var message = '';

                        // construct announcement message
                        if(lastGameState.Status == gameStateStatus.BiddingInProgress) {
                            message = player.Name + "'s turn to bid"; 
                        }
                        else if(lastGameState.Status == gameStateStatus.RoundInProgress) {
                            message = player.Name + "'s turn to play a card";
                        }
                        else if(lastGameState.Status == gameStateStatus.SelectTrump) {
                            message = player.Name + "'s turn to choose trump";
                        }
                        else {
                            message = player.Name + "'s turn!";
                        }

                        // announce via tool tip
                        showToolTip($playerDiv, message);

                        // announce to chat window
                        appendChatMessage("Server", message);
                    }
                }
                else {
                    $playerDiv.children(".player-name").addClass("label-info");
                }

                // get player data for current user
                if(player.PlayerId == currentPlayer.PlayerId) {
                    // update current player object
                    currentPlayer = player;

                    console.log("updated player object");
                    console.log(currentPlayer);
                }
            } 

            // update un-used player containers
            for(i = numPlayers; i < 6; i++) {
                $playerDiv = $("#position-" + (i + 1)); 
                $playerDiv.children(".player-name").html('Empty seat');
                $playerDiv.css('opacity', '0.15');
            };

            // update trump
            if(lastGameState.TrumpCard != null) {
                // determine trump
                if(lastGameState.TrumpCard.Suit == suit.Fluff) {
                    $(".trump").html("No trump");
                }
                else if(lastGameState.TrumpCard.Suit == suit.Wizard) {
                    // update trump value
                    $(".trump").html("Trump TBD");
                }
                else {
                    // update trump value
                    $(".trump").html(getSuitName(lastGameState.TrumpCard.Suit));
                } 
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

            // check if first hand
            if(pageJustLoaded) {
                // show deal animation
                dealCards(lastGameState.Round);
            }

            // turn off flag
            pageJustLoaded = false;
        };

        // select bid
        function selectBid(round) {
            // validate
            if(!currentPlayer.IsTurn) {
                logMessage("-- you must wait your turn --");

                return false;
            }

            // reset bid value
            $("#txtPlayerBid").val('0');

            // generate bid # buttons
            $playerBid = $(".player-bid");

            // erase buttons
            $playerBid.html('');

            // add new button based on round #
            for(var i = 0; i <= round; i++) {
                $playerBid.append("<a onclick=\"verifyBid(" + i + ");\" class=\"btn btn-lg btn-default\">" + i + "</a>&nbsp;");
            }

            // show bid box
            $('#selectBidModal').modal('show');
        };

        // select card to play
        function selectCard() {
            // validate
            if(!currentPlayer.IsTurn) {
                logMessage("-- you must wait your turn --");

                return false;
            }

            // first to act
            if(lastGameState.CardsPlayed == null) {
                $(".first-bid-info").show();
                $(".modal-cards-played").hide();
                $(".regular-bid-info").hide();
            }
            else {
                $(".first-bid-info").hide();
                $(".modal-cards-played").show();
                $(".regular-bid-info").show();
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

        function verifyBid(bidValue) {
            // validate
            if(!currentPlayer.IsTurn) {
                logMessage("-- you must wait your turn --");

                return false;
            }

            if(bidValue != null && bidValue != NaN) {
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
            // validate
            if(!currentPlayer.IsTurn) {
                logMessage("-- you must wait your turn --");

                return;
            }

            var $card = $(selectedCard);
            var cardSuit = parseInt($card.attr("suit"));
            var cardValue = parseInt($card.attr("value"));

            // check that player is following suit (except when playing fluff or wizard)
            if(cardSuit != suit.Fluff && cardSuit != suit.Wizard) {
                if(lastGameState.CardsPlayed != null && lastGameState.CardsPlayed.length > 0) {
                    var suitToFollow = null;

                    // if wizard is led, then allow any card to be played
                    if(lastGameState.CardsPlayed[0].Suit != suit.Wizard) {
                        // loop through cards played
                        for(var i = 0; i < lastGameState.CardsPlayed.length; i++) {
                            // get suit to follow from first non fluff card
                            if(lastGameState.CardsPlayed[i].Suit != suit.Fluff) {
                                // get suit from first played card
                                suitToFollow = lastGameState.CardsPlayed[i].Suit;

                                break;
                            }
                        }

                        // alert player to follow suit
                        if(suitToFollow != null && cardSuit != suitToFollow) {
                            // check that player can follow suit
                            for(var i = 0; i < currentPlayer.Cards.length; i++) {
                                if(currentPlayer.Cards[i].Suit == suitToFollow) {
                                    alert('You have to follow suit! Picked: ' + cardSuit + ' - should be: ' + suitToFollow);

                                    return;
                                }
                            } 
                        }
                    }
                }
            }
            

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

        function selectTrump() {
            // validate
            if(currentPlayer.IsTurn == false && lastGameState.Status != gameStateStatus.SelectTrump)
                return;

            // show select trump modal
            $('#selectTrumpModal').modal('show');
        };

        function verifySelectedTrump(suitId) {
            // validate
            if(!currentPlayer.IsTurn) {
                return false;
            }

            // hide select trump modal
            $('#selectTrumpModal').modal('hide');

            if(isConnected) {
                hub.server.setTrump(gameId, currentPlayer.PlayerId, suitId, groupNameId);
            }
        };

        function startTurn() {
            // log
            logMessage("-- start turn: " + currentPlayer.IsTurn + " --");

            // check if current player turn
            if(currentPlayer.IsTurn) {
                // select trump
                if(lastGameState.Status == gameStateStatus.SelectTrump) {
                    // select trump
                    selectTrump();
                }
                // enter bid
                if(lastGameState.Status == gameStateStatus.BiddingInProgress) {
                    // select bid
                    selectBid(lastGameState.Round); 
                }
                // play card
                if(lastGameState.Status == gameStateStatus.RoundInProgress) {
                    // select card to play
                    selectCard();
                }
            }
        };

        var isLastCard = false;

        function dealCards(roundNumber) {
            var dealerIndex = lastGameState.DealerPositionIndex;
            var $dealerDiv = $("#position-" + (dealerIndex + 1));
            var dealerPosition = $dealerDiv.offset();
            var numCardsToDeal = parseInt((roundNumber * lastGameState.Players.length));
            var currentIndex = dealerIndex + 1;
            var cardIndex = 0;

            if(currentIndex > lastGameState.Players.length -1)
                currentIndex = 0;

            logMessage("-- deal " + numCardsToDeal + " cards --");

            for(var x = 0; x < numCardsToDeal; x++) {
                // switch flag
                isDealing = true;

                // get target div
                console.log("#position-" + (currentIndex + 1));

                var $targetDiv = $("#position-" + (currentIndex + 1));
                var targetPosition = $targetDiv.offset();

                console.log("target position for card to deal:");
                console.log(targetPosition);

                // increment index
                currentIndex++;

                // reset to zero
                if(currentIndex > lastGameState.Players.length)
                    currentIndex = 0;

                // animate card from dealer to target player
                $("body").append("<img id='deal-card-" + x + "' src='/Assets/Cards/deck_cover.png' class='deal-card' style='position: absolute; left:" + dealerPosition.left + "px; top:" + dealerPosition.top + "px;' />").animate({
                    left: targetPosition.left + 'px',
                    top: targetPosition.top + 'px'
                }, 250, function() {
                    // if is last card to deal
                    if(isLastCard) {
                        // switch flag
                        isDealing = false;

                        // remove card
                        //$(".deal-card").remove();

                        // start turn
                        startTurn();
                    }

                    // increment card index
                    cardIndex++;
                }); 

                if(x == (numCardsToDeal - 1))
                    isLastCard = true;
            }
        };
    </script>
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <h1 class="game-info">Round: <span class="round-number">0</span>
            <span class="pull-right">Trump:
                <span class="trump label label-danger" style="top:0px;">Loading</span>
            </span>
        </h1>
        <div class="game-board">
            <table class="game-board-table">
                <tr>
                    <td>&nbsp;</td>
                    <td>
                        <div id="position-1" class="player-container">
                            <div class="label-info player-name">Player 1</div>
                            <img data-src="holder.js/64x64" class="img-circle profile-pic" />
                            <div class="player-score">0 points</div>
                        </div>
                    </td>
                    <td>
                        <div id="position-2" class="player-container">
                            <div class="label-info player-name">Player 2</div>
                            <img data-src="holder.js/64x64" class="img-circle profile-pic" />
                            <div class="player-score">0 points</div>
                        </div>
                    </td>
                    <td></td>
                </tr>
                <tr>
                    <td>
                        <div id="position-6" class="player-container">
                            <div class="label-info player-name">Player 6</div>
                            <img data-src="holder.js/64x64" class="img-circle profile-pic" />
                            <div class="player-score">0 points</div>
                        </div>
                    </td>
                    <td colspan="2">
                        <div class="cards-played">
                            <!-- place holder for cards played -->
                        </div>
                    </td>
                    <td>
                        <div id="position-3" class="player-container">
                            <div class="label-info player-name">Player 3</div>
                            <img data-src="holder.js/64x64" class="img-circle profile-pic" />
                            <div class="player-score">0 points</div>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td>
                        <div id="position-4" class="player-container">
                            <div class="label-info player-name">Player 4</div>
                            <img data-src="holder.js/64x64" class="img-circle profile-pic" />
                            <div class="player-score">0 points</div>
                        </div>
                    </td>
                    <td>
                        <div id="position-5" class="player-container">
                            <div class="label-info player-name">Player 5</div>
                            <img data-src="holder.js/64x64" class="img-circle profile-pic" />
                            <div class="player-score">0 points</div>
                        </div>
                    </td>
                    <td>&nbsp;</td>
                </tr>
            </table>
        </div>
        <style type="text/css">
                
            </style>
        <hr />
        <div class="player-cards well well-sm"></div>
        <hr />
    </div>
    <div class="container">
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
                    <div class="alert alert-info first-bid-info">
                        <p><span class="glyphicon glyphicon-info-sign"></span> <strong>Your turn to bid!</strong></p>
                        <p>Select the amount of Tricks you intend to win. Trump is currently <span class="trump" style="padding: 0; color: #f00; font-weight: bold; font-size: 14px;"></span>.</p>
                    </div>
                    <div class="form-group">
                        <label>Your cards:</label>
                        <div class="player-cards well well-sm"></div>
                    </div>
                    <div>
                        <label>Select your bid:</label>
                        <div class="player-bid">
                            <!-- placeholder for buttons -->
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="selectCardModal" tabindex="-1" role="dialog" aria-labelledby="selectCardModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title" id="selectCardModalLabel">Play a card</h4>
                </div>
                <div class="modal-body">
                    <div class="alert alert-info first-bid-info">
                        <p><span class="glyphicon glyphicon-info-sign"></span> <strong>Your are first to act!</strong></p>
                        <p>Trump is currently <span class="trump" style="padding: 0; color: #f00; font-weight: bold; font-size: 14px;"></span>. Other players have to try and follow suit with the card you lead with.</p>
                    </div>
                    <div class="alert alert-info regular-bid-info">
                        <p><span class="glyphicon glyphicon-info-sign"></span> <strong>Your turn to play a card!</strong></p>
                        <p>Trump is currently <span class="trump" style="padding: 0; color: #f00; font-weight: bold; font-size: 14px;"></span>. You have to follow suit with the first card led. If you can't follow suit, you may play any card.</p>
                    </div>
                    <div class="form-group modal-cards-played">
                        <label>Cards played:</label>
                        <div class="cards-played well well-sm"></div>
                    </div>
                    <div>
                        <label>Select a card to play:</label>
                        <div class="player-cards well well-sm"></div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="selectTrumpModal" tabindex="-1" role="dialog" aria-labelledby="selectTrumpModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title" id="selectTrumpModalLabel">Please select Trump for this round</h4>
                </div>
                <div class="modal-body">
                    <div class="alert alert-info first-bid-info">
                        <span class="glyphicon glyphicon-info-sign"></span>
                        <strong>Dealer turned a Wizard!</strong>
                        You get to select trump for this round
                    </div>
                    <div class="form-group">
                        <label>Your cards</label>
                        <div class="player-cards well well-sm"></div>
                    </div>
                    <div>
                        <a class="btn btn-default btn-lg btn-block" onclick="verifySelectedTrump(suit.Spades);">Spades
                        </a>
                        <a class="btn btn-default btn-lg btn-block" onclick="verifySelectedTrump(suit.Clubs);">Clubs
                        </a>
                        <a class="btn btn-default btn-lg btn-block" onclick="verifySelectedTrump(suit.Hearts);">Hearts
                        </a>
                        <a class="btn btn-default btn-lg btn-block" onclick="verifySelectedTrump(suit.Diamonds);">Diamonds
                        </a>
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
