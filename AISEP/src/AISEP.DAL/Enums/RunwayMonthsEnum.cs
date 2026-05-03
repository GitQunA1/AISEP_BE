using System.ComponentModel;

namespace AISEP.DAL.Enums
{
    public enum RunwayMonthsEnum
    {
        [Description("Duoi 6 thang: Nguy co can von cao, can tien gap de duy tri hoat dong.")]
        Under6Months = 1,
        [Description("Tu 6 den 12 thang: Muc do an toan trung binh, co du thoi gian de toi uu san pham trong luc goi von.")]
        SixToTwelveMonths = 2,
        [Description("Tren 12 thang: Dong tien khoe, goi von chu yeu de mo rong thi phan (Scale) chu khong phai de sinh ton.")]
        Over12Months = 3
    }
}
