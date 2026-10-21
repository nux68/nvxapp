import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonContent, IonHeader, IonModal, IonProgressBar, IonTitle, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { ParameterLoaderComponent } from './parameter-loader.component';

describe('ParameterLoaderComponent', () => {
  let component: ParameterLoaderComponent;
  let fixture: ComponentFixture<ParameterLoaderComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ParameterLoaderComponent ],
      providers: [provideIonicAngular()],
      imports: [IonContent, IonHeader, IonModal, IonProgressBar, IonTitle, IonToolbar]
    }).compileComponents();

    fixture = TestBed.createComponent(ParameterLoaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
