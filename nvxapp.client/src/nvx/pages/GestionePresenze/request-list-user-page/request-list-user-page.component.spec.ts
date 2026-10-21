import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonItemSliding, IonLabel, IonNote, IonRow, IonText, provideIonicAngular } from '@ionic/angular';

import { RequestListUserPageComponent } from './request-list-user-page.component';

describe('RequestListUserPageComponent', () => {
  let component: RequestListUserPageComponent;
  let fixture: ComponentFixture<RequestListUserPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ RequestListUserPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonItemSliding, IonLabel, IonNote, IonRow, IonText]
    }).compileComponents();

    fixture = TestBed.createComponent(RequestListUserPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
