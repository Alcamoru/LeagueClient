using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Camille.Enums;
using Camille.RiotGames;
using Camille.RiotGames.MatchV5;
using Camille.RiotGames.SummonerV4;
using Microsoft.UI.Xaml.Media.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LolClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SummonerInfoPage : Page
    {
        public SummonerInfoPage()
        {
            this.InitializeComponent();
        }
        
        private RiotGamesApi RiotGamesApi { get; set; }
        
        private Dictionary<string, List<Match>> BestChampions(string[] matchIds, string summonerName)  
        {
            Dictionary<string, List<Match>> champs = new Dictionary<string, List<Match>>();
            foreach (string matchId in matchIds)
            {
                Match match = RiotGamesApi.MatchV5().GetMatch(RegionalRoute.EUROPE, matchId);

                foreach (Participant participant in match!.Info.Participants)
                {
                    if (participant.SummonerName == summonerName)
                    {
                        if (!champs.Keys.Contains(participant.ChampionName))
                        {
                            champs.Add(participant.ChampionName, new List<Match>(){match});
                        }
                        else
                        {
                            champs[participant.ChampionName].Add(match);
                        }
                        
                    }
                }
            }
            return champs;
        }   
        

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            if (e.Parameter != null && e.Parameter is List<Object>)
            {
                var parameters = (List<Object>)e.Parameter;
                
                
                RiotGamesApi = (RiotGamesApi)parameters.ElementAt(0);
                
                
                Summoner summoner = RiotGamesApi.SummonerV4().GetBySummonerName(PlatformRoute.EUW1, (string)parameters.ElementAt(1));


                SummonerNameTextBlock.Text = "Statistiques de " + summoner!.Name;
                
                string[] matchIdsByPuuid = RiotGamesApi.MatchV5().GetMatchIdsByPUUID(RegionalRoute.EUROPE, summoner.Puuid, count: 30);
                
                Dictionary<string, List<Match>> topChamps = BestChampions(matchIdsByPuuid, summoner.Name);
                List<KeyValuePair<string, List<Match>>> champs = topChamps.ToList();
                List<KeyValuePair<string, List<Match>>> topChampsListSorted =
                    new List<KeyValuePair<string, List<Match>>>();

                foreach (int i in Enumerable.Range(0, 3))  
                {
                    int j = 1;
                    foreach (KeyValuePair<string,List<Match>> keyValuePair in champs)
                    {
                        if (keyValuePair.Value.Count > j)
                        {
                            j = keyValuePair.Value.Count();
                            topChampsListSorted.Add(keyValuePair);
                        }
                    }
                    champs.Remove(topChampsListSorted.Last());
                }

                topChampsListSorted = topChampsListSorted.Take(3).ToList();

                foreach (KeyValuePair<string,List<Match>> keyValuePair in topChampsListSorted)
                {
                    Grid grid = new Grid();
                    grid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(100, GridUnitType.Pixel)});
                    grid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(100, GridUnitType.Pixel)});
                    grid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(100, GridUnitType.Pixel)});
                    grid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(0, GridUnitType.Auto)});
                    
                    grid.RowDefinitions.Add(new RowDefinition() {Height = new GridLength(0, GridUnitType.Auto)});


                    TextBlock champName = new TextBlock()
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Text = $"{keyValuePair.Key}",
                        FontFamily=new FontFamily("/Assets/Fonts/spiegel.ttf#Spiegel"),
                        FontSize=18
                    };

                    Image champIcon = null;

                    foreach (Participant participant in keyValuePair.Value[0].Info.Participants)
                    {
                        if (participant.SummonerName == summoner.Name)
                        {
                            champIcon = new Image()
                            {
                                Source = new BitmapImage(new Uri($"http://ddragon.leagueoflegends.com/cdn/13.17.1/" +
                                                                 $"img/champion/{participant.ChampionName}.png",
                                    UriKind.Absolute))
                            };
                        }
                    }

                    int nWins = 0;
                    int nLoses = 0;
                    int nGames = keyValuePair.Value.Count();
                    
                    foreach (Match match in keyValuePair.Value)
                    {
                        foreach (Participant participant in match.Info.Participants)
                        {
                            if (participant.SummonerName == summoner.Name)  
                            {
                                if (participant.Win)
                                {
                                    nWins += 1;
                                }
                                else
                                {
                                    nLoses += 1;
                                }
                            }
                        }
                    }
                    
                    TextBlock champWins = new TextBlock()
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Text = $"{nWins} Victoires",
                        FontFamily=new FontFamily("/Assets/Fonts/spiegel.ttf#Spiegel"),
                        FontSize=18
                    };
                    
                    TextBlock champLoses= new TextBlock()
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Text = $"{nLoses} Défaites",
                        FontFamily=new FontFamily("/Assets/Fonts/spiegel.ttf#Spiegel"),
                        FontSize=18
                    };
                    
                    TextBlock champGames = new TextBlock()
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Text = $"{nGames} Parties jouées",
                        FontFamily=new FontFamily("/Assets/Fonts/spiegel.ttf#Spiegel"),
                        FontSize=18
                    };
                    
                    
                    StackPanel stackPanelChamp = new StackPanel();
                    stackPanelChamp.Margin = new Thickness(10);
                    stackPanelChamp.Orientation = Orientation.Vertical;
                    
                    stackPanelChamp.Children.Add(champName);
                    stackPanelChamp.Children.Add(champIcon);
                    
                    Grid.SetColumn(stackPanelChamp, 0);
                    grid.Children.Add(stackPanelChamp);
                    Grid.SetColumn(champWins, 1);
                    grid.Children.Add(champWins);
                    Grid.SetColumn(champLoses, 2);
                    grid.Children.Add(champLoses);
                    Grid.SetColumn(champGames, 3);
                    grid.Children.Add(champGames);
                    BestChamps.Children.Add(grid);
                }
                
            }
            
            base.OnNavigatedTo(e);
        }
        
        private void BackToMainMenu_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage));
        }
    }
}
