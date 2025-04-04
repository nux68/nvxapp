import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { ButtonItem, UserInterfaceService } from '../../Utility/user-interface.service';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { DealerEditModel, DealerGetInModel, DealerListInModel, DealerPutInModel } from '../../ClientServer-Service/Account/Models/dealer-model';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';


@Component({
  selector: 'app-dealer-edit-page',
  templateUrl: './dealer-edit-page.component.html',
  styleUrls: ['./dealer-edit-page.component.scss'],
  standalone: false
})
export class DealerEditPageComponent  implements OnInit {

  public title!: string;
  public buttonbar: ButtonItem[] = [];


  public editModel: DealerEditModel | null = null;
  public dealerForm: FormGroup;

  constructor(private navCtrl: NavController,
              private accountService: AccountService,
              private userInterfaceService: UserInterfaceService,
              private fb: FormBuilder)
  {

    this.title = 'DealerEditPage';

    this.buttonbar = userInterfaceService.Btn_ConfermaAnnulla;
    this.buttonbar[0].event = this.handleButtonConfirmClick;
    this.buttonbar[1].event = this.handleButtonCancelClick;

    this.dealerForm = this.fb.group({
      //idDealer: [0, [Validators.required, Validators.min(1)]],
      descrizione: [null, [Validators.required, Validators.maxLength(20)]],
      //idAspNetUsers: ['', Validators.required],
      //mainUser: [false, Validators.required]
    });

  }

  onSubmit(): void {
    if (this.dealerForm.valid) {
      const dealerEditModel = this.dealerForm.value;
      console.log('Dati inviati:', dealerEditModel);
    } else {
      console.error('Il form non è valido!');
    }
  }


  ionViewWillEnter() {

    const state = history.state;
    if (state && state.id) {
      console.log('Item ID:', state.id); // Usa l'itemId come necessario

      let request: GenericRequest<DealerGetInModel> = new GenericRequest<DealerGetInModel>(DealerGetInModel);

      request.data.id = state.id;

      this.accountService.DealerGet(request).subscribe(res => {

        this.editModel = res.data.dealerEdit;

        // Popola il form con i dati ottenuti
        if (this.editModel) {
          this.dealerForm.patchValue(this.editModel);
        }

      });

    }

  }


  handleButtonConfirmClick = (param: object) => {

    if (this.dealerForm.valid) {
      Object.assign(this.editModel, this.dealerForm.value);

      let request: GenericRequest<DealerPutInModel> = new GenericRequest<DealerPutInModel>(DealerPutInModel);
      request.data.dealerEdit = this.editModel;

      this.accountService.DealerPut(request).subscribe(res => {

        this.navCtrl.back();  

      });


      
    }

    
  }

  handleButtonCancelClick = (param: object) => {
    this.navCtrl.back();
  }


  ngOnInit() {}

}
