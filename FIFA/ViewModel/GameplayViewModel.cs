using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FIFA.Model;
using FIFA.View;
using Newtonsoft.Json;

namespace FIFA.ViewModel
{
    public class GameplayViewModel : BaseViewModel
    {
        static readonly Random rnd = new Random();
        private const string filePath = @"..\..\..\..\GameData.json";
        public ObservableCollection<Footballer> ComputerTeam { get; }
        public ObservableCollection<Footballer> UserTeam { get; }
        public Footballer ComputerSelected { get; set; }
        public Footballer UserSelected { get; set; }

        #region Game Data

        private int round = 1;
        public int Round
        {
            get => round;
            set => SetProperty(ref round, value);
        }

        private int stage = 1;
        public int Stage
        {
            get => stage;
            set => SetProperty(ref stage, value);
        }

        private int computerGoals;
        private int userGoals;
        public int ComputerGoals
        {
            get => computerGoals;
            set => SetProperty(ref computerGoals, value);
        }
        public int UserGoals
        {
            get => userGoals;
            set => SetProperty(ref userGoals, value);
        }
        #endregion

        #region Game Result

        private string gameResult = "You Win!!!";
        public string GameResult
        {
            get => gameResult;
            set => SetProperty(ref gameResult, value);
        }

        private Brush gameResultBrush = Brushes.Green;
        public Brush GameResultBrush
        {
            get => gameResultBrush;
            set => SetProperty(ref gameResultBrush, value);
        }

        private Visibility gameResultVisibility = Visibility.Collapsed;
        public Visibility GameResultVisibility
        {
            get => gameResultVisibility;
            set => SetProperty(ref gameResultVisibility, value);
        }

        #endregion

        private string footballersVersus;
        /// <summary>
        /// Footballers who are currently fighting
        /// </summary>
        public string FootballersVersus
        {
            get => footballersVersus;
            set => SetProperty(ref footballersVersus, value);
        }

        private string buttonContent = "Attack";
        public string ButtonContent
        {
            get => buttonContent;
            set => SetProperty(ref buttonContent, value);
        }
        public ICommand ButtonCommand { get; }

        public GameplayViewModel(List<Footballer> footballers, ObservableCollection<Footballer> userTeam)
        {
            // Selecting 11 footballers for computer team
            while (footballers.Count > 11)
                footballers.RemoveAt(rnd.Next(footballers.Count));

            ComputerTeam = new ObservableCollection<Footballer>(footballers);
            UserTeam = userTeam;

            ButtonCommand = new Command(Button);
        }

        /// <summary>
        /// Continues saved game
        /// </summary>
        /// <param name="gameData">Game data of saved game</param>
        public GameplayViewModel(GameDataConverter gameData)
        {
            UserTeam = gameData.UserTeam;
            ComputerTeam = gameData.ComputerTeam;
            Round = gameData.Round;
            Stage = gameData.Stage;
            ComputerGoals = gameData.ComputerGoals;
            UserGoals = gameData.UserGoals;

            // Determining whether user Attacking or Protecting
            if (Round % 2 == 0)
                ButtonContent = "Protect";

            ButtonCommand = new Command(Button);
        }

        void Button()
        {
            switch (ButtonContent)
            {
                case "Attack" when UserSelected != null:
                    {
                        int stageResult = BeginStage();

                        // if user won the stage
                        if (stageResult == 1)
                            ++Stage;
                        else if (stageResult == -1)
                        {
                            SaveGame();
                            ++Round;
                            Stage = 1;
                            ButtonContent = "Protect";
                        }

                        if (Stage == 5)
                        {
                            SaveGame();
                            ++UserGoals;
                            ++Round;
                            Stage = 1;
                            ButtonContent = "Protect";
                        }

                        if (Round == 31)
                        {
                            Round = 30;
                            OverallResult();
                        }

                        FootballersVersus = $"{UserSelected.ShortName} vs. {ComputerSelected.ShortName}";
                        break;
                    }
                case "Protect" when UserSelected != null:
                    {
                        int stageResult = BeginStage();
                        switch (stageResult)
                        {
                            case 1:
                                SaveGame();
                                ++Round;
                                Stage = 1;
                                ButtonContent = "Attack";
                                break;
                            case -1:
                                ++Stage;
                                break;
                        }

                        if (Stage == 5)
                        {
                            SaveGame();
                            ++ComputerGoals;
                            ++Round;
                            Stage = 1;
                            ButtonContent = "Attack";
                        }

                        if (Round == 31)
                        {
                            Round = 30;
                            OverallResult();
                        }

                        FootballersVersus = $"{UserSelected.ShortName} vs. {ComputerSelected.ShortName}";
                        break;
                    }
                // When user doesn't select any footballer
                case "Attack" when UserSelected == null:
                case "Protect" when UserSelected == null:
                    MessageBox.Show("Select a footballer!");
                    break;
                case "Play again":
                    new IntroWindow().Show();
                    Application.Current.Windows[0]?.Close();
                    break;
            }
        }

        int BeginStage()
        {
            double userResult = ((UserSelected.Height - UserSelected.Weight) / 10.0) * UserSelected.Overall / Math.Max(UserSelected.Overall - UserSelected.Potential, 1);

            ComputerSelected = ComputerTeam[rnd.Next(ComputerTeam.Count)];

            double computerResult = ((ComputerSelected.Height - ComputerSelected.Weight) / 10.0) * ComputerSelected.Overall / Math.Max(ComputerSelected.Overall - ComputerSelected.Potential, 1);

            if (userResult > computerResult)
                return 1;

            if (userResult < computerResult)
                return -1;

            return 0;
        }

        void OverallResult()
        {
            ButtonContent = "Play again";

            if (UserGoals < ComputerGoals)
            {
                GameResult = "You Lose!";
                GameResultBrush = Brushes.DarkRed;
            }
            else if (UserGoals == ComputerGoals)
            {
                GameResult = "Draw!";
                GameResultBrush = Brushes.DimGray;
            }

            GameResultVisibility = Visibility.Visible;
        }

        void SaveGame()
        {
            try
            {
                string json = JsonConvert.SerializeObject(new GameDataConverter(ComputerTeam, UserTeam, Round, Stage, ComputerGoals, UserGoals));
                using (var sw = new StreamWriter(filePath))
                    sw.Write(json);
            }
            catch (JsonSerializationException)
            {
                MessageBox.Show("Game hasn't been saved", "Error!");
            }
            catch (JsonWriterException)
            {
                MessageBox.Show("Game hasn't been saved", "Error!");
            }
            catch (JsonException)
            {
                MessageBox.Show("Game hasn't been saved", "Error!");
            }
            catch (SerializationException)
            {
                MessageBox.Show("Game hasn't been saved", "Error!");
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Game hasn't been saved", "Error!");
            }
            catch (IOException)
            {
                MessageBox.Show("Game hasn't been saved", "Error!");
            }
            catch (Exception)
            {
                MessageBox.Show("Game hasn't been saved", "Error!");
            }
        }
    }
}
