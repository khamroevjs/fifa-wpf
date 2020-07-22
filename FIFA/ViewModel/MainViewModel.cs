using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FIFA.Model;
using FIFA.View;

namespace FIFA.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollection<Footballer> ComputerTeam { get; }
        public ObservableCollection<Footballer> UserTeam { get; }
        private List<Footballer> Footballers { get; }

        #region Filter

        private string nationalityNationalityTextBox = "";
        public string NationalityTextBox
        {
            get => nationalityNationalityTextBox;
            set
            {
                SetProperty(ref nationalityNationalityTextBox, value);
                Filter();
            }
        }

        private string overallTextBox = "";
        public string OverallTextBox
        {
            get => overallTextBox;
            set
            {
                SetProperty(ref overallTextBox, value);
                Filter();
            }
        }

        private string potentialTextBox = "";

        public string PotentialTextBox
        {
            get => potentialTextBox;
            set
            {
                SetProperty(ref potentialTextBox, value);
                Filter();
            }
        }

        void Filter()
        {
            var filtered = from item in Footballers
                           where item.Nationality.StartsWith(NationalityTextBox, StringComparison.OrdinalIgnoreCase)
                           where item.Overall.ToString().StartsWith(OverallTextBox, StringComparison.OrdinalIgnoreCase)
                           where item.Potential.ToString().StartsWith(PotentialTextBox, StringComparison.OrdinalIgnoreCase)
                           select item;

            ComputerTeam.Clear();

            foreach (var item in filtered)
                ComputerTeam.Add(item);

            OnPropertyChanged("ComputerTeam");
        }

        #endregion

        #region Selection (due to this user can select multiple elements)

        private IList computerSelectedItems = new ArrayList();
        public IList ComputerSelectedItems
        {
            get => computerSelectedItems;
            set => SetProperty(ref computerSelectedItems, value);
        }

        private IList userSelectedItems = new ArrayList();
        public IList UserSelectedItems
        {
            get => userSelectedItems;
            set => SetProperty(ref userSelectedItems, value);
        }

        #endregion

        #region Start Game Button
        private bool startGameIsEnabled;
        public bool StartGameIsEnabled
        {
            get => startGameIsEnabled;
            set => SetProperty(ref startGameIsEnabled, value);
        }

        #endregion

        #region Commmands

        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand CurrentCellChangedCommand { get; }
        public ICommand StartGameCommand { get; }

        #endregion

        public MainViewModel(List<Footballer> computerTeam, List<Footballer> userTeam)
        {
            AddCommand = new Command(Add);
            RemoveCommand = new Command(Remove);
            CurrentCellChangedCommand = new Command(CurrentCellChanged);
            StartGameCommand = new Command(StartGame);

            ComputerTeam = new ObservableCollection<Footballer>(computerTeam);
            UserTeam = new ObservableCollection<Footballer>(userTeam);
            Footballers = computerTeam;
        }

        /// <summary>
        /// If there is invalid input in cell, "Start Game" button will be disabled
        /// </summary>
        void CurrentCellChanged()
        {
            StartGameIsEnabled = UserTeam.All(item => item.InEdit == false);
        }

        #region Add, Remove, Start methods

        void Add()
        {
            int count = ComputerSelectedItems.Count;
            for (int i = 0; i < count; i++)
            {
                UserTeam.Add((Footballer)ComputerSelectedItems[0]);
                Footballers.Remove((Footballer)ComputerSelectedItems[0]);
                ComputerTeam.Remove((Footballer)ComputerSelectedItems[0]);
            }

            StartGameIsEnabled = UserTeam.Count == 11;
        }

        void Remove()
        {
            int count = UserSelectedItems.Count;
            for (int i = 0; i < count; i++)
            {
                Footballers.Add((Footballer)UserSelectedItems[0]);
                ComputerTeam.Add((Footballer)UserSelectedItems[0]);
                UserTeam.Remove((Footballer)UserSelectedItems[0]);
            }

            StartGameIsEnabled = UserTeam.Count == 11;
        }

        void StartGame()
        {
            new GameplayWindow(new GameplayViewModel(Footballers, UserTeam)).Show();
            Application.Current.Windows[0]?.Close();
        }

        #endregion
    }
}
