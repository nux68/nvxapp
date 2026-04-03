import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';
import { Observable, of } from 'rxjs';
import { StringHelperService } from '../../../../Utility/infrastructure/string-helper.service';
import { Dip_GG_GiustificativiModel } from '../../../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model';
import { Par_GiustificativiModel } from '../../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';



@Component({
  selector: 'app-edit-dip-gg-giustificativi-dialog',
  templateUrl: './edit-dip-gg-giustificativi-dialog.component.html',
  styleUrls: ['./edit-dip-gg-giustificativi-dialog.component.scss'],
  standalone: false
}) 
export class EditDipGGGiustificativiDialogComponent extends BaseDialogConfirmCancelComponent<Dip_GG_GiustificativiModel> {

  @Input() dip_GG_Giustificativi: Dip_GG_GiustificativiModel;


  public _par_GiustificativiList: Par_GiustificativiModel[] = [];

  public dateTime: string;
  public formattedDate: string;
  public formattedTime: string;
  public originalId: number;

  constructor(
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    protected override modalCtrl: ModalController,
    private stringHelperService: StringHelperService,
    public sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService
  ) {
    super(userInterfaceService, fb, modalCtrl);
  }

  override ionViewWillEnter() {
    super.ionViewWillEnter();
    this.originalId = this.dip_GG_Giustificativi.id;
    this._par_GiustificativiList = this.sharedParameterGestionePresenzeService.Par_Giustificativi;
  }

  get Title(): string { return "Giustificativo"; }

  get EditForm(): FormGroup {
    return this.fb.group({
      idPar_Giustificativi: [null, [Validators.required]],
      hours: [null, [Validators.required]],
    });
  }

  LoadData = (): Observable<Dip_GG_GiustificativiModel | null> => {

    return of(this.dip_GG_Giustificativi);
  };

  SaveData = (editModel: Dip_GG_GiustificativiModel): Observable<Dip_GG_GiustificativiModel> => {
    return of(this._editModel);
  };







}
