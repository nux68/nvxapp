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

        public GestionePresenzeUserUtility(IDip_AnagraficaRepository dip_AnagraficaRepository,
                                           IDip_RapportoLavoroRepository dip_RapportoLavoroRepository)
        {
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
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
    }

    public class User_DATA_COMB_DipAna_DipRapp
    {
        public Dip_Anagrafica? dip_Anagrafica;
        public Dip_RapportoLavoro? dip_RapportoLavoro;
    }

    public interface IGestionePresenzeUserUtility
    {
        Task<User_DATA_COMB_DipAna_DipRapp> Get_DipAna_DipRapp(string IdAspNetUsers, bool InitIfNotExsist);
    }
}
