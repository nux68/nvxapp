import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { FinancialAdvisorEditPageComponent } from './financial-advisor-edit-page.component';

describe('FinancialAdvisorEditPageComponent', () => {
  let component: FinancialAdvisorEditPageComponent;
  let fixture: ComponentFixture<FinancialAdvisorEditPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ FinancialAdvisorEditPageComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(FinancialAdvisorEditPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
