using System.ComponentModel;

namespace AISEP.DAL.Enums
{
    public enum IPProtectionEnum
    {
        [Description("Khong co bao ve (None): Cong nghe de bi copy, ma nguon mo hoac mo hinh kinh doanh thuan tuy.")]
        None = 1,
        [Description("Dang cho duyet/Co rao can mem (Defensible): Dang nop don so huu tri tue, hoac so huu du lieu/thuat toan noi bo kho sao chep.")]
        Defensible = 2,
        [Description("Da bao ho (Secured): So huu bang sang che doc quyen, cong nghe loi (Deeptech, Cleantech, Healthtech) duoc phap luat bao ve.")]
        Secured = 3
    }
}
