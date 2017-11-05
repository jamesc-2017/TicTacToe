using System;
using System.Collections.Generic;
using System.Linq;

namespace TicTacToe.Models
{
    public static class AiEngine
    {
        public static Move GenerateMove(Grid grid, Team team)
        {
            //input validation
            if (grid == null || grid.IsFilled || team == Team.None)
            {
                return null;
            }

            return (new MoveCalculator(grid, team)).CalculateMove();
        }
        
        //encapsulates the move calculation logic to keep the outer endpoint stateless
        //all moves are calculated in a rule-based manner; positions are not hardcoded
        //this algorithm is an original work of the creator.
        //algorithm prioritizes loss avoidance and variety of playable moves,
        //with a secondary objective to seek wins in an opportunistic manner
        private class MoveCalculator
        {
            private Random Rng { get; }
            private Grid Grid { get; set; }
            private Team Team { get; set; }
            
            public MoveCalculator(Grid grid, Team team)
            {
                //random number generator
                Rng = new Random();
                Grid = grid;
                Team = team;
            }
            
            public Move CalculateMove()
            {
                Move move = new Move(Team);
                List<GridSquarePosition> positions;

                try
                {
                    //this code will set "positions" to each move type in order
                    //and will short circuit if a position is returned

                    //make a winning move first, if one is available
                    if ((positions = immediatePositions(Grid, Team)).Count > 0) {}
                    //prevent the opponent from making a winning move
                    else if ((positions = immediatePositions(Grid, Team.Opponent())).Count > 0) {}
                    //make a "cross move" (planning one move ahead)
                    else if ((positions = crossPositions(Grid, Team)).Count > 0) {}
                    //make a "setup move" (planning two moves ahead)
                    else if ((positions = setupPositions(Grid, Team)).Count > 0) {}
                    //move to a safe position
                    else if ((positions = safePositions(Grid, Team)).Count > 0) {}
                    //move to a vacant position as a last resort
                    else { positions = vacantPositions(Grid); }

                    //selects a random position
                    move.Position = positions.ElementAt(Rng.Next(positions.Count));
                }
                catch (Exception)
                {
                    move = null;
                }

                return move;
            }

            private List<GridSquarePosition> immediatePositions(Grid grid, Team team)
            {
                //an "immediate move" is a winning / losing move that will occur next turn

                return grid.SquarePairs
                    //retrieve all squarePairs one move away from winning
                    .Where(sp => sp.Squares.Count(s => s.IsVacant) == 1 && sp.Squares.Count(s => s.Team == team) == 2)
                    .Select(sp => sp.Squares.Single(s => s.IsVacant).Position)
                    .Distinct().ToList();
            }
            private List<GridSquarePosition> crossPositions(Grid grid, Team team)
            {
                //a "cross move" is a single move that creates multiple immediate moves the following turn
                //since your opponent can only make one move per turn, this guarantees a win the following turn
                //immediate moves should be checked before cross moves
                
                return linearPositions(grid, team)
                    .GroupBy(s => s)
                    //all positions that occur in multiple squarePairs
                    .Where(s => s.Count() > 1)
                    .Select(s => s.Key).ToList();
            }
            private List<GridSquarePosition> setupPositions(Grid grid, Team team)
            {
                //a "setup move" guarantees a cross move to be set the next turn
                //this is possible by moving into a position that creates an immediate move
                //which must be blocked instead of diverting the cross move
                //cross moves should be checked before setup moves
            
                    //retrieve all positions that would require the opponent to block an immediate move
                return linearPositions(grid, team).Distinct()
                    .Select(p => new { Position = p, Result = setupProjection(grid, new Move(team, p)) })
                    .Where(r => r.Result.WinningPositions.Count > 0
                        //setup can result in failure if the opponents response to the setup creates a situation where
                        //you must block an immediate move (at a separate position) instead of setting up the cross move
                        && r.Result.LosingPositions.All(p => r.Result.WinningPositions.Contains(p))
                        && r.Result.LosingPositions.Count(p => r.Result.WinningPositions.Contains(p)) <= 1)
                    .Select(r => r.Position).ToList();
            }
            private List<GridSquarePosition> safePositions(Grid grid, Team team)
            {
                //retrieves positions that will not result in a loss
                
                return vacantPositions(grid)
                    .Select(p => new { Position = p, Result = safeProjection(grid, new Move(team, p)) })
                    //in addition to moving directly to a position, immediate moves can be used to divert the opponent
                    //as long as the resulting "winning" position is not itself a losing position
                    .Where(r => r.Result.LosingPositions.Count == 0 || (r.Result.WinningPositions.Count > 0
                        && !r.Result.WinningPositions.Any(p => r.Result.LosingPositions.Contains(p))))
                    .Select(r => r.Position).ToList();
            }
            private List<GridSquarePosition> linearPositions(Grid grid, Team team)
            {
                //retrieves all positions capable of winning "in a straight line" of an existing position

                return grid.SquarePairs
                    //retrieve all squarePairs with only a single move present
                    .Where(sp => sp.Squares.Count(s => s.IsVacant) == 2 && sp.Squares.Count(s => s.Team == team) == 1)
                    .SelectMany(sp => sp.Squares.Where(s => s.IsVacant))
                    .Select(s => s.Position).ToList();
            }
            private List<GridSquarePosition> vacantPositions(Grid grid)
            {
                //retrieves all vacant positions on the grid

                return grid.Squares
                    .Where(s => s.IsVacant)
                    .Select(s => s.Position).ToList();
            }
            
            private ProjectionResult setupProjection(Grid grid, Move move)
            {
                //grid projection logic used to determine setup positions

                Grid projectedGrid = grid.Copy();
                Team team = move.Team;
                ProjectionResult result = new ProjectionResult();
                GridSquarePosition immediate;

                projectedGrid.SetMove(move);
                //only one position will be returned (since cross moves precede setup moves)
                immediate = immediatePositions(projectedGrid, team).Single();
                //opponents response to block your immediate move
                projectedGrid.SetMove(new Move(team.Opponent(), immediate));
                //gets the resultant cross moves from the potential setups
                result.WinningPositions = crossPositions(projectedGrid, team);
                //gets any immediate moves that may have been caused by your opponent's blocking move
                //note that it does not matter if the opponent's blocking move was a cross / setup move
                //since you will be at least one move ahead of them in either case
                result.LosingPositions = immediatePositions(projectedGrid, team.Opponent());

                return result;
            }
            private ProjectionResult safeProjection(Grid grid, Move move)
            {
                //grid projection logic used to determine safe positions

                Grid projectedGrid = grid.Copy();
                Team team = move.Team;
                ProjectionResult result = new ProjectionResult();

                projectedGrid.SetMove(move);
                result.WinningPositions.AddRange(immediatePositions(projectedGrid, team));

                //negative outcomes of the move
                result.LosingPositions.AddRange(immediatePositions(projectedGrid, team.Opponent()));
                result.LosingPositions.AddRange(crossPositions(projectedGrid, team.Opponent()));
                if (result.LosingPositions.Count == 0)
                {
                    //check setup moves if no immediate / cross moves are present
                    result.LosingPositions.AddRange(setupPositions(projectedGrid, team.Opponent()));
                }
                
                //if there is more than one winning position, or the losing positions already contains the winning position,
                //further checking to determine the safeness of the position is not needed
                if (result.WinningPositions.Count == 1 && !result.LosingPositions.Contains(result.WinningPositions.Single()))
                {
                    //opponents response to block your immediate move
                    projectedGrid.SetMove(new Move(team.Opponent(), result.WinningPositions.Single()));

                    //determine if there is at least one safe path that can be formed if the opponent blocks your immediate move
                    if (!projectedGrid.IsFilled && (immediatePositions(projectedGrid, team.Opponent()).Count > 1
                        || safePositions(projectedGrid, team).Count == 0))
                    {
                        //position is not safe
                        result.LosingPositions.Add(result.WinningPositions.Single());
                    }
                }

                return result;
            }

            private class ProjectionResult
            {
                public List<GridSquarePosition> WinningPositions { get; set; }
                public List<GridSquarePosition> LosingPositions { get; set; }

                public ProjectionResult()
                {
                    WinningPositions = new List<GridSquarePosition>();
                    LosingPositions = new List<GridSquarePosition>();
                }
            }
        }
    }
}