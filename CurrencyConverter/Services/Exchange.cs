namespace Lab1NBP.Services;

using System;
using System.Threading.Tasks;
using Lab1NBP.Interfaces;
using Lab1NBP.Models;

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