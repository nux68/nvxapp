import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonBreadcrumb, IonBreadcrumbs, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { BreadcrumbsImpersonateComponent } from './breadcrumbs-impersonate.component';

describe('BreadcrumbsImpersonateComponent', () => {
  let component: BreadcrumbsImpersonateComponent;
  let fixture: ComponentFixture<BreadcrumbsImpersonateComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ BreadcrumbsImpersonateComponent ],
      providers: [provideIonicAngular()],
      imports: [IonBreadcrumb, IonBreadcrumbs, IonToolbar]
    }).compileComponents();

    fixture = TestBed.createComponent(BreadcrumbsImpersonateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
