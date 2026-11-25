import { Component, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { Subscription } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DipAnomalieService } from '../../../ClientServer-Service/GestionePresenze/Dip_Anomalie/dip-anomalie.service';
import {
  AnomaliaGiornoModel, AnomaliaGravita, AnomalieRisultato, Dip_Anomalie_GetInModel, Dip_PrevisioneAssenze_GetInModel, PrevisioneGiornoModel, PrevisioneRisultato
} from '../../../ClientServer-Service/GestionePresenze/Dip_Anomalie/Models/dip-anomalie-model';
import { LongJobNotifierService } from '../../../Utility/infrastructure/long-job-notifier.service';
import { GestionePresenze_JobType } from '../../../Utility/GestionePresenze/GestionePresenze_JobType';
import { NvxTextPopupService } from '../../../shared/components/infrastructure/nvx-text-popup/nvx-text-popup.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ReportFabService } from '../../../Utility/GestionePresenze/report-fab.service';
import { ReportTipo } from '../../../ClientServer-Service/GestionePresenze/Report/Models/report-model';

/** giornate di un dipendente (vista mobile) */
interface GruppoDipendente {
  id: number;
  nome: string;
  daSistemare: number;
  daVerificare: number;
  giornate: AnomaliaGiornoModel[];
}

/** fino a questo numero di dipendenti i gruppi della vista mobile nascono aperti */
const GRUPPI_APERTI = 3;

/*
 Anomalie presenze (ANALISI_ATTENDANCETRACKING_ANOMALIE_AI.md, AT-AI.1-3): giornate da sistemare prima dell'export paghe
 e da verificare nel mese, ritardi ricorrenti, spiegazione con l'AI (o senza AI) e previsione delle assenze.
 Le regole sono tutte sul server; il CompanyAdmin vede solo i reparti che amministra (lo decide il server).
*/
@Component({
  selector: 'app-anomalie-page',
  templateUrl: './anomalie-page.component.html',
  styleUrls: ['./anomalie-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class AnomaliePageComponent implements OnDestroy {

  public title: string;
  public sezione: 'anomalie' | 'previsione' = 'anomalie';
  public currYear: number;
  public currMonth: number;
  public usersId: string[] = [];
  public soloDaSistemare = false;

  public risultato: AnomalieRisultato | null = null;
  public spiegazione: string | null = null;
  public spiegazioneAI = false;
  public spiegazioneInCorso = false;
  public previsione: PrevisioneRisultato | null = null;

  public Gravita = AnomaliaGravita;
  private jobSub: Subscription | null = null;

  constructor(private navCtrl: NavController,
              private dipAnomalieService: DipAnomalieService,
              private longJobNotifier: LongJobNotifierService,
              private fabMenuService: FabMenuService,
              private reportFabService: ReportFabService,
              private nvxTextPopupService: NvxTextPopupService,
              public ui: UserInterfaceService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('attendance.pages.anomalie.title');
  }

  ionViewWillEnter() {
    // a fine ricalcolo le anomalie cambiano
    this.jobSub = this.longJobNotifier.jobFinished$.subscribe(job => {
      if (job.jobType === GestionePresenze_JobType.TimeSheet_Engine_Calculate)
        this.load();
    });

    // PDF delle giornate da sistemare (job)
    this.fabMenuService.fabMenuItem = [
      this.reportFabService.FabItem([
        { tipo: ReportTipo.GiornateDaSistemare, testo: 'attendance.services.report.giornateDaSistemare' },
      ], () => ({ year: this.currYear, month: this.currMonth, selectedUserId: this.usersId, repartiId: [], idPar_TipoSpesa: null, stati: null })),
    ];
  }

  ionViewWillLeave() {
    this.jobSub?.unsubscribe();
    this.jobSub = null;
    this.fabMenuService.fabMenuItem = [];
  }

  ngOnDestroy() {
    this.jobSub?.unsubscribe();
  }

  onPeriodChange(period: { year: number, month: number } | undefined) {
    if (!period) return;
    this.currYear = period.year;
    this.currMonth = period.month;
    this.load();
  }

  onAllUsersInSelectionChanged(userIds: string[] | undefined) {
    this.usersId = userIds ?? [];
    this.load();
    this.loadPrevisione();
  }

  onSoloDaSistemare(valore: boolean) {
    this.soloDaSistemare = valore;
    this.load();
  }

  private richiesta(): GenericRequest<Dip_Anomalie_GetInModel> | null {
    if (!this.currYear || !this.currMonth || this.usersId.length === 0)
      return null;
    const request = new GenericRequest<Dip_Anomalie_GetInModel>(Dip_Anomalie_GetInModel);
    const ultimo = new Date(this.currYear, this.currMonth, 0).getDate();
    request.data.dal = `${this.currYear}-${String(this.currMonth).padStart(2, '0')}-01`;
    request.data.al = `${this.currYear}-${String(this.currMonth).padStart(2, '0')}-${String(ultimo).padStart(2, '0')}`;
    request.data.usersId = this.usersId;
    request.data.soloDaSistemare = this.soloDaSistemare;
    return request;
  }

  // ── vista mobile ──────────────────────────────────────────────────────

  /** dipendenti aperti (idDip_Anagrafica) */
  public aperti = new Set<number>();
  private gruppiDi: AnomalieRisultato | null = null;
  private gruppiCalcolati: GruppoDipendente[] = [];

  /** giornate raggruppate per dipendente, nell'ordine del server; ricalcolate solo quando cambia il risultato */
  gruppi(): GruppoDipendente[] {
    if (this.gruppiDi === this.risultato)
      return this.gruppiCalcolati;
    this.gruppiDi = this.risultato;

    const gruppi = new Map<number, GruppoDipendente>();
    for (const g of this.risultato?.giornate ?? []) {
      let d = gruppi.get(g.idDip_Anagrafica);
      if (!d)
        gruppi.set(g.idDip_Anagrafica, d = { id: g.idDip_Anagrafica, nome: g.nomeDipendente, daSistemare: 0, daVerificare: 0, giornate: [] });
      d.giornate.push(g);
      if (g.daSistemare) d.daSistemare++; else d.daVerificare++;
    }
    this.gruppiCalcolati = [...gruppi.values()];
    this.gruppiCalcolati.forEach(d => d.giornate.sort((a, b) => a.data.localeCompare(b.data)));

    // pochi dipendenti: aperti; altrimenti restano aperti quelli gia' aperti (es. dopo un ricalcolo)
    if (this.gruppiCalcolati.length <= GRUPPI_APERTI)
      this.aperti = new Set(this.gruppiCalcolati.map(d => d.id));
    return this.gruppiCalcolati;
  }

  apriChiudi(id: number) {
    if (!this.aperti.delete(id))
      this.aperti.add(id);
  }

  apriRegole() {
    this.popup(this.transloco.translate('attendance.pages.anomalie.regole'), this.transloco.translate('attendance.pages.anomalie.notaRegole'));
  }

  apriPianificati(g: PrevisioneGiornoModel) {
    this.popup(this.transloco.translate('attendance.pages.anomalie.colPianificate') + ' ' + this.formatGiorno(g.data), g.pianificati.join('\n'));
  }

  private popup(title: string, text: string) {
    const btnChiudi = this.ui.Btn_Chiudi;
    btnChiudi.event = () => { };
    this.nvxTextPopupService.show({ title, text, buttons: [btnChiudi] });
  }

  load() {
    this.spiegazione = null;
    const request = this.richiesta();
    if (!request) {
      this.risultato = null;
      return;
    }
    this.dipAnomalieService.AnomalieGet(request).subscribe(res => {
      this.risultato = res.success && res.data ? res.data.risultato : null;
    });
  }

  spiega() {
    const request = this.richiesta();
    if (!request) return;
    this.spiegazioneInCorso = true;
    this.dipAnomalieService.AnomalieSpiega(request).subscribe({
      next: res => {
        this.spiegazione = res.success && res.data ? res.data.testo : null;
        this.spiegazioneAI = !!res.data?.generatoConAI;
        this.spiegazioneInCorso = false;
      },
      error: () => this.spiegazioneInCorso = false
    });
  }

  loadPrevisione() {
    if (this.usersId.length === 0) {
      this.previsione = null;
      return;
    }
    const request = new GenericRequest<Dip_PrevisioneAssenze_GetInModel>(Dip_PrevisioneAssenze_GetInModel);
    request.data.usersId = this.usersId;
    request.data.giorni = 30;
    this.dipAnomalieService.PrevisioneAssenzeGet(request).subscribe(res => {
      this.previsione = res.success && res.data ? res.data.risultato : null;
    });
  }

  apriCalendario(g: AnomaliaGiornoModel) {
    const data = new Date(g.data);
    this.navCtrl.navigateForward('/poweradmintimesheet', {
      state: { userId: g.idAspNetUsers, year: data.getFullYear(), month: data.getMonth() + 1 }
    });
  }

  /** nome del giorno della settimana (1=lun ... 7=dom) nella lingua attiva */
  giornoSettimana(n: number): string {
    // 2024-01-01 era un lunedi'
    return new Date(2024, 0, n).toLocaleDateString(this.transloco.getActiveLang(), { weekday: 'long' });
  }

  nomeMese(n: number): string {
    return new Date(2024, n - 1, 1).toLocaleDateString(this.transloco.getActiveLang(), { month: 'long' });
  }

  /** giorno (dd/MM) di una data del server */
  public formatGiorno = (d: string): string => `${d.substring(8, 10)}/${d.substring(5, 7)}`;
}
