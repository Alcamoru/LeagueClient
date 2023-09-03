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
using Camille.RiotGames;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LolClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {

        public HomePage()
        {
            var streamReader =
                new StreamReader(
                    @"C:\Users\alcam\OneDrive\Documents\Developpement\win-projects\LolClient\LolClient\RIOT_TOKEN.txt");
            var token = streamReader.ReadLine();
            this.InitializeComponent();
            RiotApi = RiotGamesApi.NewInstance(token!);
        }

        private RiotGamesApi RiotApi { get; set; }

        private void SendSummonerName_OnClick(object sender, RoutedEventArgs e)
        {
            string summonerNameEntry = SummonerNameEntry.Text;
            var parameters = new List<Object>();
            parameters.Add(RiotApi);
            parameters.Add(summonerNameEntry);
            Frame.Navigate(typeof(SummonerInfoPage), parameters);
        }
    }
}
