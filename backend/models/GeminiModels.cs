using System.Collections.Generic;
using Newtonsoft.Json;

namespace ChatBot.Models
{
    public class GeminiRequest
    {
        [JsonProperty("contents")]
        public List<Content> Contents { get; set; } = new List<Content>();
        [JsonProperty("tools", NullValueHandling = NullValueHandling.Ignore)]
        public List<Tool>? Tools { get; set; }
    }
    public class Content
    {
        [JsonProperty("parts")]
        public List<Part> Parts { get; set; } = new List<Part>();
        
        [JsonProperty("role", NullValueHandling = NullValueHandling.Ignore)]
        public string? Role { get; set; }
    }
    public class Part
    {
        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string? Text { get; set; }

        [JsonProperty("functionResponse", NullValueHandling = NullValueHandling.Ignore)]
        public FunctionResponse? FunctionResponse { get; set; }

        [JsonProperty("functionCall", NullValueHandling = NullValueHandling.Ignore)]
        public FunctionCall? FunctionCall { get; set; }
    }
    public class Tool
    {
        [JsonProperty("function_declarations")]
        public List<FunctionDeclaration> FunctionDeclarations { get; set; } = new List<FunctionDeclaration>();
    }
    public class FunctionDeclaration
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";
        [JsonProperty("description")]
        public string Description { get; set; } = "";
        [JsonProperty("parameters")]
        public object? Parameters { get; set; }
    }
    public class FunctionResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";
        [JsonProperty("response")]
        public FunctionResponseBody Response { get; set; } = new FunctionResponseBody();
    }
    public class FunctionResponseBody
    {
        [JsonProperty("content")]
        public string Content { get; set; } = "";
    }

    public class FunctionCall
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("args", NullValueHandling = NullValueHandling.Ignore)]
        public object? Args { get; set; }
    }
}