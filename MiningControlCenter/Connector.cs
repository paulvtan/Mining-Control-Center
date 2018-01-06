using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiningControlCenter
{
    public class Connector
    {
        HttpClient client = new HttpClient();
        static Helper.Configs configs;
        //Initalize Connector() will check which setting are avilable establish retrieve info and call appropriate display screen.
        public Connector()
        {
            configs = Helper.GetConfig();
            if (configs.DwarfPoolConfig.Active)
            {
                UserInformation userInformation = GetDwarfUserInformation(configs.DwarfPoolConfig);
                ScreenBuilder.DisplayDwarfUserStatsAsync(userInformation.DwarfUserInformation);
            }
        }

        public UserInformation GetDwarfUserInformation(Helper.DwarfPoolConfig dwarfConfig)
        {
            client.BaseAddress = new Uri(dwarfConfig.BaseUri);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.GetAsync("api?wallet=" + dwarfConfig.WalletAddress + "&email=eth@example.com").Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                string jsonString = response.Content.ReadAsStringAsync().Result;
                var jObject = Newtonsoft.Json.Linq.JObject.Parse(jsonString);
                DwarfUser dwarfUser = JsonConvert.DeserializeObject<DwarfUser>(jObject.ToString());
                var workersString = jObject["workers"].First();
                List<DwarfWorker> workersList = new List<DwarfWorker>();
                foreach (var worker in workersString)
                {
                    workersList.Add(JsonConvert.DeserializeObject<DwarfWorker>(worker.ToString()));
                }
                DwarfUserInformation dwarfuserInformation = new DwarfUserInformation()
                {
                    DwarfUser = dwarfUser,
                    Workers = workersList,
                    NumberOfRig = workersString.Count(),
                    PayOutLimit = dwarfConfig.PayOutLimit
                };
                return new UserInformation() { DwarfUserInformation = dwarfuserInformation};
            }
            else
                return null;
        }
    }

    public class UserInformation
    {
        public DwarfUserInformation DwarfUserInformation { get; set; }
    }

    public class DwarfUserInformation
    {
        public DwarfUser DwarfUser { get; set; }
        public List<DwarfWorker> Workers { get; set; }
        public int NumberOfRig { get; set; }
        public double PayOutLimit { get; set; }
    }

    public class DwarfUser
    {
        public string Autopayout_from { get; set; }
        public string Earning_24_hours { get; set; }
        public bool Error { get; set; }
        public float Immature_earning { get; set; }
        public float Last_payment_amount { get; set; }
        public object Last_payment_date { get; set; }
        public string Last_share_date { get; set; }
        public bool Payout_daily { get; set; }
        public bool Payout_request { get; set; }
        public double Total_hashrate { get; set; }
        public double Total_hashrate_calculated { get; set; }
        public float Transferring_to_balance { get; set; }
        public string Wallet { get; set; }
        public string Wallet_balance { get; set; }
    }
}


public class DwarfWorker
{
    public bool Alive { get; set; }
    public double Hashrate { get; set; }
    public bool Hashrate_below_threshold { get; set; }
    public double Hashrate_calculated { get; set; }
    public string Last_submit { get; set; }
    public int Second_since_submit { get; set; }
    public string Worker { get; set; }
}
