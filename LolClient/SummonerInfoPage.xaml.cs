using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Camille.Enums;
using Camille.RiotGames;
using Camille.RiotGames.MatchV5;
using Camille.RiotGames.SummonerV4;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LolClient;

/// <summary>
///     An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SummonerInfoPage : Page
{
    public SummonerInfoPage()
    {
        InitializeComponent();
        matchList = null;
    }

    public List<Match> matchList { get; set; }

    private RiotGamesApi RiotGamesApi { get; set; }

    private List<Match> GetMatchList(string[] matchIds)
    {
        var matches = new List<Match>();
        foreach (var matchId in matchIds) matches.Add(RiotGamesApi.MatchV5().GetMatch(RegionalRoute.EUROPE, matchId));

        return matches;
    }

    private Dictionary<string, List<Match>> BestChampions(List<Match> matchList, string summonerName)
    {
        var champs = new Dictionary<string, List<Match>>();
        foreach (var match in matchList)
        foreach (var participant in match!.Info.Participants)
            if (participant.SummonerName == summonerName)
            {
                if (!champs.Keys.Contains(participant.ChampionName))
                    champs.Add(participant.ChampionName, new List<Match> { match });
                else
                    champs[participant.ChampionName].Add(match);
            }

        return champs;
    }


    private void DisplayTitle(Summoner summoner)
    {
        SummonerNameTextBlock.Text = "Statistiques de " + summoner!.Name;
    }

    private void DisplayBestChampions(RiotGamesApi api, Summoner summoner)
    {
        var matchIdsByPuuid = api.MatchV5().GetMatchIdsByPUUID(RegionalRoute.EUROPE, summoner.Puuid, 30);
        matchList = GetMatchList(matchIdsByPuuid);
        var topChamps = BestChampions(matchList, summoner.Name);
        var champs = topChamps.ToList();
        var topChampsListSorted =
            new List<KeyValuePair<string, List<Match>>>();

        foreach (var i in Enumerable.Range(0, 3))
        {
            var j = 1;
            foreach (var keyValuePair in champs)
                if (keyValuePair.Value.Count > j)
                {
                    j = keyValuePair.Value.Count();
                    topChampsListSorted.Add(keyValuePair);
                }

            champs.Remove(topChampsListSorted.Last());
        }

        topChampsListSorted = topChampsListSorted.Take(3).ToList();

        foreach (var keyValuePair in topChampsListSorted)
        {
            var champGrid = new Grid();
            champGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            champGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            champGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            champGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });


            var champName = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = $"{keyValuePair.Key}",
                FontFamily = new FontFamily("/Assets/Fonts/spiegel.ttf#Spiegel"),
                FontSize = 18
            };

            Image champIcon = null;

            foreach (var participant in keyValuePair.Value[0].Info.Participants)
                if (participant.SummonerName == summoner.Name)
                    champIcon = new Image
                    {
                        Source = new BitmapImage(new Uri($"http://ddragon.leagueoflegends.com/cdn/13.17.1/" +
                                                         $"img/champion/{participant.ChampionName}.png",
                            UriKind.Absolute)),
                        Width = 50
                    };

            double killRatio = 0;
            double deathRatio = 0;
            double assistRatio = 0;
            double winRate = 0;
            var nGames = keyValuePair.Value.Count();

            foreach (var match in keyValuePair.Value)
            foreach (var participant in match.Info.Participants)
                if (participant.SummonerName == summoner.Name)
                {
                    killRatio += participant.Kills;
                    deathRatio += participant.Deaths;
                    assistRatio += participant.Assists;
                    if (participant.Win) winRate += 1;
                }

            killRatio = Math.Round(killRatio / nGames, 2);
            deathRatio = Math.Round(deathRatio / nGames, 2);
            assistRatio = Math.Round(assistRatio / nGames, 2);
            winRate = Math.Round(winRate / nGames * 100);

            var kda = Math.Round((killRatio + assistRatio) / deathRatio);

            var winRateLabel = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = $"{winRate} %",
                FontFamily = new FontFamily("/Assets/Fonts/spiegel.ttf#Spiegel"),
                FontSize = 18
            };

            var gamesPlayedLabel = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = $"{nGames} parties",
                FontFamily = new FontFamily("/Assets/Fonts/spiegel.ttf#Spiegel"),
                FontSize = 18
            };


            var kdaLabel = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = $"{kda} KDA",
                FontFamily = new FontFamily("/Assets/Fonts/spiegel.ttf#Spiegel"),
                FontSize = 18
            };

            var killDeathsAssistsLabel = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = $"{killRatio}/{deathRatio}/{assistRatio}",
                FontFamily = new FontFamily("/Assets/Fonts/spiegel.ttf#Spiegel"),
                FontSize = 18
            };


            var stackPanelChamp = new StackPanel();
            stackPanelChamp.Margin = new Thickness(10);
            stackPanelChamp.Orientation = Orientation.Horizontal;

            stackPanelChamp.Children.Add(champIcon);
            stackPanelChamp.Children.Add(champName);

            var stackPanelKda = new StackPanel();
            stackPanelKda.Margin = new Thickness(10);
            stackPanelKda.Orientation = Orientation.Vertical;

            stackPanelKda.Children.Add(kdaLabel);
            stackPanelKda.Children.Add(killDeathsAssistsLabel);

            var stackPanelGames = new StackPanel();
            stackPanelGames.Margin = new Thickness(10);
            stackPanelGames.Orientation = Orientation.Vertical;

            stackPanelGames.Children.Add(winRateLabel);
            stackPanelGames.Children.Add(gamesPlayedLabel);

            Grid.SetColumn(stackPanelChamp, 0);
            champGrid.Children.Add(stackPanelChamp);
            Grid.SetColumn(stackPanelKda, 1);
            champGrid.Children.Add(stackPanelKda);
            Grid.SetColumn(stackPanelGames, 2);
            champGrid.Children.Add(stackPanelGames);
            BestChamps.Children.Add(champGrid);
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter != null && e.Parameter is List<object>)
        {
            var parameters = (List<object>)e.Parameter;


            RiotGamesApi = (RiotGamesApi)parameters.ElementAt(0);


            var summoner = RiotGamesApi.SummonerV4()
                .GetBySummonerName(PlatformRoute.EUW1, (string)parameters.ElementAt(1));

            DisplayTitle(summoner);
            DisplayBestChampions(RiotGamesApi, summoner);

            foreach (var match in matchList)
            {
                var matchStackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(10)
                };

                foreach (var participant in match.Info.Participants)
                    if (participant.SummonerName == summoner!.Name)
                    {
                        if (participant.Win) matchStackPanel.Background = new SolidColorBrush(Colors.Azure);

                        var champIcon = new Image
                        {
                            Source = new BitmapImage(new Uri($"http://ddragon.leagueoflegends.com/cdn/13.17.1/img" +
                                                             $"/champion/{participant.ChampionName}.png")),
                            Width = 50
                        };
                        var sumsGrid = new Grid();

                        sumsGrid.ColumnDefinitions.Add(new ColumnDefinition
                            { Width = new GridLength(25, GridUnitType.Pixel) });
                        sumsGrid.ColumnDefinitions.Add(new ColumnDefinition
                            { Width = new GridLength(25, GridUnitType.Pixel) });
                        sumsGrid.RowDefinitions.Add(new RowDefinition
                            { Height = new GridLength(25, GridUnitType.Pixel) });
                        sumsGrid.RowDefinitions.Add(new RowDefinition
                            { Height = new GridLength(25, GridUnitType.Pixel) });

                        Debug.Write(
                            $"http://ddragon.leagueoflegends.com/cdn/13.17.1/img/spell/{participant.Summoner1Id}.png\"");

                        var firstSummonerIcon = new Image
                        {
                            Source = new BitmapImage(new Uri(
                                $"http://ddragon.leagueoflegends.com/cdn/13.17.1/img/spell/{participant.Summoner1Id}.png")),
                            Width = 25
                        };

                        var secondSummonerIcon = new Image
                        {
                            Source = new BitmapImage(new Uri(
                                $"http://ddragon.leagueoflegends.com/cdn/13.17.1/img/spell/{participant.Summoner2Id}.png")),
                            Width = 25
                        };

                        // Image firstSummonerRunes = new Image()
                        // {
                        //     Source = new BitmapImage(new Uri(
                        //         $"https://ddragon.leagueoflegends.com/cdn/img/perk-images/Styles/{participant.}.png")),
                        //     Width = 25
                        // };
                        Grid.SetColumn(firstSummonerIcon, 1);
                        Grid.SetRow(firstSummonerIcon, 0);
                        sumsGrid.Children.Add(firstSummonerIcon);
                        Grid.SetColumn(secondSummonerIcon, 1);
                        Grid.SetRow(secondSummonerIcon, 1);
                        sumsGrid.Children.Add(secondSummonerIcon);

                        matchStackPanel.Children.Add(champIcon);
                        matchStackPanel.Children.Add(sumsGrid);
                    }

                Historic.Children.Add(matchStackPanel);
            }
        }

        base.OnNavigatedTo(e);
    }

    private void BackToMainMenu_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(HomePage));
    }
}