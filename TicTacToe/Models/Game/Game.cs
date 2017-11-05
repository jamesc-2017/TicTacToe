using System.Linq;

namespace TicTacToe.Models
{
    public enum GameStatus { None, Active, Tie, Win }
    public enum GridSquarePosition { None = 0
        , A1 = 1, A2 = 2, A3 = 3
        , B1 = 4, B2 = 5, B3 = 6
        , C1 = 7, C2 = 8, C3 = 9 }
    public enum Team { None, X, O }
    public enum PlayerType { None, Human, Ai }

    public class Game
    {
        public Grid Grid { get; set; }
        public Players Players { get; set; }
        public GameStatus Status
        {
            get
            {
                if (Grid.SquarePairs.Any(sp => sp.HasWin))
                {
                    return GameStatus.Win;
                }
                else if (Grid.IsFilled)
                {
                    return GameStatus.Tie;
                }
                else
                {
                    return GameStatus.Active;
                }
            }
        }
        //uint overflows back to zero instead of -2B (just in case the game is played for a very long time)
        public uint Ties { get; set; }

        public Game()
        {
            initialize();
        }
        
        public Win SetWin()
        {
            Win winner = new Win();

            if (Status != GameStatus.Win)
            {
                return null;
            }

            winner.WinningTeam = Grid.SquarePairs.First(sp => sp.HasWin).Squares.First().Team;
            winner.WinningPlayer = Players[winner.WinningTeam];
            winner.WinningPositions = Grid.SquarePairs
                .Where(sp => sp.HasWin)
                .SelectMany(sp => sp.Squares)
                .Select(s => s.Position)
                .Distinct().ToList();
            
            winner.WinningPlayer.Score++;
            startNew();

            return winner;
        }
        public void SetTie()
        {
            if (Status != GameStatus.Tie)
            {
                return;
            }

            Ties++;
            startNew();
        }
        public void Reset()
        {
            initialize();
        }

        private void initialize()
        {
            Grid = new Grid();
            Players = new Players(new HumanPlayer(1, Team.X), new AiPlayer(2, Team.O));
            Ties = 0;
        }
        private void startNew()
        {
            Grid.Clear();
            //alternating the starting players evens out the game (the starting player has the advantage)
            Players.SwapStartingPlayer();
            if (Players.StartingPlayer.Number != Players.ActivePlayer.Number)
            {
                //swap the active players, if the starting player is not already active
                Players.SwapActivePlayer();
            }
        }
    }
}