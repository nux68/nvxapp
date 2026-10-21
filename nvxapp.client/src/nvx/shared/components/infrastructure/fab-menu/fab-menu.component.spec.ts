import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonButtons, IonChip, IonContent, IonFab, IonFabButton, IonFabList, IonFooter, IonHeader, IonIcon, IonModal, IonTextarea, IonTitle, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { FabMenuComponent } from './fab-menu.component';

describe('FabMenuComponent', () => {
  let component: FabMenuComponent;
  let fixture: ComponentFixture<FabMenuComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ FabMenuComponent ],
      providers: [provideIonicAngular()],
      imports: [IonButton, IonButtons, IonChip, IonContent, IonFab, IonFabButton, IonFabList, IonFooter, IonHeader, IonIcon, IonModal, IonTextarea, IonTitle, IonToolbar]
    }).compileComponents();

    fixture = TestBed.createComponent(FabMenuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
