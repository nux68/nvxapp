using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Payments;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Payments.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.Infrastructure
{
    // Pagamenti delle licenze e ripartizione ai centri (ANALISI_INFRASTRUCTURE_PAGAMENTI_CENTRI.md): SuperUser e utenti del centro.
    // I ruoli li controlla il servizio.
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentReportController : NvxControllerBase
    {
        private readonly IPaymentReportService _service;

        public PaymentReportController(IHttpContextAccessor httpContextAccessor, IPaymentReportService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize, HttpPost, Route("PaymentReportGet")]
        public async Task<GenericResult<PaymentReportGetOutModel>> PaymentReportGet(GenericRequest<PaymentReportGetInModel> inModel)
            => await _service.PaymentReportGet(inModel, false);

        [Authorize, HttpPost, Route("PaymentDealerGet")]
        public async Task<GenericResult<PaymentDealerGetOutModel>> PaymentDealerGet(GenericRequest<PaymentDealerGetInModel> inModel)
            => await _service.PaymentDealerGet(inModel, false);

        [Authorize, HttpPost, Route("PaymentSettlementGenerate")]
        public async Task<GenericResult<PaymentSettlementGenerateOutModel>> PaymentSettlementGenerate(GenericRequest<PaymentSettlementGenerateInModel> inModel)
            => await _service.PaymentSettlementGenerate(inModel, false);

        [Authorize, HttpPost, Route("PaymentSettlementConfirm")]
        public async Task<GenericResult<PaymentSettlementConfirmOutModel>> PaymentSettlementConfirm(GenericRequest<PaymentSettlementConfirmInModel> inModel)
            => await _service.PaymentSettlementConfirm(inModel, false);

        [Authorize, HttpPost, Route("PaymentSettlementCancel")]
        public async Task<GenericResult<PaymentSettlementCancelOutModel>> PaymentSettlementCancel(GenericRequest<PaymentSettlementCancelInModel> inModel)
            => await _service.PaymentSettlementCancel(inModel, false);

        [Authorize, HttpPost, Route("PaymentSettlementPay")]
        public async Task<GenericResult<PaymentSettlementPayOutModel>> PaymentSettlementPay(GenericRequest<PaymentSettlementPayInModel> inModel)
            => await _service.PaymentSettlementPay(inModel, false);

        [Authorize, HttpPost, Route("PaymentSettlementInvoicePut")]
        public async Task<GenericResult<PaymentSettlementInvoicePutOutModel>> PaymentSettlementInvoicePut(GenericRequest<PaymentSettlementInvoicePutInModel> inModel)
            => await _service.PaymentSettlementInvoicePut(inModel, false);

        [Authorize, HttpPost, Route("PaymentSummarySend")]
        public async Task<GenericResult<PaymentSummarySendOutModel>> PaymentSummarySend(GenericRequest<PaymentSummarySendInModel> inModel)
            => await _service.PaymentSummarySend(inModel, false);

        [Authorize, HttpPost, Route("PaymentExport")]
        public async Task<GenericResult<PaymentExportOutModel>> PaymentExport(GenericRequest<PaymentExportInModel> inModel)
            => await _service.PaymentExport(inModel, false);

        [Authorize, HttpPost, Route("PaymentDealerConfigGet")]
        public async Task<GenericResult<PaymentDealerConfigGetOutModel>> PaymentDealerConfigGet(GenericRequest<PaymentDealerConfigGetInModel> inModel)
            => await _service.PaymentDealerConfigGet(inModel, false);

        [Authorize, HttpPost, Route("PaymentDealerConfigPut")]
        public async Task<GenericResult<PaymentDealerConfigPutOutModel>> PaymentDealerConfigPut(GenericRequest<PaymentDealerConfigPutInModel> inModel)
            => await _service.PaymentDealerConfigPut(inModel, false);

        [Authorize, HttpPost, Route("PaymentSettingsGet")]
        public async Task<GenericResult<PaymentSettingsGetOutModel>> PaymentSettingsGet(GenericRequest<PaymentSettingsGetInModel> inModel)
            => await _service.PaymentSettingsGet(inModel, false);

        [Authorize, HttpPost, Route("PaymentSettingsPut")]
        public async Task<GenericResult<PaymentSettingsPutOutModel>> PaymentSettingsPut(GenericRequest<PaymentSettingsPutInModel> inModel)
            => await _service.PaymentSettingsPut(inModel, false);
    }
}
