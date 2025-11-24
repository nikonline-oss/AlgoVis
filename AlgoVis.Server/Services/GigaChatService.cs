using AlgoVis.Server.Interfaces;
using System.Text;
using System.Text.Json;

namespace AlgoVis.Server.Services
{
    public class GigaChatService : IGigaChatService
    {
        private readonly string _authKey = "MDE5YTRhNzEtYWEyYy03MjM4LWExMjUtNTZmNTIwNDg1MTRhOjAzZTU1NDNkLWQ1MGQtNDVhMy1iYWU5LWE3ODkxY2Y4MzVkNA==";
        private readonly string _scope = "GIGACHAT_API_PERS";

        private string _token;
        private DateTime _tokenExpiry;
        private bool _isRefreshing;
        private readonly List<TaskCompletionSource<string>> _refreshQueue = new();
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly HttpClient _httpClient;

        public GigaChatService()
        {
            // Создаем HttpClient с отключенной проверкой SSL (аналог verify=False в Python)
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        public async Task<string> GetTokenAsync(bool forceRefresh = false)
        {
            // Если токен есть и не истек, и не запрошено принудительное обновление
            if (!string.IsNullOrEmpty(_token) &&
                _tokenExpiry > DateTime.Now &&
                !forceRefresh)
            {
                return _token;
            }

            // Если уже идет обновление токена, добавляем запрос в очередь
            if (_isRefreshing)
            {
                return await WaitForTokenRefreshAsync();
            }

            _isRefreshing = true;

            try
            {
                Console.WriteLine("Requesting new GigaChat token...");

                var url = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";
                var rqUid = Guid.NewGuid().ToString();

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("RqUID", rqUid);
                request.Headers.Add("Authorization", $"Basic {_authKey}");

                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("scope", _scope)
            });
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenData = JsonSerializer.Deserialize<JsonElement>(responseContent);

                if (!tokenData.TryGetProperty("access_token", out var accessTokenProperty))
                {
                    throw new Exception("No access token in response");
                }

                _token = accessTokenProperty.GetString();
                // Токен действует 30 минут, устанавливаем expiry на 25 минут для запаса
                _tokenExpiry = DateTime.Now.AddMinutes(25);

                Console.WriteLine($"GigaChat token obtained successfully, expires at: {_tokenExpiry:yyyy-MM-dd HH:mm:ss}");

                // Разрешаем все ожидающие запросы
                ResolvePendingRequests(_token);
                return _token;
            }
            catch (Exception error)
            {
                Console.WriteLine($"GigaChat token error: {error}");
                // Отклоняем все ожидающие запросы
                RejectPendingRequests(error);
                throw new Exception($"Failed to get GigaChat token: {error.Message}");
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private async Task<string> WaitForTokenRefreshAsync()
        {
            var tcs = new TaskCompletionSource<string>();
            await _lock.WaitAsync();
            try
            {
                _refreshQueue.Add(tcs);
            }
            finally
            {
                _lock.Release();
            }

            return await tcs.Task;
        }

        private void ResolvePendingRequests(string token)
        {
            _lock.Wait();
            try
            {
                foreach (var tcs in _refreshQueue)
                {
                    tcs.SetResult(token);
                }
                _refreshQueue.Clear();
            }
            finally
            {
                _lock.Release();
            }
        }

        private void RejectPendingRequests(Exception error)
        {
            _lock.Wait();
            try
            {
                foreach (var tcs in _refreshQueue)
                {
                    tcs.SetException(error);
                }
                _refreshQueue.Clear();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<string> ChatCompletionAsync(List<Dictionary<string, string>> messages, Dictionary<string, object> additionalParams = null)
        {
            try
            {
                Console.WriteLine("Sending request to GigaChat API...");

                var token = await GetTokenAsync();
                var url = "https://gigachat.devices.sberbank.ru/api/v1/chat/completions";

                var requestData = new Dictionary<string, object>
                {
                    ["model"] = "GigaChat",
                    ["messages"] = messages,
                    ["stream"] = false,
                    ["repetition_penalty"] = 1,
                    ["temperature"] = 0.7,
                    ["max_tokens"] = 1000
                };

                // Добавляем дополнительные параметры если есть
                if (additionalParams != null)
                {
                    foreach (var param in additionalParams)
                    {
                        requestData[param.Key] = param.Value;
                    }
                }

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Authorization", $"Bearer {token}");

                var jsonContent = JsonSerializer.Serialize(requestData);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                Console.WriteLine("GigaChat API response received");
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

                return result.GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString();
            }
            catch (HttpRequestException error) when (error.Message.Contains("401"))
            {
                Console.WriteLine("Token expired, forcing refresh...");
                // Токен истек, принудительно обновляем
                _token = null;
                _tokenExpiry = DateTime.MinValue;
                throw new Exception("Token expired, please retry");
            }
            catch (Exception error)
            {
                Console.WriteLine($"GigaChat API error: {error}");
                throw new Exception($"GigaChat API request failed: {error.Message}");
            }
        }

        public async Task<string> SendMessageAsync(string message, Dictionary<string, object> additionalParams = null)
        {
            var messages = new List<Dictionary<string, string>>
        {
            new Dictionary<string, string>
            {
                ["role"] = "user",
                ["content"] = message
            }
        };

            return await ChatCompletionAsync(messages, additionalParams);
        }

        public async Task<string> RefreshTokenAsync()
        {
            _token = null;
            _tokenExpiry = DateTime.MinValue;
            return await GetTokenAsync(true);
        }

        public string GeneratePromt(string code)
        {
            string content = File.ReadAllText("Instruction.txt");

            string finalpromt = $"Есть инструкция по языку ява : \"{content}\"," +
                $"мне нужно чтобы ты переписал вот этот код: {code} в язык ЯВА и вернул ответ в виде JSON c алгоритмом как в инструкции и без лишних слов(только Json алгоритма)";

            return finalpromt;
        }

        public Dictionary<string, object> GetTokenStatus()
        {
            return new Dictionary<string, object>
            {
                ["has_token"] = !string.IsNullOrEmpty(_token),
                ["expires_at"] = _tokenExpiry.ToString("yyyy-MM-dd HH:mm:ss"),
                ["is_expired"] = string.IsNullOrEmpty(_token) || DateTime.Now >= _tokenExpiry,
                ["time_until_expiry"] = string.IsNullOrEmpty(_token) ? null : (_tokenExpiry - DateTime.Now).TotalSeconds
            };
        }
    }
}
