import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonToggle, provideIonicAngular } from '@ionic/angular';

import { FinancialAdvisorEditPageComponent } from './financial-advisor-edit-page.component';

describe('FinancialAdvisorEditPageComponent', () => {
  let component: FinancialAdvisorEditPageComponent;
  let fixture: ComponentFixture<FinancialAdvisorEditPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ FinancialAdvisorEditPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonToggle]
    }).compileComponents();

    fixture = TestBed.createComponent(FinancialAdvisorEditPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
