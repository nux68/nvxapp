import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, provideIonicAngular } from '@ionic/angular';

import { DealerPowerAdminPageComponent } from './dealer-power-admin-page.component';

describe('DealerPowerAdminPageComponent', () => {
  let component: DealerPowerAdminPageComponent;
  let fixture: ComponentFixture<DealerPowerAdminPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DealerPowerAdminPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent]
    }).compileComponents();

    fixture = TestBed.createComponent(DealerPowerAdminPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
