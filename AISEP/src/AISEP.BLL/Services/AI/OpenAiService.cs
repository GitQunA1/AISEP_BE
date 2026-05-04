using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using AISEP.DAL.Entities;
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

        public async Task<AiAnalysisResult> AnalyzeProjectAsync(Project project, IEnumerable<Document> documents)
        {
            var docList     = documents.ToList();
            var prompt      = BuildPrompt(project, docList);
            var inputParts = await BuildInputPartsAsync(docList);

            _logger.LogInformation("Calling OpenAI: model={Model}, inputParts={Count}",
                _settings.Model, inputParts.Count);

            try
            {
                var responseJson = await CallOpenAiAsync(prompt, inputParts);
                return ParseResponse(responseJson);
            }
            catch (HttpRequestException ex) when (inputParts.Count > 0)
            {
                _logger.LogWarning("OpenAI document input call failed ({Msg}). Retrying text-only...", ex.Message);
                var responseJson = await CallOpenAiAsync(prompt, []);
                return ParseResponse(responseJson);
            }
        }

        public async Task<AiEligibilityResult> EvaluateStartupEligibilityAsync(Project project, IEnumerable<Document> documents)
        {
            var docList = documents.ToList();
            var prompt = BuildEligibilityPrompt(project, docList);
            var inputParts = await BuildInputPartsAsync(docList);

            _logger.LogInformation("Calling OpenAI eligibility evaluation: model={Model}, inputParts={Count}",
                _settings.Model, inputParts.Count);
            try
            {
                var responseJson = await CallOpenAiAsync(prompt, inputParts);
                return ParseEligibilityResponse(responseJson);
            }
            catch (HttpRequestException ex) when (inputParts.Count > 0)
            {
                _logger.LogWarning("Eligibility document input call failed ({Msg}). Retrying text-only...", ex.Message);
                var responseJson = await CallOpenAiAsync(prompt, []);
                return ParseEligibilityResponse(responseJson);
            }
        }

        public async Task<AiAnalysisResult> AnalyzeProjectForInvestorAsync(Project project, IEnumerable<Document> documents)
        {
            var docList = documents.ToList();
            var prompt = BuildInvestorPrompt(project, docList);
            var inputParts = await BuildInputPartsAsync(docList);

            _logger.LogInformation("Calling OpenAI (Investor mode): model={Model}, inputParts={Count}",
                _settings.Model, inputParts.Count);

            try
            {
                var responseJson = await CallOpenAiAsync(prompt, inputParts);
                return ParseResponse(responseJson);
            }
            catch (HttpRequestException ex) when (inputParts.Count > 0)
            {
                _logger.LogWarning("Investor document input call failed ({Msg}). Retrying text-only...", ex.Message);
                var responseJson = await CallOpenAiAsync(prompt, []);
                return ParseResponse(responseJson);
            }
        }

        private string BuildPrompt(Project project, List<Document> documents)
        {
            var readable = documents.Where(d => GetMimeType(d.FileName) is not null).ToList();
            var skipped  = documents.Where(d => GetMimeType(d.FileName) is null).ToList();
            var docCount = readable.Count;

            var docSummary = docCount > 0
                ? string.Join(", ", readable.Select(d => $"{d.DocumentType} ({d.FileName})"))
                : "Không có tài liệu đọc được.";
            var skippedSummary = skipped.Count > 0
                ? $" | Bỏ qua (định dạng không hỗ trợ): {string.Join(", ", skipped.Select(d => d.FileName))}"
                : string.Empty;

            return $$"""
                Bạn là chuyên gia thẩm định startup theo phương pháp Bill Payne Scorecard.
                Nhiệm vụ: phân tích dự án và tài liệu đính kèm, chấm điểm 7 thành phần.
                BẮT BUỘC:
                - Chỉ trả về 1 JSON hợp lệ, không markdown, không text thừa.
                - TẤT CẢ nội dung chữ trong JSON phải là tiếng Việt (reason, evidence, missingData, Summary, Strengths, Weaknesses, Recommendations).

                --- DỮ LIỆU DỰ ÁN ---
                Tên dự án: {{project.ProjectName}}
                Mô tả ngắn: {{project.ShortDescription ?? "N/A"}}
                StageOptionId: {{project.StageOptionId?.ToString() ?? "N/A"}}
                Bài toán: {{project.ProblemStatement ?? "N/A"}}
                Giải pháp: {{project.SolutionDescription ?? "N/A"}}
                Khách hàng mục tiêu: {{project.TargetCustomers ?? "N/A"}}
                Giá trị khác biệt: {{project.UniqueValueProposition ?? "N/A"}}
                Mô hình kinh doanh: {{project.BusinessModel ?? "N/A"}}
                Đối thủ cạnh tranh: {{project.Competitors ?? "N/A"}}
                Tài liệu tải lên ({{docCount}} tài liệu đọc được){{skippedSummary}}:
                {{docSummary}}

                --- CÁCH CHẤM ĐIỂM ---
                Chấm tuyệt đối từng thành phần theo thang 0.0-10.0:
                - 5.0 = mức trung bình thị trường
                - 7.5 = mạnh
                - 9.0+ = xuất sắc
                - dưới 4.0 = yếu

                7 thành phần:
                1) Team: thong tin doi ngu (neu co)
                2) Opportunity: TargetCustomers
                3) Product: SolutionDescription, StageOptionId
                4) Competition: Competitors, UniqueValueProposition
                5) Marketing: BusinessModel
                6) Investment: độ rõ ràng nhu cầu vốn và sử dụng vốn
                7) Other: chất lượng tài liệu, tính nhất quán tổng thể

                --- GIAI ĐOẠN PHÁT TRIỂN ---
                Stage là option động từ hệ thống. Chỉ dùng giá trị được cung cấp như ngữ cảnh đánh giá, không giả định danh sách stage cố định.

                --- RUBRIC (NGHIÊM NGẶT) ---
                - 0.0-2.0: Thiếu dữ liệu hoặc dữ liệu không liên quan.
                - 2.1-4.0: Có thông tin cơ bản nhưng bằng chứng yếu.
                - 4.1-6.5: Mức trung bình thị trường, bằng chứng chấp nhận được.
                - 6.6-8.5: Mạnh, có bằng chứng cụ thể và kiểm chứng được.
                - 8.6-10.0: Xuất sắc, bằng chứng nổi trội và có traction rõ.
                Nếu độ tin cậy thấp, phải chấm bảo thủ.

                Không tự tính điểm tổng có trọng số. Backend sẽ xử lý phần điểm riêng.
                Yêu cầu đầu ra:
                - Summary: 2-4 câu tiếng Việt, nêu tài liệu/bằng chứng nào ảnh hưởng điểm.
                - Strengths: 3-5 ý tiếng Việt.
                - Weaknesses: 3-5 ý tiếng Việt.
                - Recommendations: 5-8 hành động tiếng Việt, theo thứ tự ưu tiên, tập trung trực tiếp vào việc cải thiện dự án (đội ngũ, sản phẩm, thị trường, vận hành), không chỉ để tăng điểm.

                Tự kiểm tra trước khi trả kết quả:
                1) Mỗi component phải có ít nhất 1 phần tử trong evidence hoặc missingData.
                2) Điểm > 6.5 phải có bằng chứng cụ thể.
                3) Trả về JSON hợp lệ duy nhất.

                --- MẪU OUTPUT BẮT BUỘC (CHỈ JSON) ---
                {
                  "Team": {
                    "score": <decimal 0.0-10.0>,
                    "evidence": ["<bằng chứng ngắn 1>", "<bằng chứng ngắn 2>"],
                    "missingData": ["<dữ liệu còn thiếu 1>"],
                    "confidence": <decimal 0.0-1.0>,
                    "reason": "<giải thích lý do chấm điểm bằng tiếng Việt>"
                  },
                  "Opportunity": {
                    "score": <decimal 0.0-10.0>,
                    "evidence": [],
                    "missingData": [],
                    "confidence": <decimal 0.0-1.0>,
                    "reason": "<giải thích lý do chấm điểm bằng tiếng Việt>"
                  },
                  "Product": {
                    "score": <decimal 0.0-10.0>,
                    "evidence": [],
                    "missingData": [],
                    "confidence": <decimal 0.0-1.0>,
                    "reason": "<giải thích lý do chấm điểm bằng tiếng Việt>"
                  },
                  "Competition": {
                    "score": <decimal 0.0-10.0>,
                    "evidence": [],
                    "missingData": [],
                    "confidence": <decimal 0.0-1.0>,
                    "reason": "<giải thích lý do chấm điểm bằng tiếng Việt>"
                  },
                  "Marketing": {
                    "score": <decimal 0.0-10.0>,
                    "evidence": [],
                    "missingData": [],
                    "confidence": <decimal 0.0-1.0>,
                    "reason": "<giải thích lý do chấm điểm bằng tiếng Việt>"
                  },
                  "Investment": {
                    "score": <decimal 0.0-10.0>,
                    "evidence": [],
                    "missingData": [],
                    "confidence": <decimal 0.0-1.0>,
                    "reason": "<giải thích lý do chấm điểm bằng tiếng Việt>"
                  },
                  "Other": {
                    "score": <decimal 0.0-10.0>,
                    "evidence": [],
                    "missingData": [],
                    "confidence": <decimal 0.0-1.0>,
                    "reason": "<giải thích lý do chấm điểm bằng tiếng Việt>"
                  },
                  "Summary": "<tóm tắt tiếng Việt>",
                  "Strengths": ["<điểm mạnh 1>", "<điểm mạnh 2>"],
                  "Weaknesses": ["<điểm yếu 1>", "<điểm yếu 2>"],
                  "Recommendations": ["<hành động 1>", "<hành động 2>"]
                }
                """;
        }

        private string BuildInvestorPrompt(Project project, List<Document> documents)
        {
            var readable = documents.Where(d => GetMimeType(d.FileName) is not null).ToList();
            var skipped = documents.Where(d => GetMimeType(d.FileName) is null).ToList();
            var docCount = readable.Count;

            var docSummary = docCount > 0
                ? string.Join(", ", readable.Select(d => $"{d.DocumentType} ({d.FileName})"))
                : "Không có tài liệu đọc được.";
            var skippedSummary = skipped.Count > 0
                ? $" | Bỏ qua (định dạng không hỗ trợ): {string.Join(", ", skipped.Select(d => d.FileName))}"
                : string.Empty;

            return $$"""
                Bạn là chuyên gia thẩm định dự án ở góc nhìn nhà đầu tư theo Bill Payne Scorecard.
                Nhiệm vụ: phân tích dự án và tài liệu đính kèm để hỗ trợ quyết định đầu tư.
                BẮT BUỘC:
                - Chỉ trả về 1 JSON hợp lệ, không markdown, không text thừa.
                - TẤT CẢ nội dung chữ trong JSON phải là tiếng Việt.

                --- DỮ LIỆU DỰ ÁN ---
                Tên dự án: {{project.ProjectName}}
                Mô tả ngắn: {{project.ShortDescription ?? "N/A"}}
                StageOptionId: {{project.StageOptionId?.ToString() ?? "N/A"}}
                Bài toán: {{project.ProblemStatement ?? "N/A"}}
                Giải pháp: {{project.SolutionDescription ?? "N/A"}}
                Khách hàng mục tiêu: {{project.TargetCustomers ?? "N/A"}}
                Giá trị khác biệt: {{project.UniqueValueProposition ?? "N/A"}}
                Mô hình kinh doanh: {{project.BusinessModel ?? "N/A"}}
                Đối thủ cạnh tranh: {{project.Competitors ?? "N/A"}}
                Tài liệu tải lên ({{docCount}} tài liệu đọc được){{skippedSummary}}:
                {{docSummary}}

                --- HƯỚNG DẪN CHẤM ĐIỂM ---
                Chấm tuyệt đối 0.0-10.0 cho 7 thành phần.
                Không tự gán trọng số cố định, backend sẽ tổng hợp điểm theo trọng số từng giai đoạn.

                --- GIAI ĐOẠN PHÁT TRIỂN ---
                Stage là option động từ hệ thống. Chỉ dùng giá trị được cung cấp như ngữ cảnh đánh giá, không giả định danh sách stage cố định.

                Mốc tham chiếu:
                - 5.0 = trung bình thị trường
                - 7.5 = mạnh
                - 9.0+ = xuất sắc

                --- RUBRIC (NGHIÊM NGẶT) ---
                - 0.0-2.0: Thiếu dữ liệu hoặc dữ liệu không liên quan.
                - 2.1-4.0: Có dữ liệu cơ bản nhưng bằng chứng yếu.
                - 4.1-6.5: Mức trung bình thị trường, có bằng chứng chấp nhận được.
                - 6.6-8.5: Mạnh, bằng chứng rõ ràng và kiểm chứng được.
                - 8.6-10.0: Xuất sắc, bằng chứng nổi trội và traction tốt.
                Nếu độ tin cậy thấp, bắt buộc chấm bảo thủ.

                Trọng tâm nhà đầu tư:
                - Nhấn mạnh khả năng đầu tư được, rủi ro giảm giá trị, rủi ro thực thi, độ tin cậy dữ liệu.
                - Điểm > 6.5 phải có bằng chứng cụ thể từ dự án/tài liệu.
                - Nếu bằng chứng yếu, giảm điểm và bổ sung cảnh báo rủi ro.
                - KHONG tra ve ChaosScore.

                Không tự tính điểm tổng có trọng số. Backend sẽ xử lý phần điểm riêng.
                Tự kiểm tra trước khi trả kết quả:
                1) Mỗi component phải có ít nhất 1 phần tử trong evidence hoặc missingData.
                2) Điểm > 6.5 phải có bằng chứng cụ thể.
                3) InvestmentVerdict phải nhất quán với RiskFlags và DealBreakers.
                4) Trả về JSON hợp lệ duy nhất.

                --- MẪU OUTPUT BẮT BUỘC (CHỈ JSON) ---
                {
                  "Team": {"score": 0.0, "evidence": [], "missingData": [], "confidence": 0.0, "reason": ""},
                  "Opportunity": {"score": 0.0, "evidence": [], "missingData": [], "confidence": 0.0, "reason": ""},
                  "Product": {"score": 0.0, "evidence": [], "missingData": [], "confidence": 0.0, "reason": ""},
                  "Competition": {"score": 0.0, "evidence": [], "missingData": [], "confidence": 0.0, "reason": ""},
                  "Marketing": {"score": 0.0, "evidence": [], "missingData": [], "confidence": 0.0, "reason": ""},
                  "Investment": {"score": 0.0, "evidence": [], "missingData": [], "confidence": 0.0, "reason": ""},
                  "Other": {"score": 0.0, "evidence": [], "missingData": [], "confidence": 0.0, "reason": ""},
                  "Summary": "",
                  "Strengths": [],
                  "Weaknesses": [],
                  "Recommendations": [],
                  "InvestmentVerdict": "Nên đầu tư|Theo dõi|Từ chối",
                  "RiskFlags": [],
                  "DealBreakers": [],
                  "DueDiligenceQuestions": [],
                  "InvestorNextStep": ""
                }
                """;
        }

        private string BuildEligibilityPrompt(Project project, List<Document> documents)
        {
            var readable = documents.Where(d => GetMimeType(d.FileName) is not null).ToList();
            var skipped = documents.Where(d => GetMimeType(d.FileName) is null).ToList();
            var docCount = readable.Count;

            var docSummary = docCount > 0
                ? string.Join(", ", readable.Select(d => $"{d.DocumentType} ({d.FileName})"))
                : "Không có tài liệu hỗ trợ.";
            var skippedSummary = skipped.Count > 0
                ? $" | Bỏ qua (định dạng không hỗ trợ): {string.Join(", ", skipped.Select(d => d.FileName))}"
                : string.Empty;

            return $$"""
                Bạn là một chuyên gia thẩm định dự án Khởi nghiệp Đổi mới Sáng tạo tại Vườn ươm (Incubator).
                Nhiệm vụ: xác định dự án có đủ tư cách là "Startup / Ý tưởng sáng tạo" hay không.
                Bối cảnh: CHẤP NHẬN ý tưởng giai đoạn sớm (Idea Stage), dự án xã hội, mô hình có yếu tố sáng tạo.

                Đánh giá theo Khung 3 Lăng kính Đổi mới của IDEO kết hợp Lean Startup:
                1) Desirability: Có nỗi đau rõ ràng của nhóm người dùng cụ thể không? Giải pháp có logic không?
                2) Innovation / Differentiation: Có yếu tố mới mẻ/khác biệt (công nghệ, mô hình kinh doanh, quy trình, ứng dụng mới)?
                3) Scalability Potential: Nếu thành công có khả năng mở rộng vùng/quốc gia, đóng gói/nhượng quyền/qua Internet?
                4) Feasibility (lọc cơ bản): Có vi phạm pháp luật, phi logic vật lý, lừa đảo, đa cấp, hoặc quá viển vông không?

                Luật loại bỏ (is_eligible_startup = false):
                - Mô hình buôn bán/dịch vụ nhỏ lẻ truyền thống, không sáng tạo, khó nhân rộng.
                - Viển vông, phi logic, thiếu thông tin trầm trọng hoặc có dấu hiệu lừa đảo.

                Còn lại, nếu giải quyết vấn đề cụ thể, có đổi mới dù nhỏ, có tiềm năng mở rộng hoặc ứng dụng mô hình/công nghệ mới:
                - is_eligible_startup = true.

                Quy tắc đánh giá tài liệu đính kèm:
                - Bắt buộc kiểm tra mức độ liên quan của từng tài liệu với Problem Statement, Solution, Business Model và tên dự án.
                - Nếu phần lớn tài liệu không liên quan hoặc mâu thuẫn nội dung dự án, coi là thiếu thông tin trầm trọng.
                - Trong trường hợp đó, bắt buộc kết luận is_eligible_startup = false.
                - eligibility_reason bắt buộc nêu rõ cụm: "Tài liệu đính kèm không liên quan đến nội dung dự án".

                Yêu cầu cho eligibility_reason:
                - Viết tiếng Việt.
                - Tối đa 3 câu ngắn gọn để staff đọc nhanh.
                - Nêu rõ vì sao duyệt hoặc từ chối theo đúng 4 tiêu chí trên.

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

                Trả về ONLY JSON, không markdown, không text thêm:
                {
                  "is_eligible_startup": <boolean>,
                  "eligibility_reason": "<string tiếng Việt tối đa 3 câu>"
                }
                """;
        }

        private async Task<string> CallOpenAiAsync(string prompt, List<object> inputParts)
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                throw new HttpRequestException("Thiếu OpenAI API key. Hãy cấu hình OpenAISettings:ApiKey trong appsettings hoặc biến môi trường.");
            }

            var parts = new List<object> { new { type = "input_text", text = prompt } };
            parts.AddRange(inputParts);

            var requestBody = new
            {
                model = _settings.Model,
                input = new[]
                {
                    new
                    {
                        role = "user",
                        content = parts.ToArray()
                    }
                },
                temperature = _settings.Temperature,
                max_output_tokens = _settings.MaxOutputTokens
            };

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
                return JsonSerializer.Deserialize<AiAnalysisResult>(text,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new AiAnalysisResult();
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
                        EligibilityReason = "Dự án chưa có đủ dữ liệu rõ ràng để kết luận theo bộ tiêu chí IDEO và Lean Startup."
                    };
                }

                if (string.IsNullOrWhiteSpace(result.EligibilityReason))
                {
                    result.EligibilityReason = "Dự án chưa có đủ dữ liệu rõ ràng để kết luận theo bộ tiêu chí IDEO và Lean Startup.";
                }

                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError("Failed to parse OpenAI eligibility response. Raw text: {Text}\nError: {Error}", text, ex.Message);
                return new AiEligibilityResult
                {
                    IsEligibleStartup = false,
                    EligibilityReason = "Không thể phân tích kết quả AI hợp lệ. Vui lòng thử lại với thông tin dự án đầy đủ hơn."
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
