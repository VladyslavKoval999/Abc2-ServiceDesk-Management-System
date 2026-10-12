using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServiceDesk.Maui.Services
{
    public class GeminiAiService
    {
        private const string ApiKey = "__________________"; //
        private readonly HttpClient _httpClient;

        public GeminiAiService()
        {
            _httpClient = new HttpClient(GetInsecureHandler())
            {
                Timeout = TimeSpan.FromSeconds(60)
            };

            _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        private HttpMessageHandler GetInsecureHandler()
        {
#if ANDROID
            var handler = new Xamarin.Android.Net.AndroidMessageHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            return handler;
#else
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            return handler;
#endif
        }

        public async Task<(string Message, bool IsError)> AskAiAsync(string userPrompt, bool isContextAnalysis = false)
        {
            bool isPt = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower() == "pt";

            if (string.IsNullOrWhiteSpace(ApiKey))
                return (isPt
                    ? "Erro: Por favor, configure a sua chave API Gemini nas definições."
                    : "Error: Please set your Gemini API key in the settings.", true);

            string systemPrompt = isPt
                ? "Você é o assistente técnico do Abc2 ServiceDesk. Pode ajudar com TI, eletrodomésticos, telemóveis, instrumentos (como guitarras) e mais. Responda diretamente, sem saudações (como 'Olá'). Não use formatação Markdown (como ** ou *). Forneça apenas texto simples."
                : "You are the helpful technical assistant for Abc2 ServiceDesk. You troubleshoot IT issues, home appliances, phones, instruments like guitars, and more. Respond directly without greetings (like 'Hello'). Do NOT use Markdown formatting (like ** or *). Provide plain text only.";

            if (isContextAnalysis)
            {
                systemPrompt += isPt
                    ? " O usuário fornecerá a descrição de um problema. Analise e forneça uma lista curta de possíveis causas e soluções."
                    : " The user will provide a problem description. Analyze it and provide a short list of possible causes and solutions.";
            }

            string combinedPrompt = $"{systemPrompt}\n\nUser Request: {userPrompt}";

            var requestPayload = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = combinedPrompt } }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");
            string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.7-flash:generateContent";

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseJson);
                    var text = doc.RootElement
                                  .GetProperty("candidates")[0]
                                  .GetProperty("content")
                                  .GetProperty("parts")[0]
                                  .GetProperty("text")
                                  .GetString();

                    return (text?.Trim() ?? (isPt ? "Nenhuma resposta gerada." : "No response generated."), false);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    return (isPt
                        ? "Os servidores de IA estão sobrecarregados (Erro 503). Por favor, aguarde e tente novamente."
                        : "AI servers are currently overloaded (Error 503). Please wait a moment and try again.", true);

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    return (isPt
                        ? "Pedido inválido. Por favor, reformule a sua questão."
                        : "Invalid request. Please rephrase your question.", true);

                return (isPt
                    ? $"Ocorreu um erro no servidor ({response.StatusCode}). Tente novamente mais tarde."
                    : $"A server error occurred ({response.StatusCode}). Please try again later.", true);
            }
            catch (TaskCanceledException)
            {
                return (isPt
                    ? "O tempo limite foi esgotado. Verifique a sua ligação à Internet."
                    : "Request timed out. Please check your internet connection.", true);
            }
            catch (Exception)
            {
                return (isPt
                    ? "Erro de ligação. Verifique o acesso à rede."
                    : "Connection error. Please check network access.", true);
            }
        }
    }
}