import { Component, Input, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';
import { Observable, of } from 'rxjs';
import { StringHelperService } from '../../../../Utility/infrastructure/string-helper.service';
import { Dip_GG_GiustificativiModel } from '../../../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model';
import { Par_GiustificativiModel } from '../../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { TipoValore } from '../../../../ClientServer-Service/GestionePresenze/Par_Causali/Models/par-causali-model';



@Component({
  selector: 'app-edit-dip-gg-giustificativi-dialog',
  templateUrl: './edit-dip-gg-giustificativi-dialog.component.html',
  styleUrls: ['./edit-dip-gg-giustificativi-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
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
    private transloco: TranslocoService,
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

  get Title(): string { return this.transloco.translate('attendance.components.editDipGgGiustificativiDialog.title'); }

  get EditForm(): FormGroup {
    // ore per i giustificativi a ore, un solo importo (euro) per quelli a importo
    return this.fb.group({
      idPar_Giustificativi: [null, [Validators.required]],
      hours: [null],
      importo: [null, [Validators.min(0.01), Validators.max(99999999.99)]],
    }, { validators: (g: AbstractControl) => this.valoreValidator(g) });
  }

  /** giustificativo scelto a importo */
  public get aImporto(): boolean {
    return this.tipoDi(this._editForm?.get('idPar_Giustificativi')?.value) === TipoValore.Importo;
  }

  private tipoDi(idPar_Giustificativi: number | null): TipoValore {
    const par = (this.sharedParameterGestionePresenzeService.Par_Giustificativi ?? []).find(x => x.id === idPar_Giustificativi);
    return par?.tipo ?? TipoValore.Orario;
  }

  /** obbligatorio il valore del tipo del giustificativo: ore oppure importo */
  private valoreValidator(g: AbstractControl): ValidationErrors | null {
    if (this.tipoDi(g.get('idPar_Giustificativi')?.value) === TipoValore.Importo)
      return g.get('importo')?.value ? null : { importoRequired: true };
    return g.get('hours')?.value ? null : { hoursRequired: true };
  }

  LoadData = (): Observable<Dip_GG_GiustificativiModel | null> => {

    return of(this.dip_GG_Giustificativi);
  };

  SaveData = (editModel: Dip_GG_GiustificativiModel): Observable<Dip_GG_GiustificativiModel> => {
    return of(this._editModel);
  };







}
