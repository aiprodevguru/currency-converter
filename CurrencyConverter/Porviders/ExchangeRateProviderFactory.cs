using CurrencyConverter.Configurations;
using Microsoft.Extensions.Options;
using System.Xml.Linq;

namespace CurrencyConverter.Porviders
{
    public interface IExchangeRateProviderFactory {
        IExchangeRateProvider GetProvider(string? providerName);
    }
    public class ExchangeRateProviderFactory :IExchangeRateProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ExchangeRateApiOptions exchangeRateApiOptions;

        public ExchangeRateProviderFactory(IServiceProvider serviceProvider, IOptions<ExchangeRateApiOptions> settings)
        {
            _serviceProvider = serviceProvider;
            exchangeRateApiOptions = settings.Value;
        }

        public IExchangeRateProvider GetProvider(string? providerName)
        {
            providerName = providerName ?? exchangeRateApiOptions.Default;

            var providerConfig = exchangeRateApiOptions.Providers.FirstOrDefault(p =>
                p.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));

            if (providerConfig == null)
            {
                throw new ArgumentException($"Currency provider '{providerName}' is not configured.");
            }
            return providerName.ToLower() switch
            {
                "frankfurter" => _serviceProvider.GetRequiredService<FrankfurterProvider>(),
                _ => throw new NotSupportedException($"Currency provider '{providerName}' is not supported.")
            };
        }
    }
}
