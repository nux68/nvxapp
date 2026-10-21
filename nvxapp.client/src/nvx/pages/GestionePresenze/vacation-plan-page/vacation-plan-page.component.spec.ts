import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonActionSheet, IonCard, IonCardContent, IonChip, IonContent, IonLabel, provideIonicAngular } from '@ionic/angular';

import { VacationPlanPageComponent } from './vacation-plan-page.component';

describe('VacationPlanPageComponent', () => {
  let component: VacationPlanPageComponent;
  let fixture: ComponentFixture<VacationPlanPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ VacationPlanPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonActionSheet, IonCard, IonCardContent, IonChip, IonContent, IonLabel]
    }).compileComponents();

    fixture = TestBed.createComponent(VacationPlanPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
