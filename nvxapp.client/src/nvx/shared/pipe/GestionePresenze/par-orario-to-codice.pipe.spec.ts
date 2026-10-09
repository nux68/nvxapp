import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { ParOrarioToCodicePipe } from './par-orario-to-codice.pipe';

describe('ParOrarioToCodicePipe', () => {
  it('create an instance', () => {
    const pipe = new ParOrarioToCodicePipe({} as SharedParameterGestionePresenzeService);
    expect(pipe).toBeTruthy();
  });
});
