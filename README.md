# Convert a book to audio for free.

1. Azure Speech limitations (scroll to - Text to speech quotas and limits per resource): https://learn.microsoft.com/en-us/azure/ai-services/speech-service/speech-services-quotas-and-limits
   - Maximum number of transactions per time period for prebuilt neural voices and custom neural voices. = 20 transactions per 60 seconds
   - Max audio length produced per request = 10 min
   - Max text message size per turn for websocket =	64 KB
2. Split your book (.txt format) into chunks with any file splitter. I used this one https://textfilesplitter.com/
   - I my case, .txt chunk should be less than 10 KB to fit into 10 minute audio limitation
3. Chunks should be named from 1 to N and named book_[N]. Place them in "./AzureSpeech/input/book_parts"
4. Insert your Azure Speech configuration to appsettings. (AzureSpeechKey and AzureSpeechRegion are required)
5. Change your language in appsettings (SpeechSynthesisLanguage setting). Supported list of languages - https://learn.microsoft.com/en-us/azure/ai-services/speech-service/language-support?tabs=tts
