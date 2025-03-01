using CRM.Model.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Model.IdentityModels;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Gender? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public short? VerificationCode { get; set; }
    public string? ImageName { get; set; }
    public bool? Activity { get; set; }

    [NotMapped]
    public string? FullName => $"{FirstName} {LastName}";
}

