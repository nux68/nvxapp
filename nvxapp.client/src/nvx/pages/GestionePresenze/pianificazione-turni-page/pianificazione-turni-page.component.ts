import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ActionSheetController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { TurniService } from '../../../ClientServer-Service/GestionePresenze/Turni/turni.service';
import {
  Dip_Turno_PianificazioneInModel, Dip_Turno_PianificazioneOutModel, Dip_Turno_PutInModel, Dip_Turno_PutRiga, Par_TurnoModel,
  TurnoAssenzaModel, TurnoConsuntivoModel, TurnoCoperturaGiornoModel, TurnoDipendenteModel, dataLocale
} from '../../../ClientServer-Service/GestionePresenze/Turni/Models/turni-model';
import { AzSediRepartoService } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/az-sedi-reparto.service';
import { Az_SediRepartoModel, Az_SediReparto_GetAll_InModel } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';

type Vista = 'settimana' | 'mese';

/*
 Pianificazione dei turni di un reparto (AT-TURNI.2, ANALISI_ATTENDANCETRACKING_TURNI.md §4):
 griglia dipendenti x giorni (settimana o mese), clic sulla cella = scelta del turno (salvato subito, il giorno si ricalcola),
 righe delle coperture minime (rosse se insufficienti), assenze, conflitti e consuntivo dei giorni calcolati.
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

  turno(dip: TurnoDipendenteModel, d: Date): Par_TurnoModel | null {
    const a = this.dati?.assegnazioni.find(x => x.idDip_RapportoLavoro === dip.idDip_RapportoLavoro && x.data.substring(0, 10) === this.chiave(d));
    return a ? (this.dati?.turni.find(t => t.id === a.idPar_Turno) ?? null) : null;
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

  async scegliTurno(dip: TurnoDipendenteModel, d: Date) {
    if (!this.dati)
      return;
    const attuale = this.turno(dip, d);
    const buttons: any[] = this.dati.turni.map(t => ({
      text: `${t.codice} - ${t.descrizione}${t.idPar_Orario ? ' (' + (t.orarioTesto ?? '') + ')' : ''}`,
      cssClass: attuale?.id === t.id ? 'nvx-selezionato' : undefined,
      handler: () => { this.salva(dip, d, t.id); }
    }));
    if (attuale)
      buttons.push({ text: this.transloco.translate('attendance.pages.pianificazioneTurni.noShift'), role: 'destructive', handler: () => { this.salva(dip, d, 0); } });
    buttons.push({ text: this.transloco.translate('common.buttons.cancel'), role: 'cancel' });

    const sheet = await this.actionSheetCtrl.create({
      header: `${dip.nominativo} - ${d.toLocaleDateString(this.transloco.getActiveLang())}`,
      buttons
    });
    await sheet.present();
  }

  private salva(dip: TurnoDipendenteModel, d: Date, idPar_Turno: number) {
    const request = new GenericRequest<Dip_Turno_PutInModel>(Dip_Turno_PutInModel);
    request.data.idAz_SediReparto = this.idAz_SediReparto ?? 0;
    const riga = new Dip_Turno_PutRiga();
    riga.idDip_RapportoLavoro = dip.idDip_RapportoLavoro;
    riga.data = dataLocale(d);
    riga.idPar_Turno = idPar_Turno;
    request.data.assegnazioni = [riga];
    this.turniService.Put(request).subscribe(() => this.carica());
  }
}
