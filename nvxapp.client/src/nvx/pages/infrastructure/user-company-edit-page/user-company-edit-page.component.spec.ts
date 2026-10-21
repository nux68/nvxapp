import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonToggle, provideIonicAngular } from '@ionic/angular';

import { UserCompanyEditPageComponent } from './user-company-edit-page.component';

describe('UserCompanyEditPageComponent', () => {
  let component: UserCompanyEditPageComponent;
  let fixture: ComponentFixture<UserCompanyEditPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ UserCompanyEditPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonToggle]
    }).compileComponents();

    fixture = TestBed.createComponent(UserCompanyEditPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
