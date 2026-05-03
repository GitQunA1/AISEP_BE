using System.ComponentModel;

namespace AISEP.DAL.Enums
{
    public enum TeamSizeEnum
    {
        [Description("Solo Founder: Chi co 1 nguoi sang lap, ganh vac moi vai tro.")]
        Solo = 1,
        [Description("Cap Co-founders: 2 nguoi sang lap, thuong bu tru ky nang cho nhau (Tech & Business).")]
        TwoFounders = 2,
        [Description("Doi ngu hoan thien: Tu 3 co-founders tro len, cau truc phong ban ro rang.")]
        ThreeOrMore = 3
    }
}
