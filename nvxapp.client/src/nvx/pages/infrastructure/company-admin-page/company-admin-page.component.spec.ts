import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, provideIonicAngular } from '@ionic/angular';

import { CompanyAdminPageComponent } from './company-admin-page.component';

describe('DomainAdminPageComponent', () => {
  let component: CompanyAdminPageComponent;
  let fixture: ComponentFixture<CompanyAdminPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CompanyAdminPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent]
    }).compileComponents();

    fixture = TestBed.createComponent(CompanyAdminPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
