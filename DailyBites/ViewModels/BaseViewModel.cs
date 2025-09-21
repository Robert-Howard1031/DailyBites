using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyBites.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        /// <summary>
        /// The IsBusy property is available to all ViewModels. However, avoid setting it's state directly
        /// from within the ViewModel. Instead, use the ExecuteWithBusyIndicator method to wrap command logic.
        /// Also, If a ViewModel only has one command that will create a busy state, use an IAsyncRelayCommand
        /// instead and bind all controls that need to be disabled its IsRunning property.
        /// </summary>
        [ObservableProperty]
        bool _isBusy;

        /// <summary>
        /// Instead of manually setting IsBusy within command methods on individual view models,
        /// we can use this method to wrap the command logic and set the IsBusy while executing.
        /// </summary>
        protected async Task ExecuteWithBusyIndicator(Func<Task> action)
        {
            try
            {
                _isBusy = true;
                await action();
            }
            finally
            {
                _isBusy = false;
            }
        }
    }
}
