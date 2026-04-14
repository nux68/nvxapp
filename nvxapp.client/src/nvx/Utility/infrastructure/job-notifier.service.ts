import { Injectable } from '@angular/core';
import { Message } from '../../ClientServer-Service/ModelsBase/message';
import { BehaviorSubject, Observable } from 'rxjs';
import { Subject } from 'rxjs/internal/Subject';

@Injectable({
  providedIn: 'root'
})
export class JobNotifierService {


  private readonly _jobFinished = new Subject<JobNotifierData>();
  public readonly jobFinished$: Observable<JobNotifierData> = this._jobFinished.asObservable();

  constructor() { }

  public jobNotifier(jobUpdate: JobNotifierData): void {

    this._jobFinished.next(jobUpdate);

  }

}


export class JobNotifierData {
  public jobId: string = "";
  public jobType: string = "";
  public payload?: any;

  public progressPercentage: number = 0;
  public message: Message | null = null;
  public isFinished: boolean = false;
}
