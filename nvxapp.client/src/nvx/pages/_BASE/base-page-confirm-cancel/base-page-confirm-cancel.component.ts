import { Component, Injectable, OnInit } from '@angular/core';
import { ButtonItem, UserInterfaceService } from '../../../Utility/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Observable, of } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export abstract class BasePageConfirmCancelComponent<T> implements OnInit {

  public buttonbar: ButtonItem[] = [];

  public _editModel: T | null = null;
  public _editForm: FormGroup;

  constructor(protected navCtrl: NavController,
              protected userInterfaceService: UserInterfaceService,
              protected fb: FormBuilder) {

    this.buttonbar = userInterfaceService.Btn_ConfermaAnnulla;
    this.buttonbar[0].event = this._handleButtonConfirmClick;
    this.buttonbar[1].event = this._handleButtonCancelClick;
   
  }
   
  ngOnInit() {
    this._editForm = this.EditForm;
  }

  ionViewWillEnter() {
    this.LoadData().subscribe(res => {

      this._editModel = res;

      // Popola il form con i dati ottenuti
      if (this._editModel) {
        this._editForm.patchValue(this._editModel);
      }

    });
  }

  private _handleButtonConfirmClick = (param: object) => {

    this.forceValidation();
    this.ButtonConfirmClickEv(param);

  }

  private _handleButtonCancelClick = (param: object) => {
    this.ButtonCancelClickEv(param);
  }
      
  ButtonConfirmClickEv = (param: object) => {

   

    if (this._editForm.valid) {
      Object.assign(this._editModel, this._editForm.value);

      this.SaveData(this._editModel).subscribe(res => {
        this.navCtrl.back();
      });
    }

  }

  ButtonCancelClickEv = (param: object) => {
    this.navCtrl.back();
  }

  private forceValidation() {
    // Forza la validazione su tutto il form
    Object.keys(this._editForm.controls).forEach((key) => {
      const control = this._editForm.get(key);
      control?.markAsTouched(); // Segna il campo come "toccato" per attivare gli errori
      control?.updateValueAndValidity(); // Forza la validazione
    });
  }


  //ridefinire se si vuole mostrare il filtro
  get ShowFilter(): boolean { return false; }

  abstract LoadData(): Observable<T>;

  abstract SaveData(editModel: T): Observable<boolean>;

  abstract get Title(): string;
    
  abstract get EditForm(): FormGroup | null;

}




