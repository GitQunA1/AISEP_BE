using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AISEP.Models.Entities;
using AISEP.Settings;
using Microsoft.Extensions.Options;

namespace AISEP.Services.AI
{
    public class GeminiAiService : IGeminiAiService
    {
        private readonly GeminiSettings _settings;
        private readonly HttpClient     _httpClient;

        public GeminiAiService(IOptions<GeminiSettings> settings, HttpClient httpClient)
        {
            _settings   = settings.Value;
            _httpClient = httpClient;
        }

        public async Task<GeminiAnalysisResult> AnalyzeProjectAsync(Project project, IEnumerable<Document> documents)
        {
            var docList       = documents.ToList();
            var prompt        = BuildPrompt(project, docList);
            var inlineParts   = await BuildInlinePartsAsync(docList);
            var responseJson  = await CallGeminiAsync(prompt, inlineParts);
            return ParseResponse(responseJson);
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

                PotentialScore (0-100): weighted sum mapped to integer scale (100 = market average)
                ChaosScore (0-100): risk/uncertainty level (0 = stable, 100 = very risky)
                IsEligibleStartup: true if PotentialScore >= 60 and risks are manageable
                Summary: brief analysis written in Vietnamese

                --- REQUIRED OUTPUT FORMAT (return ONLY this JSON) ---
                {
                  "TeamScore":         <decimal 0.0-2.0>,
                  "OpportunityScore":  <decimal 0.0-2.0>,
                  "ProductScore":      <decimal 0.0-2.0>,
                  "CompetitionScore":  <decimal 0.0-2.0>,
                  "MarketingScore":    <decimal 0.0-2.0>,
                  "InvestmentScore":   <decimal 0.0-2.0>,
                  "OtherScore":        <decimal 0.0-2.0>,
                  "PotentialScore":    <integer 0-100>,
                  "ChaosScore":        <integer 0-100>,
                  "IsEligibleStartup": <true|false>,
                  "EligibilityReason": "<string>",
                  "Summary":           "<string in Vietnamese>"
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
                    temperature     = 0.2,
                    topK            = 40,
                    topP            = 0.95,
                    maxOutputTokens = 2048
                }
            };

            var url     = $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

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

        private static GeminiAnalysisResult ParseResponse(string responseJson)
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

            return JsonSerializer.Deserialize<GeminiAnalysisResult>(text,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new GeminiAnalysisResult();
        }
    }
}
