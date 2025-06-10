import { Component } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AzSediService } from '../../../ClientServer-Service/GestionePresenze/Az_Sedi/az-sedi.service';
import { Az_SediModel, Az_SediGetInModel, Az_SediPutInModel } from '../../../ClientServer-Service/GestionePresenze/Az_Sedi/Models/az-sedi-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';

@Component({
  selector: 'app-az-sedi-edit-page',
  templateUrl: './az-sedi-edit-page.component.html',
  styleUrls: ['./az-sedi-edit-page.component.scss'],
  standalone: false
})
export class AzSediEditPageComponent {
  public title = 'Modifica Sede';
  public _editForm: FormGroup;
  public id: number = 0;
  public isNew = false;

  constructor(
    private navCtrl: NavController,
    private fb: FormBuilder,
    private azSediService: AzSediService
  ) {
    this._editForm = this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      default: [false]
    });
  }

  ionViewWillEnter() {
    const state = history.state;
    if (state && state.id) {
      this.id = state.id;
      this.loadData();
    } else {
      this.isNew = true;
    }
  }

  loadData() {
    let request = new GenericRequest<Az_SediGetInModel>(Az_SediGetInModel);
    request.data.id = this.id;
    this.azSediService.AzSediGet(request).subscribe(res => {
      if (res.data && res.data.az_Sedi) {
        this._editForm.patchValue(res.data.az_Sedi);
      }
    });
  }

  save() {
    if (this._editForm.invalid) return;
    let model = new GenericRequest<Az_SediPutInModel>(Az_SediPutInModel);
    model.data.az_Sedi = { ...this._editForm.value, id: this.id };
    this.azSediService.AzSediPut(model).subscribe(() => {
      this.navCtrl.back();
    });
  }

  cancel() {
    this.navCtrl.back();
  }
}
