import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { BasePageConfirmCancelComponent } from './base-page-confirm-cancel.component';

describe('BasePageConfirmCancelComponent', () => {
  let component: BasePageConfirmCancelComponent;
  let fixture: ComponentFixture<BasePageConfirmCancelComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ BasePageConfirmCancelComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(BasePageConfirmCancelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
