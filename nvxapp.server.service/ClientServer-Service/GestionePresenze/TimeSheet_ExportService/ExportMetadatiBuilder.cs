using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_ExportService.Models;
using System.Globalization;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_ExportService
{
    /*
     Metadati dell'export (analisi par. 4.2), senza accessi a database: si prova da solo.
       - solo le causali presenti nel modello di export; le altre si ignorano
       - ogni causale prende codice di output, elaborazione, unita' e verso della sua riga del modello
       - raggruppamento per (dipendente, codice di output, periodo): giorno (Giornaliera) o mese (Mensile)
       - Ore: minuti con il segno del verso, sommati e convertiti in ore con 2 decimali solo alla fine
         Giorni: per ogni giorno il saldo dei minuti delle causali del codice da' +1 / -1 / 0; nel mese si sommano
         Importo: euro con il segno del verso, 2 decimali
       - valori a zero tolti; dipendenti senza causali esportate esclusi
       - i totali negativi (per il verso Sottrazione) restano con il segno e si segnalano
    */
    public static class ExportMetadatiBuilder
    {
        public record DatiUtente(string? Email, string? Username);

        public record Risultato(ExportMetadati Metadati, List<string> Negativi);

        public static Risultato Costruisci(string azienda, string codiceModello, string descrizioneModello,
                                           DateTime dal, DateTime al,
                                           IEnumerable<Dip_AnagraficaModel> anagrafiche,
                                           IEnumerable<Dip_RapportoLavoroModel> rapporti,
                                           IEnumerable<Dip_GG_CausaliModel> causaliGiorno,
                                           IEnumerable<Par_ExportCau_CausaliModel> righeModello,
                                           IReadOnlyDictionary<string, DatiUtente> utenti,
                                           DateTime generatoIl)
        {
            var metadati = new ExportMetadati
            {
                GeneratoIl = generatoIl,
                Azienda = new ExportMetadati_Azienda { Descrizione = azienda },
                ModelloExport = new ExportMetadati_Modello { Codice = codiceModello, Descrizione = descrizioneModello },
                Periodo = new ExportMetadati_Periodo { Dal = Data(dal), Al = Data(al) }
            };
            var negativi = new List<string>();

            var righe = righeModello.Where(r => !string.IsNullOrWhiteSpace(r.Codice)).ToList();
            // codice di output -> elaborazione e unita' (uguali per tutte le righe del codice, controllato al salvataggio)
            var codici = righe.GroupBy(r => r.Codice!.Trim(), StringComparer.OrdinalIgnoreCase)
                              .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            metadati.CausaliEsportate = codici.Values
                .OrderBy(r => r.Codice!.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(r => new ExportMetadati_CausaleEsportata { Codice = r.Codice!.Trim(), TipoElaborazione = Elaborazione(r.TipoElaborazione), TipoUnita = r.TipoUnita.ToString() })
                .ToList();

            // causale presenze -> righe del modello (una causale puo' andare su piu' codici)
            var perCausale = righe.GroupBy(r => r.IdCausale).ToDictionary(g => g.Key, g => g.ToList());
            var dipPerRapporto = rapporti.ToDictionary(r => r.Id, r => r.IdDip_Anagrafica);

            // contributi: (dipendente, codice, giorno) -> minuti e importo con il segno del verso
            var contributi = new List<(int IdDip, string Codice, DateTime Giorno, int Minuti, decimal Importo)>();
            foreach (var c in causaliGiorno)
            {
                if (c.Data.Date < dal.Date || c.Data.Date > al.Date) continue;
                if (!perCausale.TryGetValue(c.IdPar_Causali, out var righeCausale)) continue;
                if (!dipPerRapporto.TryGetValue(c.IdDip_RapportoLavoro, out var idDip)) continue;

                var minuti = c.Valore.Hour * 60 + c.Valore.Minute;
                foreach (var r in righeCausale)
                {
                    var segno = r.Verso == Par_Export_Verso.Sottrazione ? -1 : 1;
                    contributi.Add((idDip, r.Codice!.Trim(), c.Data.Date, segno * minuti, segno * (c.Importo ?? 0m)));
                }
            }

            foreach (var dip in anagrafiche.OrderBy(d => d.Cognome ?? string.Empty, StringComparer.CurrentCultureIgnoreCase).ThenBy(d => d.Nome ?? string.Empty, StringComparer.CurrentCultureIgnoreCase))
            {
                var causali = new List<ExportMetadati_Causale>();
                foreach (var perCodice in contributi.Where(x => x.IdDip == dip.Id).GroupBy(x => x.Codice, StringComparer.OrdinalIgnoreCase))
                {
                    var riga = codici[perCodice.Key];
                    var mensile = riga.TipoElaborazione == Par_Export_TipoElaborazione.Mensile;

                    foreach (var perPeriodo in perCodice.GroupBy(x => mensile ? x.Giorno.ToString("yyyy-MM", CultureInfo.InvariantCulture) : Data(x.Giorno)))
                    {
                        decimal valore = riga.TipoUnita switch
                        {
                            Par_Export_TipoUnita.Ore => Math.Round(perPeriodo.Sum(x => x.Minuti) / 60m, 2, MidpointRounding.AwayFromZero),
                            Par_Export_TipoUnita.Giorni => perPeriodo.GroupBy(x => x.Giorno).Sum(g => Math.Sign(g.Sum(x => x.Minuti))),
                            _ => Math.Round(perPeriodo.Sum(x => x.Importo), 2, MidpointRounding.AwayFromZero)
                        };
                        if (valore == 0) continue;

                        causali.Add(new ExportMetadati_Causale
                        {
                            Chiave = $"{riga.Codice!.Trim()}|{perPeriodo.Key}",
                            Codice = riga.Codice!.Trim(),
                            Periodo = perPeriodo.Key,
                            TipoElaborazione = Elaborazione(riga.TipoElaborazione),
                            TipoUnita = riga.TipoUnita.ToString(),
                            Valore = valore
                        });
                        if (valore < 0)
                            negativi.Add($"{riga.Codice!.Trim()} {perPeriodo.Key} ({dip.Cognome} {dip.Nome})".Trim());
                    }
                }
                if (causali.Count == 0) continue;

                utenti.TryGetValue(dip.IdAspNetUsers, out var utente);
                metadati.Dipendenti.Add(new ExportMetadati_Dipendente
                {
                    Anagrafica = new ExportMetadati_Anagrafica
                    {
                        IdDipendente = dip.Id,
                        Cognome = dip.Cognome ?? string.Empty,
                        Nome = dip.Nome ?? string.Empty,
                        Email = utente?.Email,
                        Username = utente?.Username,
                        Matricola = dip.Matricola,
                        CodiceFiscale = dip.CodiceFiscale,
                        Rapporti = rapporti.Where(r => r.IdDip_Anagrafica == dip.Id)
                                           .OrderBy(r => r.DataAss)
                                           .Select(r => new ExportMetadati_Rapporto
                                           {
                                               DataAssunzione = r.DataAss is DateTime da ? Data(da) : null,
                                               DataLicenziamento = r.DataLic is DateTime dl ? Data(dl) : null,
                                               TipoContratto = r.TipoContratto.ToString(),
                                               OreSettimanaliContratto = r.OreSettimanaliContratto,
                                               Ccnl = r.Ccnl
                                           }).ToList()
                    },
                    Causali = causali.OrderBy(c => c.Codice, StringComparer.OrdinalIgnoreCase).ThenBy(c => c.Periodo, StringComparer.Ordinal).ToList()
                });
            }

            return new Risultato(metadati, negativi);
        }

        // il nome dell'enum ha un refuso (Gionaliera): nei metadati si scrive giusto
        private static string Elaborazione(Par_Export_TipoElaborazione t) => t == Par_Export_TipoElaborazione.Mensile ? "Mensile" : "Giornaliera";

        private static string Data(DateTime d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
