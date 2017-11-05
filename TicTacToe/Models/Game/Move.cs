
namespace TicTacToe.Models
{
    public class Move
    {
        public Team Team { get; set; }
        public GridSquarePosition Position { get; set; }
        
        public Move(Team team, GridSquarePosition position)
        {
            Team = team;
            Position = position;
        }
        public Move(Team team)
        {
            Team = team;
            Position = GridSquarePosition.None;
        }
    }
}