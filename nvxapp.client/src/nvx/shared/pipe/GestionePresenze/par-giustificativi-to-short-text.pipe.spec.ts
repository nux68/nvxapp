import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { ParGiustificativiToShortTextPipe } from './par-giustificativi-to-short-text.pipe';

describe('ParGiustificativiToShortTextPipe', () => {
  it('create an instance', () => {
    const pipe = new ParGiustificativiToShortTextPipe({} as SharedParameterGestionePresenzeService);
    expect(pipe).toBeTruthy();
  });
});
