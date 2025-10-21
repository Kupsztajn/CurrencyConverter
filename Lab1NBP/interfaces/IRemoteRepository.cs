namespace Lab1NBP.Interfaces;

using System.Threading.Tasks;

public interface IRemoteRepository
{
    Task<byte[]> Get(string url);
}