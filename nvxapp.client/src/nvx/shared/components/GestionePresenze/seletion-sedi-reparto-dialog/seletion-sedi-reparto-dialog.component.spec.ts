import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { SeletionSediRepartoDialogComponent } from './seletion-sedi-reparto-dialog.component';

describe('SeletionSediRepartoDialogComponent', () => {
  let component: SeletionSediRepartoDialogComponent;
  let fixture: ComponentFixture<SeletionSediRepartoDialogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SeletionSediRepartoDialogComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(SeletionSediRepartoDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
