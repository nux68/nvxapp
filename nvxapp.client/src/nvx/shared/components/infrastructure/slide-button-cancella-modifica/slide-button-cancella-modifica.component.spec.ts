import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { SlideButtonCancellaModificaComponent } from './slide-button-cancella-modifica.component';

describe('SlideButtonCancellaModificaComponent', () => {
  let component: SlideButtonCancellaModificaComponent;
  let fixture: ComponentFixture<SlideButtonCancellaModificaComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SlideButtonCancellaModificaComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(SlideButtonCancellaModificaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
