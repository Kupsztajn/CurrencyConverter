namespace Lab1NBP.Implementations;

using Lab1NBP.Interfaces;

public class ISO : IEncoding
{
    public string GetString(byte[] data)
    {
        System.Text.Encoding.RegisterProvider(
            System.Text.CodePagesEncodingProvider.Instance);
        return System.Text.Encoding.GetEncoding("ISO-8859-2").GetString(data);
    }
}