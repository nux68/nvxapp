using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Notifiche e preferenze dell'utente corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.4)
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : NvxControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(IHttpContextAccessor httpContextAccessor,
                                      INotificationService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        [Route("NotificationList")]
        public async Task<GenericResult<NotificationListOutModel>> NotificationList(GenericRequest<NotificationListInModel> inModel)
        {
            return await _service.NotificationList(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("NotificationCount")]
        public async Task<GenericResult<NotificationCountOutModel>> NotificationCount(GenericRequest<NotificationCountInModel> inModel)
        {
            return await _service.NotificationCount(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("NotificationRead")]
        public async Task<GenericResult<NotificationReadOutModel>> NotificationRead(GenericRequest<NotificationReadInModel> inModel)
        {
            return await _service.NotificationRead(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("NotificationDelete")]
        public async Task<GenericResult<NotificationDeleteOutModel>> NotificationDelete(GenericRequest<NotificationDeleteInModel> inModel)
        {
            return await _service.NotificationDelete(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("NotificationPreferenceGet")]
        public async Task<GenericResult<NotificationPreferenceGetOutModel>> NotificationPreferenceGet(GenericRequest<NotificationPreferenceGetInModel> inModel)
        {
            return await _service.NotificationPreferenceGet(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("NotificationPreferencePut")]
        public async Task<GenericResult<NotificationPreferencePutOutModel>> NotificationPreferencePut(GenericRequest<NotificationPreferencePutInModel> inModel)
        {
            return await _service.NotificationPreferencePut(inModel, false);
        }
    }
}
