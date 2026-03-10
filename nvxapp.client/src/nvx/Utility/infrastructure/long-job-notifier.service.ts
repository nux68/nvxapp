import { Injectable } from '@angular/core';
import { Message } from '../../ClientServer-Service/ModelsBase/message';
import { BehaviorSubject, Observable } from 'rxjs';
import { Subject } from 'rxjs/internal/Subject';

@Injectable({
  providedIn: 'root'
})
export class LongJobNotifierService {

  // Private BehaviorSubject to hold the state of active jobs.
  private readonly _activeJobs = new BehaviorSubject<LongJobProgressUpdate[]>([]);
// Public Observable that components can subscribe to for real-time updates.
  public readonly activeJobs$: Observable<LongJobProgressUpdate[]> = this._activeJobs.asObservable();

  // Subject per notificare il completamento di un job
  private readonly _jobFinished = new Subject<LongJobProgressUpdate>();
  public readonly jobFinished$: Observable<LongJobProgressUpdate> = this._jobFinished.asObservable();

  constructor() { }

  /**
   * Adds a new job to the list or updates an existing one based on the jobId.
   * If the job is marked as finished, it will be removed from the list after a short delay.
   * @param jobUpdate The job progress update object received from the server.
   */
  public addOrUpdateJob(jobUpdate: LongJobProgressUpdate): void {
    // Get the current list of jobs
    const currentJobs = this._activeJobs.getValue();
    const jobIndex = currentJobs.findIndex(j => j.jobId === jobUpdate.jobId);

    if (jobIndex > -1) {
      // Job exists, update it
      currentJobs[jobIndex] = jobUpdate;
    } else {
      // Job is new, add it to the beginning of the list
      currentJobs.unshift(jobUpdate);
    }

    // Push the updated list back into the BehaviorSubject to notify subscribers.
    this._activeJobs.next([...currentJobs]);

    // If the job is finished, automatically remove it after a few seconds.
    if (jobUpdate.isFinished) {
      // Emetti l'evento con l'aggiornamento finale, che include il payload
      this._jobFinished.next(jobUpdate);

      setTimeout(() => this.removeJob(jobUpdate.jobId), 5000); // 5-second delay
    }
  }

  /**
   * Removes a job from the active jobs list.
   * @param jobId The ID of the job to remove.
   */
  public removeJob(jobId: string): void {
    const currentJobs = this._activeJobs.getValue();
    const updatedJobs = currentJobs.filter(j => j.jobId !== jobId);
    this._activeJobs.next(updatedJobs);
  }
}


// Corrisponde alla classe C# LongJobProgressUpdate
export class LongJobProgressUpdate {
  public jobId: string = "";
  public jobType: string = "";
  public payload?: any;

  public progressPercentage: number = 0;
  public message: Message | null = null;
  public isFinished: boolean = false;
}

