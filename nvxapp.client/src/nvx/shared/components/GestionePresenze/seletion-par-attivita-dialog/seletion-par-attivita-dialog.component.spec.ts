import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { SeletionParAttivitaDialogComponent } from './seletion-par-attivita-dialog.component';

describe('SeletionParAttivitaDialogComponent', () => {
  let component: SeletionParAttivitaDialogComponent;
  let fixture: ComponentFixture<SeletionParAttivitaDialogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SeletionParAttivitaDialogComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(SeletionParAttivitaDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
