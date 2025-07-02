


export class CheckObjOn_Id_Text implements ICheckObj<string> {
  public id: string;
  public checked: boolean;
}

export class CheckObjOn_Id_Number implements ICheckObj<number> {
  public id: number;
  public checked: boolean;
}


export interface ICheckObj<T> {
  id: T;
  checked: boolean;
}
