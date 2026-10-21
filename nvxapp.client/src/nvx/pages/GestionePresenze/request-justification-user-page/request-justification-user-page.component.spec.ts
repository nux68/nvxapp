import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonDatetime, IonDatetimeButton, IonIcon, IonItem, IonLabel, IonModal, IonNote, IonSelect, IonSelectOption, IonTextarea, IonToggle, provideIonicAngular } from '@ionic/angular';

import { RequestJustificationUserPageComponent } from './request-justification-user-page.component';

describe('RequestJustificationUserPageComponent', () => {
  let component: RequestJustificationUserPageComponent;
  let fixture: ComponentFixture<RequestJustificationUserPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ RequestJustificationUserPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonDatetime, IonDatetimeButton, IonIcon, IonItem, IonLabel, IonModal, IonNote, IonSelect, IonSelectOption, IonTextarea, IonToggle]
    }).compileComponents();

    fixture = TestBed.createComponent(RequestJustificationUserPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
