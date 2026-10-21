import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonCard, IonCardContent, IonCardTitle, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonIcon, IonInput, IonItem, IonItemOption, IonItemOptions, IonItemSliding, IonLabel, IonModal, IonNote, IonRange, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonToggle, provideIonicAngular } from '@ionic/angular';

import { ProfiloOrarioEditPageComponent } from './profilo-orario-edit-page.component';

describe('ProfiloOrarioEditPageComponent', () => {
  let component: ProfiloOrarioEditPageComponent;
  let fixture: ComponentFixture<ProfiloOrarioEditPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ProfiloOrarioEditPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonButton, IonCard, IonCardContent, IonCardTitle, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonIcon, IonInput, IonItem, IonItemOption, IonItemOptions, IonItemSliding, IonLabel, IonModal, IonNote, IonRange, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonToggle]
    }).compileComponents();

    fixture = TestBed.createComponent(ProfiloOrarioEditPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
