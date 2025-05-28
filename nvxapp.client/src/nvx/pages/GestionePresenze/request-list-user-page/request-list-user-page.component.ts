import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
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



@Component({
  selector: 'app-request-list-user-page',
  templateUrl: './request-list-user-page.component.html',
  styleUrls: ['./request-list-user-page.component.scss'],
  standalone:false
}) 
export class RequestListUserPageComponent implements OnInit {

  public title!: string;
  public searchText!: string;
  public dip_GG_RichiestaList: Dip_GG_RichiestaModel[] | null = null;
  public btnDelete: ButtonItem;
  public StatoRichiesta = StatoRichiesta;

  constructor(private navCtrl: NavController,
    private dipGGRichiestaService: DipGGRichiestaService,
    public fabMenuService: FabMenuService,
    public timeSheetService: TimeSheetService,
    private monthNavigatorService: MonthNavigatorService,
    private userInterfaceService: UserInterfaceService,
    private dipGGTimbraturaUtilityService: TimeSheetUtilityService,
    private userNavigationService: UserNavigationService) {

    this.title = 'Request List User';
    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
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
  }

  ngOnInit() { }


  handleButtonDeleteClick = (item: any) => {
    let IdDip_GG_Richiesta: number[] = [];
    IdDip_GG_Richiesta.push(item.id);

    let request: GenericRequest<Dip_GG_Richiesta_SetState_InModel> = new GenericRequest<Dip_GG_Richiesta_SetState_InModel>(Dip_GG_Richiesta_SetState_InModel);

    if (item.richiestaStato == this.StatoRichiesta.Approvata && (item.revocaStato == null || (item.revocaStato !== null && item.revocaStato == this.StatoRichiesta.Cancellata) )  )
      request.data.richiestaStato = StatoRichiesta.Immessa; //revoca
    else
      request.data.richiestaStato = StatoRichiesta.Cancellata;

    request.data.IdDip_GG_Richiesta = IdDip_GG_Richiesta;
    this.dipGGRichiestaService.SetState(request).subscribe(res => {
      this.loadData();
    });

    //this.navCtrl.navigateForward('/justificationedit', {
    //  state: { id: item.id }
    //});
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  isAdmin(item: any) {

    return false;
  }

  getAll() {
    //const sorted_GiustificativiList = this.par_GiustificativiList.sort((a, b) =>
    //  a.descrizione.localeCompare(b.descrizione)
    //);
    //return sorted_GiustificativiList;

    return this.dip_GG_RichiestaList;
  }

  public getDataA(item:Dip_GG_RichiestaModel): string{
    if (item.data != item.dataA)
      return item.dataA;

    return '';

  }

  public getDati(item: Dip_GG_RichiestaModel): string {

    return this.dipGGTimbraturaUtilityService.getDatitext(item);

  }


  public showBtnDelete(item: Dip_GG_RichiestaModel): boolean {

    if (item.revocaStato == null) {
      if (item.richiestaStato == StatoRichiesta.Immessa ||
        item.richiestaStato == StatoRichiesta.ApprovazioneInCorso ||
        item.richiestaStato == StatoRichiesta.Approvata)
        return true;
      return false;
    }
    else {
      if (item.revocaStato == StatoRichiesta.Immessa ||
        item.revocaStato == StatoRichiesta.ApprovazioneInCorso )
        return true;
      return false;
    }

    
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
