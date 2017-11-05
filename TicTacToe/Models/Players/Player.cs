
namespace TicTacToe.Models
{
    public abstract class Player
    {
        public int Number { get; private set; }
        public Team Team { get; set; }
        //uint overflows back to zero instead of -2B (just in case the game is played for a very long time)
        public uint Score { get; set; }
        public bool IsActive { get; set; }
        public bool IsStartingPlayer { get; set; }

        public Player(int playerNumber, Team team)
        {
            Number = playerNumber;
            Team = team;
            Score = 0;
            //Player 1 starts the game
            IsActive = ((Number == 1) ? true : false);
            IsStartingPlayer = ((Number == 1) ? true : false);
        }

        protected Move makeMove(Grid grid, GridSquarePosition position)
        {
            Move move = new Move(Team, position);

            if (!grid[move.Position].IsVacant)
            {
                return null;
            }
            else
            {
                grid.SetMove(move);
                return move;
            }
        }
    }
}