using LearnHuggingFaceLangChainApp1.Data;
using LearnHuggingFaceLangChainApp1.LLM;
using LearnHuggingFaceLangChainApp1.Repositories;
using LearnHuggingFaceLangChainApp1.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var conString = configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found.");
var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql(conString)
    .Options;

using var db = new ApplicationDbContext(optionsBuilder);
var repo = new ChatHistoryRepository(db);

var modelPath = "/Users/hieple/RiderProjects/LearnHuggingFaceLangChain/Models/models/phi3-mini-4k-instruct-onnx/cpu_and_mobile/cpu-int4-rtn-block-32";
using var llm = new OnnxModelClient(modelPath);

var chat = new ChatService(repo, llm);

var sessionId = Guid.NewGuid().ToString();
Console.WriteLine("Type your question (empty = exit).");
while (true)
{
    Console.Write("> ");
    var text = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(text)) break;

    var reply = await chat.SendAsync(sessionId, text);
    Console.WriteLine($"\nAssistant: {reply}\n");
}
            