import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { EditDipGGCausaliDialogComponent } from './edit-dip-gg-causali-dialog.component';

describe('EditDipGGCausaliDialogComponent', () => {
  let component: EditDipGGCausaliDialogComponent;
  let fixture: ComponentFixture<EditDipGGCausaliDialogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ EditDipGGCausaliDialogComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(EditDipGGCausaliDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
