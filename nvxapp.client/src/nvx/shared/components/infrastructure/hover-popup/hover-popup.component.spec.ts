import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { provideIonicAngular } from '@ionic/angular';

import { HoverPopupComponent } from './hover-popup.component';

describe('HoverPopupComponent', () => {
  let component: HoverPopupComponent;
  let fixture: ComponentFixture<HoverPopupComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ HoverPopupComponent ],
      providers: [provideIonicAngular()],
      imports: []
    }).compileComponents();

    fixture = TestBed.createComponent(HoverPopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
