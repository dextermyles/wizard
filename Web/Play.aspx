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
        hub.client.receiveBid = function receiveBid(_player, bid, gameData) {
            // validate game data
            if(gameData.GameStateData == lastGameState)
                return;

            // update game state
            lastGameState = gameData.GameStateData;

            // get player div
            var $playerDiv = getPlayerDivByPlayerId(_player.PlayerId);

            // show tool tip
            showToolTip($playerDiv, _player.Name + " bid " + bid);

            // broadcast message
            appendChatMessage("Server", _player.Name + " bid " + bid);

            // delay start
            setTimeout(function() {
                // process game data
                processGameData(gameData);

                // start turn
                startTurn();
            }, 2000);
        };

        // trumpUpdated
        hub.client.trumpUpdated = function trumpUpdated(playerId, playerName, newTrumpCard) {
            // broadcast to chat
            appendChatMessage("Server", playerName + " has made " + getSuitName(newTrumpCard.Suit) + " trump!");

            // update trump graphic
            updateTrumpCardGraphic(newTrumpCard);
        };

        // cardPlayed
        hub.client.cardPlayed = function(_card, _player, isTurnEnded, _playerWinner, isRoundOver, gameData) {
            // validate game data
            if(gameData.GameStateData == lastGameState)
                return;

            // update last game state
            lastGameState = gameData.GameStateData;

            console.log("-- card played | isTurnEnded: " + isTurnEnded + " | isRoundOver: " + isRoundOver);

            // animate card played
            var $playerDiv = getPlayerDivByPlayerId(_player.PlayerId);
            var playerPosition = $playerDiv.offset();
            var cardPlayedFilename = getCardImagePath(_card);

            // get game board position
            var $cardsPlayedDiv = $(".cards-played-container");
            var targetLeft = ($cardsPlayedDiv.offset().left + ($cardsPlayedDiv.width() / 2));
            var targetTop = ($cardsPlayedDiv.offset().top);

            console.log("card played: ");
            console.log(_card);
            console.log("by player:");
            console.log(_player);

            // spawn card
            $("body").append("<a id='card-played' class='deal-card' style='position: absolute; left:" + playerPosition.left + "px; top:" + playerPosition.top + "px;'><img src='" + cardPlayedFilename + "' class='img-rounded card'  />");  
            
            console.log("spawned played card");
            console.log("animating played card to: left: " + targetLeft + " top: " + targetTop);

            // animate to card pile + remove card
            $("#card-played")
                .animate({
                    left: targetLeft + 'px',
                    top: targetTop + 'px',
                    opacity: 0.5
                }, 
                500, 
                function() {
                    // save html
                    var cardHtml = $("#card-played").html();

                    // fade/remove played card
                    $("#card-played").remove();

                    // append new card to pile
                    $(".cards-played").append(cardHtml);

                    console.log("animation complete | isRoundOver: " + isRoundOver + " | isTurnEnded: " + isTurnEnded);

                    // round ended
                    if(isRoundOver) {
                        // winner player div
                        var $playerWinnerDiv = getPlayerDivByPlayerId(_playerWinner.PlayerId);
                        var playerWinnerPosition = $playerWinnerDiv.offset();

                        // animate card pile to winner
                        $cardsPlayedDiv.children(".card").each(function(index) {
                            // card data
                            var $card = $(this);

                            // get card position
                            var cardPosition = {
                                left: $card.offset().left,
                                top: $card.offset().top
                            };

                            console.log(cardPosition);

                            // update card css
                            $card.css({
                                position: 'absolute',
                                left: cardPosition.left + 'px',
                                top: cardPosition.top + 'px'
                            });

                            $card.animate({
                                left: playerWinnerPosition.left + 'px',
                                top: playerWinnerPosition.top + 'px'
                            }, 1000, function() {
                                $(this).remove();
                            });
                        });

                        // show tool tip
                        showToolTip($playerWinnerDiv, "I won the trick!");

                        // delay card deal
                        setTimeout(function() {
                            // update game data
                            processGameData(gameData);

                            // deal cards
                            dealCards(lastGameState.Round);
                        }, 2000);   
                    }
                    else if(isTurnEnded) {
                        // winner player div
                        var $playerWinnerDiv = getPlayerDivByPlayerId(_playerWinner.PlayerId);
                        var playerWinnerPosition = $playerWinnerDiv.offset();

                        // animate card pile to winner
                        $cardsPlayedDiv.children(".card").each(function(index) {
                            // card data
                            var $card = $(this);

                            // get card position
                            var cardPosition = {
                                left: $card.offset().left,
                                top: $card.offset().top
                            };

                            console.log(cardPosition);

                            // update card css
                            $card.css({
                                position: 'absolute',
                                left: cardPosition.left + 'px',
                                top: cardPosition.top + 'px'
                            });

                            $card.animate({
                                left: playerWinnerPosition.left + 'px',
                                top: playerWinnerPosition.top + 'px'
                            }, 1000, function() {
                                $(this).remove();
                            });
                        });

                        // show tool tip
                        showToolTip($playerWinnerDiv, "I won the trick!"); 

                        // delay card deal
                        setTimeout(function() {
                            // update game data
                            processGameData(gameData);

                            // winning player leads next card
                            startTurn();
                        }, 2000); 
                    }
                    else {
                        // update game data
                        processGameData(gameData);

                        // delay turn start
                        setTimeout(function() {
                            // announce next players turn to act
                            startTurn();
                        }, 2000);
                    }
                });
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
                title: message,
                placement: 'top'
            });

            // show tool tip
            target.tooltip('show');

            // dont overlap modal
            target.css("z-index", "1");

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

        function getTotalBids() {
            var score_total = 0;

            for(var i = 0; i < lastGameState.Players.length; i++) {
                score_total += lastGameState.Players[i].Bid;
            }

            return score_total;
        }

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

            // update total rounds
            var total_rounds = (60 / players.length);

            $(".total-rounds").html(total_rounds);

            // update UI
            for(i = 0; i < players.length; i++) {
                // num players
                numPlayers++;

                // player container
                $playerDiv = $("#position-" + (i+1));

                // player object
                player = players[i];

                // update currentPlayer object
                if(player.PlayerId == currentPlayer.PlayerId) {
                    // update current player object
                    currentPlayer = player;
                }

                // update player name
                $playerDiv.children(".player-name").html(player.Name + " (" + player.TricksTaken + "/" + player.Bid + ")");
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
                    $(".trump").html("None");
                }
                else if(lastGameState.TrumpCard.Suit == suit.Wizard) {
                    // update trump value
                    $(".trump").html("Being chosen");
                }
                else {
                    // update trump value
                    $(".trump").html(getSuitName(lastGameState.TrumpCard.Suit));
                } 
            }
            
            console.log("clearing cards played");

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

            console.log("clearing player cards in hand");

            for(i = 0; i < currentPlayer.Cards.length; i++) {
                var card = currentPlayer.Cards[i];

                var suitName = getSuitName(card.Suit);
                var value = card.Value;
                var ownerPlayerId = card.OwnerPlayerId;
                var imageFileName = getCardImagePath(card);

                var style = "";

                if(i > 0)
                    style = "style='margin-left: -30px'";

                $playerCards.append("<a class='card' onclick='verifySelectedCard(this);' suit='" + card.Suit + "' value='" + card.Value + "' " + style + "><img src=\"" + imageFileName + "\" class=\"img-rounded\" /></a>");
            }

            // check if first hand
            if(pageJustLoaded) {
                // delay start game
                setTimeout(function() {
                    // deal cards
                    dealCards(lastGameState.Round);
                }, 2000);
            }

            // turn off flag
            pageJustLoaded = false;
        };

        function drawPlayerBidsHtml() {
            // draw player bids
            var playerBidsHtml = "<tr class=''>";
            
            // round
            playerBidsHtml += "<th class='text-center'>Round</th>";

            // player names
            for(var i = 0; i < lastGameState.Players.length; i ++) {
                var is_player_class = '';

                if(currentPlayer.PlayerId == lastGameState.Players[i].PlayerId) {
                    is_player_class = 'info';
                }

                playerBidsHtml += "<th class='text-center " + is_player_class + "'>" + lastGameState.Players[i].Name + " (" + lastGameState.Players[i].Score + ")</th>";
            }
            
            playerBidsHtml += "</tr>";
            playerBidsHtml += "<tr>";
            
            // round
            playerBidsHtml += "<td class='text-center'><strong>" + lastGameState.Round + "</strong></td>";

            // player bids
            for(var i = 0; i < lastGameState.Players.length; i ++) {
                var is_player_class = '';

                if(currentPlayer.PlayerId == lastGameState.Players[i].PlayerId) {
                    is_player_class = 'info';
                }

                playerBidsHtml += "<td class='text-center " + is_player_class + "'>" + lastGameState.Players[i].TricksTaken + "/" + lastGameState.Players[i].Bid + "</td>";
            }
  
            playerBidsHtml += "</tr>";

            return playerBidsHtml;
        }

        // select bid
        function selectBid(round) {
            // validate
            if(!currentPlayer.IsTurn) {
                logMessage("-- you must wait your turn --");

                return false;
            }

            console.log("selecting bid");

            // draw player bids
            var playerBidsHtml = drawPlayerBidsHtml();

            $(".player-bids").html(''); //clear player bids
            $(".player-bids").append(playerBidsHtml);

            // update total bids
            var total_bids = getTotalBids();

            $(".total-bids").html(total_bids);

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

            console.log("selecting card to play");

            // draw player bids
            var playerBidsHtml = drawPlayerBidsHtml();

            $(".player-bids").html(''); //clear player bids
            $(".player-bids").append(playerBidsHtml);

            // update total bids
            var total_bids = getTotalBids();

            $(".total-bids").html(total_bids);

            // first to act
            if(lastGameState.CardsPlayed == null) {
                $(".modal-cards-played").hide();
            }
            else {
                $(".modal-cards-played").show();
            }

            // show select card box
            $('#selectCardModal').modal('show');
        };

        function selectTrump() {
            // validate
            if(!currentPlayer.IsTurn) {
                logMessage("-- you must wait your turn --");

                return;
            }
                

            if(lastGameState.Status != gameStateStatus.SelectTrump) {
                logMessage("-- you cant select trump right now --");

                return;
            }

            // show select trump modal
            $('#selectTrumpModal').modal('show');
        };

        function verifyBid(bidValue) {
            // validate
            if(!currentPlayer.IsTurn) {
                logMessage("-- you must wait your turn --");

                return false;
            }

            if(lastGameState.Status != gameStateStatus.BiddingInProgress) {
                logMessage("-- you cant play a card right now --");

                return;
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

            if(lastGameState.Status != gameStateStatus.RoundInProgress) {
                logMessage("-- you cant play a card right now --");

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
                                    alert('You have to follow suit! Picked: ' + getSuitName(cardSuit) + ' - must be: ' + getSuitName(suitToFollow));

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

        
        function verifySelectedTrump(suitId) {
            // validate
            if(!currentPlayer.IsTurn) {
                return;
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

        function updateTrumpCardGraphic(_trumpCard) {
            // update trump card
            var trumpFileName = getCardImagePath(_trumpCard);

            $(".trump-card").children("img").attr("src", trumpFileName);
        };

        function getPlayerDivByPlayerId(_playerId) {
            if(lastGameState != null) {
                for(var i = 0; i < lastGameState.Players.length; i++) {
                    if(lastGameState.Players[i].PlayerId == _playerId) {
                        return $("#position-" + (i + 1));
                    }
                }
            }
        }

        // num cards to deal
        var numCardsToDeal = 0;
        var dealtCardIndex = 0;
        var targetPlayerIndex = 0;

        function dealCards() {
            console.log("dealing cards");

            // update dealing flag
            isDealing = true;

            // reset indexes
            dealtCardIndex = 0;
            targetPlayerIndex = lastGameState.DealerPositionIndex + 1;

            // get num remaining cards to deal
            numCardsToDeal = (lastGameState.Round * lastGameState.Players.length);

            // deal card every 1/4th second
            dealRemainingCards();
        };

        function dealRemainingCards() {
            // dealer data
            var dealerIndex = lastGameState.DealerPositionIndex;
            var $dealerDiv = $("#position-" + (dealerIndex + 1));
            var dealerPosition = $dealerDiv.offset();

            // offset position
            dealerPosition.left = (dealerPosition.left + ($dealerDiv.width() / 2));
            dealerPosition.top = (dealerPosition.top - 20);

            // validate
            if(numCardsToDeal < 0)
                numCardsToDeal = 0;

            console.log('num cards left to deal: ' + numCardsToDeal);
            
            // no cards left to deal - flip trump card and animate
            if(numCardsToDeal == 0) {
                // trump card data
                var $trumpDiv = $(".trump-card img");
                var trumpPosition = $trumpDiv.offset();
                var trumpCard = lastGameState.TrumpCard;
                var trumpFileName = getCardImagePath(trumpCard);

                // spawn trump card at dealer position
                $("body").append("<img id='deal-card-trump' src='" + trumpFileName + "' class='deal-card' style='position: absolute; left:" + dealerPosition.left + "px; top:" + dealerPosition.top + "px;' />");

                // animate trump card
                $("#deal-card-trump")
                    .animate({
                        left: trumpPosition.left + 'px',
                        top: trumpPosition.top + 'px',
                    }, 1000, function() {
                        // update trump card
                        updateTrumpCardGraphic(trumpCard);

                        // remove dealt cards
                        $(".deal-card").remove();

                        // disable dealing flag
                        isDealing = false;

                        // start turn
                        startTurn();
                    });

                return;
            }

            // validate target player index
            if(targetPlayerIndex > (lastGameState.Players.length - 1))
                targetPlayerIndex = 0;

            // target player data
            var $targetPlayerDiv = $("#position-" + (targetPlayerIndex + 1));
            var targetPosition = $targetPlayerDiv.offset();

            // spawn card at dealer location
            $("body").append("<img id='deal-card-" + dealtCardIndex + "' src='/Assets/Cards/deck_cover.png' class='deal-card' style='position: absolute; left:" + dealerPosition.left + "px; top:" + dealerPosition.top + "px;' />");
                 
            // animate dealt card
            $("#deal-card-" + dealtCardIndex)
                .animate({
                    left: targetPosition.left + 'px',
                    top: targetPosition.top + 'px'
                }, 250, function() {
                    // amimation comete - deal next card
                    dealRemainingCards();
                });

            // indexes
            targetPlayerIndex++;
            dealtCardIndex++;
            numCardsToDeal--;
        };       
    </script>
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <h1 class="game-info">Round: <span class="round-number">0</span> of <span class="total-rounds">0</span>
            <span class="pull-right">Trump:
                <span class="trump label label-danger" style="top: 0px;">Loading</span>
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
                        <div class="cards-played cards-played-container">
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
                        <div id="position-5" class="player-container">
                            <div class="label-info player-name">Player 5</div>
                            <img data-src="holder.js/64x64" class="img-circle profile-pic" />
                            <div class="player-score">0 points</div>
                        </div>
                    </td>
                    <td>
                        <div id="position-4" class="player-container">
                            <div class="label-info player-name">Player 4</div>
                            <img data-src="holder.js/64x64" class="img-circle profile-pic" />
                            <div class="player-score">0 points</div>
                        </div>
                    </td>
                    <td>&nbsp;</td>
                </tr>
            </table>
        </div>
        <div class="card-holder" style="min-width: 500px; margin-top: 10px;">
            <div class="player-cards well well-sm col-xs-9" style="min-height: 126px;">
            </div>
            <div class="trump-card-container well well-sm col-xs-offset-2 text-center" style="min-height: 126px;">
                <a class="card trump-card">
                    <img src="Assets/Cards/deck_cover.png" class="img-rounded" alt="trump card" />
                </a>
            </div>
        </div>
        <div class="clearfix"></div>
    </div>
    <hr />
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
    <hr />
    <div class="modal" id="selectBidModal" tabindex="-1" role="dialog" aria-labelledby="selectBidModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title" id="selectBidModalLabel">
                        Your turn to bid!
                        <span class="pull-right">Trump:
                            <span class="trump label label-danger" style="top: 0px;">Loading</span>
                        </span>
                    </h4>
                </div>
                <div class="modal-body">
                    <div class="form-group">
                        <div class="panel panel-default">
                            <div class="panel-heading"><strong>Player bids - <span class="total-bids">0</span> of <span class="round-number">0</span></strong></div>
                            <table class="table table-responsive player-bids"></table>
                        </div>
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
                    <h4 class="modal-title" id="selectCardModalLabel">
                        Your turn to play a card!
                        <span class="pull-right">Trump:
                            <span class="trump label label-danger" style="top: 0px;">Loading</span>
                        </span>
                    </h4>
                </div>
                <div class="modal-body">
                    <div class="form-group">
                        <div class="panel panel-default">
                            <div class="panel-heading"><strong>Player bids - <span class="total-bids">0</span> of <span class="round-number">0</span></strong></div>
                            <table class="table table-responsive player-bids"></table>
                        </div>
                    </div>
                    <div class="modal-cards-played">
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
