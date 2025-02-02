﻿using DocumentFormat.OpenXml.Presentation;
using KoOrderRegister.Utility;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KoOrderRegister.Services
{
    public class AppUpdateService : BackgroundService, IAppUpdateService
    {
        private readonly IVersionService _versionService;
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://api.github.com/repos/BenKoncsik/KoOrderRegister/releases/latest";
#if DEBUG
        private readonly string VERSION = "DEV_VERSION";
#elif DEVBUILD
        private readonly string VERSION = "DEV_VERSION";
#else
        private readonly string VERSION = "STABIL_VERSION";
#endif

        public string AppVersion { get; }
        public AppUpdateService(IHttpClientFactory httpClientFactory, IVersionService versionService)
        {
            _versionService = versionService;
            AppVersion = _versionService.AppVersion;
            AppShell.AppVersion = _versionService.AppVersion;
            _httpClient = httpClientFactory.CreateClient("GitHubClient");
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("app-client");
        }
        public async Task<AppUpdateInfo> CheckForAppInstallerUpdatesAndLaunchAsync()
        {
            try
            {
                var (latestVersion, msixUrl) = await GetLatestReleaseInfoAsync();
                if (latestVersion == null) return new AppUpdateInfo();

                var currentVersion = _versionService.AppVersion;
                if (new Version(latestVersion) > new Version(currentVersion))
                {
                    return new AppUpdateInfo
                    {
                        OldVersion = currentVersion,
                        NewVersion = latestVersion,
                        DownloadUrl = msixUrl
                    };

                }
#if DEBUG
                else
                {
                    return new AppUpdateInfo
                    {
                        OldVersion = currentVersion,
                        NewVersion = latestVersion,
                        DownloadUrl = msixUrl
                    };
                }
#endif
                return new AppUpdateInfo();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new AppUpdateInfo();
            }
        }
        public async Task<(string version, string msixUrl)> GetLatestReleaseInfoAsync()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "app-client");

            var response = await _httpClient.GetAsync(_apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var release = JsonConvert.DeserializeObject<GitHubRelease>(jsonResponse);
                string msixUrl = string.Empty;
                string version = string.Empty;
                var architecture = _versionService.DeviceType.ToLowerInvariant();
                foreach (var asset in release.Assets)
                {
                    if (asset.name.ToLower().Contains(architecture.ToLower()) && asset.name.ToLower().Contains(VERSION.ToLower()))
                    {
                        string responseVersion = asset.browser_download_url.Split("/").Last().Split("_")[1];
                        if (string.IsNullOrEmpty(version))
                        {
                            msixUrl = asset.browser_download_url;
                            version = responseVersion;
                        }
                        else if (new Version(version) < new Version(responseVersion))
                        {
                            msixUrl = asset.browser_download_url;
                            version = responseVersion;
                        }
                    }

                }

                return (version, msixUrl);
            }

            return (null, null);
        }

        private class GitHubRelease
        {
            public string TagName { get; set; }
            public Asset[] Assets { get; set; }

            public class Asset
            {
                public string browser_download_url { get; set; }
                public string name { get; set; }
            }
        }

        public async Task<string> DownloadFileAsync(string fileUrl, IProgress<double> progress, CancellationToken stoppingToken)
        {

            this.fileUrl = fileUrl;
            this.progress = progress;
            await ExecuteAsync(stoppingToken);
            return downloadedFileUrl;
        }
        private string fileUrl;
        private IProgress<double> progress;
        private string downloadedFileUrl = string.Empty;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DownloadManager.DownloadManager.UseCustomHttpClient(_httpClient);
            downloadedFileUrl = await DownloadManager.DownloadManager.DownloadAsync(_versionService.UpdatePackageName, fileUrl, progress);
        }
    }
}
