import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { ParGiustificativiToLongTextPipe } from './par-giustificativi-to-long-text.pipe';

describe('ParGiustificativiToLongTextPipe', () => {
  it('create an instance', () => {
    const pipe = new ParGiustificativiToLongTextPipe({} as SharedParameterGestionePresenzeService);
    expect(pipe).toBeTruthy();
  });
});
