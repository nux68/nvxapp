import { Component,  OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_GiustificativiInModel, Par_GiustificativiModel } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParGiustificativiService } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/par-giustificativi.service';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { Dip_GG_Richiesta_Body_Timbratura, Dip_GG_Richiesta_GetAll4Admin_InModel, Dip_GG_Richiesta_GetAll4User_InModel, Dip_GG_Richiesta_SetState_InModel, Dip_GG_RichiestaModel, StatoRichiesta, TipoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { MonthNavigatorService } from '../../../Utility/infrastructure/month-navigator.service';
import { TimeSheetUtilityService } from '../../../Utility/GestionePresenze/time-sheet-utility.service';
import { StatoRichiestaLongTextPipe } from '../../../shared/pipe/GestionePresenze/stato-richiesta-long-text.pipe';
import { TipoRichiestaToLongTextPipe } from '../../../shared/pipe/GestionePresenze/tipo-richiesta-to-long-text.pipe';
import { TipoRichiestaToShortTextPipe } from '../../../shared/pipe/GestionePresenze/tipo-richiesta-to-short-text.pipe';
import { StatoRichiestaShortTextPipe } from '../../../shared/pipe/GestionePresenze/stato-richiesta-short-text.pipe';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { TimeSheetService } from '../../../Utility/GestionePresenze/time-sheet.service';




@Component({
  selector: 'app-request-list-admin-page',
  templateUrl: './request-list-admin-page.component.html',
  styleUrls: ['./request-list-admin-page.component.scss'],
  standalone: false
}) 
export class RequestListAdminPageComponent implements OnInit {

  public title!: string;
  public searchText!: string;
  public dip_GG_RichiestaList: Dip_GG_RichiestaModel[] | null = null;
  public btnApprova: ButtonItem;
  public btnRifiuta: ButtonItem;
  public currRappLavSel: number[] = [];
  public currYear: number
  public currMonth: number
  public StatoRichiesta = StatoRichiesta;

  //public tipoRichiestaToLongTextPipe: TipoRichiestaToLongTextPipe,
  

  constructor(private navCtrl: NavController,
    public timeSheetService: TimeSheetService,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private dipGGRichiestaService: DipGGRichiestaService,
    public fabMenuService: FabMenuService,
    
    //private monthNavigatorService: MonthNavigatorService,
    private userInterfaceService: UserInterfaceService,
    private dipGGTimbraturaUtilityService: TimeSheetUtilityService,
    private userNavigationService: UserNavigationService) {

    this.title = 'Request List Admin';
    this.btnApprova = userInterfaceService.Btn_Approva;
    this.btnApprova.event = this.handleButtonApprovaClick;

    this.btnRifiuta = userInterfaceService.Btn_Rifiuta;
    this.btnRifiuta.event = this.handleButtonRifiutaClick;

    
  }



  ionViewWillEnter() {

    this.loadData();

    //this.fabMenuService.fabMenuItem = [

    //  new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
    //    this.navCtrl.navigateForward('/justificationedit', {
    //      state: { id: 0 }
    //    });
    //  }),

    //];

  }



  ionViewWillLeave() {
    //this.fabMenuService.fabMenuItem = [];
  }

  ngOnInit() { }


  handleButtonApprovaClick = (item: any) => {
    this.sendStato(item, StatoRichiesta.Approvata );
    //this.navCtrl.navigateForward('/justificationedit', {
    //  state: { id: item.id }
    //});
  }
  handleButtonRifiutaClick = (item: any) => {
    this.sendStato(item, StatoRichiesta.Rifiutata);
    //this.navCtrl.navigateForward('/justificationedit', {
    //  state: { id: item.id }
    //});
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  private loadData() {
    let request: GenericRequest<Dip_GG_Richiesta_GetAll4Admin_InModel> = new GenericRequest<Dip_GG_Richiesta_GetAll4Admin_InModel>(Dip_GG_Richiesta_GetAll4Admin_InModel);
    request.data.month = this.currMonth;
    request.data.year = this.currYear;


    this.dipGGRichiestaService.GetAll4Admin(request).subscribe(res => {
      this.dip_GG_RichiestaList = res.data.dip_GG_Richiesta;
    });
  }

  private sendStato(item: any, statoRichiesta: StatoRichiesta) {

    let IdDip_GG_Richiesta: number[] = [];
    IdDip_GG_Richiesta.push(item.id);

    let request: GenericRequest<Dip_GG_Richiesta_SetState_InModel> = new GenericRequest<Dip_GG_Richiesta_SetState_InModel>(Dip_GG_Richiesta_SetState_InModel);
    request.data.richiestaStato = statoRichiesta;
    request.data.IdDip_GG_Richiesta = IdDip_GG_Richiesta;
    this.dipGGRichiestaService.SetState(request).subscribe(res => {
      this.loadData();
    });
  }

  isAdmin(item: any) {

    return false;
  }

  getAll() {
    //const sorted_GiustificativiList = this.par_GiustificativiList.sort((a, b) =>
    //  a.descrizione.localeCompare(b.descrizione)
    //);
    //return sorted_GiustificativiList;

    return this.dip_GG_RichiestaList.filter(x => x.richiestaStato != StatoRichiesta.Cancellata && this.currRappLavSel.includes(x.idDip_RapportoLavoro));
  }

  public getDataA(item: Dip_GG_RichiestaModel): string {
    if (item.data != item.dataA)
      return item.dataA;

    return '';

  }

  public getDati(item: Dip_GG_RichiestaModel): string {

    return this.dipGGTimbraturaUtilityService.getDatitext(item);

  }

  public showBtnApprova(item: Dip_GG_RichiestaModel): boolean {

    if (item.revocaStato == null) {
      if (item.richiestaStato != StatoRichiesta.Immessa)
        return false;

      return true;
    }
    else {
      if (item.revocaStato != StatoRichiesta.Immessa)
        return false;

      return true;
    }

    
  }

  public showBtnRifiuta(item: Dip_GG_RichiestaModel): boolean{
    if (item.revocaStato == null) {
      if (item.richiestaStato != StatoRichiesta.Immessa)
        return false;

      return true;
    }
    else {
      if (item.revocaStato != StatoRichiesta.Immessa)
        return false;

      return true;
    }
  }

  public getItemText1(item: Dip_GG_RichiestaModel):string {
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


  onPeriodChange(period: { year: number, month: number } | undefined): void {
    //this.selectedSedeId = sediId;
    console.log('Parent: Period changed to:', period.year + period.month);

    this.currYear = period.year;
    this.currMonth = period.month;

    //this.fetchRelevantData();
  }

  onSedeChanged(sediId: number | undefined): void {
    //this.selectedSedeId = sediId;
    //console.log('Parent: Sede ID changed to:', sediId);
    //this.fetchRelevantData();
  }
  onRepartiChanged(repartoIds: number[] | undefined): void {
    //this.selectedRepartoIds = repartoIds;
    //console.log('Parent: Reparto IDs changed to:', repartoIds);
    // Note: User list will be re-evaluated by the navigation component,
    // leading to onCurrentUserChanged potentially being called.
    // We might not need to call fetchRelevantData() here if onCurrentUserChanged also calls it.
    // However, if we want to show data aggregated by reparti even if no user is selected,
    // then we might fetch here. For this example, let's assume we act on user change.
  }
  onCurrentUserChanged(userId: string | undefined): void {
    //this.currentNavigationUserId = userId;
    //console.log('Parent: Current User ID changed to:', userId);
    //this.fetchRelevantData();
  }

  onAllUsersInSelectionChanged(userIds: string[] | undefined): void {
    console.log('Parent: All available User IDs in current selection:', userIds);

    var idDipRappLav = this.sharedParameterGestionePresenzeService.Dip_Anagrafica
      .filter(x => userIds.includes(x.idAspNetUsers))
      .reduce((acc, dip) => acc.concat(dip.dip_RapportoLavoro.map(rapporto => rapporto.id)), []);
    

    this.currRappLavSel = idDipRappLav;                                                                              

  }

  
}

