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

// Service - Currency Converter
public class CurrencyConverter
{
    private ExchangeTable _exchangeTable;
    private const string PlnCode = "PLN";

    public CurrencyConverter(ExchangeTable exchangeTable)
    {
        _exchangeTable = exchangeTable;
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

// Facade - Exchange Service
public class Exchange
{
    private readonly IRemoteRepository _repository;
    private readonly IEncoding _encoding;
    private readonly IDocument _parser;
    private readonly string _url;

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
        return _parser.Parse(xmlContent);
    }
}

// UI - Menu Manager
public class MenuManager
{
    private ExchangeTable _exchangeTable;
    private CurrencyConverter _converter;

    public MenuManager(ExchangeTable exchangeTable)
    {
        _exchangeTable = exchangeTable;
        _converter = new CurrencyConverter(exchangeTable);
    }

    public void ShowMainMenu()
    {
        bool running = true;
        while (running)
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║     APLIKACJA DO ZAMIANY WALUT NBP     ║");
            Console.WriteLine("╚════════════════════════════════════════╝\n");
            Console.WriteLine($"Aktualne kursy z dnia: {_exchangeTable.TimeStamp:yyyy-MM-dd}\n");
            Console.WriteLine("1. Wyświetl wszystkie kursy walut");
            Console.WriteLine("2. Zamień walutę");
            Console.WriteLine("3. Sprawdź kurs konkretnej waluty");
            Console.WriteLine("4. Wyjdź\n");
            Console.Write("Wybierz opcję (1-4): ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowAllRates();
                    break;
                case "2":
                    ConvertCurrency();
                    break;
                case "3":
                    CheckSingleRate();
                    break;
                case "4":
                    running = false;
                    Console.WriteLine("\nDo widzenia!");
                    break;
                default:
                    Console.WriteLine("\nNiepoprawna opcja!");
                    System.Threading.Thread.Sleep(1500);
                    break;
            }
        }
    }

    private void ShowAllRates()
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║         WSZYSTKIE KURSY WALUT          ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");
        Console.WriteLine($"Data: {_exchangeTable.TimeStamp:yyyy-MM-dd}");
        Console.WriteLine($"Tabela nr: {_exchangeTable.Id}\n");
        Console.WriteLine(new string('-', 70));
        Console.WriteLine("{0,-10} {1,-25} {2,15}", "KOD", "NAZWA", "KURS");
        Console.WriteLine(new string('-', 70));

        foreach (var rate in _exchangeTable.Rates.OrderBy(r => r.Code))
        {
            Console.WriteLine("{0,-10} {1,-25} {2,15:F4}", rate.Code, rate.Name, rate.Role);
        }

        Console.WriteLine(new string('-', 70));
        Console.WriteLine($"Razem walut: {_exchangeTable.Rates.Count}");
        Console.WriteLine("\nNaciśnij Enter aby kontynuować...");
        Console.ReadLine();
    }

    private void ConvertCurrency()
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║          ZAMIANA WALUT                 ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");

        try
        {
            Console.Write("Podaj walutę źródłową (np. EUR, GBP, PLN): ");
            string fromCurrency = Console.ReadLine()?.ToUpper() ?? "";

            if (string.IsNullOrEmpty(fromCurrency))
            {
                throw new InvalidOperationException("Waluta nie może być pusta");
            }

            Console.Write("Podaj walutę docelową (np. EUR, GBP, PLN): ");
            string toCurrency = Console.ReadLine()?.ToUpper() ?? "";

            if (string.IsNullOrEmpty(toCurrency))
            {
                throw new InvalidOperationException("Waluta nie może być pusta");
            }

            Console.Write("Podaj kwotę do zamiany: ");
            if (!double.TryParse(Console.ReadLine(), out double amount))
            {
                throw new InvalidOperationException("Niepoprawna kwota");
            }

            double result = _converter.Convert(fromCurrency, toCurrency, amount);

            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine($"✓ {amount:F2} {fromCurrency} = {result:F2} {toCurrency}");
            Console.WriteLine(new string('=', 50));

            var fromRate = _exchangeTable.GetRole(fromCurrency);
            var toRate = _exchangeTable.GetRole(toCurrency);

            if (fromRate != null)
                Console.WriteLine($"Kurs {fromCurrency}: {fromRate.Role:F4}");
            if (toRate != null)
                Console.WriteLine($"Kurs {toCurrency}: {toRate.Role:F4}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Błąd: {ex.Message}");
        }

        Console.WriteLine("\nNaciśnij Enter aby kontynuować...");
        Console.ReadLine();
    }

    private void CheckSingleRate()
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║      KURS KONKRETNEJ WALUTY            ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");
        
        Console.Write("Podaj kod waluty (np. EUR, GBP, USD): ");
        string currencyCode = Console.ReadLine()?.ToUpper() ?? "";

        var rate = _exchangeTable.GetRole(currencyCode);

        if (rate != null)
        {
            Console.WriteLine("\n" + new string('-', 50));
            Console.WriteLine($"Kod:      {rate.Code}");
            Console.WriteLine($"Nazwa:    {rate.Name}");
            Console.WriteLine($"Kurs:     {rate.Role:F4}");
            Console.WriteLine(new string('-', 50));
        }
        else
        {
            Console.WriteLine($"\n✗ Waluta {currencyCode} nie znaleziona w bazie!");
        }

        Console.WriteLine("\nNaciśnij Enter aby kontynuować...");
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

            var menu = new MenuManager(exchangeTable);
            menu.ShowMainMenu();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }
    }
}