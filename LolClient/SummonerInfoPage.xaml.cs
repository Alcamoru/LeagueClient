using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Text;
using Camille.Enums;
using Camille.RiotGames;
using Camille.RiotGames.MatchV5;
using Camille.RiotGames.SummonerV4;
using Microsoft.UI;
using Microsoft.UI.Text;
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
                
                TextBlock gameModeTextBlock = new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontFamily = new FontFamily("/Assets/Fonts/spiegel.ttf#Spiegel"),
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(10)
                };
                
                if (match.Info.GameMode == GameMode.CLASSIC & match.Info.GameType == GameType.MATCHED_GAME)
                {
                    gameModeTextBlock.Text = "Ranked Solo";
                }
                else if (match.Info.GameMode == GameMode.ARAM & match.Info.GameType == GameType.MATCHED_GAME)
                {
                    gameModeTextBlock.Text = "Aram";
                }
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
                        var sumsGrid = new Grid()
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

                        Dictionary<string, string> sumsCorrespondences = new Dictionary<string, string>();
                        sumsCorrespondences.Add("21", "SummonerBarrier");
                        sumsCorrespondences.Add("1", "SummonerBoost");
                        sumsCorrespondences.Add("2202", "SummonerCherryFlash");
                        sumsCorrespondences.Add("2201", "SummonerCherryHold");
                        sumsCorrespondences.Add("14", "SummonerDot");
                        sumsCorrespondences.Add("3", "SummonerExhaust");
                        sumsCorrespondences.Add("4", "SummonerFlash");
                        sumsCorrespondences.Add("6", "SummonerHaste");
                        sumsCorrespondences.Add("7", "SummonerHeal");
                        sumsCorrespondences.Add("13", "SummonerMana");
                        sumsCorrespondences.Add("30", "SummonerPoroRecall");
                        sumsCorrespondences.Add("31", "SummonerPoroThrow");
                        sumsCorrespondences.Add("11", "SummonerSmite");
                        sumsCorrespondences.Add("39", "SummonerSnowURFSnowball_Mark");
                        sumsCorrespondences.Add("32", "SummonerSnowball");
                        sumsCorrespondences.Add("12", "SummonerTeleport");
                        sumsCorrespondences.Add("54", "Summoner_UltBookPlaceholder");
                        sumsCorrespondences.Add("55", "Summoner_UltBookSmitePlaceholder");
                        
                        Dictionary<string, List<string>> mainPerksCorrespondences = new Dictionary<string, List<string>>();
                        mainPerksCorrespondences.Add("8112", new List<string>(){"Domination", "Electrocute"});
                        mainPerksCorrespondences.Add("8124", new List<string>(){"Domination", "Predator"});
                        mainPerksCorrespondences.Add("8128", new List<string>(){"Domination", "DarkHarvest"});
                        mainPerksCorrespondences.Add("9923", new List<string>(){"Domination", "HailOfBlades"});
                        mainPerksCorrespondences.Add("8351", new List<string>(){"Inspiration", "GlacialAugment"});
                        mainPerksCorrespondences.Add("8360", new List<string>(){ "Inspiration","UnsealedSpellbook"});
                        mainPerksCorrespondences.Add("8369", new List<string>(){"Inspiration","FirstStrike"});
                        mainPerksCorrespondences.Add("8005", new List<string>(){"Precision", "PressTheAttack"});
                        mainPerksCorrespondences.Add("8008", new List<string>(){"Precision", "LethalTempo"});
                        mainPerksCorrespondences.Add("8021", new List<string>(){"Precision", "FleetFootwork"});
                        mainPerksCorrespondences.Add("8010", new List<string>(){"Precision", "Conqueror"});
                        mainPerksCorrespondences.Add("8437", new List<string>(){"Resolve", "GraspOfTheUndying"});
                        mainPerksCorrespondences.Add("8439", new List<string>(){"Resolve", "VeteranAftershock"});
                        mainPerksCorrespondences.Add("8465", new List<string>(){"Resolve", "Guardian"});
                        mainPerksCorrespondences.Add("8214", new List<string>(){"Sorcery", "SummonAery"});
                        mainPerksCorrespondences.Add("8229", new List<string>(){"Sorcery" ,"ArcaneComet"});
                        mainPerksCorrespondences.Add("8230", new List<string>(){"Sorcery", "PhaseRush"});
                        
                        Dictionary<string, string> perksCategories = new Dictionary<string, string>();
                        perksCategories.Add("8100", "perk-images/Styles/7200_Domination.png");
                        perksCategories.Add("8300", "perk-images/Styles/7203_Whimsy.png");
                        perksCategories.Add("8000", "perk-images/Styles/7201_Precision.png");
                        perksCategories.Add("8400", "perk-images/Styles/7204_Resolve.png");
                        perksCategories.Add("8200", "perk-images/Styles/7202_Sorcery.png");
                        
                        Image firstSummonerIcon = new Image
                        {
                            Source = new BitmapImage(new Uri($"http://ddragon.leagueoflegends.com/cdn/13.17.1/img/spell/{sumsCorrespondences[participant.Summoner1Id.ToString()]}.png")),
                            Width = 25
                        };

                        Image secondSummonerIcon = new Image
                        {
                            Source = new BitmapImage(new Uri(
                                $"http://ddragon.leagueoflegends.com/cdn/13.17.1/img/spell/{sumsCorrespondences[participant.Summoner2Id.ToString()]}.png")),
                            Width = 25
                        };

                        string source =
                            $"https://ddragon.leagueoflegends.com/cdn/img/perk-images/Styles/{mainPerksCorrespondences[participant.Perks.Styles[0].Selections[0].Perk.ToString()][0]}/{mainPerksCorrespondences[participant.Perks.Styles[0].Selections[0].Perk.ToString()][1]}/{mainPerksCorrespondences[participant.Perks.Styles[0].Selections[0].Perk.ToString()][1]}.png";
                        
                        Image firstPerkIcon = new Image()
                        {
                            Source = new BitmapImage(new Uri(source)),
                            Width = 25
                        };

                        source = $"https://ddragon.leagueoflegends.com/cdn/img/{perksCategories[participant.Perks.Styles[1].Style.ToString()]}";

                        Image secondPerkIcon = new Image()
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

                        matchStackPanel.Children.Add(champIcon);
                        matchStackPanel.Children.Add(sumsGrid);

                        if (participant.Win)
                        {
                            matchStackPanel.Background = new SolidColorBrush(Colors.Green);
                        }
                        else
                        {
                            matchStackPanel.Background = new SolidColorBrush(Colors.Red);
                        }

                        matchStackPanel.Padding = new Thickness(15);
                        matchStackPanel.CornerRadius = new CornerRadius(15);
                        
                        StackPanel statsStackPanel = new StackPanel()
                        {
                            Orientation = Orientation.Vertical,
                            Margin = new Thickness(10)
                        };

                        string kda = $"{participant.Kills} / {participant.Deaths} / {participant.Assists}";
                        
                        TextBlock kdaTextBlock = new TextBlock()
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
                        {
                            kdaNumber = (participant.Kills + participant.Assists) / 1;
                        }
                        else
                        {
                            kdaNumber = (participant.Kills + participant.Assists) / participant.Deaths;
                        }
                        kdaNumber = Math.Round(kdaNumber);
                        TextBlock kdaNumberTextBlock = new TextBlock()
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontFamily = new FontFamily("/Assets/Fonts/spiegel.ttf#Spiegel"),
                            FontSize = 14,
                            Text = kdaNumber + " KDA",
                            Margin = new Thickness(5)
                        };
                        double csMin = participant.TotalMinionsKilled / (participant.Challenges!.GameLength.Value / 60);
                        csMin = Math.Round(csMin, 2);
                        
                        TextBlock csNumberTextBlock = new TextBlock()
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontFamily = new FontFamily("/Assets/Fonts/spiegel.ttf#Spiegel"),
                            FontSize = 14,
                            Text = participant.TotalMinionsKilled + $" ({csMin} CS/min)",
                            Margin = new Thickness(5)
                        };
                        
                        statsStackPanel.Children.Add(kdaNumberTextBlock);
                        statsStackPanel.Children.Add(csNumberTextBlock);
                        matchStackPanel.Children.Add(statsStackPanel);

                        Grid itemGrid = new Grid();
                        itemGrid.ColumnDefinitions.Add(new ColumnDefinition(){Width = new GridLength(25)});
                        itemGrid.ColumnDefinitions.Add(new ColumnDefinition(){Width = new GridLength(25)});
                        itemGrid.ColumnDefinitions.Add(new ColumnDefinition(){Width = new GridLength(25)});
                        itemGrid.ColumnDefinitions.Add(new ColumnDefinition(){Width = new GridLength(25)});
                        itemGrid.RowDefinitions.Add(new RowDefinition(){Height = new GridLength(25)});
                        itemGrid.RowDefinitions.Add(new RowDefinition(){Height = new GridLength(25)});
                        List<int> itemList = new List<int>();
                        itemList.Add(participant.Item0);
                        itemList.Add(participant.Item1);
                        itemList.Add(participant.Item2);
                        itemList.Add(participant.Item3);
                        itemList.Add(participant.Item4);
                        itemList.Add(participant.Item5);
                        itemList.Add(participant.Item6);
                        int i = 0;
                        
                        foreach (int item in itemList)
                        {
                            source = $"https://ddragon.leagueoflegends.com/cdn/13.17.1/img/item/{item}.png";
                            Debug.WriteLine(source);
                            Image itemIcon = new Image()
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
                        matchStackPanel.Children.Add(itemGrid);
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