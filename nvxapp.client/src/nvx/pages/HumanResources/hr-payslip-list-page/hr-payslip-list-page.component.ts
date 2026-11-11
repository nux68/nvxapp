import { Component, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Subscription, filter } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { HrService, hrData } from '../../../ClientServer-Service/HumanResources/hr.service';
import * as M from '../../../ClientServer-Service/HumanResources/Models/hr-model';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { LongJobNotifierService } from '../../../Utility/infrastructure/long-job-notifier.service';

// periodo del documento: "Ottobre 2026", "Tredicesima 2026", "CU 2026"
export function hrPeriodo(transloco: TranslocoService, tipo: number, anno: number, mese: number | null): string {
  return tipo === M.HrTipoDocumento.Cu ? transloco.translate('hr.enums.tipoDocumento.2') + ' ' + anno
                                       : transloco.translate('hr.enums.mese.' + (mese ?? 1)) + ' ' + anno;
}

@Component({
  selector: 'app-hr-payslip-list-page',
  templateUrl: './hr-payslip-list-page.component.html',
  styleUrls: ['./hr-payslip-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Caricamenti dei cedolini e delle CU (HR-CED): per l'ufficio del personale (azienda del token) e, nelle pagine lato studio
 (/hrstudiopayslips), per lo studio paghe sull'azienda scelta. Si ricarica alla fine dei job di divisione e pubblicazione.
*/
export class HrPayslipListPageComponent {

  public title: string;
  public searchText!: string;
  public items: M.HrCaricamentoModel[] | null = null;
  public studio = false;
  public aziende: M.HrStudioAziendaModel[] = [];
  public idCompany: number | null = null;
  public readonly columns: NvxListColumn<M.HrCaricamentoModel>[];
  public readonly actions: NvxListAction<M.HrCaricamentoModel>[];
  public readonly evidenzia = (x: M.HrCaricamentoModel) => x.stato === M.HrStatoCaricamento.DaVerificare;
  private jobs?: Subscription;

  constructor(private navCtrl: NavController,
              private router: Router,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: HrService,
              private longJobNotifierService: LongJobNotifierService,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.title = t('hr.pages.payslipList.title');
    this.columns = [
      { key: 'periodo', title: t('hr.fields.periodo'), mobile: 'title', value: x => hrPeriodo(this.transloco, x.tipo, x.anno, x.mese), sortValue: x => x.anno * 100 + (x.mese ?? 0) },
      { key: 'stato', title: t('hr.fields.stato'), mobile: 'subtitle', value: x => t('hr.enums.statoCaricamento.' + x.stato) },
      { key: 'nomeFile', title: t('hr.fields.file'), mobile: 'hidden' },
      { key: 'numeroDocumenti', title: t('hr.fields.documenti'), align: 'end', mobile: 'note', value: x => `${x.numeroDocumenti}` },
      { key: 'numeroScarti', title: t('hr.fields.daVerificare'), align: 'end', mobile: 'hidden' },
      { key: 'numeroPubblicati', title: t('hr.fields.pubblicati'), align: 'end', mobile: 'hidden' },
      { key: 'origine', title: t('hr.fields.origine'), mobile: 'hidden', value: x => t('hr.enums.origine.' + x.origine) },
      { key: 'dataCaricamento', title: t('hr.fields.dataCaricamento'), mobile: 'hidden', value: x => hrData(x.dataCaricamento), sortValue: x => x.dataCaricamento ?? '' },
    ];
    const bOpen = userInterfaceService.Btn_Apri;
    bOpen.event = (x: M.HrCaricamentoModel) => this.navCtrl.navigateForward(this.studio ? '/hrstudiopayslipdetail' : '/hrpayslipdetail', { state: { id: x.id, idCompany: this.idCompany } });
    this.actions = [{ button: bOpen }];
  }

  ionViewWillEnter() {
    this.studio = this.router.url.startsWith('/hrstudio');
    if (this.studio) {
      this.title = this.transloco.translate('hr.pages.payslipList.studioTitle');
      this.service.StudioAziende(new GenericRequest<M.HrEmptyInModel>(M.HrEmptyInModel)).subscribe(res => {
        this.aziende = res.success ? res.data.aziende : [];
        if (!this.aziende.some(a => a.idCompany === this.idCompany))
          this.idCompany = this.aziende.length > 0 ? this.aziende[0].idCompany : null;
        this.load();
      });
    }
    else
      this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('hr.pages.payslipList.upload'), 'cloud-upload-outline', () => {
        if (!this.studio || this.idCompany)
          this.navCtrl.navigateForward(this.studio ? '/hrstudiopayslipupload' : '/hrpayslipupload', { state: { idCompany: this.idCompany } });
      }),
    ];
    this.jobs = this.longJobNotifierService.jobFinished$.pipe(filter(j => j.jobType?.startsWith('Hr_Cedolini'))).subscribe(() => this.load());
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
    this.jobs?.unsubscribe();
  }

  load() {
    if (this.studio && !this.idCompany) {
      this.items = [];
      return;
    }
    const request = new GenericRequest<M.HrCaricamentoListInModel>(M.HrCaricamentoListInModel);
    request.data.idCompany = this.studio ? this.idCompany : null;
    this.service.CaricamentoList(request).subscribe(res => this.items = res.success && res.data ? res.data.caricamenti : []);
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
