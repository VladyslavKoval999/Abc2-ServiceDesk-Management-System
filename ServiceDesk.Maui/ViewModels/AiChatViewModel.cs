using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using ServiceDesk.Maui.Services;
using ServiceDesk.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;

namespace ServiceDesk.Maui.ViewModels
{
    public partial class AiChatViewModel : ObservableObject
    {
        private readonly GeminiAiService _aiService;

        [ObservableProperty]
        public partial string UserInput { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        private static readonly List<ChatMessage> _chatHistory = new();
        public ObservableCollection<ChatMessage> Messages { get; } = new();

        public AiChatViewModel(GeminiAiService aiService)
        {
            _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
            bool isPt = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("pt", StringComparison.OrdinalIgnoreCase);

            if (_chatHistory.Count == 0)
            {
                _chatHistory.Add(
                    new ChatMessage
                    {
                        Text = isPt
                            ? "Olá! Bem-vindo ao Abc2 ServiceDesk. Sou o seu assistente de IA. Qual é o equipamento que precisa de diagnóstico hoje? Ajudo com TI, eletrodomésticos, instrumentos e muito mais."
                            : "Hello! Welcome to Abc2 ServiceDesk. I am your AI Assistant. What equipment needs diagnostics today? I can help with IT, home appliances, instruments, and more.",
                        IsUser = false,
                        IsError = false,
                        Timestamp = DateTime.Now
                    });
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Messages.Clear();
                foreach (var message in _chatHistory)
                {
                    Messages.Add(message);
                }
            });
        }

        [RelayCommand]
        public async Task SendMessageAsync()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(UserInput))
                return;

            string prompt = UserInput.Trim();
            UserInput = string.Empty;

            var userMessage = new ChatMessage
            {
                Text = prompt,
                IsUser = true,
                IsError = false,
                Timestamp = DateTime.Now
            };

            Messages.Add(userMessage);
            _chatHistory.Add(userMessage);

            IsBusy = true;

            try
            {
                var response = await _aiService.AskAiAsync(prompt, isContextAnalysis: false);

                string responseText = string.IsNullOrWhiteSpace(response.Message)
                        ? "I couldn't generate a response."
                        : response.Message;

                var aiMessage = new ChatMessage
                {
                    Text = responseText,
                    IsUser = false,
                    IsError = response.IsError,
                    Timestamp = DateTime.Now
                };

                Messages.Add(aiMessage);
                _chatHistory.Add(aiMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = new ChatMessage
                {
                    Text = "Sorry, an error occurred while communicating with the AI assistant.\n\n" + ex.Message,
                    IsUser = false,
                    IsError = true,
                    Timestamp = DateTime.Now
                };

                Messages.Add(errorMessage);
                _chatHistory.Add(errorMessage);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}