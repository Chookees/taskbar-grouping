using FluentAssertions;
using FluentAssertions.Events;
using TaskbarFolders.Manager.ViewModels;
using Xunit;

namespace TaskbarFolders.Manager.Tests.ViewModels;

public class MainWindowViewModelTests
{
    [Fact]
    public void Title_HasDefaultValue()
    {
        var sut = new MainWindowViewModel();

        sut.Title.Should().Be("TaskbarFolders Manager");
    }

    [Fact]
    public void Title_RaisesPropertyChanged_WhenAssigned()
    {
        var sut = new MainWindowViewModel();
        using IMonitor<MainWindowViewModel> monitor = sut.Monitor();

        sut.Title = "Renamed";

        monitor.Should().RaisePropertyChangeFor(x => x.Title);
    }

    [Fact]
    public void Title_DoesNotRaisePropertyChanged_WhenSameValueAssigned()
    {
        var sut = new MainWindowViewModel { Title = "Same" };
        using IMonitor<MainWindowViewModel> monitor = sut.Monitor();

        sut.Title = "Same";

        monitor.Should().NotRaisePropertyChangeFor(x => x.Title);
    }
}
