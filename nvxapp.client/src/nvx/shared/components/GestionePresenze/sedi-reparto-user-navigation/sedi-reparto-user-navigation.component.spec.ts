import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonCol, IonIcon, IonItem, IonLabel, IonRow, IonSelect, IonSelectOption, IonText, provideIonicAngular } from '@ionic/angular';

import { SediRepartoUserNavigationComponent } from './sedi-reparto-user-navigation.component';

describe('SediRepartoUserNavigationComponent', () => {
  let component: SediRepartoUserNavigationComponent;
  let fixture: ComponentFixture<SediRepartoUserNavigationComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SediRepartoUserNavigationComponent ],
      providers: [provideIonicAngular()],
      imports: [IonButton, IonCol, IonIcon, IonItem, IonLabel, IonRow, IonSelect, IonSelectOption, IonText]
    }).compileComponents();

    fixture = TestBed.createComponent(SediRepartoUserNavigationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
