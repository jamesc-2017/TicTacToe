
namespace TicTacToe.Models
{
    public class HumanPlayer : Player
    {
        public HumanPlayer(int playerNumber, Team team) : base(playerNumber, team) {}
        
        public Move MakeMove(Grid grid, GridSquarePosition position)
        {
            return makeMove(grid, position);
        }
        
        public static explicit operator AiPlayer(HumanPlayer humanPlayer)
        {
            //explicit cast to AiPlayer
            return new AiPlayer(humanPlayer.Number, humanPlayer.Team) 
            {
                Score = humanPlayer.Score
                , IsActive = humanPlayer.IsActive
                , IsStartingPlayer = humanPlayer.IsStartingPlayer
            };
        }
    }
}