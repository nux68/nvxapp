import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, provideIonicAngular } from '@ionic/angular';

import { FinancialAdvisorPowerAdminPageComponent } from './financial-advisor-power-admin-page.component';

describe('FinancialAdvisorPowerAdminPageComponent', () => {
  let component: FinancialAdvisorPowerAdminPageComponent;
  let fixture: ComponentFixture<FinancialAdvisorPowerAdminPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ FinancialAdvisorPowerAdminPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent]
    }).compileComponents();

    fixture = TestBed.createComponent(FinancialAdvisorPowerAdminPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
