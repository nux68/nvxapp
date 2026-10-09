import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { ParCausaliToShortTextPipe } from './par-causali-to-short-text.pipe';

describe('ParCausaliToShortTextPipe', () => {
  it('create an instance', () => {
    const pipe = new ParCausaliToShortTextPipe({} as SharedParameterGestionePresenzeService);
    expect(pipe).toBeTruthy();
  });
});
