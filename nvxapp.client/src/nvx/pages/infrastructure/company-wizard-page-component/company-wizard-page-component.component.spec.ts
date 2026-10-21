import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, provideIonicAngular } from '@ionic/angular';

import { CompanyWizardPageComponentComponent } from './company-wizard-page-component.component';

describe('CompanyWizardPageComponentComponent', () => {
  let component: CompanyWizardPageComponentComponent;
  let fixture: ComponentFixture<CompanyWizardPageComponentComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CompanyWizardPageComponentComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote]
    }).compileComponents();

    fixture = TestBed.createComponent(CompanyWizardPageComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
