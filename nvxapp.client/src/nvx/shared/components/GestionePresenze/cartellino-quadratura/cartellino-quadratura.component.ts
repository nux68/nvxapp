import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, ChangeDetectionStrategy } from '@angular/core';
import { DatePipe } from '@angular/common';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { DipPeriodoService } from '../../../../ClientServer-Service/GestionePresenze/Dip_Periodo/dip-periodo.service';
import { Dip_Periodo_Get_InModel, Dip_Periodo_Get_OutModel, Dip_Periodo_Giorno, Dip_Periodo_SetRiconosciute_InModel, Dip_Periodo_SetStato_InModel, minutiToHHMM, PeriodoStato } from '../../../../ClientServer-Service/GestionePresenze/Dip_Periodo/Models/dip-periodo-model';
import { DestinazioneEccedenze, TipoContratto } from '../../../../ClientServer-Service/GestionePresenze/Dip_RapportoLavoro/Models/dip-rapporto-lavoro-model';
import { StraoTipoConteggio } from '../../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/Models/par-profilo-orario-model';
import { CollectionDialogService } from '../../infrastructure/generic-dialog/collection-dialog.service';

/** Voce del riepilogo: la chiave "dettaglio" indica quali giorni l'hanno generata (drill-down). */
interface VoceRiepilogo {
  etichetta: string;
  minuti: number;
  nota?: string;
  livello?: number;                                  // 1 = "di cui"
  dettaglio?: (g: Dip_Periodo_Giorno) => number;
  evidenza?: 'positivo' | 'negativo' | 'provvisorio';
}

/*
 Cartellino di quadratura del mese: totali del periodo, stato (provvisorio / da approvare / approvato),
 dettaglio dei giorni che hanno generato le ore in piu' e, per l'HR, approvazione e ore riconosciute.
*/
@Component({
  selector: 'app-cartellino-quadratura',
  templateUrl: './cartellino-quadratura.component.html',
  styleUrls: ['./cartellino-quadratura.component.scss'],
  providers: [DatePipe],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CartellinoQuadraturaComponent implements OnChanges {

  @Input() userId: string | null | undefined = null;   // vuoto = utente corrente
  @Input() year: number;
  @Input() month: number;                               // 1-12
  @Input() isHR: boolean = false;
  @Input() refresh: number = 0;                         // incrementato dal chiamante per rileggere
  @Output() changed = new EventEmitter<void>();

  public dati: Dip_Periodo_Get_OutModel | null = null;
  public voci: VoceRiepilogo[] = [];
  public voceAperta: VoceRiepilogo | null = null;
  public aperto = false;
  public riconosciuteTesto = '';

  public PeriodoStato = PeriodoStato;
  public hhmm = minutiToHHMM;

  constructor(private dipPeriodoService: DipPeriodoService,
              private collectionDialogService: CollectionDialogService,
              private datePipe: DatePipe) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.year && this.month)
      this.load();
  }

  load() {
    const request = new GenericRequest<Dip_Periodo_Get_InModel>(Dip_Periodo_Get_InModel);
    request.data.idAspNetUsers = this.userId ?? null;
    request.data.anno = this.year;
    request.data.mese = this.month;
    this.dipPeriodoService.Dip_Periodo_Get(request).subscribe(res => {
      this.dati = res.data;
      this.voceAperta = null;
      this.costruisciVoci();
      const ric = this.dati?.dip_Periodo?.min_Riconosciute;
      this.riconosciuteTesto = ric === null || ric === undefined ? '' : minutiToHHMM(ric);
    });
  }

  get stato(): PeriodoStato {
    return this.dati?.dip_Periodo?.stato ?? PeriodoStato.Provvisorio;
  }

  get statoTesto(): string {
    switch (this.stato) {
      case PeriodoStato.Approvato: return 'Approvato';
      case PeriodoStato.DaApprovare: return 'Chiuso, in attesa di approvazione';
      default: return 'Provvisorio, periodo in corso';
    }
  }

  get statoColore(): string {
    switch (this.stato) {
      case PeriodoStato.Approvato: return 'success';
      case PeriodoStato.DaApprovare: return 'warning';
      default: return 'medium';
    }
  }

  get statoIcona(): string {
    switch (this.stato) {
      case PeriodoStato.Approvato: return 'checkmark-circle';
      case PeriodoStato.DaApprovare: return 'hourglass';
      default: return 'time';
    }
  }

  get conteggioTesto(): string {
    switch (this.dati?.tipoConteggio) {
      case StraoTipoConteggio.Settimanale: return 'conteggio settimanale';
      case StraoTipoConteggio.Mensile: return 'conteggio mensile';
      default: return 'conteggio giornaliero';
    }
  }

  get conteggioSettimanale(): boolean {
    return this.dati?.tipoConteggio === StraoTipoConteggio.Settimanale;
  }

  get partTime(): boolean {
    return this.dati?.tipoContratto === TipoContratto.PartTime;
  }

  get destinazioneTesto(): string {
    return this.dati?.destinazioneEccedenze === DestinazioneEccedenze.BancaOre ? 'Accreditate in banca ore' : 'In pagamento';
  }

  private costruisciVoci() {
    const p = this.dati?.dip_Periodo;
    if (!this.dati || !p) {
      this.voci = [];
      return;
    }

    const voci: VoceRiepilogo[] = [
      { etichetta: 'Ore teoriche previste', minuti: p.min_Teo, nota: this.partTime && this.dati.oreSettimanaliContratto ? `part-time ${this.dati.oreSettimanaliContratto}h/settimana` : undefined },
      { etichetta: 'Ore lavorate effettive', minuti: p.min_Lav, nota: 'timbrature nette' },
      { etichetta: 'di cui ordinarie', minuti: p.min_Ord, livello: 1, nota: 'retribuite con paga base' },
      { etichetta: 'Permessi, ferie, assenze', minuti: p.min_Assenze, nota: 'sottratti dal teorico' },
      { etichetta: 'Ore in più', minuti: p.min_Ecc, nota: 'oltre il teorico, dopo arrotondamenti', dettaglio: g => g.min_Ecc, evidenza: 'positivo' },
    ];

    if (p.min_Provvisorie > 0)
      voci.push({ etichetta: 'in attesa di quadratura', minuti: p.min_Provvisorie, livello: 1, nota: 'periodo di conteggio in corso', dettaglio: g => g.eccProvvisoria ? g.min_Ecc : 0, evidenza: 'provvisorio' });
    if (p.min_Compensate > 0)
      voci.push({ etichetta: 'compensate', minuti: p.min_Compensate, livello: 1, nota: 'recuperano ore mancanti nello stesso periodo' });
    if (this.partTime || p.min_Suppl + p.min_SupplEcc > 0) {
      voci.push({ etichetta: 'Lavoro supplementare', minuti: p.min_Suppl, livello: 1, nota: 'entro il limite del contratto', dettaglio: g => g.min_Suppl });
      if (p.min_SupplEcc > 0)
        voci.push({ etichetta: 'Supplementare oltre il limite', minuti: p.min_SupplEcc, livello: 1, dettaglio: g => g.min_SupplEcc });
    }
    voci.push({ etichetta: 'Lavoro straordinario', minuti: p.min_Strao, livello: 1, nota: 'oltre la soglia full-time', dettaglio: g => g.min_Strao });
    if (p.min_StraoNott > 0)
      voci.push({ etichetta: 'Straordinario notturno', minuti: p.min_StraoNott, livello: 1, dettaglio: g => g.min_StraoNott });
    if (p.min_StraoFest > 0)
      voci.push({ etichetta: 'Straordinario festivo', minuti: p.min_StraoFest, livello: 1, dettaglio: g => g.min_StraoFest });
    if (p.min_StraoFestNott > 0)
      voci.push({ etichetta: 'Straordinario festivo notturno', minuti: p.min_StraoFestNott, livello: 1, dettaglio: g => g.min_StraoFestNott });
    if (p.min_EccOrd > 0)
      voci.push({ etichetta: 'Ore eccedenti non straordinarie', minuti: p.min_EccOrd, livello: 1, nota: 'oltre il teorico ma entro la soglia', dettaglio: g => g.min_EccOrd });
    if (p.min_NonAutorizzate > 0) {
      voci.push({ etichetta: 'Ore in più non autorizzate', minuti: p.min_NonAutorizzate, nota: 'oltre l\'autorizzazione approvata: non riconosciute', dettaglio: g => g.min_NonAutorizzate, evidenza: 'negativo' });
      if (p.min_DaAutorizzare > 0)
        voci.push({ etichetta: 'in attesa di autorizzazione', minuti: p.min_DaAutorizzare, livello: 1, nota: 'richieste ancora da approvare', dettaglio: g => g.min_DaAutorizzare, evidenza: 'provvisorio' });
    }
    if (p.min_NonRiconosciute > 0)
      voci.push({ etichetta: 'Non riconosciute', minuti: p.min_NonRiconosciute, livello: 1, nota: 'escluse dall\'HR', dettaglio: g => g.min_NonRiconosciute, evidenza: 'negativo' });

    voci.push({ etichetta: 'Saldo del periodo', minuti: this.dati.min_Saldo, nota: 'lavorato meno lavorabile, fino a ieri', evidenza: this.dati.min_Saldo < 0 ? 'negativo' : 'positivo' });

    this.voci = voci;
  }

  apriVoce(voce: VoceRiepilogo) {
    if (!voce.dettaglio || voce.minuti === 0)
      return;
    this.voceAperta = this.voceAperta === voce ? null : voce;
  }

  /** Giorni che hanno generato la voce aperta (principio causa-effetto). */
  giorniVoce(): { testo: string, minuti: number, provvisoria: boolean }[] {
    if (!this.voceAperta?.dettaglio || !this.dati)
      return [];
    const f = this.voceAperta.dettaglio;
    return this.dati.giorni
      .filter(g => f(g) > 0)
      .map(g => ({
        testo: (this.datePipe.transform(g.data, 'EEE dd/MM') ?? '') + (g.festivo ? ' (festivo)' : ''),
        minuti: f(g),
        provvisoria: g.eccProvvisoria
      }));
  }

  totaleQualificate(): number {
    const p = this.dati?.dip_Periodo;
    if (!p) return 0;
    return p.min_Suppl + p.min_SupplEcc + p.min_Strao + p.min_StraoNott + p.min_StraoFest + p.min_StraoFestNott + p.min_EccOrd;
  }

  // ── azioni HR ──────────────────────────────────────────────────────────

  async cambiaStato(stato: PeriodoStato) {
    const p = this.dati?.dip_Periodo;
    if (!p) return;

    const domanda = stato === PeriodoStato.Approvato
      ? `Approvi il cartellino di ${String(p.mese).padStart(2, '0')}/${p.anno}? I giorni del mese non saranno più modificabili.`
      : `Riapri il cartellino di ${String(p.mese).padStart(2, '0')}/${p.anno}? I giorni tornano modificabili e ricalcolabili.`;
    if (!await this.collectionDialogService.ConfirmCancelDialog(domanda))
      return;

    const request = new GenericRequest<Dip_Periodo_SetStato_InModel>(Dip_Periodo_SetStato_InModel);
    request.data.id = [p.id];
    request.data.stato = stato;
    this.dipPeriodoService.Dip_Periodo_SetStato(request).subscribe(() => {
      this.load();
      this.changed.emit();
    });
  }

  /** Tetto delle ore in piu' riconosciute ("h:mm"; vuoto = tutte, 0 = nessuna). Avvia il ricalcolo del mese. */
  salvaRiconosciute(valore: string | null) {
    const p = this.dati?.dip_Periodo;
    if (!p) return;

    let minuti: number | null = null;
    if (valore !== null && valore.trim() !== '') {
      const [h, m] = valore.trim().split(':');
      minuti = (parseInt(h, 10) || 0) * 60 + (parseInt(m ?? '0', 10) || 0);
    }

    const request = new GenericRequest<Dip_Periodo_SetRiconosciute_InModel>(Dip_Periodo_SetRiconosciute_InModel);
    request.data.id = p.id;
    request.data.min_Riconosciute = minuti;
    request.data.note = p.note;
    this.dipPeriodoService.Dip_Periodo_SetRiconosciute(request).subscribe(() => {
      // il ricalcolo gira in background: il calendario si aggiorna alla notifica di fine calcolo
      this.load();
      this.changed.emit();
    });
  }
}
