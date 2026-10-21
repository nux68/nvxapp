import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonToggle, provideIonicAngular } from '@ionic/angular';

import { UserFinancialAdvisorEditPageComponent } from './user-financial-advisor-edit-page.component';

describe('UserFinancialAdvisorEditPageComponent', () => {
  let component: UserFinancialAdvisorEditPageComponent;
  let fixture: ComponentFixture<UserFinancialAdvisorEditPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ UserFinancialAdvisorEditPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonToggle]
    }).compileComponents();

    fixture = TestBed.createComponent(UserFinancialAdvisorEditPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
