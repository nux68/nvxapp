import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, provideIonicAngular } from '@ionic/angular';

import { AddUserCompanyComponent } from './add-user-company.component';

describe('AddUserCompanyComponent', () => {
  let component: AddUserCompanyComponent;
  let fixture: ComponentFixture<AddUserCompanyComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ AddUserCompanyComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption]
    }).compileComponents();

    fixture = TestBed.createComponent(AddUserCompanyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
