// request-clocking-user-page.component.ts
import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';

@Component({
  selector: 'app-request-clocking-user-page',
  templateUrl: './request-clocking-user-page.component.html',
  styleUrls: ['./request-clocking-user-page.component.scss'],
  standalone: false
})
export class RequestClockingUserPageComponent implements OnInit {
  public title: string;
  public requestType: string;
  public startDate: string;
  public startDateTime: string;
  public supervisors: string[];
  public notes: string;

  constructor(public userNavigationService: UserNavigationService) {
    this.title = 'Richiedi timbratura';
    this.requestType = 'ENTRATA';

    // Initialize with current date
    const now = new Date();
    this.startDate = this.formatDate(now);
    this.startDateTime = this.formatDateTime(now);

    this.supervisors = ['manzo.admin'];
    this.notes = '';
  }

  ionViewWillEnter() {
  }

  ngOnInit() { }

  formatDate(date: Date): string {
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
  }

  formatDateTime(date: Date): string {
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${this.formatDate(date)} ${hours}:${minutes}`;
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
      startDate: this.startDate,
      startDateTime: this.startDateTime,
      supervisors: this.supervisors,
      notes: this.notes
    });

    // In a real app, this would send the data to a service
    alert('Richiesta inviata con successo!');
  }
}
