using System;

namespace ResortManagement.Domain.Common;

public abstract class MustHaveTenantEntityBase : EntityBase, IMustHaveTenant
{
    public Guid TenantId { get; set; }
}
