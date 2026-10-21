import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonContent, provideIonicAngular } from '@ionic/angular';

import { SuperUserPageComponent } from './super-user-page.component';

describe('SuperUserPageComponent', () => {
  let component: SuperUserPageComponent;
  let fixture: ComponentFixture<SuperUserPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SuperUserPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonContent]
    }).compileComponents();

    fixture = TestBed.createComponent(SuperUserPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
