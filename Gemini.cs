using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;

static class Gemini
{
    public static async Task<string> InvokePromptAsync(string prompt, string apiKey)
    {
        var builder = Kernel.CreateBuilder();
        builder.Services.AddGoogleAIGeminiChatCompletion(
            serviceId: "gemini",
            // modelId: "gemini-2.5-flash-lite",
            modelId: "gemini-2.5-flash",
            // modelId: "gemini-2.5-pro-exp-03-25",
            apiKey: apiKey);
        var kernel = builder.Build();
        var settings = new GeminiPromptExecutionSettings
        {
            Temperature = 0,
            // FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            // ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions
        };

        // トリミング対応のため以下は使用せず GetRequiredService に置き換え
        // var result = await kernel.InvokePromptAsync(prompt, new(settings)).ConfigureAwait(false);
        // return result.ToString();

        var chat = kernel.GetRequiredService<IChatCompletionService>("gemini");
        var response = await chat.GetChatMessageContentAsync(
            prompt,
            settings,
            kernel
        ).ConfigureAwait(false);
        return response?.Content ?? string.Empty;
    } 
}