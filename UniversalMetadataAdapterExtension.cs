using SwarmUI.Core;
using SwarmUI.Utils;
using SwarmUI.WebAPI;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System;

namespace UniversalMetadataAdapter
{
    /// <summary>
    /// Universal Metadata Adapter Extension for SwarmUI.
    /// Fixes and imports metadata from ComfyUI, AUTOMATIC1111, Fooocus, and more using stable-diffusion-prompt-reader.
    /// </summary>
    public class UniversalMetadataAdapterExtension : Extension
    {
        public override string Name => "Universal Metadata Adapter";
        public override string Description => "Fix and import metadata from various Stable Diffusion UIs using stable-diffusion-prompt-reader.";

        // Cached CLI status
        private static JObject _cliStatusCache = null;
        private static DateTime _cliStatusLastChecked = DateTime.MinValue;
        private static readonly object _cliStatusLock = new();

        public override void OnInit()
        {
            // Register API endpoints
            API.RegisterAPICall(CheckCliStatus, false, null, "universal_metadata_adapter/check_cli_status");
            API.RegisterAPICall(RefreshCliStatus, true, null, "universal_metadata_adapter/refresh_cli_status");
            API.RegisterAPICall(DownloadOrUpdateCli, true, null, "universal_metadata_adapter/download_or_update_cli");
            API.RegisterAPICall(RunMetadataFixer, true, null, "universal_metadata_adapter/run_metadata_fixer");
            // Register tab assets
            ScriptFiles.Add("Assets/universal_metadata_adapter_tab.js");
            OtherAssets.Add("Assets/universal_metadata_adapter_tab.html");
            // Run initial CLI check at startup
            _ = System.Threading.Tasks.Task.Run(() => UpdateCliStatusCache());
        }

        // Helper: Find CLI path
        public static string FindPromptReaderCli()
        {
            string exeName = "sd-prompt-reader";
#if WINDOWS
            exeName += ".exe";
#endif
            // 1. Check extension directory
            string extPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "Extensions", "UniversalMetadataAdapter", exeName);
            if (File.Exists(extPath))
                return extPath;
            // 2. Check PATH
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = exeName,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var process = Process.Start(psi))
                {
                    process.WaitForExit(2000);
                    if (process.ExitCode == 0)
                        return exeName; // Found in PATH
                }
            }
            catch { }
            return null;
        }

        // Helper: Actually check CLI status and update cache
        public static void UpdateCliStatusCache()
        {
            lock (_cliStatusLock)
            {
                string cliPath = FindPromptReaderCli();
                bool installed = cliPath != null;
                string version = "";
                if (installed)
                {
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = cliPath,
                            Arguments = "--version",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        using (var process = Process.Start(psi))
                        {
                            version = process.StandardOutput.ReadToEnd().Trim();
                            process.WaitForExit(2000);
                        }
                    }
                    catch { }
                }
                _cliStatusCache = new JObject
                {
                    ["installed"] = installed,
                    ["version"] = version,
                    ["path"] = cliPath ?? "",
                    ["last_checked_utc"] = DateTime.UtcNow.ToString("o")
                };
                _cliStatusLastChecked = DateTime.UtcNow;
            }
        }

        // 1. Check CLI status (returns cached value)
        public static JObject CheckCliStatus(Session session)
        {
            lock (_cliStatusLock)
            {
                if (_cliStatusCache == null)
                {
                    // If not checked yet, do a quick check now (blocking)
                    UpdateCliStatusCache();
                }
                return new JObject(_cliStatusCache);
            }
        }

        // 1b. Manual refresh endpoint
        public static JObject RefreshCliStatus(Session session)
        {
            UpdateCliStatusCache();
            return CheckCliStatus(session);
        }

        // 2. Download or update CLI
        public static JObject DownloadOrUpdateCli(Session session)
        {
            // TODO: Implement logic to download or update the CLI tool
            // After download/update, refresh the cache
            UpdateCliStatusCache();
            return CheckCliStatus(session);
        }

        // 3. Run metadata fixer
        public static JObject RunMetadataFixer(Session session)
        {
            // TODO: Implement logic to scan images, run CLI, and inject metadata
            return new JObject
            {
                ["fixed"] = 0,
                ["errors"] = 0,
                ["details"] = new JArray()
            };
        }
    }
} 