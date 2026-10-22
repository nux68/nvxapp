import { TranslocoService } from '@jsverse/transloco';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { My_template1_GetAllInModel, My_Template1Model, My_template1_DeleteInModel } from '../../../ClientServer-Service/GestionePresenze/My_Template1Service/Models/my-template1-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { MyTemplate1Service } from '../../../ClientServer-Service/GestionePresenze/My_Template1Service/my-template1.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { map, catchError } from 'rxjs';

// Modello delle pagine elenco.
// Se la tabella e' tra quelle caricate al login (SharedParameterGestionePresenzeService), l'elenco va letto
// dalla cache con un getter (vedi causali-list-page) e dopo salvataggi / cancellazioni si chiama
// RefresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.<tabella>) invece di loadData().
@Component({
  selector: 'app-mytemplate1-list-page',
  templateUrl: './mytemplate1-list-page.component.html',
  styleUrls: ['./mytemplate1-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class MyTemplate1ListPageComponent {
  public title!: string;
  public searchText!: string;
  public myTemplate1List: My_Template1Model[] | null = null;
  public readonly columns: NvxListColumn<My_Template1Model>[] = [
    { key: 'descrizione', title: this.transloco.translate('attendance.pages.mytemplate1List.columnDescription') },
  ];
  public actions: NvxListAction<My_Template1Model>[] = [];
  public readonly sortList = (a: My_Template1Model, b: My_Template1Model) => (a.descrizione ?? '').localeCompare(b.descrizione ?? '');
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private myTemplate1Service: MyTemplate1Service,
              private userInterfaceService: UserInterfaceService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('attendance.pages.mytemplate1List.title');
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;

    this.actions = [{ button: this.btnDelete }, { button: this.btnEdit }];
  }

  private loadData() {
    let request: GenericRequest<My_template1_GetAllInModel> = new GenericRequest<My_template1_GetAllInModel>(My_template1_GetAllInModel);
    this.myTemplate1Service.GetAll(request).subscribe(res => {
      this.myTemplate1List = res.data.my_Template1;
    });
  }

  ionViewWillEnter() {
    this.loadData();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('attendance.pages.mytemplate1List.fabAdd'), 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/mytemplate1edit', {
          state: { id: 0 }
        });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }


  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/mytemplate1edit', {
      state: { id: item.id }
    });
  }

  handleButtonDeleteClick = async (item: any) => {
    const result = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.mytemplate1List.confirmDelete'));
    if (result) {
      let request: GenericRequest<My_template1_DeleteInModel> = new GenericRequest<My_template1_DeleteInModel>(My_template1_DeleteInModel);
      request.data.id = item.id;
      this.myTemplate1Service.MyTemplate1Delete(request).pipe(
        map(() => {
          this.loadData();
          return true;
        }),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [false];
        })
      ).subscribe();
    }
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
