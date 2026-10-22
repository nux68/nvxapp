import { Component, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { NavController, Platform } from '@ionic/angular';
import { Subscription } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DipPeriodoService } from '../../../ClientServer-Service/GestionePresenze/Dip_Periodo/dip-periodo.service';
import { Dip_Periodo_GetAll_InModel, Dip_Periodo_Riga, Dip_Periodo_SetStato_InModel, Dip_PeriodoModel, minutiToHHMM, PeriodoStato } from '../../../ClientServer-Service/GestionePresenze/Dip_Periodo/Models/dip-periodo-model';
import { DestinazioneEccedenze, TipoContratto } from '../../../ClientServer-Service/GestionePresenze/Dip_RapportoLavoro/Models/dip-rapporto-lavoro-model';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { LongJobNotifierService } from '../../../Utility/infrastructure/long-job-notifier.service';
import { GestionePresenze_JobType } from '../../../Utility/GestionePresenze/GestionePresenze_JobType';
import { TranslocoService } from '@jsverse/transloco';

/*
 Quadratura mensile: per i dipendenti selezionati, totali del cartellino del mese (ore in piu',
 supplementare, straordinario, banca ore) e stato; approvazione di piu' cartellini insieme.
 Il dettaglio di un dipendente si apre nel Calendario HR.
*/
@Component({
  selector: 'app-quadratura-mensile-page',
  templateUrl: './quadratura-mensile-page.component.html',
  styleUrls: ['./quadratura-mensile-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class QuadraturaMensilePageComponent implements OnDestroy {

  public title: string;
  public currYear: number;
  public currMonth: number;     // 1-12
  public usersId: string[] = [];
  public righe: Dip_Periodo_Riga[] = [];
  public selezionati = new Set<number>();

  public PeriodoStato = PeriodoStato;
  public hhmm = minutiToHHMM;

  private jobSub: Subscription | null = null;

  constructor(private navCtrl: NavController,
              private dipPeriodoService: DipPeriodoService,
              private collectionDialogService: CollectionDialogService,
              private longJobNotifier: LongJobNotifierService,
              private platform: Platform,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('attendance.pages.quadraturaMensile.title');
  }

  public get isMobile(): boolean {
    return this.platform.width() < 576;
  }

  ionViewWillEnter() {
    // a fine ricalcolo i totali cambiano
    this.jobSub = this.longJobNotifier.jobFinished$.subscribe(job => {
      if (job.jobType === GestionePresenze_JobType.TimeSheet_Engine_Calculate)
        this.load();
    });
  }

  ionViewWillLeave() {
    this.jobSub?.unsubscribe();
    this.jobSub = null;
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
  }

  load() {
    if (!this.currYear || !this.currMonth || this.usersId.length === 0) {
      this.righe = [];
      return;
    }
    const request = new GenericRequest<Dip_Periodo_GetAll_InModel>(Dip_Periodo_GetAll_InModel);
    request.data.anno = this.currYear;
    request.data.mese = this.currMonth;
    request.data.usersId = this.usersId;
    this.dipPeriodoService.Dip_Periodo_GetAll(request).subscribe(res => {
      this.righe = res.data.righe;
      this.selezionati.clear();
    });
  }

  nome(r: Dip_Periodo_Riga): string {
    const nome = [r.cognome, r.nome].filter(x => !!x).join(' ');
    return nome || r.userName || r.idAspNetUsers;
  }

  contratto(r: Dip_Periodo_Riga): string {
    return (r.tipoContratto === TipoContratto.PartTime ? this.transloco.translate('attendance.pages.quadraturaMensile.partTime') : this.transloco.translate('attendance.pages.quadraturaMensile.fullTime')) +
           (r.destinazioneEccedenze === DestinazioneEccedenze.BancaOre ? ' · ' + this.transloco.translate('attendance.pages.quadraturaMensile.bancaOre') : '');
  }

  statoTesto(p: Dip_PeriodoModel | null): string {
    if (!p) return this.transloco.translate('attendance.pages.quadraturaMensile.statoNonCalcolato');
    switch (p.stato) {
      case PeriodoStato.Approvato: return this.transloco.translate('attendance.pages.quadraturaMensile.statoApprovato');
      case PeriodoStato.DaApprovare: return this.transloco.translate('attendance.pages.quadraturaMensile.statoDaApprovare');
      default: return this.transloco.translate('attendance.pages.quadraturaMensile.statoProvvisorio');
    }
  }

  statoColore(p: Dip_PeriodoModel | null): string {
    switch (p?.stato) {
      case PeriodoStato.Approvato: return 'success';
      case PeriodoStato.DaApprovare: return 'warning';
      default: return 'medium';
    }
  }

  supplementare(p: Dip_PeriodoModel | null): number {
    return p ? p.min_Suppl + p.min_SupplEcc : 0;
  }

  straordinario(p: Dip_PeriodoModel | null): number {
    return p ? p.min_Strao + p.min_StraoNott + p.min_StraoFest + p.min_StraoFestNott : 0;
  }

  // ── selezione e approvazione ──────────────────────────────────────────

  selezionabile(r: Dip_Periodo_Riga): boolean {
    return r.dip_Periodo?.stato === PeriodoStato.DaApprovare;
  }

  toggle(r: Dip_Periodo_Riga) {
    const id = r.dip_Periodo?.id;
    if (!id || !this.selezionabile(r)) return;
    this.selezionati.has(id) ? this.selezionati.delete(id) : this.selezionati.add(id);
  }

  selezionaTutti() {
    const ids = this.righe.filter(r => this.selezionabile(r)).map(r => r.dip_Periodo!.id);
    const tutti = ids.length > 0 && ids.every(id => this.selezionati.has(id));
    this.selezionati.clear();
    if (!tutti) ids.forEach(id => this.selezionati.add(id));
  }

  async approvaSelezionati() {
    if (this.selezionati.size === 0) return;
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.quadraturaMensile.confirmApprova', { n: this.selezionati.size })))
      return;

    const request = new GenericRequest<Dip_Periodo_SetStato_InModel>(Dip_Periodo_SetStato_InModel);
    request.data.id = [...this.selezionati];
    request.data.stato = PeriodoStato.Approvato;
    this.dipPeriodoService.Dip_Periodo_SetStato(request).subscribe(() => this.load());
  }

  apriCalendario(r: Dip_Periodo_Riga) {
    this.navCtrl.navigateForward('/poweradmintimesheet', {
      state: { userId: r.idAspNetUsers, year: this.currYear, month: this.currMonth }
    });
  }
}
