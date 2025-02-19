import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { FinancialAdvisorAdminPageComponent } from './financial-advisor-admin-page.component';

describe('FinancialAdvisorAdminPageComponent', () => {
  let component: FinancialAdvisorAdminPageComponent;
  let fixture: ComponentFixture<FinancialAdvisorAdminPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ FinancialAdvisorAdminPageComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(FinancialAdvisorAdminPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
