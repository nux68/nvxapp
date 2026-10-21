import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonHeader, IonNote, IonTitle, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { SeletionSediRepartoDialogComponent } from './seletion-sedi-reparto-dialog.component';

describe('SeletionSediRepartoDialogComponent', () => {
  let component: SeletionSediRepartoDialogComponent;
  let fixture: ComponentFixture<SeletionSediRepartoDialogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SeletionSediRepartoDialogComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonHeader, IonNote, IonTitle, IonToolbar]
    }).compileComponents();

    fixture = TestBed.createComponent(SeletionSediRepartoDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
