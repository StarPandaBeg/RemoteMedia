using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RemoteMedia.Application.Security {
    class SecurityUtility {

        private byte[] _key;
        private HashSet<int> _nonceValues = new HashSet<int>();

        public SecurityUtility(byte[] key) {
            _key = key;
        }

        public byte[] Encrypt(byte[] data) {
            using (Aes aes = Aes.Create()) {
                var key = _key.Take(16).ToArray();
                aes.Key = key;

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.GenerateIV();
                var iv = aes.IV;

                using (var stream = new MemoryStream()) {
                    stream.Write(iv, 0, iv.Length);

                    using (var cs = new CryptoStream(stream, aes.CreateEncryptor(), CryptoStreamMode.Write)) {
                        cs.Write(data, 0, data.Length);
                    }
                    return stream.ToArray();
                }
            }
        }

        public byte[] Decrypt(byte[] data) {
            using (Aes aes = Aes.Create()) {
                var key = _key.Take(16).ToArray();
                aes.Key = key;

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var iv = aes.IV = data.Take(aes.BlockSize / 8).ToArray();

                using (var stream = new MemoryStream(data.Skip(aes.BlockSize / 8).ToArray())) {
                    using (var cs = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read)) {
                        using (MemoryStream plainStream = new MemoryStream()) {
                            cs.CopyTo(plainStream);
                            return plainStream.ToArray();
                        }
                    }
                }
            }
        }

        public byte[] Sign(byte[] data) {
            using (HMACSHA256 hmac = new HMACSHA256(_key)) {
                var hash = hmac.ComputeHash(data);
                return data.Concat(hash).ToArray();
            }
        }

        public bool Validate(byte[] data, out byte[] plainData) {
            plainData = null;

            if (data.Length < 32) return false;
            plainData = data.Take(data.Length - 32).ToArray();

            var expected = Sign(plainData).Skip(plainData.Length).ToArray();
            var actual = data.Skip(data.Length - 32).ToArray();

            return Enumerable.SequenceEqual(expected, actual);
        }

        public bool CheckNonce(byte[] data, out byte[] plainData) {
            plainData = null;
            if (data.Length < 12) return false;

            using (var ms = new MemoryStream(data)) {
                using (var br = new BinaryReader(ms)) {
                    var timestamp = br.ReadInt64();
                    var nonce = br.ReadInt32();

                    var now = DateTimeOffset.Now.ToUnixTimeSeconds();
                    var delta = TimeSpan.FromSeconds(now - timestamp);
                    if (delta.TotalMinutes >= 5) _nonceValues.Clear();

                    if (_nonceValues.Contains(nonce)) return false;
                    _nonceValues.Add(nonce);

                    plainData = br.ReadBytes(data.Length - 12);
                }
            }
            return true;
        }
    }
}
