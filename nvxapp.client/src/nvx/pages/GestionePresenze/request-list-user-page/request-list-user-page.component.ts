import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_GiustificativiInModel, Par_GiustificativiModel } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParGiustificativiService } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/par-giustificativi.service';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { Dip_GG_Richiesta_GetAll4User_InModel, Dip_GG_Richiesta_SetState_InModel, Dip_GG_RichiestaModel, StatoRichiesta, TipoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { MonthNavigatorService } from '../../../Utility/infrastructure/month-navigator.service';
import { TimeSheetUtilityService } from '../../../Utility/GestionePresenze/time-sheet-utility.service';
import { StatoRichiestaShortTextPipe } from '../../../shared/pipe/GestionePresenze/stato-richiesta-short-text.pipe';
import { TipoRichiestaToShortTextPipe } from '../../../shared/pipe/GestionePresenze/tipo-richiesta-to-short-text.pipe';
import { TimeSheetService } from '../../../Utility/GestionePresenze/time-sheet.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { TipoRichiestaToLongTextPipe } from '../../../shared/pipe/GestionePresenze/tipo-richiesta-to-long-text.pipe';
import { TranslocoService } from '@jsverse/transloco';
import { Subscription } from 'rxjs';
import { RichiestaNotifierService } from '../../../Utility/GestionePresenze/richiesta-notifier.service';
import { NotaSpesaDettaglioDialogService } from '../../../shared/components/GestionePresenze/nota-spesa-dettaglio-dialog/nota-spesa-dettaglio-dialog.service';



@Component({
  selector: 'app-request-list-user-page',
  templateUrl: './request-list-user-page.component.html',
  styleUrls: ['./request-list-user-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone:false
}) 
export class RequestListUserPageComponent {

  public title!: string;
  public searchText!: string;
  public dip_GG_RichiestaList: Dip_GG_RichiestaModel[] | null = null;
  public readonly columns: NvxListColumn<Dip_GG_RichiestaModel>[] = [
    { key: 'data', title: this.transloco.translate('attendance.pages.requestListUser.columnDal') },
    { key: 'dataA', title: this.transloco.translate('attendance.pages.requestListUser.columnAl'), value: x => this.getDataA(x) },
    { key: 'richiestaTipo', title: this.transloco.translate('attendance.pages.requestListUser.columnTipo'), value: x => this.tipoRichiestaToLongText.transform(x.richiestaTipo) },
    { key: 'dati', title: this.transloco.translate('attendance.pages.requestListUser.columnDettaglio'), value: x => this.getDati(x), sortable: false },
    { key: 'stato', title: this.transloco.translate('attendance.pages.requestListUser.columnStato'), value: x => this.timeSheetService.get_StatoRichiesta_Approval_text(x) },
  ];
  public actions: NvxListAction<Dip_GG_RichiestaModel>[] = [];
  private readonly tipoRichiestaToLongText = new TipoRichiestaToLongTextPipe();
  public btnDelete: ButtonItem;
  public btnDettaglioNotaSpese: ButtonItem;
  public StatoRichiesta = StatoRichiesta;
  // richieste cambiate da altri (notifica SignalR): la lista si ricarica
  private changedSub?: Subscription;

  constructor(private navCtrl: NavController,
    private dipGGRichiestaService: DipGGRichiestaService,
    public fabMenuService: FabMenuService,
    public timeSheetService: TimeSheetService,
    private monthNavigatorService: MonthNavigatorService,
    private userInterfaceService: UserInterfaceService,
    private collectionDialogService: CollectionDialogService,
    private dipGGTimbraturaUtilityService: TimeSheetUtilityService,
    private userNavigationService: UserNavigationService,
    private notaSpesaDettaglioDialogService: NotaSpesaDettaglioDialogService,
    private richiestaNotifierService: RichiestaNotifierService,
    private transloco: TranslocoService) {

    this.title = this.transloco.translate('attendance.pages.requestListUser.title');
    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;

    // nota spese: si apre il dettaglio (spese e allegati), come nel cartellino; il dipendente la vede soltanto
    this.btnDettaglioNotaSpese = userInterfaceService.Btn_Dettaglio;
    this.btnDettaglioNotaSpese.event = this.handleButtonDettaglioNotaSpeseClick;

    this.actions = [
      { button: this.btnDettaglioNotaSpese, visible: item => item.richiestaTipo === TipoRichiesta.NotaSpesa },
      { button: this.btnDelete, visible: item => this.timeSheetService.Dip_GG_Richiesta_User_Can_Delete(item) },
    ];
  }

  handleButtonDettaglioNotaSpeseClick = (item: Dip_GG_RichiestaModel) => {
    this.notaSpesaDettaglioDialogService.Apri(item.id);
  }

  ionViewWillEnter() {

    this.loadData();
    this.richiestaNotifierService.refreshCount();
    this.changedSub?.unsubscribe();
    this.changedSub = this.richiestaNotifierService.changed$.subscribe(() => this.loadData());
 

    //this.fabMenuService.fabMenuItem = [

    //  new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
    //    this.navCtrl.navigateForward('/justificationedit', {
    //      state: { id: 0 }
    //    });
    //  }),

    //];

  }

  private loadData() {
    let request: GenericRequest<Dip_GG_Richiesta_GetAll4User_InModel> = new GenericRequest<Dip_GG_Richiesta_GetAll4User_InModel>(Dip_GG_Richiesta_GetAll4User_InModel);
    request.data.month = this.monthNavigatorService.currentMonth + 1;
    request.data.year = this.monthNavigatorService.currentYear;

    this.dipGGRichiestaService.GetAll4User(request).subscribe(res => {
      this.dip_GG_RichiestaList = res.data.dip_GG_Richiesta;
    });
  }

  ionViewWillLeave() {
    //this.fabMenuService.fabMenuItem = [];
    this.changedSub?.unsubscribe();
  }



  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.requestListUser.confirmDelete'));
    if (result) {

        let IdDip_GG_Richiesta: number[] = [];
        IdDip_GG_Richiesta.push(item.id);

        let request: GenericRequest<Dip_GG_Richiesta_SetState_InModel> = new GenericRequest<Dip_GG_Richiesta_SetState_InModel>(Dip_GG_Richiesta_SetState_InModel);

        if (item.richiestaStato == this.StatoRichiesta.Approvata && (item.revocaStato == null || (item.revocaStato !== null && item.revocaStato == this.StatoRichiesta.Cancellata) )  )
          request.data.richiestaStato = StatoRichiesta.Immessa; //revoca
        else
          request.data.richiestaStato = StatoRichiesta.Cancellata;

        request.data.idDip_GG_Richiesta = IdDip_GG_Richiesta;
        this.dipGGRichiestaService.SetState(request).subscribe(res => {
          this.loadData();
        });

    }

  
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  isAdmin(item: any) {

    return false;
  }


  public getDataA(item:Dip_GG_RichiestaModel): string{
    if (item.data != item.dataA)
      return item.dataA;

    return '';

  }

  public getDati(item: Dip_GG_RichiestaModel): string {

    return this.dipGGTimbraturaUtilityService.getDatitext(item);

  }



  public getItemText1(item: Dip_GG_RichiestaModel): string {
    let retVal = "";

    const tipoRichiesta = new StatoRichiestaShortTextPipe();

    retVal = item.data + " " + this.getDataA(item) + " " + tipoRichiesta.transform(item.richiestaStato);

    return retVal;

  }

  public getItemText2(item: Dip_GG_RichiestaModel): string {
    let retVal = "";

    const tipoRichiesta = new TipoRichiestaToShortTextPipe();

    retVal = tipoRichiesta.transform(item.richiestaTipo) + " " + this.getDati(item);;

    return retVal;

  }



}
