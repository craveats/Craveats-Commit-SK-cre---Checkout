namespace Generic.Obfuscation.TripleDES
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.IO;

    public sealed class DataSecurityTripleDES
    {
        private static byte[] _Key = null;
        private static byte[] Key 
        {
            get
            {
                try
                {
                    if (_Key == null)
                    {
                        _Key = HexToByteArray(Properties.Settings.Default.TDESKeyHex?.Replace(" ", ""));
                    }
                }
                catch
                {
                    _Key = null;
                }

                return _Key;
            }
        }

        private static byte[] _IV = null;
        private static byte[] IV
        {
            get
            {
                try
                {
                    if (_IV == null)
                    {
                        _IV = HexToByteArray(Properties.Settings.Default.TDESIVHex?.Replace(" ", ""));
                    }
                }
                catch
                {
                    _IV = null;
                }

                return _IV;
            }
        }

        static string PrettyPrintBase64String(string feed)
        {
            if (feed?.Length > 0)
            {
                return BitConverter.ToString(Convert.FromBase64String(feed)).Replace("-", "");
            }
            return feed;
        }

        static TripleDESCryptoServiceProvider GetTripleDESCryptoServiceProviderInstance()
        {
            return new TripleDESCryptoServiceProvider()
            {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
        }

        static string GetCryptoString(object feed, byte[] key = null, byte[] iv = null)
        {
            if (feed != null && feed.ToString().Trim().Length > 0)
            {
                byte[] encBytes = null;

                using (TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = GetTripleDESCryptoServiceProviderInstance())
                {
                    ICryptoTransform cryptoTransform = tripleDESCryptoServiceProvider.CreateEncryptor(
                        key ?? Key ?? tripleDESCryptoServiceProvider.Key, 
                        iv ?? IV ?? tripleDESCryptoServiceProvider.IV);
                    encBytes = UTF8Encoding.UTF8.GetBytes(feed.ToString());
                    encBytes = cryptoTransform.TransformFinalBlock(encBytes, 0, encBytes.Length);
                    tripleDESCryptoServiceProvider.Clear();
                }

                return Convert.ToBase64String(encBytes, 0, encBytes.Length);
            }
            return feed?.ToString();
        }

        static string GetPlainString(object feed, byte[] key = null, byte[] iv = null)
        {
            if (feed != null && feed.ToString().Trim().Length > 0)
            {
                byte[] encBytes = null;

                using (TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = GetTripleDESCryptoServiceProviderInstance())
                {
                    ICryptoTransform cryptoTransform = tripleDESCryptoServiceProvider.CreateDecryptor(
                        key ?? Key ?? tripleDESCryptoServiceProvider.Key,
                        iv ?? IV ?? tripleDESCryptoServiceProvider.IV);
                    encBytes = Convert.FromBase64String(feed.ToString());
                    encBytes = cryptoTransform.TransformFinalBlock(encBytes, 0, encBytes.Length);
                    tripleDESCryptoServiceProvider.Clear();
                }

                return UTF8Encoding.UTF8.GetString(encBytes);
            }
            return feed?.ToString();
        }

        private static byte[] HexToByteArray(string input)
        {
            byte[] outArray = null;
            try
            {
                if (input == null || input.Length % 2 != 0)
                {
                    throw new ArgumentException("parameter cannot be null or asymmetric in length", "input");
                }

                int j = (input.Length / 2), i = 0;
                outArray = new byte[j];
                for (; i < j; i++)
                {
                    outArray[i] = Convert.ToByte(input.Substring(i * 2, 2), 16);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return outArray;
        }

        public static string GetEncryptedText(object feed, byte[] key = null, byte[] iv = null)
        {
            if (feed != null && feed.ToString().Trim().Length > 0)
            {
                return PrettyPrintBase64String(GetCryptoString(feed, key, iv));
            }
            return feed?.ToString();
        }

        public static string GetPlainText(object feed, byte[] key = null, byte[] iv = null)
        {
            if (feed != null && feed.ToString().Trim().Length > 0)
            {
                return GetPlainString(
                    Convert.ToBase64String(
                        HexToByteArray(
                            feed.ToString().ToLowerInvariant())), key, iv);
            }
            return feed?.ToString();
        }

        static bool IsPatternValid(object data)
        {
            if (data?.ToString().Trim().Length > 0)
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[^a-e0-9]", 
                    System.Text.RegularExpressions.RegexOptions.Compiled);
                if (regex.IsMatch(data.ToString()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

namespace Generic.Obfuscation.SHA1
{
    using System;
    using System.Security.Cryptography;

    public sealed class SHA1HashProvider
    {
        /// <summary>Creates a random string suitable for hiding in xxx-hashed strings</summary>
        public string GetRandomHexString(int Length)
        {
            string[] hexChars = { "0", "f", "1", "e", "2", "d", "3", "c", "4", "b", "5", "a", "6", "9", "7", "8" };
            Random rnd = new Random(new Random().Next(1000000));
            System.Text.StringBuilder strRandom = new System.Text.StringBuilder("");
            for (int i = 0; i < Length; i++)
                strRandom.Append(hexChars[rnd.Next(0, 16)]);
            return strRandom.ToString();
        }

        /// <summary>
        /// Hashes text as SHA1.
        /// </summary>
        /// <param name="clearText">The clear text.</param>
        /// <returns></returns>
        public string HashSHA1(string clearText)
        {
            // create hash
            System.Security.Cryptography.SHA1CryptoServiceProvider SHA1CSP = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] byteStream = System.Text.Encoding.UTF8.GetBytes(clearText);
            byteStream = SHA1CSP.ComputeHash(byteStream);
            System.Text.StringBuilder hashText = new System.Text.StringBuilder();
            foreach (byte b in byteStream)
            {
                hashText.Append(b.ToString("x2").ToLower());
            }
            //Console.WriteLine($"hCT: {hashText.ToString()}");
            return hashText.ToString();
        }

        public string SecureSHA1(string clearText)
        {
            // get random seeds
            byte[] baLeft = new byte[4],
                baRight = new byte[4];
            RandomNumberGenerator rngInstance = RandomNumberGenerator.Create();
            rngInstance.GetBytes(baLeft);
            rngInstance.GetBytes(baRight);
            string randomSeedStart = System.BitConverter.ToString(baLeft).Replace("-", ""),
                randomSeedEnd = System.BitConverter.ToString(baRight).Replace("-", ""),
                hashText = string.Empty; ;

            {
                // append random seed to end of input string
                clearText += randomSeedStart + randomSeedEnd;
                hashText = HashSHA1(clearText);

                // append random seeds to start and end of hash value for extra security
                hashText = randomSeedStart + hashText + randomSeedEnd;
            }

            return hashText;
        }

        public bool CheckHashSHA1(string clearText, string hashText, int seedLength = 8)
        {
            if (hashText.Length < 50) return false;

            if (string.IsNullOrEmpty(clearText)) return false;

            string checkHashText = string.Empty;

            {
                string randomSeedStart = hashText.Substring(0, seedLength), 
                    randomSeedEnd = hashText.Substring(hashText.Length - seedLength);

                clearText += randomSeedStart + randomSeedEnd;

                checkHashText = HashSHA1(clearText);
                checkHashText = randomSeedStart + checkHashText + randomSeedEnd;
            }
            return string.Compare(hashText, checkHashText, true) == 0;
        }
    }
}