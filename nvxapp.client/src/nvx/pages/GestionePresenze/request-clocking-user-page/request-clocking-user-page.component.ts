// request-clocking-user-page.component.ts
import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';

@Component({
  selector: 'app-request-clocking-user-page',
  templateUrl: './request-clocking-user-page.component.html',
  styleUrls: ['./request-clocking-user-page.component.scss'],
  standalone: false
})
export class RequestClockingUserPageComponent implements OnInit {
  public title: string;
  public requestType: string;
  public dateTime: string;
  public formattedDateTime: string;
  public supervisors: string[];
  public notes: string;

  constructor(public userNavigationService: UserNavigationService,
              private pipGGRichiestaService: DipGGRichiestaService) {

    this.title = 'Richiedi timbratura';
    this.requestType = 'ENTRATA';

    // Initialize with current date and time
    const now = new Date();
    // Format date for ion-datetime (ISO format)
    this.dateTime = now.toISOString();
    this.formattedDateTime = this.formatDateTime(now);

    this.supervisors = ['manzo.admin'];
    this.notes = '';
  }

  ionViewWillEnter() {
  }

  ngOnInit() { }

  formatDateTime(date: Date): string {
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${day}/${month}/${year} ${hours}:${minutes}`;
  }

  updateDateTime(event: any) {
    const selectedDate = new Date(event.detail.value);
    this.formattedDateTime = this.formatDateTime(selectedDate);
  }

  addSupervisor() {
    // In a real app, this would open a modal or dropdown to select from available supervisors
    // For demo purposes, we'll just add a mock supervisor
    if (!this.supervisors.includes('new.supervisor')) {
      this.supervisors.push('new.supervisor');
    }
  }

  removeSupervisor(index: number) {
    this.supervisors.splice(index, 1);
  }

  submitRequest() {

    console.log('Request submitted', {
      type: this.requestType,
      dateTime: this.dateTime,
      formattedDateTime: this.formattedDateTime,
      supervisors: this.supervisors,
      notes: this.notes
    });

    // In a real app, this would send the data to a service
    alert('Richiesta inviata con successo!');

  }
}
