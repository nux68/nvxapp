import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonToggle, provideIonicAngular } from '@ionic/angular';

import { JustificationEditPageComponent } from './justification-edit-page.component';

describe('JustificationEditComponent', () => {
  let component: JustificationEditPageComponent;
  let fixture: ComponentFixture<JustificationEditPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ JustificationEditPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonToggle]
    }).compileComponents();

    fixture = TestBed.createComponent(JustificationEditPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
