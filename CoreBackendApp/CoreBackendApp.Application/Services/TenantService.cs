using CoreBackendApp.Application.Common.Models;
using CoreBackendApp.Application.Interface;

namespace CoreBackendApp.Application.Services;

public class TenantService(ITenantRepository tenantRepository) : ITenantService
{
    private readonly ITenantRepository _tenantRepository = tenantRepository;

    public async Task<Result<IEnumerable<TenantResponse>>> GetAllAsync()
    {
        var tenants = await _tenantRepository.GetAllAsync();
        
        var response = tenants.Select(t => new TenantResponse(
            t.Id, 
            t.Name, 
            t.TenantFeatures.Select(tf => tf.Feature.Key)));

        return Result.Success(response);
    }
}
