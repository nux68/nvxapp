using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.DocumentManagement;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Common;
using nvxapp.server.data.Repositories.Tenant.DocumentManagement;
using nvxapp.server.service.ClientServer_Service.DocumentManagement._utility;
using nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_ConfigService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_ConfigService
{
    /*
     Configurazione del documentale dell'azienda (ANALISI_DOCUMENTALE.md §6.1): impostazioni, tipologie, cartelle,
     abilitazioni ai documenti del personale. Tipologie, cartelle e impostazioni: CompanyPowerAdmin.
     Abilitazioni dei CompanyAdmin: CompanyPowerAdmin, SuperUser, DealerPowerAdmin del centro (il CompanyPowerAdmin e'
     sempre abilitato e non si modifica).
    */
    public class Doc_ConfigService : ServiceBase, IDoc_ConfigService
    {
        private readonly IDoc_CfgRepository _cfgRepository;
        private readonly IDoc_Par_TipoRepository _tipoRepository;
        private readonly IDoc_CartellaRepository _cartellaRepository;
        private readonly IDoc_UtenteAbilitazioneRepository _abilitazioneRepository;
        private readonly IDoc_DocumentoRepository _documentoRepository;
        private readonly IDoc_Dip_AnagraficaReadRepository _dipendenteReadRepository;
        private readonly IDip_AnagraficaCommonRepository _dipendenteRepository;
        private readonly IAspNetUsersRepository _usersRepository;
        private readonly IAspNetRolesRepository _rolesRepository;
        private readonly IAspNetUserRolesRepository _userRolesRepository;
        private readonly IUserCompanyRepository _userCompanyRepository;
        private readonly IDocumentManagementUtility _utility;

        public Doc_ConfigService(IMapper mapper,
                                 UserManager<ApplicationUser> userManager,
                                 IAspNetUsersRepository aspNetUsersRepository,
                                 IOptions<JwtParameter> jwtParameter,
                                 IHttpContextAccessor httpContextAccessor,
                                 IConfiguration configuration,
                                 IDoc_CfgRepository cfgRepository,
                                 IDoc_Par_TipoRepository tipoRepository,
                                 IDoc_CartellaRepository cartellaRepository,
                                 IDoc_UtenteAbilitazioneRepository abilitazioneRepository,
                                 IDoc_DocumentoRepository documentoRepository,
                                 IDoc_Dip_AnagraficaReadRepository dipendenteReadRepository,
                                 IDip_AnagraficaCommonRepository dipendenteRepository,
                                 IAspNetRolesRepository rolesRepository,
                                 IAspNetUserRolesRepository userRolesRepository,
                                 IUserCompanyRepository userCompanyRepository,
                                 IDocumentManagementUtility utility) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _cfgRepository = cfgRepository;
            _tipoRepository = tipoRepository;
            _cartellaRepository = cartellaRepository;
            _abilitazioneRepository = abilitazioneRepository;
            _documentoRepository = documentoRepository;
            _dipendenteReadRepository = dipendenteReadRepository;
            _dipendenteRepository = dipendenteRepository;
            _usersRepository = aspNetUsersRepository;
            _rolesRepository = rolesRepository;
            _userRolesRepository = userRolesRepository;
            _userCompanyRepository = userCompanyRepository;
            _utility = utility;
        }

        public virtual async Task<GenericResult<Doc_SetupGetOutModel>> Doc_SetupGet(GenericRequest<Doc_SetupGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var retVal = new Doc_SetupGetOutModel
                {
                    Permessi = new Doc_PermessiModel
                    {
                        PowerAdmin = ctx.PowerAdmin,
                        AmministraArchivio = ctx.AmministraArchivio,
                        VedePersonale = ctx.VedePersonale,
                        CaricaPersonale = ctx.CaricaPersonale,
                        GestisceAbilitazioni = ctx.GestisceAbilitazioni,
                        Dipendente = ctx.IdDip_Anagrafica != null
                    },
                    Cfg = await CfgAsync(ctx),
                    Tipi = await TipiAsync(ctx),
                    Cartelle = await CartelleAsync(ctx)
                };

                if (ctx.VedePersonale)
                {
                    await AllineaUtentiAsync(ctx);
                    retVal.Dipendenti = await DipendentiAsync(ctx);
                }

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Doc_CfgPutOutModel>> Doc_CfgPut(GenericRequest<Doc_CfgPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await SoloPowerAdmin();
                var dati = model.Data.Cfg;
                if (dati.GiorniCestino < 0 || dati.VersioniDaConservare < 0)
                    throw new UserMessageException(DocumentManagementMessages.Get("CfgInvalid"));

                var cfg = await _cfgRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstAsync();
                cfg.GiorniCestino = dati.GiorniCestino;
                cfg.VersioniDaConservare = dati.VersioniDaConservare;
                cfg.NotificheEmail = dati.NotificheEmail;
                cfg.ModifiedDate = DateTime.Now;
                cfg.ChangeUser = ctx.IdUtente;
                await _cfgRepository.UpdateAsync(cfg);
                return new Doc_CfgPutOutModel { Cfg = await CfgAsync(ctx) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Doc_TipoPutOutModel>> Doc_TipoPut(GenericRequest<Doc_TipoPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await SoloPowerAdmin();
                var dati = model.Data.Tipo;
                var codice = (dati.Codice ?? string.Empty).Trim().ToUpperInvariant();
                if (codice.Length == 0 || string.IsNullOrWhiteSpace(dati.Descrizione) || (dati.AnniConservazione.HasValue && dati.AnniConservazione <= 0))
                    throw new UserMessageException(DocumentManagementMessages.Get("TypeInvalid"));
                if (await _tipoRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Codice == codice && x.Id != dati.Id).AnyAsync())
                    throw new UserMessageException(DocumentManagementMessages.Get("TypeDuplicate", codice));

                Doc_Par_Tipo tipo;
                if (dati.Id > 0)
                {
                    tipo = await _tipoRepository.FindAll(x => x.Id == dati.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
                           ?? throw new UserMessageException(DocumentManagementMessages.Get("TypeNotFound"));
                    // la categoria decide chi vede i documenti: non si cambia se la tipologia e' gia' usata
                    if (tipo.Categoria != dati.Categoria && await _documentoRepository.FindAll(x => x.IdDoc_Par_Tipo == tipo.Id).AnyAsync())
                        throw new UserMessageException(DocumentManagementMessages.Get("TypeCategoryInUse"));
                }
                else
                {
                    tipo = new Doc_Par_Tipo { IdAz_Anagrafica = ctx.IdAz_Anagrafica, CreationDate = DateTime.Now };
                }

                tipo.Codice = codice;
                tipo.Descrizione = dati.Descrizione.Trim();
                tipo.Categoria = dati.Categoria;
                tipo.AnniConservazione = dati.AnniConservazione;
                tipo.HaScadenza = dati.HaScadenza;
                tipo.PresaVisioneRichiesta = dati.PresaVisioneRichiesta;
                tipo.HaCompetenza = dati.HaCompetenza;
                tipo.Ordine = dati.Ordine;
                tipo.Active = dati.Active;
                tipo.ModifiedDate = DateTime.Now;
                tipo.ChangeUser = ctx.IdUtente;
                tipo = await _tipoRepository.UpsertAsync(tipo);

                return new Doc_TipoPutOutModel { Tipo = (await TipiAsync(ctx)).First(x => x.Id == tipo.Id) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Doc_DeleteOutModel>> Doc_TipoDelete(GenericRequest<Doc_DeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await SoloPowerAdmin();
                var tipo = await _tipoRepository.FindAll(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
                           ?? throw new UserMessageException(DocumentManagementMessages.Get("TypeNotFound"));
                var retVal = new Doc_DeleteOutModel();
                if (await _documentoRepository.FindAll(x => x.IdDoc_Par_Tipo == tipo.Id).AnyAsync())
                {
                    tipo.Active = false;
                    tipo.ModifiedDate = DateTime.Now;
                    await _tipoRepository.UpdateAsync(tipo);
                }
                else
                {
                    await _tipoRepository.DeleteAsync(tipo);
                    retVal.Deleted = true;
                }
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Doc_CartellaPutOutModel>> Doc_CartellaPut(GenericRequest<Doc_CartellaPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await SoloPowerAdmin();
                var dati = model.Data.Cartella;
                if (string.IsNullOrWhiteSpace(dati.Descrizione))
                    throw new UserMessageException(DocumentManagementMessages.Get("FolderInvalid"));

                Doc_Cartella cartella;
                if (dati.Id > 0)
                {
                    cartella = await _cartellaRepository.FindAll(x => x.Id == dati.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
                               ?? throw new UserMessageException(DocumentManagementMessages.Get("FolderNotFound"));
                }
                else
                {
                    cartella = new Doc_Cartella { IdAz_Anagrafica = ctx.IdAz_Anagrafica, CreationDate = DateTime.Now };
                }

                // cartella padre dell'azienda e senza cicli
                if (dati.IdDoc_Cartella.HasValue)
                {
                    var tutte = await _cartellaRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToDictionaryAsync(x => x.Id, x => x.IdDoc_Cartella);
                    int? corrente = dati.IdDoc_Cartella;
                    while (corrente.HasValue)
                    {
                        if (!tutte.ContainsKey(corrente.Value) || (cartella.Id > 0 && corrente == cartella.Id))
                            throw new UserMessageException(DocumentManagementMessages.Get("FolderParentInvalid"));
                        corrente = tutte[corrente.Value];
                    }
                }

                cartella.IdDoc_Cartella = dati.IdDoc_Cartella;
                cartella.Descrizione = dati.Descrizione.Trim();
                cartella.Ordine = dati.Ordine;
                cartella.VisibileUtenti = dati.VisibileUtenti;
                cartella.ModifiedDate = DateTime.Now;
                cartella.ChangeUser = ctx.IdUtente;
                cartella = await _cartellaRepository.UpsertAsync(cartella);

                return new Doc_CartellaPutOutModel { Cartella = (await CartelleAsync(ctx)).First(x => x.Id == cartella.Id) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Doc_DeleteOutModel>> Doc_CartellaDelete(GenericRequest<Doc_DeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await SoloPowerAdmin();
                var cartella = await _cartellaRepository.FindAll(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
                               ?? throw new UserMessageException(DocumentManagementMessages.Get("FolderNotFound"));
                if (cartella.DiSistema)
                    throw new UserMessageException(DocumentManagementMessages.Get("FolderSystem"));
                if (await _cartellaRepository.FindAll(x => x.IdDoc_Cartella == cartella.Id).AnyAsync() || await _documentoRepository.FindAll(x => x.IdDoc_Cartella == cartella.Id).AnyAsync())
                    throw new UserMessageException(DocumentManagementMessages.Get("FolderNotEmpty"));

                await _cartellaRepository.DeleteAsync(cartella);
                return new Doc_DeleteOutModel { Deleted = true };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Doc_AbilitazioneListOutModel>> Doc_AbilitazioneList(GenericRequest<Doc_AbilitazioneListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await SoloGestoreAbilitazioni();
                return new Doc_AbilitazioneListOutModel { Abilitazioni = await AbilitazioniAsync(ctx) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Doc_AbilitazionePutOutModel>> Doc_AbilitazionePut(GenericRequest<Doc_AbilitazionePutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await SoloGestoreAbilitazioni();
                var dati = model.Data.Abilitazione;

                // solo gli amministratori dell'azienda (CompanyPowerAdmin e CompanyAdmin) si abilitano
                var ammessi = await AbilitazioniAsync(ctx);
                var ammesso = ammessi.FirstOrDefault(x => x.IdAspNetUsers == dati.IdAspNetUsers);
                if (ammesso == null)
                    throw new UserMessageException(DocumentManagementMessages.Get("PermissionUserInvalid"));
                if (ammesso.Fisso)
                    throw new UserMessageException(DocumentManagementMessages.Get("PermissionFixed"));

                var row = await _abilitazioneRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.IdAspNetUsers == dati.IdAspNetUsers).FirstOrDefaultAsync()
                          ?? new Doc_UtenteAbilitazione { IdAz_Anagrafica = ctx.IdAz_Anagrafica, IdAspNetUsers = dati.IdAspNetUsers, CreationDate = DateTime.Now };
                // chi carica vede anche
                row.CaricaPersonale = dati.CaricaPersonale;
                row.VedePersonale = dati.VedePersonale || dati.CaricaPersonale;
                row.ModifiedDate = DateTime.Now;
                row.ChangeUser = ctx.IdUtente;
                await _abilitazioneRepository.UpsertAsync(row);

                return new Doc_AbilitazionePutOutModel { Abilitazione = (await AbilitazioniAsync(ctx)).First(x => x.IdAspNetUsers == dati.IdAspNetUsers) };
            }, isSubProcess);
        }

        /*
         Dipendente (tabella comune Dip_Anagrafica): con il solo documentale i dipendenti si inseriscono qui, anche senza
         account (ANALISI_DOCUMENTALE.md §2.1). Chi carica i documenti del personale. Codice fiscale e matricola unici
         nell'azienda. Utente collegato, azienda e dati delle presenze non si toccano da qui.
        */
        public virtual async Task<GenericResult<Doc_DipendentePutOutModel>> Doc_DipendentePut(GenericRequest<Doc_DipendentePutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                if (!ctx.CaricaPersonale)
                    throw new UserMessageException(DocumentManagementMessages.Get("PersonnelNotAllowed"));

                var dati = model.Data.Dipendente;
                var cognome = dati.Cognome?.Trim();
                var nome = dati.Nome?.Trim();
                var cf = string.IsNullOrWhiteSpace(dati.CodiceFiscale) ? null : dati.CodiceFiscale.Trim().ToUpperInvariant();
                var matricola = string.IsNullOrWhiteSpace(dati.Matricola) ? null : dati.Matricola.Trim();
                if (string.IsNullOrEmpty(cognome) && string.IsNullOrEmpty(nome))
                    throw new UserMessageException(DocumentManagementMessages.Get("EmployeeNameRequired"));
                if (cf != null && cf.Length != 16 && cf.Length != 11)
                    throw new UserMessageException(DocumentManagementMessages.Get("EmployeeTaxCodeInvalid"));
                if (cf != null && await _dipendenteReadRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.CodiceFiscale == cf && x.Id != dati.Id).AnyAsync())
                    throw new UserMessageException(DocumentManagementMessages.Get("EmployeeTaxCodeDuplicate", cf));
                if (matricola != null && await _dipendenteReadRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Matricola == matricola && x.Id != dati.Id).AnyAsync())
                    throw new UserMessageException(DocumentManagementMessages.Get("EmployeeNumberDuplicate", matricola));

                nvxapp.server.data.Entities.Tenant.Dip_Anagrafica dip;
                if (dati.Id > 0)
                {
                    dip = await _dipendenteRepository.FindAll(x => x.Id == dati.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
                          ?? throw new UserMessageException(DocumentManagementMessages.Get("EmployeeNotFound"));
                }
                else
                {
                    dip = new nvxapp.server.data.Entities.Tenant.Dip_Anagrafica { IdAz_Anagrafica = ctx.IdAz_Anagrafica, CreationDate = DateTime.Now };
                }
                dip.Cognome = cognome;
                dip.Nome = nome;
                dip.CodiceFiscale = cf;
                dip.Matricola = matricola;
                dip.Email = string.IsNullOrWhiteSpace(dati.Email) ? null : dati.Email.Trim();
                dip.DataCessazione = dati.DataCessazione?.Date;
                dip.Attivo = dati.Attivo;
                dip.ModifiedDate = DateTime.Now;
                dip.ChangeUser = ctx.IdUtente;
                dip = await _dipendenteRepository.UpsertAsync(dip);

                return new Doc_DipendentePutOutModel { Dipendente = (await DipendentiAsync(ctx)).First(x => x.Id == dip.Id) };
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------

        private async Task<List<Doc_DipendenteModel>> DipendentiAsync(DocContesto ctx)
        {
            var dipendenti = await _dipendenteReadRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToListAsync();
            var idUtenti = dipendenti.Where(x => x.IdAspNetUsers != null).Select(x => x.IdAspNetUsers!).ToList();
            var nomi = await _usersRepository.FindAll(u => idUtenti.Contains(u.Id)).Select(u => new { u.Id, u.UserName }).ToListAsync();
            return dipendenti.Select(x => new Doc_DipendenteModel
                             {
                                 Id = x.Id, Nominativo = (x.Cognome + " " + x.Nome).Trim(), Cognome = x.Cognome, Nome = x.Nome,
                                 CodiceFiscale = x.CodiceFiscale, Matricola = x.Matricola, Email = x.Email, DataCessazione = x.DataCessazione,
                                 Attivo = x.Attivo, HaUtente = x.IdAspNetUsers != null,
                                 UserName = nomi.FirstOrDefault(n => n.Id == x.IdAspNetUsers)?.UserName
                             })
                             .OrderBy(x => x.Nominativo)
                             .ToList();
        }

        /*
         Gli utenti dell'azienda con il ruolo User (i dipendenti con accesso all'app) sono dipendenti anche nel documentale:
         chi non ha ancora l'anagrafica comune (es. le presenze la creano solo al primo accesso, o le presenze non sono attive)
         la riceve qui, con il nome utente come cognome provvisorio (modificabile). Le presenze completano poi i loro dati
         (rapporto di lavoro, reparto e competenza) al primo accesso.
        */
        private async Task AllineaUtentiAsync(DocContesto ctx)
        {
            var idRuoloUser = await _rolesRepository.FindAll(r => r.Name == nameof(RoleCode.User)).Select(r => r.Id).FirstOrDefaultAsync();
            if (idRuoloUser == null)
                return;
            var ruoliUser = _userRolesRepository.FindAll(r => r.RoleId == idRuoloUser);
            var utenti = await _userCompanyRepository.FindAll(x => x.IdCompany == ctx.IdCompany && !x.Disabled)
                                        .Where(x => ruoliUser.Any(r => r.UserId == x.IdAspNetUsers))
                                        .Select(x => x.IdAspNetUsers).Distinct().ToListAsync();
            var conAnagrafica = await _dipendenteReadRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.IdAspNetUsers != null)
                                         .Select(x => x.IdAspNetUsers!).ToListAsync();
            var mancanti = utenti.Except(conAnagrafica).ToList();
            if (mancanti.Count == 0)
                return;

            var nomi = await _usersRepository.FindAll(u => mancanti.Contains(u.Id)).Select(u => new { u.Id, u.UserName }).ToListAsync();
            foreach (var idUtente in mancanti)
                // tabella comune: si scrive con il repository Common (un solo salvataggio per tutti)
                await _dipendenteRepository.CreateAsync(new nvxapp.server.data.Entities.Tenant.Dip_Anagrafica
                {
                    IdAz_Anagrafica = ctx.IdAz_Anagrafica,
                    IdAspNetUsers = idUtente,
                    Cognome = nomi.FirstOrDefault(n => n.Id == idUtente)?.UserName,
                    Attivo = true,
                    CreationDate = DateTime.Now,
                    ChangeUser = ctx.IdUtente
                }, false);
            try
            {
                await _dipendenteRepository.SaveChange();
            }
            catch (DbUpdateException ex)
            {
                // creata nel frattempo da un'altra richiesta (es. le presenze): indice unico azienda/utente
                ex.Entries.FirstOrDefault()?.Context.ChangeTracker.Clear();
            }
        }

        private async Task<Doc_CfgModel> CfgAsync(DocContesto ctx)
        {
            var cfg = await _cfgRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstAsync();
            return new Doc_CfgModel { GiorniCestino = cfg.GiorniCestino, VersioniDaConservare = cfg.VersioniDaConservare, NotificheEmail = cfg.NotificheEmail };
        }

        private async Task<List<Doc_Par_TipoModel>> TipiAsync(DocContesto ctx)
        {
            var conteggi = await _documentoRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                                  .GroupBy(x => x.IdDoc_Par_Tipo).Select(g => new { g.Key, Count = g.Count() }).ToListAsync();
            return (await _tipoRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).OrderBy(x => x.Ordine).ThenBy(x => x.Descrizione).ToListAsync())
                   .Select(x => new Doc_Par_TipoModel
                   {
                       Id = x.Id, Codice = x.Codice, Descrizione = x.Descrizione, Categoria = x.Categoria, AnniConservazione = x.AnniConservazione,
                       HaScadenza = x.HaScadenza, PresaVisioneRichiesta = x.PresaVisioneRichiesta, HaCompetenza = x.HaCompetenza,
                       Ordine = x.Ordine, Active = x.Active, Documenti = conteggi.FirstOrDefault(c => c.Key == x.Id)?.Count ?? 0
                   })
                   .ToList();
        }

        private async Task<List<Doc_CartellaModel>> CartelleAsync(DocContesto ctx)
        {
            var conteggi = await _documentoRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.IdDoc_Cartella != null && !x.NelCestino)
                                                  .GroupBy(x => x.IdDoc_Cartella).Select(g => new { g.Key, Count = g.Count() }).ToListAsync();
            var tutte = await _cartellaRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToListAsync();
            var perId = tutte.ToDictionary(x => x.Id);

            string Percorso(Doc_Cartella c)
            {
                var parti = new List<string>();
                var visti = new HashSet<int>();
                for (Doc_Cartella? x = c; x != null && visti.Add(x.Id); x = x.IdDoc_Cartella.HasValue && perId.TryGetValue(x.IdDoc_Cartella.Value, out var p) ? p : null)
                    parti.Insert(0, x.Descrizione);
                return string.Join(" / ", parti);
            }

            // utenti senza ruolo di amministratore: solo le cartelle visibili agli utenti
            return tutte.Where(x => ctx.AmministraArchivio || x.VisibileUtenti)
                        .Select(x => new Doc_CartellaModel
                        {
                            Id = x.Id, IdDoc_Cartella = x.IdDoc_Cartella, Descrizione = x.Descrizione, Percorso = Percorso(x),
                            Ordine = x.Ordine, VisibileUtenti = x.VisibileUtenti, DiSistema = x.DiSistema,
                            Documenti = conteggi.FirstOrDefault(c => c.Key == x.Id)?.Count ?? 0
                        })
                        .OrderBy(x => x.Percorso)
                        .ToList();
        }

        private async Task<List<Doc_AbilitazioneModel>> AbilitazioniAsync(DocContesto ctx)
        {
            var ruoli = await _rolesRepository.FindAll(r => r.Name == nameof(RoleCode.CompanyPowerAdmin) || r.Name == nameof(RoleCode.CompanyAdmin))
                                             .Select(r => new { r.Id, r.Name }).ToListAsync();
            var idRuoli = ruoli.Select(r => r.Id).ToList();
            var utenti = await _userCompanyRepository.FindAll(x => x.IdCompany == ctx.IdCompany && !x.Disabled)
                                                    .Select(x => x.IdAspNetUsers).Distinct().ToListAsync();
            var conRuolo = await _userRolesRepository.FindAll(x => utenti.Contains(x.UserId) && idRuoli.Contains(x.RoleId)).ToListAsync();
            var nomi = await _usersRepository.FindAll(u => utenti.Contains(u.Id)).Select(u => new { u.Id, u.UserName }).ToListAsync();
            var abil = await _abilitazioneRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToListAsync();

            var idPowerAdmin = ruoli.FirstOrDefault(r => r.Name == nameof(RoleCode.CompanyPowerAdmin))?.Id;

            // il CompanyPowerAdmin (anche se e' pure CompanyAdmin) e' sempre abilitato: riga fissa, non modificabile
            return conRuolo.GroupBy(x => x.UserId)
                           .Select(g =>
                           {
                               bool fisso = g.Any(r => r.RoleId == idPowerAdmin);
                               var a = abil.FirstOrDefault(x => x.IdAspNetUsers == g.Key);
                               return new Doc_AbilitazioneModel
                               {
                                   IdAspNetUsers = g.Key,
                                   UserName = nomi.FirstOrDefault(n => n.Id == g.Key)?.UserName ?? g.Key,
                                   Ruoli = string.Join(", ", g.Select(r => ruoli.First(x => x.Id == r.RoleId).Name)),
                                   VedePersonale = fisso || (a?.VedePersonale ?? false),
                                   CaricaPersonale = fisso || (a?.CaricaPersonale ?? false),
                                   Fisso = fisso,
                               };
                           })
                           .OrderBy(x => x.UserName)
                           .ToList();
        }

        private async Task<DocContesto> SoloPowerAdmin()
        {
            var ctx = await _utility.ContestoAsync();
            if (!ctx.PowerAdmin)
                throw new UserMessageException(DocumentManagementMessages.Get("NotAllowed"));
            return ctx;
        }

        private async Task<DocContesto> SoloGestoreAbilitazioni()
        {
            var ctx = await _utility.ContestoAsync();
            if (!ctx.GestisceAbilitazioni)
                throw new UserMessageException(DocumentManagementMessages.Get("NotAllowed"));
            return ctx;
        }
    }

    public interface IDoc_ConfigService : IServiceBase
    {
        Task<GenericResult<Doc_SetupGetOutModel>> Doc_SetupGet(GenericRequest<Doc_SetupGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<Doc_CfgPutOutModel>> Doc_CfgPut(GenericRequest<Doc_CfgPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Doc_TipoPutOutModel>> Doc_TipoPut(GenericRequest<Doc_TipoPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Doc_DeleteOutModel>> Doc_TipoDelete(GenericRequest<Doc_DeleteInModel> model, Boolean isSubProcess);
        Task<GenericResult<Doc_CartellaPutOutModel>> Doc_CartellaPut(GenericRequest<Doc_CartellaPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Doc_DeleteOutModel>> Doc_CartellaDelete(GenericRequest<Doc_DeleteInModel> model, Boolean isSubProcess);
        Task<GenericResult<Doc_AbilitazioneListOutModel>> Doc_AbilitazioneList(GenericRequest<Doc_AbilitazioneListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Doc_AbilitazionePutOutModel>> Doc_AbilitazionePut(GenericRequest<Doc_AbilitazionePutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Doc_DipendentePutOutModel>> Doc_DipendentePut(GenericRequest<Doc_DipendentePutInModel> model, Boolean isSubProcess);
    }
}
