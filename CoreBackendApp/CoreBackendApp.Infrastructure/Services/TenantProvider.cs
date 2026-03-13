using CoreBackendApp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CoreBackendApp.Infrastructure.Services;

public class TenantProvider(IHttpContextAccessor httpContextAccessor) : ITenantProvider
{
    public Guid? TenantId
    {
        get
        {
            var tenantIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst("tenantId")?.Value;

            if (Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                return tenantId;
            }

            return null;
        }
    }
}
