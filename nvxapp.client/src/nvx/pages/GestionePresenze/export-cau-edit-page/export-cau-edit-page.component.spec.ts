import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonInput, IonItem, IonItemSliding, IonLabel, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, provideIonicAngular } from '@ionic/angular';

import { ExportCauEditPageComponent } from './export-cau-edit-page.component';

describe('ExportCauEditPageComponent', () => {
  let component: ExportCauEditPageComponent;
  let fixture: ComponentFixture<ExportCauEditPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ExportCauEditPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonInput, IonItem, IonItemSliding, IonLabel, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption]
    }).compileComponents();

    fixture = TestBed.createComponent(ExportCauEditPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
