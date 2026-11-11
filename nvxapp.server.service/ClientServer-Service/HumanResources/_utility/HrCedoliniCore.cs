using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.HumanResources;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.HumanResources;
using nvxapp.server.service.ClientServer_Service.HumanResources.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Acknowledgement;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage;
using Serilog;
using System.Globalization;

namespace nvxapp.server.service.ClientServer_Service.HumanResources._utility
{
    /*
     Logica dei cedolini e delle CU (HR-CED, ANALISI_HUMANRESOURCES_APPLICATIVO.md §4.2). Lavora sull'azienda del contesto
     ricevuto: per lo studio va chiamata in HrAzienda.RunAsync (nuovo scope sull'azienda), come nei job.
        1. caricamento del PDF unico (file in IFileStorage, riga Hr_DocCaricamento)
        2. divisione per dipendente (job): testo di ogni pagina, codice fiscale di Dip_Anagrafica, poi matricola; le pagine
           senza un solo dipendente diventano scarti con la proposta per cognome e nome trovati nel testo
        3. scarti assegnati a mano (il documento del dipendente si ricompone con la pagina) o ignorati
        4. pubblicazione: data di pubblicazione, presa visione (IAcknowledgementService, fonte hr.cedolino, con il PDF per il
           canale del documentale) e notifica "nuovo cedolino" (INotificationSender, se attive nella configurazione)
    */
    public interface IHrCedoliniCore
    {
        Task<Hr_DocCaricamento> CreaAsync(HrContesto ctx, Hr_CaricamentoUploadModel d, Stream content, string fileName, long length);
        Task DividiAsync(HrContesto ctx, int idCaricamento, Func<int, Task> progresso);
        Task<int> PubblicaAsync(HrContesto ctx, int idCaricamento, Func<int, Task> progresso);
        Task<List<Hr_CaricamentoModel>> ListAsync(HrContesto ctx, int? anno);
        Task<Hr_CaricamentoGetOutModel> GetAsync(HrContesto ctx, int id);
        Task<Hr_CaricamentoGetOutModel> AssegnaAsync(HrContesto ctx, int idScarto, int idDip_Anagrafica);
        Task<Hr_CaricamentoGetOutModel> IgnoraAsync(HrContesto ctx, int idScarto);
        Task DeleteAsync(HrContesto ctx, int id);
        Task<Hr_DocCaricamento> CaricamentoAsync(HrContesto ctx, int id);
        Task<Hr_FileDownload> DownloadAsync(HrContesto ctx, int idDocumento, bool soloDelDipendente);
        Task<Hr_MieiDocumentiOutModel> MieiAsync(HrContesto ctx);
    }

    public class HrCedoliniCore : IHrCedoliniCore
    {
        public const string FontePresaVisione = "hr.cedolino";
        // dimensione massima del PDF unico
        public const long MaxPdf = 100L * 1024 * 1024;

        private readonly IHr_DocCaricamentoRepository _Hr_DocCaricamento;
        private readonly IHr_DocDipendenteRepository _Hr_DocDipendente;
        private readonly IHr_DocScartoRepository _Hr_DocScarto;
        private readonly IHr_Dip_AnagraficaReadRepository _Dip_Anagrafica;
        private readonly IPresaVisioneRepository _presaVisioneRepository;
        private readonly IHrUtility _utility;
        private readonly IFileStorage _fileStorage;
        private readonly IAcknowledgementService _acknowledgementService;
        private readonly INotificationSender _notificationSender;

        public HrCedoliniCore(IHr_DocCaricamentoRepository hr_DocCaricamento, IHr_DocDipendenteRepository hr_DocDipendente, IHr_DocScartoRepository hr_DocScarto,
                              IHr_Dip_AnagraficaReadRepository dip_Anagrafica, IPresaVisioneRepository presaVisioneRepository, IHrUtility utility,
                              IFileStorage fileStorage, IAcknowledgementService acknowledgementService, INotificationSender notificationSender)
        {
            _Hr_DocCaricamento = hr_DocCaricamento;
            _Hr_DocDipendente = hr_DocDipendente;
            _Hr_DocScarto = hr_DocScarto;
            _Dip_Anagrafica = dip_Anagrafica;
            _presaVisioneRepository = presaVisioneRepository;
            _utility = utility;
            _fileStorage = fileStorage;
            _acknowledgementService = acknowledgementService;
            _notificationSender = notificationSender;
        }

        // ------------------------------------------------------------------------------------------------ caricamento
        public async Task<Hr_DocCaricamento> CreaAsync(HrContesto ctx, Hr_CaricamentoUploadModel d, Stream content, string fileName, long length)
        {
            if (!Enum.IsDefined(typeof(Hr_TipoDocumento), d.Tipo))
                throw new UserMessageException(HumanResourcesMessages.Get("PayslipTypeInvalid"));
            var tipo = (Hr_TipoDocumento)d.Tipo;
            if (d.Anno < 2000 || d.Anno > 2100)
                throw new UserMessageException(HumanResourcesMessages.Get("PayslipYearInvalid"));
            int? mese = tipo == Hr_TipoDocumento.Cu ? null : d.Mese;
            if (tipo == Hr_TipoDocumento.Cedolino && (mese is null || mese < 1 || mese > 13))
                throw new UserMessageException(HumanResourcesMessages.Get("PayslipMonthInvalid"));
            if (length <= 0)
                throw new UserMessageException(HumanResourcesMessages.Get("FileEmpty"));
            if (length > MaxPdf)
                throw new UserMessageException(HumanResourcesMessages.Get("FileTooLarge", MaxPdf / (1024 * 1024)));
            if (!string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase))
                throw new UserMessageException(HumanResourcesMessages.Get("PayslipPdfRequired"));

            var key = await _fileStorage.SaveAsync(ctx.IdCompany, content);
            try
            {
                return await _Hr_DocCaricamento.CreateAsync(new Hr_DocCaricamento
                {
                    IdAz_Anagrafica = ctx.IdAz_Anagrafica, Tipo = tipo, Anno = d.Anno, Mese = mese,
                    Origine = ctx.Studio ? Hr_OrigineCaricamento.Studio : Hr_OrigineCaricamento.Azienda, IdFinancialAdvisor = ctx.IdFinancialAdvisor,
                    IdAspNetUsersCaricamento = ctx.IdUtente, NomeFile = NomeFile(fileName), StorageKey = key, Dimensione = length,
                    Stato = Hr_StatoCaricamento.InElaborazione
                });
            }
            catch
            {
                await _fileStorage.DeleteAsync(key);
                throw;
            }
        }

        // ------------------------------------------------------------------------------------------------ divisione (job)
        public async Task DividiAsync(HrContesto ctx, int idCaricamento, Func<int, Task> progresso)
        {
            var c = await CaricamentoAsync(ctx, idCaricamento);
            try
            {
                byte[] pdf;
                await using (var s = await _fileStorage.OpenAsync(c.StorageKey))
                using (var ms = new MemoryStream())
                {
                    await s.CopyToAsync(ms);
                    pdf = ms.ToArray();
                }
                var testi = HrPdf.TestoPagine(pdf);
                await progresso(20);

                var dipendenti = await _Dip_Anagrafica.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToListAsync();
                var perCf = Univoci(dipendenti, x => (x.CodiceFiscale ?? string.Empty).Trim().ToUpperInvariant());
                var perMatricola = Univoci(dipendenti, x => HrPdf.NormalizzaMatricola(x.Matricola));

                var pagineDi = new Dictionary<int, (List<int> Pagine, Hr_MetodoAbbinamento Metodo)>();
                var scarti = new List<Hr_DocScarto>();
                for (int i = 0; i < testi.Count; i++)
                {
                    var testo = testi[i];
                    int pagina = i + 1;
                    var daCf = HrPdf.CodiciFiscali(testo).Where(perCf.ContainsKey).Select(x => perCf[x]).Distinct().ToList();
                    int? id = null;
                    var metodo = Hr_MetodoAbbinamento.CodiceFiscale;
                    string? motivo = null;
                    if (daCf.Count == 1)
                        id = daCf[0];
                    else if (daCf.Count > 1)
                        motivo = HumanResourcesMessages.Get("ScrapReasonMany");
                    else
                    {
                        var daMatr = HrPdf.Matricole(testo).Where(perMatricola.ContainsKey).Select(x => perMatricola[x]).Distinct().ToList();
                        if (daMatr.Count == 1) { id = daMatr[0]; metodo = Hr_MetodoAbbinamento.Matricola; }
                        else if (daMatr.Count > 1) motivo = HumanResourcesMessages.Get("ScrapReasonMany");
                        else motivo = HumanResourcesMessages.Get(string.IsNullOrWhiteSpace(testo) ? "ScrapReasonNoText" : "ScrapReasonNoCode");
                    }

                    if (id is int idDip)
                    {
                        if (!pagineDi.TryGetValue(idDip, out var v))
                            pagineDi[idDip] = v = (new List<int>(), metodo);
                        v.Pagine.Add(pagina);
                        if (metodo < v.Metodo)
                            pagineDi[idDip] = (v.Pagine, metodo);
                    }
                    else
                        scarti.Add(new Hr_DocScarto
                        {
                            IdHr_DocCaricamento = c.Id, Pagina = pagina, Testo = HrPdf.Taglia(Compatta(testo), 2000), Motivo = HrPdf.Taglia(motivo, 200),
                            IdDip_AnagraficaProposta = Proposta(testo, dipendenti)
                        });
                    if (testi.Count > 0 && i % Math.Max(1, testi.Count / 10) == 0)
                        await progresso(20 + 30 * (i + 1) / testi.Count);
                }

                int n = 0;
                foreach (var (idDip, v) in pagineDi)
                {
                    var file = HrPdf.Estrai(pdf, v.Pagine);
                    var key = await _fileStorage.SaveAsync(ctx.IdCompany, new MemoryStream(file));
                    await _Hr_DocDipendente.CreateAsync(new Hr_DocDipendente
                    {
                        IdAz_Anagrafica = ctx.IdAz_Anagrafica, IdHr_DocCaricamento = c.Id, IdDip_Anagrafica = idDip, Tipo = c.Tipo, Anno = c.Anno, Mese = c.Mese,
                        Pagine = HrPdf.PagineTesto(v.Pagine), StorageKey = key, Dimensione = file.LongLength, Metodo = v.Metodo
                    }, false);
                    if (++n % 10 == 0)
                        await progresso(50 + 45 * n / Math.Max(1, pagineDi.Count));
                }
                foreach (var s in scarti)
                    await _Hr_DocScarto.CreateAsync(s, false);

                c.NumeroPagine = testi.Count;
                c.Stato = testi.Count == 0 ? Hr_StatoCaricamento.Errore : scarti.Count > 0 ? Hr_StatoCaricamento.DaVerificare : Hr_StatoCaricamento.DaPubblicare;
                c.Messaggio = testi.Count == 0 ? HumanResourcesMessages.Get("PayslipNoPages") : null;
                await _Hr_DocCaricamento.UpdateAsync(c);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[HR] Divisione del caricamento {Id} non riuscita", c.Id);
                await _Hr_DocCaricamento.FindAll(x => x.Id == c.Id).ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.Stato, Hr_StatoCaricamento.Errore)
                    .SetProperty(x => x.Messaggio, HumanResourcesMessages.Get("PayslipNotReadable")));
                throw;
            }
        }

        // codice -> dipendente, solo per i codici di un solo dipendente
        private static Dictionary<string, int> Univoci(List<Dip_Anagrafica> dipendenti, Func<Dip_Anagrafica, string> codice)
            => dipendenti.Select(d => (Codice: codice(d), d.Id)).Where(x => x.Codice.Length > 0)
                         .GroupBy(x => x.Codice).Where(g => g.Count() == 1).ToDictionary(g => g.Key, g => g.First().Id);

        // un solo dipendente con cognome e nome nel testo della pagina
        private static int? Proposta(string testo, List<Dip_Anagrafica> dipendenti)
        {
            if (string.IsNullOrWhiteSpace(testo))
                return null;
            var t = " " + Compatta(testo).ToUpperInvariant() + " ";
            var trovati = dipendenti.Where(d => (d.Cognome ?? "").Trim().Length >= 2 && (d.Nome ?? "").Trim().Length >= 2
                                                && t.Contains(" " + d.Cognome!.Trim().ToUpperInvariant() + " ")
                                                && t.Contains(" " + d.Nome!.Trim().ToUpperInvariant() + " "))
                                    .Select(d => d.Id).Distinct().ToList();
            return trovati.Count == 1 ? trovati[0] : null;
        }

        private static string Compatta(string testo) => System.Text.RegularExpressions.Regex.Replace(testo ?? string.Empty, @"\s+", " ").Trim();

        // ------------------------------------------------------------------------------------------------ scarti
        public async Task<Hr_CaricamentoGetOutModel> AssegnaAsync(HrContesto ctx, int idScarto, int idDip_Anagrafica)
        {
            var (scarto, c) = await ScartoAsync(ctx, idScarto);
            await _utility.DipendenteAsync(ctx, idDip_Anagrafica);
            if (scarto.Esito == Hr_EsitoScarto.Assegnato && scarto.IdDip_Anagrafica != idDip_Anagrafica)
                throw new UserMessageException(HumanResourcesMessages.Get("ScrapAlreadyAssigned"));

            byte[] pdf = await LeggiAsync(c.StorageKey);
            var doc = await _Hr_DocDipendente.FindAll(x => x.IdHr_DocCaricamento == c.Id && x.IdDip_Anagrafica == idDip_Anagrafica).AsTracking().FirstOrDefaultAsync();
            var pagine = HrPdf.PagineLista(doc?.Pagine);
            pagine.Add(scarto.Pagina);
            var file = HrPdf.Estrai(pdf, pagine);
            var key = await _fileStorage.SaveAsync(ctx.IdCompany, new MemoryStream(file));
            string? vecchia = doc?.StorageKey;
            if (doc == null)
            {
                doc = new Hr_DocDipendente
                {
                    IdAz_Anagrafica = ctx.IdAz_Anagrafica, IdHr_DocCaricamento = c.Id, IdDip_Anagrafica = idDip_Anagrafica, Tipo = c.Tipo, Anno = c.Anno, Mese = c.Mese,
                    Metodo = Hr_MetodoAbbinamento.Manuale
                };
                await _Hr_DocDipendente.CreateAsync(doc, false);
            }
            doc.Pagine = HrPdf.PagineTesto(pagine);
            doc.StorageKey = key;
            doc.Dimensione = file.LongLength;
            doc.ModifiedDate = DateTime.Now;
            scarto.Esito = Hr_EsitoScarto.Assegnato;
            scarto.IdDip_Anagrafica = idDip_Anagrafica;
            scarto.ModifiedDate = DateTime.Now;
            await _Hr_DocDipendente.SaveChange();
            if (vecchia != null)
                await EliminaFileAsync(vecchia);

            // caricamento gia' pubblicato: il documento (nuovo o ricomposto) si pubblica subito e si richiede la presa visione
            if (c.Stato == Hr_StatoCaricamento.Pubblicato)
                await PubblicaDocumentiAsync(ctx, c, new List<Hr_DocDipendente> { doc }, true);
            await AggiornaStatoAsync(c.Id);
            return await GetAsync(ctx, c.Id);
        }

        public async Task<Hr_CaricamentoGetOutModel> IgnoraAsync(HrContesto ctx, int idScarto)
        {
            var (scarto, c) = await ScartoAsync(ctx, idScarto);
            if (scarto.Esito == Hr_EsitoScarto.Assegnato)
                throw new UserMessageException(HumanResourcesMessages.Get("ScrapAlreadyAssigned"));
            scarto.Esito = Hr_EsitoScarto.Ignorato;
            scarto.ModifiedDate = DateTime.Now;
            await _Hr_DocScarto.SaveChange();
            await AggiornaStatoAsync(c.Id);
            return await GetAsync(ctx, c.Id);
        }

        private async Task<(Hr_DocScarto, Hr_DocCaricamento)> ScartoAsync(HrContesto ctx, int idScarto)
        {
            var scarto = await _Hr_DocScarto.FindAll(x => x.Id == idScarto && x.Hr_DocCaricamentoNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                         ?? throw new UserMessageException(HumanResourcesMessages.Get("ScrapNotFound"));
            var c = await CaricamentoAsync(ctx, scarto.IdHr_DocCaricamento);
            if (c.Stato == Hr_StatoCaricamento.InElaborazione || c.Stato == Hr_StatoCaricamento.Errore)
                throw new UserMessageException(HumanResourcesMessages.Get("PayslipNotReady"));
            return (scarto, c);
        }

        private async Task AggiornaStatoAsync(int idCaricamento)
        {
            bool daVerificare = await _Hr_DocScarto.FindAll(x => x.IdHr_DocCaricamento == idCaricamento && x.Esito == Hr_EsitoScarto.DaVerificare).AnyAsync();
            await _Hr_DocCaricamento.FindAll(x => x.Id == idCaricamento && (x.Stato == Hr_StatoCaricamento.DaVerificare || x.Stato == Hr_StatoCaricamento.DaPubblicare))
                                    .ExecuteUpdateAsync(u => u.SetProperty(x => x.Stato, daVerificare ? Hr_StatoCaricamento.DaVerificare : Hr_StatoCaricamento.DaPubblicare));
        }

        // ------------------------------------------------------------------------------------------------ pubblicazione (job)
        public async Task<int> PubblicaAsync(HrContesto ctx, int idCaricamento, Func<int, Task> progresso)
        {
            var c = await CaricamentoAsync(ctx, idCaricamento);
            var docs = await _Hr_DocDipendente.FindAll(x => x.IdHr_DocCaricamento == c.Id && x.DataPubblicazione == null).AsTracking().ToListAsync();
            int n = await PubblicaDocumentiAsync(ctx, c, docs, false, progresso);
            await _Hr_DocCaricamento.FindAll(x => x.Id == c.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.Stato, Hr_StatoCaricamento.Pubblicato)
                                                                                     .SetProperty(x => x.DataPubblicazione, DateTime.Now));
            return n;
        }

        private async Task<int> PubblicaDocumentiAsync(HrContesto ctx, Hr_DocCaricamento c, List<Hr_DocDipendente> docs, bool richiediDiNuovo, Func<int, Task>? progresso = null)
        {
            if (docs.Count == 0)
                return 0;
            var cfg = await _utility.CfgAsync(ctx);
            var ids = docs.Select(x => x.IdDip_Anagrafica).Distinct().ToList();
            var dipendenti = await _Dip_Anagrafica.FindAll(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
            var titolo = Titolo(c.Tipo, c.Anno, c.Mese);
            var notificare = new List<string>();
            int i = 0;
            foreach (var doc in docs)
            {
                doc.DataPubblicazione = DateTime.Now;
                doc.ModifiedDate = DateTime.Now;
                await _Hr_DocDipendente.SaveChange();

                var dip = dipendenti.GetValueOrDefault(doc.IdDip_Anagrafica);
                if (!string.IsNullOrEmpty(dip?.IdAspNetUsers))
                {
                    try
                    {
                        await _acknowledgementService.RequestAsync(new AcknowledgementRequest
                        {
                            IdCompany = ctx.IdCompany, Application = ApplicationType.HumanResources, Fonte = FontePresaVisione, IdRiferimento = doc.Id,
                            UserIds = new[] { dip.IdAspNetUsers! }, Titolo = titolo, Url = "/hrmypayslips",
                            NomeFile = NomeDocumento(c.Tipo, c.Anno, c.Mese, dip), Contenuto = await LeggiAsync(doc.StorageKey),
                            RichiediDiNuovo = richiediDiNuovo, Notifica = false
                        });
                        notificare.Add(dip.IdAspNetUsers!);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[HR] Presa visione del documento {Id} non richiesta", doc.Id);
                    }
                }
                if (progresso != null && ++i % 5 == 0)
                    await progresso(10 + 85 * i / docs.Count);
            }

            if (cfg.NotificheAttive && notificare.Count > 0)
                await _notificationSender.SendAsync(new NotificationMessage
                {
                    UserIds = notificare.Distinct().ToList(),
                    IdCompany = ctx.IdCompany,
                    Categoria = HrNotificationCategoryProvider.Cedolini.Code,
                    Titolo = HumanResourcesMessages.Get("PayslipNotificationTitle", titolo),
                    Messaggio = HumanResourcesMessages.Get("PayslipNotificationText", titolo),
                    Url = "/hrmypayslips"
                });
            return docs.Count;
        }

        public static string Titolo(Hr_TipoDocumento tipo, int anno, int? mese)
            => tipo == Hr_TipoDocumento.Cu
               ? HumanResourcesMessages.Get("TitleCu", anno)
               : HumanResourcesMessages.Get("TitlePayslip", mese == 13 ? HumanResourcesMessages.Get("Month13") : CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(mese ?? 1), anno);

        private static string NomeDocumento(Hr_TipoDocumento tipo, int anno, int? mese, Dip_Anagrafica? dip)
        {
            var nome = string.Concat((dip == null ? string.Empty : HrUtility.NomeDipendente(dip)).Split(Path.GetInvalidFileNameChars())).Replace(' ', '_');
            return (tipo == Hr_TipoDocumento.Cu ? $"CU_{anno}" : $"Cedolino_{anno}_{mese:00}") + (nome.Length > 0 ? "_" + nome : string.Empty) + ".pdf";
        }

        // ------------------------------------------------------------------------------------------------ letture
        public async Task<List<Hr_CaricamentoModel>> ListAsync(HrContesto ctx, int? anno)
        {
            return await _Hr_DocCaricamento.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && (anno == null || x.Anno == anno))
                .OrderByDescending(x => x.Anno).ThenByDescending(x => x.Mese).ThenByDescending(x => x.Id)
                .Select(x => new Hr_CaricamentoModel
                {
                    Id = x.Id, Tipo = (int)x.Tipo, Anno = x.Anno, Mese = x.Mese, Origine = (int)x.Origine, NomeFile = x.NomeFile, Stato = (int)x.Stato,
                    NumeroPagine = x.NumeroPagine, Messaggio = x.Messaggio, DataCaricamento = x.CreationDate, DataPubblicazione = x.DataPubblicazione,
                    NumeroDocumenti = x.Hr_DocDipendente!.Count(),
                    NumeroPubblicati = x.Hr_DocDipendente!.Count(d => d.DataPubblicazione != null),
                    NumeroScarti = x.Hr_DocScarto!.Count(s => s.Esito == Hr_EsitoScarto.DaVerificare),
                }).ToListAsync();
        }

        public async Task<Hr_CaricamentoGetOutModel> GetAsync(HrContesto ctx, int id)
        {
            var c = (await ListAsync(ctx, null)).FirstOrDefault(x => x.Id == id) ?? throw new UserMessageException(HumanResourcesMessages.Get("PayslipUploadNotFound"));
            var docs = await _Hr_DocDipendente.FindAll(x => x.IdHr_DocCaricamento == id)
                .Select(x => new Hr_DocDipendenteModel
                {
                    Id = x.Id, IdDip_Anagrafica = x.IdDip_Anagrafica, Dipendente = (x.Dip_AnagraficaNavigation!.Cognome + " " + x.Dip_AnagraficaNavigation.Nome).Trim(),
                    CodiceFiscale = x.Dip_AnagraficaNavigation.CodiceFiscale, Matricola = x.Dip_AnagraficaNavigation.Matricola, Pagine = x.Pagine, Metodo = (int)x.Metodo,
                    DataPubblicazione = x.DataPubblicazione, SenzaUtente = x.Dip_AnagraficaNavigation.IdAspNetUsers == null
                }).OrderBy(x => x.Dipendente).ToListAsync();

            // conferme di presa visione (tabella comune di public)
            var idDocs = docs.Select(x => (int?)x.Id).ToList();
            var prese = await _presaVisioneRepository.FindAll(x => x.IdCompany == ctx.IdCompany && x.Application == (int)ApplicationType.HumanResources
                                                                   && x.Fonte == FontePresaVisione && idDocs.Contains(x.IdRiferimento) && !x.Annullata)
                                                     .Select(x => new { x.IdRiferimento, x.CreationDate, x.DataConferma, x.Canale }).ToListAsync();
            foreach (var d in docs)
            {
                var p = prese.Where(x => x.IdRiferimento == d.Id).OrderByDescending(x => x.CreationDate).FirstOrDefault();
                d.DataRichiesta = p?.CreationDate;
                d.DataConferma = p?.DataConferma;
                d.Canale = p?.Canale;
            }

            var dipendenti = await DipendentiAsync(ctx);
            var nomi = dipendenti.ToDictionary(x => x.Id, x => $"{x.Cognome} {x.Nome}".Trim());
            var scarti = await _Hr_DocScarto.FindAll(x => x.IdHr_DocCaricamento == id).OrderBy(x => x.Pagina)
                .Select(x => new Hr_DocScartoModel
                {
                    Id = x.Id, Pagina = x.Pagina, Testo = x.Testo, Motivo = x.Motivo, IdDip_AnagraficaProposta = x.IdDip_AnagraficaProposta, Esito = (int)x.Esito,
                    IdDip_Anagrafica = x.IdDip_Anagrafica
                }).ToListAsync();
            foreach (var s in scarti)
            {
                s.Proposta = s.IdDip_AnagraficaProposta is int pr ? nomi.GetValueOrDefault(pr) : null;
                s.Dipendente = s.IdDip_Anagrafica is int di ? nomi.GetValueOrDefault(di) : null;
            }

            var pubblicati = docs.Where(x => x.DataPubblicazione != null).ToList();
            return new Hr_CaricamentoGetOutModel
            {
                Caricamento = c, Documenti = docs, Scarti = scarti, Dipendenti = dipendenti,
                Conferme = new Hr_ConfermeModel
                {
                    Pubblicati = pubblicati.Count,
                    Confermati = pubblicati.Count(x => x.DataConferma != null),
                    DaConfermare = pubblicati.Count(x => x.DataRichiesta != null && x.DataConferma == null),
                    SenzaUtente = pubblicati.Count(x => x.SenzaUtente)
                }
            };
        }

        private Task<List<Hr_DipendenteBreveModel>> DipendentiAsync(HrContesto ctx)
            => _Dip_Anagrafica.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).OrderBy(x => x.Cognome).ThenBy(x => x.Nome)
                              .Select(x => new Hr_DipendenteBreveModel
                              {
                                  Id = x.Id, Cognome = x.Cognome ?? string.Empty, Nome = x.Nome ?? string.Empty, Matricola = x.Matricola, CodiceFiscale = x.CodiceFiscale, Attivo = x.Attivo
                              }).ToListAsync();

        public async Task<Hr_DocCaricamento> CaricamentoAsync(HrContesto ctx, int id)
            => await _Hr_DocCaricamento.FindAll(x => x.Id == id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
               ?? throw new UserMessageException(HumanResourcesMessages.Get("PayslipUploadNotFound"));

        // cancellazione di un caricamento non pubblicato (con i suoi file)
        public async Task DeleteAsync(HrContesto ctx, int id)
        {
            var c = await CaricamentoAsync(ctx, id);
            if (c.Stato == Hr_StatoCaricamento.Pubblicato || c.Stato == Hr_StatoCaricamento.InElaborazione
                || await _Hr_DocDipendente.FindAll(x => x.IdHr_DocCaricamento == id && x.DataPubblicazione != null).AnyAsync())
                throw new UserMessageException(HumanResourcesMessages.Get("PayslipUploadNotDeletable"));
            var chiavi = await _Hr_DocDipendente.FindAll(x => x.IdHr_DocCaricamento == id).Select(x => x.StorageKey).ToListAsync();
            chiavi.Add(c.StorageKey);
            await _Hr_DocCaricamento.FindAll(x => x.Id == id).ExecuteDeleteAsync();
            foreach (var k in chiavi)
                await EliminaFileAsync(k);
        }

        public async Task<Hr_FileDownload> DownloadAsync(HrContesto ctx, int idDocumento, bool soloDelDipendente)
        {
            var doc = await _Hr_DocDipendente.FindAll(x => x.Id == idDocumento && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                             .Include(x => x.Dip_AnagraficaNavigation).FirstOrDefaultAsync()
                      ?? throw new UserMessageException(HumanResourcesMessages.Get("DocumentNotFound"));
            if (soloDelDipendente && (doc.DataPubblicazione == null || doc.Dip_AnagraficaNavigation?.IdAspNetUsers != ctx.IdUtente || string.IsNullOrEmpty(ctx.IdUtente)))
                throw new UserMessageException(HumanResourcesMessages.Get("DocumentNotFound"));
            return new Hr_FileDownload
            {
                Content = await _fileStorage.OpenAsync(doc.StorageKey), ContentType = "application/pdf",
                NomeFile = NomeDocumento(doc.Tipo, doc.Anno, doc.Mese, doc.Dip_AnagraficaNavigation)
            };
        }

        public async Task<Hr_MieiDocumentiOutModel> MieiAsync(HrContesto ctx)
        {
            var dip = await _utility.DipendenteCorrenteAsync(ctx);
            if (dip == null)
                return new Hr_MieiDocumentiOutModel { Dipendente = false };
            var docs = await _Hr_DocDipendente.FindAll(x => x.IdDip_Anagrafica == dip.Id && x.DataPubblicazione != null)
                .OrderByDescending(x => x.Anno).ThenByDescending(x => x.Mese).ThenByDescending(x => x.Id)
                .Select(x => new Hr_MioDocumentoModel { Id = x.Id, Tipo = (int)x.Tipo, Anno = x.Anno, Mese = x.Mese, DataPubblicazione = x.DataPubblicazione })
                .ToListAsync();
            var ids = docs.Select(x => (int?)x.Id).ToList();
            var prese = await _presaVisioneRepository.FindAll(x => x.IdAspNetUsers == ctx.IdUtente && x.IdCompany == ctx.IdCompany && x.Application == (int)ApplicationType.HumanResources
                                                                   && x.Fonte == FontePresaVisione && ids.Contains(x.IdRiferimento) && !x.Annullata)
                                                     .Select(x => new { x.Id, x.IdRiferimento, x.DataConferma, x.Canale }).ToListAsync();
            foreach (var d in docs)
            {
                var p = prese.Where(x => x.IdRiferimento == d.Id).OrderByDescending(x => x.Id).FirstOrDefault();
                d.IdPresaVisione = p?.Id;
                d.DataConferma = p?.DataConferma;
                d.ConfermabileQui = p != null && p.DataConferma == null && p.Canale == ConfirmationChannel.CodeConferma;
            }
            return new Hr_MieiDocumentiOutModel { Dipendente = true, Documenti = docs };
        }

        // ------------------------------------------------------------------------------------------------ file
        private async Task<byte[]> LeggiAsync(string key)
        {
            await using var s = await _fileStorage.OpenAsync(key);
            using var ms = new MemoryStream();
            await s.CopyToAsync(ms);
            return ms.ToArray();
        }

        private async Task EliminaFileAsync(string key)
        {
            try { await _fileStorage.DeleteAsync(key); }
            catch (Exception ex) { Log.Error(ex, "[HR] File {Key} non eliminato", key); }
        }

        public static string NomeFile(string fileName)
            => Path.GetFileName(fileName) is { Length: > 0 } nome ? (nome.Length > 260 ? nome[^260..] : nome) : "file.pdf";
    }
}
