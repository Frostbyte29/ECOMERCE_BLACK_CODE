using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BlackCode.Web.Models;

// Usuario base de ASP.NET Core Identity. El módulo de login/registro puede ampliarlo.
public class ApplicationUser : IdentityUser
{
    [StringLength(120)]
    public string FullName { get; set; } = "";

    public bool IsActive { get; set; } = true;

    public List<Order> Orders { get; set; } = [];
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Cliente = "Cliente";
}
