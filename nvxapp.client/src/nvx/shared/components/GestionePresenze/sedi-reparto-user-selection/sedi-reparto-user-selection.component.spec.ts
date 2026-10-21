import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonCol, IonIcon, IonItem, IonLabel, IonRow, IonSelect, IonSelectOption, IonText, provideIonicAngular } from '@ionic/angular';

import { SediRepartoUserSelectionComponent } from './sedi-reparto-user-selection.component';

describe('SediRepartoUserSelectionComponent', () => {
  let component: SediRepartoUserSelectionComponent;
  let fixture: ComponentFixture<SediRepartoUserSelectionComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SediRepartoUserSelectionComponent ],
      providers: [provideIonicAngular()],
      imports: [IonButton, IonCol, IonIcon, IonItem, IonLabel, IonRow, IonSelect, IonSelectOption, IonText]
    }).compileComponents();

    fixture = TestBed.createComponent(SediRepartoUserSelectionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
