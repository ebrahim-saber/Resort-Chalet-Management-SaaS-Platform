using System;

namespace ResortManagement.Application.Common.Interfaces;

public interface ITenantProvider
{
    Guid TenantId { get; }
    void SetTenantId(Guid tenantId);
}
