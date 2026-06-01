using ResortManagement.Domain.Common;

namespace ResortManagement.Domain.Entities.Identity;

public class Permission : EntityBase
{
    public string Code { get; set; } = default!;
    public string Description { get; set; } = default!;

    private Permission() { } // EF Core

    public Permission(string code, string description)
    {
        Code = code;
        Description = description;
    }
}
