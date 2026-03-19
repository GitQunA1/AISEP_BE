using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AISEP.DAL.Entities;
using AISEP.BLL.Settings;
using Microsoft.Extensions.Options;

namespace AISEP.BLL.Services.AI
{
    public class GeminiAiService : IGeminiAiService
    {
        private readonly GeminiSettings          _settings;
        private readonly HttpClient              _httpClient;
        private readonly ILogger<GeminiAiService> _logger;

        public GeminiAiService(IOptions<GeminiSettings> settings, HttpClient httpClient, ILogger<GeminiAiService> logger)
        {
            _settings   = settings.Value;
            _httpClient = httpClient;
            _logger     = logger;
        }

        public async Task<GeminiAnalysisResult> AnalyzeProjectAsync(Project project, IEnumerable<Document> documents)
        {
            var docList     = documents.ToList();
            var prompt      = BuildPrompt(project, docList);
            var inlineParts = await BuildInlinePartsAsync(docList);

            _logger.LogInformation("Calling Gemini: model={Model}, inlineParts={Count}",
                _settings.Model, inlineParts.Count);

            try
            {
                var responseJson = await CallGeminiAsync(prompt, inlineParts);
                return ParseResponse(responseJson);
            }
            catch (HttpRequestException ex) when (inlineParts.Count > 0)
            {
                // Fallback: retry text-only if inline_data caused the error
                _logger.LogWarning("Inline_data call failed ({Msg}). Retrying text-only...", ex.Message);
                var responseJson = await CallGeminiAsync(prompt, []);
                return ParseResponse(responseJson);
            }
        }

        public async Task<GeminiAnalysisResult> AnalyzeProjectForInvestorAsync(Project project, IEnumerable<Document> documents)
        {
            var docList = documents.ToList();
            var prompt = BuildInvestorPrompt(project, docList);
            var inlineParts = await BuildInlinePartsAsync(docList);

            _logger.LogInformation("Calling Gemini (Investor mode): model={Model}, inlineParts={Count}",
                _settings.Model, inlineParts.Count);

            try
            {
                var responseJson = await CallGeminiAsync(prompt, inlineParts);
                return ParseResponse(responseJson);
            }
            catch (HttpRequestException ex) when (inlineParts.Count > 0)
            {
                _logger.LogWarning("Inline_data investor call failed ({Msg}). Retrying text-only...", ex.Message);
                var responseJson = await CallGeminiAsync(prompt, []);
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
                : "None";
            var skippedSummary = skipped.Count > 0
                ? $" | Skipped (unsupported format): {string.Join(", ", skipped.Select(d => d.FileName))}"
                : string.Empty;

            return $$"""
                You are an expert startup evaluator using the Bill Payne Scorecard Valuation Method.
                Analyze the startup project below and score each component.
                All attached files are documents belonging to this project. Read them to improve scoring evidence.
                Return ONLY a valid JSON object — no markdown, no extra text.

                --- PROJECT DATA ---
                Name: {{project.ProjectName}}
                Short Description: {{project.ShortDescription ?? "N/A"}}
                Development Stage: {{project.DevelopmentStage?.ToString() ?? "N/A"}}
                Problem Statement: {{project.ProblemStatement ?? "N/A"}}
                Solution: {{project.SolutionDescription ?? "N/A"}}
                Target Customers: {{project.TargetCustomers ?? "N/A"}}
                Unique Value Proposition: {{project.UniqueValueProposition ?? "N/A"}}
                Market Size: {{(project.MarketSize.HasValue ? project.MarketSize.Value.ToString("N0") + " USD" : "N/A")}}
                Business Model: {{project.BusinessModel ?? "N/A"}}
                Revenue: {{(project.Revenue.HasValue ? project.Revenue.Value.ToString("N0") + " USD" : "N/A")}}
                Competitors: {{project.Competitors ?? "N/A"}}
                Team Members: {{project.TeamMembers ?? "N/A"}}
                Key Skills: {{project.KeySkills ?? "N/A"}}
                Team Experience: {{project.TeamExperience ?? "N/A"}}
                Uploaded Documents ({{docCount}} attached for reading){{skippedSummary}}:
                {{docSummary}}

                --- SCORING INSTRUCTIONS ---
                Score each component as a multiplier relative to the market average:
                  1.0 = average | 1.5 = 50% above average | 0.7 = 30% below average

                Components (Bill Payne weights):
                  1. Team              (30%): TeamMembers, KeySkills, TeamExperience
                  2. Opportunity       (25%): TargetCustomers, MarketSize
                  3. Product/Tech      (15%): SolutionDescription, DevelopmentStage
                  4. Competition       (10%): Competitors, UniqueValueProposition
                  5. Marketing/Sales   (10%): BusinessModel, Revenue
                  6. Investment Need    (5%): clarity of funding requirements
                  7. Other              (5%): document count, overall pitch quality

                --- SCORING RUBRIC (strict) ---
                For each component, choose score range based on evidence quality:
                - 0.0 - 0.3: Missing or irrelevant information (e.g. placeholder text like "string", no usable document proof).
                - 0.4 - 0.7: Basic information exists but weak evidence / unclear execution.
                - 0.8 - 1.2: Market-average quality with reasonable evidence.
                - 1.3 - 1.6: Strong quality with clear, specific, verifiable evidence.
                - 1.7 - 2.0: Exceptional quality with outstanding evidence and traction.
                If confidence is low, prefer conservative score.

                --- FEW-SHOT EXAMPLES (style reference) ---
                Golden Sample A (Weak / Low-quality submission):
                Input signals:
                - TeamMembers = "string", KeySkills = "string", TeamExperience = "string"
                - MarketSize = 0, BusinessModel = "string", Competitors = "string"
                - Attached "PitchDeck" is unrelated image (signature/photo, not startup content)
                Target output style:
                {
                  "Team": {
                    "score": 0.1,
                    "evidence": [],
                    "missingData": ["Founder background", "Role split", "Domain experience"],
                    "confidence": 0.9,
                    "reason": "Team information is placeholder-only and not verifiable."
                  },
                  "Opportunity": {
                    "score": 0.1,
                    "evidence": [],
                    "missingData": ["TAM/SAM/SOM", "ICP detail", "Growth assumptions"],
                    "confidence": 0.9,
                    "reason": "No usable market evidence provided."
                  },
                  "Product": {
                    "score": 0.2,
                    "evidence": ["Development Stage = Idea"],
                    "missingData": ["Product architecture", "MVP proof", "Technical differentiation"],
                    "confidence": 0.8,
                    "reason": "Only early idea signal exists, no supporting product evidence."
                  },
                  "Competition": {
                    "score": 0.1,
                    "evidence": [],
                    "missingData": ["Direct competitors", "Positioning map", "UVP proof"],
                    "confidence": 0.9,
                    "reason": "Competitive analysis is missing."
                  },
                  "Marketing": {
                    "score": 0.1,
                    "evidence": [],
                    "missingData": ["Go-to-market plan", "Traction", "Conversion metrics"],
                    "confidence": 0.9,
                    "reason": "No real business model or traction data."
                  },
                  "Investment": {
                    "score": 0.1,
                    "evidence": [],
                    "missingData": ["Fundraising ask", "Use-of-funds", "Milestones"],
                    "confidence": 0.9,
                    "reason": "Investment ask section is missing."
                  },
                  "Other": {
                    "score": 0.0,
                    "evidence": ["Attached document is irrelevant to project evaluation."],
                    "missingData": ["Valid pitch deck", "Supporting legal/financial documents"],
                    "confidence": 0.95,
                    "reason": "Attached file does not support evaluation."
                  },
                  "ChaosScore": 95,
                  "Summary": "Thiếu dữ liệu nghiêm trọng và tài liệu không liên quan.",
                  "Strengths": ["Có tên ý tưởng rõ ràng."],
                  "Weaknesses": ["Thiếu dữ liệu đội ngũ", "Thiếu dữ liệu thị trường", "Tài liệu đính kèm không hợp lệ"],
                  "Recommendations": ["Bổ sung team profile", "Nộp pitch deck thực tế", "Mô tả GTM và traction"]
                }

                Golden Sample B (Good MVP / Investment-ready soon):
                Input signals:
                - Team has clear founder roles and 5+ years relevant experience
                - Market slide includes TAM/SAM/SOM and growth trend
                - Product demo exists + MVP users + measurable traction
                - Business model + early paid users + clear competitor differentiation
                Target output style:
                {
                  "Team": {
                    "score": 1.3,
                    "evidence": ["Founder CEO: agritech operations 7 years", "CTO built similar IoT stack"],
                    "missingData": [],
                    "confidence": 0.85,
                    "reason": "Team is complete, experienced, and aligned with domain."
                  },
                  "Opportunity": {
                    "score": 1.2,
                    "evidence": ["TAM/SAM/SOM provided", "Market growth rate stated"],
                    "missingData": ["Independent benchmark source link"],
                    "confidence": 0.8,
                    "reason": "Opportunity is large with reasonable quantification."
                  },
                  "Product": {
                    "score": 1.3,
                    "evidence": ["MVP demo video", "Pilot customer feedback"],
                    "missingData": [],
                    "confidence": 0.82,
                    "reason": "Product has concrete validation signals."
                  },
                  "Competition": {
                    "score": 1.1,
                    "evidence": ["Competitor table", "UVP statement with pricing edge"],
                    "missingData": ["Win/loss data"],
                    "confidence": 0.76,
                    "reason": "Positioning is fairly clear but more proof is needed."
                  },
                  "Marketing": {
                    "score": 1.2,
                    "evidence": ["Go-to-market plan", "Early paid customers"],
                    "missingData": ["Channel CAC breakdown"],
                    "confidence": 0.78,
                    "reason": "Commercial logic is sound with initial traction."
                  },
                  "Investment": {
                    "score": 1.0,
                    "evidence": ["Ask and use-of-funds table"],
                    "missingData": ["Quarterly milestone sensitivity analysis"],
                    "confidence": 0.72,
                    "reason": "Funding ask is clear at baseline quality."
                  },
                  "Other": {
                    "score": 1.0,
                    "evidence": ["Pitch deck quality is coherent", "Customer testimonial screenshot"],
                    "missingData": [],
                    "confidence": 0.75,
                    "reason": "Supporting materials are coherent and useful."
                  },
                  "ChaosScore": 42,
                  "Summary": "Dự án MVP khá tốt, còn thiếu một số bằng chứng tài chính sâu.",
                  "Strengths": ["Đội ngũ phù hợp", "MVP có validation", "GTM rõ"],
                  "Weaknesses": ["Thiếu benchmark độc lập", "Thiếu CAC chi tiết"],
                  "Recommendations": ["Bổ sung unit economics", "Tăng bằng chứng cạnh tranh", "Chuẩn hóa roadmap vốn"]
                }

                Do NOT compute weighted total score. Backend will compute PotentialScore using:
                0.30*Team + 0.25*Opportunity + 0.15*Product + 0.10*Competition + 0.10*Marketing + 0.05*Investment + 0.05*Other,
                then multiply by 100.
                ChaosScore (0-100): risk/uncertainty level (0 = stable, 100 = very risky)
                Summary: brief analysis written in Vietnamese, mention which documents/slides influenced the scoring.
                Strengths: 3-5 key strengths (Vietnamese).
                Weaknesses: 3-5 key weaknesses/gaps (Vietnamese).
                Recommendations: 5-8 actionable recommendations in priority order (Vietnamese), focused on improving score.
                Final self-check before output:
                1) Every component must include at least one meaningful evidence or missingData item.
                2) High score (>1.2) requires specific evidence.
                3) Return valid JSON only.

                --- REQUIRED OUTPUT FORMAT (return ONLY this JSON) ---
                {
                  "Team": {
                    "score": <decimal 0.0-2.0>,
                    "evidence": ["<short evidence 1>", "<short evidence 2>"],
                    "missingData": ["<missing data 1>"],
                    "confidence": <decimal 0.0-1.0>,
                    "reason": "<why this score>"
                  },
                  "Opportunity": {
                    "score": <decimal 0.0-2.0>,
                    "evidence": [],
                    "missingData": [],
                    "confidence": <decimal 0.0-1.0>,
                    "reason": "<why this score>"
                  },
                  "Product": {
                    "score": <decimal 0.0-2.0>,
                    "evidence": [],
                    "missingData": [],
                    "confidence": <decimal 0.0-1.0>,
                    "reason": "<why this score>"
                  },
                  "Competition": {
                    "score": <decimal 0.0-2.0>,
                    "evidence": [],
                    "missingData": [],
                    "confidence": <decimal 0.0-1.0>,
                    "reason": "<why this score>"
                  },
                  "Marketing": {
                    "score": <decimal 0.0-2.0>,
                    "evidence": [],
                    "missingData": [],
                    "confidence": <decimal 0.0-1.0>,
                    "reason": "<why this score>"
                  },
                  "Investment": {
                    "score": <decimal 0.0-2.0>,
                    "evidence": [],
                    "missingData": [],
                    "confidence": <decimal 0.0-1.0>,
                    "reason": "<why this score>"
                  },
                  "Other": {
                    "score": <decimal 0.0-2.0>,
                    "evidence": [],
                    "missingData": [],
                    "confidence": <decimal 0.0-1.0>,
                    "reason": "<why this score>"
                  },
                  "ChaosScore":        <integer 0-100>,
                  "Summary":           "<string in Vietnamese>",
                  "Strengths":         ["<strength 1>", "<strength 2>"],
                  "Weaknesses":        ["<weakness 1>", "<weakness 2>"],
                  "Recommendations":   ["<action 1>", "<action 2>"]
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
                : "None";
            var skippedSummary = skipped.Count > 0
                ? $" | Skipped (unsupported format): {string.Join(", ", skipped.Select(d => d.FileName))}"
                : string.Empty;

            return $$"""
                You are an investor-side startup evaluator using the Bill Payne Scorecard Valuation Method.
                Analyze the project and attached project documents from an INVESTOR DECISION perspective.
                Return ONLY a valid JSON object — no markdown, no extra text.

                --- PROJECT DATA ---
                Name: {{project.ProjectName}}
                Short Description: {{project.ShortDescription ?? "N/A"}}
                Development Stage: {{project.DevelopmentStage?.ToString() ?? "N/A"}}
                Problem Statement: {{project.ProblemStatement ?? "N/A"}}
                Solution: {{project.SolutionDescription ?? "N/A"}}
                Target Customers: {{project.TargetCustomers ?? "N/A"}}
                Unique Value Proposition: {{project.UniqueValueProposition ?? "N/A"}}
                Market Size: {{(project.MarketSize.HasValue ? project.MarketSize.Value.ToString("N0") + " USD" : "N/A")}}
                Business Model: {{project.BusinessModel ?? "N/A"}}
                Revenue: {{(project.Revenue.HasValue ? project.Revenue.Value.ToString("N0") + " USD" : "N/A")}}
                Competitors: {{project.Competitors ?? "N/A"}}
                Team Members: {{project.TeamMembers ?? "N/A"}}
                Key Skills: {{project.KeySkills ?? "N/A"}}
                Team Experience: {{project.TeamExperience ?? "N/A"}}
                Uploaded Documents ({{docCount}} attached for reading){{skippedSummary}}:
                {{docSummary}}

                --- SCORING INSTRUCTIONS ---
                Use Bill Payne component multipliers (relative to market average):
                1. Team (30%), 2. Opportunity (25%), 3. Product/Tech (15%),
                4. Competition (10%), 5. Marketing/Sales (10%),
                6. Investment Need (5%), 7. Other (5%).

                Multiplier guide:
                - 1.0 = market average
                - 1.5 = 50% above average
                - 0.7 = 30% below average
                - Range must be 0.0 to 2.0

                --- SCORING RUBRIC (strict) ---
                For each component, choose score range based on evidence quality:
                - 0.0 - 0.3: Missing or irrelevant information (placeholder text, no usable document proof).
                - 0.4 - 0.7: Basic info exists but weak evidence / unclear execution.
                - 0.8 - 1.2: Market-average quality with reasonable evidence.
                - 1.3 - 1.6: Strong quality with clear, specific, verifiable evidence.
                - 1.7 - 2.0: Exceptional quality with outstanding evidence and traction.
                If confidence is low, prefer conservative scoring.

                --- GOLDEN SAMPLES (style reference) ---
                Golden Sample A (Pass / high risk):
                Input signals:
                - Team data is placeholder, market size = 0, business model unclear.
                - Uploaded pitch deck is irrelevant image/document.
                Expected style:
                - Team/Opportunity/Competition/Marketing/Investment near 0.1~0.3.
                - Other near 0.0 if docs are irrelevant.
                - ChaosScore very high (85-100).
                - InvestmentVerdict = "Pass".
                - RiskFlags and DealBreakers must be explicit.

                Golden Sample B (Watchlist / promising MVP):
                Input signals:
                - Founder team clear, MVP demo exists, early paid users, TAM/SAM/SOM provided.
                - Competitive positioning present but still lacks deep financial proof.
                Expected style:
                - Team/Product/Marketing around 1.0~1.4.
                - Opportunity around 1.0~1.3 with evidence.
                - ChaosScore medium (35-60).
                - InvestmentVerdict = "Watchlist".
                - DueDiligenceQuestions focus on unit economics and validation depth.

                Investor focus:
                - Emphasize investability, downside risk, execution risk, and data credibility.
                - High score (>1.2) MUST include concrete evidence from project/docs.
                - If evidence is weak, score conservatively and add risk flags.

                Do NOT compute weighted total score. Backend computes PotentialScore.
                Final self-check before output:
                1) Every component must include at least one meaningful evidence or missingData item.
                2) High score (>1.2) requires specific evidence.
                3) InvestmentVerdict must align with RiskFlags and DealBreakers.
                4) Return valid JSON only.

                --- REQUIRED OUTPUT FORMAT (JSON only) ---
                {
                  "Team": {"score": 0.0, "evidence": [], "missingData": [], "confidence": 0.0, "reason": ""},
                  "Opportunity": {"score": 0.0, "evidence": [], "missingData": [], "confidence": 0.0, "reason": ""},
                  "Product": {"score": 0.0, "evidence": [], "missingData": [], "confidence": 0.0, "reason": ""},
                  "Competition": {"score": 0.0, "evidence": [], "missingData": [], "confidence": 0.0, "reason": ""},
                  "Marketing": {"score": 0.0, "evidence": [], "missingData": [], "confidence": 0.0, "reason": ""},
                  "Investment": {"score": 0.0, "evidence": [], "missingData": [], "confidence": 0.0, "reason": ""},
                  "Other": {"score": 0.0, "evidence": [], "missingData": [], "confidence": 0.0, "reason": ""},
                  "ChaosScore": 0,
                  "Summary": "",
                  "Strengths": [],
                  "Weaknesses": [],
                  "Recommendations": [],
                  "InvestmentVerdict": "Strong|Watchlist|Pass",
                  "RiskFlags": [],
                  "DealBreakers": [],
                  "DueDiligenceQuestions": [],
                  "InvestorNextStep": ""
                }
                """;
        }

        private async Task<string> CallGeminiAsync(string prompt, List<object> inlineParts)
        {
            // Build parts: text prompt first, then each document as inline_data
            var parts = new List<object> { new { text = prompt } };
            parts.AddRange(inlineParts);

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = parts.ToArray() }
                },
                generationConfig = new
                {
                    temperature     = 0.05,
                    topK            = 40,
                    topP            = 0.95,
                    maxOutputTokens = 8192
                }
            };

            var url         = $"{_settings.BaseUrl}/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";
            var bodyJson    = JsonSerializer.Serialize(requestBody);
            var content     = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            _logger.LogDebug("Gemini URL: {Url}", $"{_settings.BaseUrl}/models/{_settings.Model}:generateContent?key=***");
            _logger.LogDebug("Gemini request body size: {Size} bytes", Encoding.UTF8.GetByteCount(bodyJson));

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                var statusCode = (int)response.StatusCode;

                var message = statusCode switch
                {
                    401 => "Gemini API key không hợp lệ hoặc chưa được cấp quyền.",
                    404 => $"Model '{_settings.Model}' không tồn tại hoặc không được hỗ trợ. Kiểm tra lại GeminiSettings.Model trong appsettings.json.",
                    429 => "Gemini API vượt quá quota. Free tier giới hạn số request/phút và token/ngày. Vui lòng chờ hoặc nâng cấp plan.",
                    500 => "Gemini API lỗi phía server. Thử lại sau.",
                    _   => $"Gemini API trả về lỗi {statusCode}."
                };

                throw new HttpRequestException($"{message}\nChi tiết: {errorBody}");
            }

            return await response.Content.ReadAsStringAsync();
        }

    
        private async Task<List<object>> BuildInlinePartsAsync(List<Document> documents)
        {
            var parts = new List<object>();

            foreach (var doc in documents)
            {
                var mimeType = GetMimeType(doc.FileName);
                if (mimeType is null) continue;

                try
                {
                    var bytes  = await _httpClient.GetByteArrayAsync(doc.FileUrl);
                    var base64 = Convert.ToBase64String(bytes);
                    parts.Add(new
                    {
                        inline_data = new
                        {
                            mime_type = mimeType,
                            data      = base64
                        }
                    });
                }
                catch
                {
                    // Skip documents that fail to download
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
                ".heic" => "image/heic",
                ".heif" => "image/heif",
                ".gif"  => "image/gif",
                _       => null  
            };
        }

        private GeminiAnalysisResult ParseResponse(string responseJson)
        {
            var doc  = JsonDocument.Parse(responseJson);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "{}";

            // Strip markdown code blocks Gemini sometimes wraps around JSON
            text = Regex.Replace(text, @"```json\s*", "").Replace("```", "").Trim();

            _logger.LogDebug("Gemini raw text response: {Text}", text);

            // Handle truncated JSON: attempt to auto-close the object
            if (!text.TrimEnd().EndsWith('}'))
            {
                _logger.LogWarning("Gemini response appears truncated. Attempting auto-repair...");
                text = text.TrimEnd().TrimEnd(',') + "}";
            }

            try
            {
                return JsonSerializer.Deserialize<GeminiAnalysisResult>(text,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new GeminiAnalysisResult();
            }
            catch (JsonException ex)
            {
                _logger.LogError("Failed to parse Gemini response. Raw text: {Text}\nError: {Error}", text, ex.Message);
                return new GeminiAnalysisResult();
            }
        }
    }
}
