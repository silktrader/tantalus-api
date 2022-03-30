namespace Tantalus.Models;

using System.ComponentModel.DataAnnotations;

public record UserRegisterRequest(
    [Required, StringLength(100, MinimumLength = 3)] string FullName, 
    [Required, StringLength(16, MinimumLength = 5)] string Name, 
    [Required, EmailAddress] string Email,
    [Required, StringLength(100, MinimumLength = 8)] string Password);