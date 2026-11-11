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
  selector: 'app-hr-course-list-page',
  templateUrl: './hr-course-list-page.component.html',
  styleUrls: ['./hr-course-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Catalogo dei corsi (HR-FOR.1): validita' dell'attestato, competenza comune collegata, edizioni
export class HrCourseListPageComponent {

  public title: string;
  public searchText!: string;
  public items: M.HrCorsoModel[] | null = null;
  public showDisabled = false;
  public readonly columns: NvxListColumn<M.HrCorsoModel>[];
  public readonly actions: NvxListAction<M.HrCorsoModel>[];
  public readonly isDisabled = (x: M.HrCorsoModel) => !x.attivo;
  public readonly sort = (a: M.HrCorsoModel, b: M.HrCorsoModel) => a.titolo.localeCompare(b.titolo);

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: HrService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.title = t('hr.pages.courseList.title');
    this.columns = [
      { key: 'titolo', title: t('hr.fields.corso'), mobile: 'title', value: x => x.titolo + (x.obbligatorio ? ' *' : '') },
      { key: 'ente', title: t('hr.fields.ente'), mobile: 'subtitle', value: x => x.ente ?? '' },
      { key: 'competenza', title: t('hr.fields.competenza'), mobile: 'hidden', value: x => x.competenza ?? '' },
      { key: 'validitaMesi', title: t('hr.fields.validitaMesi'), align: 'end', mobile: 'hidden', value: x => x.validitaMesi ? `${x.validitaMesi}` : t('hr.pages.courseList.noExpiry') },
      { key: 'numeroEdizioni', title: t('hr.fields.edizioni'), align: 'end', mobile: 'note', value: x => `${x.numeroEdizioni}` },
    ];
    const bEdit = userInterfaceService.Btn_Modifica;
    bEdit.event = (x: M.HrCorsoModel) => this.navCtrl.navigateForward('/hrcourseedit', { state: { id: x.id } });
    const bDel = userInterfaceService.Btn_Cancella;
    bDel.event = (x: M.HrCorsoModel) => this.elimina(x);
    this.actions = [{ button: bDel, visible: x => x.numeroEdizioni === 0 }, { button: bEdit }];
  }

  ionViewWillEnter() {
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('hr.pages.courseList.new'), 'add-circle-outline', () => this.navCtrl.navigateForward('/hrcourseedit', { state: { id: 0 } })),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    this.service.CorsoList(new GenericRequest<M.HrEmptyInModel>(M.HrEmptyInModel)).subscribe(res => this.items = res.success && res.data ? res.data.corsi : []);
  }

  private async elimina(x: M.HrCorsoModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('hr.pages.courseList.confirmDelete', { nome: x.titolo })))
      return;
    const request = new GenericRequest<M.HrIdInModel>(M.HrIdInModel);
    request.data.id = x.id;
    this.service.CorsoDelete(request).subscribe(res => { if (res.success) this.load(); });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
