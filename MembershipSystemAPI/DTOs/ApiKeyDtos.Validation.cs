using FluentValidation;

namespace MembershipSystemAPI.DTOs;

// API Key Request DTOs Validators
/// <summary>
/// Validator for GenerateApiKeyRequest DTO
/// </summary>
public class GenerateApiKeyRequestValidator : Validator<GenerateApiKeyRequest>
{
    public GenerateApiKeyRequestValidator()
    {
        // Currently no validation rules needed for GenerateApiKeyRequest
        // as it's an empty record
    }
}