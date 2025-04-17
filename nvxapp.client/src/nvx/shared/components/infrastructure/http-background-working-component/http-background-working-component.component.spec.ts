import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { HttpBackgroundWorkingComponentComponent } from './http-background-working-component.component';

describe('HttpBackgroundWorkingComponentComponent', () => {
  let component: HttpBackgroundWorkingComponentComponent;
  let fixture: ComponentFixture<HttpBackgroundWorkingComponentComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ HttpBackgroundWorkingComponentComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(HttpBackgroundWorkingComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
