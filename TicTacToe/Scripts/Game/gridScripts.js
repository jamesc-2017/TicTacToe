
"use strict";

var Grid = (function () {
    var gridElement = document.getElementById("grid");
    var activePlayer = { PlayerNumber: 1, PlayerTeam: null, IsAi: null }; //game starts with player 1 active
    var teamImageHtmlBase = '<span class="verticalAlignHelper"></span><img src="../Images/{0}_transparent.png" />';
    var isLocked = false;
    var hasActiveMove = false;
    var cancellationToken = false;
    var cancellationCallback;

    function randomIntBetween(min, max) {
        return Math.floor(Math.random() * (max - min + 1) + min);
    }

    function initiateMove(gridSquareId) {
        //begins the move making process
        //gridSquareId is required for human moves, and should be UNDEFINED for AI moves

        startMoveChain();

        //initialize the active player
        if (activePlayer.PlayerNumber == 1) {
            activePlayer.PlayerTeam = String(Form.TeamP1Element.innerText).toUpperCase();
            activePlayer.IsAi = Boolean(Form.IsAiP1Element.checked);
        }
        else if (activePlayer.PlayerNumber == 2) {
            activePlayer.PlayerTeam = String(Form.TeamP2Element.innerText).toUpperCase();
            activePlayer.IsAi = Boolean(Form.IsAiP2Element.checked);
        }

        try {
            //input validation
            if (gridSquareId != undefined && gridSquareId != "A1" && gridSquareId != "A2" && gridSquareId != "A3"
                && gridSquareId != "B1" && gridSquareId != "B2" && gridSquareId != "B3"
                && gridSquareId != "C1" && gridSquareId != "C2" && gridSquareId != "C3") {
                throw "argument error";
            }
            else if ((activePlayer.PlayerTeam != "X" && activePlayer.PlayerTeam != "O")
                || (gridSquareId == undefined && !activePlayer.IsAi)
                //position is already occupied
                || (gridSquareId != undefined && document.getElementById(gridSquareId).innerHTML != "")) {
                throw "input error";
            }

            if (activePlayer.IsAi) {
                makeAiMove();
            }
            else {
                makeHumanMove(gridSquareId);
            }

        } catch (e) {
            endMoveChain();
        }
    };
    function makeHumanMove(gridSquareId) {
        //sends human move to the server, continuing on with post-move processing if successful

        var teamImageHtml = String(teamImageHtmlBase).replace("{0}", activePlayer.PlayerTeam.toLowerCase());

        $.ajax({type: "POST", url: "/TicTacToe/MakeHumanMove", dataType: "json",
            data: JSON.stringify({ position: String(gridSquareId) }),
            contentType: "application/json; charset=utf-8"
        }).done(function (response) {
            if (Boolean(cancellationToken)) {
                processCancellation();
            }
            else if (String(response.status).toUpperCase().indexOf("ERROR") >= 0) {
                endMoveChain();
            }
            else {
                doneCallback(response);
            }
        }).fail(function () {
            endMoveChain();
        });

        function doneCallback(response) {
            document.getElementById(gridSquareId).innerHTML = teamImageHtml;
            processGameState(response.gameStatus.Data);
        }
    };
    function makeAiMove() {
        //generates AI move, continuing on with post-move processing if successful

        var delayLowerBound = 300;  //should be no less than 250 (average human reaction time)
        var delayUpperBound = 1900;
        var teamImageHtml = String(teamImageHtmlBase).replace("{0}", activePlayer.PlayerTeam.toLowerCase());

        $.ajax({type: "POST", url: "/TicTacToe/MakeAiMove", dataType: "json"
        }).done(function (response) {
            if (Boolean(cancellationToken)) {
                processCancellation();
            }
            else if (String(response.status).toUpperCase().indexOf("ERROR") >= 0) {
                endMoveChain();
            }
            else {
                doneCallback(response);
            }
        }).fail(function () {
            endMoveChain();
        });

        function doneCallback(response) {
            //delay move for random interval to make AI look more realistic
            setTimeout(function () {
                if (Boolean(cancellationToken)) {
                    processCancellation();
                }
                else {
                    document.getElementById(response.position).innerHTML = teamImageHtml;
                    processGameState(response.gameStatus.Data);
                }
            }, randomIntBetween(delayLowerBound, delayUpperBound));
        }
    };

    function processGameState(gameStatus) {
        //processes the new game state on the UI, after a move is made

        if (String(gameStatus.gameStatus) == "WIN") {
            formatWinningSquares({ Team: gameStatus.winningTeam, WinningSquares: gameStatus.winningSquares });
            formatWinningPlayer({ PlayerNumber: gameStatus.winningPlayer, Score: gameStatus.winnersScore });
            startNewGame(gameStatus.nextActivePlayer);
        }
        else if (String(gameStatus.gameStatus) == "TIE") {
            formatWinningPlayer({ PlayerNumber: gameStatus.winningPlayer, Score: gameStatus.winnersScore });
            startNewGame(gameStatus.nextActivePlayer);
        }
        else {
            //game is still active
            setActivePlayer(gameStatus.nextActivePlayer);
        }
    };
    function formatWinningSquares(winner) {
        //formats the winning positions on the UI

        var winningSquares = String(winner.WinningSquares).split(",");
        var lightBlue = "#C8E1FF";
        var lightGreen = "#C8FFC8";
        var backgroundColor = (winner.Team == "X") ? lightBlue : lightGreen;

        for (var i = 0; i < winningSquares.length; i++) {
            document.getElementById(String(winningSquares[i])).style.backgroundColor = backgroundColor;
        }
    };
    function formatWinningPlayer(winner) {
        //formats the winning player on the UI

        var winningPlayer = winner.PlayerNumber;
        var winnersScore = String(winner.Score);

        if (winningPlayer == 1) {
            //prevents overflow for very large scores
            Form.ScoreP1Element.innerText = (winnersScore.length > 8) ? "You win" : winnersScore;
            Form.ScoreP1Element.style.fontWeight = "bold";
        }
        else if (winningPlayer == 2) {
            Form.ScoreP2Element.innerText = (winnersScore.length > 8) ? "You win" : winnersScore;
            Form.ScoreP2Element.style.fontWeight = "bold";
        }
        else {
            //tie
            Form.ScoreTiesElement.innerText = (winnersScore.length > 8) ? "Draw" : winnersScore;
            Form.ScoreTiesElement.style.fontWeight = "bold";
        }

        //removes bolding from active player swapping that occurred throughout the game
        Form.PlayerNameP1Element.style.fontWeight = "normal";
        Form.PlayerNameP2Element.style.fontWeight = "normal";
    };
    function startNewGame(startingPlayer) {
        //automatically starts a new game, after a brief delay

        var delay = 2000;

        setTimeout(function () {
            if (Boolean(cancellationToken)) {
                processCancellation();
            }
            else {
                resetGrid();
                //removes bolding that occurred on the winning move
                Form.ScoreP1Element.style.fontWeight = "normal";
                Form.ScoreP2Element.style.fontWeight = "normal";
                Form.ScoreTiesElement.style.fontWeight = "normal";
                setActivePlayer(startingPlayer);
            }
        }, delay);
    };
    function setActivePlayer(playerNumber) {
        //sets the player that is able to make the next move

        if (playerNumber == 1) {
            Form.PlayerNameP1Element.style.fontWeight = "bold";
            Form.PlayerNameP2Element.style.fontWeight = "normal";
        }
        else if (playerNumber == 2) {
            Form.PlayerNameP2Element.style.fontWeight = "bold";
            Form.PlayerNameP1Element.style.fontWeight = "normal";
        }

        activePlayer.PlayerNumber = playerNumber;
        activePlayer.PlayerTeam = null;
        activePlayer.IsAi = null;

        if (Boolean(cancellationToken)) {
            processCancellation();
        }
        else {
            //initiates a new move which will only continue if the active player is AI
            initiateMove();
        }
    };
    
    function startMoveChain() {
        //starts a move chain

        hasActiveMove = true;
        Form.LockForm();
        lockGrid();
    }
    function endMoveChain() {
        //ends an active move chain

        unlockGrid();
        hasActiveMove = false;
        if (Boolean(cancellationToken)) {
            processCancellation();
        }
    }
    function requestCancellation(callback) {
        //requests the active move chain be cancelled

        if (Boolean(hasActiveMove)) {
            cancellationToken = true;
            cancellationCallback = callback;
        }
        //if no active move chain in progress, call the cancellationCallback now
        else if (cancellationCallback) {
            cancellationCallback();
        }
    };
    function processCancellation() {
        //processes a pending cancellation request

        //set cancellationToken to false first, to prevent endless loop in endMoveChain call
        cancellationToken = false;
        //end move chain prior to cancellationCallback to ensure move is no longer "active"
        endMoveChain();
        if (cancellationCallback) {
            cancellationCallback();
            cancellationCallback = undefined;
        }
    };
    function getActiveMove() {
        //exposes the current value of hasActiveMove

        return hasActiveMove;
    }

    function lockGrid() {
        //disables the grid on the UI, to prevent new moves from being made

        var squares = gridElement.getElementsByTagName('div');

        //prevents processing if grid is already locked
        if (Boolean(isLocked)) {
            return;
        }

        for (var i = 0; i < squares.length; i++) {
            squares[i].style.pointerEvents = "none";
        }

        isLocked = true;
    };
    function unlockGrid() {
        //reactivate the grid on the UI, to allow new moves to be made

        var squares = gridElement.getElementsByTagName('div');

        //prevents processing if grid is already unlocked
        if (!Boolean(isLocked)) {
            return;
        }

        for (var i = 0; i < squares.length; i++) {
            squares[i].style.pointerEvents = "auto";
        }

        isLocked = false;
    };
    function resetGrid() {
        //clears the grid back to its default state

        var squares = gridElement.getElementsByTagName('div');

        for (var i = 0; i < squares.length; i++) {
            squares[i].innerHTML = "";
            squares[i].style.backgroundColor = "white";
        }

        //reset the activePlayer
        activePlayer.PlayerNumber = 1;
        activePlayer.PlayerTeam = null;
        activePlayer.IsAi = null;
    };

    return {
        InitiateMove: initiateMove,
        HasActiveMove: getActiveMove,
        RequestCancellation: requestCancellation,

        GridElement: gridElement,
        LockGrid: lockGrid,
        UnlockGrid: unlockGrid,
        ResetGrid: resetGrid
    };
}());