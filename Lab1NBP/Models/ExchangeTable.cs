namespace Lab1NBP.Models;

using System;
using System.Collections.Generic;
using System.Linq;

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