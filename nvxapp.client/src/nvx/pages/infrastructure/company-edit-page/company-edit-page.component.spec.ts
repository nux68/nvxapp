import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonCheckbox, IonContent, IonInput, IonItem, IonLabel, IonNote, IonToggle, provideIonicAngular } from '@ionic/angular';

import { CompanyEditPageComponent } from './company-edit-page.component';

describe('CompanyEditPageComponent', () => {
  let component: CompanyEditPageComponent;
  let fixture: ComponentFixture<CompanyEditPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CompanyEditPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonCheckbox, IonContent, IonInput, IonItem, IonLabel, IonNote, IonToggle]
    }).compileComponents();

    fixture = TestBed.createComponent(CompanyEditPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
