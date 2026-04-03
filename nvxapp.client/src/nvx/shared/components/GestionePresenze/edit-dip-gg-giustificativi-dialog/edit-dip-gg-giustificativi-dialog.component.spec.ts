import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { EditDipGGGiustificativiDialogComponent } from './edit-dip-gg-giustificativi-dialog.component';

describe('EditDipGGGiustificativiDialogComponent', () => {
  let component: EditDipGGGiustificativiDialogComponent;
  let fixture: ComponentFixture<EditDipGGGiustificativiDialogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ EditDipGGGiustificativiDialogComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(EditDipGGGiustificativiDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
