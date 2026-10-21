import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonContent, provideIonicAngular } from '@ionic/angular';

import { PowerAdminPageComponent } from './power-admin-page.component';

describe('PowerAdminPageComponent', () => {
  let component: PowerAdminPageComponent;
  let fixture: ComponentFixture<PowerAdminPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ PowerAdminPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonContent]
    }).compileComponents();

    fixture = TestBed.createComponent(PowerAdminPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
