import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { HttpBackgroundErrorDialogComponent } from './http-background-error-dialog.component';

describe('HttpBackgroundErrorDialogComponent', () => {
  let component: HttpBackgroundErrorDialogComponent;
  let fixture: ComponentFixture<HttpBackgroundErrorDialogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ HttpBackgroundErrorDialogComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(HttpBackgroundErrorDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
