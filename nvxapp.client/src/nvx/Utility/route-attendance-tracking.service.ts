import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class RouteAttendanceTrackingService {

  constructor() { }

  public getRoutes(): Routes {
    return [];
  }
}
