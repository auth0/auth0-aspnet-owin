using System;
using System.Collections.Generic;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Nito.AsyncEx;

namespace Client
{
    class Program
    {
        static int Main(string[] args)
        {
            return AsyncContext.Run(() => AsyncMain(args));
        }

        static async Task<int> AsyncMain(string[] args)
        {
            Console.WriteLine("Press ENTER to call the API.");
            Console.ReadLine();

            // Get a token using the Authentication client
            var client = new AuthenticationApiClient(new Uri("https://{DOMAIN}"));
            var token = await client.Authenticate(new AuthenticationRequest
            {
                ClientId = "{CLIENT_ID}",
                Connection = "Username-Password-Authentication",
                Username = "user@email.com",
                Password = "password",
                Scope = "openid profile"
            });

            // Create a new HttpClient, and set the Auth header to the token we obtained
            var apiClient = new HttpClient();
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.IdToken);

            // Call the API, and extract the response
            var response = await apiClient.GetAsync("http://localhost:25100/api/sample");
            var content = await response.Content.ReadAsAsync<IEnumerable<ClaimItem>>();

            Console.WriteLine("Call complete. Data received:");

            // Write all the claims received from the API to the console
            foreach (var item in content)
                Console.WriteLine(" > {0}: {1}", item.Type, item.Value);
            Console.ReadLine();

            // Return A-OK
            return 0;
        }
    }
}
