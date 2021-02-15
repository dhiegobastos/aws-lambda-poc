using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Weather.Api.Service;

namespace Weather.Api.Controllers
{
    [ApiController]
    [Route("/Weather")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get([FromServices] WeatherForecastService service, string nextDays)
        {
            int? days = null;
            try
            {
                if (nextDays != null)
                {
                    days = int.Parse(nextDays);
                }
                return Ok(service.RetrieveForecast(days));
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "O valor informado {NextDays} não é um valor aceito.", nextDays);
                return BadRequest();
            }
        }
    }
}
