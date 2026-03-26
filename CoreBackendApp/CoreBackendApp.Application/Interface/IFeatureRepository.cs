using CoreBackendApp.Domain.Entities;

namespace CoreBackendApp.Application.Interface;

public interface IFeatureRepository
{
    Task<IEnumerable<Feature>> GetAllAsync();
}
