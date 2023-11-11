namespace Resiliency.Patterns.Labs.Api.Services.Interfaces;

public interface IHttpBinService
{
    Task<int> Get(int statusCode);
}