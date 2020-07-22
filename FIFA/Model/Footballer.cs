using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace FIFA.Model
{
    public class Footballer : IDataErrorInfo, IEditableObject
    {
        public string ShortName { get; set; }
        public string LongName { get; set; }
        public int Age { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public string Nationality { get; set; }
        public string Club { get; set; }
        public int Overall { get; set; }
        public int Potential { get; set; }
        public int SofifaID { get; set; }
        public string PlayerURL { get; set; }

        #region For value validation 

        [JsonIgnore]
        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                return columnName switch
                {
                    "ShortName" when string.IsNullOrEmpty(ShortName.Trim()) => "Short Name can't be empty",

                    "LongName" when string.IsNullOrEmpty(LongName.Trim()) => "Long Name can't be empty",

                    "Age" when Age < 18 || Age > 100 => "Age must be between [18, 100]",

                    "Height" when Height < 100 || Height > 300 => "Height must be between [100, 300]",

                    "Weight" when Weight < 40 || Weight > 200 => "Weight must be between [40, 200]",

                    "Club" when string.IsNullOrEmpty(Club.Trim()) => "Club can't be empty",

                    "Nationality" when string.IsNullOrEmpty(Nationality.Trim()) => "Nationality can't be empty",

                    "Overall" when Overall < 0 || Overall > 100 => "Overall must be between [0, 100]",

                    "Potential" when Potential < 0 || Potential > 100 => "Potential must be between [0, 100]",

                    "SofifaID" when SofifaID < 0 => "SofifaID can't be negative",

                    "PlayerURL" when string.IsNullOrEmpty(PlayerURL.Trim()) => "Player URL can't be empty",

                    _ => null
                };
            }
        }

        #endregion

        #region For canceling invalid value

        private Footballer backupCopy;
        [JsonIgnore]
        public bool InEdit { get; private set; }

        public void BeginEdit()
        {
            if (InEdit) return;
            InEdit = true;
            backupCopy = MemberwiseClone() as Footballer;
        }

        public void CancelEdit()
        {
            if (!InEdit) return;
            InEdit = false;
            ShortName = backupCopy.ShortName;
            LongName = backupCopy.LongName;
            Age = backupCopy.Age;
            DateOfBirth = backupCopy.DateOfBirth;
            Height = backupCopy.Height;
            Weight = backupCopy.Weight;
            Nationality = backupCopy.Nationality;
            Club = backupCopy.Club;
            Overall = backupCopy.Overall;
            Potential = backupCopy.Potential;
            SofifaID = backupCopy.SofifaID;
            PlayerURL = backupCopy.PlayerURL;
        }

        public void EndEdit()
        {
            if (!InEdit) return;
            InEdit = false;
            backupCopy = null;
        }

        #endregion
    }
}
