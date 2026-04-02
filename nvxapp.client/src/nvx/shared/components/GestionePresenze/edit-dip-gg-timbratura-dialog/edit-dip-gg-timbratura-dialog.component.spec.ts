import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { EditDipGGTimbraturaDialogComponent } from './edit-dip-gg-timbratura-dialog.component';

describe('EditDipGGTimbraturaDialogComponent', () => {
  let component: EditDipGGTimbraturaDialogComponent;
  let fixture: ComponentFixture<EditDipGGTimbraturaDialogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ EditDipGGTimbraturaDialogComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(EditDipGGTimbraturaDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
