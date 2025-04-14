import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { UserFinancialAdvisorListPageComponent } from './user-financial-advisor-list-page.component';

describe('UserFinancialAdvisorListPageComponent', () => {
  let component: UserFinancialAdvisorListPageComponent;
  let fixture: ComponentFixture<UserFinancialAdvisorListPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ UserFinancialAdvisorListPageComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(UserFinancialAdvisorListPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
