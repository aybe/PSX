using Microsoft.Extensions.Options;
using Microsoft.Toolkit.Mvvm.Input;
using PSX.Frontend.Services.Dialog;
using PSX.Frontend.Services.Navigation;

namespace PSX.Frontend.Interface;

public sealed class MainViewModelCommands
{
    public MainViewModelCommands(MainModel model, IDialogService dialogService, INavigationService navigationService, IOptions<AppSettings> settings)
    {
        Shutdown = new RelayCommand(
            model.Shutdown
        );

        OpenFile = new RelayCommand(
            () =>
            {
                model.OpenFile();
                NotifyCanExecuteChanged(EmuStart, EmuStop, EmuPause, EmuFrame, EmuContinue);
            }
        );

        OpenFileDirect = new RelayCommand<string>(
            path =>
            {
                if (File.Exists(path))
                {
                    // TODO the file should become first in position
                    NotifyCanExecuteChanged(EmuStart, EmuStop, EmuPause, EmuFrame, EmuContinue);
                }
                else
                {
                    var message = $"The file cannot '{path}' be opened. Do you want to remove the reference to it from the Recent list?";

                    var result = dialogService.Show("PSX", message, DialogButton.YesNo, DialogImage.Error);

                    if (result is DialogResult.Yes)
                    {
                        settings.Value.RecentlyUsed.Remove(path!);
                    }
                }
            }
        );

        ShowVideoScreen = new RelayCommand(
            navigationService.Navigate<IVideoScreenView>
        );

        ShowVideoMemory = new RelayCommand(
            navigationService.Navigate<IVideoMemoryView>
        );

        EmuStart = new RelayCommand(
            () =>
            {
                model.Start();
                NotifyCanExecuteChanged(EmuStart, EmuStop, EmuPause, EmuFrame, EmuContinue);
            },
            () => model.CanStart
        );

        EmuStop = new RelayCommand(
            () =>
            {
                model.Stop();
                NotifyCanExecuteChanged(EmuStart, EmuStop);
            },
            () => model.CanStop
        );

        EmuPause = new RelayCommand(
            () =>
            {
                model.Pause();
                NotifyCanExecuteChanged(EmuPause, EmuFrame, EmuContinue);
            },
            () => model.CanPause
        );

        EmuFrame = new RelayCommand(
            () =>
            {
                model.Frame();
                NotifyCanExecuteChanged(EmuPause, EmuFrame, EmuContinue);
            },
            () => model.CanFrame
        );

        EmuContinue = new RelayCommand(
            () =>
            {
                model.Continue();
                NotifyCanExecuteChanged(EmuPause, EmuFrame, EmuContinue);
            },
            () => model.CanContinue
        );

        static void NotifyCanExecuteChanged(params IRelayCommand?[] commands)
        {
            foreach (var command in commands)
            {
                command?.NotifyCanExecuteChanged();
            }
        }
    }

    public IRelayCommand EmuStart { get; }

    public IRelayCommand EmuStop { get; }

    public IRelayCommand EmuPause { get; }

    public IRelayCommand EmuFrame { get; }

    public IRelayCommand EmuContinue { get; }

    public IRelayCommand ShowVideoScreen { get; }

    public IRelayCommand ShowVideoMemory { get; }

    public IRelayCommand OpenFile { get; }

    public IRelayCommand<string> OpenFileDirect { get; }

    public IRelayCommand Shutdown { get; }
}