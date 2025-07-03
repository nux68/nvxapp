using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.Interfaces;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    public class GestionePresenzeUserUtility : IServiceBase, IGestionePresenzeUserUtility
    {
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




        public GestionePresenzeUserUtility(IDip_AnagraficaRepository dip_AnagraficaRepository,
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
                                           IAz_SubCommessaSediRepartoRepository az_SubCommessaSediRepartoRepository
                                           )
        {
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
        }

        public async Task<User_DATA_COMB_DipAna_DipRapp> Get_DipAna_DipRapp(string IdAspNetUsers, bool InitIfNotExsist)
        {
            User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = new User_DATA_COMB_DipAna_DipRapp();

            user_DATA_COMB_DipAna_DipRapp.dip_Anagrafica = await _dip_AnagraficaRepository.FindAll(x => x.IdAspNetUsers == IdAspNetUsers).FirstOrDefaultAsync();
            if (user_DATA_COMB_DipAna_DipRapp.dip_Anagrafica == null)
            {
                if (InitIfNotExsist)
                {
                    user_DATA_COMB_DipAna_DipRapp.dip_Anagrafica = new Dip_Anagrafica()
                    {
                        IdAspNetUsers = IdAspNetUsers
                    };
                    await _dip_AnagraficaRepository.UpsertAsync(user_DATA_COMB_DipAna_DipRapp.dip_Anagrafica);
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
                    user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro = new Dip_RapportoLavoro
                    {
                        IdDip_Anagrafica = user_DATA_COMB_DipAna_DipRapp.dip_Anagrafica.Id,
                        DataAss = new DateTime(2025, 1, 1)
                    };
                    await _dip_RapportoLavoroRepository.UpsertAsync(user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro);
                }
                else
                {
                    throw new Exception("Rapporto lavoro non trovato");
                }
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
        //////

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
                                Data= DateTime.Today,
                                DataA= new DateTime(DateTime.Today.Year,12,31)
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
        Task<Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg> Get_AzAna_AzSedi_AzReparto_Az_Cfg(int IdCompany, bool InitIfNotExsist);
    }
}
