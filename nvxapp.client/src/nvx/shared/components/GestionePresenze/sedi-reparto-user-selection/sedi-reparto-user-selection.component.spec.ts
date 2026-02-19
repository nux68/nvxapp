import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { SediRepartoUserSelectionComponent } from './sedi-reparto-user-selection.component';

describe('SediRepartoUserSelectionComponent', () => {
  let component: SediRepartoUserSelectionComponent;
  let fixture: ComponentFixture<SediRepartoUserSelectionComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SediRepartoUserSelectionComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(SediRepartoUserSelectionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
