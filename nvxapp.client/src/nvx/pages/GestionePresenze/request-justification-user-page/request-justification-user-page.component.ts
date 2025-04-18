// request-justification-user-page.component.ts
import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';

@Component({
  selector: 'app-request-justification-user-page',
  templateUrl: './request-justification-user-page.component.html',
  styleUrls: ['./request-justification-user-page.component.scss'],
  standalone: false // Assuming this component is declared in an NgModule
})
export class RequestJustificationUserPageComponent implements OnInit {
  public title: string;
  public justificationType: string;
  public requestType: string;
  // Store dates in ISO 8601 format, compatible with ion-datetime's ngModel
  public startDate: string;
  public endDate: string;
  public hours: number;
  public hoursFormatted: string;
  public fullDay: boolean;
  public supervisors: string[];
  public notes: string;

  // Optional: Define a maximum date if needed for the pickers
  // public maxDate: string = new Date(new Date().getFullYear() + 5, 11, 31).toISOString();

  constructor(public userNavigationService: UserNavigationService) {
    this.title = 'Richiesta ferie e permessi';
    this.justificationType = 'FERIE';
    this.requestType = 'A_DURATA';

    // Initialize with current date in ISO 8601 format
    const now = new Date();
    // Set time to midnight for consistency when dealing with dates only
    now.setHours(0, 0, 0, 0);
    this.startDate = now.toISOString();
    this.endDate = now.toISOString(); // Initialize end date same as start date

    this.hours = 1;
    this.hoursFormatted = '01:00';
    this.fullDay = false;
    this.supervisors = ['manzo.admin'];
    this.notes = '';
  }

  ionViewWillEnter() {
    // Add any logic needed when the view is about to enter
  }

  ngOnInit() {
    // Add any initialization logic needed after the component is created
  }

  // Removed formatDate function as it's no longer needed for binding.
  // ion-datetime handles presentation format internally via locale and displayFormat (if used),
  // and ngModel uses ISO 8601 string format.

  increaseHours() {
    if (!this.fullDay && this.hours < 8) { // Prevent changing hours if fullDay is true
      this.hours++;
      this.updateHoursFormatted();
    }
  }

  decreaseHours() {
    if (!this.fullDay && this.hours > 1) { // Prevent changing hours if fullDay is true
      this.hours--;
      this.updateHoursFormatted();
    }
  }

  updateHoursFormatted() {
    this.hoursFormatted = `${this.hours.toString().padStart(2, '0')}:00`;
  }

  toggleFullDay() {
    if (this.fullDay) {
      this.hours = 8; // Set hours to 8 for a full day
    } else {
      this.hours = 1; // Reset to default or minimum hours when not full day
    }
    this.updateHoursFormatted();
    // Optionally disable hour controls when fullDay is true (handled via [disabled] in template now)
  }

  addSupervisor() {
    // In a real app, this would likely open a user selection modal/popup
    // For demo purposes, adding a static mock supervisor
    const newSupervisor = 'new.supervisor.' + (this.supervisors.length + 1); // Make it unique for demo
    if (!this.supervisors.includes(newSupervisor)) {
      this.supervisors.push(newSupervisor);
    }
    // Alternatively, prompt the user:
    // const supervisorToAdd = prompt("Inserisci nome utente responsabile:");
    // if (supervisorToAdd && !this.supervisors.includes(supervisorToAdd)) {
    //   this.supervisors.push(supervisorToAdd);
    // }
  }

  removeSupervisor(index: number) {
    if (index >= 0 && index < this.supervisors.length) {
      this.supervisors.splice(index, 1);
    }
  }

  submitRequest() {
    // Note: startDate and endDate are now in ISO 8601 format (e.g., "2023-10-27T00:00:00.000Z")
    // Adjust the hours value if the request type is 'GIORNALIERA' or if fullDay is true
    const hoursToSend = (this.requestType === 'GIORNALIERA' || this.fullDay) ? 8 : this.hours;

    const requestData = {
      justificationType: this.justificationType,
      requestType: this.requestType,
      // You might want to format the date for the backend here if ISO is not desired
      // e.g., startDate: this.formatDateForBackend(new Date(this.startDate)),
      startDate: this.startDate,
      endDate: this.endDate,
      hours: hoursToSend, // Use adjusted hours
      fullDay: this.fullDay, // Keep track if it was explicitly set as full day
      supervisors: this.supervisors,
      notes: this.notes
    };

    console.log('Request submitted', requestData);

    // --- Placeholder for actual service call ---
    // this.yourApiService.submitJustification(requestData).subscribe({
    //   next: (response) => {
    //     console.log('Submission successful', response);
    //     alert('Richiesta inviata con successo!');
    //     // Optionally navigate away or reset form
    //     // this.resetForm();
    //     // this.userNavigationService.goBack();
    //   },
    //   error: (error) => {
    //     console.error('Submission failed', error);
    //     alert('Errore durante l\'invio della richiesta. Riprova.');
    //   }
    // });
    // -----------------------------------------

    // Using alert for demo purposes as before
    alert('Richiesta inviata con successo! (Simulato)');
  }

  // Optional: Helper function if backend requires specific format like 'dd/MM/yyyy'
  // formatDateForBackend(date: Date): string {
  //   if (!date) return '';
  //   const day = date.getDate().toString().padStart(2, '0');
  //   const month = (date.getMonth() + 1).toString().padStart(2, '0'); // Month is 0-indexed
  //   const year = date.getFullYear();
  //   return `${day}/${month}/${year}`;
  // }

  // Optional: Method to reset the form after submission
  // resetForm() {
  //   const now = new Date();
  //   now.setHours(0, 0, 0, 0);
  //   this.startDate = now.toISOString();
  //   this.endDate = now.toISOString();
  //   this.justificationType = 'FERIE';
  //   this.requestType = 'A_DURATA';
  //   this.hours = 1;
  //   this.updateHoursFormatted();
  //   this.fullDay = false;
  //   // Decide whether to keep supervisors or reset them
  //   // this.supervisors = ['manzo.admin'];
  //   this.notes = '';
  // }
}
