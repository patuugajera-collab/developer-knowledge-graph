import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({ providedIn: 'root' })
export class ToastService {
  constructor(private readonly snackBar: MatSnackBar) {}

  info(message: string): void {
    this.snackBar.open(message, 'OK', { duration: 3500, panelClass: 'toast-info' });
  }

  error(message: string): void {
    this.snackBar.open(message, 'Dismiss', { duration: 6000, panelClass: 'toast-error' });
  }

  success(message: string): void {
    this.snackBar.open(message, 'OK', { duration: 3000, panelClass: 'toast-success' });
  }
}