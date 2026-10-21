import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonHeader, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonTitle, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { EditParExportCauCausaliComponentComponent } from './edit-par-export-cau-causali-component.component';

describe('EditParExportCauCausaliComponentComponent', () => {
  let component: EditParExportCauCausaliComponentComponent;
  let fixture: ComponentFixture<EditParExportCauCausaliComponentComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ EditParExportCauCausaliComponentComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonHeader, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonTitle, IonToolbar]
    }).compileComponents();

    fixture = TestBed.createComponent(EditParExportCauCausaliComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
