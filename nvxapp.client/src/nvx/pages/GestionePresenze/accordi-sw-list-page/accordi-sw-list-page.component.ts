import { Component, ChangeDetectionStrategy } from '@angular/core';
import { Subscription } from 'rxjs';
import { AcknowledgementNotifierService } from '../../../Utility/infrastructure/acknowledgement-notifier.service';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { SmartWorkingService } from '../../../ClientServer-Service/GestionePresenze/SmartWorking/smart-working.service';
import { Dip_AccordoSWModel, Dip_AccordoSW_DeleteInModel, Dip_AccordoSW_GetAllInModel } from '../../../ClientServer-Service/GestionePresenze/SmartWorking/Models/smart-working-model';

// Accordi individuali di lavoro agile (AT-SW.1): elenco con stato della firma/presa visione; si ricarica da solo quando un
// dipendente conferma l'accordo (avviso push "AcknowledgementNotifier", fonte at.accordo-sw)
@Component({
  selector: 'app-accordi-sw-list-page',
  templateUrl: './accordi-sw-list-page.component.html',
  styleUrls: ['./accordi-sw-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class AccordiSwListPageComponent {

  public title: string;
  public searchText!: string;
  public lista: Dip_AccordoSWModel[] | null = null;
  public readonly columns: NvxListColumn<Dip_AccordoSWModel>[];
  public actions: NvxListAction<Dip_AccordoSWModel>[] = [];
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;
  /** fonte delle prese visione degli accordi (Dip_AccordoSWService) */
  private static readonly FONTE = 'at.accordo-sw';
  private presaVisioneSub: Subscription | null = null;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private smartWorkingService: SmartWorkingService,
              private userInterfaceService: UserInterfaceService,
              private collectionDialogService: CollectionDialogService,
              private acknowledgementNotifierService: AcknowledgementNotifierService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('attendance.pages.accordiSw.title');
    const data = (s: string) => s ? `${s.substring(8, 10)}/${s.substring(5, 7)}/${s.substring(0, 4)}` : '';
    this.columns = [
      { key: 'nominativo', title: this.transloco.translate('attendance.pages.accordiSw.employee') },
      { key: 'periodo', title: this.transloco.translate('attendance.pages.accordiSw.period'), value: x => `${data(x.dal)} - ${data(x.al)}`, sortValue: x => x.dal },
      { key: 'maxGiorniSettimana', title: this.transloco.translate('attendance.pages.accordiSw.maxDays') },
      {
        key: 'firmato', title: this.transloco.translate('attendance.pages.accordiSw.signature'),
        value: x => x.firmato ? this.transloco.translate('attendance.pages.accordiSw.signed', { date: data(x.dataFirma ?? '') }) : this.transloco.translate('attendance.pages.accordiSw.waiting')
      },
    ];
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = (item: Dip_AccordoSWModel) => this.navCtrl.navigateForward('/accordoswedit', { state: { id: item.id } });
    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.elimina;
    this.actions = [{ button: this.btnDelete }, { button: this.btnEdit }];
  }

  ionViewWillEnter() {
    this.carica();
    // accordo confermato (o firmato) dal dipendente: lo stato della firma si aggiorna senza ricaricare la pagina
    this.presaVisioneSub?.unsubscribe();
    this.presaVisioneSub = this.acknowledgementNotifierService.changed$.subscribe(d => {
      if (d.fonte === AccordiSwListPageComponent.FONTE)
        this.carica();
    });
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('attendance.pages.accordiSw.add'), 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/accordoswedit', { state: { id: 0 } });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
    this.presaVisioneSub?.unsubscribe();
    this.presaVisioneSub = null;
  }

  private carica() {
    this.smartWorkingService.GetAll(new GenericRequest<Dip_AccordoSW_GetAllInModel>(Dip_AccordoSW_GetAllInModel))
      .subscribe(res => this.lista = res.data?.dip_AccordoSW ?? []);
  }

  elimina = async (item: Dip_AccordoSWModel) => {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.accordiSw.deleteConfirm')))
      return;
    const request = new GenericRequest<Dip_AccordoSW_DeleteInModel>(Dip_AccordoSW_DeleteInModel);
    request.data.id = item.id;
    this.smartWorkingService.Dip_AccordoSWDelete(request).subscribe(() => this.carica());
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
