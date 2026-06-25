using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace HocaPuan.Core.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class StrongPasswordAttribute : ValidationAttribute
{
    private static readonly Regex Uppercase = new(@"[A-Z]", RegexOptions.Compiled);
    private static readonly Regex Lowercase = new(@"[a-z]", RegexOptions.Compiled);
    private static readonly Regex Digit = new(@"\d", RegexOptions.Compiled);

    public const string DefaultMessage =
        "Şifre en az 8 karakter olmalı, en az bir büyük harf, bir küçük harf ve bir sayı içermelidir.";

    public StrongPasswordAttribute()
        : base(DefaultMessage)
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string password || string.IsNullOrWhiteSpace(password))
            return ValidationResult.Success;

        if (password.Length < 8
            || !Uppercase.IsMatch(password)
            || !Lowercase.IsMatch(password)
            || !Digit.IsMatch(password))
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}
