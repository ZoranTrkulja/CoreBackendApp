using CoreBackendApp.Application.Common.Models;

namespace CoreBackendApp.Application.Interface;

public record FeatureResponse(Guid Id, string Key, string Name);

public interface IFeatureService
{
    Task<Result<IEnumerable<FeatureResponse>>> GetAllAsync();
}
