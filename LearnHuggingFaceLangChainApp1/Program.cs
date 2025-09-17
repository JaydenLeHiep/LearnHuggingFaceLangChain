using Microsoft.ML.OnnxRuntimeGenAI;

using OgaHandle ogaHandle = new OgaHandle();

var modelPath = "/Users/hieple/RiderProjects/LearnHuggingFaceLangChain/Models/models/phi3-mini-4k-instruct-onnx/cpu_and_mobile/cpu-int4-rtn-block-32";
using var model = new Model(modelPath);
using var tokenizer = new Tokenizer(model);

using var gen = new GeneratorParams(model);
gen.SetSearchOption("max_length", 256);
gen.SetSearchOption("temperature", 0.7);
gen.SetSearchOption("top_p", 0.95);

var input = tokenizer.Encode("You are helpful.\nQ: What is API?\nA:");

using var generator = new Generator(model, gen);
// Newer migration path: append sequences on the generator
// (exact name per your version—common names seen were AppendTokenSequences / AppendSequences)
generator.AppendTokenSequences(input);        // <-- per migrate guide

using var stream = tokenizer.CreateStream();
while (!generator.IsDone())
{
    generator.GenerateNextToken();
    var tokens = generator.GetSequence(0);
    var lastId = tokens[^1];
    var piece = stream.Decode(lastId);
    Console.Write(piece);
}

// Or decode full sequence after done:
var full = tokenizer.Decode(generator.GetSequence(0));
Console.WriteLine(full);