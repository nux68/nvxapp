import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonDatetime, IonDatetimeButton, IonInput, IonItem, IonLabel, IonModal, IonNote, IonSelect, IonSelectOption, IonText, provideIonicAngular } from '@ionic/angular';

import { UserDepartmentWizardPageComponent } from './user-department-wizard-page.component';

describe('UserDepartmentWizardPageComponent', () => {
  let component: UserDepartmentWizardPageComponent;
  let fixture: ComponentFixture<UserDepartmentWizardPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ UserDepartmentWizardPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonDatetime, IonDatetimeButton, IonInput, IonItem, IonLabel, IonModal, IonNote, IonSelect, IonSelectOption, IonText]
    }).compileComponents();

    fixture = TestBed.createComponent(UserDepartmentWizardPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
