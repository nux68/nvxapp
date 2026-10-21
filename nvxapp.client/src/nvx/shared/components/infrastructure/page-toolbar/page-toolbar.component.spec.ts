import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonButtons, IonCol, IonGrid, IonHeader, IonIcon, IonItem, IonLabel, IonMenuButton, IonProgressBar, IonRow, IonSearchbar, IonTitle, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { PageToolbarComponent } from './page-toolbar.component';

describe('PageToolbarComponent', () => {
  let component: PageToolbarComponent;
  let fixture: ComponentFixture<PageToolbarComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ PageToolbarComponent ],
      providers: [provideIonicAngular()],
      imports: [IonButton, IonButtons, IonCol, IonGrid, IonHeader, IonIcon, IonItem, IonLabel, IonMenuButton, IonProgressBar, IonRow, IonSearchbar, IonTitle, IonToolbar]
    }).compileComponents();

    fixture = TestBed.createComponent(PageToolbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
