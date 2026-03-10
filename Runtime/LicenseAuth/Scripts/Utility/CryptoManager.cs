using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Landscape2.Runtime
{
    public static class CryptoManager
    {
        // セキュリティ要件によっては外部化
        private const string Passphrase = "SecretPassphrase";

        private static readonly byte[] Salt = Encoding.UTF8.GetBytes("FixedSalt1234567");

        public static byte[] GetOrCreateKey(int keySize = 16, int iterations = 100_000)
        {
            using var derive = new Rfc2898DeriveBytes(Passphrase, Salt, iterations, HashAlgorithmName.SHA256);
            return derive.GetBytes(keySize);
        }

        public static void EncryptToFile(string plainText, string outputPath)
        {
            byte[] key = GetOrCreateKey();
            byte[] iv = GenerateRandomBytes(16);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            ms.Write(iv, 0, iv.Length); // prefix IV

            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using var sw = new StreamWriter(cs);
            sw.Write(plainText);
            sw.Close();

            File.WriteAllBytes(outputPath, ms.ToArray());
        }

        public static string DecryptFromFile(string inputPath)
        {
            byte[] allBytes = File.ReadAllBytes(inputPath);
            byte[] iv = new byte[16];
            Array.Copy(allBytes, 0, iv, 0, 16);
            byte[] cipherText = new byte[allBytes.Length - 16];
            Array.Copy(allBytes, 16, cipherText, 0, cipherText.Length);

            byte[] key = GetOrCreateKey();

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(cipherText);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }

        private static byte[] GenerateRandomBytes(int length)
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return bytes;
        }
    }
}