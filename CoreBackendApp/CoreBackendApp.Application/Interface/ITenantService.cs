using CoreBackendApp.Application.Common.Models;

namespace CoreBackendApp.Application.Interface;

public record TenantResponse(Guid Id, string Name, IEnumerable<string> Features);

public interface ITenantService
{
    Task<Result<IEnumerable<TenantResponse>>> GetAllAsync();
}
