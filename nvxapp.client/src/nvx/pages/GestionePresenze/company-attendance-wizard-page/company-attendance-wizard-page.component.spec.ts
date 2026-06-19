import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { CompanyAttendanceWizardPageComponent } from './company-attendance-wizard-page.component';

describe('CompanyAttendanceWizardPageComponent', () => {
  let component: CompanyAttendanceWizardPageComponent;
  let fixture: ComponentFixture<CompanyAttendanceWizardPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CompanyAttendanceWizardPageComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(CompanyAttendanceWizardPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
