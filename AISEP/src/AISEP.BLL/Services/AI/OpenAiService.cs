using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using System.Globalization;
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

        public async Task<AiAnalysisResult> AnalyzeProjectAsync(Project project, decimal baseScore)
        {
            var prompt = BuildAnalysisPrompt(project, baseScore, "startup");

            _logger.LogInformation("Calling OpenAI startup analysis: model={Model}, baseScore={BaseScore}",
                _settings.Model, baseScore);

            var responseJson = await CallOpenAiAsync(prompt, [], BuildAnalysisJsonSchemaFormat());
            return ParseResponse(responseJson);
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

        public async Task<AiAnalysisResult> AnalyzeProjectForInvestorAsync(Project project, decimal baseScore)
        {
            var prompt = BuildAnalysisPrompt(project, baseScore, "investor");

            _logger.LogInformation("Calling OpenAI investor analysis: model={Model}, baseScore={BaseScore}",
                _settings.Model, baseScore);

            var responseJson = await CallOpenAiAsync(prompt, [], BuildAnalysisJsonSchemaFormat());
            return ParseResponse(responseJson);
        }

        private string BuildAnalysisPrompt(Project project, decimal baseScore, string audience)
        {
            var baseScoreText = baseScore.ToString("0.##", CultureInfo.InvariantCulture);
            var audienceDescription = audience == "investor"
                ? "nhà đầu tư đang đánh giá cơ hội đầu tư"
                : "startup đang tự đánh giá và cải thiện dự án";
            var stageOptionId = project.StageOptionId?.ToString() ?? "N/A";

            return $$"""
                Bạn là chuyên gia thẩm định đầu tư cấp cao.
                Hệ thống toán học đã chấm dự án này đạt BaseScore là {{baseScoreText}}/100 điểm.
                Hãy đọc kỹ văn bản mô tả của dự án.
                Nếu cách trình bày sắc bén, logic và khả thi, hãy cấp điểm thưởng từ +1 đến +10.
                Nếu mô tả ngây ngô, thiếu thực tế hoặc viển vông, hãy trừ điểm từ -1 đến -10.
                Nếu văn bản trung tính, đủ rõ nhưng không nổi bật, có thể trả về 0.

                RÀNG BUỘC NGÔN NGỮ QUAN TRỌNG:
                Toàn bộ nội dung phân tích, nhận xét, đánh giá BẮT BUỘC phải được viết bằng Tiếng Việt chuyên ngành kinh doanh/startup một cách tự nhiên và trôi chảy.

                Đối tượng sử dụng kết quả: {{audienceDescription}}.

                Chỉ đánh giá chất lượng phần mô tả định tính. Không tự tính điểm cuối cùng.

                --- DỮ LIỆU DỰ ÁN ---
                Tên dự án: {{project.ProjectName}}
                Mô tả ngắn: {{project.ShortDescription ?? "N/A"}}
                StageOptionId: {{stageOptionId}}
                Bài toán: {{project.ProblemStatement ?? "N/A"}}
                Giải pháp: {{project.SolutionDescription ?? "N/A"}}
                Khách hàng mục tiêu: {{project.TargetCustomers ?? "N/A"}}
                Giá trị khác biệt: {{project.UniqueValueProposition ?? "N/A"}}
                Mô hình kinh doanh: {{project.BusinessModel ?? "N/A"}}
                Đối thủ cạnh tranh: {{project.Competitors ?? "N/A"}}
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
                        AIAdjustmentScore = new
                        {
                            type = "integer",
                            minimum = -10,
                            maximum = 10,
                            description = "Diem cong hoac tru tu -10 den 10."
                        },
                        Reasoning = new
                        {
                            type = "string",
                            description = "Giai thich ngan gon bang tieng Viet ve ly do cong/tru diem."
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
                        "AIAdjustmentScore",
                        "Reasoning",
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

                result.AIAdjustmentScore = Math.Clamp(result.AIAdjustmentScore, -10, 10);
                result.Reasoning ??= string.Empty;
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
