import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { provideIonicAngular } from '@ionic/angular';

import { HttpBackgroundWorkingComponentComponent } from './http-background-working-component.component';

describe('HttpBackgroundWorkingComponentComponent', () => {
  let component: HttpBackgroundWorkingComponentComponent;
  let fixture: ComponentFixture<HttpBackgroundWorkingComponentComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ HttpBackgroundWorkingComponentComponent ],
      providers: [provideIonicAngular()],
      imports: []
    }).compileComponents();

    fixture = TestBed.createComponent(HttpBackgroundWorkingComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
