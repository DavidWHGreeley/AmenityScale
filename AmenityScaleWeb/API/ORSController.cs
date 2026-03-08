using AmenityScaleCore.Data;
using AmenityScaleCore.Models.AmenitiesInRadius;
using AmenityScaleWeb.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-03-03  Patrick                 Initial
/// 0.2             2026-03-07  Patrick                 Updated for Constants.js isochrone format change


namespace AmenityScaleWeb.Controllers
{
    public class ORSController : ApiController
    {
        [HttpGet]
        // Set web address that will run the GetIsochrone function when loaded
        [Route("api/GetIsochrone")]
        // Origin coordinates and minutes are the inputs
        public async Task<IHttpActionResult> GetIsochrone(string lng, string lat, string minutes)
        {
            // Enter your ORS api key here
            string apiKey = "YOUR_ORS_API_KEY";
            // Specific ORS url for walking
            string url = "https://api.openrouteservice.org/v2/isochrones/foot-walking";

            // Convert the minutes string into an array of seconds for ORS
            var secondsArray = minutes.Split(',').Select(m => int.Parse(m.Trim()) * 60).ToArray();

            // ORS formatting
            var requestBody = new
            {
                locations = new[] { new[] { double.Parse(lng), double.Parse(lat) } },
                range = secondsArray,
                range_type = "time"
            };

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    // ORS needs the coordinates in the "body" of the request so POST is used
                    var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);

                    // Fixes authorization errors
                    request.Headers.TryAddWithoutValidation("Authorization", apiKey);
                    request.Headers.Add("User-Agent", "ASP.NET-App");

                    // Convert body to JSON for ORS
                    var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                    request.Content = new System.Net.Http.StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

                    // Send request
                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        return BadRequest("ORS Bad Request");
                    }

                    // Convert back from JSON
                    var jsonString = await response.Content.ReadAsStringAsync();
                    return Ok(Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString));
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}
