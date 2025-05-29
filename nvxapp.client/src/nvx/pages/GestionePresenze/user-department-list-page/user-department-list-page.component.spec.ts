import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';
import { UserDepartmentListPageComponent } from './user-department-list-page.component';
describe('UserDepartmentListPageComponent', () => {
  let component: UserDepartmentListPageComponent;
  let fixture: ComponentFixture<UserDepartmentListPageComponent>;
  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ UserDepartmentListPageComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();
    fixture = TestBed.createComponent(UserDepartmentListPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
