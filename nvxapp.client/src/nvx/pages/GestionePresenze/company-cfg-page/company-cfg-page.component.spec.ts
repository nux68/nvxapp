import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonItem, IonLabel, IonList, IonListHeader, IonRadio, IonRadioGroup, IonSegment, IonSegmentButton, provideIonicAngular } from '@ionic/angular';

import { CompanyCfgPageComponent } from './company-cfg-page.component';

describe('CompanyCfgPageComponent', () => {
  let component: CompanyCfgPageComponent;
  let fixture: ComponentFixture<CompanyCfgPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CompanyCfgPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonItem, IonLabel, IonList, IonListHeader, IonRadio, IonRadioGroup, IonSegment, IonSegmentButton]
    }).compileComponents();

    fixture = TestBed.createComponent(CompanyCfgPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
