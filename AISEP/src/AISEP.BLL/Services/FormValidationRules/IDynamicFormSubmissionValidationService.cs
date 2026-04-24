namespace AISEP.BLL.Services.FormValidationRules
{
    public interface IDynamicFormSubmissionValidationService
    {
        Task ValidateAsync(string formKey, object request);
    }
}
