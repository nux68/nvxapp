import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonCheckbox, IonCol, IonContent, IonGrid, IonHeader, IonLabel, IonNote, IonRow, IonTitle, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { SeletionParAttivitaDialogComponent } from './seletion-par-attivita-dialog.component';

describe('SeletionParAttivitaDialogComponent', () => {
  let component: SeletionParAttivitaDialogComponent;
  let fixture: ComponentFixture<SeletionParAttivitaDialogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SeletionParAttivitaDialogComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonCheckbox, IonCol, IonContent, IonGrid, IonHeader, IonLabel, IonNote, IonRow, IonTitle, IonToolbar]
    }).compileComponents();

    fixture = TestBed.createComponent(SeletionParAttivitaDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
