using MacAuth.ConfigModels;
using MacAuth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;

namespace MacAuth.Controllers
{
    [Produces("application/json")]
    public class StatusController : Controller
    {
        private readonly MacAuthContext _context;
        private readonly IOptions<MacAuthConfig> _config;
        private readonly IOptions<Clients> _clients;

        public StatusController(MacAuthContext context, IOptions<MacAuthConfig> config, IOptions<Clients> clients)
        {
            _context = context;
            _config = config;
            _clients = clients;
        }

        [HttpPost]
        public StatusResponse Index(string ma_client_id, string device_code)
        {
            var authRequest = _context.AuthRequests.FirstOrDefault(a => a.ClientId == ma_client_id &&  a.DeviceCode == device_code);

            if(authRequest == null)
            {
                // TODO
            }

            return new StatusResponse
            {
                status = authRequest.Status,
                code = authRequest.Code,
                error = authRequest.Error
            };
        }
    }
}
