import { Component, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { Subject, debounceTime, takeUntil } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DipGGNotaSpesaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_NotaSpesa/dip-gg-nota-spesa.service';
import { Dip_GG_NotaSpesa_AllegatoModel, Dip_GG_NotaSpesa_Search_InModel, Dip_GG_NotaSpesaModel } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_NotaSpesa/Models/dip-gg-nota-spesa-model';
import { ParTipoSpesaService } from '../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/par-tipo-spesa.service';
import { Par_TipoSpesaInModel, Par_TipoSpesaModel } from '../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/Models/par-tipo-spesa-model';
import { StatoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { NotaSpesaDettaglioDialogService } from '../../../shared/components/GestionePresenze/nota-spesa-dettaglio-dialog/nota-spesa-dettaglio-dialog.service';
import { TranslocoService } from '@jsverse/transloco';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';


/** gruppo di spese con il suo totale: una nota spese di un dipendente, o un tipo di spesa */
interface GruppoSpese {
  chiave: string;
  titolo: string;
  spese: Dip_GG_NotaSpesaModel[];
  totale: number;
  /** solo per la nota spese: periodo (date delle spese, "YYYY-MM-DD"), descrizione, stato e dipendente */
  nota?: { idDip_GG_Richiesta: number | null, idAspNetUsers: string, dal: string, al: string, descrizione: string, stato: StatoRichiesta };
}

/*
 Esame delle note spese (CompanyPowerAdmin): spese del mese per reparto e dipendenti scelti,
 filtrabili per tipo di spesa e stato, raggruppate per nota spese (dipendente) o per tipo di spesa con i totali.
 Ogni gruppo e' una sola riga che si espande con un clic: la nota spese con dipendente, data, descrizione,
 stato, numero di spese e totale (o il tipo di spesa con numero di spese e totale); dentro, ogni spesa e' una riga essenziale (data, dipendente o tipo,
 stato, importo) che a sua volta si espande per vedere descrizione e allegati. Il pulsante Dettaglio, a destra dell'importo, apre la
 nota spese (NotaSpesaDettaglioDialogComponent), da cui si puo' approvare o rifiutare la nota: sulla riga della nota (raggruppamento
 per dipendente) e su ogni spesa del raggruppamento per tipo (non sulla riga del tipo, che contiene spese di piu' note)
 (ciclo standard delle richieste, come dalla pagina "Richieste").
*/
@Component({
  selector: 'app-nota-spese-admin-page',
  templateUrl: './nota-spese-admin-page.component.html',
  styleUrls: ['./nota-spese-admin-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class NotaSpeseAdminPageComponent implements OnDestroy {

  public readonly title: string;

  public year = new Date().getFullYear();
  public month = new Date().getMonth() + 1;
  private usersId: string[] = [];

  public tipiSpesa: Par_TipoSpesaModel[] = [];
  public idTipoSpesa: number = 0;               // 0 = tutti
  // predefinito: in attesa e approvate (le note rifiutate o annullate restano, ma non entrano nei totali)
  public filtroStato: 'valide' | 'attesa' | 'approvate' | 'respinte' | 'tutte' = 'valide';
  public raggruppa: 'dipendente' | 'tipo' = 'dipendente';

  public spese: Dip_GG_NotaSpesaModel[] | null = null;
  public gruppi: GruppoSpese[] = [];
  public totale = 0;

  /** gruppi espansi (chiave del gruppo) e spese con la riga espansa (id della spesa) */
  public gruppiEspansi = new Set<string>();
  public espanse = new Set<number>();

  /** pulsante della collezione (UserInterfaceService) per aprire il dettaglio della nota spese */
  public readonly btnDettaglio: ButtonItem;

  private readonly _ricerca$ = new Subject<void>();
  private readonly _destroy$ = new Subject<void>();

  constructor(private dipGGNotaSpesaService: DipGGNotaSpesaService,
              private parTipoSpesaService: ParTipoSpesaService,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              private notaSpesaDettaglioDialogService: NotaSpesaDettaglioDialogService,
              private userInterfaceService: UserInterfaceService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('attendance.pages.notaSpeseAdmin.title');
    this.btnDettaglio = this.userInterfaceService.Btn_Dettaglio;
    this._ricerca$.pipe(debounceTime(300), takeUntil(this._destroy$)).subscribe(() => this.cerca());
  }

  ionViewWillEnter() {
    this.parTipoSpesaService.GetAll(new GenericRequest<Par_TipoSpesaInModel>(Par_TipoSpesaInModel)).subscribe(res => {
      this.tipiSpesa = res.data?.par_TipoSpesa ?? [];
      this.raggruppaSpese();
    });
    this._ricerca$.next();
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }

  onPeriodChange(period: { year: number, month: number } | undefined): void {
    if (!period) return;
    this.year = period.year;
    this.month = period.month;
    this._ricerca$.next();
  }

  onAllUsersInSelectionChanged(userIds: string[] | undefined): void {
    this.usersId = userIds ?? [];
    this._ricerca$.next();
  }

  public filtriCambiati(): void {
    this._ricerca$.next();
  }

  private cerca(): void {
    const request = new GenericRequest<Dip_GG_NotaSpesa_Search_InModel>(Dip_GG_NotaSpesa_Search_InModel);
    const fine = new Date(this.year, this.month, 0).getDate();
    request.data.dal = `${this.year}-${String(this.month).padStart(2, '0')}-01`;
    request.data.al = `${this.year}-${String(this.month).padStart(2, '0')}-${String(fine).padStart(2, '0')}`;
    request.data.usersId = this.usersId;
    request.data.idPar_TipoSpesa = this.idTipoSpesa || null;
    const attesa = [StatoRichiesta.Immessa, StatoRichiesta.ApprovazioneInCorso, StatoRichiesta.ParzialmenteApprovata];
    request.data.stati = this.filtroStato === 'approvate' ? [StatoRichiesta.Approvata]
      : this.filtroStato === 'attesa' ? attesa
      : this.filtroStato === 'valide' ? [...attesa, StatoRichiesta.Approvata]
      : this.filtroStato === 'respinte' ? [StatoRichiesta.Rifiutata, StatoRichiesta.Cancellata]
      : null;

    this.dipGGNotaSpesaService.Search(request).subscribe(res => {
      this.spese = res.data?.dip_GG_NotaSpesa ?? [];
      this.raggruppaSpese();
    });
  }

  /** gruppi per dipendente o per tipo di spesa, con totali */
  public raggruppaSpese(): void {
    const spese = this.spese ?? [];
    const gruppi = new Map<string, GruppoSpese>();
    for (const s of spese) {
      // per dipendente: una riga per nota spese (le spese della stessa richiesta)
      const chiave = this.raggruppa === 'dipendente' ? 'R' + (s.idDip_GG_Richiesta ?? 'S' + s.id) : String(s.idPar_TipoSpesa);
      let g = gruppi.get(chiave);
      if (!g) {
        g = { chiave, titolo: this.raggruppa === 'dipendente' ? this.dipendente(s.idAspNetUsers) : this.tipo(s.idPar_TipoSpesa), spese: [], totale: 0 };
        if (this.raggruppa === 'dipendente')
          g.nota = { idDip_GG_Richiesta: s.idDip_GG_Richiesta, idAspNetUsers: s.idAspNetUsers, dal: this.giorno(s.data), al: this.giorno(s.data), descrizione: s.richiestaDescrizione ?? '', stato: s.richiestaStato };
        gruppi.set(chiave, g);
      }
      g.spese.push(s);
      g.totale += s.importo;
      if (g.nota) {
        const giorno = this.giorno(s.data);
        if (giorno < g.nota.dal) g.nota.dal = giorno;
        if (giorno > g.nota.al) g.nota.al = giorno;
      }
    }
    // note spese per dipendente e data; tipi di spesa per descrizione
    this.gruppi = [...gruppi.values()].sort((a, b) => a.titolo.localeCompare(b.titolo) || (a.nota?.dal ?? '').localeCompare(b.nota?.dal ?? ''));
    this.totale = spese.reduce((t, s) => t + s.importo, 0);
  }

  /** "YYYY-MM-DD" della data di una spesa */
  private giorno(data: string): string {
    return String(data ?? '').substring(0, 10);
  }

  public dipendente(idAspNetUsers: string): string {
    const a = (this.sharedParameterGestionePresenzeService.Dip_Anagrafica ?? []).find(x => x.idAspNetUsers === idAspNetUsers);
    return a ? `${a.cognome ?? ''} ${a.nome ?? ''}`.trim() : idAspNetUsers;
  }

  public tipo(id: number): string {
    return this.tipiSpesa.find(t => t.id === id)?.descrizione ?? '';
  }

  /** colore dello stato: approvata verde, rifiutata rossa, annullata scura, in attesa grigia */
  public coloreStato(stato: StatoRichiesta): string {
    switch (stato) {
      case StatoRichiesta.Approvata: return 'success';
      case StatoRichiesta.Rifiutata: return 'danger';
      case StatoRichiesta.Cancellata: return 'dark';
      default: return 'medium';
    }
  }

  public euro(valore: number): string {
    return '€ ' + (valore ?? 0).toFixed(2).replace('.', ',');
  }

  // ------------------------------------------------------------------ righe espandibili

  public gruppoEspanso(gruppo: GruppoSpese): boolean {
    return this.gruppiEspansi.has(gruppo.chiave);
  }

  public espandiGruppo(gruppo: GruppoSpese): void {
    if (!this.gruppiEspansi.delete(gruppo.chiave))
      this.gruppiEspansi.add(gruppo.chiave);
  }

  public espansa(spesa: Dip_GG_NotaSpesaModel): boolean {
    return this.espanse.has(spesa.id);
  }

  public espandi(spesa: Dip_GG_NotaSpesaModel): void {
    if (!this.espanse.delete(spesa.id))
      this.espanse.add(spesa.id);
  }

  /** tutti i gruppi espansi o tutti chiusi (chiudendo si chiudono anche le spese) */
  public get tutteEspanse(): boolean {
    return this.gruppi.length > 0 && this.gruppi.every(g => this.gruppiEspansi.has(g.chiave));
  }

  public espandiTutte(): void {
    if (this.tutteEspanse) {
      this.gruppiEspansi = new Set<string>();
      this.espanse = new Set<number>();
    }
    else
      this.gruppiEspansi = new Set(this.gruppi.map(g => g.chiave));
  }

  public apriAllegato(allegato: Dip_GG_NotaSpesa_AllegatoModel, event?: Event): void {
    event?.stopPropagation();   // il clic sull'allegato non chiude la riga
    this.dipGGNotaSpesaService.ApriAllegato(allegato);
  }

  /** dettaglio della nota spese (dalla riga della nota o da una sua spesa), con approvazione e rifiuto */
  public async apriDettaglio(nota: { idDip_GG_Richiesta: number | null, idAspNetUsers: string }, event?: Event): Promise<void> {
    event?.stopPropagation();   // il clic sul pulsante non espande / chiude la riga
    if (!nota.idDip_GG_Richiesta)
      return;
    if (await this.notaSpesaDettaglioDialogService.Apri(nota.idDip_GG_Richiesta, this.dipendente(nota.idAspNetUsers), true))
      this._ricerca$.next();
  }
}
