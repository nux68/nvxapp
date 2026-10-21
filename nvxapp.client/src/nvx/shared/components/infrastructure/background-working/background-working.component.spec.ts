import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonSpinner, provideIonicAngular } from '@ionic/angular';

import { BackgroundWorkingComponent } from './background-working.component';

describe('HttpBackgroundWorkingComponent', () => {
  let component: BackgroundWorkingComponent;
  let fixture: ComponentFixture<BackgroundWorkingComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ BackgroundWorkingComponent ],
      providers: [provideIonicAngular()],
      imports: [IonSpinner]
    }).compileComponents();

    fixture = TestBed.createComponent(BackgroundWorkingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
