import { Component, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { Subject, debounceTime, takeUntil } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DipGGNotaSpesaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_NotaSpesa/dip-gg-nota-spesa.service';
import { Dip_GG_NotaSpesa_AllegatoModel, Dip_GG_NotaSpesa_Get4Richiesta_InModel, Dip_GG_NotaSpesa_Search_InModel, Dip_GG_NotaSpesaModel } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_NotaSpesa/Models/dip-gg-nota-spesa-model';
import { ParTipoSpesaService } from '../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/par-tipo-spesa.service';
import { Par_TipoSpesaInModel, Par_TipoSpesaModel } from '../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/Models/par-tipo-spesa-model';
import { Dip_GG_Richiesta_SetState_InModel, Dip_GG_RichiestaModel, riepilogoNotaSpesa, StatoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { TimeSheetService } from '../../../Utility/GestionePresenze/time-sheet.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { TranslocoService } from '@jsverse/transloco';


/** gruppo di spese con il suo totale (per dipendente o per tipo di spesa) */
interface GruppoSpese {
  chiave: string;
  titolo: string;
  spese: Dip_GG_NotaSpesaModel[];
  totale: number;
}

/*
 Esame delle note spese (CompanyPowerAdmin): spese del mese per reparto e dipendenti scelti,
 filtrabili per tipo di spesa e stato, raggruppate per dipendente o per tipo di spesa con i totali.
 Con un clic su una spesa si apre il dettaglio della nota spese che la contiene, da cui si puo'
 approvare o rifiutare la nota (ciclo standard delle richieste, come dalla pagina "Richieste").
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

  /** dialog di dettaglio: nota spese (richiesta) con tutte le sue spese */
  public dettaglioAperto = false;
  public dettaglioRichiesta: Dip_GG_RichiestaModel | null = null;
  public dettaglioSpese: Dip_GG_NotaSpesaModel[] = [];
  public dettaglioIdAspNetUsers = '';

  /** pulsanti di approvazione, rifiuto e chiusura: quelli standard, come nella lista richieste admin */
  public btnApprova: ButtonItem;
  public btnRifiuta: ButtonItem;
  public btnAnnulla: ButtonItem;

  private readonly _ricerca$ = new Subject<void>();
  private readonly _destroy$ = new Subject<void>();

  constructor(private dipGGNotaSpesaService: DipGGNotaSpesaService,
              private dipGGRichiestaService: DipGGRichiestaService,
              private timeSheetService: TimeSheetService,
              private collectionDialogService: CollectionDialogService,
              private parTipoSpesaService: ParTipoSpesaService,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              userInterfaceService: UserInterfaceService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('attendance.pages.notaSpeseAdmin.title');
    this.btnApprova = userInterfaceService.Btn_Approva;
    this.btnApprova.event = () => this.decidi(true);
    this.btnRifiuta = userInterfaceService.Btn_Rifiuta;
    this.btnRifiuta.event = () => this.decidi(false);
    this.btnAnnulla = userInterfaceService.Btn_Annulla;
    this.btnAnnulla.event = () => this.chiudiDettaglio();
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
      const chiave = this.raggruppa === 'dipendente' ? s.idAspNetUsers : String(s.idPar_TipoSpesa);
      let g = gruppi.get(chiave);
      if (!g) {
        g = { chiave, titolo: this.raggruppa === 'dipendente' ? this.dipendente(s.idAspNetUsers) : this.tipo(s.idPar_TipoSpesa), spese: [], totale: 0 };
        gruppi.set(chiave, g);
      }
      g.spese.push(s);
      g.totale += s.importo;
    }
    this.gruppi = [...gruppi.values()].sort((a, b) => a.titolo.localeCompare(b.titolo));
    this.totale = spese.reduce((t, s) => t + s.importo, 0);
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

  public apriAllegato(allegato: Dip_GG_NotaSpesa_AllegatoModel, event?: Event): void {
    event?.stopPropagation();   // il clic sull'allegato non apre il dettaglio della riga
    this.dipGGNotaSpesaService.ApriAllegato(allegato);
  }

  // ------------------------------------------------------------------ dettaglio della nota spese

  public apriDettaglio(spesa: Dip_GG_NotaSpesaModel): void {
    if (!spesa.idDip_GG_Richiesta)
      return;
    this.dettaglioIdAspNetUsers = spesa.idAspNetUsers;
    this.dettaglioRichiesta = null;
    this.dettaglioSpese = [];
    this.dettaglioAperto = true;
    const request = new GenericRequest<Dip_GG_NotaSpesa_Get4Richiesta_InModel>(Dip_GG_NotaSpesa_Get4Richiesta_InModel);
    request.data.idDip_GG_Richiesta = spesa.idDip_GG_Richiesta;
    this.dipGGNotaSpesaService.Get4Richiesta(request).subscribe(res => {
      this.dettaglioRichiesta = res.data?.dip_GG_Richiesta ?? null;
      this.dettaglioSpese = res.data?.dip_GG_NotaSpesa ?? [];
    });
  }

  public chiudiDettaglio(): void {
    this.dettaglioAperto = false;
  }

  public get dettaglioRiepilogo() {
    return riepilogoNotaSpesa(this.dettaglioRichiesta?.dati);
  }

  public get dettaglioTotale(): number {
    return this.dettaglioSpese.reduce((t, s) => t + s.importo, 0);
  }

  /** revoca chiesta dal dipendente su una nota gia' approvata: i pulsanti decidono sulla revoca */
  public get dettaglioRevoca(): boolean {
    return this.dettaglioRichiesta?.revocaStato === StatoRichiesta.Immessa || this.dettaglioRichiesta?.revocaStato === StatoRichiesta.ApprovazioneInCorso;
  }

  public get puoApprovare(): boolean {
    return !!this.dettaglioRichiesta && this.timeSheetService.Dip_GG_Richiesta_Admin_Can_Approve(this.dettaglioRichiesta);
  }

  public get puoRifiutare(): boolean {
    return !!this.dettaglioRichiesta && this.timeSheetService.Dip_GG_Richiesta_Admin_Can_Reject(this.dettaglioRichiesta);
  }

  /** approva o rifiuta la nota spese (o la sua revoca), con conferma */
  public async decidi(approva: boolean): Promise<void> {
    if (!this.dettaglioRichiesta)
      return;
    const chiave = (approva ? 'confermaApprovazione' : 'confermaRifiuto') + (this.dettaglioRevoca ? 'Revoca' : '');
    const conferma = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.notaSpeseAdmin.' + chiave));
    if (!conferma)
      return;

    const request = new GenericRequest<Dip_GG_Richiesta_SetState_InModel>(Dip_GG_Richiesta_SetState_InModel);
    request.data.fromHR = true;
    request.data.richiestaStato = approva ? StatoRichiesta.Approvata : StatoRichiesta.Rifiutata;
    request.data.idDip_GG_Richiesta = [this.dettaglioRichiesta.id];
    this.dipGGRichiestaService.SetState(request).subscribe(res => {
      if (res.success) {
        this.dettaglioAperto = false;
        this._ricerca$.next();
      }
    });
  }

  /** colore dello stato della nota, tenendo conto della revoca */
  public statoNota(richiesta: Dip_GG_RichiestaModel): { testo: string | null, colore: string } {
    if (richiesta.revocaStato === StatoRichiesta.Approvata)
      return { testo: this.transloco.translate('attendance.pages.notaSpeseAdmin.revocata'), colore: 'dark' };
    if (this.dettaglioRevoca)
      return { testo: this.transloco.translate('attendance.pages.notaSpeseAdmin.revocaRichiesta'), colore: 'warning' };
    return { testo: null, colore: this.coloreStato(richiesta.richiestaStato) };
  }
}
