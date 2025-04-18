import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { RequestJustificationUserPageComponent } from './request-justification-user-page.component';

describe('RequestJustificationUserPageComponent', () => {
  let component: RequestJustificationUserPageComponent;
  let fixture: ComponentFixture<RequestJustificationUserPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ RequestJustificationUserPageComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(RequestJustificationUserPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
