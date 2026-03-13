using CoreBackendApp.Application.Common.Interfaces;
using System.Security.Claims;

namespace CoreBackendApp.Api.Services;

public class TenantProvider(IHttpContextAccessor httpContextAccessor) : ITenantProvider
{
    public Guid? TenantId
    {
        get
        {
            var tenantIdClaim = httpContextAccessor.HttpContext?.User?.FindFirstValue("tenantId");

            if (Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                return tenantId;
            }

            return null;
        }
    }
}
