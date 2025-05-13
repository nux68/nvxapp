import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { SediRepartoUserNavigationComponent } from './sedi-reparto-user-navigation.component';

describe('SediRepartoUserNavigationComponent', () => {
  let component: SediRepartoUserNavigationComponent;
  let fixture: ComponentFixture<SediRepartoUserNavigationComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SediRepartoUserNavigationComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(SediRepartoUserNavigationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
