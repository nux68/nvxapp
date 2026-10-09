import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { ParOrarioToLongTextPipe } from './par-orario-to-long-text.pipe';

describe('ParOrarioToLongTextPipe', () => {
  it('create an instance', () => {
    const pipe = new ParOrarioToLongTextPipe({} as SharedParameterGestionePresenzeService);
    expect(pipe).toBeTruthy();
  });
});
