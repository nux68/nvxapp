import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { IdAspNetUsersToCognomePipe } from './id-asp-net-users-to-cognome.pipe';

describe('IdAspNetUsersToCognomePipe', () => {
  it('create an instance', () => {
    const pipe = new IdAspNetUsersToCognomePipe({} as SharedParameterGestionePresenzeService);
    expect(pipe).toBeTruthy();
  });
});
