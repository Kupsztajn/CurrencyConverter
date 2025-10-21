namespace Lab1NBP.Interfaces;

using Lab1NBP.Models;

public interface IDocument
{
    ExchangeTable Parse(string xmlContent);
}