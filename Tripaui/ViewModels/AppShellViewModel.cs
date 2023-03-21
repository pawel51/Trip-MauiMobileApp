using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tripaui.Messages;

namespace Tripaui.ViewModels
{
    public partial class AppShellViewModel : BaseViewModel, IRecipient<OpenFlyOutMessage>
    {
        [ObservableProperty]
        bool flyOutIsPresented = false;

        public AppShellViewModel()
        {
            WeakReferenceMessenger.Default.Register(this);
        }

        public void Receive(OpenFlyOutMessage message)
        {
            FlyOutIsPresented = message.Value;
        }
    }
}
