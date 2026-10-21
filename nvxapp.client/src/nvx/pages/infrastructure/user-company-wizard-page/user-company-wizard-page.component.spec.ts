import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, provideIonicAngular } from '@ionic/angular';

import { UserCompanyWizardPageComponent } from './user-company-wizard-page.component';

describe('UserCompanyWizardPageComponent', () => {
  let component: UserCompanyWizardPageComponent;
  let fixture: ComponentFixture<UserCompanyWizardPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ UserCompanyWizardPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption]
    }).compileComponents();

    fixture = TestBed.createComponent(UserCompanyWizardPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
