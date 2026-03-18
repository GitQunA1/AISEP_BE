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
                Example A (Weak project):
                Input signals: team undefined, market size missing, pitch deck irrelevant image.
                Expected style:
                - Team.score around 0.1~0.3, evidence empty, missingData includes founder experience.
                - Opportunity.score around 0.1~0.3, missing TAM/SAM/SOM.
                - ChaosScore high (80-100).

                Example B (Good MVP):
                Input signals: clear team roles, demo + pitch deck, defined ICP, early revenue.
                Expected style:
                - Team.score around 1.1~1.4 with evidence from team slide.
                - Product.score around 1.1~1.5 with evidence from demo.
                - Marketing.score around 1.0~1.3 with traction evidence.
                - ChaosScore medium (30-55).

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
