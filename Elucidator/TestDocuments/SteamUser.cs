using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using Newtonsoft.Json.Linq;
using TinySteamWrapper;

namespace TestNancy.App.Models
{
    public class SteamUser
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public List<SteamGame> Games = new List<SteamGame>();

        public List<SteamGames> GamesWithPlays
        {
            get { return Games.Where(d => d.Plays > 0); }
        }

        public static void Init()
        {
            
        }

        public void WhatYouDo()
        {
            
        }

        private enum PossibleGuys
        {
            CoolGuy,
            BadGuy
        }

        public async Task LoadGames()
        {
            var w = new WebClient();

            SteamProfile profile = await SteamManager.GetSteamProfileByID(Id);

            if (profile == null) { throw new ArgumentException("Invalid Profile"); }
            await SteamManager.LoadGamesForProfile(profile);

            var sb = new StringBuilder();

            var allgames = profile.Games.ToList();

            var AllGameIds = allgames.Select(d => d.App.ID.ToString());

            var GamesToLoadIds = allgames.Select(d => d.App.ID.ToString()).ToList();

            foreach (string s in GamesToLoadIds)
            {
                sb.Append(s);
                if (s != GamesToLoadIds.Last())
                {
                    sb.Append(",");
                }
            }
        }

        public SteamUser(string username)
        {
            Name = username;

            var task = new Task<Task>(async () =>
            {
                Id = await SteamManager.GetSteamIDByName(Name);
                Program.Log.Information("{@Name} = {@Id}", Name, Id);
                await LoadGames();
            });

            task.Start();
            task.Result.Wait();
            
        }
    }
}
