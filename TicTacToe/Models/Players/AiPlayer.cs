
namespace TicTacToe.Models
{
    public class AiPlayer : Player
    {
        public AiPlayer(int playerNumber, Team team) : base(playerNumber, team) {}
        
        public Move MakeMove(Grid grid)
        {
            return makeMove(grid, AiEngine.GenerateMove(grid, Team).Position);
        }

        public static explicit operator HumanPlayer(AiPlayer aiPlayer)
        {
            //explicit cast to HumanPlayer
            return new HumanPlayer(aiPlayer.Number, aiPlayer.Team) 
            {
                Score = aiPlayer.Score
                , IsActive = aiPlayer.IsActive
                , IsStartingPlayer = aiPlayer.IsStartingPlayer
            };
        }
    }
}