import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonContent, IonIcon, provideIonicAngular } from '@ionic/angular';

import { DashboardPageComponent } from './dashboard-page.component';

describe('DashboardPageComponent', () => {
  let component: DashboardPageComponent;
  let fixture: ComponentFixture<DashboardPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DashboardPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonContent, IonIcon]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
