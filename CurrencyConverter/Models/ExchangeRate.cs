namespace Lab1NBP.Models;

using System;

public class ExchangeRate
{
    public double Role { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not ExchangeRate other) return false;
        return Role == other.Role &&
               Code == other.Code &&
               Name == other.Name;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Role, Code, Name);
    }

    public override string ToString()
    {
        return $"[{Code}] {Name}: Kurs={Role}";
    }
}