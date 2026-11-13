using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Common;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimbraturaEvolutaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using QRCoder;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TimbraturaEvolutaService
{
    /*
     Timbratura evoluta (AT-TIMB, ANALISI_ATTENDANCETRACKING_TIMBRATURA.md §4):
       - configurazione per sede (Az_SediTimbratura): QR, geofence, kiosk, limite delle differite
       - Stamp: timbratura dal telefono con QR (facoltativo o obbligatorio), posizione (geofence) e invio differito (offline)
       - QrCode: codice firmato e rotante della postazione, con l'immagine del QR
       - kiosk: registrazione del dispositivo e timbratura con PIN o badge del dipendente
     La timbratura creata e' una normale Dip_GG_Timbratura (Dip_GG_TimbraturaPut); come e' nata sta in Dip_GG_TimbraturaInfo.
    */
    public class TimbraturaEvolutaService : ServiceBase, ITimbraturaEvolutaService
    {
        private static readonly ConcurrentDictionary<string, (int Errori, DateTime Inizio, DateTime BloccatoFino)> _tentativiKiosk = new();
        private const int MaxErroriKiosk = 5;
        // sblocco dei terminali (postazione QR e kiosk) con la password di un CompanyPowerAdmin: stesso limite del kiosk, per utente collegato
        private static readonly ConcurrentDictionary<string, (int Errori, DateTime Inizio, DateTime BloccatoFino)> _tentativiSblocco = new();
        private const double MaxPrecisioneMetri = 100;
        // validita' del ticket del QR (Opzioni): il tempo per scegliere reparto e attivita'
        private static readonly TimeSpan DurataTicket = TimeSpan.FromMinutes(3);

        private readonly ITurniUtility _turniUtility;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IDip_GG_TimbraturaService _dip_GG_TimbraturaService;
        private readonly IDip_GG_TimbraturaRepository _dip_GG_TimbraturaRepository;
        private readonly IAz_SediTimbraturaRepository _az_SediTimbraturaRepository;
        private readonly IDip_GG_TimbraturaInfoRepository _dip_GG_TimbraturaInfoRepository;
        private readonly IDip_TimbraturaPinRepository _dip_TimbraturaPinRepository;
        private readonly IAz_SediCommonRepository _az_SediRepository;
        private readonly IAz_SediRepartoCommonRepository _az_SediRepartoRepository;
        private readonly IAz_SediRepartoUserRepository _az_SediRepartoUserRepository;
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        private readonly ILavoroAgileGiorno _lavoroAgileGiorno;
        private readonly IUserCompanyRepository _userCompanyRepository;
        private readonly IAz_SubCommessaAttivitaService _az_SubCommessaAttivitaService;
        private readonly IAz_SediRepartoAttivitaRepository _az_SediRepartoAttivitaRepository;
        private readonly IPar_AttivitaCompetenzaRepository _par_AttivitaCompetenzaRepository;
        private readonly IDip_CompetenzaCommonRepository _dip_CompetenzaRepository;

        public TimbraturaEvolutaService(IMapper mapper,
                                        UserManager<ApplicationUser> userManager,
                                        IAspNetUsersRepository aspNetUsersRepository,
                                        IOptions<JwtParameter> jwtParameter,
                                        IHttpContextAccessor httpContextAccessor,
                                        IConfiguration configuration,

                                        ITurniUtility turniUtility,
                                        IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                        IDip_GG_TimbraturaService dip_GG_TimbraturaService,
                                        IDip_GG_TimbraturaRepository dip_GG_TimbraturaRepository,
                                        IAz_SediTimbraturaRepository az_SediTimbraturaRepository,
                                        IDip_GG_TimbraturaInfoRepository dip_GG_TimbraturaInfoRepository,
                                        IDip_TimbraturaPinRepository dip_TimbraturaPinRepository,
                                        IAz_SediCommonRepository az_SediRepository,
                                        IAz_SediRepartoCommonRepository az_SediRepartoRepository,
                                        IAz_SediRepartoUserRepository az_SediRepartoUserRepository,
                                        IDip_AnagraficaRepository dip_AnagraficaRepository,
                                        IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                        ILavoroAgileGiorno lavoroAgileGiorno,
                                        IUserCompanyRepository userCompanyRepository,
                                        IAz_SubCommessaAttivitaService az_SubCommessaAttivitaService,
                                        IAz_SediRepartoAttivitaRepository az_SediRepartoAttivitaRepository,
                                        IPar_AttivitaCompetenzaRepository par_AttivitaCompetenzaRepository,
                                        IDip_CompetenzaCommonRepository dip_CompetenzaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _turniUtility = turniUtility;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _dip_GG_TimbraturaService = dip_GG_TimbraturaService;
            _dip_GG_TimbraturaRepository = dip_GG_TimbraturaRepository;
            _az_SediTimbraturaRepository = az_SediTimbraturaRepository;
            _dip_GG_TimbraturaInfoRepository = dip_GG_TimbraturaInfoRepository;
            _dip_TimbraturaPinRepository = dip_TimbraturaPinRepository;
            _az_SediRepository = az_SediRepository;
            _az_SediRepartoRepository = az_SediRepartoRepository;
            _az_SediRepartoUserRepository = az_SediRepartoUserRepository;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _lavoroAgileGiorno = lavoroAgileGiorno;
            _userCompanyRepository = userCompanyRepository;
            _az_SubCommessaAttivitaService = az_SubCommessaAttivitaService;
            _az_SediRepartoAttivitaRepository = az_SediRepartoAttivitaRepository;
            _par_AttivitaCompetenzaRepository = par_AttivitaCompetenzaRepository;
            _dip_CompetenzaRepository = dip_CompetenzaRepository;
        }

        private static string Msg(string key, params object?[] args) => AttendanceTrackingMessages.Get(key, args);

        private async Task VerificaAdmin(bool powerAdmin = false)
        {
            bool ok = powerAdmin ? await _turniUtility.HaRuoloAsync(CurrentUserId, "CompanyPowerAdmin") : await _turniUtility.IsAdminAsync(CurrentUserId);
            if (!ok)
                throw new UserMessageException(Msg("TimbNonAutorizzato"));
        }

        private Az_Sedi SedeAzienda(int idAz_Sedi, int idAzienda)
            => _az_SediRepository.FindAll(s => s.Id == idAz_Sedi && s.IdAz_Anagrafica == idAzienda).FirstOrDefault()
               ?? throw new UserMessageException(Msg("TimbSedeNonValida"));

        private Az_SediTimbraturaModel Modello(Az_Sedi sede, Az_SediTimbratura? c) => new()
        {
            IdAz_Sedi = sede.Id,
            Sede = sede.Descrizione,
            Latitudine = c?.Latitudine,
            Longitudine = c?.Longitudine,
            RaggioMetri = c?.RaggioMetri ?? 200,
            RegolaGeofence = c?.RegolaGeofence ?? RegolaGeofence.Nessuna,
            QrAttivo = c?.QrAttivo ?? false,
            QrObbligatorio = c?.QrObbligatorio ?? false,
            QrDurataSecondi = c?.QrDurataSecondi ?? 30,
            KioskAttivo = c?.KioskAttivo ?? false,
            DifferitaMaxOre = c?.DifferitaMaxOre ?? 24,
            KioskRegistrato = !string.IsNullOrEmpty(c?.KioskToken)
        };

        private static string NuovoSegreto() => Base64Url(RandomNumberGenerator.GetBytes(32));

        private static string Base64Url(byte[] b) => Convert.ToBase64String(b).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        // ── configurazione ────────────────────────────────────────────────

        public virtual async Task<GenericResult<TimbraturaEvoluta_SediOutModel>> Sedi(GenericRequest<TimbraturaEvoluta_SediInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TimbraturaEvoluta_SediOutModel();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var sedi = _az_SediRepository.FindAll(s => s.IdAz_Anagrafica == idAzienda).OrderBy(s => s.Descrizione).ToList();
                var idSedi = sedi.Select(s => s.Id).ToList();
                var config = _az_SediTimbraturaRepository.FindAll(c => idSedi.Contains(c.IdAz_Sedi)).ToList();
                retVal.Sedi = sedi.Select(s => Modello(s, config.FirstOrDefault(c => c.IdAz_Sedi == s.Id))).ToList();
                // le coordinate servono solo all'HR
                if (!await _turniUtility.IsAdminAsync(CurrentUserId))
                    foreach (var s in retVal.Sedi) { s.Latitudine = null; s.Longitudine = null; }
                retVal.IdAz_Sedi_Proposta = SedeDelDipendente(CurrentUserId, idSedi);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<TimbraturaEvoluta_ConfigGetOutModel>> ConfigGet(GenericRequest<TimbraturaEvoluta_ConfigGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await VerificaAdmin();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var sede = SedeAzienda(model.Data.IdAz_Sedi, idAzienda);
                var c = _az_SediTimbraturaRepository.FindAll(x => x.IdAz_Sedi == sede.Id).FirstOrDefault();
                return new TimbraturaEvoluta_ConfigGetOutModel { Config = Modello(sede, c) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<TimbraturaEvoluta_ConfigPutOutModel>> ConfigPut(GenericRequest<TimbraturaEvoluta_ConfigPutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await VerificaAdmin(powerAdmin: true);
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var m = model.Data.Config;
                var sede = SedeAzienda(m.IdAz_Sedi, idAzienda);

                if ((m.Latitudine == null) != (m.Longitudine == null) ||
                    (m.Latitudine is double lat && (lat < -90 || lat > 90)) || (m.Longitudine is double lon && (lon < -180 || lon > 180)))
                    throw new UserMessageException(Msg("TimbCoordinateNonValide"));
                if (m.RegolaGeofence != RegolaGeofence.Nessuna && m.Latitudine == null)
                    throw new UserMessageException(Msg("TimbGeofenceSenzaCoordinate"));

                var c = _az_SediTimbraturaRepository.FindAll(x => x.IdAz_Sedi == sede.Id).FirstOrDefault() ?? new Az_SediTimbratura { IdAz_Sedi = sede.Id };
                c.Latitudine = m.Latitudine;
                c.Longitudine = m.Longitudine;
                c.RaggioMetri = Math.Clamp(m.RaggioMetri, 10, 100000);
                c.RegolaGeofence = m.RegolaGeofence;
                c.QrAttivo = m.QrAttivo;
                c.QrObbligatorio = m.QrAttivo && m.QrObbligatorio;
                c.QrDurataSecondi = Math.Clamp(m.QrDurataSecondi, 10, 600);
                c.KioskAttivo = m.KioskAttivo;
                c.DifferitaMaxOre = Math.Clamp(m.DifferitaMaxOre, 0, 24 * 31);
                if (c.QrAttivo && (string.IsNullOrEmpty(c.QrSegreto) || m.RigeneraSegretoQr))
                    c.QrSegreto = NuovoSegreto();
                if (!c.KioskAttivo)
                    c.KioskToken = null;
                c = await _az_SediTimbraturaRepository.UpsertAsync(c);
                return new TimbraturaEvoluta_ConfigPutOutModel { Config = Modello(sede, c) };
            }, isSubProcess);
        }

        // ── QR ────────────────────────────────────────────────────────────

        public static long Slot(DateTime ora, int durataSecondi)
            => new DateTimeOffset(ora).ToUnixTimeSeconds() / Math.Max(1, durataSecondi);

        public static string Firma(string segreto, int idAz_Sedi, long slot)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(segreto));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{idAz_Sedi}.{slot}"));
            return Base64Url(hash[..16]);
        }

        public static string CodiceQr(string segreto, int idAz_Sedi, long slot) => $"{idAz_Sedi}.{slot}.{Firma(segreto, idAz_Sedi, slot)}";

        public static (int IdAz_Sedi, long Slot, string Firma)? LeggiCodice(string? codice)
        {
            var parti = (codice ?? string.Empty).Trim().Split('.');
            if (parti.Length != 3 || !int.TryParse(parti[0], out var idSede) || !long.TryParse(parti[1], out var slot) || parti[2].Length == 0)
                return null;
            return (idSede, slot, parti[2]);
        }

        // ticket del QR (AT-TIMB.7): "t.<sede>.<slot>.<scadenza unix>.<firma>", firmato col segreto della sede e legato all'utente
        public static string FirmaTicket(string segreto, string idAspNetUsers, int idAz_Sedi, long slot, long scadenza)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(segreto));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"t|{idAspNetUsers}|{idAz_Sedi}|{slot}|{scadenza}"));
            return Base64Url(hash[..16]);
        }

        public static string Ticket(string segreto, string idAspNetUsers, int idAz_Sedi, long slot, long scadenza)
            => $"t.{idAz_Sedi}.{slot}.{scadenza}.{FirmaTicket(segreto, idAspNetUsers, idAz_Sedi, slot, scadenza)}";

        public static (int IdAz_Sedi, long Slot, long Scadenza, string Firma)? LeggiTicket(string? ticket)
        {
            var parti = (ticket ?? string.Empty).Trim().Split('.');
            if (parti.Length != 5 || parti[0] != "t" || !int.TryParse(parti[1], out var idSede) || !long.TryParse(parti[2], out var slot) ||
                !long.TryParse(parti[3], out var scadenza) || parti[4].Length == 0)
                return null;
            return (idSede, slot, scadenza, parti[4]);
        }

        // codice del QR valido ora (firma, slot corrente o precedente) e non ancora usato dal dipendente
        private void VerificaCodiceQr(Az_SediTimbratura? config, (int IdAz_Sedi, long Slot, string Firma) qr, DateTime ora, int idDip_RapportoLavoro)
        {
            if (config == null || !config.QrAttivo || string.IsNullOrEmpty(config.QrSegreto))
                throw new UserMessageException(Msg("TimbQrNonAttivo"));
            var atteso = Firma(config.QrSegreto, qr.IdAz_Sedi, qr.Slot);
            if (!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(atteso), Encoding.ASCII.GetBytes(qr.Firma)))
                throw new UserMessageException(Msg("TimbQrNonValido"));
            var slotOra = Slot(ora, config.QrDurataSecondi);
            if (qr.Slot > slotOra || qr.Slot < slotOra - 1)
                throw new UserMessageException(Msg("TimbQrScaduto"));
            VerificaQrNonUsato(qr.IdAz_Sedi, qr.Slot, idDip_RapportoLavoro);
        }

        private void VerificaQrNonUsato(int idAz_Sedi, long slot, int idDip_RapportoLavoro)
        {
            if (_dip_GG_TimbraturaInfoRepository.FindAll(i => i.IdAz_Sedi == idAz_Sedi && i.QrSlot == slot && i.IdDip_RapportoLavoro == idDip_RapportoLavoro).Any())
                throw new UserMessageException(Msg("TimbQrGiaUsato"));
        }

        public virtual async Task<GenericResult<TimbraturaEvoluta_QrCodeOutModel>> QrCode(GenericRequest<TimbraturaEvoluta_QrCodeInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await VerificaAdmin();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var sede = SedeAzienda(model.Data.IdAz_Sedi, idAzienda);
                var c = _az_SediTimbraturaRepository.FindAll(x => x.IdAz_Sedi == sede.Id).FirstOrDefault();
                if (c == null || !c.QrAttivo || string.IsNullOrEmpty(c.QrSegreto))
                    throw new UserMessageException(Msg("TimbQrNonAttivo"));

                var ora = DateTime.Now;
                var slot = Slot(ora, c.QrDurataSecondi);
                var retVal = new TimbraturaEvoluta_QrCodeOutModel
                {
                    Codice = CodiceQr(c.QrSegreto, sede.Id, slot),
                    DurataSecondi = c.QrDurataSecondi,
                    ScadeTraSecondi = (int)((slot + 1) * c.QrDurataSecondi - new DateTimeOffset(ora).ToUnixTimeSeconds())
                };
                var origine = (model.Data.Origine ?? string.Empty).Trim().TrimEnd('/');
                retVal.Url = origine.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || origine.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                    ? $"{origine}/timbraturaqr?c={Uri.EscapeDataString(retVal.Codice)}"
                    : retVal.Codice;

                using var generator = new QRCodeGenerator();
                using var data = generator.CreateQrCode(retVal.Url, QRCodeGenerator.ECCLevel.M);
                retVal.ImmaginePng = Convert.ToBase64String(new PngByteQRCode(data).GetGraphic(8));
                return retVal;
            }, isSubProcess);
        }

        // ── timbratura ────────────────────────────────────────────────────

        // sede del primo reparto in cui l'utente lavora (null se nessuno o non dell'azienda)
        private int? SedeDelDipendente(string idAspNetUsers, List<int> idSediAzienda)
        {
            var reparti = _az_SediRepartoUserRepository.FindAll(x => x.IdAspNetUsers == idAspNetUsers && x.UserInDepartment &&
                                                                           (x.DataDal == null || x.DataDal <= DateTime.Today) && (x.DataAl == null || x.DataAl >= DateTime.Today))
                                                       .Select(x => x.IdAz_SediReparto).ToList();
            var sedi = _az_SediRepartoRepository.FindAll(r => reparti.Contains(r.Id)).Select(r => r.IdAz_Sedi).ToList();
            return sedi.FirstOrDefault(s => idSediAzienda.Contains(s)) is int s && s > 0 ? s : null;
        }

        // distanza in metri tra due punti (formula dell'emisenoverso)
        public static double Distanza(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000;
            double rad(double g) => g * Math.PI / 180;
            var dLat = rad(lat2 - lat1);
            var dLon = rad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(rad(lat1)) * Math.Cos(rad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return 2 * R * Math.Asin(Math.Min(1, Math.Sqrt(a)));
        }

        private class Esito
        {
            public DateTime Ora;
            public int? IdAz_Sedi;
            public int? IdAz_SediReparto;
            public long? QrSlot;
            public EsitoGeofence Geofence = EsitoGeofence.NonRichiesto;
            public double? Distanza;
            public double? Latitudine, Longitudine, Precisione;
            public bool Differita;
            public DateTime? DataDispositivo;
            public string? Avviso;
        }

        public virtual async Task<GenericResult<TimbraturaEvoluta_StampOutModel>> Stamp(GenericRequest<TimbraturaEvoluta_StampInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TimbraturaEvoluta_StampOutModel();
                var m = model.Data;
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var dip = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(CurrentUserId, true);
                if (dip?.dip_RapportoLavoro == null || dip.dip_Anagrafica == null)
                    throw new UserMessageException(Msg("EmploymentNotFound"));
                var rapporto = dip.dip_RapportoLavoro;
                var adesso = DateTime.Now;
                var esito = new Esito
                {
                    Ora = new DateTime(adesso.Year, adesso.Month, adesso.Day, adesso.Hour, adesso.Minute, 0),
                    Latitudine = m.Latitudine,
                    Longitudine = m.Longitudine,
                    Precisione = m.PrecisioneMetri
                };

                var idSediAzienda = _az_SediRepository.FindAll(s => s.IdAz_Anagrafica == idAzienda).Select(s => s.Id).ToList();

                // sede: quella del ticket o del QR, quella indicata o quella del reparto del dipendente
                var ticket = string.IsNullOrWhiteSpace(m.Ticket) ? null : LeggiTicket(m.Ticket);
                if (!string.IsNullOrWhiteSpace(m.Ticket) && ticket == null)
                    throw new UserMessageException(Msg("TimbQrNonValido"));
                var qr = ticket != null || string.IsNullOrWhiteSpace(m.Codice) ? null : LeggiCodice(m.Codice);
                if (ticket == null && !string.IsNullOrWhiteSpace(m.Codice) && qr == null)
                    throw new UserMessageException(Msg("TimbQrNonValido"));
                esito.IdAz_Sedi = ticket?.IdAz_Sedi ?? qr?.IdAz_Sedi ?? m.IdAz_Sedi ?? SedeDelDipendente(CurrentUserId, idSediAzienda);
                if (esito.IdAz_Sedi != null && !idSediAzienda.Contains(esito.IdAz_Sedi.Value))
                    throw new UserMessageException(ticket != null || qr != null ? Msg("TimbQrNonValido") : Msg("TimbSedeNonValida"));
                var config = esito.IdAz_Sedi != null ? _az_SediTimbraturaRepository.FindAll(c => c.IdAz_Sedi == esito.IdAz_Sedi).FirstOrDefault() : null;

                // offline: vale l'ora del dispositivo, entro il limite della sede (senza configurazione 24 ore)
                if (m.Offline)
                {
                    if (!DateTime.TryParse(m.DataDispositivo, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var oraDispositivo))
                        throw new UserMessageException(Msg("TimbDataDispositivoNonValida"));
                    int maxOre = config?.DifferitaMaxOre ?? 24;
                    if (maxOre <= 0)
                        throw new UserMessageException(Msg("TimbDifferitaNonAccettata"));
                    if (oraDispositivo > adesso.AddMinutes(5))
                        throw new UserMessageException(Msg("TimbDifferitaFutura"));
                    if (adesso - oraDispositivo > TimeSpan.FromHours(maxOre))
                        throw new UserMessageException(Msg("TimbDifferitaOltreLimite", maxOre));
                    esito.Ora = new DateTime(oraDispositivo.Year, oraDispositivo.Month, oraDispositivo.Day, oraDispositivo.Hour, oraDispositivo.Minute, 0);
                    esito.Differita = true;
                    esito.DataDispositivo = oraDispositivo;
                }

                // QR firmato: slot dell'ora della timbratura o il precedente, una sola volta per dipendente
                var oraRiferimento = esito.Differita ? esito.DataDispositivo!.Value : adesso;
                if (ticket != null)
                {
                    // ticket di Opzioni: il codice e' gia' stato verificato alla lettura, qui firma (utente compreso) e scadenza
                    if (config == null || !config.QrAttivo || string.IsNullOrEmpty(config.QrSegreto))
                        throw new UserMessageException(Msg("TimbQrNonAttivo"));
                    var atteso = FirmaTicket(config.QrSegreto, CurrentUserId, ticket.Value.IdAz_Sedi, ticket.Value.Slot, ticket.Value.Scadenza);
                    if (!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(atteso), Encoding.ASCII.GetBytes(ticket.Value.Firma)))
                        throw new UserMessageException(Msg("TimbQrNonValido"));
                    // timbratura entro la validita' del ticket; differita: l'ora del dispositivo non puo' precedere l'emissione del ticket
                    var oraTicket = new DateTimeOffset(oraRiferimento).ToUnixTimeSeconds();
                    if (oraTicket > ticket.Value.Scadenza || oraTicket < ticket.Value.Scadenza - (long)DurataTicket.TotalSeconds - 60)
                        throw new UserMessageException(Msg("TimbQrScaduto"));
                    VerificaQrNonUsato(ticket.Value.IdAz_Sedi, ticket.Value.Slot, rapporto.Id);
                    esito.QrSlot = ticket.Value.Slot;
                }
                else if (qr != null)
                {
                    VerificaCodiceQr(config, qr.Value, oraRiferimento, rapporto.Id);
                    esito.QrSlot = qr.Value.Slot;
                }
                else if (config?.QrObbligatorio == true)
                    throw new UserMessageException(Msg("TimbQrObbligatorio"));

                // geofence (non nei giorni di lavoro agile approvati)
                if (config != null && config.RegolaGeofence != RegolaGeofence.Nessuna && config.Latitudine != null && config.Longitudine != null)
                {
                    if (_lavoroAgileGiorno.IsGiornoLavoroAgile(rapporto.Id, esito.Ora.Date))
                        esito.Geofence = EsitoGeofence.LavoroAgile;
                    else if (m.Latitudine == null || m.Longitudine == null)
                        esito.Geofence = EsitoGeofence.PosizioneMancante;
                    else
                    {
                        esito.Distanza = Math.Round(Distanza(config.Latitudine.Value, config.Longitudine.Value, m.Latitudine.Value, m.Longitudine.Value), 1);
                        var tolleranza = Math.Min(Math.Max(0, m.PrecisioneMetri ?? 0), MaxPrecisioneMetri);
                        esito.Geofence = esito.Distanza <= config.RaggioMetri + tolleranza ? EsitoGeofence.Dentro : EsitoGeofence.Fuori;
                    }

                    if (esito.Geofence == EsitoGeofence.Fuori || esito.Geofence == EsitoGeofence.PosizioneMancante)
                    {
                        var testo = esito.Geofence == EsitoGeofence.Fuori ? Msg("TimbFuoriGeofence", (int)esito.Distanza!.Value, config.RaggioMetri) : Msg("TimbPosizioneMancante");
                        if (config.RegolaGeofence == RegolaGeofence.Blocca)
                            throw new UserMessageException(testo);
                        esito.Avviso = testo;
                    }
                }

                // reparto-attivita': una delle opzioni della sede (AT-TIMB.7); senza sede l'attivita' consentita (come la timbratura semplice)
                int idAttivita;
                if (esito.IdAz_Sedi != null)
                {
                    var (opzioni, indiceDefault) = await OpzioniAsync(dip.dip_Anagrafica, rapporto.Id, esito.IdAz_Sedi.Value);
                    var scelta = Scelta(opzioni, indiceDefault, m.IdAz_SubCommessaAttivita, m.IdAz_SediReparto);
                    idAttivita = scelta.IdAz_SubCommessaAttivita;
                    esito.IdAz_SediReparto = scelta.IdAz_SediReparto;
                }
                else
                {
                    idAttivita = await _gestionePresenzeUserUtility.GetIdAz_SubCommessaAttivita_4Timbratura(rapporto.Id, m.IdAz_SubCommessaAttivita);
                    var nonConsentita = await _gestionePresenzeUserUtility.VerificaAttivitaConsentita(rapporto.Id, idAttivita);
                    if (nonConsentita != null)
                        throw new UserMessageException(nonConsentita);
                }

                retVal.Info = await Registra(rapporto, idAttivita, esito, esito.QrSlot != null ? OrigineTimbratura.Qr : OrigineTimbratura.App, null);
                if (esito.Avviso != null)
                    retVal.Messages.Add(new Message(esito.Avviso, MessageType.Warning));
                return retVal;
            }, isSubProcess);
        }

        // ── reparto e attivita' (AT-TIMB.7) ───────────────────────────────

        // opzioni dell'utente collegato: dal codice del QR (verificato, con il ticket per lo Stamp) o dalla sede indicata/del dipendente
        public virtual async Task<GenericResult<TimbraturaEvoluta_OpzioniOutModel>> Opzioni(GenericRequest<TimbraturaEvoluta_OpzioniInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TimbraturaEvoluta_OpzioniOutModel();
                var m = model.Data;
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var dip = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(CurrentUserId, true);
                if (dip?.dip_RapportoLavoro == null || dip.dip_Anagrafica == null)
                    throw new UserMessageException(Msg("EmploymentNotFound"));
                var idSediAzienda = _az_SediRepository.FindAll(s => s.IdAz_Anagrafica == idAzienda).Select(s => s.Id).ToList();

                if (!string.IsNullOrWhiteSpace(m.Codice))
                {
                    var qr = LeggiCodice(m.Codice);
                    if (qr == null || !idSediAzienda.Contains(qr.Value.IdAz_Sedi))
                        throw new UserMessageException(Msg("TimbQrNonValido"));
                    var config = _az_SediTimbraturaRepository.FindAll(c => c.IdAz_Sedi == qr.Value.IdAz_Sedi).FirstOrDefault();
                    var adesso = DateTime.Now;
                    VerificaCodiceQr(config, qr.Value, adesso, dip.dip_RapportoLavoro.Id);
                    var scadenza = new DateTimeOffset(adesso.Add(DurataTicket)).ToUnixTimeSeconds();
                    retVal.Ticket = Ticket(config!.QrSegreto!, CurrentUserId, qr.Value.IdAz_Sedi, qr.Value.Slot, scadenza);
                    retVal.IdAz_Sedi = qr.Value.IdAz_Sedi;
                }
                else
                {
                    retVal.IdAz_Sedi = m.IdAz_Sedi ?? SedeDelDipendente(CurrentUserId, idSediAzienda);
                    if (retVal.IdAz_Sedi != null && !idSediAzienda.Contains(retVal.IdAz_Sedi.Value))
                        throw new UserMessageException(Msg("TimbSedeNonValida"));
                }

                if (retVal.IdAz_Sedi != null)
                    (retVal.Opzioni, retVal.IndiceDefault) = await OpzioniAsync(dip.dip_Anagrafica, dip.dip_RapportoLavoro.Id, retVal.IdAz_Sedi.Value);
                return retVal;
            }, isSubProcess);
        }

        /*
         Reparto - attivita' della sede compatibili con il dipendente (ANALISI_ATTENDANCETRACKING_TIMBRATURA.md §4.6):
           - attivita' consentite al dipendente (sub commesse sue o dei suoi reparti) per cui ha le competenze
             (almeno una di quelle richieste; senza competenze richieste va bene per tutti, come Az_SubCommessa/GetCompatibili)
           - per ogni reparto della sede: le attivita' che vi si svolgono (Az_SediRepartoAttivita); un reparto senza attivita'
             configurate solo se il dipendente ne fa parte, con tutte le sue attivita' compatibili
           - nessuna coppia: le attivita' compatibili senza reparto; ancora nessuna: l'attivita' proposta (non si blocca mai)
         Default: l'attivita' proposta (ultima timbratura, rapporto, prima consentita) se c'e', altrimenti la prima opzione.
        */
        private async Task<(List<TimbraturaOpzioneModel> Opzioni, int IndiceDefault)> OpzioniAsync(Dip_Anagrafica anagrafica, int idDip_RapportoLavoro, int idAz_Sedi)
        {
            var oggi = DateTime.Today;
            var idAspNetUsers = anagrafica.IdAspNetUsers ?? string.Empty;

            var elenco = (await _az_SubCommessaAttivitaService.GetAll_4FullList(new GenericRequest<Az_SubCommessaAttivita_GetAll_4FullList_InModel>(), true)).Data?.Az_SubCommessaAttivita
                         ?? new List<Az_SubCommessaAttivita_4FullListModel>();
            var consentite = _gestionePresenzeUserUtility.GetIdAz_SubCommessaAttivita_Consentite(idAspNetUsers);
            var attivita = elenco.Where(a => consentite.Contains(a.SubCommessaAttivita_Id)).ToList();

            var idPar_Attivita = attivita.Select(a => a.SubCommessaAttivita_IdPar_Attivita).Distinct().ToList();
            var richieste = _par_AttivitaCompetenzaRepository.FindAll(x => idPar_Attivita.Contains(x.IdPar_Attivita)).ToList();
            var competenze = _dip_CompetenzaRepository.FindAll(x => x.IdDip_Anagrafica == anagrafica.Id).Select(x => x.IdPar_Competenza).ToList();
            bool Compatibile(int idPar)
            {
                var r = richieste.Where(x => x.IdPar_Attivita == idPar).Select(x => x.IdPar_Competenza).ToList();
                return r.Count == 0 || r.Any(competenze.Contains);
            }
            attivita = attivita.Where(a => Compatibile(a.SubCommessaAttivita_IdPar_Attivita)).ToList();

            var reparti = _az_SediRepartoRepository.FindAll(r => r.IdAz_Sedi == idAz_Sedi).OrderBy(r => r.Descrizione).ToList();
            var idReparti = reparti.Select(r => r.Id).ToList();
            var mieiReparti = _az_SediRepartoUserRepository.FindAll(x => x.IdAspNetUsers == idAspNetUsers && x.UserInDepartment && idReparti.Contains(x.IdAz_SediReparto) &&
                                                                         (x.DataDal == null || x.DataDal <= oggi) && (x.DataAl == null || x.DataAl >= oggi))
                                                           .Select(x => x.IdAz_SediReparto).Distinct().ToList();
            var svolte = _az_SediRepartoAttivitaRepository.FindAll(x => idReparti.Contains(x.IdAz_SediReparto)).ToList();

            var opzioni = new List<TimbraturaOpzioneModel>();
            foreach (var reparto in reparti)
            {
                var attivitaReparto = svolte.Where(x => x.IdAz_SediReparto == reparto.Id).Select(x => x.IdPar_Attivita).ToList();
                if (attivitaReparto.Count == 0 && !mieiReparti.Contains(reparto.Id))
                    continue;
                opzioni.AddRange(attivita.Where(a => attivitaReparto.Count == 0 || attivitaReparto.Contains(a.SubCommessaAttivita_IdPar_Attivita))
                                         .Select(a => Opzione(a, reparto.Id, reparto.Descrizione)));
            }
            if (opzioni.Count == 0)
                opzioni = attivita.Select(a => Opzione(a, null, null)).ToList();

            int proposta = await _gestionePresenzeUserUtility.GetIdAz_SubCommessaAttivita_Proposta(idDip_RapportoLavoro);
            if (opzioni.Count == 0 && proposta > 0)
            {
                var p = elenco.FirstOrDefault(a => a.SubCommessaAttivita_Id == proposta);
                opzioni.Add(p != null ? Opzione(p, null, null) : new TimbraturaOpzioneModel { IdAz_SubCommessaAttivita = proposta });
            }
            return (opzioni, Math.Max(0, opzioni.FindIndex(o => o.IdAz_SubCommessaAttivita == proposta)));
        }

        private static TimbraturaOpzioneModel Opzione(Az_SubCommessaAttivita_4FullListModel a, int? idAz_SediReparto, string? reparto) => new()
        {
            IdAz_SediReparto = idAz_SediReparto,
            Reparto = reparto,
            IdAz_SubCommessaAttivita = a.SubCommessaAttivita_Id,
            Attivita = a.SubCommessaAttivita_Par_Attivita_Descrizione,
            Commessa = string.Join(" · ", new[] { a.Cliente_Default ? null : a.Cliente_Descrizione, a.Commessa_Decrizione, a.SubCommessa_Decrizione }
                                         .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
        };

        // scelta del dipendente: deve essere una delle opzioni; senza scelta (0) quella di default
        private static TimbraturaOpzioneModel Scelta(List<TimbraturaOpzioneModel> opzioni, int indiceDefault, int idAz_SubCommessaAttivita, int? idAz_SediReparto)
        {
            if (opzioni.Count == 0)
                throw new UserMessageException(AttendanceTrackingMessages.Get("ActivityNotAllowed"));
            if (idAz_SubCommessaAttivita <= 0)
                return opzioni[Math.Clamp(indiceDefault, 0, opzioni.Count - 1)];
            return opzioni.FirstOrDefault(o => o.IdAz_SubCommessaAttivita == idAz_SubCommessaAttivita && o.IdAz_SediReparto == idAz_SediReparto)
                   ?? throw new UserMessageException(AttendanceTrackingMessages.Get("ActivityNotAllowed"));
        }

        // salva la timbratura (stesse regole dell'inserimento dell'HR) e come e' nata; ricalcola il giorno
        private async Task<Dip_GG_TimbraturaInfoModel> Registra(Dip_RapportoLavoro rapporto, int idAttivita, Esito esito, OrigineTimbratura origine, string? operatore)
        {
            var fuori = _gestionePresenzeUserUtility.VerificaGiorniNelRapporto(rapporto.Id, esito.Ora.Date, esito.Ora.Date);
            if (fuori != null)
                throw new UserMessageException(fuori);

            var req = new GenericRequest<Dip_GG_TimbraturaPutInModel>();
            req.Data.IdDip_RapportoLavoro = rapporto.Id;
            req.Data.ExcludeRicalc = false;
            req.Data.Dip_GG_Timbratura = new Dip_GG_TimbraturaModel
            {
                Id = 0,
                IdDip_RapportoLavoro = rapporto.Id,
                Timbratura = esito.Ora,
                TimbraturaOriginale = esito.Ora,
                TimbraturaArrotondata = esito.Ora,
                GiornoCompetenza = esito.Ora.Date,
                TimbraturaTipo = TipoTimbratura.SenzaVerso,
                RichiestaStato = StatoRichiesta.Diretta,
                IdAz_SubCommessaAttivita = idAttivita
            };
            var res = await _dip_GG_TimbraturaService.Dip_GG_TimbraturaPut(req, true);
            var timbratura = res.Data?.Dip_GG_Timbratura;
            if (!res.Success || timbratura == null || timbratura.Id <= 0)
                throw new UserMessageException(res.Data?.Messages.FirstOrDefault()?.Text ?? Msg("TimbNonRegistrata"));

            var info = new Dip_GG_TimbraturaInfo
            {
                IdDip_GG_Timbratura = timbratura.Id,
                IdDip_RapportoLavoro = rapporto.Id,
                IdAz_Sedi = esito.IdAz_Sedi,
                IdAz_SediReparto = esito.IdAz_SediReparto,
                Origine = origine,
                Differita = esito.Differita,
                DataDispositivo = esito.DataDispositivo,
                DataRicezione = DateTime.Now,
                Latitudine = esito.Latitudine,
                Longitudine = esito.Longitudine,
                PrecisioneMetri = esito.Precisione,
                DistanzaMetri = esito.Distanza,
                EsitoGeofence = esito.Geofence,
                QrSlot = esito.QrSlot,
                IdAspNetUsers_Operatore = operatore
            };
            try
            {
                info = await _dip_GG_TimbraturaInfoRepository.CreateAsync(info);
            }
            catch (DbUpdateException)
            {
                // stesso QR usato in contemporanea: la timbratura appena creata si toglie
                var t = await _dip_GG_TimbraturaRepository.FindByIdAsync(timbratura.Id);
                if (t != null)
                    await _dip_GG_TimbraturaRepository.DeleteAsync(t);
                throw new UserMessageException(Msg("TimbQrGiaUsato"));
            }
            return ModelloInfo(info, timbratura.Timbratura);
        }

        private Dip_GG_TimbraturaInfoModel ModelloInfo(Dip_GG_TimbraturaInfo i, DateTime ora) => new()
        {
            IdDip_GG_Timbratura = i.IdDip_GG_Timbratura,
            Timbratura = ora,
            IdAz_Sedi = i.IdAz_Sedi,
            Sede = i.IdAz_Sedi != null ? _az_SediRepository.FindAll(s => s.Id == i.IdAz_Sedi).Select(s => s.Descrizione).FirstOrDefault() : null,
            IdAz_SediReparto = i.IdAz_SediReparto,
            Reparto = i.IdAz_SediReparto != null ? _az_SediRepartoRepository.FindAll(r => r.Id == i.IdAz_SediReparto).Select(r => r.Descrizione).FirstOrDefault() : null,
            Origine = i.Origine,
            Differita = i.Differita,
            DataDispositivo = i.DataDispositivo,
            DataRicezione = i.DataRicezione,
            DistanzaMetri = i.DistanzaMetri,
            EsitoGeofence = i.EsitoGeofence,
            Operatore = i.IdAspNetUsers_Operatore
        };

        // ── kiosk ─────────────────────────────────────────────────────────

        public virtual async Task<GenericResult<TimbraturaEvoluta_KioskRegistraOutModel>> KioskRegistra(GenericRequest<TimbraturaEvoluta_KioskRegistraInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await VerificaAdmin();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var sede = SedeAzienda(model.Data.IdAz_Sedi, idAzienda);
                var c = _az_SediTimbraturaRepository.FindAll(x => x.IdAz_Sedi == sede.Id).FirstOrDefault();
                if (c == null || !c.KioskAttivo)
                    throw new UserMessageException(Msg("TimbKioskNonAttivo"));
                c.KioskToken = NuovoSegreto();
                await _az_SediTimbraturaRepository.UpdateAsync(c);
                return new TimbraturaEvoluta_KioskRegistraOutModel { KioskToken = c.KioskToken };
            }, isSubProcess);
        }

        private string PinHash(int idAzienda, string pin)
        {
            var chiave = _configuration["Timbratura:PinKey"];
            if (string.IsNullOrEmpty(chiave))
                chiave = _jwtParameter.Key;
            using var hmac = new HMACSHA256(SHA256.HashData(Encoding.UTF8.GetBytes("nvx-pin|" + chiave)));
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{idAzienda}:{pin}")));
        }

        // dispositivo kiosk della sede (token) e dipendente dell'azienda dal PIN o dal badge, con il limite dei tentativi per dispositivo
        private async Task<(Az_Sedi Sede, Dip_Anagrafica Anagrafica, Dip_RapportoLavoro Rapporto)> KioskDipendenteAsync(TimbraturaEvoluta_KioskIdentificaInModel m)
        {
            // il dispositivo e' collegato con un amministratore dell'azienda: e' il permesso di timbrare per altri
            await VerificaAdmin();
            int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
            var sede = SedeAzienda(m.IdAz_Sedi, idAzienda);
            var c = _az_SediTimbraturaRepository.FindAll(x => x.IdAz_Sedi == sede.Id).FirstOrDefault();
            if (c == null || !c.KioskAttivo || string.IsNullOrEmpty(c.KioskToken))
                throw new UserMessageException(Msg("TimbKioskNonAttivo"));
            if (string.IsNullOrEmpty(m.KioskToken) ||
                !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(c.KioskToken), Encoding.UTF8.GetBytes(m.KioskToken)))
                throw new UserMessageException(Msg("TimbKioskNonRegistrato"));

            // limite dei tentativi per dispositivo
            var chiave = $"{idAzienda}:{sede.Id}:{c.KioskToken}";
            var adesso = DateTime.Now;
            var stato = _tentativiKiosk.GetOrAdd(chiave, _ => (0, adesso, DateTime.MinValue));
            if (stato.BloccatoFino > adesso)
                throw new UserMessageException(Msg("TimbKioskBloccato"));

            Dip_TimbraturaPin? cred = null;
            if (!string.IsNullOrWhiteSpace(m.Badge))
            {
                var badge = m.Badge.Trim();
                cred = _dip_TimbraturaPinRepository.FindAll(p => p.IdAz_Anagrafica == idAzienda && p.Badge == badge).FirstOrDefault();
            }
            else if (!string.IsNullOrWhiteSpace(m.Pin))
            {
                var hash = PinHash(idAzienda, m.Pin.Trim());
                cred = _dip_TimbraturaPinRepository.FindAll(p => p.IdAz_Anagrafica == idAzienda && p.PinHash == hash).FirstOrDefault();
            }

            var anagrafica = cred != null ? _dip_AnagraficaRepository.FindAll(a => a.Id == cred.IdDip_Anagrafica && a.IdAz_Anagrafica == idAzienda).FirstOrDefault() : null;
            if (anagrafica?.IdAspNetUsers == null)
            {
                var errori = adesso - stato.Inizio > TimeSpan.FromMinutes(1) ? 1 : stato.Errori + 1;
                var inizio = errori == 1 ? adesso : stato.Inizio;
                _tentativiKiosk[chiave] = (errori, inizio, errori >= MaxErroriKiosk ? adesso.AddMinutes(1) : DateTime.MinValue);
                throw new UserMessageException(Msg("TimbKioskCredenzialiErrate"));
            }
            _tentativiKiosk.TryRemove(chiave, out _);

            var dip = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(anagrafica.IdAspNetUsers, false);
            if (dip?.dip_RapportoLavoro == null)
                throw new UserMessageException(Msg("EmploymentNotFound"));
            return (sede, anagrafica, dip.dip_RapportoLavoro);
        }

        // primo passo del kiosk: chi e' il dipendente e quali reparto-attivita' puo' scegliere nella sede del kiosk
        public virtual async Task<GenericResult<TimbraturaEvoluta_KioskIdentificaOutModel>> KioskIdentifica(GenericRequest<TimbraturaEvoluta_KioskIdentificaInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var (sede, anagrafica, rapporto) = await KioskDipendenteAsync(model.Data);
                var (opzioni, indiceDefault) = await OpzioniAsync(anagrafica, rapporto.Id, sede.Id);
                return new TimbraturaEvoluta_KioskIdentificaOutModel
                {
                    Nominativo = TurniUtility.Nominativo(anagrafica),
                    Opzioni = opzioni,
                    IndiceDefault = indiceDefault
                };
            }, isSubProcess);
        }

        // secondo passo (o unico, con una sola opzione): identificazione ripetuta e timbratura sul reparto-attivita' scelto
        public virtual async Task<GenericResult<TimbraturaEvoluta_KioskStampOutModel>> KioskStamp(GenericRequest<TimbraturaEvoluta_KioskStampInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var (sede, anagrafica, rapporto) = await KioskDipendenteAsync(model.Data);
                var (opzioni, indiceDefault) = await OpzioniAsync(anagrafica, rapporto.Id, sede.Id);
                var scelta = Scelta(opzioni, indiceDefault, model.Data.IdAz_SubCommessaAttivita, model.Data.IdAz_SediReparto);

                var adesso = DateTime.Now;
                var esito = new Esito
                {
                    Ora = new DateTime(adesso.Year, adesso.Month, adesso.Day, adesso.Hour, adesso.Minute, 0),
                    IdAz_Sedi = sede.Id,
                    IdAz_SediReparto = scelta.IdAz_SediReparto
                };
                return new TimbraturaEvoluta_KioskStampOutModel
                {
                    Nominativo = TurniUtility.Nominativo(anagrafica),
                    Ora = esito.Ora,
                    Info = await Registra(rapporto, scelta.IdAz_SubCommessaAttivita, esito, OrigineTimbratura.Kiosk, CurrentUserId)
                };
            }, isSubProcess);
        }

        // ── terminali (postazione QR e kiosk) ─────────────────────────────

        /*
         Uscita dalla modalita' terminale: la pagina a schermo intero si lascia solo con la password di un CompanyPowerAdmin
         dell'azienda (uno qualsiasi tra gli utenti collegati e non disabilitati). Dopo 5 errori in un minuto lo sblocco
         resta bloccato per un minuto (per utente collegato al dispositivo).
        */
        public virtual async Task<GenericResult<TimbraturaEvoluta_TerminaleSbloccaOutModel>> TerminaleSblocca(GenericRequest<TimbraturaEvoluta_TerminaleSbloccaInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await VerificaAdmin();
                if (!int.TryParse(CurrentCompany, out var idCompany) || idCompany <= 0)
                    throw new UserMessageException(Msg("TimbNonAutorizzato"));

                var chiave = $"{idCompany}:{CurrentUserId}";
                var adesso = DateTime.Now;
                var stato = _tentativiSblocco.GetOrAdd(chiave, _ => (0, adesso, DateTime.MinValue));
                if (stato.BloccatoFino > adesso)
                    throw new UserMessageException(Msg("TimbKioskBloccato"));

                var password = model.Data.Password ?? string.Empty;
                bool ok = false;
                if (password.Length > 0)
                {
                    var idUtenti = await _userCompanyRepository.FindAll(x => x.IdCompany == idCompany && !x.Disabled)
                                                               .Select(x => x.IdAspNetUsers).Distinct().ToListAsync();
                    foreach (var idUtente in idUtenti)
                    {
                        var utente = await _userManager.FindByIdAsync(idUtente);
                        if (utente == null || !await _userManager.IsInRoleAsync(utente, "CompanyPowerAdmin"))
                            continue;
                        if (await _userManager.CheckPasswordAsync(utente, password))
                        {
                            ok = true;
                            break;
                        }
                    }
                }

                if (!ok)
                {
                    var errori = adesso - stato.Inizio > TimeSpan.FromMinutes(1) ? 1 : stato.Errori + 1;
                    var inizio = errori == 1 ? adesso : stato.Inizio;
                    _tentativiSblocco[chiave] = (errori, inizio, errori >= MaxErroriKiosk ? adesso.AddMinutes(1) : DateTime.MinValue);
                    throw new UserMessageException(Msg("TimbTerminalePasswordErrata"));
                }
                _tentativiSblocco.TryRemove(chiave, out _);
                return new TimbraturaEvoluta_TerminaleSbloccaOutModel { Sbloccato = true };
            }, isSubProcess);
        }

        // ── PIN e badge ───────────────────────────────────────────────────

        public virtual async Task<GenericResult<TimbraturaEvoluta_PinGetAllOutModel>> PinGetAll(GenericRequest<TimbraturaEvoluta_PinGetAllInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await VerificaAdmin();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var anagrafiche = _dip_AnagraficaRepository.FindAll(a => a.IdAz_Anagrafica == idAzienda && a.IdAspNetUsers != null).ToList();
                var cred = _dip_TimbraturaPinRepository.FindAll(p => p.IdAz_Anagrafica == idAzienda).ToList();
                return new TimbraturaEvoluta_PinGetAllOutModel
                {
                    Dipendenti = anagrafiche.Select(a =>
                    {
                        var c = cred.FirstOrDefault(x => x.IdDip_Anagrafica == a.Id);
                        return new Dip_TimbraturaPinModel { IdAspNetUsers = a.IdAspNetUsers!, Nominativo = TurniUtility.Nominativo(a), PinImpostato = c?.PinHash != null, Badge = c?.Badge };
                    }).OrderBy(x => x.Nominativo).ToList()
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<TimbraturaEvoluta_PinPutOutModel>> PinPut(GenericRequest<TimbraturaEvoluta_PinPutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var m = model.Data;
                // PIN e badge li assegna solo l'HR: con il controllo di unicita' un dipendente che sceglie il proprio PIN
                // scoprirebbe quelli dei colleghi (e potrebbe timbrare per loro al kiosk)
                await VerificaAdmin();
                bool perAltri = !string.IsNullOrEmpty(m.IdAspNetUsers) && m.IdAspNetUsers != CurrentUserId;

                var idUtente = perAltri ? m.IdAspNetUsers! : CurrentUserId;
                var anagrafica = _dip_AnagraficaRepository.FindAll(a => a.IdAz_Anagrafica == idAzienda && a.IdAspNetUsers == idUtente).FirstOrDefault()
                                 ?? throw new UserMessageException(Msg("EmployeeRecordNotFound"));
                var cred = _dip_TimbraturaPinRepository.FindAll(p => p.IdDip_Anagrafica == anagrafica.Id).FirstOrDefault()
                           ?? new Dip_TimbraturaPin { IdDip_Anagrafica = anagrafica.Id, IdAz_Anagrafica = idAzienda };

                if (m.Pin != null)
                {
                    var pin = m.Pin.Trim();
                    if (pin.Length == 0)
                        cred.PinHash = null;
                    else
                    {
                        if (pin.Length < 4 || pin.Length > 8 || !pin.All(char.IsDigit))
                            throw new UserMessageException(Msg("TimbPinNonValido"));
                        var hash = PinHash(idAzienda, pin);
                        if (_dip_TimbraturaPinRepository.FindAll(p => p.IdAz_Anagrafica == idAzienda && p.PinHash == hash && p.Id != cred.Id).Any())
                            throw new UserMessageException(Msg("TimbPinGiaUsato"));
                        cred.PinHash = hash;
                    }
                }
                if (m.Badge != null)
                {
                    var badge = m.Badge.Trim();
                    if (badge.Length > 50)
                        throw new UserMessageException(Msg("TimbBadgeNonValido"));
                    if (badge.Length > 0 && _dip_TimbraturaPinRepository.FindAll(p => p.IdAz_Anagrafica == idAzienda && p.Badge == badge && p.Id != cred.Id).Any())
                        throw new UserMessageException(Msg("TimbBadgeGiaUsato"));
                    cred.Badge = badge.Length > 0 ? badge : null;
                }
                await _dip_TimbraturaPinRepository.UpsertAsync(cred);
                return new TimbraturaEvoluta_PinPutOutModel();
            }, isSubProcess);
        }

        // ── informazioni delle timbrature di un giorno (cartellino) ──────

        public virtual async Task<GenericResult<TimbraturaEvoluta_Info4GiornoOutModel>> Info4Giorno(GenericRequest<TimbraturaEvoluta_Info4GiornoInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var (anagrafica, rapporto) = _turniUtility.DipendenteDelRapporto(model.Data.IdDip_RapportoLavoro);
                if (anagrafica == null || anagrafica.IdAz_Anagrafica != idAzienda ||
                    (anagrafica.IdAspNetUsers != CurrentUserId && !await _turniUtility.IsAdminAsync(CurrentUserId)))
                    throw new UserMessageException(Msg("TimbNonAutorizzato"));

                var giorno = model.Data.Data.Date;
                var timbrature = _dip_GG_TimbraturaRepository.FindAll(t => t.IdDip_RapportoLavoro == rapporto!.Id && t.GiornoCompetenza >= giorno && t.GiornoCompetenza < giorno.AddDays(1))
                                                             .Select(t => new { t.Id, t.Timbratura }).ToList();
                var ids = timbrature.Select(t => t.Id).ToList();
                var info = _dip_GG_TimbraturaInfoRepository.FindAll(i => ids.Contains(i.IdDip_GG_Timbratura)).ToList();
                return new TimbraturaEvoluta_Info4GiornoOutModel
                {
                    Info = info.Select(i => ModelloInfo(i, timbrature.First(t => t.Id == i.IdDip_GG_Timbratura).Timbratura)).OrderBy(i => i.Timbratura).ToList()
                };
            }, isSubProcess);
        }
    }

    public interface ITimbraturaEvolutaService : IServiceBase
    {
        Task<GenericResult<TimbraturaEvoluta_SediOutModel>> Sedi(GenericRequest<TimbraturaEvoluta_SediInModel> model, bool isSubProcess);
        Task<GenericResult<TimbraturaEvoluta_ConfigGetOutModel>> ConfigGet(GenericRequest<TimbraturaEvoluta_ConfigGetInModel> model, bool isSubProcess);
        Task<GenericResult<TimbraturaEvoluta_ConfigPutOutModel>> ConfigPut(GenericRequest<TimbraturaEvoluta_ConfigPutInModel> model, bool isSubProcess);
        Task<GenericResult<TimbraturaEvoluta_QrCodeOutModel>> QrCode(GenericRequest<TimbraturaEvoluta_QrCodeInModel> model, bool isSubProcess);
        Task<GenericResult<TimbraturaEvoluta_StampOutModel>> Stamp(GenericRequest<TimbraturaEvoluta_StampInModel> model, bool isSubProcess);
        Task<GenericResult<TimbraturaEvoluta_KioskRegistraOutModel>> KioskRegistra(GenericRequest<TimbraturaEvoluta_KioskRegistraInModel> model, bool isSubProcess);
        Task<GenericResult<TimbraturaEvoluta_OpzioniOutModel>> Opzioni(GenericRequest<TimbraturaEvoluta_OpzioniInModel> model, bool isSubProcess);
        Task<GenericResult<TimbraturaEvoluta_KioskIdentificaOutModel>> KioskIdentifica(GenericRequest<TimbraturaEvoluta_KioskIdentificaInModel> model, bool isSubProcess);
        Task<GenericResult<TimbraturaEvoluta_KioskStampOutModel>> KioskStamp(GenericRequest<TimbraturaEvoluta_KioskStampInModel> model, bool isSubProcess);
        Task<GenericResult<TimbraturaEvoluta_TerminaleSbloccaOutModel>> TerminaleSblocca(GenericRequest<TimbraturaEvoluta_TerminaleSbloccaInModel> model, bool isSubProcess);
        Task<GenericResult<TimbraturaEvoluta_PinGetAllOutModel>> PinGetAll(GenericRequest<TimbraturaEvoluta_PinGetAllInModel> model, bool isSubProcess);
        Task<GenericResult<TimbraturaEvoluta_PinPutOutModel>> PinPut(GenericRequest<TimbraturaEvoluta_PinPutInModel> model, bool isSubProcess);
        Task<GenericResult<TimbraturaEvoluta_Info4GiornoOutModel>> Info4Giorno(GenericRequest<TimbraturaEvoluta_Info4GiornoInModel> model, bool isSubProcess);
    }

    /// <summary>Giorno di lavoro agile approvato (AT-SW): il geofence non si applica.</summary>
    public interface ILavoroAgileGiorno
    {
        bool IsGiornoLavoroAgile(int idDip_RapportoLavoro, DateTime giorno);
    }
}
