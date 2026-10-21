import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonButtons, IonCheckbox, IonContent, IonHeader, IonIcon, IonItem, IonLabel, IonList, IonSearchbar, IonTitle, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { DipSelectorModalComponent } from './dip-selector-modal.component';

describe('DipSelectorModalComponent', () => {
  let component: DipSelectorModalComponent;
  let fixture: ComponentFixture<DipSelectorModalComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DipSelectorModalComponent ],
      providers: [provideIonicAngular()],
      imports: [IonButton, IonButtons, IonCheckbox, IonContent, IonHeader, IonIcon, IonItem, IonLabel, IonList, IonSearchbar, IonTitle, IonToolbar]
    }).compileComponents();

    fixture = TestBed.createComponent(DipSelectorModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
