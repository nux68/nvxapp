import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { SeletionSediRepartoPageComponent } from './seletion-sedi-reparto-page.component';

describe('SeletionSediRepartoPageComponent', () => {
  let component: SeletionSediRepartoPageComponent;
  let fixture: ComponentFixture<SeletionSediRepartoPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SeletionSediRepartoPageComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(SeletionSediRepartoPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
