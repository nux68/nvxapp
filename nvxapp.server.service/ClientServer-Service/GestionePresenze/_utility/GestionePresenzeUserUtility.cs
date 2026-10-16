using AutoMapper;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    public class GestionePresenzeUserUtility : IServiceBase, IGestionePresenzeUserUtility
    {
        protected readonly IMapper _mapper;
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;

        private readonly IAz_AnagraficaRepository _az_AnagraficaRepository;
        private readonly IAz_SediRepository _az_SediRepository;
        private readonly IAz_SediRepartoRepository _az_RepartoRepository;
        private readonly IAz_CfgRepository _az_CfgRepository;

        private readonly IPar_CompetenzaRepository _par_CompetenzaRepository;
        private readonly IPar_AttivitaRepository _par_AttivitaRepository;
        private readonly IPar_AttivitaCompetenzaRepository _par_AttivitaCompetenzaRepository;


        private readonly IAz_ClienteRepository _az_ClienteRepository;
        private readonly IAz_CommessaRepository _az_CommessaRepository;
        private readonly IAz_SubCommessaRepository _az_SubCommessaRepository;
        private readonly IAz_SubCommessaAttivitaRepository _az_SubCommessaAttivitaRepository;
        private readonly IAz_SediAttivitaRepository _az_SediAttivitaRepository;
        private readonly IAz_SediRepartoAttivitaRepository _az_SediRepartoAttivitaRepository;

        private readonly IAz_SubCommessaSediRepartoRepository _az_SubCommessaSediRepartoRepository;
        private readonly IUserCompanyRepository _userCompanyRepository;
        private readonly IDip_ProfiloOrarioRepository _dip_ProfiloOrarioRepository;
        private readonly IGestionePresenzeParametriDefault _gestionePresenzeParametriDefault;
        private readonly IAz_SediRepartoUserRepository _az_SediRepartoUserRepository;
        private readonly IAspNetUserRolesRepository _aspNetUserRolesRepository;
        private readonly IAspNetRolesRepository _aspNetRolesRepository;
        private readonly IDip_PeriodoRepository _dip_PeriodoRepository;
        private readonly IDip_CompetenzaRepository _dip_CompetenzaRepository;
        private readonly IAz_SubCommessaUserRepository _az_SubCommessaUserRepository;
        private readonly IDip_GG_TimbraturaRepository _dip_GG_TimbraturaRepository;



        public GestionePresenzeUserUtility(IMapper mapper,
                                           IDip_AnagraficaRepository dip_AnagraficaRepository,
                                           IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                           IAz_AnagraficaRepository az_AnagraficaRepository,
                                           IAz_SediRepository az_SediRepository,
                                           IAz_SediRepartoRepository az_RepartoRepository,
                                           IAz_CfgRepository az_CfgRepository,
                                           IPar_CompetenzaRepository par_CompetenzaRepository,
                                           IPar_AttivitaCompetenzaRepository par_AttivitaCompetenzaRepository,
                                           IPar_AttivitaRepository par_AttivitaRepository,
                                           IAz_ClienteRepository az_ClienteRepository,
                                           IAz_CommessaRepository az_CommessaRepository,
                                           IAz_SubCommessaRepository az_SubCommessaRepository,
                                           IAz_SubCommessaAttivitaRepository az_SubCommessaAttivitaRepository,
                                           IAz_SediAttivitaRepository az_SediAttivitaRepository,
                                           IAz_SediRepartoAttivitaRepository az_SediRepartoAttivitaRepository,
                                           IAz_SubCommessaSediRepartoRepository az_SubCommessaSediRepartoRepository,
                                           IUserCompanyRepository userCompanyRepository,
                                           IDip_ProfiloOrarioRepository dip_ProfiloOrarioRepository,
                                           IGestionePresenzeParametriDefault gestionePresenzeParametriDefault,
                                           IAz_SediRepartoUserRepository az_SediRepartoUserRepository,
                                           IAspNetUserRolesRepository aspNetUserRolesRepository,
                                           IAspNetRolesRepository aspNetRolesRepository,
                                           IDip_PeriodoRepository dip_PeriodoRepository,
                                           IDip_CompetenzaRepository dip_CompetenzaRepository,
                                           IAz_SubCommessaUserRepository az_SubCommessaUserRepository,
                                           IDip_GG_TimbraturaRepository dip_GG_TimbraturaRepository
                                           )
        {
            _mapper = mapper;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;

            _az_AnagraficaRepository = az_AnagraficaRepository;
            _az_SediRepository = az_SediRepository;
            _az_RepartoRepository = az_RepartoRepository;
            _az_CfgRepository = az_CfgRepository;

            _par_CompetenzaRepository = par_CompetenzaRepository;
            _par_AttivitaRepository = par_AttivitaRepository;
            _par_AttivitaCompetenzaRepository = par_AttivitaCompetenzaRepository;
            _az_ClienteRepository = az_ClienteRepository;
            _az_CommessaRepository = az_CommessaRepository;
            _az_SubCommessaRepository = az_SubCommessaRepository;
            _az_SubCommessaAttivitaRepository = az_SubCommessaAttivitaRepository;
            _az_SediAttivitaRepository = az_SediAttivitaRepository;
            _az_SediRepartoAttivitaRepository = az_SediRepartoAttivitaRepository;
            _az_SubCommessaSediRepartoRepository = az_SubCommessaSediRepartoRepository;
            _userCompanyRepository = userCompanyRepository;
            _dip_ProfiloOrarioRepository = dip_ProfiloOrarioRepository;
            _gestionePresenzeParametriDefault = gestionePresenzeParametriDefault;
            _az_SediRepartoUserRepository = az_SediRepartoUserRepository;
            _aspNetUserRolesRepository = aspNetUserRolesRepository;
            _aspNetRolesRepository = aspNetRolesRepository;
            _dip_PeriodoRepository = dip_PeriodoRepository;
            _dip_CompetenzaRepository = dip_CompetenzaRepository;
            _az_SubCommessaUserRepository = az_SubCommessaUserRepository;
            _dip_GG_TimbraturaRepository = dip_GG_TimbraturaRepository;

        }

        public async Task<User_DATA_COMB_DipAna_DipRapp> Get_DipAna_DipRapp(string IdAspNetUsers, bool InitIfNotExsist)
        {
            User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = new User_DATA_COMB_DipAna_DipRapp();

            user_DATA_COMB_DipAna_DipRapp.dip_Anagrafica = await _dip_AnagraficaRepository.FindAll(x => x.IdAspNetUsers == IdAspNetUsers).FirstOrDefaultAsync();
            if (user_DATA_COMB_DipAna_DipRapp.dip_Anagrafica == null)
            {
                if (InitIfNotExsist)
                {
                    // Cognome e Nome iniziali: personaggi Disney in sequenza (modificabili poi dall'anagrafica)
                    var (cognome, nome) = PersonaggioDisney(_dip_AnagraficaRepository.FindAll(x => true).Count());
                    user_DATA_COMB_DipAna_DipRapp.dip_Anagrafica = new Dip_Anagrafica()
                    {
                        IdAspNetUsers = IdAspNetUsers,
                        Cognome = cognome,
                        Nome = nome
                    };
                    await _dip_AnagraficaRepository.UpsertAsync(user_DATA_COMB_DipAna_DipRapp.dip_Anagrafica);

                    // nuovo dipendente: assegnazione al reparto e alla competenza di default dell'azienda
                    var usrCompany = _userCompanyRepository.FindAll(x => x.IdAspNetUsers == IdAspNetUsers).FirstOrDefault();
                    if (usrCompany != null)
                    {
                        await AssegnaRepartoDefault(IdAspNetUsers, usrCompany.IdCompany);

                        var company_DATA = await Get_AzAna_AzSedi_AzReparto_Az_Cfg(usrCompany.IdCompany, true);
                        if (company_DATA.az_Anagrafica != null)
                            await CompletaCompetenzaDefault(new List<string> { IdAspNetUsers }, company_DATA.az_Anagrafica.Id);
                    }
                }
                else
                {
                    throw new Exception("Anagrafica non trovata");
                }
            }

            user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro = await _dip_RapportoLavoroRepository.FindAll(x => x.IdDip_Anagrafica == user_DATA_COMB_DipAna_DipRapp.dip_Anagrafica.Id).FirstOrDefaultAsync();
            if (user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro == null)
            {
                if (InitIfNotExsist)
                {

                    var usrC = _userCompanyRepository.FindAll(x => x.IdAspNetUsers == IdAspNetUsers).FirstOrDefault();
                    if (usrC != null)
                    {
                        var res_1 = await Get_Az_SubCommessaAttivita_Default(usrC.IdCompany);
                        var company_DATA = await Get_AzAna_AzSedi_AzReparto_Az_Cfg(usrC.IdCompany, true);

                        // assunzione (e profilo orario) dal primo giorno del mese di inizializzazione dell'azienda
                        var creazione = company_DATA.az_Anagrafica?.CreationDate ?? DateTime.Now;
                        user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro = new Dip_RapportoLavoro
                        {
                            IdDip_Anagrafica = user_DATA_COMB_DipAna_DipRapp.dip_Anagrafica.Id,
                            DataAss = new DateTime(creazione.Year, creazione.Month, 1),
                            IdAz_SubCommessaAttivita = res_1.Az_SubCommessaAttivita.Id
                        };
                        await _dip_RapportoLavoroRepository.UpsertAsync(user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro);

                        // Profilo orario di default dell'azienda (DAY7H8) dalla data di assunzione
                        var par_ProfiloOrario = _gestionePresenzeParametriDefault.GetProfiloOrarioDefault(company_DATA.az_Anagrafica!.Id);
                        if (par_ProfiloOrario != null)
                        {
                            await _dip_ProfiloOrarioRepository.UpsertAsync(new Dip_ProfiloOrario
                            {
                                IdDip_RapportoLavoro = user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id,
                                Dal = user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.DataAss!.Value,
                                Al = new DateTime(2099, 12, 31),
                                IdPar_ProfiloOrario = par_ProfiloOrario.Id,
                                NumGiornoPartenzaCiclo = 1
                            });
                        }
                        //}
                    }




                }
                else
                {
                    throw new Exception("Rapporto lavoro non trovato");
                }
            }



            return user_DATA_COMB_DipAna_DipRapp;
        }

        // Nomi di partenza per le anagrafiche dei dipendenti: personaggi di Topolino e Paperino.
        // Finito l'elenco si riparte con un numero progressivo nel nome (es. "Topolino 2").
        private static readonly (string Cognome, string Nome)[] _personaggiDisney =
        {
            ("Mouse", "Topolino"),
            ("Mouse", "Minni"),
            ("Paperino", "Paolino"),
            ("De' Paperoni", "Paperon"),
            ("Papera", "Paperina"),
            ("Duck", "Qui"),
            ("Duck", "Quo"),
            ("Duck", "Qua"),
            ("Goof", "Pippo"),
            ("Paperone", "Gastone"),
            ("Pitagorico", "Archimede"),
            ("Papera", "Nonna"),
            ("Duck", "Paperoga"),
            ("Cavezza", "Orazio"),
            ("Cow", "Clarabella"),
            ("Gambadilegno", "Pietro"),
            ("Beta", "Eta"),
            ("Basettoni", "Commissario"),
            ("Manetta", "Ispettore"),
            ("Nera", "Macchia"),
            ("Rockerduck", "John"),
            ("McBridge", "Brigitta"),
        };

        private static (string Cognome, string Nome) PersonaggioDisney(int indice)
        {
            var p = _personaggiDisney[indice % _personaggiDisney.Length];
            int giro = indice / _personaggiDisney.Length;
            return giro == 0 ? p : (p.Cognome, $"{p.Nome} {giro + 1}");
        }

        // Completa Cognome e Nome delle anagrafiche esistenti che li hanno entrambi vuoti
        // (create prima dell'assegnazione automatica), con lo stesso criterio delle nuove:
        // il personaggio dipende dalla posizione dell'anagrafica nella tabella.
        public async Task CompletaCognomeNome(List<string> IdAspNetUsers)
        {
            var vuote = _dip_AnagraficaRepository.FindAll(x => IdAspNetUsers.Contains(x.IdAspNetUsers) &&
                                                               (x.Cognome == null || x.Cognome == "") &&
                                                               (x.Nome == null || x.Nome == ""))
                                                 .OrderBy(x => x.Id)
                                                 .ToList();
            foreach (var dip_Anagrafica in vuote)
            {
                var (cognome, nome) = PersonaggioDisney(_dip_AnagraficaRepository.FindAll(x => x.Id < dip_Anagrafica.Id).Count());
                dip_Anagrafica.Cognome = cognome;
                dip_Anagrafica.Nome = nome;
                await _dip_AnagraficaRepository.UpsertAsync(dip_Anagrafica);
            }
        }

        // Competenza di default dell'azienda (creata con l'azienda e collegata all'attivita' di default)
        public Par_Competenza? GetCompetenzaDefault(int IdAz_Anagrafica)
        {
            return _par_CompetenzaRepository.FindAll(x => x.IdAz_Anagrafica == IdAz_Anagrafica && x.Default)
                                            .OrderBy(x => x.Id)
                                            .FirstOrDefault();
        }

        // Assegna la competenza di default ai dipendenti che non hanno ancora competenze:
        // ogni dipendente ha almeno una competenza, cosi' puo' svolgere l'attivita' di default.
        public async Task CompletaCompetenzaDefault(List<string> IdAspNetUsers, int IdAz_Anagrafica)
        {
            var par_Competenza = GetCompetenzaDefault(IdAz_Anagrafica);
            if (par_Competenza == null)
                return;

            var idDip_Anagrafica_List = _dip_AnagraficaRepository.FindAll(x => IdAspNetUsers.Contains(x.IdAspNetUsers))
                                                                 .Select(x => x.Id)
                                                                 .ToList();
            var conCompetenze = _dip_CompetenzaRepository.FindAll(x => idDip_Anagrafica_List.Contains(x.IdDip_Anagrafica))
                                                         .Select(x => x.IdDip_Anagrafica)
                                                         .Distinct()
                                                         .ToList();
            var senzaCompetenze = idDip_Anagrafica_List.Except(conCompetenze).ToList();
            foreach (var idDip_Anagrafica in senzaCompetenze)
            {
                await _dip_CompetenzaRepository.UpsertAsync(new Dip_Competenza
                {
                    IdDip_Anagrafica = idDip_Anagrafica,
                    IdPar_Competenza = par_Competenza.Id
                });
            }
        }

        // Assegna l'utente al reparto di default dell'azienda in base al ruolo:
        // CompanyAdmin / CompanyPowerAdmin -> amministratore del reparto (EnabledToAdmin), abilitato
        //                                     all'approvazione e accodato agli approvatori (ApprovalZOrder)
        // User                             -> utente del reparto (UserInDepartment)
        // Non fa nulla se l'utente e' gia' presente nel reparto.
        private async Task AssegnaRepartoDefault(string IdAspNetUsers, int IdCompany)
        {
            var company_DATA = await Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
            if (company_DATA.az_Anagrafica == null)
                return;

            // reparto Default della sede Default (altrimenti quello restituito da Get_AzAna_AzSedi_AzReparto_Az_Cfg)
            var idSedi = _az_SediRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA.az_Anagrafica.Id)
                                           .OrderByDescending(x => x.Default).ThenBy(x => x.Id)
                                           .Select(x => x.Id).ToList();
            var reparto = _az_RepartoRepository.FindAll(x => idSedi.Contains(x.IdAz_Sedi) && x.Default)
                                               .AsEnumerable()
                                               .OrderBy(x => idSedi.IndexOf(x.IdAz_Sedi)).ThenBy(x => x.Id)
                                               .FirstOrDefault() ?? company_DATA.az_SediReparto;
            if (reparto == null)
                return;

            if (_az_SediRepartoUserRepository.FindAll(x => x.IdAz_SediReparto == reparto.Id && x.IdAspNetUsers == IdAspNetUsers).Any())
                return;

            var roleIds = _aspNetUserRolesRepository.FindAll(x => x.UserId == IdAspNetUsers).Select(x => x.RoleId).ToList();
            var roleCodes = _aspNetRolesRepository.FindAll(x => roleIds.Contains(x.Id)).Select(x => x.Code).ToList();

            bool isAdmin = roleCodes.Contains(RoleCode.CompanyAdmin) || roleCodes.Contains(RoleCode.CompanyPowerAdmin);
            bool isUser = roleCodes.Contains(RoleCode.User);
            if (!isAdmin && !isUser)
                return;

            int approvalZOrder = 0;
            if (isAdmin)
                approvalZOrder = (_az_SediRepartoUserRepository.FindAll(x => x.IdAz_SediReparto == reparto.Id && x.EnabledToApproval)
                                                               .Select(x => (int?)x.ApprovalZOrder)
                                                               .Max() ?? 0) + 1;

            await _az_SediRepartoUserRepository.UpsertAsync(new Az_SediRepartoUser
            {
                IdAz_SediReparto = reparto.Id,
                IdAspNetUsers = IdAspNetUsers,
                EnabledToAdmin = isAdmin,
                EnabledToApproval = isAdmin,
                ApprovalZOrder = approvalZOrder,
                UserInDepartment = isUser
            });
        }

        public async Task<User_DATA_COMB_DipAna_DipRapp> Get_DipRapp_DipAna(int IdDipRapp)
        {
            User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = new User_DATA_COMB_DipAna_DipRapp();



            user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro = await _dip_RapportoLavoroRepository.FindAll(x => x.Id == IdDipRapp).FirstOrDefaultAsync();
            if (user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro == null)
            {
                throw new Exception("Rapporto lavoro non trovato");
            }

            user_DATA_COMB_DipAna_DipRapp.dip_Anagrafica = await _dip_AnagraficaRepository.FindAll(x => x.Id == user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.IdDip_Anagrafica).FirstOrDefaultAsync();
            if (user_DATA_COMB_DipAna_DipRapp.dip_Anagrafica == null)
            {
                throw new Exception("Anagrafica non trovata");
            }


            return user_DATA_COMB_DipAna_DipRapp;
        }

        public async Task<Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg> Get_AzAna_AzSedi_AzReparto_Az_Cfg(int IdCompany, bool InitIfNotExsist)
        {
            Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = new Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg();

            Boolean isNewCompany = false;

            company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica = await _az_AnagraficaRepository.FindAll(x => x.IdCompany == IdCompany).FirstOrDefaultAsync();
            if (company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica == null)
            {
                if (InitIfNotExsist)
                {
                    isNewCompany = true;
                    company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica = new Az_Anagrafica()
                    {
                        IdCompany = IdCompany,
                    };
                    company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica = await _az_AnagraficaRepository.UpsertAsync(company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica);
                }
                else
                {
                    throw new Exception("Az_Anagrafica non trovata");
                }
            }

            //Az_Cfg
            company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Cfg = await _az_CfgRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id).FirstOrDefaultAsync();
            if (company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Cfg == null)
            {
                if (InitIfNotExsist)
                {
                    company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Cfg = new Az_Cfg()
                    {
                        IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id
                    };
                    company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Cfg = await _az_CfgRepository.UpsertAsync(company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Cfg);
                }
                else
                {
                    throw new Exception("Az_Cfg non trovata");
                }
            }

            //Sedi
            company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi = await _az_SediRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id).FirstOrDefaultAsync();
            if (company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi == null)
            {
                if (InitIfNotExsist)
                {
                    company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi = new Az_Sedi
                    {
                        IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id,
                        Descrizione = "Default",
                        Default = true
                    };
                    company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi = await _az_SediRepository.UpsertAsync(company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi);
                }
                else
                {
                    throw new Exception("Sede non trovata");
                }
            }

            //reparto
            company_DATA_COMB_AzAna_AzSedi_AzReparto.az_SediReparto = await _az_RepartoRepository.FindAll(x => x.IdAz_Sedi == company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi.Id).FirstOrDefaultAsync();
            if (company_DATA_COMB_AzAna_AzSedi_AzReparto.az_SediReparto == null)
            {
                if (InitIfNotExsist)
                {
                    company_DATA_COMB_AzAna_AzSedi_AzReparto.az_SediReparto = new Az_SediReparto
                    {
                        IdAz_Sedi = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi.Id,
                        Descrizione = "Default",
                        Default = true
                    };
                    company_DATA_COMB_AzAna_AzSedi_AzReparto.az_SediReparto = await _az_RepartoRepository.UpsertAsync(company_DATA_COMB_AzAna_AzSedi_AzReparto.az_SediReparto);
                }
                else
                {
                    throw new Exception("Sede non trovata");
                }
            }


            //iniziallizzazione varie alla creazione della azienda
            if (InitIfNotExsist && isNewCompany)
            {
                await InitDataCompany(company_DATA_COMB_AzAna_AzSedi_AzReparto);
            }

            return company_DATA_COMB_AzAna_AzSedi_AzReparto;
        }


        public async Task<Get_Az_SubCommessaAttivita_Default_OutModel> Get_Az_SubCommessaAttivita_Default(int IdCompany)
        {
            Get_Az_SubCommessaAttivita_Default_OutModel retVal = new Get_Az_SubCommessaAttivita_Default_OutModel();


            Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await this.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
            if (company_DATA != null && company_DATA.az_Anagrafica != null)
            {
                int idAz = company_DATA.az_Anagrafica.Id;

                // ?? 1. Cliente default ???????????????????????????????????????????
                var cliente = (await _az_ClienteRepository.FindAll())
                    .FirstOrDefault(c => c.IdAz_Anagrafica == idAz && c.Default);

                if (cliente == null)
                {
                    cliente = await _az_ClienteRepository.UpsertAsync(new Az_Cliente
                    {
                        IdAz_Anagrafica = idAz,
                        Descrizione = "Default",
                        Default = true
                    });
                }

                // ?? 2. Commessa default ??????????????????????????????????????????
                var commessa = (await _az_CommessaRepository.FindAll())
                    .FirstOrDefault(c => c.IdAz_Anagrafica == idAz && c.IdAz_Cliente == cliente.Id && c.Default);

                if (commessa == null)
                {
                    commessa = await _az_CommessaRepository.UpsertAsync(new Az_Commessa
                    {
                        IdAz_Anagrafica = idAz,
                        IdAz_Cliente = cliente.Id,
                        Descrizione = "Default",
                        Default = true
                    });
                }

                // ?? 3. SubCommessa default ???????????????????????????????????????
                var subCommessa = (await _az_SubCommessaRepository.FindAll())
                    .FirstOrDefault(s => s.IdAz_Commessa == commessa.Id && s.Default);

                if (subCommessa == null)
                {
                    subCommessa = await _az_SubCommessaRepository.UpsertAsync(new Az_SubCommessa
                    {
                        IdAz_Commessa = commessa.Id,
                        Descrizione = "Default",
                        Default = true,
                        Data = DateTime.Today,
                        DataA = DateTime.Today.AddYears(10)
                    });
                }

                // ?? 4. Par_Attivita default ??????????????????????????????????????
                var attivita = (await _par_AttivitaRepository.FindAll())
                    .FirstOrDefault(a => a.IdAz_Anagrafica == idAz && a.Default);

                if (attivita == null)
                {
                    attivita = await _par_AttivitaRepository.UpsertAsync(new Par_Attivita
                    {
                        IdAz_Anagrafica = idAz,
                        Descrizione = "Default",
                        Default = true
                    });
                }

                // ?? 5. Az_SubCommessaAttivita default ????????????????????????????
                var subCommessaAttivita = (await _az_SubCommessaAttivitaRepository.FindAll())
                    .FirstOrDefault(a => a.IdAz_SubCommessa == subCommessa.Id && a.IdPar_Attivita == attivita.Id && a.Default);

                if (subCommessaAttivita == null)
                {
                    subCommessaAttivita = await _az_SubCommessaAttivitaRepository.UpsertAsync(new Az_SubCommessaAttivita
                    {
                        IdAz_SubCommessa = subCommessa.Id,
                        IdPar_Attivita = attivita.Id,
                        Default = true
                    });
                }

                retVal.Az_SubCommessaAttivita = _mapper.Map<Az_SubCommessaAttivitaModel>(subCommessaAttivita);
            }


            return retVal;
        }


        // public virtual async Task<GenericResult<Get_Az_SubCommessaAttivita_Default_OutModel>> Get_Az_SubCommessaAttivita_Default(GenericRequest<Get_Az_SubCommessaAttivita_Default_InModel> model, bool isSubProcess)
        //{
        //    return await ExecuteAction(model, async () =>
        //    {
        //        Get_Az_SubCommessaAttivita_Default_OutModel retVal = new Get_Az_SubCommessaAttivita_Default_OutModel();

        //        int IdCompany;
        //        int.TryParse(this.CurrentCompany, out IdCompany);

        //        Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
        //        if (company_DATA != null && company_DATA.az_Anagrafica != null)
        //        {
        //            int idAz = company_DATA.az_Anagrafica.Id;

        //            // ?? 1. Cliente default ???????????????????????????????????????????
        //            var cliente = (await _az_ClienteRepository.FindAll())
        //                .FirstOrDefault(c => c.IdAz_Anagrafica == idAz && c.Default);

        //            if (cliente == null)
        //            {
        //                cliente = await _az_ClienteRepository.UpsertAsync(new Az_Cliente
        //                {
        //                    IdAz_Anagrafica = idAz,
        //                    Descrizione     = "Default",
        //                    Default         = true
        //                });
        //            }

        //            // ?? 2. Commessa default ??????????????????????????????????????????
        //            var commessa = (await _az_CommessaRepository.FindAll())
        //                .FirstOrDefault(c => c.IdAz_Anagrafica == idAz && c.IdAz_Cliente == cliente.Id && c.Default);

        //            if (commessa == null)
        //            {
        //                commessa = await _az_CommessaRepository.UpsertAsync(new Az_Commessa
        //                {
        //                    IdAz_Anagrafica = idAz,
        //                    IdAz_Cliente    = cliente.Id,
        //                    Descrizione     = "Default",
        //                    Default         = true
        //                });
        //            }

        //            // ?? 3. SubCommessa default ???????????????????????????????????????
        //            var subCommessa = (await _az_SubCommessaRepository.FindAll())
        //                .FirstOrDefault(s => s.IdAz_Commessa == commessa.Id && s.Default);

        //            if (subCommessa == null)
        //            {
        //                subCommessa = await _az_SubCommessaRepository.UpsertAsync(new Az_SubCommessa
        //                {
        //                    IdAz_Commessa = commessa.Id,
        //                    Descrizione   = "Default",
        //                    Default       = true,
        //                    Data          = DateTime.Today,
        //                    DataA         = DateTime.Today.AddYears(10)
        //                });
        //            }

        //            // ?? 4. Par_Attivita default ??????????????????????????????????????
        //            var attivita = (await _par_AttivitaRepository.FindAll())
        //                .FirstOrDefault(a => a.IdAz_Anagrafica == idAz && a.Default);

        //            if (attivita == null)
        //            {
        //                attivita = await _par_AttivitaRepository.UpsertAsync(new Par_Attivita
        //                {
        //                    IdAz_Anagrafica = idAz,
        //                    Descrizione     = "Default",
        //                    Default         = true
        //                });
        //            }

        //            // ?? 5. Az_SubCommessaAttivita default ????????????????????????????
        //            var subCommessaAttivita = (await _az_SubCommessaAttivitaRepository.FindAll())
        //                .FirstOrDefault(a => a.IdAz_SubCommessa == subCommessa.Id && a.IdPar_Attivita == attivita.Id && a.Default);

        //            if (subCommessaAttivita == null)
        //            {
        //                subCommessaAttivita = await _az_SubCommessaAttivitaRepository.UpsertAsync(new Az_SubCommessaAttivita
        //                {
        //                    IdAz_SubCommessa = subCommessa.Id,
        //                    IdPar_Attivita   = attivita.Id,
        //                    Default          = true
        //                });
        //            }

        //            retVal.Az_SubCommessaAttivita = _mapper.Map<Az_SubCommessaAttivitaModel>(subCommessaAttivita);
        //        }

        //        await Task.Delay(DelayAsyncMethod);
        //        return retVal;
        //    }, isSubProcess);
        //}

        //////

        // Movimenti (richieste, timbrature, giustificativi, causali) ammessi solo nei giorni del rapporto di lavoro
        // e fuori dai mesi con il cartellino approvato:
        // ritorna il messaggio d'errore per il primo giorno non ammesso, altrimenti null
        public string? VerificaGiorniNelRapporto(int IdDip_RapportoLavoro, DateTime Dal, DateTime Al)
        {
            var rapporto = _dip_RapportoLavoroRepository.FindAll(r => r.Id == IdDip_RapportoLavoro).FirstOrDefault();
            if (rapporto == null)
                return "Rapporto di lavoro non trovato.";

            for (var giorno = Dal.Date; giorno <= Al.Date; giorno = giorno.AddDays(1))
            {
                if (rapporto.DataAss != null && giorno < rapporto.DataAss.Value.Date)
                    return $"Il {giorno:dd/MM/yyyy} è prima dell'assunzione ({rapporto.DataAss.Value:dd/MM/yyyy}): non si possono inserire movimenti.";
                if (rapporto.DataLic != null && giorno > rapporto.DataLic.Value.Date)
                    return $"Il {giorno:dd/MM/yyyy} è dopo la cessazione ({rapporto.DataLic.Value:dd/MM/yyyy}): non si possono inserire movimenti.";
            }

            var approvati = _dip_PeriodoRepository.FindAll(p => p.IdDip_RapportoLavoro == IdDip_RapportoLavoro && p.Stato == PeriodoStato.Approvato)
                                                  .Select(p => new { p.Anno, p.Mese })
                                                  .ToList();
            for (var giorno = Dal.Date; giorno <= Al.Date; giorno = giorno.AddDays(1))
            {
                if (approvati.Any(p => p.Anno == giorno.Year && p.Mese == giorno.Month))
                    return $"Il cartellino di {giorno:MM/yyyy} è approvato: per inserire o modificare movimenti l'HR deve riaprirlo.";
            }
            return null;
        }

        // ── Attivita' delle timbrature ───────────────────────────────────────────
        // Ogni timbratura e' sempre su un'attivita' (Az_SubCommessaAttivita).

        // Attivita' che il dipendente puo' usare: quelle delle sub commesse in cui e' collegato
        // direttamente come utente o tramite un reparto di cui fa parte (utente del reparto)
        public List<int> GetIdAz_SubCommessaAttivita_Consentite(string IdAspNetUsers)
        {
            var idSubCommessa = _az_SubCommessaUserRepository.FindAll(x => x.IdAspNetUsers == IdAspNetUsers)
                                                             .Select(x => x.IdAz_SubCommessa)
                                                             .ToList();
            var idAz_SediReparto = _az_SediRepartoUserRepository.FindAll(x => x.IdAspNetUsers == IdAspNetUsers && x.UserInDepartment)
                                                                .Select(x => x.IdAz_SediReparto)
                                                                .ToList();
            idSubCommessa.AddRange(_az_SubCommessaSediRepartoRepository.FindAll(x => idAz_SediReparto.Contains(x.IdAz_SediReparto))
                                                                       .Select(x => x.IdAz_SubCommessa)
                                                                       .ToList());

            return _az_SubCommessaAttivitaRepository.FindAll(x => idSubCommessa.Contains(x.IdAz_SubCommessa))
                                                    .OrderByDescending(x => x.Default).ThenBy(x => x.Id)
                                                    .Select(x => x.Id)
                                                    .ToList();
        }

        // Attivita' da proporre per una timbratura del rapporto (mai 0 se il rapporto esiste):
        // - con attivita' consentite: quella dell'ultima timbratura, poi quella del rapporto, poi la prima consentita
        // - senza attivita' consentite: quella del rapporto, altrimenti quella di default dell'azienda
        public async Task<int> GetIdAz_SubCommessaAttivita_Proposta(int IdDip_RapportoLavoro)
        {
            var rapporto = _dip_RapportoLavoroRepository.FindAll(x => x.Id == IdDip_RapportoLavoro).FirstOrDefault();
            if (rapporto == null)
                return 0;

            var dip_Anagrafica = _dip_AnagraficaRepository.FindAll(x => x.Id == rapporto.IdDip_Anagrafica).FirstOrDefault();
            var consentite = dip_Anagrafica != null ? GetIdAz_SubCommessaAttivita_Consentite(dip_Anagrafica.IdAspNetUsers) : new List<int>();

            if (consentite.Count > 0)
            {
                var adesso = DateTime.Now;
                var ultima = _dip_GG_TimbraturaRepository.FindAll(x => x.IdDip_RapportoLavoro == IdDip_RapportoLavoro && x.Timbratura <= adesso)
                                                         .OrderByDescending(x => x.Timbratura)
                                                         .Select(x => x.IdAz_SubCommessaAttivita)
                                                         .FirstOrDefault();
                if (consentite.Contains(ultima))
                    return ultima;
                if (consentite.Contains(rapporto.IdAz_SubCommessaAttivita))
                    return rapporto.IdAz_SubCommessaAttivita;
                return consentite[0];
            }

            if (EsisteAttivita(rapporto.IdAz_SubCommessaAttivita))
                return rapporto.IdAz_SubCommessaAttivita;

            var usrCompany = dip_Anagrafica != null ? _userCompanyRepository.FindAll(x => x.IdAspNetUsers == dip_Anagrafica.IdAspNetUsers).FirstOrDefault() : null;
            if (usrCompany == null)
                return 0;
            var res = await Get_Az_SubCommessaAttivita_Default(usrCompany.IdCompany);
            return res.Az_SubCommessaAttivita.Id;
        }

        // Attivita' da salvare sulla timbratura: quella indicata se esiste, altrimenti quella proposta
        public async Task<int> GetIdAz_SubCommessaAttivita_4Timbratura(int IdDip_RapportoLavoro, int IdAz_SubCommessaAttivita)
        {
            if (EsisteAttivita(IdAz_SubCommessaAttivita))
                return IdAz_SubCommessaAttivita;
            return await GetIdAz_SubCommessaAttivita_Proposta(IdDip_RapportoLavoro);
        }

        // Messaggio d'errore se l'attivita' non e' tra quelle consentite al dipendente, altrimenti null.
        // Senza attivita' consentite vale solo quella proposta (rapporto o default dell'azienda).
        public async Task<string?> VerificaAttivitaConsentita(int IdDip_RapportoLavoro, int IdAz_SubCommessaAttivita)
        {
            var rapporto = _dip_RapportoLavoroRepository.FindAll(x => x.Id == IdDip_RapportoLavoro).FirstOrDefault();
            var dip_Anagrafica = rapporto != null ? _dip_AnagraficaRepository.FindAll(x => x.Id == rapporto.IdDip_Anagrafica).FirstOrDefault() : null;
            var consentite = dip_Anagrafica != null ? GetIdAz_SubCommessaAttivita_Consentite(dip_Anagrafica.IdAspNetUsers) : new List<int>();
            if (consentite.Count == 0)
                consentite.Add(await GetIdAz_SubCommessaAttivita_Proposta(IdDip_RapportoLavoro));

            return consentite.Contains(IdAz_SubCommessaAttivita)
                ? null
                : "L'attività selezionata non è tra quelle consentite al dipendente: sceglierne una delle sub commesse a cui è collegato.";
        }

        private bool EsisteAttivita(int IdAz_SubCommessaAttivita)
        {
            return IdAz_SubCommessaAttivita > 0 && _az_SubCommessaAttivitaRepository.FindAll(x => x.Id == IdAz_SubCommessaAttivita).Any();
        }

        public async Task<List<Dip_AnagraficaModel>> GetAnagraficheByUsersId(List<string> usersId)
        {
            await Task.Delay(1);

            var dipana = _dip_AnagraficaRepository.FindAll(a => usersId.Contains(a.IdAspNetUsers)).ToList();

            var dipanaModel = _mapper.Map<List<Dip_AnagraficaModel>>(dipana);

            return dipanaModel;
        }

        private async Task InitDataCompany(Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto)
        {
            if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica != null)
            {
                // Inizializzazione Par_Competenza per l'azienda
                var par_Competenza = await _par_CompetenzaRepository.UpsertAsync(new Par_Competenza()
                {
                    IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id,
                    Default = true,
                    Descrizione = "Default"
                });

                var par_Attivita = await _par_AttivitaRepository.UpsertAsync(new Par_Attivita()
                {
                    IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id,
                    Default = true,
                    Descrizione = "Default"
                });

                if (par_Competenza != null && par_Attivita != null)
                {
                    await _par_AttivitaCompetenzaRepository.UpsertAsync(new Par_AttivitaCompetenza()
                    {
                        IdPar_Attivita = par_Attivita.Id,
                        IdPar_Competenza = par_Competenza.Id
                    });


                    var az_Cliente = await _az_ClienteRepository.UpsertAsync(new Az_Cliente()
                    {
                        IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id,
                        Default = true,
                        Descrizione = "Default",
                    });


                    var az_Commessa = await _az_CommessaRepository.UpsertAsync(new Az_Commessa()
                    {
                        IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id,
                        Default = true,
                        Descrizione = "Default",
                        IdAz_Cliente = az_Cliente.Id
                    });

                    if (az_Cliente != null && az_Commessa != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_SediReparto != null)
                    {
                        var az_SediAttivita = await _az_SediAttivitaRepository.UpsertAsync(new Az_SediAttivita()
                        {
                            IdAz_Sedi = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi.Id,
                            IdPar_Attivita = par_Attivita.Id
                        });

                        if (az_SediAttivita != null)
                        {
                            var az_SediRepartoAttivita = await _az_SediRepartoAttivitaRepository.UpsertAsync(new Az_SediRepartoAttivita()
                            {
                                IdPar_Attivita = az_SediAttivita.IdPar_Attivita,
                                IdAz_SediReparto = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_SediReparto.Id
                            });


                            var az_SubCommessa = await _az_SubCommessaRepository.UpsertAsync(new Az_SubCommessa()
                            {
                                IdAz_Commessa = az_Commessa.Id,
                                Default = true,
                                Descrizione = "Default",
                                Data = DateTime.Today,
                                DataA = new DateTime(DateTime.Today.Year, 12, 31)
                            });

                            if (az_SubCommessa != null)
                            {
                                var az_SubCommessaAttivita = await _az_SubCommessaAttivitaRepository.UpsertAsync(new Az_SubCommessaAttivita()
                                {
                                    IdAz_SubCommessa = az_SubCommessa.Id,
                                    IdPar_Attivita = par_Attivita.Id,
                                    Default = true,
                                });

                                var az_SubCommessaSediReparto = await _az_SubCommessaSediRepartoRepository.UpsertAsync(new Az_SubCommessaSediReparto()
                                {
                                    IdAz_SediReparto = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_SediReparto.Id,
                                    IdAz_SubCommessa = az_SubCommessaAttivita.Id
                                });

                            }


                        }
                    }
                }

                // Parametri presenze di partenza (causali, giustificativi, orario, profilo orario, export)
                await _gestionePresenzeParametriDefault.InitParametri(company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id);
            }

        }


    }

    public class User_DATA_COMB_DipAna_DipRapp
    {
        public Dip_Anagrafica? dip_Anagrafica;
        public Dip_RapportoLavoro? dip_RapportoLavoro;
    }

    public class Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg
    {
        public Az_Anagrafica? az_Anagrafica;
        public Az_Sedi? az_Sedi;
        public Az_SediReparto? az_SediReparto;
        public Az_Cfg? az_Cfg;
    }


    public interface IGestionePresenzeUserUtility
    {
        Task<User_DATA_COMB_DipAna_DipRapp> Get_DipAna_DipRapp(string IdAspNetUsers, bool InitIfNotExsist);
        Task<User_DATA_COMB_DipAna_DipRapp> Get_DipRapp_DipAna(int IdDipRapp);
        string? VerificaGiorniNelRapporto(int IdDip_RapportoLavoro, DateTime Dal, DateTime Al);
        Task<Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg> Get_AzAna_AzSedi_AzReparto_Az_Cfg(int IdCompany, bool InitIfNotExsist);
        Task<List<Dip_AnagraficaModel>> GetAnagraficheByUsersId(List<string> usersId);
        Task CompletaCognomeNome(List<string> IdAspNetUsers);
        Par_Competenza? GetCompetenzaDefault(int IdAz_Anagrafica);
        Task CompletaCompetenzaDefault(List<string> IdAspNetUsers, int IdAz_Anagrafica);
        Task<Get_Az_SubCommessaAttivita_Default_OutModel> Get_Az_SubCommessaAttivita_Default(int IdCompany);
        List<int> GetIdAz_SubCommessaAttivita_Consentite(string IdAspNetUsers);
        Task<int> GetIdAz_SubCommessaAttivita_Proposta(int IdDip_RapportoLavoro);
        Task<int> GetIdAz_SubCommessaAttivita_4Timbratura(int IdDip_RapportoLavoro, int IdAz_SubCommessaAttivita);
        Task<string?> VerificaAttivitaConsentita(int IdDip_RapportoLavoro, int IdAz_SubCommessaAttivita);
    }



    public class Get_Az_SubCommessaAttivita_Default_OutModel 
    {
        public Az_SubCommessaAttivitaModel Az_SubCommessaAttivita { get; set; } = new Az_SubCommessaAttivitaModel();
    }
}
