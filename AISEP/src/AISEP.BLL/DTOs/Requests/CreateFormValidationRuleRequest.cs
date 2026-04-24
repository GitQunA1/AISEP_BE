namespace AISEP.BLL.DTOs.Requests
{
    public class CreateFormValidationRuleRequest : UpsertFormValidationRuleRequest
    {
        public string FormKey { get; set; } = string.Empty;
        public string FieldKey { get; set; } = string.Empty;
    }
}
