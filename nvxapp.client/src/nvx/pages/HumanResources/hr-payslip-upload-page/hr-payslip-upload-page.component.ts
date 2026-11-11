import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { HrService } from '../../../ClientServer-Service/HumanResources/hr.service';
import * as M from '../../../ClientServer-Service/HumanResources/Models/hr-model';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { hrScegliFile } from '../hr-ui';

class HrUploadForm extends M.HrCaricamentoUploadModel { public nomeFile: string | null = null; }

@Component({
  selector: 'app-hr-payslip-upload-page',
  templateUrl: './hr-payslip-upload-page.component.html',
  styleUrls: ['./hr-payslip-upload-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Caricamento del PDF unico dei cedolini o delle CU (azienda o studio): la divisione per dipendente parte come job sul server
export class HrPayslipUploadPageComponent extends BasePageConfirmCancelComponent<HrUploadForm> {

  public readonly mesi = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13];
  private file: File | null = null;
  private idCompany: number | null = null;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: HrService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    this.btnConfirm.text = this.transloco.translate('hr.pages.payslipUpload.start');
    this.btnConfirm.image = 'cloud-upload-outline';
  }

  get Title(): string { return this.transloco.translate('hr.pages.payslipUpload.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      tipo: [M.HrTipoDocumento.Cedolino, [Validators.required]],
      anno: [new Date().getFullYear(), [Validators.required, Validators.min(2000), Validators.max(2100)]],
      mese: [new Date().getMonth() + 1],
      nomeFile: [null, [Validators.required]],
    });
  }

  get cedolino(): boolean { return this._editForm?.value.tipo === M.HrTipoDocumento.Cedolino; }

  LoadData = (): Observable<HrUploadForm> => {
    this.idCompany = history.state?.idCompany ?? null;
    this.file = null;
    return of(new HrUploadForm());
  };

  async scegli() {
    this.file = await hrScegliFile('application/pdf,.pdf');
    this._editForm.patchValue({ nomeFile: this.file?.name ?? null });
  }

  SaveData = (editModel: HrUploadForm): Observable<boolean> => {
    if (!this.file)
      return of(false);
    const data = new M.HrCaricamentoUploadModel();
    data.idCompany = this.idCompany;
    data.tipo = Number(editModel.tipo);
    data.anno = Number(editModel.anno);
    data.mese = data.tipo === M.HrTipoDocumento.Cedolino ? Number(editModel.mese) : null;
    return this.service.CaricamentoUpload(data, this.file).pipe(map(res => res.success), catchError(() => of(false)));
  };
}
