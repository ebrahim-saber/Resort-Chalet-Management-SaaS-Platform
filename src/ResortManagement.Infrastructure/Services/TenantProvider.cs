using System;
using ResortManagement.Application.Common.Interfaces;

namespace ResortManagement.Infrastructure.Services;

public class TenantProvider : ITenantProvider
{
    private Guid _tenantId;

    public Guid TenantId => _tenantId;

    public void SetTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
    }
}
