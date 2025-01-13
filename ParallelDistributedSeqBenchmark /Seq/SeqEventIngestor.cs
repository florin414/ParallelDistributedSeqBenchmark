using System.Text;
using System.Text.Json;

namespace ParallelDistributedSeqBenchmark.Seq
{
    public class SeqEventIngestor
    {
        private readonly string _seqUrl;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public SeqEventIngestor(string seqUrl, string apiKey)
        {
            _seqUrl = seqUrl ?? throw new ArgumentNullException(nameof(seqUrl));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _httpClient = new HttpClient();
        }
        
        
        /// <summary>
        /// Creates an instance of the class from a JSON configuration file.
        /// </summary>
        /// <param name="filePath">Path to the JSON configuration file.</param>
        /// <returns>An instance of <see cref="SeqEventIngestor"/>.</returns>
        public static SeqEventIngestor FromJsonFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The configuration file was not found: {filePath}");

            var jsonContent = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<SeqConfig>(jsonContent);

            if (config == null || string.IsNullOrEmpty(config.SeqUrl) || string.IsNullOrEmpty(config.ApiKey))
                throw new InvalidOperationException("Invalid JSON configuration.");

            return new SeqEventIngestor(config.SeqUrl, config.ApiKey);
        }

        /// <summary>
        /// Sends an event to Seq.
        /// </summary>
        /// <param name="level">The event level (e.g., Information, Warning, Error).</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="seqEvent">The event object containing additional properties.</param>
        /// <returns>A task that completes after the event is sent.</returns>
        public async Task SendEventAsync(string level, string messageTemplate, SeqEvent seqEvent)
        {
            var jsonPayload = BuildJsonPayload(level, messageTemplate, seqEvent);

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_seqUrl}/api/events/raw")
            {
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Seq-ApiKey", _apiKey);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}. Details: {errorDetails}");
            }
        }

        /// <summary>
        /// Builds the JSON payload for sending to Seq.
        /// </summary>
        /// <param name="level">The event level.</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="seqEvent">The event object.</param>
        /// <returns>The JSON payload as a string.</returns>
        private string BuildJsonPayload(string level, string messageTemplate, SeqEvent seqEvent)
        {
            return $@"
            {{
                ""Events"": [
                    {{
                        ""Timestamp"": ""{DateTime.UtcNow:O}"",
                        ""Level"": ""{level}"",
                        ""MessageTemplate"": ""{messageTemplate}"",
                        ""Properties"": {{
                            ""Tag"": ""{seqEvent.Tag}"",
                            ""Count"": {seqEvent.Count},
                            ""Method"": ""{seqEvent.Method}"",
                            ""MaxTimeUs"": ""{seqEvent.MaxTimeUs}"",
                            ""Parameters"": {JsonSerializer.Serialize(seqEvent.Parameters)},
                            ""AllocatedMemoryBytes"": {seqEvent.AllocatedMemoryBytes}
                        }}
                    }}
                ]
            }}";
        }

        /// <summary>
        /// Reads events from a JSON file and sends them to Seq.
        /// </summary>
        /// <param name="filePath">Path to the JSON file containing events.</param>
        /// <returns>A task that completes after all events are sent.</returns>
        public async Task SendEventsFromFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The file was not found: {filePath}");

            var jsonContent = File.ReadAllText(filePath);
            var events = JsonSerializer.Deserialize<List<SeqEvent>>(jsonContent);

            if (events == null || events.Count == 0)
                throw new InvalidOperationException("No events found in the JSON file.");

            foreach (var seqEvent in events)
            {
                await SendEventAsync("Information", "Benchmark completed", seqEvent);
            }
        }

        /// <summary>
        /// Represents the configuration for Seq (URL and API key).
        /// </summary>
        private class SeqConfig
        {
            public string SeqUrl { get; set; }
            public string ApiKey { get; set; }
        }
    }
}
