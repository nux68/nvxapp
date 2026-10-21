import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonItemSliding, IonLabel, IonRow, provideIonicAngular } from '@ionic/angular';

import { ProfiloOrarioListPageComponent } from './profilo-orario-list-page.component';

describe('ProfiloOrarioListPageComponent', () => {
  let component: ProfiloOrarioListPageComponent;
  let fixture: ComponentFixture<ProfiloOrarioListPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ProfiloOrarioListPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonItemSliding, IonLabel, IonRow]
    }).compileComponents();

    fixture = TestBed.createComponent(ProfiloOrarioListPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
