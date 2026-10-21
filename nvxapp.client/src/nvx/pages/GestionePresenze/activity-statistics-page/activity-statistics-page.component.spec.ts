import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonBadge, IonCard, IonCardContent, IonChip, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonLabel, IonNote, IonRow, IonSegment, IonSegmentButton, provideIonicAngular } from '@ionic/angular';

import { ActivityStatisticsPageComponent } from './activity-statistics-page.component';

describe('ActivityStatisticsPageComponent', () => {
  let component: ActivityStatisticsPageComponent;
  let fixture: ComponentFixture<ActivityStatisticsPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ActivityStatisticsPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonBadge, IonCard, IonCardContent, IonChip, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonLabel, IonNote, IonRow, IonSegment, IonSegmentButton]
    }).compileComponents();

    fixture = TestBed.createComponent(ActivityStatisticsPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
