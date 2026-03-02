import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { EditParProfiloOrarioDettaglioOrarioDialogComponent } from './edit-par-profilo-orario-dettaglio-orario-dialog.component';

describe('EditParProfiloOrarioDettaglioOrarioDialogComponent', () => {
  let component: EditParProfiloOrarioDettaglioOrarioDialogComponent;
  let fixture: ComponentFixture<EditParProfiloOrarioDettaglioOrarioDialogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ EditParProfiloOrarioDettaglioOrarioDialogComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(EditParProfiloOrarioDettaglioOrarioDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
