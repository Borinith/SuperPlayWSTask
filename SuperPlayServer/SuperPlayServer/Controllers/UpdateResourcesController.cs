using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SuperPlayServer.Data;
using SuperPlayServer.DTOs;
using System.Threading.Tasks;

namespace SuperPlayServer.Controllers
{
    public class UpdateResourcesController : ControllerBase
    {
        private readonly SuperplayContext _context;
        private readonly ILogger<UpdateResourcesController> _logger;

        public UpdateResourcesController(SuperplayContext context, ILogger<UpdateResourcesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> UpdateResources(UpdateResourcesDTO updateResources)
        {
            var resource = await _context.Resources
                .Include(x => x.Device)
                .FirstOrDefaultAsync(x => x.Device.Id == updateResources.DeviceId && x.ResourceType == updateResources.ResourceType);

            if (resource is null)
            {
                var errorMessage = $"Resource with device id={updateResources.DeviceId} not found";
                _logger.LogError(errorMessage);

                return errorMessage;
            }

            var newResourceValue = resource.ResourceValue + updateResources.Value;

            resource.ResourceValue = newResourceValue;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"New value of resource is {newResourceValue}");

            return newResourceValue.ToString();
        }
    }
}