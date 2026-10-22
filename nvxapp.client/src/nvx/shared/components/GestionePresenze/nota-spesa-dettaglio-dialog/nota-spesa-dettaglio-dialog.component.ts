import { Component, ChangeDetectionStrategy, ElementRef, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonBadge, IonChip, IonContent, IonHeader, IonIcon, IonLabel, IonSpinner, IonTitle, IonToolbar, ModalController } from '@ionic/angular';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { SharedComponentInfrastructureModule } from '../../../shared-component-infrastructure.module';
import { StatoRichiestaLongTextPipe } from '../../../pipe/GestionePresenze/stato-richiesta-long-text.pipe';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { DipGGNotaSpesaService } from '../../../../ClientServer-Service/GestionePresenze/Dip_GG_NotaSpesa/dip-gg-nota-spesa.service';
import { Dip_GG_NotaSpesa_AllegatoModel, Dip_GG_NotaSpesa_Get4Richiesta_InModel, Dip_GG_NotaSpesaModel } from '../../../../ClientServer-Service/GestionePresenze/Dip_GG_NotaSpesa/Models/dip-gg-nota-spesa-model';
import { ParTipoSpesaService } from '../../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/par-tipo-spesa.service';
import { Par_TipoSpesaInModel, Par_TipoSpesaModel } from '../../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/Models/par-tipo-spesa-model';
import { DipGGRichiestaService } from '../../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { Dip_GG_Richiesta_SetState_InModel, Dip_GG_RichiestaModel, riepilogoNotaSpesa, StatoRichiesta } from '../../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { TimeSheetService } from '../../../../Utility/GestionePresenze/time-sheet.service';
import { RefresherService } from '../../../../Utility/GestionePresenze/refresher.service';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../infrastructure/generic-dialog/collection-dialog.service';

/**
 * Dettaglio di una nota spese (richiesta di tipo NotaSpesa) con tutte le sue spese e gli allegati.
 * Se consentiDecisione, chi puo' decidere (HR) approva o rifiuta la nota o la sua revoca
 * (ciclo standard delle richieste); il dialog si chiude con ruolo 'confirm' quando lo stato cambia.
 * Si apre con NotaSpesaDettaglioDialogService.Apri (esame spese, richieste, calendari).
 * Non ha un form (nessun dato da inserire): per questo non deriva da BaseDialogConfirmCancelComponent.
 */
@Component({
  selector: 'app-nota-spesa-dettaglio-dialog',
  templateUrl: './nota-spesa-dettaglio-dialog.component.html',
  styleUrls: ['./nota-spesa-dettaglio-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: true,
  imports: [CommonModule, IonBadge, IonChip, IonContent, IonHeader, IonIcon, IonLabel, IonSpinner, IonTitle, IonToolbar, TranslocoPipe, StatoRichiestaLongTextPipe, SharedComponentInfrastructureModule]
})
export class NotaSpesaDettaglioDialogComponent implements OnInit {

  /** richiesta (nota spese) da mostrare */
  @Input() idDip_GG_Richiesta!: number;
  /** nome del dipendente (vuoto nel calendario del dipendente stesso) */
  @Input() dipendente: string = '';
  /** pulsanti di approvazione e rifiuto (solo per chi decide) */
  @Input() consentiDecisione: boolean = false;

  public richiesta: Dip_GG_RichiestaModel | null = null;
  public spese: Dip_GG_NotaSpesaModel[] = [];
  public tipiSpesa: Par_TipoSpesaModel[] = [];

  public buttonbar: ButtonItem[] = [];
  private btnApprova: ButtonItem;
  private btnRifiuta: ButtonItem;
  private btnChiudi: ButtonItem;
  private readonly testoApprova: string;
  private readonly testoRifiuta: string;

  constructor(private modalCtrl: ModalController,
              private dipGGNotaSpesaService: DipGGNotaSpesaService,
              private dipGGRichiestaService: DipGGRichiestaService,
              private parTipoSpesaService: ParTipoSpesaService,
              private timeSheetService: TimeSheetService,
              private refresherService: RefresherService,
              private collectionDialogService: CollectionDialogService,
              private elementRef: ElementRef<HTMLElement>,
              private transloco: TranslocoService,
              userInterfaceService: UserInterfaceService) {

    // pulsanti standard di approvazione, rifiuto e chiusura, come nella lista richieste
    this.btnApprova = userInterfaceService.Btn_Approva;
    this.btnApprova.event = () => this.decidi(true);
    this.btnRifiuta = userInterfaceService.Btn_Rifiuta;
    this.btnRifiuta.event = () => this.decidi(false);
    this.testoApprova = this.btnApprova.text ?? '';
    this.testoRifiuta = this.btnRifiuta.text ?? '';
    this.btnChiudi = userInterfaceService.Btn_Chiudi;
    this.btnChiudi.event = () => this.modalCtrl.dismiss(null, 'cancel');
    this.buttonbar = [this.btnChiudi];
  }

  ngOnInit() {
    this.ingrandisciDialog();

    this.parTipoSpesaService.GetAll(new GenericRequest<Par_TipoSpesaInModel>(Par_TipoSpesaInModel)).subscribe(res => {
      this.tipiSpesa = res.data?.par_TipoSpesa ?? [];
    });

    const request = new GenericRequest<Dip_GG_NotaSpesa_Get4Richiesta_InModel>(Dip_GG_NotaSpesa_Get4Richiesta_InModel);
    request.data.idDip_GG_Richiesta = this.idDip_GG_Richiesta;
    this.dipGGNotaSpesaService.Get4Richiesta(request).subscribe(res => {
      this.richiesta = res.data?.dip_GG_Richiesta ?? null;
      this.spese = res.data?.dip_GG_NotaSpesa ?? [];
      this.aggiornaPulsanti();
    });
  }

  // all'apertura il componente e' gia' dentro ion-modal (in ngOnInit puo' non esserlo ancora)
  ionViewWillEnter() {
    this.ingrandisciDialog();
  }

  /** Dialog piu' grande del normale da tablet in su (su mobile resta a tutto schermo). */
  private ingrandisciDialog(): void {
    const modal = this.elementRef.nativeElement.closest('ion-modal');
    if (modal && window.matchMedia('(min-width: 576px)').matches) {
      modal.style.setProperty('--width', 'min(760px, 96vw)');
      modal.style.setProperty('--height', 'min(640px, 92vh)');
    }
  }

  private t(key: string, params?: any): string {
    return this.transloco.translate('attendance.components.notaSpesaDettaglioDialog.' + key, params);
  }

  private aggiornaPulsanti(): void {
    const bar: ButtonItem[] = [];
    if (this.consentiDecisione && this.richiesta) {
      // su una revoca chiesta dal dipendente i pulsanti decidono sulla revoca
      this.btnApprova.text = this.revoca ? this.t('azioneRevoca', { azione: this.testoApprova }) : this.testoApprova;
      this.btnRifiuta.text = this.revoca ? this.t('azioneRevoca', { azione: this.testoRifiuta }) : this.testoRifiuta;
      if (this.timeSheetService.Dip_GG_Richiesta_Admin_Can_Approve(this.richiesta)) bar.push(this.btnApprova);
      if (this.timeSheetService.Dip_GG_Richiesta_Admin_Can_Reject(this.richiesta)) bar.push(this.btnRifiuta);
    }
    bar.push(this.btnChiudi);
    this.buttonbar = bar;
  }

  public get riepilogo() {
    return riepilogoNotaSpesa(this.richiesta?.dati);
  }

  public get totale(): number {
    return this.spese.reduce((t, s) => t + s.importo, 0);
  }

  /** revoca chiesta dal dipendente su una nota gia' approvata */
  public get revoca(): boolean {
    return this.richiesta?.revocaStato === StatoRichiesta.Immessa || this.richiesta?.revocaStato === StatoRichiesta.ApprovazioneInCorso;
  }

  /** testo e colore dello stato della nota, tenendo conto della revoca */
  public get stato(): { testo: string | null, colore: string } {
    if (this.richiesta?.revocaStato === StatoRichiesta.Approvata)
      return { testo: this.t('revocata'), colore: 'dark' };
    if (this.revoca)
      return { testo: this.t('revocaRichiesta'), colore: 'warning' };
    switch (this.richiesta?.richiestaStato) {
      case StatoRichiesta.Approvata: return { testo: null, colore: 'success' };
      case StatoRichiesta.Rifiutata: return { testo: null, colore: 'danger' };
      case StatoRichiesta.Cancellata: return { testo: null, colore: 'dark' };
      default: return { testo: null, colore: 'medium' };
    }
  }

  public tipo(id: number): string {
    return this.tipiSpesa.find(t => t.id === id)?.descrizione ?? '';
  }

  public euro(valore: number): string {
    return '€ ' + (valore ?? 0).toFixed(2).replace('.', ',');
  }

  public apriAllegato(allegato: Dip_GG_NotaSpesa_AllegatoModel): void {
    this.dipGGNotaSpesaService.ApriAllegato(allegato);
  }

  /** approva o rifiuta la nota spese (o la sua revoca), con conferma */
  private async decidi(approva: boolean): Promise<void> {
    if (!this.richiesta)
      return;
    const chiave = (approva ? 'confermaApprovazione' : 'confermaRifiuto') + (this.revoca ? 'Revoca' : '');
    const conferma = await this.collectionDialogService.ConfirmCancelDialog(this.t(chiave));
    if (!conferma)
      return;

    const request = new GenericRequest<Dip_GG_Richiesta_SetState_InModel>(Dip_GG_Richiesta_SetState_InModel);
    request.data.fromHR = true;
    request.data.richiestaStato = approva ? StatoRichiesta.Approvata : StatoRichiesta.Rifiutata;
    request.data.idDip_GG_Richiesta = [this.richiesta.id];
    this.dipGGRichiestaService.SetState(request).subscribe(res => {
      if (res.success) {
        // le pagine che mostrano le richieste (calendari) si rileggono
        this.refresherService.Dip_GG_Richiesta_triggerRefresh();
        this.modalCtrl.dismiss(true, 'confirm');
      }
    });
  }
}
