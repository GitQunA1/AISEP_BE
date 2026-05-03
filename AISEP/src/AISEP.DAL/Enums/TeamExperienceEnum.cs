using System.ComponentModel;

namespace AISEP.DAL.Enums
{
    public enum TeamExperienceEnum
    {
        [Description("Lan dau khoi nghiep: Doi ngu tre, nhiet huyet nhung chua co kinh nghiem quan ly hoac goi von truoc day.")]
        FirstTime = 1,
        [Description("Chuyen gia trong nganh: Founder co nhieu nam kinh nghiem lam viec chuyen sau trong linh vuc dang khoi nghiep.")]
        IndustryExpert = 2,
        [Description("Serial Founder: Da tung sang lap/dong sang lap va thoai von (exit) hoac van hanh thanh cong startup truoc do.")]
        SerialFounder = 3
    }
}
