using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

using Microsoft.IdentityModel.Tokens;


namespace EastFive.Security
{
    public static class JwtTools
    {
        public static TResult ParseToken<TResult>(
            this string jwtEncodedString,
            string issuer,
            string rsaKeyToValidateAgainst,
            Func<Claim[], DateTime, DateTime, TResult> success,
            Func<string, TResult> invalidToken,
            Func<string, TResult> onInvalidKey)
        {
            return rsaKeyToValidateAgainst.RsaProvider(
                rsaProvider =>
                {
                    var validationParameters = new TokenValidationParameters()
                    {
                        ValidateAudience = false,
                        ValidIssuer = issuer,
                        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.RsaSecurityKey(rsaProvider),
                        RequireExpirationTime = true,
                    };

                    try
                    {
                        Microsoft.IdentityModel.Tokens.SecurityToken validatedToken;
                        var handler = new JwtSecurityTokenHandler();
                        var principal = handler.ValidateToken(jwtEncodedString, validationParameters, out validatedToken);

                        // TODO: Check if token is still valid at current date / time?
                        var claims = principal.Claims.ToArray();
                        return success(claims, validatedToken.ValidFrom, validatedToken.ValidTo);

                    }
                    catch (ArgumentException ex)
                    {
                        return invalidToken(ex.Message);
                    }
                    catch (Microsoft.IdentityModel.Tokens.SecurityTokenInvalidIssuerException ex)
                    {
                        return invalidToken(ex.Message);
                    }
                    catch (Microsoft.IdentityModel.Tokens.SecurityTokenExpiredException ex)
                    {
                        return invalidToken(ex.Message);
                    }
                    catch (Microsoft.IdentityModel.Tokens.SecurityTokenException ex)
                    {
                        return invalidToken(ex.Message);
                    }
                },
                onInvalidKey);
        }

        public static TResult CreateToken<TResult>(
            this string secretAsRSAXmlBase64,
            string issuer,
            Uri scope,
            DateTime issued, TimeSpan duration,
            IEnumerable<Claim> claims,
            Func<string, TResult> tokenCreated,
            Func<string, TResult> onInvalidSecret)
        {
            return secretAsRSAXmlBase64.RsaProvider(
                (rsaProvider) =>
                {
                    var token = rsaProvider.JwtToken(issuer, scope, claims,
                        issued, duration);
                    return tokenCreated(token);
                },
                onInvalidSecret);
        }

        public static string JwtToken(this System.Security.Cryptography.RSACryptoServiceProvider rsaProvider,
            string issuer, Uri scope,
            IEnumerable<Claim> claims,
            DateTime issued, TimeSpan duration)
        {
            var securityKey = new Microsoft.IdentityModel.Tokens.RsaSecurityKey(rsaProvider);

            var signature = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.RsaSha256Signature);
            var expires = (issued + duration);
            var token = new JwtSecurityToken(issuer, scope.AbsoluteUri, claims, issued, expires, signature);
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.WriteToken(token);
            return jwt;
        }

    }
}