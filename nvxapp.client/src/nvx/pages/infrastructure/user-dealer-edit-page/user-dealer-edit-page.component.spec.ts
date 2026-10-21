import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonToggle, provideIonicAngular } from '@ionic/angular';

import { UserDealerEditPageComponent } from './user-dealer-edit-page.component';

describe('UserDealerEditPageComponent', () => {
  let component: UserDealerEditPageComponent;
  let fixture: ComponentFixture<UserDealerEditPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ UserDealerEditPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonToggle]
    }).compileComponents();

    fixture = TestBed.createComponent(UserDealerEditPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
