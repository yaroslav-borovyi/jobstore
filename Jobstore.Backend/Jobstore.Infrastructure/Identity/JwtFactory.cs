﻿using Jobstore.Infrastructure.Core;
using Jobstore.Infrastructure.Identity.Models;
using Microsoft.Extensions.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Jobstore.Infrastructure.Identity
{
	public class JwtFactory : IJwtFactory
	{
		private readonly JwtIssuerOptions _jwtOptions;

		public JwtFactory(IOptions<JwtIssuerOptions> jwtOptions)
		{
			_jwtOptions = jwtOptions.Value;
			ThrowIfInvalidOptions(_jwtOptions);
		}

		public async Task<string> GenerateEncodedToken(string userId)
		{
			var claims = new[]
		 {
				 new Claim(JwtRegisteredClaimNames.Sub, userId),
				 new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
				 new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
			 };

			var jwt = new JwtSecurityToken(
				issuer: _jwtOptions.Issuer,
				audience: _jwtOptions.Audience,
				claims: claims,
				notBefore: _jwtOptions.NotBefore,
				expires: _jwtOptions.Expiration,
				signingCredentials: _jwtOptions.SigningCredentials);

			var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

			return encodedJwt;
		}

		public ClaimsIdentity GenerateClaimsIdentity(string userName, string id)
		{
			return new ClaimsIdentity(new[]
			{
				new Claim(ClaimTypes.NameIdentifier, userName),
				new Claim(ClaimTypes.Email, userName),
				new Claim(ClaimTypes.Name, id),
                new Claim("Api", "Apiuser")
			});
		}

		private static long ToUnixEpochDate(DateTime date)
		  => (long)Math.Round((date.ToUniversalTime() -
							   new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
							  .TotalSeconds);

		private static void ThrowIfInvalidOptions(JwtIssuerOptions options)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));

			if (options.ValidFor <= TimeSpan.Zero)
			{
				throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(JwtIssuerOptions.ValidFor));
			}

			if (options.SigningCredentials == null)
			{
				throw new ArgumentNullException(nameof(JwtIssuerOptions.SigningCredentials));
			}

			if (options.JtiGenerator == null)
			{
				throw new ArgumentNullException(nameof(JwtIssuerOptions.JtiGenerator));
			}
		}
	}
}
