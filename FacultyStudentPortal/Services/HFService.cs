
using FacultyStudentPortal.Services;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class HFService : IHFService
{
    private readonly HttpClient _http;

    public HFService(HttpClient httpClient, string hfToken)
    {
        // LOG: check that the token is being passed in
        Console.WriteLine($"[HFService] Constructor received HF token: '{hfToken}'");

        _http = httpClient;

        // Build an HttpClient pointed at the HF inference endpoint:
        var baseUri = "https://api-inference.huggingface.co/models/mistralai/Mistral-7B-Instruct-v0.3";
        //var baseUri = "https://api-inference.huggingface.co/models/HuggingFaceH4/zephyr-7b-beta";
        Console.WriteLine($"[HFService] Setting BaseAddress to: {baseUri}");

        //_http = new HttpClient
        //{
        //    BaseAddress = new Uri(baseUri),
        //    Timeout = TimeSpan.FromSeconds(60)
        //};
        //_http.DefaultRequestHeaders.Authorization
        //    = new AuthenticationHeaderValue("Bearer", hfToken);

        _http = httpClient;
        _http.BaseAddress = new Uri(baseUri);
        _http.Timeout = TimeSpan.FromSeconds(60);
        _http.DefaultRequestHeaders.Authorization
            = new AuthenticationHeaderValue("Bearer", hfToken);

    }

    public async Task<string> GenerateFeedbackFromContentAsync(string prompt)
    {

        // Package payload according to HF Inference API:
        var payload = new
        {
            inputs = prompt,
            parameters = new
            {
                max_new_tokens = 150,
                temperature = 0.7,
                top_p = 0.9
            }
        };

        string jsonPayload = JsonSerializer.Serialize(payload);
        Console.WriteLine($"[HFService] JSON payload size: {jsonPayload.Length} characters");

        using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        Console.WriteLine($"[HFService] Sending POST to '{_http.BaseAddress}'");
        using var resp = await _http.PostAsync(string.Empty, content);

        Console.WriteLine($"[HFService] Received HTTP {(int)resp.StatusCode} from HF API");

        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"[HFService] Error response body: {err}");
            throw new InvalidOperationException($"HF Inference failed: {(int)resp.StatusCode}: {err}");
        }

        var raw = await resp.Content.ReadAsStringAsync();
        Console.WriteLine($"[HFService] Raw response size: {raw.Length} characters");
        Console.WriteLine(raw);

        using var doc = JsonDocument.Parse(raw);
        var arr = doc.RootElement;
        if (arr.GetArrayLength() == 0)
        {
            Console.WriteLine("[HFService] Response array was empty");
            return "No feedback generated.";
        }


        string full = "";
        if (arr[0].TryGetProperty("summary_text", out var summaryProp))
        {
            full = summaryProp.GetString();
        }
        else if (arr[0].TryGetProperty("generated_text", out var genProp))
        {
            full = genProp.GetString();
        }
        else
        {
            Console.WriteLine("[HFService] Neither 'summary_text' nor 'generated_text' found in response.");
            return "AI model returned no usable feedback.";
        }

        // Strip off everything before “Feedback:” if present
        var idx = full.IndexOf("Feedback:", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            var result = full[(idx + "Feedback:".Length)..].Trim();
            Console.WriteLine($"[HFService] Returning text after 'Feedback:' (length {result.Length})");
            return result;
        }

        Console.WriteLine("[HFService] 'Feedback:' marker not found, returning full text");
        return full.Trim();
    }
}
