
namespace TicTacToe.Models
{
    public class GridSquare
    {
        public GridSquarePosition Position { get; }
        public Team Team { get; set; }
        public int GridRow
        {
            get
            {
                switch (Position)
                {
                    case GridSquarePosition.A1:
                    case GridSquarePosition.A2:
                    case GridSquarePosition.A3:
                        return 0;
                    case GridSquarePosition.B1:
                    case GridSquarePosition.B2:
                    case GridSquarePosition.B3:
                        return 1;
                    case GridSquarePosition.C1:
                    case GridSquarePosition.C2:
                    case GridSquarePosition.C3:
                        return 2;
                    default:
                        return -1;
                }
            }
        }
        public int GridColumn
        {
            get
            {
                switch (Position)
                {
                    case GridSquarePosition.A1:
                    case GridSquarePosition.B1:
                    case GridSquarePosition.C1:
                        return 0;
                    case GridSquarePosition.A2:
                    case GridSquarePosition.B2:
                    case GridSquarePosition.C2:
                        return 1;
                    case GridSquarePosition.A3:
                    case GridSquarePosition.B3:
                    case GridSquarePosition.C3:
                        return 2;
                    default:
                        return -1;
                }
            }
        }
        public bool IsVacant 
        { 
            get
            {
                if (Team == Team.None)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        
        public GridSquare(GridSquarePosition position, Team team)
        {
            Position = position;
            Team = team;
        }
    }
}