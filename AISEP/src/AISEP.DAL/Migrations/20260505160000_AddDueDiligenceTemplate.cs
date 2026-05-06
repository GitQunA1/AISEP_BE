using System;
using AISEP.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260505160000_AddDueDiligenceTemplate")]
    public partial class AddDueDiligenceTemplate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "due_diligence_templates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContentJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_due_diligence_templates", x => x.Id);
                });

            var templateJson = """
{
  "title": "[TÊN DỰ ÁN CỦA BẠN]",
  "documentTitle": "TÀI LIỆU THẨM ĐỊNH CHI TIẾT (DUE DILIGENCE DOCUMENT)",
  "note": "(Vui lòng trình bày ngắn gọn, đưa số liệu chứng minh cụ thể. Hệ thống Kiểm toán AI sẽ đối chiếu trực tiếp tài liệu này với Form Khai báo của bạn).",
  "sections": [
    {
      "key": "team",
      "title": "I. ĐỘI NGŨ SÁNG LẬP (TEAM)",
      "items": [
        {
          "key": "team_size",
          "title": "1. Kích thước đội ngũ (Team Size)",
          "bullets": [
            "Mô tả số lượng thành viên sáng lập (Co-founders) hiện tại.",
            "Sự phân bổ vai trò của từng người trong dự án."
          ]
        },
        {
          "key": "team_experience",
          "title": "2. Kinh nghiệm & Chuyên môn (Team Experience)",
          "bullets": [
            "Mô tả kinh nghiệm làm việc, số năm hoạt động trong ngành của các founder.",
            "Các dự án/startup đã từng tham gia hoặc điều hành thành công trước đây (nếu có)."
          ]
        },
        {
          "key": "technical_cofounder",
          "title": "3. Năng lực Kỹ thuật (Technical Co-founder)",
          "bullets": [
            "Đội ngũ có thành viên xuất thân từ lập trình/kỹ thuật làm nòng cốt không?",
            "(Trường hợp thuê ngoài/outsource: Vui lòng giải thích rõ cách quản lý chất lượng và rủi ro công nghệ)."
          ]
        }
      ]
    },
    {
      "key": "market",
      "title": "II. THỊ TRƯỜNG MỤC TIÊU (MARKET)",
      "items": [
        {
          "key": "market_size",
          "title": "1. Quy mô thị trường (Target Market Size)",
          "bullets": [
            "Xác định tệp khách hàng nhắm tới (Thị trường ngách hay Đại chúng).",
            "Ước tính quy mô thị trường bằng số liệu định lượng (USD hoặc VNĐ) có trích dẫn nguồn."
          ]
        },
        {
          "key": "market_growth",
          "title": "2. Tốc độ tăng trưởng (Market Growth)",
          "bullets": [
            "Đánh giá tốc độ phát triển của thị trường này (Chậm, Ổn định, hay Tăng trưởng đột phá).",
            "Cung cấp tỷ lệ % tăng trưởng hàng năm (CAGR) nếu có."
          ]
        }
      ]
    },
    {
      "key": "product_ip",
      "title": "III. SẢN PHẨM & SỞ HỮU TRÍ TUỆ (PRODUCT & IP)",
      "items": [
        {
          "key": "product_readiness",
          "title": "1. Độ sẵn sàng của sản phẩm (Product Readiness)",
          "bullets": [
            "Sản phẩm đang ở giai đoạn nào? (Chỉ là ý tưởng, Đã có bản mẫu - Prototype, Đã có bản dùng thử - MVP, hay Sẵn sàng bán đại trà)."
          ]
        },
        {
          "key": "ip_protection",
          "title": "2. Bảo vệ công nghệ (IP Protection)",
          "bullets": [
            "Mô tả các cơ chế bảo vệ sản phẩm khỏi việc bị sao chép.",
            "(Ghi rõ nếu có Bằng sáng chế, dữ liệu độc quyền, thuật toán lõi, hoặc bí mật kinh doanh)."
          ]
        }
      ]
    },
    {
      "key": "competition",
      "title": "IV. MÔI TRƯỜNG CẠNH TRANH (COMPETITION)",
      "items": [
        {
          "key": "competitors",
          "title": "1. Các đối thủ hiện tại (Competitors)",
          "bullets": [
            "Liệt kê 1-3 đối thủ chính đang giải quyết cùng bài toán.",
            "Điểm khác biệt cốt lõi (Unique Value Proposition) của dự án so với họ."
          ]
        },
        {
          "key": "barrier_to_entry",
          "title": "2. Rào cản gia nhập (Barrier To Entry)",
          "bullets": [
            "Mô tả lý do tại sao các đối thủ mới rất khó để nhảy vào cạnh tranh với bạn.",
            "(Ví dụ: Rào cản về giấy phép y tế/tài chính, đòi hỏi vốn cực lớn, hiệu ứng mạng lưới, hoặc chi phí chuyển đổi của khách hàng cao)."
          ]
        }
      ]
    },
    {
      "key": "traction",
      "title": "V. LỰC KÉO THỊ TRƯỜNG (TRACTION)",
      "items": [
        {
          "key": "current_traction",
          "title": "1. Tình trạng kinh doanh hiện tại (Current Traction)",
          "bullets": [
            "Dự án đang ở giai đoạn: Chưa có doanh thu, Đã có người dùng thử nghiệm, Đã có doanh thu, hay Đã có lợi nhuận?",
            "Cung cấp số liệu chứng minh (Ví dụ: Số lượng người dùng, doanh thu hàng tháng, số lượng hợp đồng B2B đã ký...)."
          ]
        }
      ]
    },
    {
      "key": "investment_need",
      "title": "VI. TÀI CHÍNH & NHU CẦU GỌI VỐN (INVESTMENT NEED)",
      "items": [
        {
          "key": "runway",
          "title": "1. Dòng tiền sinh tồn (Runway)",
          "bullets": [
            "Tốc độ \"đốt tiền\" (Burn rate) mỗi tháng hiện tại là bao nhiêu?",
            "Với số vốn tự có hiện tại, dự án có thể duy trì hoạt động thêm bao nhiêu tháng nữa nếu không có doanh thu/vốn mới? (Dưới 6 tháng, 6-12 tháng, hay trên 12 tháng)."
          ]
        },
        {
          "key": "use_of_funds",
          "title": "2. Mục đích gọi vốn (Use of Funds)",
          "bullets": [
            "Nêu rõ số tiền muốn gọi ở vòng này.",
            "Kế hoạch phân bổ số vốn này trong 6 - 12 tháng tiếp theo."
          ]
        }
      ]
    }
  ]
}
""";

            migrationBuilder.Sql($"""
INSERT INTO due_diligence_templates ("Id", "ContentJson", "CreatedAt", "UpdatedAt")
VALUES (1, $$ {templateJson} $$, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT ("Id") DO UPDATE
SET "ContentJson" = EXCLUDED."ContentJson",
    "UpdatedAt" = CURRENT_TIMESTAMP;
""");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "due_diligence_templates");
        }
    }
}
