import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, provideIonicAngular } from '@ionic/angular';

import { SeletionSediRepartoPageComponent } from './seletion-sedi-reparto-page.component';

describe('SeletionSediRepartoPageComponent', () => {
  let component: SeletionSediRepartoPageComponent;
  let fixture: ComponentFixture<SeletionSediRepartoPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SeletionSediRepartoPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent]
    }).compileComponents();

    fixture = TestBed.createComponent(SeletionSediRepartoPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
