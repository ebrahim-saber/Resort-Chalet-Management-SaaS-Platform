using System;
using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Identity;

public class UserRole : EntityBase
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    private UserRole() { } // EF Core

    public UserRole(Guid userId, Guid roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }
}
