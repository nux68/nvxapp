import { Component, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { SmartWorkingService } from '../../../ClientServer-Service/GestionePresenze/SmartWorking/smart-working.service';
import { Dip_AccordoSW_CalendarioInModel, Dip_AccordoSW_CalendarioOutModel, LavoroAgileGiornoModel, LavoroAgileSettimanaModel } from '../../../ClientServer-Service/GestionePresenze/SmartWorking/Models/smart-working-model';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { Dip_GG_Richiesta_Send_InModel, StatoRichiesta, TipoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';

/*
 Calendario del lavoro agile (AT-SW.2): giorni consentiti dall'accordo firmato, giorni richiesti e approvati, giorni usati
 per settimana. Clic su un giorno consentito = richiesta del giorno (richiesta di giustificativo "lavoro agile", con gli
 approvatori del reparto); il server controlla accordo, giorni consentiti e limite settimanale.
*/
@Component({
  selector: 'app-lavoro-agile-page',
  templateUrl: './lavoro-agile-page.component.html',
  styleUrls: ['./lavoro-agile-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class LavoroAgilePageComponent {

  public title: string;
  public mese = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
  public dati: Dip_AccordoSW_CalendarioOutModel | null = null;
  public settimane: { lunedi: Date, giorni: (LavoroAgileGiornoModel | null)[], conteggio: LavoroAgileSettimanaModel | null }[] = [];
  public readonly stato = StatoRichiesta;
  public readonly intestazione = [0, 1, 2, 3, 4, 5, 6];

  constructor(private smartWorkingService: SmartWorkingService,
              private dipGGRichiestaService: DipGGRichiestaService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('attendance.pages.lavoroAgile.title');
  }

  ionViewWillEnter() {
    this.carica();
  }

  get meseTesto(): string {
    return this.mese.toLocaleDateString(this.transloco.getActiveLang(), { month: 'long', year: 'numeric' });
  }

  nomeGiorno(i: number): string {
    return this.transloco.translate('attendance.pages.turni.wd' + i);
  }

  sposta(verso: number) {
    this.mese = new Date(this.mese.getFullYear(), this.mese.getMonth() + verso, 1);
    this.carica();
  }

  carica() {
    const request = new GenericRequest<Dip_AccordoSW_CalendarioInModel>(Dip_AccordoSW_CalendarioInModel);
    request.data.year = this.mese.getFullYear();
    request.data.month = this.mese.getMonth() + 1;
    this.smartWorkingService.Calendario(request).subscribe(res => {
      this.dati = res.data ?? null;
      this.componi();
    });
  }

  private componi() {
    this.settimane = [];
    if (!this.dati)
      return;
    const primo = new Date(this.mese);
    const lunedi = new Date(primo);
    lunedi.setDate(primo.getDate() - ((primo.getDay() + 6) % 7));
    for (let l = new Date(lunedi); l.getMonth() <= this.mese.getMonth() || l.getFullYear() < this.mese.getFullYear(); l.setDate(l.getDate() + 7)) {
      if (l.getFullYear() > this.mese.getFullYear()) break;
      const giorni: (LavoroAgileGiornoModel | null)[] = [];
      for (let i = 0; i < 7; i++) {
        const d = new Date(l);
        d.setDate(l.getDate() + i);
        const k = this.chiave(d);
        giorni.push(d.getMonth() === this.mese.getMonth() ? (this.dati.giorni.find(g => g.data.substring(0, 10) === k) ?? null) : null);
      }
      const kl = this.chiave(l);
      this.settimane.push({ lunedi: new Date(l), giorni, conteggio: this.dati.settimane.find(s => s.lunedi.substring(0, 10) === kl) ?? null });
    }
  }

  private chiave(d: Date): string {
    const p = (n: number) => n.toString().padStart(2, '0');
    return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}`;
  }

  giornoNumero(g: LavoroAgileGiornoModel): number {
    return Number(g.data.substring(8, 10));
  }

  approvato(g: LavoroAgileGiornoModel): boolean {
    return g.stato === StatoRichiesta.Approvata || g.stato === StatoRichiesta.Diretta;
  }

  async richiedi(g: LavoroAgileGiornoModel | null) {
    if (!g || !this.dati || g.stato !== null || !g.consentito || !g.lavorativo)
      return;
    const data = `${g.data.substring(8, 10)}/${g.data.substring(5, 7)}/${g.data.substring(0, 4)}`;
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.lavoroAgile.confirm', { date: data })))
      return;
    const request = new GenericRequest<Dip_GG_Richiesta_Send_InModel>(Dip_GG_Richiesta_Send_InModel);
    request.data.excludeRicalc = false;
    request.data.fromHR = false;
    request.data.dip_GG_Richiesta.id = 0;
    request.data.dip_GG_Richiesta.idDip_RapportoLavoro = 0;
    request.data.dip_GG_Richiesta.richiestaStato = StatoRichiesta.Immessa;
    request.data.dip_GG_Richiesta.richiestaTipo = TipoRichiesta.Giustificativo;
    request.data.dip_GG_Richiesta.data = data;
    request.data.dip_GG_Richiesta.dataA = data;
    request.data.dip_GG_Richiesta.dati = JSON.stringify({ hhmm: '', allDay: true, idPar_Giustificativi: this.dati.idPar_Giustificativi });
    this.dipGGRichiestaService.Send(request).subscribe(() => this.carica());
  }
}
