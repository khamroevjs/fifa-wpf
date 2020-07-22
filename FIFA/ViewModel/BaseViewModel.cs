using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FIFA.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Changes property
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="backingStore">Reference to the field</param>
        /// <param name="value">Value</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="onChanged"></param>
        /// <returns>true - if property is changed
        /// false - if property isn't changed</returns>
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Fires when property is changed
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
