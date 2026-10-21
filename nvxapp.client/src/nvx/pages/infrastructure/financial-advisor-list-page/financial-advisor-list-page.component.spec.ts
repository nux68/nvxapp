import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonCard, IonCardContent, IonCheckbox, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonItemOption, IonItemOptions, IonItemSliding, IonLabel, IonRow, provideIonicAngular } from '@ionic/angular';

import { FinancialAdvisorListPageComponent } from './financial-advisor-list-page.component';

describe('FinancialAdvisorListPageComponent', () => {
  let component: FinancialAdvisorListPageComponent;
  let fixture: ComponentFixture<FinancialAdvisorListPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ FinancialAdvisorListPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonButton, IonCard, IonCardContent, IonCheckbox, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonItemOption, IonItemOptions, IonItemSliding, IonLabel, IonRow]
    }).compileComponents();

    fixture = TestBed.createComponent(FinancialAdvisorListPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
