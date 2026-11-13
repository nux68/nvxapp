import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ActionSheetController, AlertController, ModalController, ToastController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { TurniService } from '../../../ClientServer-Service/GestionePresenze/Turni/turni.service';
import {
  Dip_Turno_PianificazioneInModel, Dip_Turno_PianificazioneOutModel, Dip_Turno_PutInModel, Dip_Turno_PutRiga, Par_TurnoModel,
  TurnoAssenzaModel, TurnoConsuntivoModel, TurnoCoperturaGiornoModel, TurnoDipendenteModel, dataLocale
} from '../../../ClientServer-Service/GestionePresenze/Turni/Models/turni-model';
import { AzSediRepartoService } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/az-sedi-reparto.service';
import { Az_SediRepartoModel, Az_SediReparto_GetAll_InModel } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';
import { DipSelectorItem, DipSelectorModalComponent, DipSelectorResult } from '../../../shared/components/infrastructure/dip-selector-modal/dip-selector-modal.component';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { SeletionParTurnoDialogComponent, SeletionParTurnoDialogResult } from '../../../shared/components/GestionePresenze/seletion-par-turno-dialog/seletion-par-turno-dialog.component';

type Vista = 'settimana' | 'mese';

/*
 Pianificazione dei turni di un reparto (AT-TURNI.2, ANALISI_ATTENDANCETRACKING_TURNI.md §4):
 griglia dipendenti x giorni (settimana o mese), clic sulla cella = scelta dei turni del giorno (uno o piu', compatibili:
 niente sovrapposizioni, il riposo da solo; salvati subito, il giorno si ricalcola),
 righe delle coperture minime (rosse se insufficienti), assenze, conflitti e consuntivo dei giorni calcolati.
 Duplica settimana (vista settimana): la settimana visualizzata si copia in una delle settimane successive per i dipendenti
 scelti (dialog standard di selezione multipla); nella destinazione i loro turni diventano uguali a quelli di partenza.
*/
@Component({
  selector: 'app-pianificazione-turni-page',
  templateUrl: './pianificazione-turni-page.component.html',
  styleUrls: ['./pianificazione-turni-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class PianificazioneTurniPageComponent {

  public title: string;
  public reparti: Az_SediRepartoModel[] = [];
  public idAz_SediReparto: number | null = null;
  public vista: Vista = 'settimana';
  public inizio: Date = this.lunedi(new Date());
  public giorni: Date[] = [];
  public dati: Dip_Turno_PianificazioneOutModel | null = null;

  constructor(private turniService: TurniService,
              private azSediRepartoService: AzSediRepartoService,
              private actionSheetCtrl: ActionSheetController,
              private alertCtrl: AlertController,
              private modalCtrl: ModalController,
              private toastCtrl: ToastController,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('attendance.pages.pianificazioneTurni.title');
  }

  ionViewWillEnter() {
    this.azSediRepartoService.GetAll(new GenericRequest<Az_SediReparto_GetAll_InModel>(Az_SediReparto_GetAll_InModel)).subscribe(res => {
      this.reparti = res.data?.az_SediReparto ?? [];
      if (!this.idAz_SediReparto && this.reparti.length > 0)
        this.idAz_SediReparto = this.reparti[0].id;
      this.carica();
    });
  }

  private lunedi(d: Date): Date {
    const x = new Date(d.getFullYear(), d.getMonth(), d.getDate());
    const dow = (x.getDay() + 6) % 7;
    x.setDate(x.getDate() - dow);
    return x;
  }

  private periodo(): { dal: Date, al: Date } {
    if (this.vista === 'settimana') {
      const al = new Date(this.inizio);
      al.setDate(al.getDate() + 6);
      return { dal: this.inizio, al };
    }
    const dal = new Date(this.inizio.getFullYear(), this.inizio.getMonth(), 1);
    return { dal, al: new Date(dal.getFullYear(), dal.getMonth() + 1, 0) };
  }

  get periodoTesto(): string {
    const { dal, al } = this.periodo();
    const f = (d: Date) => d.toLocaleDateString(this.transloco.getActiveLang(), { day: '2-digit', month: '2-digit', year: 'numeric' });
    return this.vista === 'mese'
      ? dal.toLocaleDateString(this.transloco.getActiveLang(), { month: 'long', year: 'numeric' })
      : `${f(dal)} - ${f(al)}`;
  }

  cambiaVista(v: any) {
    this.vista = v === 'mese' ? 'mese' : 'settimana';
    this.inizio = this.vista === 'mese' ? new Date(this.inizio.getFullYear(), this.inizio.getMonth(), 1) : this.lunedi(this.inizio);
    this.carica();
  }

  sposta(verso: number) {
    const d = new Date(this.inizio);
    if (this.vista === 'settimana')
      d.setDate(d.getDate() + 7 * verso);
    else
      d.setMonth(d.getMonth() + verso, 1);
    this.inizio = d;
    this.carica();
  }

  oggi() {
    this.inizio = this.vista === 'settimana' ? this.lunedi(new Date()) : new Date(new Date().getFullYear(), new Date().getMonth(), 1);
    this.carica();
  }

  carica() {
    if (!this.idAz_SediReparto)
      return;
    const { dal, al } = this.periodo();
    this.giorni = [];
    for (let d = new Date(dal); d <= al; d.setDate(d.getDate() + 1))
      this.giorni.push(new Date(d));

    const request = new GenericRequest<Dip_Turno_PianificazioneInModel>(Dip_Turno_PianificazioneInModel);
    request.data.idAz_SediReparto = this.idAz_SediReparto;
    request.data.dal = dataLocale(dal);
    request.data.al = dataLocale(al);
    this.turniService.Pianificazione(request).subscribe(res => {
      this.dati = res.success ? res.data : null;
    });
  }

  // ── lettura della griglia ──────────────────────────────────────────

  private chiave(d: Date): string { return dataLocale(d).substring(0, 10); }

  /** turni del dipendente nel giorno, in ordine di orario */
  turni(dip: TurnoDipendenteModel, d: Date): Par_TurnoModel[] {
    const ids = (this.dati?.assegnazioni ?? [])
      .filter(x => x.idDip_RapportoLavoro === dip.idDip_RapportoLavoro && x.data.substring(0, 10) === this.chiave(d))
      .map(x => x.idPar_Turno);
    return (this.dati?.turni ?? []).filter(t => ids.includes(t.id))
      .sort((a, b) => (a.orarioTesto ?? '').localeCompare(b.orarioTesto ?? ''));
  }

  assenze(dip: TurnoDipendenteModel, d: Date): TurnoAssenzaModel[] {
    return this.dati?.assenze.filter(x => x.idDip_RapportoLavoro === dip.idDip_RapportoLavoro && x.data.substring(0, 10) === this.chiave(d)) ?? [];
  }

  conflitto(dip: TurnoDipendenteModel, d: Date): string | null {
    const c = this.dati?.conflitti.filter(x => x.idDip_RapportoLavoro === dip.idDip_RapportoLavoro && x.data.substring(0, 10) === this.chiave(d)) ?? [];
    return c.length > 0 ? c.map(x => x.messaggio).join('\n') : null;
  }

  conflittoGrave(dip: TurnoDipendenteModel, d: Date): boolean {
    return this.dati?.conflitti.some(x => x.idDip_RapportoLavoro === dip.idDip_RapportoLavoro && x.data.substring(0, 10) === this.chiave(d) && x.grave) ?? false;
  }

  consuntivo(dip: TurnoDipendenteModel, d: Date): TurnoConsuntivoModel | null {
    return this.dati?.consuntivo.find(x => x.idDip_RapportoLavoro === dip.idDip_RapportoLavoro && x.data.substring(0, 10) === this.chiave(d)) ?? null;
  }

  nome(idDip_RapportoLavoro: number): string {
    return this.dati?.dipendenti.find(x => x.idDip_RapportoLavoro === idDip_RapportoLavoro)?.nominativo ?? '';
  }

  ore(min: number): string {
    return `${Math.floor(min / 60)}:${(min % 60).toString().padStart(2, '0')}`;
  }

  get turniCopertura(): Par_TurnoModel[] {
    const ids = new Set((this.dati?.turniReparto ?? []).filter(t => t.minPersone > 0).map(t => t.idPar_Turno));
    return (this.dati?.turni ?? []).filter(t => ids.has(t.id));
  }

  copertura(t: Par_TurnoModel, d: Date): TurnoCoperturaGiornoModel | null {
    return this.dati?.coperture.find(c => c.idPar_Turno === t.id && c.data.substring(0, 10) === this.chiave(d)) ?? null;
  }

  festivo(d: Date): boolean {
    return d.getDay() === 0 || d.getDay() === 6;
  }

  intestazioneGiorno(d: Date): string {
    return this.transloco.translate('attendance.pages.turni.wd' + ((d.getDay() + 6) % 7)) + ' ' + d.getDate();
  }

  // ── modifica ───────────────────────────────────────────────────────

  /**
   turni del giorno con il dialog standard di selezione (SeletionParTurnoDialogComponent): uno o piu', nessuno = giorno senza
   turno; la compatibilita' la controlla il server
  */
  async scegliTurno(dip: TurnoDipendenteModel, d: Date) {
    if (!this.dati)
      return;
    const attuali = new Set(this.turni(dip, d).map(t => t.id));
    const modal = await this.modalCtrl.create({
      component: SeletionParTurnoDialogComponent,
      componentProps: {
        turni: this.dati.turni,
        selezionati: [...attuali],
        sottotitolo: `${dip.nominativo} - ${d.toLocaleDateString(this.transloco.getActiveLang())}`,
      }
    });
    await modal.present();
    const { data, role } = await modal.onWillDismiss<SeletionParTurnoDialogResult>();
    if (role !== 'confirm' || !data)
      return;
    const scelti = data.id;
    if (scelti.length === attuali.size && scelti.every(x => attuali.has(x)))
      return;
    this.salva(dip, d, scelti);
  }

  // ── duplica settimana ──────────────────────────────────────────────

  /** settimane in cui si puo' copiare quella visualizzata (le 8 successive) */
  private static readonly SETTIMANE_DESTINAZIONE = 8;

  private testoSettimana(lunedi: Date): string {
    const al = new Date(lunedi);
    al.setDate(al.getDate() + 6);
    const f = (d: Date) => d.toLocaleDateString(this.transloco.getActiveLang(), { day: '2-digit', month: '2-digit', year: 'numeric' });
    return `${f(lunedi)} - ${f(al)}`;
  }

  private async avviso(chiave: string, params?: any) {
    const toast = await this.toastCtrl.create({ message: this.transloco.translate(chiave, params), duration: 4000, position: 'top' });
    await toast.present();
  }

  /**
   copia la settimana visualizzata in un'altra: scelta della settimana, dei dipendenti (proposti quelli con turni nella
   settimana di partenza), conferma; per ogni dipendente scelto i 7 giorni di destinazione prendono il turno del giorno
   corrispondente (nessun turno = il turno della destinazione si toglie), con il ricalcolo dei giorni (Dip_Turno/Put)
  */
  async duplicaSettimana() {
    if (!this.dati || this.vista !== 'settimana' || !this.idAz_SediReparto)
      return;
    const t = (k: string, p?: any) => this.transloco.translate('attendance.pages.pianificazioneTurni.' + k, p);
    const sorgente = new Date(this.inizio);
    const assegnazioni = this.dati.assegnazioni;
    if (assegnazioni.length === 0) {
      await this.avviso('attendance.pages.pianificazioneTurni.duplicateEmpty');
      return;
    }

    // 1. settimana di destinazione
    const settimane: Date[] = [];
    for (let i = 1; i <= PianificazioneTurniPageComponent.SETTIMANE_DESTINAZIONE; i++) {
      const d = new Date(sorgente);
      d.setDate(d.getDate() + 7 * i);
      settimane.push(d);
    }
    const alert = await this.alertCtrl.create({
      header: t('duplicateTarget'),
      subHeader: t('duplicateFrom', { periodo: this.testoSettimana(sorgente) }),
      inputs: settimane.map((d, i) => ({ type: 'radio' as const, label: this.testoSettimana(d), value: i, checked: i === 0 })),
      buttons: [
        { text: this.transloco.translate('common.buttons.cancel'), role: 'cancel' },
        { text: t('duplicateNext'), role: 'confirm' },
      ],
    });
    await alert.present();
    const sceltaSettimana = await alert.onDidDismiss();
    if (sceltaSettimana.role !== 'confirm' || sceltaSettimana.data?.values == null)
      return;
    const destinazione = settimane[Number(sceltaSettimana.data.values)];

    // 2. dipendenti (dialog standard a selezione multipla)
    const conTurni = new Set(assegnazioni.map(a => a.idDip_RapportoLavoro));
    const items: DipSelectorItem[] = this.dati.dipendenti.map(d => ({ id: d.idDip_RapportoLavoro, cognome: d.nominativo, nome: '' }));
    const modal = await this.modalCtrl.create({
      component: DipSelectorModalComponent,
      componentProps: { mode: 'multi', items, preselected: [...conTurni] }
    });
    await modal.present();
    const { data, role } = await modal.onWillDismiss<DipSelectorResult<number>>();
    if (role !== 'confirm' || !data || data.selected.length === 0)
      return;

    // 3. conferma: nella destinazione i turni dei dipendenti scelti si sostituiscono
    if (!await this.collectionDialogService.ConfirmCancelDialog(t('duplicateConfirm', {
          dal: this.testoSettimana(sorgente), al: this.testoSettimana(destinazione), n: data.selected.length })))
      return;

    // 4. righe: 7 giorni per dipendente, stesso giorno della settimana
    const request = new GenericRequest<Dip_Turno_PutInModel>(Dip_Turno_PutInModel);
    request.data.idAz_SediReparto = this.idAz_SediReparto;
    request.data.assegnazioni = [];
    for (const idRapporto of data.selected) {
      for (let g = 0; g < 7; g++) {
        const giornoSorgente = new Date(sorgente);
        giornoSorgente.setDate(giornoSorgente.getDate() + g);
        const giornoDestinazione = new Date(destinazione);
        giornoDestinazione.setDate(giornoDestinazione.getDate() + g);
        // tutti i turni del giorno di partenza (anche piu' d'uno); nessuno = la destinazione resta senza turni
        const delGiorno = assegnazioni.filter(x => x.idDip_RapportoLavoro === idRapporto && x.data.substring(0, 10) === this.chiave(giornoSorgente));
        const riga = new Dip_Turno_PutRiga();
        riga.idDip_RapportoLavoro = idRapporto;
        riga.data = dataLocale(giornoDestinazione);
        riga.idPar_Turni = delGiorno.map(x => x.idPar_Turno);
        riga.note = delGiorno.find(x => !!x.note)?.note ?? null;
        request.data.assegnazioni.push(riga);
      }
    }
    this.turniService.Put(request).subscribe(async res => {
      if (!res.success)
        return;
      await this.avviso('attendance.pages.pianificazioneTurni.duplicateDone', { salvati: res.data?.salvati ?? 0, tolti: res.data?.tolti ?? 0 });
      // si va alla settimana copiata
      this.inizio = destinazione;
      this.carica();
    });
  }

  private salva(dip: TurnoDipendenteModel, d: Date, idPar_Turni: number[]) {
    const request = new GenericRequest<Dip_Turno_PutInModel>(Dip_Turno_PutInModel);
    request.data.idAz_SediReparto = this.idAz_SediReparto ?? 0;
    const riga = new Dip_Turno_PutRiga();
    riga.idDip_RapportoLavoro = dip.idDip_RapportoLavoro;
    riga.data = dataLocale(d);
    riga.idPar_Turni = idPar_Turni;
    request.data.assegnazioni = [riga];
    this.turniService.Put(request).subscribe(() => this.carica());
  }
}
