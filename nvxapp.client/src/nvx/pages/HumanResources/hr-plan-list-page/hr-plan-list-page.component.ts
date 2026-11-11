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
  selector: 'app-hr-plan-list-page',
  templateUrl: './hr-plan-list-page.component.html',
  styleUrls: ['./hr-plan-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Piani di formazione per anno (HR-FOR.1), con l'avanzamento (corsi superati nell'anno)
export class HrPlanListPageComponent {

  public title: string;
  public searchText!: string;
  public items: M.HrPianoModel[] | null = null;
  public readonly columns: NvxListColumn<M.HrPianoModel>[];
  public readonly actions: NvxListAction<M.HrPianoModel>[];

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: HrService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.title = t('hr.pages.planList.title');
    this.columns = [
      { key: 'anno', title: t('hr.fields.anno'), mobile: 'note' },
      { key: 'descrizione', title: t('hr.fields.descrizione'), mobile: 'title' },
      { key: 'avanzamento', title: t('hr.fields.avanzamento'), align: 'end', mobile: 'subtitle', value: x => `${x.numeroSvolte}/${x.numeroRighe}`, sortValue: x => x.numeroRighe ? x.numeroSvolte / x.numeroRighe : 0 },
    ];
    const bEdit = userInterfaceService.Btn_Modifica;
    bEdit.event = (x: M.HrPianoModel) => this.navCtrl.navigateForward('/hrplanedit', { state: { id: x.id } });
    const bDel = userInterfaceService.Btn_Cancella;
    bDel.event = (x: M.HrPianoModel) => this.elimina(x);
    this.actions = [{ button: bDel }, { button: bEdit }];
  }

  ionViewWillEnter() {
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('hr.pages.planList.new'), 'add-circle-outline', () => this.navCtrl.navigateForward('/hrplanedit', { state: { id: 0 } })),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    this.service.PianoList(new GenericRequest<M.HrPianoListInModel>(M.HrPianoListInModel)).subscribe(res => this.items = res.success && res.data ? res.data.piani : []);
  }

  private async elimina(x: M.HrPianoModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('hr.pages.planList.confirmDelete', { nome: x.descrizione })))
      return;
    const request = new GenericRequest<M.HrIdInModel>(M.HrIdInModel);
    request.data.id = x.id;
    this.service.PianoDelete(request).subscribe(res => { if (res.success) this.load(); });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
