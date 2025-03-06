using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.Messages;
using LAN_Fileshare.Stores;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LAN_Fileshare.ViewModels
{
    public class HostListingViewModel : ViewModelBase, IRecipient<HostAddedMessage>, IDisposable
    {
        private readonly ObservableCollection<HostListingItemViewModel> _hostListingItemViewModels;
        public IEnumerable<HostListingItemViewModel> HostListingItemViewModels => _hostListingItemViewModels;
        public bool IsAnyHostAvailable => _hostListingItemViewModels != null ? _hostListingItemViewModels.Count > 0 : false;

        public HostListingViewModel(AppStateStore appStateStore)
        {
            _hostListingItemViewModels = new();

            StrongReferenceMessenger.Default.Register<HostAddedMessage>(this);
        }

        public void Receive(HostAddedMessage message)
        {
            _hostListingItemViewModels.Add(new HostListingItemViewModel(message.Value));
        }

        public void Dispose()
        {
            StrongReferenceMessenger.Default.Unregister<HostAddedMessage>(this);
        }
    }
}
