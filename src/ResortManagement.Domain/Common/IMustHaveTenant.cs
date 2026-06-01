using System;

namespace ResortManagement.Domain.Common;

public interface IMustHaveTenant
{
    Guid TenantId { get; set; }
}
