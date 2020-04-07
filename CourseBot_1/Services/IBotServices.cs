using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;

namespace Microsoft.BotBuilderSamples
{
    public interface IBotServices
    {
        LuisRecognizer Dispatch { get; }
        QnAMaker SampleQnA { get; }
    }
}