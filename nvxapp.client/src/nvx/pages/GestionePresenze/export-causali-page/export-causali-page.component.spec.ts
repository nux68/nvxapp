import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonCard, IonCardContent, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonIcon, IonItem, IonLabel, IonModal, IonNote, IonRow, IonSelect, IonSelectOption, provideIonicAngular } from '@ionic/angular';

import { ExportCausaliPageComponent } from './export-causali-page.component';

describe('ExportCausaliPageComponent', () => {
  let component: ExportCausaliPageComponent;
  let fixture: ComponentFixture<ExportCausaliPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ExportCausaliPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonButton, IonCard, IonCardContent, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonIcon, IonItem, IonLabel, IonModal, IonNote, IonRow, IonSelect, IonSelectOption]
    }).compileComponents();

    fixture = TestBed.createComponent(ExportCausaliPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
