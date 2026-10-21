import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonRadio, IonRadioGroup, provideIonicAngular } from '@ionic/angular';

import { UserImpersonatePageComponent } from './user-impersonate-page.component';

describe('UserImpersonateComponent', () => {
  let component: UserImpersonatePageComponent;
  let fixture: ComponentFixture<UserImpersonatePageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ UserImpersonatePageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonRadio, IonRadioGroup]
    }).compileComponents();

    fixture = TestBed.createComponent(UserImpersonatePageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
