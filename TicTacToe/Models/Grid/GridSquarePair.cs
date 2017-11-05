using System.Collections.Generic;
using System.Linq;

namespace TicTacToe.Models
{
    public class GridSquarePair
    {
        public List<GridSquare> Squares { get; private set; }
        public bool HasWin
        {
            get
            {
                if (Squares.All(s => s.Team == Team.X)
                    || Squares.All(s => s.Team == Team.O))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        
        public GridSquarePair(List<GridSquare> squares)
        {
            Squares = squares;
        }
        
    }
}