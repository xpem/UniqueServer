using System.ComponentModel.DataAnnotations;

namespace BaseModels.Request
{
    public record ReqBaseModel
    {
        public string? ValidateBook()
        {
            var validationResult = new List<ValidationResult>();

            Validator.TryValidateObject(this, new ValidationContext(this), validationResult, true);

            if (validationResult.Count > 0) return validationResult.First().ErrorMessage;
            else return null;
        }
    }
}
