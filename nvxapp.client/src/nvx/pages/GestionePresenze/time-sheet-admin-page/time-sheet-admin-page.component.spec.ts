import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonActionSheet, IonCard, IonCardContent, IonChip, IonContent, IonIcon, provideIonicAngular } from '@ionic/angular';

import { TimeSheetAdminPageComponent } from './time-sheet-admin-page.component';

describe('TimeSheetAdminComponent', () => {
  let component: TimeSheetAdminPageComponent;
  let fixture: ComponentFixture<TimeSheetAdminPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ TimeSheetAdminPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonActionSheet, IonCard, IonCardContent, IonChip, IonContent, IonIcon]
    }).compileComponents();

    fixture = TestBed.createComponent(TimeSheetAdminPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
