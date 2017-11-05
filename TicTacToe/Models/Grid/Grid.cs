using System;
using System.Collections.Generic;
using System.Linq;

namespace TicTacToe.Models
{
    public class Grid
    {
        public List<GridSquare> Squares { get; private set; }
        public List<GridSquarePair> SquarePairs { get; private set; }
        public bool IsFilled
        {
            get
            {
                return Squares.All(s => !s.IsVacant);
            }
        }

        public Grid()
        {
            Squares = new List<GridSquare>(9)
            {
                new GridSquare(GridSquarePosition.A1, Team.None)
                , new GridSquare(GridSquarePosition.A2, Team.None)
                , new GridSquare(GridSquarePosition.A3, Team.None)
                , new GridSquare(GridSquarePosition.B1, Team.None)
                , new GridSquare(GridSquarePosition.B2, Team.None)
                , new GridSquare(GridSquarePosition.B3, Team.None)
                , new GridSquare(GridSquarePosition.C1, Team.None)
                , new GridSquare(GridSquarePosition.C2, Team.None)
                , new GridSquare(GridSquarePosition.C3, Team.None)
            };

            SquarePairs = new List<GridSquarePair>(8)
            {
                new GridSquarePair(Squares.Where(s => s.GridRow == 0).ToList())
                , new GridSquarePair(Squares.Where(s => s.GridRow == 1).ToList())
                , new GridSquarePair(Squares.Where(s => s.GridRow == 2).ToList())
                , new GridSquarePair(Squares.Where(s => s.GridColumn == 0).ToList())
                , new GridSquarePair(Squares.Where(s => s.GridColumn == 1).ToList())
                , new GridSquarePair(Squares.Where(s => s.GridColumn == 2).ToList())
                , new GridSquarePair(new List<GridSquare>() { Squares[0], Squares[4], Squares[8] })
                , new GridSquarePair(new List<GridSquare>() { Squares[2], Squares[4], Squares[6] })
            };
        }

        public GridSquare this[GridSquarePosition position]
        {
            get
            {
                return Squares.Find(s => s.Position == position);
            }
        }
        
        public void SetMove(Move move)
        {
            this[move.Position].Team = move.Team;
        }
        public Grid Copy()
        {
            Grid copy = new Grid();

            foreach (GridSquare s in Squares)
            {
                copy.SetMove(new Move(s.Team, s.Position));
            }

            return copy;
        }
        public void Clear()
        {
            foreach (GridSquare s in Squares)
            {
                s.Team = Team.None;
            }
        }
        public override string ToString()
        {
            //useful for debugging
            return String.Concat(Squares.Select(s =>
                s.Position.ToString() + ((s.Team == Team.None) ? " " : s.Team.ToString()).Surround("|")));
        }
    }
}