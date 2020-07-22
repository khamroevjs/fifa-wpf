using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FIFA.Model;
using FIFA.View;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace FIFA.ViewModel
{
    public class IntroViewModel : BaseViewModel
    {
        List<Footballer> ComputerTeam { get; }
        List<Footballer> UserTeam { get; }

        private const string filePath = @"..\..\..\..\GameData.json";

        private bool continueIsEnabled;
        public bool ContinueIsEnabled
        {
            get => continueIsEnabled;
            set => SetProperty(ref continueIsEnabled, value);
        }

        public ICommand NewGameCommand { get; }

        public ICommand ContinueCommand { get; }

        public IntroViewModel()
        {
            if (File.Exists(filePath))
                ContinueIsEnabled = true;

            NewGameCommand = new Command(NewGame);
            ContinueCommand = new Command(Continue);
            ComputerTeam = new List<Footballer>();
            UserTeam = new List<Footballer>();
        }
        async void NewGame()
        {
            var openFileDialog = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv*", RestoreDirectory = true };

            // If user chooses file and file successfully parsed
            if (openFileDialog.ShowDialog() == true && await ReadFileAsync(openFileDialog.FileName))
            {
                // Launching the game
                new MainWindow(new MainViewModel(ComputerTeam, UserTeam)).Show();
                Application.Current.Windows[0]?.Close();
            }
        }

        async void Continue()
        {
            GameDataConverter gameData;
            if (File.Exists(filePath) && (gameData = await LoadGame()) != null)
            {
                new GameplayWindow(new GameplayViewModel(gameData)).Show();
                Application.Current.Windows[0]?.Close();
            }
            else
                MessageBox.Show("File cannot be opened", "Error!");
        }
        /// <summary>
        /// Loads saved game
        /// </summary>
        /// <returns>GameDataConverter</returns>
        async Task<GameDataConverter> LoadGame()
        {
            GameDataConverter gameData = null;
            try
            {
                using var sr = new StreamReader(filePath);
                string json = await sr.ReadToEndAsync();
                gameData = JsonConvert.DeserializeObject<GameDataConverter>(json);
            }
            catch (JsonReaderException ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
            catch (JsonSerializationException ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
            catch (JsonException ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
            catch (SerializationException ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
            catch (DirectoryNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
            catch (OutOfMemoryException ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }

            return gameData;
        }

        /// <summary>
        /// Reads CSV file and fills properties
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns>
        /// true - if file has been read successfully
        /// false - otherwise
        /// </returns>
        async Task<bool> ReadFileAsync(string path)
        {
            try
            {
                using (var sr = new StreamReader(path))
                {
                    // If does not satisfy the condition, false
                    if (!CheckFirstRow(await sr.ReadLineAsync()))
                        throw new FileFormatException("Incorrect column order!");

                    string line;
                    while ((line = await sr.ReadLineAsync()) != null)
                        ComputerTeam.Add(await Task.Run(() => CreateFootballer(line)));
                }

                // If there are less than 22 players
                if (ComputerTeam.Count < 22)
                    throw new FileFormatException("There are less than 22 players in the file!");

                return true; // File successfully parsed
            }
            catch (FileFormatException fce)
            {
                MessageBox.Show(fce.Message, "File was corrupted!");
                return false;
            }
            catch (DirectoryNotFoundException directory)
            {
                MessageBox.Show(directory.Message, "Error!");
                return false;
            }
            catch (FileNotFoundException noFile)
            {
                MessageBox.Show(noFile.Message, "File not found!");
                return false;
            }
            catch (UnauthorizedAccessException unauthorized)
            {
                MessageBox.Show(unauthorized.Message, "Inaccessible!");
                return false;
            }
            catch (IOException io)
            {
                MessageBox.Show(io.Message, "Inaccessible!");
                return false;
            }
            catch (System.Security.SecurityException security)
            {
                MessageBox.Show(security.Message);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!");
                return false;
            }
        }

        /// <summary>
        /// Creates a footballer
        /// </summary>
        /// <param name="row">String from which will be created a footballer</param>
        /// <returns>Footballer</returns>
        Footballer CreateFootballer(string row)
        {
            var cells = row.Split(new[] { ';' });

            int cellsCounter = cells.Length;
            if (cellsCounter != 12)
                throw new FileFormatException("Invalid amount of columns");

            var footballer = new Footballer();

            for (int i = 0; i < cellsCounter; i++)
            {
                switch (i)
                {
                    case 0: // sofifa_id
                        if (!(int.TryParse(cells[i], out int sofifaID) && sofifaID >= 1))
                            throw new FormatException("Invalid Sofifa ID");
                        footballer.SofifaID = sofifaID;
                        break;
                    case 1: // player_url
                        if (string.IsNullOrEmpty(cells[i].Trim()))
                            throw new FileFormatException("Invalid Player URL");
                        footballer.PlayerURL = cells[i];
                        break;

                    case 2: // short_name
                        if (string.IsNullOrEmpty(cells[i].Trim()))
                            throw new FileFormatException("Invalid Short Name");
                        footballer.ShortName = cells[i];
                        break;
                    case 3: // long_name
                        if (string.IsNullOrEmpty(cells[i].Trim()))
                            throw new FileFormatException("Invalid Long Name");
                        footballer.LongName = cells[i];
                        break;
                    case 4: // age
                        if (!int.TryParse(cells[i], out int age) || age < 18 || age > 100)
                            throw new FileFormatException("Invalid Age");
                        footballer.Age = age;
                        break;
                    case 5: // dob
                        var ruCulture = new CultureInfo("ru-RU");
                        if (!(DateTime.TryParse(cells[i], ruCulture, DateTimeStyles.None, out DateTime dob) && dob < DateTime.Now.AddYears(-18)))
                            throw new FileFormatException("Invalid Date of birth");
                        footballer.DateOfBirth = dob;
                        break;
                    case 6: // height_cm
                        if (!int.TryParse(cells[i], out int height) || height < 100 || height > 300)
                            throw new FileFormatException("Invalid Height");
                        footballer.Height = height;
                        break;
                    case 7: // weight_kg 
                        if (!int.TryParse(cells[i], out int weight) || weight < 40 || weight > 200)
                            throw new FileFormatException("Invalid Weight");
                        footballer.Weight = weight;
                        break;
                    case 8: // nationality
                        if (string.IsNullOrEmpty(cells[i].Trim()))
                            throw new FileFormatException("Invalid Nationality");
                        footballer.Nationality = cells[i];
                        break;
                    case 9: // club
                        if (string.IsNullOrEmpty(cells[i].Trim()))
                            throw new FileFormatException("Invalid Club");
                        footballer.Club = cells[i];
                        break;
                    case 10: // overall
                        if (!(int.TryParse(cells[i], out int overall) && overall >= 0 && overall <= 100))
                            throw new FileFormatException("Invalid Overall");
                        footballer.Overall = overall;
                        break;
                    case 11: // potential
                        if (!(int.TryParse(cells[i], out int potential) && potential >= 0 && potential <= 100))
                            throw new FileFormatException("Invalid Potential");
                        footballer.Potential = potential;
                        break;
                }
            }

            return footballer;
        }

        /// <summary>
        /// Checks first row in the csv file
        /// </summary>
        /// <param name="row">First row</param>
        /// <returns>
        /// true - if correct
        /// false - if columns in wrong the order or name of columns wrong
        /// </returns>
        bool CheckFirstRow(string row)
        {
            string[] str = row.Split(new[] { ';' });

            // Checking next conditions
            return str.Length == 12 &&
                   str[0] == "sofifa_id" &&
                   str[1] == "player_url" &&
                   str[2] == "short_name" &&
                   str[3] == "long_name" &&
                   str[4] == "age" &&
                   str[5] == "dob" &&
                   str[6] == "height_cm" &&
                   str[7] == "weight_kg" &&
                   str[8] == "nationality" &&
                   str[9] == "club" &&
                   str[10] == "overall" &&
                   str[11] == "potential";
        }
    }
}
