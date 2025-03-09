using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.Messages;
using LAN_Fileshare.Stores;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LAN_Fileshare.ViewModels
{
    public class HostListingViewModel : ViewModelBase, IRecipient<HostAddedMessage>, IRecipient<HostRemovedMessage>, IDisposable
    {
        private readonly ObservableCollection<HostListingItemViewModel> _hostListingItemViewModels;
        private readonly AppStateStore _appStateStore;

        public IEnumerable<HostListingItemViewModel> HostListingItemViewModels => _hostListingItemViewModels;
        public bool IsAnyHostAvailable => _hostListingItemViewModels != null ? _hostListingItemViewModels.Count > 0 : false;
        private HostListingItemViewModel _selectedHostListingItemViewModel = null!;
        public HostListingItemViewModel SelectedHostListingItemViewModel
        {
            get => _selectedHostListingItemViewModel;
            set
            {
                _selectedHostListingItemViewModel = value;
                _appStateStore.SelectedHost = value?.Host;
                OnPropertyChanged(nameof(SelectedHostListingItemViewModel));
                StrongReferenceMessenger.Default.Send(new SelectedHostChangedMessage(value!));
            }
        }

        public HostListingViewModel(AppStateStore appStateStore)
        {
            _appStateStore = appStateStore;
            _hostListingItemViewModels = new();

            StrongReferenceMessenger.Default.Register<HostAddedMessage>(this);
            StrongReferenceMessenger.Default.Register<HostRemovedMessage>(this);
        }

        public void Receive(HostAddedMessage message)
        {
            _hostListingItemViewModels.Add(new HostListingItemViewModel(message.Value));
        }
        public void Receive(HostRemovedMessage message)
        {
            HostListingItemViewModel? hostListingItemViewModel = _hostListingItemViewModels.FirstOrDefault(x => x.PhysicalAddress == message.Value.PhysicalAddress);
            if (hostListingItemViewModel != null)
            {
                _hostListingItemViewModels.Remove(hostListingItemViewModel);
            }
        }

        public void Dispose()
        {
            StrongReferenceMessenger.Default.Unregister<HostAddedMessage>(this);
            StrongReferenceMessenger.Default.Unregister<HostRemovedMessage>(this);
        }

    }
}
