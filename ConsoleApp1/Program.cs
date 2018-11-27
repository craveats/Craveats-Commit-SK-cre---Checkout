using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string keyHexString = "123A 456B 789C 012D 345E 678F 901A 234B C567 D890 E123 F456"/*" 7E89 01A2 34B5 67C8"*/.Replace(" ", ""),
                ivHexString = "345E 87F6 9A01 B423".Replace(" ", "");
            byte[] key = HexToByteArray(keyHexString), iv = HexToByteArray(ivHexString);

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("Original: {1}: [0x: {0}]", 
                    PrettyPrintBase64String(GetCryptoString(i, key, iv)), i);
                Console.WriteLine("Return trip: {1}: {0}", 
                    GetPlainString(
                        Convert.ToBase64String(
                            HexToByteArray(
                                PrettyPrintBase64String(
                                    GetCryptoString(i, key, iv)).ToLowerInvariant())), key, iv),
                    i);
            }

            Console.ReadKey();
        }

        static string PrettyPrintBase64String(string feed)
        {
            if (feed?.Length > 0)
            {
                return BitConverter.ToString(Convert.FromBase64String(feed)).Replace("-","");
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
            if (feed != null && feed.ToString().Length > 0)
            {
                byte[] encBytes = null;

                using (TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = GetTripleDESCryptoServiceProviderInstance())
                {
                    ICryptoTransform cryptoTransform = tripleDESCryptoServiceProvider.CreateEncryptor(key ?? tripleDESCryptoServiceProvider.Key, iv ?? tripleDESCryptoServiceProvider.IV);
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
            if (feed !=null && feed.ToString().Length > 0)
            {
                byte[] encBytes = null;

                using (TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = GetTripleDESCryptoServiceProviderInstance())
                {
                    ICryptoTransform cryptoTransform = tripleDESCryptoServiceProvider.CreateDecryptor(key ?? tripleDESCryptoServiceProvider.Key, iv ?? tripleDESCryptoServiceProvider.IV);
                    encBytes = Convert.FromBase64String(feed.ToString());
                    encBytes = cryptoTransform.TransformFinalBlock(encBytes, 0, encBytes.Length);
                    tripleDESCryptoServiceProvider.Clear();
                }

                return UTF8Encoding.UTF8.GetString(encBytes);
            }
            return feed?.ToString();
        }

        static byte[] HexToByteArray(string input)
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
    }
}
