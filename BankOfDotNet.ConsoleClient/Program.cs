﻿using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BankOfDotNet.ConsoleClient
{
    class Program
    {
        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();
        private static async Task MainAsync()
        {
            GetResourceOwnerPassword();

            GetClientCredentials();   

            Console.Read();
        }

        private static async void GetResourceOwnerPassword()
        {
            //ResourceOwnerPassword
            DiscoveryResponse discoverRo = await DiscoveryClient.GetAsync("http://localhost:5000");
            if (discoverRo.IsError)
            {
                Console.WriteLine(discoverRo.Error);
                return;
            }

            TokenClient tokenClientRo = new TokenClient(discoverRo.TokenEndpoint, "ro.client", "secret");
            TokenResponse tokenResponseRo = await tokenClientRo.RequestResourceOwnerPasswordAsync("Manish", "password", "bankOfDotNetApi");

            if (tokenResponseRo.IsError)
            {
                Console.WriteLine(tokenResponseRo.Error);
                return;
            }

            Console.WriteLine(tokenResponseRo.Json);
            Console.WriteLine("\n\n");
        }

        private static async void GetClientCredentials()
        {
            //ClientCredentials
            var discovery = await DiscoveryClient.GetAsync("http://localhost:5000");
            if (discovery.IsError)
            {
                Console.WriteLine(discovery.Error);
                return;
            }

            TokenClient tokenClient = new TokenClient(discovery.TokenEndpoint, "client", "secret");
            TokenResponse tokenResponse = await tokenClient.RequestClientCredentialsAsync("bankOfDotNetApi");

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            HttpClient client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var customerInfo = new StringContent(JsonConvert.SerializeObject(
                    new { Id = 1, FirstName = "Douglas", LastName = "Costa" }),
                    Encoding.UTF8, "application/json"
                );
            var createCostumerResponse = await client.PostAsync("http://localhost:8035/api/customers", customerInfo);

            if (!createCostumerResponse.IsSuccessStatusCode)
            {
                Console.WriteLine(createCostumerResponse.StatusCode);
            }

            var getCustomerResponse = await client.GetAsync("http://localhost:8035/api/customers");
            if (!getCustomerResponse.IsSuccessStatusCode)
            {
                Console.WriteLine(getCustomerResponse.StatusCode);
            }
            else
            {
                var content = await getCustomerResponse.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
        }

    }
}
