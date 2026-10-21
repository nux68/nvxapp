import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonButtons, IonFooter, IonIcon, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { PageButtonbarComponent } from './page-buttonbar.component';

describe('PageButtonbarComponent', () => {
  let component: PageButtonbarComponent;
  let fixture: ComponentFixture<PageButtonbarComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ PageButtonbarComponent ],
      providers: [provideIonicAngular()],
      imports: [IonButton, IonButtons, IonFooter, IonIcon, IonToolbar]
    }).compileComponents();

    fixture = TestBed.createComponent(PageButtonbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
