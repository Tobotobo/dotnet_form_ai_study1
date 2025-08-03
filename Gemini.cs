using Microsoft.SemanticKernel;
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
        var result = await kernel.InvokePromptAsync(prompt, new(settings)).ConfigureAwait(false);
        return result.ToString();
    } 
}