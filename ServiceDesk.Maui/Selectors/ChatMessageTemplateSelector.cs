using Microsoft.Maui.Controls;
using ServiceDesk.Shared.Entities;

namespace ServiceDesk.Maui.Selectors
{
    public class ChatMessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UserTemplate { get; set; } = null!;

        public DataTemplate AiTemplate { get; set; } = null!;


        protected override DataTemplate OnSelectTemplate(
            object item,
            BindableObject container)
        {
            if (item is ChatMessage message)
            {
                return message.IsUser
                    ? UserTemplate
                    : AiTemplate;
            }

            return AiTemplate;
        }
    }
}