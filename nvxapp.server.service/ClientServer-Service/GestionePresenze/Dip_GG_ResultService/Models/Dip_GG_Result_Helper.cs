using nvxapp.server.data.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService.Models
{



    public static class Dip_GG_Result_Helper
    {
        // Imposta lo stato principale (mutuamente esclusivo)
        public static void SetState(ref GG_ResultStato stato, GG_ResultStato state)
        {
            stato &= ~GG_ResultStato.STATE_MASK; // pulisce gli stati
            stato |= state;                       // imposta il nuovo
        }

        // Aggiunge un dettaglio (warning/errore)
        public static void AddDetail(ref GG_ResultStato stato, GG_ResultStato detail)
        {
            stato |= detail;
        }

        // Rimuove un dettaglio
        public static void RemoveDetail(ref GG_ResultStato stato, GG_ResultStato detail)
        {
            stato &= ~detail;
        }

        // Controlla un dettaglio
        public static bool HasDetail(GG_ResultStato stato, GG_ResultStato detail)
        {
            return (stato & detail) != 0;
        }

        // Legge lo stato principale
        public static GG_ResultStato GetState(GG_ResultStato stato)
        {
            return stato & GG_ResultStato.STATE_MASK;
        }

        public static int ToInt(GG_ResultStato stato)
        {
            return (int)stato;
        }
    }



        //GG_ResultStato2 stato = GG_ResultStato2.Init;

    //// Imposto stato principale
    //Dip_GG_Result_Helper.SetState(ref stato, GG_ResultStato2.Err);

    //// Aggiungo dettagli
    //Dip_GG_Result_Helper.AddDetail(ref stato, GG_ResultStato2.Err_1);
    //Dip_GG_Result_Helper.AddDetail(ref stato, GG_ResultStato2.Err_3);

    //// Controllo
    //bool hasErr1 = Dip_GG_Result_Helper.HasDetail(stato, GG_ResultStato2.Err_1); // true

    //// Leggo lo stato principale
    //var mainState = Dip_GG_Result_Helper.GetState(stato); // Err



}
