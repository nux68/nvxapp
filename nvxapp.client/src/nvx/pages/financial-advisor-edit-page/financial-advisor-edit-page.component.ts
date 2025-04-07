import { Component, OnInit } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../Utility/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FinancialAdvisorEditModel } from '../../ClientServer-Service/Account/Models/financial-advisor-model';

@Component({
  selector: 'app-financial-advisor-edit-page',
  templateUrl: './financial-advisor-edit-page.component.html',
  styleUrls: ['./financial-advisor-edit-page.component.scss'],
  standalone: false
})
export class FinancialAdvisorEditPageComponent extends BasePageConfirmCancelComponent<FinancialAdvisorEditModel> implements OnInit {

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder) {
    super(navCtrl,userInterfaceService,fb);
  }

  ngOnInit() { }

  get Title(): string { return "FinancialAdvisorEditPage"; }

  override get EditForm(): FormGroup {
    return this.fb.group({
      
      descrizione: [null, [Validators.required, Validators.maxLength(20)]],
      
    });
  }

  
  

  LoadData() {
    var c = 0;
  }

  //override ButtonCancelClickEv = (param: object) => {
  ////  this.navCtrl.back();
  //}
  


}


