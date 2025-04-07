import { Component, OnInit } from '@angular/core';
import { ButtonItem, UserInterfaceService } from '../../../Utility/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup } from '@angular/forms';

//@Component({
//  selector: 'app-base-page-confirm-cancel',
//  templateUrl: './base-page-confirm-cancel.component.html',
//  styleUrls: ['./base-page-confirm-cancel.component.scss'],
//  standalone: false
//})
export abstract class BasePageConfirmCancelComponent<T>  /*implements OnInit*/ {

  public buttonbar: ButtonItem[] = [];

  public _editModel: T | null = null;
  public _editForm: FormGroup;

  constructor(protected navCtrl: NavController,
              protected userInterfaceService: UserInterfaceService,
              protected fb: FormBuilder) {

    this.buttonbar = userInterfaceService.Btn_ConfermaAnnulla;
    this.buttonbar[0].event = this._handleButtonConfirmClick;
    this.buttonbar[1].event = this._handleButtonCancelClick;

    this._editForm = this.EditForm;
  }

  //ngOnInit() {
  //  //this._editForm = this.EditForm;
  //}

  ionViewWillEnter() {
    
    this.LoadData();
  }

  _handleButtonConfirmClick = (param: object) => {
    this.ButtonConfirmClickEv(param);
  }

  _handleButtonCancelClick = (param: object) => {
    this.ButtonCancelClickEv(param);
  }


  abstract get Title(): string;

  /*abstract*/ get EditForm(): FormGroup | null { return null};

  get ShowFilter(): boolean { return false; }


  abstract LoadData(): void;

  ButtonConfirmClickEv = (param: object) => {

    if (this._editForm.valid) {
      Object.assign(this._editModel, this._editForm.value);

    //  let request: GenericRequest<DealerPutInModel> = new GenericRequest<DealerPutInModel>(DealerPutInModel);
    //  request.data.dealerEdit = this.editModel;

    //  this.accountService.DealerPut(request).subscribe(res => {

        this.navCtrl.back();

    //  });



    }

  }

  ButtonCancelClickEv = (param: object) => {
    this.navCtrl.back();
  }


  ////ngOnInit() {
  ////  var c = 0;
  ////}

  //conferma(): void {
  //  console.log('Conferma dal componente base');
  //}

  //annulla(): void {
  //  console.log('Annulla dal componente base');
  //}

  

}


