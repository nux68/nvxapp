import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { ProfiloOrarioListPageComponent } from './profilo-orario-list-page.component';

describe('ProfiloOrarioListPageComponent', () => {
  let component: ProfiloOrarioListPageComponent;
  let fixture: ComponentFixture<ProfiloOrarioListPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ProfiloOrarioListPageComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(ProfiloOrarioListPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
