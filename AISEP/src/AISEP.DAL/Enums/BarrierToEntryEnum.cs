using System.ComponentModel;

namespace AISEP.DAL.Enums
{
    public enum BarrierToEntryEnum
    {
        [Description("Thap (Low): Chi phi bat dau re, cong nghe don gian, nhieu doi thu de dang nhay vao (vd: E-commerce nho, App dich vu).")]
        Low = 1,
        [Description("Trung binh (Medium): Doi hoi mot luong von nhat dinh hoac network tot de bat dau.")]
        Medium = 2,
        [Description("Cao (High): Can von dau tu ban dau khong lo (Manufacturing), hoac doi hoi giay phep phap ly khat khe (Fintech, Healthtech).")]
        High = 3
    }
}
