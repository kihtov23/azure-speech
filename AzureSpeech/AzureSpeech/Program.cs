using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Configuration;

IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .Build();

SynthesisToSpeakerAsync(config).GetAwaiter().GetResult();

static async Task SynthesisToSpeakerAsync(IConfigurationRoot configurationRoot)
{
    var azureSpeechKey = configurationRoot.GetSection("AzureSpeechKey").Value;
    var azureSpeechRegion = configurationRoot.GetSection("AzureSpeechRegion").Value;

    var config = SpeechConfig.FromSubscription(azureSpeechKey, azureSpeechRegion);
    config.SpeechSynthesisLanguage = configurationRoot.GetSection("SpeechSynthesisLanguage").Value;
    config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio48Khz192KBitRateMonoMp3);
    config.SetProperty(PropertyId.SpeechServiceResponse_RequestSentenceBoundary, "true");

    using (var synthesizer = new SpeechSynthesizer(config, null))
    {
        Console.WriteLine("Enter number of chunks:");
        var chunksNumber = int.Parse(Console.ReadLine());

        Console.WriteLine("Enter book name:");
        var bookName = Console.ReadLine();

        for (int i = 1; i < chunksNumber + 1; i++)
        {
            string chunkName = $"{bookName}_{i}";
            string text = File.ReadAllText($"..\\..\\..\\input\\book_parts\\{chunkName}.txt");

            using (var result = await synthesizer.SpeakTextAsync(text))
            {
                using (var audioDataStream = AudioDataStream.FromResult(result))
                {
                    byte[] buffer = new byte[16000];
                    uint filledSize = 0;

                    using (var outputStream = new FileStream($"..\\..\\..\\output\\{chunkName}.mp3", FileMode.Create))
                    {
                        while ((filledSize = audioDataStream.ReadData(buffer)) > 0)
                        {
                            outputStream.Write(buffer, 0, (int)filledSize);
                            Console.WriteLine($"{filledSize} bytes received.");
                        }
                        outputStream.Close();
                    }
                    Console.WriteLine($"Chunk {chunkName} completed.");
                }

                switch (result.Reason)
                {
                    case ResultReason.SynthesizingAudioCompleted:
                        Console.WriteLine("SynthesizingAudioCompleted result");
                        break;
                    case ResultReason.Canceled:
                        var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                        Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                            Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
