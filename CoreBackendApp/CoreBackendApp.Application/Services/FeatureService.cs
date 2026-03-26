using CoreBackendApp.Application.Common.Models;
using CoreBackendApp.Application.Interface;

namespace CoreBackendApp.Application.Services;

public class FeatureService(IFeatureRepository featureRepository) : IFeatureService
{
    private readonly IFeatureRepository _featureRepository = featureRepository;

    public async Task<Result<IEnumerable<FeatureResponse>>> GetAllAsync()
    {
        var features = await _featureRepository.GetAllAsync();
        
        var response = features.Select(f => new FeatureResponse(f.Id, f.Key, f.Name));

        return Result.Success(response);
    }
}
