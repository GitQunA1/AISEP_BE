namespace AISEP.DAL.Entities
{
    public class FormValidationRuleStageOption
    {
        public int FormValidationRuleId { get; set; }
        public int StageOptionId { get; set; }

        public FormValidationRule FormValidationRule { get; set; } = null!;
        public StageOption StageOption { get; set; } = null!;
    }
}
