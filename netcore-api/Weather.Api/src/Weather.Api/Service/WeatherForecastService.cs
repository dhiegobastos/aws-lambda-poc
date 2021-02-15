using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weather.Api.Domain;

namespace Weather.Api.Service
{
    public class WeatherForecastService
    {
        private readonly int DefaultNextDays = 5;
        private readonly ILogger<WeatherForecastService> _logger;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public WeatherForecastService(ILogger<WeatherForecastService> logger)
        {
            _logger = logger;
        }

        public IEnumerable<WeatherForecast> RetrieveForecast(int? nextDays)
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["Country"] = "Brazil",
                ["State"] = "MG"
            }))
            {
                _logger.LogDebug("Iniciando execução do método RetrieveForecast()");

                if (nextDays == null)
                {
                    _logger.LogWarning("Valor informado é nulo. O padrão de {DefaultNextDays} dias será utilizado.", DefaultNextDays);
                }

                var days = nextDays.GetValueOrDefault(DefaultNextDays);

                var rng = new Random();
                var forecasts = Enumerable.Range(1, days).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                }).ToList();

                using (_logger.BeginScope("Verificando localidade com maior temperatura"))
                {
                    _logger.LogInformation("A máxima temperatura dos próximos {NextDays} será de {MaxTemperatureC} graus Celsius.", days, forecasts.Max(t => t.TemperatureC));

                    var maxForecast = forecasts.OrderByDescending(t => t.TemperatureC).Take(1);
                    _logger.LogTrace("Forecast: {ForecastDetailString}", maxForecast); // Sem o operador @ o valor informado é tratado como string
                    _logger.LogTrace("Forecast: {@ForecastDetail}", maxForecast); // O operador @ informa ao Serilog para serializar o valor informado
                }

                _logger.LogDebug("Finalizando execução do método RetrieveForecast()");

                return forecasts;
            }
        }
    }
}
