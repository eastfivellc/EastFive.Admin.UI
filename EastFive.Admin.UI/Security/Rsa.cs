﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EastFive.Security
{
    public static class RSA
    {
        public static TResult RsaProvider<TResult>(this string secretAsRSAXmlBase64,
            Func<RSACryptoServiceProvider, TResult> success,
            Func<string, TResult> invalidSecret)
        {
            try
            {
                var bytes = Convert.FromBase64String(secretAsRSAXmlBase64);
                var xml = Encoding.ASCII.GetString(bytes);
                var rsaProvider = new RSACryptoServiceProvider();
                try
                {
                    rsaProvider.FromXmlString(xml);
                    return success(rsaProvider);
                }
                catch (CryptographicException ex)
                {
                    return invalidSecret(ex.Message);
                }
            }
            catch (FormatException ex)
            {
                return invalidSecret(ex.Message);
            }
        }

        public static TResult Generate<TResult>(Func<string, string, TResult> success)
        {
            var cspParams = new CspParameters()
            {
                ProviderType = 1, // PROV_RSA_FULL
                Flags = CspProviderFlags.UseArchivableKey,
                KeyNumber = (int)KeyNumber.Exchange,
            };
            var rsaProvider = new RSACryptoServiceProvider(2048, cspParams);

            // Export public key
            var publicKey = Convert.ToBase64String(
                Encoding.ASCII.GetBytes(
                    rsaProvider.ToXmlString(false)));

            // Export private/public key pair
            var privateKey = Convert.ToBase64String(
                Encoding.ASCII.GetBytes(
                    rsaProvider.ToXmlString(true)));

            return success(publicKey, privateKey);
        }
    }
}
