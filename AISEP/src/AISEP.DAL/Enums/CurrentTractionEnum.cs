using System.ComponentModel;

namespace AISEP.DAL.Enums
{
    public enum CurrentTractionEnum
    {
        [Description("Chua co doanh thu (Pre-Revenue): Dang trong giai doan phat trien, chua phat sinh dong tien.")]
        PreRevenue = 1,
        [Description("Co nguoi dung thu (User Acquisition): Chua co loi nhuan nhung da co luong nguoi dung thuong xuyen hoac dang ky cho (Waitlist) lon.")]
        UserAcquisition = 2,
        [Description("Da co doanh thu (Revenue-Generating): Da bat dau co khach hang tra tien, mo hinh kinh doanh duoc chung minh tinh kha thi.")]
        RevenueGenerating = 3,
        [Description("Dang tang truong manh/Co lai (Scaling/Profitable): Dong tien duong hoac doanh thu tang truong tinh bang lan (MoM/YoY).")]
        ScalingOrProfitable = 4
    }
}
