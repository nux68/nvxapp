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
                                        IUserCompanyRepository userCompanyRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
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
            var reparti = _az_SediRepartoUserRepository.FindAll(x => x.IdAspNetUsers == idAspNetUsers && x.UserInDepartment)
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

                // sede: quella del QR, quella indicata o quella del reparto del dipendente
                var qr = string.IsNullOrWhiteSpace(m.Codice) ? null : LeggiCodice(m.Codice);
                if (!string.IsNullOrWhiteSpace(m.Codice) && qr == null)
                    throw new UserMessageException(Msg("TimbQrNonValido"));
                esito.IdAz_Sedi = qr?.IdAz_Sedi ?? m.IdAz_Sedi ?? SedeDelDipendente(CurrentUserId, idSediAzienda);
                if (esito.IdAz_Sedi != null && !idSediAzienda.Contains(esito.IdAz_Sedi.Value))
                    throw new UserMessageException(qr != null ? Msg("TimbQrNonValido") : Msg("TimbSedeNonValida"));
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
                if (qr != null)
                {
                    if (config == null || !config.QrAttivo || string.IsNullOrEmpty(config.QrSegreto))
                        throw new UserMessageException(Msg("TimbQrNonAttivo"));
                    var atteso = Firma(config.QrSegreto, qr.Value.IdAz_Sedi, qr.Value.Slot);
                    if (!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(atteso), Encoding.ASCII.GetBytes(qr.Value.Firma)))
                        throw new UserMessageException(Msg("TimbQrNonValido"));
                    var slotOra = Slot(esito.Differita ? esito.DataDispositivo!.Value : adesso, config.QrDurataSecondi);
                    if (qr.Value.Slot > slotOra || qr.Value.Slot < slotOra - 1)
                        throw new UserMessageException(Msg("TimbQrScaduto"));
                    if (_dip_GG_TimbraturaInfoRepository.FindAll(i => i.IdAz_Sedi == qr.Value.IdAz_Sedi && i.QrSlot == qr.Value.Slot && i.IdDip_RapportoLavoro == rapporto.Id).Any())
                        throw new UserMessageException(Msg("TimbQrGiaUsato"));
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

                // attivita' consentita al dipendente (come la timbratura semplice)
                int idAttivita = await _gestionePresenzeUserUtility.GetIdAz_SubCommessaAttivita_4Timbratura(rapporto.Id, m.IdAz_SubCommessaAttivita);
                var nonConsentita = await _gestionePresenzeUserUtility.VerificaAttivitaConsentita(rapporto.Id, idAttivita);
                if (nonConsentita != null)
                    throw new UserMessageException(nonConsentita);

                retVal.Info = await Registra(rapporto, idAttivita, esito, esito.QrSlot != null ? OrigineTimbratura.Qr : OrigineTimbratura.App, null);
                if (esito.Avviso != null)
                    retVal.Messages.Add(new Message(esito.Avviso, MessageType.Warning));
                return retVal;
            }, isSubProcess);
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

        public virtual async Task<GenericResult<TimbraturaEvoluta_KioskStampOutModel>> KioskStamp(GenericRequest<TimbraturaEvoluta_KioskStampInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                // il dispositivo e' collegato con un amministratore dell'azienda: e' il permesso di timbrare per altri
                await VerificaAdmin();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var sede = SedeAzienda(model.Data.IdAz_Sedi, idAzienda);
                var c = _az_SediTimbraturaRepository.FindAll(x => x.IdAz_Sedi == sede.Id).FirstOrDefault();
                if (c == null || !c.KioskAttivo || string.IsNullOrEmpty(c.KioskToken))
                    throw new UserMessageException(Msg("TimbKioskNonAttivo"));
                if (string.IsNullOrEmpty(model.Data.KioskToken) ||
                    !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(c.KioskToken), Encoding.UTF8.GetBytes(model.Data.KioskToken)))
                    throw new UserMessageException(Msg("TimbKioskNonRegistrato"));

                // limite dei tentativi per dispositivo
                var chiave = $"{idAzienda}:{sede.Id}:{c.KioskToken}";
                var adesso = DateTime.Now;
                var stato = _tentativiKiosk.GetOrAdd(chiave, _ => (0, adesso, DateTime.MinValue));
                if (stato.BloccatoFino > adesso)
                    throw new UserMessageException(Msg("TimbKioskBloccato"));

                Dip_TimbraturaPin? cred = null;
                if (!string.IsNullOrWhiteSpace(model.Data.Badge))
                {
                    var badge = model.Data.Badge.Trim();
                    cred = _dip_TimbraturaPinRepository.FindAll(p => p.IdAz_Anagrafica == idAzienda && p.Badge == badge).FirstOrDefault();
                }
                else if (!string.IsNullOrWhiteSpace(model.Data.Pin))
                {
                    var hash = PinHash(idAzienda, model.Data.Pin.Trim());
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
                var esito = new Esito { Ora = new DateTime(adesso.Year, adesso.Month, adesso.Day, adesso.Hour, adesso.Minute, 0), IdAz_Sedi = sede.Id };
                int idAttivita = await _gestionePresenzeUserUtility.GetIdAz_SubCommessaAttivita_4Timbratura(dip.dip_RapportoLavoro.Id, 0);

                var retVal = new TimbraturaEvoluta_KioskStampOutModel
                {
                    Nominativo = TurniUtility.Nominativo(anagrafica),
                    Ora = esito.Ora,
                    Info = await Registra(dip.dip_RapportoLavoro, idAttivita, esito, OrigineTimbratura.Kiosk, CurrentUserId)
                };
                return retVal;
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
