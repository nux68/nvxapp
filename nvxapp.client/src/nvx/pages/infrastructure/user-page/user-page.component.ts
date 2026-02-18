import { Component, OnInit } from '@angular/core';
import { SignalrService } from '../../../Utility/infrastructure/signalr.service';
import { environment } from '../../../../environments/environment';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { MyMokeLongJobService } from '../../../ClientServer-Service/Infrastructure/MyMokeLongJob/my-moke-long-job.service';
import { UserGetInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { MyMokeLongJobInModel } from '../../../ClientServer-Service/Infrastructure/MyMokeLongJob/Models/my-moke-long-job-model';







@Component({
  selector: 'app-user-page',
  templateUrl: './user-page.component.html',
  styleUrls: ['./user-page.component.scss'],
  standalone: false
})
export class UserPageComponent implements OnInit {

  public title!: string;


  public btnTask: ButtonItem;

  constructor(private signalrService: SignalrService,
    public userNavigationService: UserNavigationService,
    private userInterfaceService: UserInterfaceService,
    private myMokeLongJobService: MyMokeLongJobService
  )

  {
    this.title = 'UserPage';

    this.btnTask = userInterfaceService.Btn_Esegui;
    this.btnTask.event = this.handleButtontaskClick;

  }

  ionViewWillEnter() {
    if (environment.signalR.useSignalR) {
      this.signalrService.send("SendMessage", { 'text': "ciao" });
    }
  }


  ngOnInit() { }


  handleButtontaskClick = async (item: any) => {

    let request: GenericRequest<MyMokeLongJobInModel> = new GenericRequest<MyMokeLongJobInModel>(MyMokeLongJobInModel);

    this.myMokeLongJobService.StartJob(request).subscribe(x=>{

      let c = 0;

    });

  }

}
