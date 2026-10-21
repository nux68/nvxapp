import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonCheckbox, IonCol, IonContent, IonGrid, IonInput, IonItem, IonItemSliding, IonLabel, IonNote, IonReorder, IonReorderGroup, IonRow, IonSegment, IonSegmentButton, provideIonicAngular } from '@ionic/angular';

import { DepartmentEditPageComponent } from './department-edit-page.component';

describe('DepartmentEditPageComponent', () => {
  let component: DepartmentEditPageComponent;
  let fixture: ComponentFixture<DepartmentEditPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DepartmentEditPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonCheckbox, IonCol, IonContent, IonGrid, IonInput, IonItem, IonItemSliding, IonLabel, IonNote, IonReorder, IonReorderGroup, IonRow, IonSegment, IonSegmentButton]
    }).compileComponents();

    fixture = TestBed.createComponent(DepartmentEditPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
