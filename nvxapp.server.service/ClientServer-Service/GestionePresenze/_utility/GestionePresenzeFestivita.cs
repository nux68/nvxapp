using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_FestivitaService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    // Riconoscimento dei giorni festivi dal calendario dell'azienda (Par_Festivita)
    public static class GestionePresenzeFestivita
    {
        public static bool IsFestivita(IEnumerable<Par_FestivitaModel> calendario, DateTime giorno)
        {
            giorno = giorno.Date;
            foreach (var f in calendario)
            {
                switch (f.TipoFestivita)
                {
                    case TipoFestivita.DataRicorrente:
                        if (f.Data.Month == giorno.Month && f.Data.Day == giorno.Day) return true;
                        break;
                    case TipoFestivita.DataSingola:
                        if (f.Data.Date == giorno) return true;
                        break;
                    case TipoFestivita.Pasquetta:
                        if (Pasqua(giorno.Year).AddDays(1) == giorno) return true;
                        break;
                }
            }
            return false;
        }

        /// <summary>Domenica di Pasqua (calendario gregoriano, algoritmo di Meeus/Jones/Butcher).</summary>
        public static DateTime Pasqua(int anno)
        {
            int a = anno % 19, b = anno / 100, c = anno % 100;
            int d = b / 4, e = b % 4, f = (b + 8) / 25, g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4, k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * l) / 451;
            int mese = (h + l - 7 * m + 114) / 31;
            int giorno = ((h + l - 7 * m + 114) % 31) + 1;
            return new DateTime(anno, mese, giorno);
        }
    }
}
