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
                
                string[] matchIdsByPuuid = RiotGamesApi.MatchV5().GetMatchIdsByPUUID(RegionalRoute.EUROPE, summoner.Puuid, count: 10);
                
                Dictionary<string, List<Match>> topChamps = BestChampions(matchIdsByPuuid, summoner.Name);
                Dictionary<string, List<Match>> bestChamp = null;
                int maxGames = 0;
                foreach (KeyValuePair<string,List<Match>> keyValuePair in topChamps)
                {
                    if (keyValuePair.Value.Count > maxGames)
                    {
                        maxGames = keyValuePair.Value.Count;
                        bestChamp = new Dictionary<string, List<Match>>() {{keyValuePair.Key, keyValuePair.Value}};
                    }
                }

                BestChampionName.Text = bestChamp!.ToList()[0].Key;
                List<Match> champMatches = bestChamp.ToList()[0].Value;
                int nWins = 0;
                int nLosses = 0;
                int nMatches = champMatches.Count;
                foreach (Match champMatch in champMatches)
                {
                    foreach (Participant participant in champMatch.Info.Participants)
                    {
                        if (participant.SummonerName == summoner.Name)
                        {
                            if (participant.Win)
                            {
                                nWins += 1;
                            }
                            else
                            {
                                nLosses += 1;
                            }
                        }
                    }
                }
                BestChampionIcon.Source =
                    new BitmapImage(
                        new Uri($"http://ddragon.leagueoflegends.com/cdn/13.17.1/img/champion/{BestChampionName.Text}.png", UriKind.Absolute));
                BestChampionWins.Text = nWins + " Victoires";
                BestChampionLosses.Text = nLosses + " Défaites";
                BestChampionGames.Text = nMatches + " Parties jouées";
            }
            
            base.OnNavigatedTo(e);
        }
        
        private void BackToMainMenu_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage));
        }
    }
}
