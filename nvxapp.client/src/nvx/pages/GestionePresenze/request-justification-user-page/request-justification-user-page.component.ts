// request-justification-user-page.component.ts
import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';

@Component({
  selector: 'app-request-justification-user-page',
  templateUrl: './request-justification-user-page.component.html',
  styleUrls: ['./request-justification-user-page.component.scss'],
  standalone: false
})
export class RequestJustificationUserPageComponent implements OnInit {
  public title: string;
  public justificationType: string;
  public requestType: string;
  public startDate: string;
  public endDate: string;
  public hours: number;
  public hoursFormatted: string;
  public fullDay: boolean;
  public supervisors: string[];
  public notes: string;

  constructor(public userNavigationService: UserNavigationService) {
    this.title = 'Richiesta ferie e permessi';
    this.justificationType = 'FERIE';
    this.requestType = 'A_DURATA';

    // Initialize with current date
    const now = new Date();
    this.startDate = this.formatDate(now);
    this.endDate = this.formatDate(now);

    this.hours = 1;
    this.hoursFormatted = '01:00';
    this.fullDay = false;
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

  increaseHours() {
    if (this.hours < 8) {
      this.hours++;
      this.updateHoursFormatted();
    }
  }

  decreaseHours() {
    if (this.hours > 1) {
      this.hours--;
      this.updateHoursFormatted();
    }
  }

  updateHoursFormatted() {
    this.hoursFormatted = `${this.hours.toString().padStart(2, '0')}:00`;
  }

  toggleFullDay() {
    if (this.fullDay) {
      this.hours = 8;
    } else {
      this.hours = 1;
    }
    this.updateHoursFormatted();
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
      justificationType: this.justificationType,
      requestType: this.requestType,
      startDate: this.startDate,
      endDate: this.endDate,
      hours: this.hours,
      fullDay: this.fullDay,
      supervisors: this.supervisors,
      notes: this.notes
    });

    // In a real app, this would send the data to a service
    alert('Richiesta inviata con successo!');
  }
}
