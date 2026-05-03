using System.ComponentModel;

namespace AISEP.DAL.Enums
{
    public enum ProductReadinessEnum
    {
        [Description("Y tuong (Idea): Moi nam tren giay, dang khao sat thi truong, chua co san pham thuc te.")]
        Idea = 1,
        [Description("Ban mau (Prototype): Da co ban phac thao hoac thiet ke tinh nang co ban, dung de demo.")]
        Prototype = 2,
        [Description("San pham kha dung toi thieu (MVP): Da code/san xuat xong tinh nang cot loi, co the dua cho nguoi dung dau tien test.")]
        MVP = 3,
        [Description("San sang tung ra thi truong (Market-Ready): San pham hoan thien, it loi, da dong goi san sang de ban dai tra.")]
        MarketReady = 4
    }
}
