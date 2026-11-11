import { Component, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { Subscription } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DipAnomalieService } from '../../../ClientServer-Service/GestionePresenze/Dip_Anomalie/dip-anomalie.service';
import {
  AnomaliaGiornoModel, AnomaliaGravita, AnomalieRisultato, Dip_Anomalie_GetInModel, Dip_PrevisioneAssenze_GetInModel, PrevisioneRisultato
} from '../../../ClientServer-Service/GestionePresenze/Dip_Anomalie/Models/dip-anomalie-model';
import { LongJobNotifierService } from '../../../Utility/infrastructure/long-job-notifier.service';
import { GestionePresenze_JobType } from '../../../Utility/GestionePresenze/GestionePresenze_JobType';
import { FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ReportFabService } from '../../../Utility/GestionePresenze/report-fab.service';
import { ReportTipo } from '../../../ClientServer-Service/GestionePresenze/Report/Models/report-model';

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
