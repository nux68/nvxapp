import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonItemOption, IonItemOptions, IonItemSliding, IonLabel, IonNote, IonRow, IonText, provideIonicAngular } from '@ionic/angular';

import { RequestListAdminPageComponent } from './request-list-admin-page.component';

describe('RequestListAdminPageComponent', () => {
  let component: RequestListAdminPageComponent;
  let fixture: ComponentFixture<RequestListAdminPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ RequestListAdminPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonItemOption, IonItemOptions, IonItemSliding, IonLabel, IonNote, IonRow, IonText]
    }).compileComponents();

    fixture = TestBed.createComponent(RequestListAdminPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
