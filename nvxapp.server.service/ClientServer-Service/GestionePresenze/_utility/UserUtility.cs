using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant;
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
        private readonly IAz_RepartoRepository _az_RepartoRepository;

        public GestionePresenzeUserUtility(IDip_AnagraficaRepository dip_AnagraficaRepository,
                                           IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,

                                           IAz_AnagraficaRepository az_AnagraficaRepository,
                                           IAz_SediRepository az_SediRepository,
                                           IAz_RepartoRepository az_RepartoRepository )
        {
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;

            _az_AnagraficaRepository = az_AnagraficaRepository;
            _az_SediRepository = az_SediRepository;
            _az_RepartoRepository = az_RepartoRepository;
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

        
        public async Task<Company_DATA_COMB_AzAna_AzSedi_AzReparto> Get_AzAna_AzSedi_AzReparto(int IdCompany, bool InitIfNotExsist)
        {
            Company_DATA_COMB_AzAna_AzSedi_AzReparto company_DATA_COMB_AzAna_AzSedi_AzReparto = new Company_DATA_COMB_AzAna_AzSedi_AzReparto();

            company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica = await _az_AnagraficaRepository.FindAll(x => x.IdCompany == IdCompany).FirstOrDefaultAsync();
            if (company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica == null)
            {
                if (InitIfNotExsist)
                {
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


            company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi = await _az_SediRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id).FirstOrDefaultAsync();
            if (company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi == null)
            {
                if (InitIfNotExsist)
                {
                    company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi = new Az_Sedi
                    {
                         IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id,
                         Descrizione ="Default"
                    };
                    company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi = await _az_SediRepository.UpsertAsync(company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi);
                }
                else
                {
                    throw new Exception("Sede non trovata");
                }
            }


            company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Reparto = await _az_RepartoRepository.FindAll(x => x.IdAz_Sedi == company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi.Id).FirstOrDefaultAsync();
            if (company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Reparto == null)
            {
                if (InitIfNotExsist)
                {
                    company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Reparto = new Az_Reparto
                    {
                        IdAz_Sedi = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi.Id,
                        Descrizione = "Default"
                    };
                    company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Reparto = await _az_RepartoRepository.UpsertAsync(company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Reparto);
                }
                else
                {
                    throw new Exception("Sede non trovata");
                }
            }

            return company_DATA_COMB_AzAna_AzSedi_AzReparto;
        }
        //////
    }

    public class User_DATA_COMB_DipAna_DipRapp
    {
        public Dip_Anagrafica? dip_Anagrafica;
        public Dip_RapportoLavoro? dip_RapportoLavoro;
    }

    public class Company_DATA_COMB_AzAna_AzSedi_AzReparto
    {
        public Az_Anagrafica? az_Anagrafica;
        public Az_Sedi? az_Sedi;
        public Az_Reparto? az_Reparto;
    }


    public interface IGestionePresenzeUserUtility
    {
        Task<User_DATA_COMB_DipAna_DipRapp> Get_DipAna_DipRapp(string IdAspNetUsers, bool InitIfNotExsist);
        Task<Company_DATA_COMB_AzAna_AzSedi_AzReparto> Get_AzAna_AzSedi_AzReparto(int  IdCompany, bool InitIfNotExsist);
    }
}
