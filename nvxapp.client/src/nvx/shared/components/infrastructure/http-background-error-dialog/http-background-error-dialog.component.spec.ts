import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonButtons, IonCard, IonCardContent, IonCardHeader, IonCardTitle, IonContent, IonFooter, IonHeader, IonIcon, IonItem, IonLabel, IonList, IonModal, IonTitle, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { HttpBackgroundErrorDialogComponent } from './http-background-error-dialog.component';

describe('HttpBackgroundErrorDialogComponent', () => {
  let component: HttpBackgroundErrorDialogComponent;
  let fixture: ComponentFixture<HttpBackgroundErrorDialogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ HttpBackgroundErrorDialogComponent ],
      providers: [provideIonicAngular()],
      imports: [IonButton, IonButtons, IonCard, IonCardContent, IonCardHeader, IonCardTitle, IonContent, IonFooter, IonHeader, IonIcon, IonItem, IonLabel, IonList, IonModal, IonTitle, IonToolbar]
    }).compileComponents();

    fixture = TestBed.createComponent(HttpBackgroundErrorDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
