using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Landscape2.Runtime.LicenseAuth
{
    public static class LicenseManager
    {
        public static UserDto CurrentUser { get; set; }

        public static LicenseDto CurrentLicense { get; set; }

        public static UpdateDto CurrentUpdate { get; set; }

        // 有効期限内かチェック
        public static bool IsLicenseValid()
        {
            if (CurrentLicense == null)
            {
                Debug.Log("[LicenseManager] Current license or expiry date is not set.");
                return false;
            }

            return DateTime.Now < CurrentLicense.ExpiryDate.ToLocalTime();
        }

        public static void SaveLoginCache()
        {
            var cache = new LoginCacheDto
            {
                CurrentTokenInfo = AuthManager.CurrentTokenInfo,
                CurrentUser = LicenseManager.CurrentUser,
                CurrentLicense = LicenseManager.CurrentLicense,
                CurrentUpdate = LicenseManager.CurrentUpdate
            };

            LoginCache.Save(cache);
        }

        public static void ClearLoginCache()
        {
            AuthManager.SetTokenInfo(new TokenInfo());
            CurrentUser = null;
            CurrentLicense = null;
            CurrentUpdate = null;

            LoginCache.Clear();
        }

        public static bool LoadLoginCache()
        {
            var cache = LoginCache.Load();

            if (cache == null)
            {
                Debug.Log("[LicenseManager] No login cache found.");

                return false;
            }

            if (cache.CacheExpiration < DateTimeOffset.UtcNow)
            {
                return false;
            }

            if (cache != null)
            {
                AuthManager.SetTokenInfo(cache.CurrentTokenInfo);
                CurrentUser = cache.CurrentUser;
                CurrentLicense = cache.CurrentLicense;
                CurrentUpdate = cache.CurrentUpdate;
            }

            Debug.Log("[LicenseManager] login cache found.");

            return true;
        }

        public class LoginCacheDto
        {
            public DateTimeOffset CacheExpiration { get; set; } = DateTimeOffset.UtcNow.AddDays(30);

            public TokenInfo CurrentTokenInfo { get; set; }

            public UserDto CurrentUser { get; set; }

            public LicenseDto CurrentLicense { get; set; }

            public UpdateDto CurrentUpdate { get; set; }
        }

        public static class LoginCache
        {
            private static readonly string FilePath = Path.Combine(Application.persistentDataPath, "login_cache.dat");

            public static void Save(LoginCacheDto cache)
            {
                string json = JsonConvert.SerializeObject(cache);

                CryptoManager.EncryptToFile(json, FilePath);

                Debug.Log($"[LoginCache] Saved to {FilePath}");
            }

            public static LoginCacheDto Load()
            {
                if (!File.Exists(FilePath))
                {
                    Debug.Log("[LoginCache] No saved cache file found.");

                    return null;
                }

                try
                {
                    string json = CryptoManager.DecryptFromFile(FilePath);

                    var cache = JsonConvert.DeserializeObject<LoginCacheDto>(json);

                    return cache;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[LoginCache] Failed to load: {ex.Message}");

                    return null;
                }
            }

            public static void Clear()
            {
                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }
            }
        }
    }
}