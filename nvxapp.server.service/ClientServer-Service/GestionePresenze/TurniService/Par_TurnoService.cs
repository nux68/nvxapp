using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService
{
    /*
     Modelli di turno e turni/coperture dei reparti (AT-TURNI.1). Scrittura: CompanyPowerAdmin (parametri);
     lettura: anche CompanyAdmin (pianificazione).
    */
    public class Par_TurnoService : ServiceBase, IPar_TurnoService
    {
        private readonly ITurniUtility _turniUtility;
        private readonly IPar_TurnoRepository _par_TurnoRepository;
        private readonly IAz_SediRepartoTurnoRepository _az_SediRepartoTurnoRepository;
        private readonly IDip_TurnoRepository _dip_TurnoRepository;
        private readonly IPar_OrarioRepository _par_OrarioRepository;

        public Par_TurnoService(IMapper mapper,
                                UserManager<ApplicationUser> userManager,
                                IAspNetUsersRepository aspNetUsersRepository,
                                IOptions<JwtParameter> jwtParameter,
                                IHttpContextAccessor httpContextAccessor,
                                IConfiguration configuration,

                                ITurniUtility turniUtility,
                                IPar_TurnoRepository par_TurnoRepository,
                                IAz_SediRepartoTurnoRepository az_SediRepartoTurnoRepository,
                                IDip_TurnoRepository dip_TurnoRepository,
                                IPar_OrarioRepository par_OrarioRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _turniUtility = turniUtility;
            _par_TurnoRepository = par_TurnoRepository;
            _az_SediRepartoTurnoRepository = az_SediRepartoTurnoRepository;
            _dip_TurnoRepository = dip_TurnoRepository;
            _par_OrarioRepository = par_OrarioRepository;
        }

        private async Task VerificaPowerAdmin()
        {
            if (!await _turniUtility.HaRuoloAsync(CurrentUserId, "CompanyPowerAdmin"))
                throw new UserMessageException(AttendanceTrackingMessages.Get("TurnoNonAutorizzato"));
        }

        private async Task VerificaAdmin()
        {
            if (!await _turniUtility.IsAdminAsync(CurrentUserId))
                throw new UserMessageException(AttendanceTrackingMessages.Get("TurnoNonAutorizzato"));
        }

        public virtual async Task<GenericResult<Par_Turno_GetAllOutModel>> GetAll(GenericRequest<Par_Turno_GetAllInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_Turno_GetAllOutModel();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                retVal.Par_Turno = _par_TurnoRepository.FindAll(t => t.IdAz_Anagrafica == idAzienda)
                                                       .OrderBy(t => t.Codice).ToList()
                                                       .Select(_turniUtility.Modello).ToList();
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Turno_GetOutModel>> Par_TurnoGet(GenericRequest<Par_Turno_GetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_Turno_GetOutModel();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var turno = _par_TurnoRepository.FindAll(t => t.Id == model.Data.Id && t.IdAz_Anagrafica == idAzienda).FirstOrDefault();
                retVal.Par_Turno = turno != null ? _turniUtility.Modello(turno) : new Par_TurnoModel { IdAz_Anagrafica = idAzienda };
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Turno_PutOutModel>> Par_TurnoPut(GenericRequest<Par_Turno_PutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await VerificaPowerAdmin();
                var retVal = new Par_Turno_PutOutModel();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var m = model.Data.Par_Turno;

                var codice = (m.Codice ?? string.Empty).Trim().ToUpperInvariant();
                if (codice.Length == 0 || string.IsNullOrWhiteSpace(m.Descrizione))
                    throw new UserMessageException(AttendanceTrackingMessages.Get("TurnoCodiceDescrizione"));
                if (_par_TurnoRepository.FindAll(t => t.IdAz_Anagrafica == idAzienda && t.Codice == codice && t.Id != m.Id).Any())
                    throw new UserMessageException(AttendanceTrackingMessages.Get("TurnoCodiceDuplicato", codice));
                if (m.IdPar_Orario is int idOrario && idOrario > 0 && !_par_OrarioRepository.FindAll(o => o.Id == idOrario && o.IdAz_Anagrafica == idAzienda).Any())
                    throw new UserMessageException(AttendanceTrackingMessages.Get("TurnoOrarioNonValido"));

                Par_Turno? turno = m.Id > 0 ? _par_TurnoRepository.FindAll(t => t.Id == m.Id && t.IdAz_Anagrafica == idAzienda).FirstOrDefault() : null;
                turno ??= new Par_Turno { IdAz_Anagrafica = idAzienda };
                turno.Codice = codice;
                turno.Descrizione = m.Descrizione!.Trim();
                turno.IdPar_Orario = m.IdPar_Orario > 0 ? m.IdPar_Orario : null;
                turno.Colore = string.IsNullOrWhiteSpace(m.Colore) ? null : m.Colore.Trim();
                turno = await _par_TurnoRepository.UpsertAsync(turno);

                retVal.Par_Turno = _turniUtility.Modello(turno);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Turno_DeleteOutModel>> Par_TurnoDelete(GenericRequest<Par_Turno_DeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await VerificaPowerAdmin();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var turno = _par_TurnoRepository.FindAll(t => t.Id == model.Data.Id && t.IdAz_Anagrafica == idAzienda).FirstOrDefault();
                if (turno != null)
                {
                    if (_dip_TurnoRepository.FindAll(d => d.IdPar_Turno == turno.Id).Any())
                        throw new UserMessageException(AttendanceTrackingMessages.Get("TurnoUsato", turno.Codice));
                    await _par_TurnoRepository.DeleteAsync(turno);
                }
                return new Par_Turno_DeleteOutModel();
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_SediRepartoTurno_GetOutModel>> Coperture4Reparto(GenericRequest<Az_SediRepartoTurno_GetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await VerificaAdmin();
                var retVal = new Az_SediRepartoTurno_GetOutModel { IdAz_SediReparto = model.Data.IdAz_SediReparto };
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                if (_turniUtility.Reparto(model.Data.IdAz_SediReparto, idAzienda) == null)
                    return retVal;
                retVal.Az_SediRepartoTurno = _mapper.Map<List<Az_SediRepartoTurnoModel>>(
                    _az_SediRepartoTurnoRepository.FindAll(x => x.IdAz_SediReparto == model.Data.IdAz_SediReparto).ToList());
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_SediRepartoTurno_PutOutModel>> Coperture4RepartoPut(GenericRequest<Az_SediRepartoTurno_PutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await VerificaPowerAdmin();
                var retVal = new Az_SediRepartoTurno_PutOutModel();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                int idReparto = model.Data.IdAz_SediReparto;
                if (_turniUtility.Reparto(idReparto, idAzienda) == null)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("TurnoRepartoNonValido"));

                var idTurniAzienda = _par_TurnoRepository.FindAll(t => t.IdAz_Anagrafica == idAzienda).Select(t => t.Id).ToList();
                var esistenti = _az_SediRepartoTurnoRepository.FindAll(x => x.IdAz_SediReparto == idReparto).ToList();
                var nuove = model.Data.Az_SediRepartoTurno.Where(r => idTurniAzienda.Contains(r.IdPar_Turno))
                                                          .GroupBy(r => r.IdPar_Turno).Select(g => g.Last()).ToList();

                await _az_SediRepartoTurnoRepository.DeleteRangeAsync(esistenti.Where(e => !nuove.Any(n => n.IdPar_Turno == e.IdPar_Turno)).ToList());
                foreach (var n in nuove)
                {
                    var riga = esistenti.FirstOrDefault(e => e.IdPar_Turno == n.IdPar_Turno)
                               ?? new Az_SediRepartoTurno { IdAz_SediReparto = idReparto, IdPar_Turno = n.IdPar_Turno };
                    riga.MinPersone = Math.Max(0, n.MinPersone);
                    riga.GiorniSettimana = MascheraGiorni(n.GiorniSettimana);
                    retVal.Az_SediRepartoTurno.Add(_mapper.Map<Az_SediRepartoTurnoModel>(await _az_SediRepartoTurnoRepository.UpsertAsync(riga)));
                }
                return retVal;
            }, isSubProcess);
        }

        // maschera lun..dom di 7 caratteri 0/1 (vuota o non valida = tutti i giorni)
        public static string MascheraGiorni(string? maschera)
        {
            if (string.IsNullOrEmpty(maschera) || maschera.Length != 7 || maschera.Any(c => c != '0' && c != '1'))
                return "1111111";
            return maschera;
        }

        // indice lun=0..dom=6 nella maschera
        public static bool GiornoNellaMaschera(string? maschera, DateTime giorno)
        {
            var m = MascheraGiorni(maschera);
            int idx = giorno.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)giorno.DayOfWeek - 1;
            return m[idx] == '1';
        }
    }

    public interface IPar_TurnoService : IServiceBase
    {
        Task<GenericResult<Par_Turno_GetAllOutModel>> GetAll(GenericRequest<Par_Turno_GetAllInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Turno_GetOutModel>> Par_TurnoGet(GenericRequest<Par_Turno_GetInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Turno_PutOutModel>> Par_TurnoPut(GenericRequest<Par_Turno_PutInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Turno_DeleteOutModel>> Par_TurnoDelete(GenericRequest<Par_Turno_DeleteInModel> model, bool isSubProcess);
        Task<GenericResult<Az_SediRepartoTurno_GetOutModel>> Coperture4Reparto(GenericRequest<Az_SediRepartoTurno_GetInModel> model, bool isSubProcess);
        Task<GenericResult<Az_SediRepartoTurno_PutOutModel>> Coperture4RepartoPut(GenericRequest<Az_SediRepartoTurno_PutInModel> model, bool isSubProcess);
    }
}
