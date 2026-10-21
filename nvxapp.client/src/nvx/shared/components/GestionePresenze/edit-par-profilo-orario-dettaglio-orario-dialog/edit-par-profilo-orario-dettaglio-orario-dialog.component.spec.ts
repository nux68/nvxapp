import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonHeader, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonTitle, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { EditParProfiloOrarioDettaglioOrarioDialogComponent } from './edit-par-profilo-orario-dettaglio-orario-dialog.component';

describe('EditParProfiloOrarioDettaglioOrarioDialogComponent', () => {
  let component: EditParProfiloOrarioDettaglioOrarioDialogComponent;
  let fixture: ComponentFixture<EditParProfiloOrarioDettaglioOrarioDialogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ EditParProfiloOrarioDettaglioOrarioDialogComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonHeader, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonTitle, IonToolbar]
    }).compileComponents();

    fixture = TestBed.createComponent(EditParProfiloOrarioDettaglioOrarioDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
