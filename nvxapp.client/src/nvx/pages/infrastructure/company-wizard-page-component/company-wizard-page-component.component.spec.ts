import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { CompanyWizardPageComponentComponent } from './company-wizard-page-component.component';

describe('CompanyWizardPageComponentComponent', () => {
  let component: CompanyWizardPageComponentComponent;
  let fixture: ComponentFixture<CompanyWizardPageComponentComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CompanyWizardPageComponentComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(CompanyWizardPageComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
