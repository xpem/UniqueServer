using System.ComponentModel.DataAnnotations;

namespace FinancialService.Model.Req
{
    public record BaseReq
    {
        public string? Validate()
        {
            List<ValidationResult> validationResult = [];

            Validator.TryValidateObject(this, new ValidationContext(this), validationResult, true);

            if (validationResult.Count > 0) return validationResult.First().ErrorMessage;
            else return null;
        }
    }
}
