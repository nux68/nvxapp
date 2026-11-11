using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Repositories.Tenant.Crm;
using nvxapp.server.service.ClientServer_Service.Crm.Models;

namespace nvxapp.server.service.ClientServer_Service.Crm._utility
{
    // Elenchi condivisi dalle schede (cliente, opportunita', lead, offerta) e dalle pagine elenco
    public static class CrmLists
    {
        public static string NumeroOfferta(Crm_Cfg cfg, int anno, int numero)
            => $"{cfg.OffertaPrefisso}{numero}/{anno}";

        public static string NumeroRichiesta(int anno, int numero) => $"{numero}/{anno}";

        public static Crm_ContattoModel Contatto(Az_ClienteContatto x) => new()
        {
            Id = x.Id, IdAz_Cliente = x.IdAz_Cliente, Cognome = x.Cognome, Nome = x.Nome, Ruolo = x.Ruolo, Telefono = x.Telefono,
            Cellulare = x.Cellulare, Email = x.Email, Principale = x.Principale, Note = x.Note
        };

        public static async Task<List<Crm_ContattoModel>> ContattiAsync(ICrm_Az_ClienteContattoReadRepository contatti, int idAz_Cliente)
            => (await contatti.FindAll(x => x.IdAz_Cliente == idAz_Cliente)
                        .OrderByDescending(x => x.Principale).ThenBy(x => x.Cognome).ThenBy(x => x.Nome).ToListAsync())
               .Select(Contatto).ToList();

        public static async Task<List<Crm_OpportunitaListModel>> OpportunitaAsync(IQueryable<Crm_Opportunita> q, ICrmUtility utility)
        {
            var righe = await q.AsNoTracking()
                .Select(o => new Crm_OpportunitaListModel
                {
                    Id = o.Id, Titolo = o.Titolo, IdAz_Cliente = o.IdAz_Cliente, Cliente = o.Az_ClienteNavigation!.Descrizione,
                    IdCrm_Par_Fase = o.IdCrm_Par_Fase, Fase = o.Crm_Par_FaseNavigation!.Descrizione, FaseTipo = o.Crm_Par_FaseNavigation!.Tipo,
                    Valore = o.Valore, Probabilita = o.Probabilita, DataChiusuraPrevista = o.DataChiusuraPrevista, DataChiusura = o.DataChiusura,
                    IdResponsabile = o.IdAspNetUsersResponsabile
                })
                .OrderBy(o => o.DataChiusuraPrevista ?? DateTime.MaxValue).ThenBy(o => o.Titolo)
                .ToListAsync();
            var nomi = await utility.NomiAsync(righe.Select(r => r.IdResponsabile));
            righe.ForEach(r => r.Responsabile = r.IdResponsabile != null && nomi.TryGetValue(r.IdResponsabile, out var n) ? n : null);
            return righe;
        }

        public static async Task<List<Crm_AttivitaListModel>> AttivitaAsync(IQueryable<Crm_Attivita> q, ICrmUtility utility)
        {
            var righe = await q.AsNoTracking()
                .Select(a => new Crm_AttivitaListModel
                {
                    Id = a.Id, IdCrm_Par_TipoAttivita = a.IdCrm_Par_TipoAttivita, Tipo = a.Crm_Par_TipoAttivitaNavigation!.Descrizione,
                    Icona = a.Crm_Par_TipoAttivitaNavigation!.Icona, Colore = a.Crm_Par_TipoAttivitaNavigation!.Colore,
                    Oggetto = a.Oggetto, Inizio = a.Inizio, Fine = a.Fine, Stato = a.Stato, IdAssegnatario = a.IdAspNetUsersAssegnatario,
                    IdAz_Cliente = a.IdAz_Cliente, Cliente = a.Az_ClienteNavigation != null ? a.Az_ClienteNavigation.Descrizione : null,
                    IdCrm_Opportunita = a.IdCrm_Opportunita, Opportunita = a.Crm_OpportunitaNavigation != null ? a.Crm_OpportunitaNavigation.Titolo : null,
                    IdCrm_Lead = a.IdCrm_Lead,
                    Lead = a.Crm_LeadNavigation != null ? ((a.Crm_LeadNavigation.Cognome ?? "") + " " + (a.Crm_LeadNavigation.Nome ?? "") + (a.Crm_LeadNavigation.Azienda != null ? " - " + a.Crm_LeadNavigation.Azienda : "")).Trim() : null,
                })
                .OrderBy(a => a.Inizio)
                .ToListAsync();
            var nomi = await utility.NomiAsync(righe.Select(r => r.IdAssegnatario));
            righe.ForEach(r => r.Assegnatario = nomi.TryGetValue(r.IdAssegnatario, out var n) ? n : null);
            return righe;
        }

        public static async Task<List<Crm_OffertaListModel>> OfferteAsync(IQueryable<Crm_Offerta> q, Crm_Cfg cfg)
        {
            var righe = await q.AsNoTracking()
                .Select(o => new { o.Id, o.Anno, o.Numero, o.Data, o.ValidaFinoAl, o.Stato, o.IdAz_Cliente, Cliente = o.Az_ClienteNavigation!.Descrizione, o.Oggetto, o.Totale, o.IdCrm_Opportunita, o.DataEsito })
                .OrderByDescending(o => o.Anno).ThenByDescending(o => o.Numero)
                .ToListAsync();
            return righe.Select(o => new Crm_OffertaListModel
            {
                Id = o.Id, NumeroCompleto = NumeroOfferta(cfg, o.Anno, o.Numero), Data = o.Data, ValidaFinoAl = o.ValidaFinoAl, Stato = o.Stato,
                IdAz_Cliente = o.IdAz_Cliente, Cliente = o.Cliente, Oggetto = o.Oggetto, Totale = o.Totale, IdCrm_Opportunita = o.IdCrm_Opportunita, DataEsito = o.DataEsito
            }).ToList();
        }

        // richieste di assistenza (CRM-ASS): le aperte prima, poi le piu' recenti
        public static async Task<List<Crm_RichiestaListModel>> RichiesteAsync(IQueryable<Crm_Richiesta> q, ICrmUtility utility)
        {
            var righe = await q.AsNoTracking()
                .Select(r => new
                {
                    r.Id, r.Anno, r.Numero, r.IdAz_Cliente, Cliente = r.Az_ClienteNavigation!.Descrizione, r.Oggetto, r.Priorita, r.Stato,
                    r.IdAspNetUsersAssegnatario, r.DataApertura, r.DataPresaInCarico, r.DataRisoluzione
                })
                .OrderByDescending(r => r.DataApertura)
                .ToListAsync();
            var nomi = await utility.NomiAsync(righe.Select(r => r.IdAspNetUsersAssegnatario));
            return righe.Select(r => new Crm_RichiestaListModel
            {
                Id = r.Id, NumeroCompleto = NumeroRichiesta(r.Anno, r.Numero), IdAz_Cliente = r.IdAz_Cliente, Cliente = r.Cliente, Oggetto = r.Oggetto,
                Priorita = r.Priorita, Stato = r.Stato, IdAssegnatario = r.IdAspNetUsersAssegnatario,
                Assegnatario = r.IdAspNetUsersAssegnatario != null && nomi.TryGetValue(r.IdAspNetUsersAssegnatario, out var n) ? n : null,
                DataApertura = r.DataApertura, DataPresaInCarico = r.DataPresaInCarico, DataRisoluzione = r.DataRisoluzione,
                OrePresaInCarico = Crm_RichiestaService.Ore(r.DataApertura, r.DataPresaInCarico), OreRisoluzione = Crm_RichiestaService.Ore(r.DataApertura, r.DataRisoluzione),
            })
            .OrderBy(r => Crm_RichiestaService.Aperta(r.Stato) ? 0 : 1)
            .ToList();
        }

        public static async Task<List<Crm_NotaModel>> NoteAsync(IQueryable<Crm_Nota> q, CrmContesto ctx, ICrmUtility utility)
        {
            var righe = await q.AsNoTracking().OrderByDescending(n => n.Fissata).ThenByDescending(n => n.CreationDate).ToListAsync();
            var nomi = await utility.NomiAsync(righe.Select(r => r.IdAspNetUsersAutore));
            return righe.Select(n => new Crm_NotaModel
            {
                Id = n.Id, IdAz_Cliente = n.IdAz_Cliente, IdCrm_Opportunita = n.IdCrm_Opportunita, IdCrm_Lead = n.IdCrm_Lead, Testo = n.Testo,
                Fissata = n.Fissata, Data = n.ModifiedDate ?? n.CreationDate,
                Autore = n.IdAspNetUsersAutore != null && nomi.TryGetValue(n.IdAspNetUsersAutore, out var a) ? a : null,
                Modificabile = ctx.Amministratore || n.IdAspNetUsersAutore == ctx.IdUtente
            }).ToList();
        }

        public static async Task<List<Crm_AllegatoModel>> AllegatiAsync(IQueryable<Crm_Allegato> q, ICrmUtility utility)
        {
            var righe = await q.AsNoTracking().OrderByDescending(a => a.CreationDate).ToListAsync();
            var nomi = await utility.NomiAsync(righe.Select(r => r.IdAspNetUsersAutore));
            return righe.Select(a => new Crm_AllegatoModel
            {
                Id = a.Id, NomeFile = a.NomeFile, ContentType = a.ContentType, Dimensione = a.Dimensione, Data = a.CreationDate,
                Autore = a.IdAspNetUsersAutore != null && nomi.TryGetValue(a.IdAspNetUsersAutore, out var n) ? n : null
            }).ToList();
        }
    }
}
