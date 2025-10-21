namespace Lab1NBP.UI;

using System;
using System.Linq;
using Lab1NBP.Models;
using Lab1NBP.Services;

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
        Console.Write("Waluta zrodlowa (Podaj kod waluty np. EUR): ");
        string from = Console.ReadLine()?.ToUpper() ?? "";
        Console.Write("Waluta docelowa (rowniez kod): ");
        string to = Console.ReadLine()?.ToUpper() ?? "";
        Console.Write("Ilosc: ");
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