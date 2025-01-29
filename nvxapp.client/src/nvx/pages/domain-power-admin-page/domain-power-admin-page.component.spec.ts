import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { DomainPowerAdminPageComponent } from './domain-power-admin-page.component';

describe('DomainPowerAdminPageComponent', () => {
  let component: DomainPowerAdminPageComponent;
  let fixture: ComponentFixture<DomainPowerAdminPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DomainPowerAdminPageComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(DomainPowerAdminPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
