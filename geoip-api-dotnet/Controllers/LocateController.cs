﻿// LocateController.cs CLown1331
// CREATED: 08-05-2021
// LAST: 08-05-2021


using System.Net;
using MaxMind.GeoIP2.Exceptions;
using MaxMind.GeoIP2.Responses;

namespace GeoipApiDotnet.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using MaxMind.GeoIP2;
    using Microsoft.Extensions.Configuration;

    [ApiController]
    [Route("[controller]")]
    public class LocateController : ControllerBase
    {
        private readonly ILogger<LocateController> _logger;
        private readonly string _geodbCityPath;
        private readonly DatabaseReader _reader;

        public LocateController(ILogger<LocateController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _geodbCityPath =  configuration.GetValue<string>("GEODB_CITY");
            _logger.LogInformation($"Db_path: {_geodbCityPath}");
            _reader = new DatabaseReader(_geodbCityPath);
        }

        [HttpGet]
        public Task<IActionResult> Locate([FromQuery] string ip)
        {
            return Task.Run(() => this.TryGetResult(ip));
        }

        private async Task<IActionResult> TryGetResult(string ip)
        {
            try
            {
                _logger.LogInformation($"Trying {ip}");
                if (_reader.TryCity(ip, out CityResponse city))
                {
                    return Ok(new
                    {
                        ipAddress = ip,
                        countryName = city.Country.Names["en"],
                        cityName = city.City.Names["en"],
                        latitude = city.Location.Latitude,
                        longitude = city.Location.Longitude,
                        v = "d",
                    });
                }
                else
                {
                    return BadRequest("bad ip");
                }
            }
            catch (GeoIP2Exception ex)
            {
                _logger.LogError("Exception occured", ex);
                return BadRequest("bad ip");
            }
        }
    }
}
