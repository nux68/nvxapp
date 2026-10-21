import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonActionSheet, IonButton, IonCard, IonCardContent, IonChip, IonCol, IonContent, IonGrid, IonIcon, IonRow, provideIonicAngular } from '@ionic/angular';

import { TimeSheetPowerAdminPageComponent } from './time-sheet-power-admin-page.component';

describe('TimeSheetPowerAdminPageComponent', () => {
  let component: TimeSheetPowerAdminPageComponent;
  let fixture: ComponentFixture<TimeSheetPowerAdminPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ TimeSheetPowerAdminPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonActionSheet, IonButton, IonCard, IonCardContent, IonChip, IonCol, IonContent, IonGrid, IonIcon, IonRow]
    }).compileComponents();

    fixture = TestBed.createComponent(TimeSheetPowerAdminPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
