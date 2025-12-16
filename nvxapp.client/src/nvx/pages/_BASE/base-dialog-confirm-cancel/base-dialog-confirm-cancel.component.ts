import { Component, Injectable, OnInit } from '@angular/core';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ModalController, NavController } from '@ionic/angular';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Observable, of } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export abstract class BaseDialogConfirmCancelComponent<T> implements OnInit {

  public buttonbar: ButtonItem[] = [];

  public _editModel: T | null = null;
  public _editForm: FormGroup;

  constructor(protected userInterfaceService: UserInterfaceService,
              protected fb: FormBuilder,
              protected modalCtrl: ModalController) {

    this.buttonbar = userInterfaceService.Btn_ConfermaAnnulla;
    this.buttonbar[0].event = this._handleButtonConfirmClick;
    this.buttonbar[1].event = this._handleButtonCancelClick;
   
  }
   
  ngOnInit() {
    this._editForm = this.EditForm;

    this._editForm.statusChanges.subscribe(() => {
      this.buttonbar[0].disabled = !this._editForm.valid;
    });

  }

  private _handleButtonConfirmClick = () => {
    this.forceValidation();

    if (this._editForm.valid) {
      // Applica le modifiche del form al modello dati
      this.patchObject(this._editModel, this._editForm.value);

      this.LoadData().subscribe(res => {

        // Chiude il modal e restituisce l'oggetto aggiornato con il ruolo 'confirm'
        this.modalCtrl.dismiss(res, 'confirm');

      });


      
    }
  }

  private _handleButtonCancelClick = () => {
    // Chiude il modal senza restituire dati, con il ruolo 'cancel'
    this.modalCtrl.dismiss(null, 'cancel');
  }


  ionViewWillEnter() {

    this.LoadData().subscribe(res => {

      this._editModel = res;

      // Popola il form con i dati ottenuti
      if (this._editModel) {
        this._editForm.patchValue(this._editModel);
      }
      else {
        this._editForm.setErrors({ formInvalid: true });
      }

    });

  }

  

  //Assegna le var degli oggetti di ogegtti
  private patchObject(target: any, source: any) {
    if (!target || !source) return;
    Object.keys(source).forEach(key => {
      if (
        source[key] !== null &&
        typeof source[key] === 'object' &&
        !Array.isArray(source[key]) &&
        target[key] !== undefined
      ) {
        this.patchObject(target[key], source[key]);
      } else {
        target[key] = source[key];
      }
    });
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

  abstract SaveData(editModel: T): Observable<T>;

  abstract get Title(): string;
    
  abstract get EditForm(): FormGroup | null;



}




