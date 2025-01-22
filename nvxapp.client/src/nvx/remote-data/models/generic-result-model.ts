

// classi da derivare
export interface IGenericResultModel {
  succes: boolean;
  message: string;
}

export class GenericResultModel implements IGenericResultModel {
  succes: boolean = false;
  message: string = '';

  constructor(succes: boolean, message: string) {
    this.succes = succes;
    this.message = message;
  }

}

//classe da istanziare al volo
export class GenericResult<T>  {
  data: T;
  success: boolean;
  error: string;

  constructor() {

  }

}
