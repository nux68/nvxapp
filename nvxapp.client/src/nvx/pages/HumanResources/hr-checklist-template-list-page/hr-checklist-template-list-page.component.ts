import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { HrService } from '../../../ClientServer-Service/HumanResources/hr.service';
import * as M from '../../../ClientServer-Service/HumanResources/Models/hr-model';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';

@Component({
  selector: 'app-hr-checklist-template-list-page',
  templateUrl: './hr-checklist-template-list-page.component.html',
  styleUrls: ['./hr-checklist-template-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Modelli delle checklist di assunzione e cessazione (HR-ONB.1, CompanyPowerAdmin): il predefinito di ogni tipo genera le checklist
export class HrChecklistTemplateListPageComponent {

  public title: string;
  public searchText!: string;
  public items: M.HrModelloModel[] | null = null;
  public showDisabled = false;
  public readonly columns: NvxListColumn<M.HrModelloModel>[];
  public readonly actions: NvxListAction<M.HrModelloModel>[];
  public readonly isDisabled = (x: M.HrModelloModel) => !x.attivo;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: HrService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.title = t('hr.pages.templateList.title');
    this.columns = [
      { key: 'descrizione', title: t('hr.fields.descrizione'), mobile: 'title', value: x => x.descrizione + (x.predefinito ? ' (' + t('hr.fields.predefinito').toLowerCase() + ')' : '') },
      { key: 'tipo', title: t('hr.fields.tipoChecklist'), mobile: 'subtitle', value: x => t('hr.enums.tipoChecklist.' + x.tipo) },
      { key: 'numeroVoci', title: t('hr.fields.voci'), align: 'end', mobile: 'note', value: x => `${x.numeroVoci}` },
    ];
    const bEdit = userInterfaceService.Btn_Modifica;
    bEdit.event = (x: M.HrModelloModel) => this.navCtrl.navigateForward('/hrchecklisttemplateedit', { state: { id: x.id } });
    const bDel = userInterfaceService.Btn_Cancella;
    bDel.event = (x: M.HrModelloModel) => this.elimina(x);
    this.actions = [{ button: bDel }, { button: bEdit }];
  }

  ionViewWillEnter() {
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('hr.pages.templateList.new'), 'add-circle-outline', () => this.navCtrl.navigateForward('/hrchecklisttemplateedit', { state: { id: 0 } })),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    this.service.ModelloList(new GenericRequest<M.HrEmptyInModel>(M.HrEmptyInModel)).subscribe(res => this.items = res.success && res.data ? res.data.modelli : []);
  }

  private async elimina(x: M.HrModelloModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('hr.pages.templateList.confirmDelete', { nome: x.descrizione })))
      return;
    const request = new GenericRequest<M.HrIdInModel>(M.HrIdInModel);
    request.data.id = x.id;
    this.service.ModelloDelete(request).subscribe(res => { if (res.success) this.load(); });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
