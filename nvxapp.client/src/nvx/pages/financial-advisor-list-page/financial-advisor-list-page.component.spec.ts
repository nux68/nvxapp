import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { FinancialAdvisorListPageComponent } from './financial-advisor-list-page.component';

describe('FinancialAdvisorListPageComponent', () => {
  let component: FinancialAdvisorListPageComponent;
  let fixture: ComponentFixture<FinancialAdvisorListPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ FinancialAdvisorListPageComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(FinancialAdvisorListPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
