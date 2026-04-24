namespace AISEP.BLL.DTOs.Responses
{
    public class FormValidationConfigResponse
    {
        public string FormKey { get; set; } = string.Empty;
        public List<FormValidationRuleResponse> Fields { get; set; } = [];
    }
}
