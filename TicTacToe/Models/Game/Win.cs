using System.Collections.Generic;

namespace TicTacToe.Models
{
    public class Win
    {
        public Player WinningPlayer { get; set; }
        public Team WinningTeam { get; set; }
        public List<GridSquarePosition> WinningPositions { get; set; }
        
        public Win()
        {
        }
    }
}