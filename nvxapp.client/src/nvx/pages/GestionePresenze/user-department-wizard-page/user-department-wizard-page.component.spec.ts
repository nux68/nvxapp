import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { UserDepartmentWizardPageComponent } from './user-department-wizard-page.component';

describe('UserDepartmentWizardPageComponent', () => {
  let component: UserDepartmentWizardPageComponent;
  let fixture: ComponentFixture<UserDepartmentWizardPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ UserDepartmentWizardPageComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(UserDepartmentWizardPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
