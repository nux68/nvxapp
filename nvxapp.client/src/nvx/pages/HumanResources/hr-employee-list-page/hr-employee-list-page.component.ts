import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { HrService, hrData } from '../../../ClientServer-Service/HumanResources/hr.service';
import * as M from '../../../ClientServer-Service/HumanResources/Models/hr-model';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

@Component({
  selector: 'app-hr-employee-list-page',
  templateUrl: './hr-employee-list-page.component.html',
  styleUrls: ['./hr-employee-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Dipendenti visti da Risorse Umane (anagrafica comune): codice fiscale e matricola per i cedolini, date per le checklist, account
export class HrEmployeeListPageComponent {

  public title: string;
  public searchText!: string;
  public items: M.HrDipendenteModel[] | null = null;
  public showDisabled = false;
  public readonly columns: NvxListColumn<M.HrDipendenteModel>[];
  public readonly actions: NvxListAction<M.HrDipendenteModel>[];
  public readonly isDisabled = (x: M.HrDipendenteModel) => !x.attivo;
  public readonly sort = (a: M.HrDipendenteModel, b: M.HrDipendenteModel) => ((a.cognome ?? '') + (a.nome ?? '')).localeCompare((b.cognome ?? '') + (b.nome ?? ''));

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: HrService,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.title = t('hr.pages.employeeList.title');
    this.columns = [
      { key: 'nominativo', title: t('hr.fields.dipendente'), mobile: 'title', value: x => `${x.cognome ?? ''} ${x.nome ?? ''}`.trim() },
      { key: 'codiceFiscale', title: t('hr.fields.codiceFiscale'), mobile: 'subtitle', value: x => x.codiceFiscale ?? '' },
      { key: 'matricola', title: t('hr.fields.matricola'), mobile: 'hidden', value: x => x.matricola ?? '' },
      { key: 'dataAssunzione', title: t('hr.fields.dataAssunzione'), mobile: 'hidden', value: x => hrData(x.dataAssunzione), sortValue: x => x.dataAssunzione ?? '' },
      { key: 'dataCessazione', title: t('hr.fields.dataCessazione'), mobile: 'hidden', value: x => hrData(x.dataCessazione), sortValue: x => x.dataCessazione ?? '' },
      { key: 'account', title: t('hr.fields.account'), mobile: 'note',
        value: x => x.account.presente ? (x.account.userName ?? '') + (x.account.disabilitato ? ' (' + t('hr.pages.employeeEdit.accountDisabled') + ')' : '') : t('hr.pages.employeeEdit.noAccount') },
    ];
    const bEdit = userInterfaceService.Btn_Modifica;
    bEdit.event = (x: M.HrDipendenteModel) => this.navCtrl.navigateForward('/hremployeeedit', { state: { id: x.id } });
    this.actions = [{ button: bEdit }];
  }

  ionViewWillEnter() {
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('hr.pages.employeeList.new'), 'add-circle-outline', () => this.navCtrl.navigateForward('/hremployeeedit', { state: { id: 0 } })),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    const request = new GenericRequest<M.HrDipendenteListInModel>(M.HrDipendenteListInModel);
    this.service.DipendenteList(request).subscribe(res => this.items = res.success && res.data ? res.data.dipendenti : []);
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
