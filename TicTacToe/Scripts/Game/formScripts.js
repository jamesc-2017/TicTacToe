
"use strict";

var Form = (function () {
    var playerNameP1Element = document.getElementById("playerName_p1");
    var playerNameP2Element = document.getElementById("playerName_p2");
    var outerTeamP1Element = document.getElementById("outerTeam_p1");
    var outerTeamP2Element = document.getElementById("outerTeam_p2");
    var teamP1Element = document.getElementById("team_p1");
    var teamP2Element = document.getElementById("team_p2");
    var scoreP1Element = document.getElementById("score_p1");
    var scoreP2Element = document.getElementById("score_p2");
    var scoreTiesElement = document.getElementById("score_ties");
    var isAiP1Element = document.getElementById("isAi_p1");
    var isAiP2Element = document.getElementById("isAi_p2");
    var inputButtonElement = document.getElementById("inputButton");
    var submitCallback = submitReset;
    var isLocked = false;

    function swapTeam() {
        //swaps the teams of the two players

        var currentTeam_p1 = teamP1Element.innerText.toString();
        var currentTeam_p2 = teamP2Element.innerText.toString();
        var isAi_p1 = isAiP1Element.checked;
        var isAi_p2 = isAiP2Element.checked;
        var newTeam_p1;
        var newTeam_p2;

        //input validation
        if ((currentTeam_p1 == "X" || currentTeam_p1 == "O") && (currentTeam_p2 == "X" || currentTeam_p2 == "O")
            && (currentTeam_p1 != currentTeam_p2)) {
            newTeam_p1 = currentTeam_p2;
            newTeam_p2 = currentTeam_p1;
        }
        else {
            //validation failure.  reassign to default values
            newTeam_p1 = "X";
            newTeam_p2 = "O";
        }

        teamP1Element.innerText = newTeam_p1;
        teamP2Element.innerText = newTeam_p2;

        configurePlayers(String(isAi_p1).toUpperCase(), newTeam_p1
            , String(isAi_p2).toUpperCase(), newTeam_p2);
    };
    function swapAiStatus() {
        //toggles between AI and human players

        var team_p1 = teamP1Element.innerText.toString().toUpperCase();
        var team_p2 = teamP2Element.innerText.toString().toUpperCase();
        var isAi_p1 = isAiP1Element.checked;
        var isAi_p2 = isAiP2Element.checked;

        if (Boolean(isAi_p1)) {
            setButtonFunctionality("START");
            Grid.LockGrid();
        }
        else {
            setButtonFunctionality("RESET");
            Grid.UnlockGrid();
        }

        configurePlayers(String(isAi_p1).toUpperCase(), team_p1
            , String(isAi_p2).toUpperCase(), team_p2);
    };
    function configurePlayers(isAi_p1, team_p1, isAi_p2, team_p2) {
        //syncs the server with the UI

        $.ajax({type: "POST",url: "/TicTacToe/ConfigurePlayers", dataType: "json",
            data: JSON.stringify({ isAi_p1: isAi_p1, team_p1: team_p1, isAi_p2: isAi_p2, team_p2: team_p2 }),
            contentType: "application/json; charset=utf-8"
        }).done(function (response) {
            if (String(response.status).toUpperCase().indexOf("ERROR") >= 0) {
                submitReset();
            }
        }).fail(function () {
            submitReset();
        });
    };

    function setButtonFunctionality(functionality) {
        //switches the button functionality between "start" and "reset"

        if (String(functionality).toUpperCase() == "START") {
            inputButtonElement.value = "Start";
            submitCallback = submitStart;
        }
        else {
            inputButtonElement.value = "Reset";
            submitCallback = submitReset;
        }

        inputButtonElement.onclick = submitCallback;
    };
    function submitReset() {
        //resets the entire game

        try {
            //if there is an active async move chain, the reset will be handled once the chain has 
            //reached a "checkpoint" instead of being handled immediately
            if (Boolean(Grid.HasActiveMove())) {
                Grid.RequestCancellation(submitReset);
                //since it could take a second or two for the requested cancellation to go through,
                //a reset of the grid / form is done to give the user immediate feedback to their button press
                Grid.ResetGrid();
                resetForm();
                return;
            }

            //grid
            Grid.UnlockGrid();
            Grid.ResetGrid();

            //form
            unlockForm();
            resetForm();

            //backend
            $.ajax({type: "POST", url: "/TicTacToe/Reset"
            }).done(function (response) {
                if (String(response.status).toUpperCase().indexOf("ERROR") >= 0) {
                    errorCallback();
                }
            }).fail(function () {
                errorCallback();
            });
        } catch (e) {
            errorCallback();
        }

        function errorCallback() {
            //reload the entire page in the event of an error
            window.location.reload(false);
        }
    };
    function submitStart() {
        //enables user to initiate an AI move for player 1, to start the game

        setButtonFunctionality("RESET");
        Grid.InitiateMove();
    };

    function lockForm() {
        //locks the form

        //prevents processing, if form is already locked
        if (Boolean(isLocked)) {
            return;
        }

        outerTeamP1Element.title = "";
        outerTeamP2Element.title = "";
        outerTeamP1Element.style.pointerEvents = "none";
        outerTeamP2Element.style.pointerEvents = "none";

        isAiP1Element.disabled = true;
        isAiP2Element.disabled = true;
        isAiP1Element.style.pointerEvents = "none";
        isAiP2Element.style.pointerEvents = "none";

        isLocked = true;
    };
    function unlockForm() {
        //unlocks the form

        //prevents processing, if form is already unlocked
        if (!Boolean(isLocked)) {
            return;
        }

        outerTeamP1Element.title = "Click to swap";
        outerTeamP2Element.title = "Click to swap";
        outerTeamP1Element.style.pointerEvents = "auto";
        outerTeamP2Element.style.pointerEvents = "auto";

        isAiP1Element.disabled = false;
        isAiP2Element.disabled = false;
        isAiP1Element.style.pointerEvents = "auto";
        isAiP2Element.style.pointerEvents = "auto";

        isLocked = false;
    };
    function resetForm() {
        //clears the form back to its default state

        teamP1Element.innerText = "X";
        teamP2Element.innerText = "O";
        playerNameP1Element.style.fontWeight = "normal";
        playerNameP2Element.style.fontWeight = "normal";

        scoreP1Element.innerText = "0";
        scoreP2Element.innerText = "0";
        scoreTiesElement.innerText = "0";
        scoreP1Element.style.fontWeight = "normal";
        scoreP2Element.style.fontWeight = "normal";
        scoreTiesElement.style.fontWeight = "normal";

        isAiP1Element.checked = false;
        isAiP2Element.checked = true;

        setButtonFunctionality("RESET");
    };

    return {
        Submit: submitCallback,
        Start: submitStart,
        Reset: submitReset,

        SwapTeam: swapTeam,
        SwapAiStatus: swapAiStatus,
        //only exposes the element variables that are used externally
        PlayerNameP1Element: playerNameP1Element,
        PlayerNameP2Element: playerNameP2Element,
        TeamP1Element: teamP1Element,
        TeamP2Element: teamP2Element,
        ScoreP1Element: scoreP1Element,
        ScoreP2Element: scoreP2Element,
        ScoreTiesElement: scoreTiesElement,
        IsAiP1Element: isAiP1Element,
        IsAiP2Element: isAiP2Element,

        LockForm: lockForm,
        UnlockForm: unlockForm,
        ResetForm: resetForm
    };
}());