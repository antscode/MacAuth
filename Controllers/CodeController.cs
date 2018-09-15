﻿using HashidsNet;
using MacAuth.ConfigModels;
using MacAuth.DbModels;
using MacAuth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Web;

namespace MacAuth.Controllers
{
    [Produces("application/json")]
    public class CodeController : Controller
    {
        private const int MAX_PARAMS = 10;
        private const int MAX_PARAM_LENGTH = 255;
        private readonly MacAuthContext _context;
        private readonly IOptions<MacAuthConfig> _config;
        private readonly IOptions<Clients> _clients;

        public CodeController(MacAuthContext context, IOptions<MacAuthConfig> config, IOptions<Clients> clients)
        {
            _context = context;
            _config = config;
            _clients = clients;
        }

        [HttpPost]
        public CodeResponse Index(string ma_provider, string ma_client_id)
        {
            string error = null;

            if(!ValidateRequest(ma_provider, ma_client_id, out error))
            {
                return new CodeResponse
                {
                    error = error
                };
            }

            var authRequest = new AuthRequest
            {
                Provider = ma_provider,
                ClientId = ma_client_id,
                DeviceCode = Guid.NewGuid().ToString().ToLower(),
                CreatedDate = DateTime.UtcNow,
                Status = AuthRequestStatus.Pending
            };

            authRequest.UserCode = this.Hashid.Encode(authRequest.Id).ToUpper();
            _context.AuthRequests.Add(authRequest);

            int count = 0;
            var queryParams = HttpUtility.ParseQueryString(Request.QueryString.ToString());

            foreach (var param in queryParams.AllKeys)
            {
                if (param != null &&
                    param != "ma_provider" &&
                    param != "ma_client_id")
                {
                    if(!ValidateParameter(count, param, queryParams[param], out error))
                    {
                        return new CodeResponse
                        {
                            error = error
                        };
                    }

                    _context.AuthRequestParams.Add(new AuthRequestParam
                    {
                        AuthRequestId = authRequest.Id,
                        Name = param,
                        Value = queryParams[param]
                    });

                    count++;

                    if (count > MAX_PARAMS)
                    {
                        return new CodeResponse
                        {
                            error = $"Number of parameters exceeds maximum of {MAX_PARAMS}."
                        };
                    }
                }
            }

            _context.SaveChanges();
            
            return new CodeResponse
            {
                device_code = authRequest.DeviceCode,
                user_code = authRequest.UserCode,
                expires_in = _config.Value.ExpirySeconds,
                verification_url = _config.Value.VerificationUrl
            };
        }

        private bool ValidateRequest(string ma_provider, string ma_client_id, out string error)
        {
            error = null;

            // Validate client id
            if (_clients.Value == null ||
                ma_client_id == null ||
                _clients.Value.FirstOrDefault(c => c.Id == ma_client_id) == null)
            {
                error = "Invalid Client ID.";
                return false;
            }

            // Validate provider
            if (_config.Value.Providers == null ||
                ma_provider == null ||
                _config.Value.Providers.FirstOrDefault(p => p.Id == ma_provider) == null)
            {
                error = "Invalid Provider.";
                return false;
            }

            return true;
        }

        private bool ValidateParameter(int index, string key, string value, out string error)
        {
            error = null;

            if (key.Length > MAX_PARAM_LENGTH)
            {
                error = $"Parameter {index} exceeds maximum length of {MAX_PARAM_LENGTH}.";
                return false;
            }

            if (value != null && value.Length > MAX_PARAM_LENGTH)
            {
                error = $"Parameter {key} value exceeds maximum length of {MAX_PARAM_LENGTH}.";
                return false;
            }

            return true;
        }

        private Hashids Hashid
        {
            get
            {
                var items = HttpContext.Items;
                if (!items.ContainsKey("hashids"))
                {
                    items["hashids"] = new Hashids(Guid.NewGuid().ToString(), 6);
                }
                return items["hashids"] as Hashids;
            }
        }
    }
}
