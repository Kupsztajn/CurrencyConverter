namespace Lab1NBP.Implementations;

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Lab1NBP.Interfaces;
using Lab1NBP.Models;

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