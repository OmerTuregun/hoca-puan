using System.ComponentModel.DataAnnotations;

namespace HocaPuan.Core.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class EduTrEmailAttribute : ValidationAttribute
{
    private const string RequiredSuffix = ".edu.tr";

    public EduTrEmailAttribute()
        : base("Sadece .edu.tr uzantılı üniversite e-postaları ile kayıt olabilirsiniz.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string email || string.IsNullOrWhiteSpace(email))
            return ValidationResult.Success;

        if (!email.EndsWith(RequiredSuffix, StringComparison.OrdinalIgnoreCase))
            return new ValidationResult(ErrorMessage);

        return ValidationResult.Success;
    }
}
