import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { EditDipProfiloOrarioDialogComponent } from './edit-dip-profilo-orario-dialog.component';

describe('EditDipProfiloOrarioDialogComponent', () => {
  let component: EditDipProfiloOrarioDialogComponent;
  let fixture: ComponentFixture<EditDipProfiloOrarioDialogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ EditDipProfiloOrarioDialogComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(EditDipProfiloOrarioDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
