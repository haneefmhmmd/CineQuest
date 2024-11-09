using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace _301301555_301287005_Laylay_Muhammad__Lab3.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
}
