using MacAuth.ConfigModels;
using MacAuth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net;

namespace MacAuth.Controllers
{
    public class LoginController : Controller
    {
        private readonly MacAuthContext _context;
        private readonly IOptions<MacAuthConfig> _config;

        public LoginController(MacAuthContext context, IOptions<MacAuthConfig> config)
        {
            _context = context;
            _config = config;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginRequest loginRequest)
        {
            if (string.IsNullOrWhiteSpace(loginRequest.user_code))
            {
                ModelState.AddModelError("user_code", "Please enter a code.");
            }

            var authRequest = _context.AuthRequests.FirstOrDefault(a => a.UserCode == loginRequest.user_code.ToUpper());

            if(authRequest == null)
            {
                ModelState.AddModelError("user_code", "Sorry, your code was invalid.");
            }
            else
            { 
                if (_config.Value.Providers == null)
                {
                    ModelState.AddModelError("user_code", "Sorry, an error occurred (0).");
                }
                else
                {
                    // TODO: Validate expiry!

                    var provider = _config.Value.Providers.FirstOrDefault(p => p.Id == authRequest.Provider);

                    if (provider == null)
                    {
                        ModelState.AddModelError("user_code", "Sorry, an error occurred (1).");
                    }
                    else
                    {
                        // Build provider url
                        var redirectTo = $"https://{Request.Host.Host}/login/response";
                        var url = $"{provider.LoginUrl}?{provider.RedirectParam}={WebUtility.UrlEncode(redirectTo)}";

                        var authRequestParams = _context.AuthRequestParams.Where(p => p.AuthRequestId == authRequest.Id);

                        foreach (var param in authRequestParams)
                        {
                            if (param.Name.Equals(provider.RedirectParam, StringComparison.CurrentCultureIgnoreCase))
                            {
                                continue;
                            }

                            url += $"&{param.Name}={WebUtility.UrlEncode(param.Value)}";
                        }

                        return Redirect(url);
                    }
                }
            }

            return View();
        }
    }
}