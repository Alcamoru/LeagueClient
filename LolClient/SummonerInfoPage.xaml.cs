using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Windows.UI.Core;
using Camille.Enums;
using Camille.RiotGames;
using Camille.RiotGames.MatchV5;
using Camille.RiotGames.SummonerV4;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LolClient;

/// <summary>
///     An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SummonerInfoPage
{
    public SummonerInfoPage()
    {
        InitializeComponent();
        MatchList = null;
    }

    private List<Match> MatchList { get; set; }

    private RiotGamesApi RiotGamesApi { get; set; }

    private List<Match> GetMatchList(string[] matchIds)
    {
        var matches = new List<Match>();
        foreach (var matchId in matchIds) matches.Add(RiotGamesApi.MatchV5().GetMatch(RegionalRoute.EUROPE, matchId));

        return matches;
    }

    private Dictionary<string, List<Match>> BestChampions(List<Match> matchesList, string summonerName)
    {
        var champs = new Dictionary<string, List<Match>>();
        foreach (var match in matchesList)
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
        MatchList = GetMatchList(matchIdsByPuuid);
        var topChamps = BestChampions(MatchList, summoner.Name);
        var champs = topChamps.ToList();
        var topChampsListSorted =
            new List<KeyValuePair<string, List<Match>>>();

        foreach (var unused in Enumerable.Range(0, 3))
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


            var stackPanelChamp = new StackPanel
            {
                Margin = new Thickness(10),
                Orientation = Orientation.Horizontal
            };

            stackPanelChamp.Children.Add(champIcon);
            stackPanelChamp.Children.Add(champName);

            var stackPanelKda = new StackPanel
            {
                Margin = new Thickness(10),
                Orientation = Orientation.Vertical
            };

            stackPanelKda.Children.Add(kdaLabel);
            stackPanelKda.Children.Add(killDeathsAssistsLabel);

            var stackPanelGames = new StackPanel
            {
                Margin = new Thickness(10),
                Orientation = Orientation.Vertical
            };

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

    [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
    private void DisplayHistoric(Summoner summoner)
    {
        foreach (var match in MatchList)
        {
            var matchStackPanel = new Grid
            {
                Width = 500,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            matchStackPanel.ColumnDefinitions.Add(new ColumnDefinition());
            matchStackPanel.ColumnDefinitions.Add(new ColumnDefinition());
            matchStackPanel.ColumnDefinitions.Add(new ColumnDefinition());
            matchStackPanel.ColumnDefinitions.Add(new ColumnDefinition());


            var gameModeTextBlock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("/Assets/Fonts/spiegel.ttf#Spiegel"),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(10)
            };


            if ((match.Info.GameMode == GameMode.CLASSIC) & (match.Info.GameType == GameType.MATCHED_GAME))
                gameModeTextBlock.Text = "Ranked Solo";
            else if ((match.Info.GameMode == GameMode.ARAM) & (match.Info.GameType == GameType.MATCHED_GAME))
                gameModeTextBlock.Text = "Aram";

            gameModeTextBlock.PointerExited += GameModeTextBlockOnPointerExited;
            gameModeTextBlock.PointerEntered += GameModeTextBlockOnPointerEntered;
            gameModeTextBlock.PointerPressed += GameModeTextBlockOnPointerPressed;

            Grid.SetColumn(gameModeTextBlock, 0);
            matchStackPanel.Children.Add(gameModeTextBlock);

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
                    var sumsGrid = new Grid
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    sumsGrid.ColumnDefinitions.Add(new ColumnDefinition
                        { Width = new GridLength(25, GridUnitType.Pixel) });
                    sumsGrid.ColumnDefinitions.Add(new ColumnDefinition
                        { Width = new GridLength(25, GridUnitType.Pixel) });
                    sumsGrid.RowDefinitions.Add(new RowDefinition
                        { Height = new GridLength(25, GridUnitType.Pixel) });
                    sumsGrid.RowDefinitions.Add(new RowDefinition
                        { Height = new GridLength(25, GridUnitType.Pixel) });

                    var sumsCorrespondences = new Dictionary<string, string>
                    {
                        { "21", "SummonerBarrier" },
                        { "1", "SummonerBoost" },
                        { "2202", "SummonerCherryFlash" },
                        { "2201", "SummonerCherryHold" },
                        { "14", "SummonerDot" },
                        { "3", "SummonerExhaust" },
                        { "4", "SummonerFlash" },
                        { "6", "SummonerHaste" },
                        { "7", "SummonerHeal" },
                        { "13", "SummonerMana" },
                        { "30", "SummonerPoroRecall" },
                        { "31", "SummonerPoroThrow" },
                        { "11", "SummonerSmite" },
                        { "39", "SummonerSnowURFSnowball_Mark" },
                        { "32", "SummonerSnowball" },
                        { "12", "SummonerTeleport" },
                        { "54", "Summoner_UltBookPlaceholder" },
                        { "55", "Summoner_UltBookSmitePlaceholder" }
                    };

                    var mainPerksCorrespondences = new Dictionary<string, List<string>>
                    {
                        { "8112", new List<string> { "Domination", "Electrocute" } },
                        { "8124", new List<string> { "Domination", "Predator" } },
                        { "8128", new List<string> { "Domination", "DarkHarvest" } },
                        { "9923", new List<string> { "Domination", "HailOfBlades" } },
                        { "8351", new List<string> { "Inspiration", "GlacialAugment" } },
                        { "8360", new List<string> { "Inspiration", "UnsealedSpellbook" } },
                        { "8369", new List<string> { "Inspiration", "FirstStrike" } },
                        { "8005", new List<string> { "Precision", "PressTheAttack" } },
                        { "8008", new List<string> { "Precision", "LethalTempo" } },
                        { "8021", new List<string> { "Precision", "FleetFootwork" } },
                        { "8010", new List<string> { "Precision", "Conqueror" } },
                        { "8437", new List<string> { "Resolve", "GraspOfTheUndying" } },
                        { "8439", new List<string> { "Resolve", "VeteranAftershock" } },
                        { "8465", new List<string> { "Resolve", "Guardian" } },
                        { "8214", new List<string> { "Sorcery", "SummonAery" } },
                        { "8229", new List<string> { "Sorcery", "ArcaneComet" } },
                        { "8230", new List<string> { "Sorcery", "PhaseRush" } }
                    };

                    var perksCategories = new Dictionary<string, string>
                    {
                        { "8100", "perk-images/Styles/7200_Domination.png" },
                        { "8300", "perk-images/Styles/7203_Whimsy.png" },
                        { "8000", "perk-images/Styles/7201_Precision.png" },
                        { "8400", "perk-images/Styles/7204_Resolve.png" },
                        { "8200", "perk-images/Styles/7202_Sorcery.png" }
                    };

                    var firstSummonerIcon = new Image
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.17.1/img/spell/{sumsCorrespondences[participant.Summoner1Id.ToString()]}.png")),
                        Width = 25
                    };

                    var secondSummonerIcon = new Image
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.17.1/img/spell/{sumsCorrespondences[participant.Summoner2Id.ToString()]}.png")),
                        Width = 25
                    };

                    var source =
                        $"https://ddragon.leagueoflegends.com/cdn/img/perk-images/Styles/{mainPerksCorrespondences[participant.Perks.Styles[0].Selections[0].Perk.ToString()][0]}/{mainPerksCorrespondences[participant.Perks.Styles[0].Selections[0].Perk.ToString()][1]}/{mainPerksCorrespondences[participant.Perks.Styles[0].Selections[0].Perk.ToString()][1]}.png";

                    var firstPerkIcon = new Image
                    {
                        Source = new BitmapImage(new Uri(source)),
                        Width = 25
                    };

                    source =
                        $"https://ddragon.leagueoflegends.com/cdn/img/{perksCategories[participant.Perks.Styles[1].Style.ToString()]}";

                    var secondPerkIcon = new Image
                    {
                        Source = new BitmapImage(new Uri(source)),
                        Width = 20
                    };


                    Grid.SetColumn(firstSummonerIcon, 0);
                    Grid.SetRow(firstSummonerIcon, 0);
                    sumsGrid.Children.Add(firstSummonerIcon);
                    Grid.SetColumn(secondSummonerIcon, 0);
                    Grid.SetRow(secondSummonerIcon, 1);
                    sumsGrid.Children.Add(secondSummonerIcon);
                    Grid.SetColumn(firstPerkIcon, 1);
                    Grid.SetRow(firstPerkIcon, 0);
                    sumsGrid.Children.Add(firstPerkIcon);
                    Grid.SetColumn(secondPerkIcon, 1);
                    Grid.SetRow(secondPerkIcon, 1);
                    sumsGrid.Children.Add(secondPerkIcon);
                    sumsGrid.Background = new SolidColorBrush(Colors.Transparent);

                    var champIconAndSums = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };
                    champIconAndSums.Children.Add(champIcon);
                    champIconAndSums.Children.Add(sumsGrid);

                    Grid.SetColumn(champIconAndSums, 1);
                    matchStackPanel.Children.Add(champIconAndSums);

                    matchStackPanel.Background = participant.Win ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);

                    matchStackPanel.Padding = new Thickness(15);
                    matchStackPanel.CornerRadius = new CornerRadius(15);

                    var statsStackPanel = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Margin = new Thickness(10)
                    };

                    var kda = $"{participant.Kills} / {participant.Deaths} / {participant.Assists}";

                    var kdaTextBlock = new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontFamily = new FontFamily("/Assets/Fonts/spiegel.ttf#Spiegel"),
                        FontSize = 18,
                        Text = kda,
                        Margin = new Thickness(5)
                    };
                    statsStackPanel.Children.Add(kdaTextBlock);
                    double kdaNumber;
                    if (participant.Deaths == 0)
                        kdaNumber = (participant.Kills + participant.Assists) / 1;
                    else
                        kdaNumber = (participant.Kills + participant.Assists) / participant.Deaths;
                    kdaNumber = Math.Round(kdaNumber);
                    var kdaNumberTextBlock = new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontFamily = new FontFamily("/Assets/Fonts/spiegel.ttf#Spiegel"),
                        FontSize = 14,
                        Text = kdaNumber + " KDA",
                        Margin = new Thickness(5)
                    };
                    // ReSharper disable once PossibleInvalidOperationException
                    var csMin = participant.TotalMinionsKilled / (participant.Challenges!.GameLength.Value / 60);
                    csMin = Math.Round(csMin, 2);

                    var csNumberTextBlock = new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontFamily = new FontFamily("/Assets/Fonts/spiegel.ttf#Spiegel"),
                        FontSize = 14,
                        Text = participant.TotalMinionsKilled + $" CS ({csMin})",
                        Margin = new Thickness(5)
                    };

                    statsStackPanel.Children.Add(kdaNumberTextBlock);
                    statsStackPanel.Children.Add(csNumberTextBlock);

                    Grid.SetColumn(statsStackPanel, 2);
                    matchStackPanel.Children.Add(statsStackPanel);

                    var itemGrid = new Grid
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    itemGrid.HorizontalAlignment = HorizontalAlignment.Center;
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(25) });
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(25) });
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(25) });
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(25) });
                    itemGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25) });
                    itemGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25) });
                    var itemList = new List<int>
                    {
                        participant.Item0,
                        participant.Item1,
                        participant.Item2,
                        participant.Item3,
                        participant.Item4,
                        participant.Item5,
                        participant.Item6
                    };
                    var i = 0;

                    foreach (var item in itemList)
                    {
                        source = $"https://ddragon.leagueoflegends.com/cdn/13.17.1/img/item/{item}.png";
                        Debug.WriteLine(source);
                        var itemIcon = new Image
                        {
                            Source = new BitmapImage(
                                new Uri(source)),
                            Width = 25
                        };


                        if (i <= 3)
                        {
                            Grid.SetColumn(itemIcon, i);
                            Grid.SetRow(itemIcon, 0);
                            itemGrid.Children.Add(itemIcon);
                        }
                        else
                        {
                            Grid.SetColumn(itemIcon, i - 4);
                            Grid.SetRow(itemIcon, 1);
                            itemGrid.Children.Add(itemIcon);
                        }

                        i++;
                    }

                    Grid.SetColumn(itemGrid, 3);
                    matchStackPanel.Children.Add(itemGrid);
                }

            Historic.Children.Add(matchStackPanel);
        }
    }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is not List<object> parameters) return;
        RiotGamesApi = (RiotGamesApi)parameters.ElementAt(0);


        var summoner = RiotGamesApi.SummonerV4()
            .GetBySummonerName(PlatformRoute.EUW1, (string)parameters.ElementAt(1));

        DisplayTitle(summoner);
        DisplayBestChampions(RiotGamesApi, summoner);
        DisplayHistoric(summoner);
    }

    private void GameModeTextBlockOnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputCursor.CreateFromCoreCursor(new CoreCursor(CoreCursorType.Arrow, 1));
    }

    private void GameModeTextBlockOnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputCursor.CreateFromCoreCursor(new CoreCursor(CoreCursorType.Hand, 1));
    }

    private void GameModeTextBlockOnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        Debug.WriteLine("Pressed");
    }

    private void BackToMainMenu_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(HomePage));
    }
}