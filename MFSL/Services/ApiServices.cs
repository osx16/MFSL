using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using MFSL.Models;
using MFSL.ViewModels;
using MFSL.Helpers;

namespace MFSL.Services
{
    public class ApiServices
    {
        public async Task<string> LoginAsync(string userName, string password)
        {
            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("username", userName),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("grant_type", "password")
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post, Constants.BaseApiAddress + "Token");

            request.Content = new FormUrlEncodedContent(keyValues);

            var client = new HttpClient();
            try
            {
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                JObject jwtDynamic = JsonConvert.DeserializeObject<dynamic>(content);
                var accessTokenExpiration = jwtDynamic.Value<DateTime>(".expires");
                var accessToken = jwtDynamic.Value<string>("access_token");
                Settings.AccessTokenExpirationDate = accessTokenExpiration;
                //Debug.WriteLine(accessTokenExpiration);
                //Debug.WriteLine(content);
                return accessToken;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> CreateNewFile(MemberFile model)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);
            var json = JsonConvert.SerializeObject(model);
            HttpContent content = new StringContent(json);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            try
            {
                var response = await client.PostAsync(Constants.BaseApiAddress + "api/MemberFilesAPI", content);
                Debug.WriteLine(response);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine("\nException Caught!");
                Debug.WriteLine("Message :{0} ", e.Message);
                return false;
            }
        }// End of RegisterAsync

        public async Task<bool> CreateNewRef(MemberFile model)
        {
            var client = new HttpClient();
            FileReferences FR = new FileReferences();
            FR.DateCreated = model.DateCreated;
            FR.MemberNo = model.MemberNo;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);
            var json = JsonConvert.SerializeObject(FR);
            HttpContent content = new StringContent(json);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            try
            {
                var response = await client.PostAsync(Constants.BaseApiAddress + "api/MemberFilesAPI/PostReference", content);
                Debug.WriteLine(response);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine("\nException Caught!");
                Debug.WriteLine("Message :{0} ", e.Message);
                return false;
            }
        }// End of RegisterAsync

        public async Task<List<MemberFile>> FetchFilesForUserAsync()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);
            try
            {
                var json = await client.GetStringAsync (Constants.BaseApiAddress + "api/MemberFilesAPI/GetFileForUser");
                var Files = JsonConvert.DeserializeObject<List<MemberFile>>(json);

                return Files;
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine("\nException Caught!");
                Debug.WriteLine("Message :{0} ", e.Message);
                return null;
            }
        }

        public async Task<List<MemberFile>> FetchAllFilesAsync()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);

            var json = await client.GetStringAsync(Constants.BaseApiAddress + "api/MemberFilesAPI/FetchAll");
            var Files = JsonConvert.DeserializeObject<List<MemberFile>>(json);

            return Files;
        }

        public async Task<List<FileReferences>> GetFileByMemberNo(int MemberNo)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);

            var json = await client.GetStringAsync(Constants.BaseApiAddress + "api/MemberFilesAPI/GetFileByMemberNo/"+ MemberNo);
            var Files = JsonConvert.DeserializeObject<List<FileReferences>>(json);

            return Files;
        }
    }
}