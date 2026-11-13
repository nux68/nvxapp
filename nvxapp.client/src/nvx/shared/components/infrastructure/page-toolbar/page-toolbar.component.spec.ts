import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonButtons, IonCol, IonGrid, IonHeader, IonIcon, IonItem, IonLabel, IonMenuButton, IonProgressBar, IonRow, IonSearchbar, IonTitle, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { TranslocoTestingModule } from '@jsverse/transloco';
import { PageToolbarComponent } from './page-toolbar.component';

describe('PageToolbarComponent', () => {
  let component: PageToolbarComponent;
  let fixture: ComponentFixture<PageToolbarComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ PageToolbarComponent ],
      providers: [provideIonicAngular()],
      imports: [TranslocoTestingModule.forRoot({ langs: {} }), IonButton, IonButtons, IonCol, IonGrid, IonHeader, IonIcon, IonItem, IonLabel, IonMenuButton, IonProgressBar, IonRow, IonSearchbar, IonTitle, IonToolbar]
    }).compileComponents();

    fixture = TestBed.createComponent(PageToolbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
