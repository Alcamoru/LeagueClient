using System.Collections.Generic;
using System.IO;
using Camille.RiotGames;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LolClient;

/// <summary>
///     An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class HomePage : Page
{
    public HomePage()
    {
        var streamReader =
            new StreamReader(
                @"C:\Users\alcam\OneDrive\Documents\Developpement\win-projects\LolClient\LolClient\RIOT_TOKEN.txt");
        var token = streamReader.ReadLine();
        InitializeComponent();
        RiotApi = RiotGamesApi.NewInstance(token!);
    }

    private RiotGamesApi RiotApi { get; }

    private void SendSummonerName_OnClick(object sender, RoutedEventArgs e)
    {
        var summonerNameEntry = SummonerNameEntry.Text;
        var parameters = new List<object>();
        parameters.Add(RiotApi);
        parameters.Add(summonerNameEntry);
        Frame.Navigate(typeof(SummonerInfoPage), parameters);
    }
}