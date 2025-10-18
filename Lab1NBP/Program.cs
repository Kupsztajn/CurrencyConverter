namespace Lab1NBP;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

// Interface segregation - Remote Repository abstraction
public interface IRemoteRepository
{
    Task<byte[]> Get(string url);
}

// Interface segregation - Encoding abstraction
public interface IEncoding
{
    string GetString(byte[] data);
}

// Interface segregation - Document parser abstraction
public interface IDocument
{
    ExchangeTable Parse(string xmlContent);
}

// Single Responsibility - REST implementation
public class Rest : IRemoteRepository
{
    private readonly HttpClient _httpClient;

    public Rest()
    {
        _httpClient = new HttpClient();
    }

    public async Task<byte[]> Get(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching data from {url}", ex);
        }
    }
}

// Single Responsibility - ISO8859-2 Encoding
public class ISO : IEncoding
{
    public string GetString(byte[] data)
    {
        System.Text.Encoding.RegisterProvider(
            System.Text.CodePagesEncodingProvider.Instance);
        return System.Text.Encoding.GetEncoding("ISO-8859-2").GetString(data);
    }
}

// Value Object - Exchange Rate
public class ExchangeRate
{
    public double Role { get; set; }
    public double Vault { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not ExchangeRate other) return false;
        return Role == other.Role &&
               Vault == other.Vault &&
               Code == other.Code &&
               Name == other.Name;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Role, Vault, Code, Name);
    }

    public override string ToString()
    {
        return $"[{Code}] {Name}: Kurs={Role}";
    }
}

// Entity - Exchange Table
public class ExchangeTable
{
    public string Id { get; set; }
    public DateTime TimeStamp { get; set; }
    public List<ExchangeRate> Rates { get; set; } = new();

    public ExchangeRate GetRole(string currencyCode)
    {
        return Rates.FirstOrDefault(r => r.Code == currencyCode);
    }
}

// Single Responsibility - XML Parser (unified, bez redundacji)
public class XMLDocument : IDocument
{
    public ExchangeTable Parse(string xmlContent)
    {
        try
        {
            var doc = XDocument.Parse(xmlContent);
            var root = doc.Root ?? throw new Exception("Brak root element w XML");

            var table = new ExchangeTable
            {
                Id = root.Element("numer_tabeli")?.Value ?? "",
                TimeStamp = DateTime.Parse(root.Element("data_publikacji")?.Value ?? DateTime.Now.ToString()),
                Rates = new List<ExchangeRate>()
            };

            var pozycje = root.Elements("pozycja");
            foreach (var pozycja in pozycje)
            {
                var rate = new ExchangeRate
                {
                    Code = pozycja.Element("kod_waluty")?.Value ?? "",
                    Name = pozycja.Element("nazwa_waluty")?.Value ?? "",
                    Role = double.Parse(pozycja.Element("kurs_sredni")?.Value ?? "0".Replace(",", ".")),
                    Vault = double.Parse(pozycja.Element("kurs_sredni")?.Value ?? "0".Replace(",", "."))
                };
                table.Rates.Add(rate);
            }

            return table;
        }
        catch (Exception ex)
        {
            throw new Exception("Error parsing XML content", ex);
        }
    }
}

// Facade - Exchange Service
public class Exchange
{
    private readonly IRemoteRepository _repository;
    private readonly IEncoding _encoding;
    private readonly IDocument _parser;
    private readonly string _url;
    private ExchangeTable _exchangeTable;
    private const string PlnCode = "PLN";

    public Exchange(
        IRemoteRepository repository,
        IEncoding encoding,
        IDocument parser,
        string url)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _url = url ?? throw new ArgumentNullException(nameof(url));
    }

    public async Task<ExchangeTable> FetchCurrentExchangeRatesAsync()
    {
        var data = await _repository.Get(_url);
        var xmlContent = _encoding.GetString(data);
        _exchangeTable = _parser.Parse(xmlContent);
        return _exchangeTable;
    }

    public double Convert(string fromCurrency, string toCurrency, double amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Kwota musi być większa od 0");

        fromCurrency = fromCurrency.ToUpper();
        toCurrency = toCurrency.ToUpper();

        if (fromCurrency == toCurrency)
            return amount;

        double amountInPln;

        if (fromCurrency == PlnCode)
        {
            amountInPln = amount;
        }
        else
        {
            var fromRate = _exchangeTable.GetRole(fromCurrency);
            if (fromRate == null)
                throw new InvalidOperationException($"Waluta {fromCurrency} nie znaleziona");
            amountInPln = amount * fromRate.Role;
        }

        if (toCurrency == PlnCode)
        {
            return amountInPln;
        }
        else
        {
            var toRate = _exchangeTable.GetRole(toCurrency);
            if (toRate == null)
                throw new InvalidOperationException($"Waluta {toCurrency} nie znaleziona");
            return amountInPln / toRate.Role;
        }
    }
}

// UI - Menu Manager
public class MenuManager
{
    private ExchangeTable _exchangeTable;
    private Exchange _exchange;

    public MenuManager(ExchangeTable exchangeTable, Exchange exchange)
    {
        _exchangeTable = exchangeTable;
        _exchange = exchange;
    }

    public void ShowMainMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("1. Kursy");
            Console.WriteLine("2. Zamiana");
            Console.WriteLine("3. Wyjdź");
            Console.Write("> ");

            switch (Console.ReadLine())
            {
                case "1":
                    ShowAllRates();
                    break;
                case "2":
                    ConvertCurrency();
                    break;
                case "3":
                    return;
            }
        }
    }

    private void ShowAllRates()
    {
        Console.Clear();
        foreach (var rate in _exchangeTable.Rates.OrderBy(r => r.Code))
            Console.WriteLine($"{rate.Code} {rate.Name} {rate.Role:F4}");
        Console.WriteLine("\nEnter...");
        Console.ReadLine();
    }

    private void ConvertCurrency()
    {
        Console.Write("Z: ");
        string from = Console.ReadLine()?.ToUpper() ?? "";
        Console.Write("Na: ");
        string to = Console.ReadLine()?.ToUpper() ?? "";
        Console.Write("Ile: ");
        if (double.TryParse(Console.ReadLine(), out double amount))
        {
            try
            {
                double result = _exchange.Convert(from, to, amount);
                Console.WriteLine($"{amount} {from} = {result:F2} {to}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd: {ex.Message}");
            }
        }
        Console.WriteLine("Enter...");
        Console.ReadLine();
    }
}

// Main Program
class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Pobieranie kursów walut z NBP...\n");

            var nbpUrl = "https://static.nbp.pl/dane/kursy/xml/lastA.xml";
            var repository = new Rest();
            var encoding = new ISO();
            var parser = new XMLDocument();

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