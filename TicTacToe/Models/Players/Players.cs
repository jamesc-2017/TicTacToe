
namespace TicTacToe.Models
{
    public class Players
    {
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }
        public Player ActivePlayer { get { return (Player1.IsActive) ? Player1 : Player2; } }
        public Player StartingPlayer { get { return (Player1.IsStartingPlayer) ? Player1 : Player2; } }

        public Players(Player player1, Player player2)
        {
            Player1 = player1;
            Player2 = player2;
        }
        
        public Player this[Player p]
        {
            get { return (Player1.Number == p.Number) ? Player1 : Player2; }
            set
            {
                if (Player1.Number == value.Number)
                {
                    Player1 = value;
                }
                else
                {
                    Player2 = value;
                }
            }
        }
        public Player this[Team t]
        {
            get { return (Player1.Team == t) ? Player1 : Player2; }
        }
        
        public void SwapActivePlayer()
        {
            bool temp = Player1.IsActive;

            Player1.IsActive = Player2.IsActive;
            Player2.IsActive = temp;
        }
        public void SwapStartingPlayer()
        {
            bool temp = Player1.IsStartingPlayer;

            Player1.IsStartingPlayer = Player2.IsStartingPlayer;
            Player2.IsStartingPlayer = temp;
        }
        public void SwapTeams()
        {
            Team temp = Player1.Team;

            Player1.Team = Player2.Team;
            Player2.Team = temp;
        }
        public void SwapPlayerType(Player player)
        {
            //convert the player type
            if (player is HumanPlayer)
            {
                player = (AiPlayer)((HumanPlayer)player);
            }
            else if (player is AiPlayer)
            {
                player = (HumanPlayer)((AiPlayer)player);
            }

            //reassign the converted player
            this[player] = player;
        }
    }
}