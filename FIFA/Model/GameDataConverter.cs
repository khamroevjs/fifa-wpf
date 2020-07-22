using System.Collections.ObjectModel;

namespace FIFA.Model
{
    /// <summary>
    /// For serialization
    /// </summary>
    public class GameDataConverter
    {
        public ObservableCollection<Footballer> ComputerTeam { get; }
        public ObservableCollection<Footballer> UserTeam { get; }
        public int Round { get; }
        public int Stage { get; }
        public int ComputerGoals { get; }
        public int UserGoals { get; }

        public GameDataConverter(ObservableCollection<Footballer> computerTeam,
            ObservableCollection<Footballer> userTeam, int round, int stage, int computerGoals, int userGoals)
        {
            ComputerTeam = computerTeam;
            UserTeam = userTeam;
            Round = round;
            Stage = stage;
            ComputerGoals = computerGoals;
            UserGoals = userGoals;
        }
    }
}
