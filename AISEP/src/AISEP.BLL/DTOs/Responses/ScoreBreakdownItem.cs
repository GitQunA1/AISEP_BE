namespace AISEP.BLL.DTOs.Responses
{
    public class ScoreBreakdownItem
    {
        public string Component { get; set; } = string.Empty;
        public double MaxPoints { get; set; }
        public double Score { get; set; }
    }
}
