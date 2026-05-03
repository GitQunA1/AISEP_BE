using System.ComponentModel;

namespace AISEP.DAL.Enums
{
    public enum TargetMarketSizeEnum
    {
        [Description("Thi truong ngach (Niche): Quy mo duoi 10 trieu USD, tap trung vao tep khach hang rat dac thu.")]
        Niche = 1,
        [Description("Thi truong tam trung (Medium): Quy mo tu 10 - 100 trieu USD, khong gian tang truong tot tai VN hoac khu vuc.")]
        Medium = 2,
        [Description("Thi truong lon (Large): Quy mo tren 100 trieu USD, tiem nang mo rong (Scale) ra toan cau hoac thong tri khu vuc.")]
        Large = 3
    }
}
