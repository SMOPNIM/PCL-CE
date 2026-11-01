using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PCL.Phase0.OAuthLoopbackProbe;

internal static class Program
{
    private const string Authority = "https://login.microsoftonline.com/consumers";
    private const string Scope = "openid profile offline_access User.Read";

    public static async Task<int> Main()
    {
        var clientId = Environment.GetEnvironmentVariable("PHASE0_MS_CLIENT_ID");
        if (string.IsNullOrWhiteSpace(clientId))
        {
            Console.Error.WriteLine("Set PHASE0_MS_CLIENT_ID to your own registered Microsoft public-client application ID.");
            return 2;
        }

        var redirectUri = Environment.GetEnvironmentVariable("PHASE0_MS_REDIRECT_URI") ?? "http://localhost:8400/";
        if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var redirect) ||
            redirect.Scheme != Uri.UriSchemeHttp ||
            !redirect.IsLoopback ||
            redirect.Port <= 0 ||
            redirect.AbsolutePath != "/" ||
            !string.IsNullOrEmpty(redirect.Query) ||
            !string.IsNullOrEmpty(redirect.Fragment) ||
            !redirectUri.EndsWith("/", StringComparison.Ordinal))
        {
            Console.Error.WriteLine("PHASE0_MS_REDIRECT_URI must be an exact http://localhost:<port>/ loopback URI.");
            return 2;
        }

        var state = _Base64Url(RandomNumberGenerator.GetBytes(32));
        var verifier = _Base64Url(RandomNumberGenerator.GetBytes(32));
        var challenge = _Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));

        using var listener = new HttpListener();
        listener.Prefixes.Add(redirectUri);
        listener.Start();
        Console.WriteLine($"Loopback listener ready: {redirectUri}");

        var authorizeUri = _BuildAuthorizeUri(clientId, redirectUri, state, challenge);
        _OpenBrowser(authorizeUri);
        Console.WriteLine("Complete sign-in in the system browser. This probe does not collect or display credentials.");

        HttpListenerContext context;
        try
        {
            context = await listener.GetContextAsync().WaitAsync(TimeSpan.FromMinutes(3));
        }
        catch (TimeoutException)
        {
            Console.Error.WriteLine("OAuth callback timed out after three minutes.");
            return 3;
        }

        var response = context.Response;
        try
        {
            if (!string.Equals(context.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(context.Request.Url?.AbsolutePath, "/", StringComparison.Ordinal))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                await _WriteResponseAsync(response, "Unexpected callback request.");
                Console.Error.WriteLine("OAuth callback rejected: unexpected method or path.");
                return 1;
            }

            var query = context.Request.QueryString;
            if (string.IsNullOrEmpty(query["state"]) ||
                !CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(query["state"]!),
                    Encoding.UTF8.GetBytes(state)))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                await _WriteResponseAsync(response, "OAuth state validation failed. You may close this window.");
                Console.Error.WriteLine("OAuth callback rejected: state mismatch.");
                return 1;
            }

            if (!string.IsNullOrWhiteSpace(query["error"]))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                await _WriteResponseAsync(response, "Sign-in was not completed. You may close this window.");
                Console.Error.WriteLine($"OAuth authorization failed: {query["error"]}");
                return 1;
            }

            var code = query["code"];
            if (string.IsNullOrWhiteSpace(code))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                await _WriteResponseAsync(response, "Authorization code was missing. You may close this window.");
                Console.Error.WriteLine("OAuth callback rejected: authorization code missing.");
                return 1;
            }

            await _WriteResponseAsync(response, "Loopback callback received. You may close this window.");
            var lifetimeSeconds = await _ExchangeCodeAsync(clientId, redirectUri, code, verifier);
            Console.WriteLine($"TOKEN_EXCHANGE_OK expires_in={lifetimeSeconds}");
            Console.WriteLine("Access and refresh tokens were kept in memory only and were not printed or persisted.");
            return 0;
        }
        finally
        {
            response.Close();
        }
    }

    private static Uri _BuildAuthorizeUri(string clientId, string redirectUri, string state, string challenge)
    {
        var query = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["response_type"] = "code",
            ["redirect_uri"] = redirectUri,
            ["response_mode"] = "query",
            ["scope"] = Scope,
            ["state"] = state,
            ["code_challenge"] = challenge,
            ["code_challenge_method"] = "S256"
        };
        var builder = new UriBuilder($"{Authority}/oauth2/v2.0/authorize")
        {
            Query = string.Join("&", query.Select(pair =>
                $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"))
        };
        return builder.Uri;
    }

    private static async Task<int> _ExchangeCodeAsync(
        string clientId,
        string redirectUri,
        string code,
        string verifier)
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri,
            ["scope"] = Scope,
            ["code_verifier"] = verifier
        });

        using var response = await httpClient.PostAsync($"{Authority}/oauth2/v2.0/token", content);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Token exchange failed with HTTP {(int)response.StatusCode}; response body withheld.");

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        if (!root.TryGetProperty("access_token", out var accessToken) ||
            accessToken.ValueKind != JsonValueKind.String ||
            string.IsNullOrEmpty(accessToken.GetString()))
            throw new InvalidDataException("Token endpoint response did not contain an access token.");

        if (!root.TryGetProperty("expires_in", out var expiresIn) || !expiresIn.TryGetInt32(out var seconds))
            throw new InvalidDataException("Token endpoint response did not contain a valid expires_in value.");

        return seconds;
    }

    private static void _OpenBrowser(Uri authorizeUri)
    {
        var startInfo = OperatingSystem.IsWindows()
            ? new ProcessStartInfo(authorizeUri.ToString()) { UseShellExecute = true }
            : new ProcessStartInfo("xdg-open")
            {
                UseShellExecute = false,
                ArgumentList = { authorizeUri.ToString() }
            };

        if (Process.Start(startInfo) is null)
            throw new InvalidOperationException("Could not start the system browser.");
    }

    private static string _Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static async Task _WriteResponseAsync(HttpListenerResponse response, string message)
    {
        var body = Encoding.UTF8.GetBytes(
            $"<!doctype html><meta charset=\"utf-8\"><title>Phase 0 OAuth Probe</title><p>{WebUtility.HtmlEncode(message)}</p>");
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = body.Length;
        await response.OutputStream.WriteAsync(body);
    }
}
