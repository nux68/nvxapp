import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonItemSliding, IonLabel, IonRow, provideIonicAngular } from '@ionic/angular';
import { CommessaListPageComponent } from './commessa-list-page.component';

describe('CommessaListPageComponent', () => {
  let component: CommessaListPageComponent;
  let fixture: ComponentFixture<CommessaListPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CommessaListPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonItemSliding, IonLabel, IonRow]
    }).compileComponents();

    fixture = TestBed.createComponent(CommessaListPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
