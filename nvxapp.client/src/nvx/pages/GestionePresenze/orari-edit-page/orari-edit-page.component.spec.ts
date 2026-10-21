import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonCard, IonCardContent, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonIcon, IonInput, IonItem, IonItemSliding, IonLabel, IonModal, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, provideIonicAngular } from '@ionic/angular';

import { OrariEditPageComponent } from './orari-edit-page.component';

describe('OrariEditPageComponent', () => {
  let component: OrariEditPageComponent;
  let fixture: ComponentFixture<OrariEditPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ OrariEditPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonButton, IonCard, IonCardContent, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonIcon, IonInput, IonItem, IonItemSliding, IonLabel, IonModal, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption]
    }).compileComponents();

    fixture = TestBed.createComponent(OrariEditPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
