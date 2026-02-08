namespace Lab1NBP;

using System;
using System.Threading.Tasks;
using Lab1NBP.Implementations;
using Lab1NBP.Services;
using Lab1NBP.UI;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Pobieranie kursów walut z NBP...\n");

            var nbpUrl = "https://static.nbp.pl/dane/kursy/xml/lastA.xml";
            var repository = new Rest();
            var encoding = new Iso();
            var parser = new XmlDocument();

            var exchange = new Exchange(repository, encoding, parser, nbpUrl);
            var exchangeTable = await exchange.FetchCurrentExchangeRatesAsync();

            var menu = new MenuManager(exchangeTable, exchange);
            menu.ShowMainMenu();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }
    }
}