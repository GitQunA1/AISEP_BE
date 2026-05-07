using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using System.Globalization;
using AISEP.DAL.Entities;
using AISEP.BLL.Helpers;
using AISEP.BLL.Settings;
using Microsoft.Extensions.Options;

namespace AISEP.BLL.Services.AI
{
    public class OpenAiService : IOpenAiService
    {
        private readonly OpenAiSettings          _settings;
        private readonly HttpClient              _httpClient;
        private readonly ILogger<OpenAiService> _logger;

        public OpenAiService(IOptions<OpenAiSettings> settings, HttpClient httpClient, ILogger<OpenAiService> logger)
        {
            _settings   = settings.Value;
            _httpClient = httpClient;
            _logger     = logger;
        }

        public async Task<AiAnalysisResult> AnalyzeProjectAsync(Project project, ScorecardBaseScoreResult baseScore, string? documentText = null)
        {
            var prompt = BuildAnalysisPrompt(project, baseScore, "startup", documentText);

            _logger.LogInformation("Calling OpenAI startup analysis: model={Model}, baseScore={BaseScore}",
                _settings.Model, baseScore.TotalScore);

            var responseJson = await CallOpenAiAsync(prompt, [], BuildAnalysisJsonSchemaFormat());
            return ParseResponse(responseJson);
        }

        public async Task<AiEligibilityResult> EvaluateStartupEligibilityAsync(Project project, IEnumerable<Document> documents, string? documentText = null)
        {
            var docList = documents.ToList();
            var prompt = BuildEligibilityPrompt(project, docList, documentText);

            _logger.LogInformation("Calling OpenAI eligibility evaluation: model={Model}", _settings.Model);
            var responseJson = await CallOpenAiAsync(prompt, []);
            return ParseEligibilityResponse(responseJson);
        }

        public async Task<AiAnalysisResult> AnalyzeProjectForInvestorAsync(Project project, ScorecardBaseScoreResult baseScore, string? documentText = null)
        {
            var prompt = BuildAnalysisPrompt(project, baseScore, "investor", documentText);

            _logger.LogInformation("Calling OpenAI investor analysis: model={Model}, baseScore={BaseScore}",
                _settings.Model, baseScore.TotalScore);

            var responseJson = await CallOpenAiAsync(prompt, [], BuildAnalysisJsonSchemaFormat());
            return ParseResponse(responseJson);
        }

        private string BuildAnalysisPrompt(Project project, ScorecardBaseScoreResult baseScore, string audience, string? documentText = null)
        {
            var baseScoreText = baseScore.TotalScore.ToString("0.##", CultureInfo.InvariantCulture);
            var audienceGuidance = audience == "investor"
                ? """
                Đối tượng sử dụng kết quả: nhà đầu tư đang đánh giá cơ hội đầu tư.

                HƯỚNG DẪN NỘI DUNG CHO NHÀ ĐẦU TƯ:
                - Strengths: Viết dưới góc nhìn cơ hội đầu tư: điểm hấp dẫn về thị trường, traction, đội ngũ, sản phẩm, lợi thế cạnh tranh, khả năng tạo upside.
                - Weaknesses: Viết dưới góc nhìn rủi ro đầu tư: rủi ro mất vốn, thiếu bằng chứng, thị trường nhỏ, traction yếu, sao chép dễ, phụ thuộc outsource, runway ngắn, câu chuyện tăng trưởng chưa rõ.
                - Advice: Không viết kiểu "sửa PDF" hoặc "startup nên bổ sung hồ sơ" là trọng tâm chính. Hãy viết thành khuyến nghị thẩm định cho investor: nên hỏi startup câu gì, cần kiểm chứng chỉ số nào, cần xem tài liệu nào, nên cân nhắc quyết định đầu tư/watchlist/reject ra sao dựa trên rủi ro.
                - Ngôn ngữ phải giúp investor hiểu: cơ hội đầu tư này đáng quan tâm ở đâu, rủi ro lớn nhất là gì, và trước khi đầu tư cần due diligence thêm gì.
                """
                : """
                Đối tượng sử dụng kết quả: startup đang tự đánh giá và cải thiện dự án.

                HƯỚNG DẪN NỘI DUNG CHO STARTUP:
                - Strengths: Viết dưới góc nhìn điểm mạnh hiện có của dự án/startup, những phần nên giữ lại và nhấn mạnh trong hồ sơ.
                - Weaknesses: Viết dưới góc nhìn điểm yếu cần cải thiện: thiếu bằng chứng, checklist khai quá cao, tài liệu chưa rõ, chỉ số traction/market/team/product còn thiếu.
                - Advice: Viết thành hành động cải thiện dự án và hồ sơ: cần bổ sung số liệu nào, chỉnh checklist nào, thêm bằng chứng nào vào PDF, cải thiện pitch deck/business plan ra sao.
                - Ngôn ngữ phải giúp startup biết chính xác cần sửa gì để hồ sơ minh bạch hơn và điểm đánh giá tốt hơn.
                """;
            var stageOptionId = project.StageOptionId?.ToString() ?? "N/A";
            var documentSection = string.IsNullOrWhiteSpace(documentText)
                ? "Không có nội dung PDF đọc được."
                : documentText.Trim();
            var scoreBreakdownJson = JsonSerializer.Serialize(baseScore.ToScoreBreakdown());
            var checklistJson = JsonSerializer.Serialize(BuildStartupChecklist(project));

            return $$"""
                Bạn là Kiểm toán viên Thẩm định Đầu tư (Due Diligence Auditor) cấp cao.
                Nhiệm vụ của bạn là kiểm tra chéo (Cross-check) độ trung thực giữa Form khai báo (Checklist) của Startup và bằng chứng thực tế trong file PDF.

                DỮ LIỆU ĐẦU VÀO:

                Bảng phân bổ điểm (Score Breakdown): Chứa MaxScore (Điểm tối đa) và CurrentBaseScore (Điểm gốc hiện tại) của từng tiêu chí.

                Dữ liệu Form Checklist mà Startup đã khai báo.

                Nội dung Text rút trích từ tài liệu PDF.

                QUY TẮC TRỪ/THƯỞNG ĐIỂM (BẮT BUỘC TUÂN THỦ NGHIÊM NGẶT VỀ TOÁN HỌC):
                Duyệt qua từng hạng mục. Giá trị 'Adjustment' (Điểm điều chỉnh) phải được tính dựa trên CurrentBaseScore của hạng mục đó. Adjustment phải là số âm hoặc 0, trừ khi minh chứng cực kỳ xuất sắc mới được thưởng nhẹ.

                Tình huống 1 (Khớp / Có minh chứng đầy đủ): PDF có bằng chứng rõ ràng, cụ thể và khớp với Form Startup đã khai. Không trừ điểm. Adjustment = 0. Nếu minh chứng cực kỳ xuất sắc, được thưởng nhẹ nhưng tổng điểm thưởng toàn bài tối đa +5.

                Tình huống 2 (Có minh chứng nhưng yếu hơn Form khai): PDF có nhắc đến nội dung Startup đã khai, nhưng mức độ chứng minh thấp hơn, thiếu chi tiết, hoặc không đủ mạnh so với Form. BẮT BUỘC chọn đúng một trong 3 mức sau, không được tự chọn số khác:
                - 2A. Lệch nhẹ: PDF có bằng chứng nhưng chỉ thiếu chi tiết phụ, số liệu phụ hoặc diễn giải chưa đầy đủ. Adjustment = -25% CurrentBaseScore của hạng mục đó.
                - 2B. Lệch vừa: PDF có nhắc đến nhưng thiếu dữ liệu quan trọng, thiếu bằng chứng chính, hoặc chứng minh yếu hơn đáng kể so với Form. Adjustment = -50% CurrentBaseScore của hạng mục đó.
                - 2C. Lệch nặng: PDF có nhắc đến rất sơ sài, gần như không đủ chứng minh cho mức Form đã khai, nhưng chưa hoàn toàn trái ngược. Adjustment = -75% CurrentBaseScore của hạng mục đó.

                Tình huống 3 (Không có bằng chứng / Khai khống / Trái ngược): Form khai một ưu điểm nhưng PDF hoàn toàn không có bằng chứng cụ thể, hoặc PDF ghi thông tin trái ngược. Adjustment = -100% CurrentBaseScore của hạng mục đó, tức bằng đúng -CurrentBaseScore. TUYỆT ĐỐI KHÔNG TRỪ LỐ.

                LƯU Ý TOÁN HỌC:
                - Làm tròn Adjustment đến 2 chữ số thập phân.
                - Adjustment không được nhỏ hơn -CurrentBaseScore của hạng mục.
                - Với Tình huống 2, bắt buộc dùng đúng -25%, -50%, hoặc -75%; không dùng các mức mơ hồ như -30% hoặc -40%.

                RÀNG BUỘC NGÔN NGỮ: Bắt buộc trả lời bằng Tiếng Việt chuyên ngành.

                ĐỊNH DẠNG OUTPUT JSON:
                Trả về chuẩn JSON cấu trúc sau. AI chỉ phân tích nội dung và trả Adjustment từng tiêu chí. KHÔNG trả TotalAIAdjustmentScore, BaseScore, FinalPotentialScore, MaxScore, BaseScore hoặc FinalScore trong output. Backend C# sẽ tự tính toàn bộ các tổng điểm và điểm cuối cùng.

                AuditedItems (array of objects): Mỗi object gồm:

                Criteria (string): Tên tiêu chí.

                Finding (string): Nhận xét, so sánh giữa Form và PDF.

                Adjustment (decimal): Điểm cộng/trừ của mục này (Tuân thủ nghiêm ngặt luật toán học ở trên).

                Strengths (array of strings): Điểm mạnh/điểm hấp dẫn, viết đúng theo vai trò người đọc.

                Weaknesses (array of strings): Điểm yếu/rủi ro lớn nhất, viết đúng theo vai trò người đọc.

                Advice (array of strings): Khuyến nghị hành động tiếp theo, viết đúng theo vai trò người đọc.

                {{audienceGuidance}}

                BaseScore tổng hiện tại: {{baseScoreText}}/100.
                Không tự tính FinalPotentialScore hoặc TotalAIAdjustmentScore. Backend C# sẽ tự cộng Adjustment từ AuditedItems và tính các điểm tổng.

                --- BẢNG ĐIỂM CHI TIẾT C# ---
                {{scoreBreakdownJson}}

                --- CHECKLIST/FORM STARTUP ĐÃ KHAI BÁO ---
                {{checklistJson}}

                --- DỮ LIỆU TEXT DỰ ÁN ---
                Tên dự án: {{project.ProjectName}}
                Mô tả ngắn: {{project.ShortDescription ?? "N/A"}}
                StageOptionId: {{stageOptionId}}
                Bài toán: {{project.ProblemStatement ?? "N/A"}}
                Giải pháp: {{project.SolutionDescription ?? "N/A"}}
                Khách hàng mục tiêu: {{project.TargetCustomers ?? "N/A"}}
                Giá trị khác biệt: {{project.UniqueValueProposition ?? "N/A"}}
                Mô hình kinh doanh: {{project.BusinessModel ?? "N/A"}}
                Đối thủ cạnh tranh: {{project.Competitors ?? "N/A"}}

                --- NỘI DUNG PDF ĐÍNH KÈM ĐÃ TRÍCH XUẤT ---
                {{documentSection}}
                """;
        }

        private static Dictionary<string, object?> BuildStartupChecklist(Project project)
        {
            var scorecard = project.Scorecard;
            return new Dictionary<string, object?>
            {
                ["ProjectName"] = project.ProjectName,
                ["ShortDescription"] = project.ShortDescription,
                ["StageOptionId"] = project.StageOptionId,
                ["ProblemStatement"] = project.ProblemStatement,
                ["SolutionDescription"] = project.SolutionDescription,
                ["TargetCustomers"] = project.TargetCustomers,
                ["UniqueValueProposition"] = project.UniqueValueProposition,
                ["BusinessModel"] = project.BusinessModel,
                ["Competitors"] = project.Competitors,
                ["IndustryOptionId"] = project.IndustryOptionId,
                ["Team"] = scorecard is null ? null : new
                {
                    scorecard.TeamSize,
                    scorecard.TeamExperience,
                    scorecard.HasTechnicalCofounder
                },
                ["Market"] = scorecard is null ? null : new
                {
                    scorecard.TargetMarketSize,
                    scorecard.MarketGrowth
                },
                ["Product"] = scorecard is null ? null : new
                {
                    scorecard.ProductReadiness,
                    scorecard.IPProtection
                },
                ["Competition"] = scorecard is null ? null : new
                {
                    scorecard.BarrierToEntry
                },
                ["Traction"] = scorecard is null ? null : new
                {
                    scorecard.CurrentTraction
                },
                ["InvestmentNeed"] = scorecard is null ? null : new
                {
                    scorecard.RunwayMonths
                }
            };
        }

        private string BuildEligibilityPrompt(Project project, List<Document> documents, string? documentText)
        {
            var readable = documents.Where(d => GetMimeType(d.FileName) is not null).ToList();
            var skipped = documents.Where(d => GetMimeType(d.FileName) is null).ToList();
            var docCount = readable.Count;
            var documentSection = string.IsNullOrWhiteSpace(documentText)
                ? "Không có nội dung PDF đọc được."
                : documentText.Trim();

            var docSummary = docCount > 0
                ? string.Join(", ", readable.Select(d => $"{d.DocumentType} ({d.FileName})"))
                : "Không có tài liệu hỗ trợ.";
            var skippedSummary = skipped.Count > 0
                ? $" | Bỏ qua (định dạng không hỗ trợ): {string.Join(", ", skipped.Select(d => d.FileName))}"
                : string.Empty;

            return $$"""
                Bạn là Trợ lý Sàng lọc Hồ sơ Dự án (Document Screening AI) của nền tảng kết nối đầu tư AISEP.
                Nhiệm vụ của bạn là kiểm tra nhanh (sanity check) xem các file PDF người dùng tải lên có thực sự thuộc về dự án đang khai báo hay không.

                MỤC TIÊU:
                - Chỉ xác nhận tài liệu có cùng dự án, cùng sản phẩm, cùng lĩnh vực, hoặc cùng bài toán với thông tin dự án hay không.
                - Đây KHÔNG phải bước chấm điểm startup, KHÔNG phải thẩm định traction/tài chính/pháp lý.
                - Chấm rất nhẹ tay. Nếu tài liệu có liên quan cơ bản đến dự án thì chấp nhận.

                THÔNG TIN DỰ ÁN CẦN ĐỐI CHIẾU:
                - Tên dự án.
                - Mô tả ngắn.
                - Vấn đề giải quyết.
                - Giải pháp.
                - Khách hàng mục tiêu.
                - Giá trị khác biệt, mô hình kinh doanh, đối thủ nếu có.

                TIÊU CHÍ CHẤP NHẬN (is_eligible_startup = true):
                - PDF nói về cùng tên dự án, hoặc tên hơi khác nhưng core sản phẩm/giải pháp giống.
                - PDF nói về cùng lĩnh vực, cùng nhóm khách hàng, hoặc cùng bài toán mà dự án khai báo.
                - PDF thiếu số liệu tài chính, thiếu traction, thiếu market size, hoặc mô tả chưa đầy đủ nhưng vẫn liên quan đến dự án.
                - PDF là pitch deck, proposal, mô tả sản phẩm, tài liệu kỹ thuật, business plan, hoặc tài liệu giới thiệu có liên quan.

                CHỈ TỪ CHỐI (is_eligible_startup = false) KHI VÀ CHỈ KHI:
                - Nội dung PDF hoàn toàn không liên quan đến dự án đang khai báo, ví dụ sách giáo khoa, thực đơn, tiểu thuyết, hóa đơn, CV cá nhân không liên quan, tài liệu rác.
                - PDF nói về một công ty/startup/sản phẩm hoàn toàn khác và giải quyết bài toán hoàn toàn khác.
                - PDF không đọc được hoặc không có nội dung đủ để đối chiếu với dự án.

                NHỮNG ĐIỀU PHẢI BỎ QUA:
                - Không từ chối chỉ vì thiếu tài chính, thiếu traction, thiếu revenue, thiếu market size.
                - Không từ chối chỉ vì tên dự án hơi khác nếu sản phẩm/bài toán/giải pháp vẫn khớp.
                - Không đánh giá dự án có sáng tạo, có tiềm năng mở rộng, hay có đủ điều kiện startup theo IDEO/Lean Startup.

                RÀNG BUỘC NGÔN NGỮ:
                - Bắt buộc trả lời bằng tiếng Việt.
                - Lý do tối đa 2 câu, rõ ràng để Staff báo lại cho người dùng.

                --- PROJECT DATA ---
                Name: {{project.ProjectName}}
                Short Description: {{project.ShortDescription ?? "N/A"}}
                StageOptionId: {{project.StageOptionId?.ToString() ?? "N/A"}}
                Problem Statement: {{project.ProblemStatement ?? "N/A"}}
                Solution: {{project.SolutionDescription ?? "N/A"}}
                Target Customers: {{project.TargetCustomers ?? "N/A"}}
                Unique Value Proposition: {{project.UniqueValueProposition ?? "N/A"}}
                Business Model: {{project.BusinessModel ?? "N/A"}}
                Competitors: {{project.Competitors ?? "N/A"}}
                Uploaded Documents ({{docCount}} tài liệu đọc được){{skippedSummary}}:
                {{docSummary}}

                --- NỘI DUNG PDF ĐÍNH KÈM ĐÃ TRÍCH XUẤT ---
                {{documentSection}}

                ĐỊNH DẠNG OUTPUT JSON:
                Trả về ONLY JSON, không markdown, không text thêm:
                {
                  "is_eligible_startup": <boolean>,
                  "eligibility_reason": "<string tiếng Việt tối đa 2 câu>"
                }

                Quy ước:
                - is_eligible_startup = true nghĩa là tài liệu hợp lệ, thuộc về dự án hoặc liên quan cơ bản đến dự án.
                - is_eligible_startup = false nghĩa là tài liệu rác, tải nhầm, không đọc được, hoặc thuộc dự án khác.
                """;
        }

        private async Task<string> CallOpenAiAsync(string prompt, List<object> inputParts, object? textFormat = null)
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                throw new HttpRequestException("Thiếu OpenAI API key. Hãy cấu hình OpenAISettings:ApiKey trong appsettings hoặc biến môi trường.");
            }

            var parts = new List<object> { new { type = "input_text", text = prompt } };
            parts.AddRange(inputParts);

            var requestBody = new Dictionary<string, object?>
            {
                ["model"] = _settings.Model,
                ["input"] = new[]
                {
                    new
                    {
                        role = "user",
                        content = parts.ToArray()
                    }
                },
                ["temperature"] = _settings.Temperature,
                ["max_output_tokens"] = _settings.MaxOutputTokens
            };

            if (textFormat is not null)
            {
                requestBody["text"] = new { format = textFormat };
            }

            var url         = $"{_settings.BaseUrl.TrimEnd('/')}/responses";
            var bodyJson    = JsonSerializer.Serialize(requestBody);
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            _logger.LogDebug("OpenAI URL: {Url}", url);
            _logger.LogDebug("OpenAI request body size: {Size} bytes", Encoding.UTF8.GetByteCount(bodyJson));

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                var statusCode = (int)response.StatusCode;

                var message = statusCode switch
                {
                    401 => "OpenAI API key không hợp lệ hoặc chưa được cấp quyền.",
                    403 => "OpenAI API bị từ chối quyền. Kiểm tra OpenAISettings:ApiKey và quyền truy cập model.",
                    404 => $"Model '{_settings.Model}' không tồn tại hoặc không được hỗ trợ. Kiểm tra lại OpenAISettings.Model trong appsettings.json.",
                    429 => "OpenAI API vượt quá quota hoặc rate limit. Vui lòng chờ hoặc kiểm tra billing/limits.",
                    500 => "OpenAI API lỗi phía server. Thử lại sau.",
                    _   => $"OpenAI API trả về lỗi {statusCode}."
                };

                throw new HttpRequestException($"{message}\nChi tiết: {errorBody}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        private static object BuildAnalysisJsonSchemaFormat()
        {
            return new
            {
                type = "json_schema",
                name = "project_ai_adjustment_analysis",
                strict = true,
                schema = new
                {
                    type = "object",
                    additionalProperties = false,
                    properties = new
                    {
                        AuditedItems = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "object",
                                additionalProperties = false,
                                properties = new
                                {
                                    Criteria = new
                                    {
                                        type = "string",
                                        description = "Ten tieu chi duoc kiem toan, vi du Team, Market, Product, Competition, Traction, InvestmentNeed."
                                    },
                                    Finding = new
                                    {
                                        type = "string",
                                        description = "Nhan xet kiem toan bang tieng Viet ve viec checklist co khop voi bang chung PDF hay khong."
                                    },
                                    Adjustment = new
                                    {
                                        type = "number",
                                        minimum = -100,
                                        maximum = 5,
                                        description = "Diem cong/tru rieng cua tieu chi nay."
                                    }
                                },
                                required = new[] { "Criteria", "Finding", "Adjustment" }
                            },
                            description = "Danh sach kiem toan tung hang muc."
                        },
                        Strengths = new
                        {
                            type = "array",
                            items = new { type = "string" },
                            description = "Cac diem manh bang tieng Viet."
                        },
                        Weaknesses = new
                        {
                            type = "array",
                            items = new { type = "string" },
                            description = "Cac rui ro hoac diem yeu bang tieng Viet."
                        },
                        Advice = new
                        {
                            type = "array",
                            items = new { type = "string" },
                            description = "Loi khuyen hanh dong bang tieng Viet."
                        }
                    },
                    required = new[]
                    {
                        "AuditedItems",
                        "Strengths",
                        "Weaknesses",
                        "Advice"
                    }
                }
            };
        }

    
        private async Task<List<object>> BuildInputPartsAsync(List<Document> documents)
        {
            var parts = new List<object>();
            var index = 0;

            foreach (var doc in documents)
            {
                var mimeType = GetMimeType(doc.FileName);
                if (mimeType is null) continue;

                index++;

                try
                {
                    var bytes  = await _httpClient.GetByteArrayAsync(doc.FileUrl);
                    var base64 = Convert.ToBase64String(bytes);

                    parts.Add(new
                    {
                        type = "input_text",
                        text = $"Document #{index}: Type={doc.DocumentType}, FileName={doc.FileName}. Hãy kiểm tra mức độ liên quan của tài liệu này với dự án trước khi dùng làm bằng chứng."
                    });

                    if (mimeType == "application/pdf")
                    {
                        parts.Add(new
                        {
                            type = "input_file",
                            filename = doc.FileName,
                            file_data = base64
                        });
                    }
                    else
                    {
                        parts.Add(new
                        {
                            type = "input_image",
                            image_url = $"data:{mimeType};base64,{base64}"
                        });
                    }
                }
                catch
                {
                    
                }
            }

            return parts;
        }

   
        private static string? GetMimeType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".pdf"  => "application/pdf",
                ".png"  => "image/png",
                ".jpg"  => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                ".gif"  => "image/gif",
                _       => null  
            };
        }

        private AiAnalysisResult ParseResponse(string responseJson)
        {
            var text = ExtractTextFromOpenAiResponse(responseJson);

            // Strip markdown code blocks the model may wrap around JSON.
            text = Regex.Replace(text, @"```json\s*", "").Replace("```", "").Trim();

            _logger.LogDebug("OpenAI raw text response: {Text}", text);

            // Handle truncated JSON: attempt to auto-close the object
            if (!text.TrimEnd().EndsWith('}'))
            {
                _logger.LogWarning("OpenAI response appears truncated. Attempting auto-repair...");
                text = text.TrimEnd().TrimEnd(',') + "}";
            }

            try
            {
                var result = JsonSerializer.Deserialize<AiAnalysisResult>(text,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new AiAnalysisResult();

                result.TotalAIAdjustmentScore = Math.Clamp(result.TotalAIAdjustmentScore, -100m, 5m);
                result.AIAdjustmentScore = result.TotalAIAdjustmentScore;
                result.Reasoning ??= string.Empty;
                result.AuditedItems ??= [];
                result.Strengths ??= [];
                result.Weaknesses ??= [];
                result.Advice ??= [];
                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError("Failed to parse OpenAI response. Raw text: {Text}\nError: {Error}", text, ex.Message);
                return new AiAnalysisResult();
            }
        }

        private AiEligibilityResult ParseEligibilityResponse(string responseJson)
        {
            var text = ExtractTextFromOpenAiResponse(responseJson);

            text = Regex.Replace(text, @"```json\s*", "").Replace("```", "").Trim();

            if (!text.TrimEnd().EndsWith('}'))
            {
                text = text.TrimEnd().TrimEnd(',') + "}";
            }

            try
            {
                var result = JsonSerializer.Deserialize<AiEligibilityResult>(
                    text,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result is null)
                {
                    return new AiEligibilityResult
                    {
                        IsEligibleStartup = false,
                        EligibilityReason = "Không thể đọc được kết quả sàng lọc tài liệu. Vui lòng thử lại hoặc kiểm tra file PDF đã tải lên."
                    };
                }

                if (string.IsNullOrWhiteSpace(result.EligibilityReason))
                {
                    result.EligibilityReason = result.IsEligibleStartup
                        ? "Tài liệu có liên quan cơ bản đến nội dung dự án."
                        : "Tài liệu đính kèm không đủ thông tin để đối chiếu với nội dung dự án.";
                }

                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError("Failed to parse OpenAI eligibility response. Raw text: {Text}\nError: {Error}", text, ex.Message);
                return new AiEligibilityResult
                {
                    IsEligibleStartup = false,
                    EligibilityReason = "Không thể phân tích kết quả sàng lọc tài liệu hợp lệ. Vui lòng thử lại hoặc kiểm tra file PDF đã tải lên."
                };
            }
        }

        private static string ExtractTextFromOpenAiResponse(string responseJson)
        {
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("output_text", out var outputText)
                && outputText.ValueKind == JsonValueKind.String)
            {
                return outputText.GetString() ?? "{}";
            }

            if (root.TryGetProperty("output", out var output)
                && output.ValueKind == JsonValueKind.Array)
            {
                var builder = new StringBuilder();
                foreach (var item in output.EnumerateArray())
                {
                    if (!item.TryGetProperty("content", out var content)
                        || content.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (var part in content.EnumerateArray())
                    {
                        if (part.TryGetProperty("text", out var text)
                            && text.ValueKind == JsonValueKind.String)
                        {
                            builder.Append(text.GetString());
                        }
                    }
                }

                if (builder.Length > 0)
                {
                    return builder.ToString();
                }
            }

            return "{}";
        }
    }
}
