using System.ComponentModel;

namespace AISEP.DAL.Enums
{
    public enum MarketGrowthEnum
    {
        [Description("Cham (Slow): Nganh truyen thong, toc do so hoa cham hoac da bao hoa (duoi 5%/nam).")]
        Slow = 1,
        [Description("On dinh (Steady): Tang truong deu dan cung nhip voi nen kinh te (5% - 15%/nam).")]
        Steady = 2,
        [Description("Nong/Dot pha (Fast): Dang la xu huong manh, tang truong rat nhanh (vi du: AI, Proptech, Fintech) (> 15%/nam).")]
        Fast = 3
    }
}
