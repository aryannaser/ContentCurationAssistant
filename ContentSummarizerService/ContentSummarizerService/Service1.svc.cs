using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web.ModelBinding;
using System.Xml.Linq;

using OpenAI.Chat;

namespace ContentSummarizerService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        public string SummarizeText(string content)
        {
            // Placeholder logic: return the first 100 characters followed by an ellipsis
            if (string.IsNullOrWhiteSpace(content))
            {
                return "Input content is empty.";
            }

            string apiKey = Environment.GetEnvironmentVariable("OpenAiKey");

            if (string.IsNullOrEmpty(apiKey))
            {
                return "API key is missing or not properly set.";
            }
            try
            {
                ChatClient client = new ChatClient("gpt-4o-mini", apiKey);

                List<ChatMessage> messages = new List<ChatMessage>
                {
                    new UserChatMessage("Please summarize the following text:"),
                    new SystemChatMessage(content)
                };

                ChatCompletion completion = client.CompleteChat(messages);

                messages.Add(new AssistantChatMessage(completion));

                // Extract the assistant's response (summary)
                var assistantMessage = messages.OfType<AssistantChatMessage>().LastOrDefault();
                if (assistantMessage != null && assistantMessage.Content.Count > 0)
                {
                    // Return the summarized text, cleaned of unnecessary spaces and line breaks
                    string summary = assistantMessage.Content[0].Text;

                    // Normalize whitespace, remove line breaks, and return a single paragraph
                    return summary.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Trim();
                }

                return "Failed to generate a summary.";

            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }
    }
}
