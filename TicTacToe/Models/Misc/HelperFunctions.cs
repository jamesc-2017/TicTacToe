using System;

namespace TicTacToe.Models
{
    public static class HelperFunctions
    {
        /// <summary>
        /// Gets the opposing team.
        /// </summary>
        public static Team Opponent(this Team team)
        {
            switch (team)
            {
                case Team.X:
                    return Team.O;
                case Team.O:
                    return Team.X;
                default:
                    return Team.None;
            }
        }
        
        /// <summary>
        /// Surrounds the calling string with the argument supplied.
        /// </summary>
        public static string Surround(this string inner, string outer)
        {
            return String.Format("{0}{1}{0}", outer, inner, outer);
        }
    }
}