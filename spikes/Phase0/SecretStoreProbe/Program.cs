using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace PCL.Phase0.SecretStoreProbe;

internal static class Program
{
    private const string ProbeKey = "phase0-secret-store";

    public static async Task<int> Main(string[] args)
    {
        if (!OperatingSystem.IsLinux())
        {
            Console.Error.WriteLine("This probe is intended to run on Linux.");
            return 2;
        }

        if (args.Length != 1 || args[0] is not ("store" or "read" or "clear"))
        {
            Console.Error.WriteLine("Usage: SecretStoreProbe <store|read|clear>");
            return 2;
        }

        var store = new SecretStore();
        switch (args[0])
        {
            case "store":
            {
                var secret = $"PCL Phase 0 test value {Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))}";
                var backend = await store.StoreAsync(ProbeKey, secret);
                Console.WriteLine($"STORE_OK backend={backend} fingerprint={Fingerprint(secret)}");
                return 0;
            }
            case "read":
            {
                var (secret, backend) = await store.ReadAsync(ProbeKey);
                Console.WriteLine($"READ_OK backend={backend} fingerprint={Fingerprint(secret)}");
                return 0;
            }
            case "clear":
            {
                var backend = await store.ClearAsync(ProbeKey);
                Console.WriteLine($"CLEAR_OK backend={backend}");
                return 0;
            }
            default:
                return 2;
        }
    }

    private static string Fingerprint(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))[..12];
}

internal enum SecretBackend
{
    SecretService,
    AesFallback
}

internal interface ISecretStore
{
    Task<SecretBackend> StoreAsync(string key, string secret);
    Task<(string Secret, SecretBackend Backend)> ReadAsync(string key);
    Task<SecretBackend> ClearAsync(string key);
}

[SupportedOSPlatform("linux")]
internal sealed class SecretStore : ISecretStore
{
    private const string ServiceApplication = "pcl-phase0-secret-store-probe";
    private const string FileDirectoryName = "pcl-phase0-secret-store-probe";
    private static readonly UnixFileMode PrivateDirectoryMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
    private static readonly UnixFileMode PrivateFileMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite;

    private readonly string _directory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".local",
        "share",
        FileDirectoryName);

    public async Task<SecretBackend> StoreAsync(string key, string secret)
    {
        var result = await _RunSecretToolAsync("store", key, secret);
        if (result is { ExitCode: 0 })
            return SecretBackend.SecretService;

        _LogFallback(result?.Error);
        await _StoreFallbackAsync(key, secret);
        return SecretBackend.AesFallback;
    }

    public async Task<(string Secret, SecretBackend Backend)> ReadAsync(string key)
    {
        var result = await _RunSecretToolAsync("lookup", key);
        if (result is { ExitCode: 0 })
            return (result.Output.TrimEnd('\r', '\n'), SecretBackend.SecretService);

        _LogFallback(result?.Error);
        return (await _ReadFallbackAsync(key), SecretBackend.AesFallback);
    }

    public async Task<SecretBackend> ClearAsync(string key)
    {
        var result = await _RunSecretToolAsync("clear", key);
        if (result is { ExitCode: 0 })
        {
            _DeleteFallback(key);
            return SecretBackend.SecretService;
        }

        _LogFallback(result?.Error);
        _DeleteFallback(key);
        return SecretBackend.AesFallback;
    }

    private async Task<SecretToolResult?> _RunSecretToolAsync(string operation, string key, string? secret = null)
    {
        var startInfo = new ProcessStartInfo("secret-tool")
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        startInfo.ArgumentList.Add(operation);
        if (operation == "store")
            startInfo.ArgumentList.Add("--label=PCL Phase 0 Secret Store Probe");
        startInfo.ArgumentList.Add("application");
        startInfo.ArgumentList.Add(ServiceApplication);
        startInfo.ArgumentList.Add("key");
        startInfo.ArgumentList.Add(key);

        using var process = new Process { StartInfo = startInfo };
        try
        {
            if (!process.Start())
                return null;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return null;
        }

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        if (secret is not null)
            await process.StandardInput.WriteLineAsync(secret);
        process.StandardInput.Close();
        await process.WaitForExitAsync();

        return new SecretToolResult(process.ExitCode, await outputTask, await errorTask);
    }

    private async Task _StoreFallbackAsync(string key, string secret)
    {
        _EnsurePrivateDirectory();
        var encryptionKey = _ReadOrCreateEncryptionKey(key);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plaintext = Encoding.UTF8.GetBytes(secret);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[16];
        using (var aes = new AesGcm(encryptionKey, tag.Length))
            aes.Encrypt(nonce, plaintext, ciphertext, tag);

        var payload = new byte[nonce.Length + tag.Length + ciphertext.Length];
        nonce.CopyTo(payload, 0);
        tag.CopyTo(payload, nonce.Length);
        ciphertext.CopyTo(payload, nonce.Length + tag.Length);
        await _WritePrivateFileAsync(_GetDataPath(key), payload);
    }

    private async Task<string> _ReadFallbackAsync(string key)
    {
        _EnsurePrivateDirectory();
        var keyPath = _GetKeyPath(key);
        var dataPath = _GetDataPath(key);
        _VerifyPrivateFile(keyPath);
        _VerifyPrivateFile(dataPath);
        var encryptionKey = await File.ReadAllBytesAsync(keyPath);
        var payload = await File.ReadAllBytesAsync(dataPath);
        if (payload.Length < 28)
            throw new InvalidDataException("Fallback secret payload is incomplete.");

        var nonce = payload.AsSpan(0, 12);
        var tag = payload.AsSpan(12, 16);
        var ciphertext = payload.AsSpan(28);
        var plaintext = new byte[ciphertext.Length];
        using (var aes = new AesGcm(encryptionKey, tag.Length))
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    private byte[] _ReadOrCreateEncryptionKey(string key)
    {
        var path = _GetKeyPath(key);
        if (File.Exists(path))
        {
            _VerifyPrivateFile(path);
            return File.ReadAllBytes(path);
        }

        var encryptionKey = RandomNumberGenerator.GetBytes(32);
        try
        {
            using var stream = _CreatePrivateFile(path);
            stream.Write(encryptionKey);
            stream.Flush(true);
        }
        catch (IOException) when (File.Exists(path))
        {
            _VerifyPrivateFile(path);
            return File.ReadAllBytes(path);
        }

        return encryptionKey;
    }

    private async Task _WritePrivateFileAsync(string path, byte[] contents)
    {
        var temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";
        try
        {
            await using (var stream = _CreatePrivateFile(temporaryPath))
            {
                await stream.WriteAsync(contents);
                await stream.FlushAsync();
            }

            File.Move(temporaryPath, path, overwrite: true);
            _VerifyPrivateFile(path);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private FileStream _CreatePrivateFile(string path) =>
        new(path, new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.Write,
            Share = FileShare.None,
            UnixCreateMode = PrivateFileMode
        });

    private void _EnsurePrivateDirectory()
    {
        Directory.CreateDirectory(_directory);
        File.SetUnixFileMode(_directory, PrivateDirectoryMode);
        var mode = File.GetUnixFileMode(_directory);
        if ((mode & (UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
                     UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute)) != 0)
            throw new UnauthorizedAccessException("Fallback directory must have mode 0700.");
    }

    private static void _VerifyPrivateFile(string path)
    {
        var mode = File.GetUnixFileMode(path);
        if ((mode & (UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
                     UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute)) != 0)
            throw new UnauthorizedAccessException($"Fallback file must have mode 0600: {Path.GetFileName(path)}");
    }

    private string _GetKeyPath(string key) => Path.Combine(_directory, $"{_GetKeyHash(key)}.key");
    private string _GetDataPath(string key) => Path.Combine(_directory, $"{_GetKeyHash(key)}.enc");

    private static string _GetKeyHash(string key) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key))).ToLowerInvariant();

    private void _DeleteFallback(string key)
    {
        var keyPath = _GetKeyPath(key);
        var dataPath = _GetDataPath(key);
        if (File.Exists(keyPath))
            File.Delete(keyPath);
        if (File.Exists(dataPath))
            File.Delete(dataPath);
    }

    private static void _LogFallback(string? reason)
    {
        var detail = string.IsNullOrWhiteSpace(reason) ? "secret-tool unavailable or Secret Service request failed" : reason.Trim();
        Console.Error.WriteLine($"WARNING: Secret Service unavailable ({detail}); using AES-256-GCM file fallback.");
        Console.Error.WriteLine("WARNING: This Phase 0 probe stores its random key and encrypted value under one 0700 directory; it tests permissions and round-trip only, not production-grade key isolation.");
    }

    private sealed record SecretToolResult(int ExitCode, string Output, string Error);
}
