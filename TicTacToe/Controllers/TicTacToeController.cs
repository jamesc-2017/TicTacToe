using System;
using System.Linq;
using System.Web.Mvc;
using TicTacToe.Models;

namespace TicTacToe.Controllers
{
    public class TicTacToeController : Controller
    {
        public static Game ActiveGame { get; set; }

        [HttpGet]
        public ActionResult Game()
        {
            //retrieves the view and initializes the game

            ActiveGame = new Game();

            return View();
        }        
        [HttpPost]
        public JsonResult ConfigurePlayers(string isAi_p1, string team_p1, string isAi_p2, string team_p2)
        {
            //syncs the player type and team with the arguments supplied

            Team _team_p1, _team_p2;
            bool _isAi_p1, _isAi_p2;

            //input validation
            if (!validateInput_ConfigurePlayers(isAi_p1, team_p1, isAi_p2, team_p2))
            {
                return Json(new { status = "ERROR - Invalid input" });
            }
            else
            {
                //validation successful.  parsing is not case sensitive, and allows leading/trailing spaces
                _team_p1 = (Team)Enum.Parse(typeof(Team), team_p1, true);
                _team_p2 = (Team)Enum.Parse(typeof(Team), team_p2, true);
                _isAi_p1 = Boolean.Parse(isAi_p1);
                _isAi_p2 = Boolean.Parse(isAi_p2);
            }

            return configurePlayers(_isAi_p1, _team_p1, _isAi_p2, _team_p2);
            
        }
        [HttpPost]
        public JsonResult MakeHumanMove(string position)
        {
            //makes a human move
            
            GridSquarePosition _position;

            //input validation
            if (!validateInput_MakeHumanMove(position))
            {
                return Json(new { status = "ERROR - Invalid input", position = String.Empty, gameStatus = String.Empty });
            }
            else
            {
                //validation successful.  parsing is not case sensitive, and allows leading/trailing spaces
                _position = (GridSquarePosition)Enum.Parse(typeof(GridSquarePosition), position, true);
            }
            
            return makeHumanMove(_position);
            
        }        
        [HttpPost]
        public JsonResult MakeAiMove()
        {
            //makes an AI move

            if (!validateInput_MakeAiMove())
            {
                return Json(new { status = "ERROR - Invalid input", position = String.Empty, gameStatus = String.Empty });
            }

            return makeAiMove();
        }        
        [HttpPost]
        public JsonResult Reset()
        {
            //resets the game

            return reset();
        }

        private bool validateInput_ConfigurePlayers(string isAi_p1, string team_p1, string isAi_p2, string team_p2)
        {
            bool success = true;
            isAi_p1 = isAi_p1.ToUpper().Trim();
            team_p1 = team_p1.ToUpper().Trim();
            isAi_p2 = isAi_p2.ToUpper().Trim();
            team_p2 = team_p2.ToUpper().Trim();

            if ((!Enum.IsDefined(typeof(Team), team_p1) || team_p1 == Team.None.ToString().ToUpper())
                || (!Enum.IsDefined(typeof(Team), team_p2) || team_p2 == Team.None.ToString().ToUpper())
                || (team_p1 == team_p2)
                || (isAi_p1 != "TRUE" && isAi_p1 != "FALSE")
                || (isAi_p2 != "TRUE" && isAi_p2 != "FALSE"))
            {
                success = false;
            }

            return success;
        }
        private bool validateInput_MakeHumanMove(string position)
        {
            bool success = true;
            position = position.ToUpper().Trim();
            
            if ((!Enum.IsDefined(typeof(GridSquarePosition), position)
                    || position == GridSquarePosition.None.ToString().ToUpper())
                || (!(ActiveGame.Players.ActivePlayer is HumanPlayer)))
            {
                success = false;
            }

            return success;
        }
        private bool validateInput_MakeAiMove()
        {
            bool success = true;
            
            if (!(ActiveGame.Players.ActivePlayer is AiPlayer))
            {
                success = false;
            }

            return success;
        }
        private JsonResult configurePlayers(bool isAi_p1, Team team_p1, bool isAi_p2, Team team_p2)
        {
            try
            {
                //determine if teams need to be swapped
                if (team_p1 != ActiveGame.Players.Player1.Team && team_p2 != ActiveGame.Players.Player2.Team)
                {
                    ActiveGame.Players.SwapTeams();
                }
                //determine if p1's type needs to be swapped
                if ((isAi_p1 && (ActiveGame.Players.Player1 is HumanPlayer))
                    || (!isAi_p1 && (ActiveGame.Players.Player1 is AiPlayer)))
                {
                    ActiveGame.Players.SwapPlayerType(ActiveGame.Players.Player1);
                }
                //determine if p2's type needs to be swapped
                if ((isAi_p2 && (ActiveGame.Players.Player2 is HumanPlayer))
                    || (!isAi_p2 && (ActiveGame.Players.Player2 is AiPlayer)))
                {
                    ActiveGame.Players.SwapPlayerType(ActiveGame.Players.Player2);
                }
            }
            catch (Exception)
            {
                return Json(new { status = "ERROR - Unable to configure players" });
            }

            return Json(new { status = "SUCCESS" });
        }
        private JsonResult makeHumanMove(GridSquarePosition position)
        {
            Move move;

            try
            {
                move = ((HumanPlayer)ActiveGame.Players.ActivePlayer).MakeMove(ActiveGame.Grid, position);
            }
            catch (Exception)
            {
                return Json(new { status = "ERROR - Unable to make move"
                    , position = String.Empty
                    , gameStatus = String.Empty });
            }

            return getMoveOutput(move);
        }
        private JsonResult makeAiMove()
        {
            Move move;

            try
            {
                move = ((AiPlayer)ActiveGame.Players.ActivePlayer).MakeMove(ActiveGame.Grid);
            }
            catch (Exception)
            {
                return Json(new { status = "ERROR - Unable to make move"
                    , position = String.Empty
                    , gameStatus = String.Empty });
            }

            return getMoveOutput(move);
        }
        private JsonResult getMoveOutput(Move move)
        {
            if (move == null)
            {
                return Json(new { status = "ERROR - Square occupied"
                    , position = String.Empty
                    , gameStatus = String.Empty });
            }
            else
            {
                //move was successful
                return Json(new { status = "SUCCESS"
                    , position = move.Position.ToString()
                    , gameStatus = gameStatus() });
            }
        }
        private JsonResult gameStatus()
        {
            //retrieves the current status of the game
            //this should be checked after every move

            string gameStatus = ActiveGame.Status.ToString().ToUpper();
            Win winner = null;
            string score = String.Empty;

            if (ActiveGame.Status == GameStatus.Win)
            {
                winner = ActiveGame.SetWin();
            }
            else if (ActiveGame.Status == GameStatus.Tie)
            {
                ActiveGame.SetTie();
                score = ActiveGame.Ties.ToString();
            }
            else if (ActiveGame.Status == GameStatus.Active)
            {
                ActiveGame.Players.SwapActivePlayer();
            }
            
            return Json(new { gameStatus = gameStatus
                , nextActivePlayer = ActiveGame.Players.ActivePlayer.Number.ToString()
                , winningPlayer = (winner == null) ? String.Empty : winner.WinningPlayer.Number.ToString()
                , winningTeam = (winner == null) ? String.Empty : winner.WinningTeam.ToString()
                , winningSquares = (winner == null) ? String.Empty
                    : String.Join(",", winner.WinningPositions.Select(s=>s.ToString()))
                , winnersScore = (winner == null) ? score : winner.WinningPlayer.Score.ToString() });
        }
        private JsonResult reset()
        {
            try
            {
                ActiveGame.Reset();
            }
            catch (Exception)
            {
                return Json(new { status = "ERROR - Unable to reset the game" });
            }

            return Json(new { status = "SUCCESS" });
        }
    }
}