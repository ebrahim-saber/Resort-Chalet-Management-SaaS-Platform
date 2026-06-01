using System;
using ResortManagement.Application.Common.Interfaces;

namespace ResortManagement.Infrastructure.Services;

public class TenantProvider : ITenantProvider
{
    private Guid _tenantId;

    public Guid TenantId => _tenantId;

    public void SetTenantId(Guid tenantId)
    {
        if (_tenantId != Guid.Empty && _tenantId != tenantId)
        {
            throw new InvalidOperationException("Tenant ID has already been set for this request context.");
        }
        _tenantId = tenantId;
    }
}
