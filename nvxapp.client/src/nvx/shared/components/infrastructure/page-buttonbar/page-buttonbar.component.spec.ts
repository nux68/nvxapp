import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonButtons, IonFooter, IonIcon, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { HoverPopupDirective } from '../hover-popup/hover-popup.directive';
import { NvxActionsMenuComponent } from '../nvx-actions-menu/nvx-actions-menu.component';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { PageButtonbarComponent } from './page-buttonbar.component';

describe('PageButtonbarComponent', () => {
  let component: PageButtonbarComponent;
  let fixture: ComponentFixture<PageButtonbarComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ PageButtonbarComponent ],
      providers: [provideIonicAngular(), { provide: UserInterfaceService, useValue: { isMobileSignal: () => false } }],
      imports: [IonButton, IonButtons, IonFooter, IonIcon, IonToolbar, HoverPopupDirective, NvxActionsMenuComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(PageButtonbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
