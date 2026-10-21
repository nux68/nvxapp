import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonToggle, provideIonicAngular } from '@ionic/angular';

import { DealerEditPageComponent } from './dealer-edit-page.component';

describe('DealerEditPageComponent', () => {
  let component: DealerEditPageComponent;
  let fixture: ComponentFixture<DealerEditPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DealerEditPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonToggle]
    }).compileComponents();

    fixture = TestBed.createComponent(DealerEditPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
