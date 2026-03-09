import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { TimeSheetEngineCallerComponent } from './time-sheet-engine-caller.component';

describe('TimeSheetEngineCallerComponent', () => {
  let component: TimeSheetEngineCallerComponent;
  let fixture: ComponentFixture<TimeSheetEngineCallerComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ TimeSheetEngineCallerComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(TimeSheetEngineCallerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
