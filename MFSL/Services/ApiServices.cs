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
using MFSL.Controllers;
namespace MFSL.Services
{
    public class ApiServices
    {
        /// <summary>
        /// Task Method for posting new member file to web api
        /// </summary>
        /// <param name="model"> Member File</param>
        /// <returns>Boolean</returns>
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
    }
}