import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { IonCard, IonCardContent, IonContent, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonSpinner, provideIonicAngular } from '@ionic/angular';

import { CompanyMovePageComponent } from './company-move-page.component';

describe('CompanyMovePageComponent', () => {
  let component: CompanyMovePageComponent;
  let fixture: ComponentFixture<CompanyMovePageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CompanyMovePageComponent ],
      providers: [provideIonicAngular(), provideHttpClient()],
      imports: [ReactiveFormsModule, TranslocoTestingModule.forRoot({ langs: {} }), IonCard, IonCardContent, IonContent, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonSpinner]
    }).compileComponents();

    fixture = TestBed.createComponent(CompanyMovePageComponent);
    component = fixture.componentInstance;
    component.dealers = [{ idDealer: 1, descrizione: 'Centro A' }, { idDealer: 2, descrizione: 'Centro B' }];
    component.financialAdvisors = [
      { idFinancialAdvisor: 10, idDealer: 1, descrizione: 'Studio A1' },
      { idFinancialAdvisor: 20, idDealer: 2, descrizione: 'Studio B1' },
    ];
    component.companies = [
      { idCompany: 100, idFinancialAdvisor: 10, descrizione: 'Azienda 1' },
      { idCompany: 200, idFinancialAdvisor: 20, descrizione: 'Azienda 2' },
    ];
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('il centro filtra studi e aziende', () => {
    component.onOrigineDealer(2);
    expect(component.origineStudi.map(f => f.idFinancialAdvisor)).toEqual([20]);
    expect(component.origineAziende.map(c => c.idCompany)).toEqual([200]);
  });

  it('scegliendo l\'azienda si impostano studio e centro', () => {
    component.onOrigineAzienda(100);
    expect(component._editForm.value.origineIdFinancialAdvisor).toBe(10);
    expect(component._editForm.value.origineIdDealer).toBe(1);
  });

  it('non si conferma verso lo stesso studio', () => {
    component.onOrigineAzienda(100);
    component.onDestinazioneStudio(10);
    expect(component.stessoStudio).toBeTrue();
    expect(component.buttonbar[0].disabled).toBeTrue();
    component.onDestinazioneStudio(20);
    expect(component.buttonbar[0].disabled).toBeFalse();
    expect(component._editForm.value.destinazioneIdDealer).toBe(2);
  });
});
