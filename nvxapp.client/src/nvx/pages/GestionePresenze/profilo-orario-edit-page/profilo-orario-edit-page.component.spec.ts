import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { ProfiloOrarioEditPageComponent } from './profilo-orario-edit-page.component';

describe('ProfiloOrarioEditPageComponent', () => {
  let component: ProfiloOrarioEditPageComponent;
  let fixture: ComponentFixture<ProfiloOrarioEditPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ProfiloOrarioEditPageComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(ProfiloOrarioEditPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
